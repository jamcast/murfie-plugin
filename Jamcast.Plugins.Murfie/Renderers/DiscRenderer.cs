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

using Jamcast.Extensibility.ContentDirectory;
using Jamcast.Extensibility.Metadata;

namespace Jamcast.Plugins.Murfie.Renderers
{
    [ContainerRenderer(ContainerType = ContainerType.Album)]
    public class DiscRenderer : ContainerRenderer
    {
        private API.track[] tracks;

        public override ObjectRenderInfo GetChildAt(int index)
        {
            return new ObjectRenderInfo(typeof(TrackRenderer), new API.PersistedTrackKey() { DiscID = tracks[index].Disc.id, TrackID = tracks[index].id });
        }

        public override int PrepareGetChildren(int startIndex, int count)
        {
            var disc = API.GetDisc((int)this.ObjectData);
            disc.LoadTracks();
            tracks = disc.tracks.ToArray();
            return tracks.Length;
        }

        public override Extensibility.Metadata.ServerObject GetMetadata()
        {
            var disc = API.GetDisc((int)this.ObjectData);
            return new AlbumContainer(disc.album.title, disc.album.main_artist, new ImageResource(new UriLocation(disc.album.album_art), MediaFormats.JPEG));
        }
    }
}