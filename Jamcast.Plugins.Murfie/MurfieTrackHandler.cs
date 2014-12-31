﻿/*-
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
using System.Net;
using Jamcast.Extensibility.MediaServer;
using Jamcast.Extensibility.Metadata;

namespace Jamcast.Plugins.Murfie
{
    public class MurfieTrackHandler : MediaRequestHandler
    {
        private API.track _track;

        public override RequestInitializationResult Initialize()
        {
            _track = API.GetMediaUri(Convert.ToInt32(this.Context.Data[0]), Convert.ToInt32(this.Context.Data[1]), Configuration.Instance.EnableFlac);
            var format = Configuration.Instance.EnableFlac ? MediaFormats.FLAC : MediaFormats.MP3;
            AudioRequestInitializationResult result = new AudioRequestInitializationResult()
            {
                CanProceed = true,
                IsConversion = this.Context.Format != format,
                SupportsSeeking = false,
                InputProperties = new AudioStreamProperties(format)
            };
            return result;
        }

        public override DataPipeBase RetrieveMedia()
        {
            HttpWebRequest request = WebRequest.Create(_track.url) as HttpWebRequest;
            return new StreamDataPipe("Murfie", request.GetResponse().GetResponseStream());
        }
    }
}