/*-
 * Copyright (c) 2015 Software Development Solutions, Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using fastJSON;
using Jamcast.Extensibility;
using Jamcast.Extensibility.ContentDirectory;

namespace Jamcast.Plugins.Murfie
{
    public static class API
    {
        private static string LOG_MODULE = "Murfie";
        private static string MURFIE_API_ROOT_URL = "https://www.murfie.com/api";
        private static string MURFIE_API_AUTH_URL = MURFIE_API_ROOT_URL + "/tokens";
        private static string MURFIE_API_DISCS_URL = MURFIE_API_ROOT_URL + "/discs.json";
        private static string MURFIE_API_TRACKS_URL_TEMPLATE = MURFIE_API_ROOT_URL + "/discs/{0}.json";
        private static string MURFIE_API_MEDIA_URI_QUERY_TEMPLATE = MURFIE_API_ROOT_URL + "/discs/{0}/tracks/{1}.json?auth_token={2}";

        private static string _token;
        private static List<discQueryResult> _discCache;

        public static bool IsLoggedIn()
        {
            return _token != null;
        }

        public static void LogOut()
        {
            _token = null;
            _discCache = null;
        }

        public static bool Authenticate(string email, string password)
        {
            Log.Info(LOG_MODULE, "Authenticating...");
            HttpWebRequest request = HttpWebRequest.Create(MURFIE_API_AUTH_URL) as HttpWebRequest;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            using (var stream = request.GetRequestStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(String.Format("email={0}&password={1}", Uri.EscapeDataString(email), Uri.EscapeDataString(password)));
                }
            }
            try
            {
                var response = request.GetResponse();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = JSON.ToObject<authResult>(reader.ReadToEnd());
                    if (result == null)
                        return false;
                    if (result.message != null)
                        throw new ApplicationException(result.message);
                    if (result.user == null)
                        return false;
                    _token = result.user.token;
                }
                return IsLoggedIn();
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Unauthorized)
                    return false;
                throw;
            }
        }

        public static discQueryResult[] Search(string query)
        {
            doRequiresAuth();
            Log.Info(LOG_MODULE, "Search for keyword {0}...", query);
            HttpWebRequest request = HttpWebRequest.Create(String.Format("{0}?q={1}&auth_token={2}", MURFIE_API_DISCS_URL, Uri.EscapeDataString(query), _token)) as HttpWebRequest;
            var response = request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var result = JSON.ToObject(reader.ReadToEnd(), typeof(List<discQueryResult>)) as List<discQueryResult>;
                return result.ToArray();
            }
        }

        public static discQueryResult[] GetDiscs()
        {
            doRequiresAuth();
            Log.Info(LOG_MODULE, "Retrieving discs...");
            HttpWebRequest request = HttpWebRequest.Create(String.Format("{0}?auth_token={1}", MURFIE_API_DISCS_URL, _token)) as HttpWebRequest;
            var response = request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                _discCache = JSON.ToObject(reader.ReadToEnd(), typeof(List<discQueryResult>)) as List<discQueryResult>;
                return _discCache.ToArray();
            }
        }

        public static disc GetDisc(int id)
        {
            verifyDiskCache();
            return (from discQueryResult d in _discCache where d.disc.id.Equals(id) select d.disc).FirstOrDefault();
        }

        public static track GetTrack(int discID, int trackID)
        {
            var disc = GetDisc(discID);
            disc.LoadTracks();
            return (from t in disc.tracks where t.id == trackID select t).FirstOrDefault();
        }

        public static track GetMediaUri(int discID, int trackID, bool getFlac)
        {
            doRequiresAuth();
            string url = String.Format(MURFIE_API_MEDIA_URI_QUERY_TEMPLATE, discID, trackID, _token);
            if (getFlac)
                url += "&media_format=flac";
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            var response = request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var result = JSON.ToObject(reader.ReadToEnd(), typeof(mediaUriQueryResult)) as mediaUriQueryResult;
                return result.track;
            }
        }

        private static void verifyDiskCache()
        {
            if (_discCache == null)
                GetDiscs();
        }

        private static List<track> GetDiscTracks(int id)
        {
            verifyDiskCache();
            Log.Info(LOG_MODULE, "Retrieving tracks for disc {0}...", id);
            HttpWebRequest request = HttpWebRequest.Create(String.Format(MURFIE_API_TRACKS_URL_TEMPLATE, id)) as HttpWebRequest;
            var response = request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var result = JSON.ToObject(reader.ReadToEnd(), typeof(discQueryResult)) as discQueryResult;
                (from t in result.disc.tracks select t).ToList().ForEach(t => t.Disc = result.disc);
                return result.disc.tracks;
            }
        }

        private static void doRequiresAuth()
        {
            if (_token == null)
                throw new ApplicationException("No authorization token, please log in to Murfie");
        }

        [DataContract]
        public class PersistedTrackKey : IPersisted
        {
            [DataMember(Order = 0)]
            public int DiscID { get; set; }

            [DataMember(Order = 1)]
            public int TrackID { get; set; }

            public int GetPersistenceHash()
            {
                return String.Format("{0}|{1}", this.DiscID, this.TrackID).GetHashCode();
            }
        }

        public class mediaUriQueryResult
        {
            public track track { get; set; }
        }

        public class discQueryResult
        {
            public disc disc { get; set; }
        }

        public class track
        {
            public int id { get; set; }

            public string title { get; set; }

            public int duration { get; set; }

            public string url { get; set; }

            public disc Disc { get; set; }
        }

        public class disc
        {
            public int id { get; set; }

            public bool is_streamable { get; set; }

            public string sonos_media_format { get; set; }

            public DateTime updated_at { get; set; }

            public int price_in_cents { get; set; }

            public album album { get; set; }

            public List<track> tracks { get; set; }

            public void LoadTracks()
            {
                if (tracks == null)
                    tracks = GetDiscTracks(this.id);
            }
        }

        public class album
        {
            public string title { get; set; }

            public string is_streamable { get; set; }

            public string main_artist { get; set; }

            public string genre { get; set; }

            public string album_art { get; set; }
        }

        public class authResult
        {
            public user user { get; set; }

            public object email { get; set; }

            public object password { get; set; }

            public string message { get; set; }
        }

        public class user
        {
            public string name { get; set; }

            public string profile_name { get; set; }

            public string image_url { get; set; }

            public streaming_capabilities capabilities { get; set; }

            public string token { get; set; }
        }

        public class streaming_capabilities
        {
            public string lossless { get; set; }
        }
    }
}