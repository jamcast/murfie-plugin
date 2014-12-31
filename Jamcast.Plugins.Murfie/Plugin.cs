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
using Jamcast.Extensibility;
using Jamcast.Plugins.Murfie.Renderers;

namespace Jamcast.Plugins.Murfie
{
    public class Plugin : SimplePlugin
    {
        public override Type RootObjectRendererType
        {
            get { return typeof(DiscListRenderer); }
        }

        public override Type ConfigurationPanelType
        {
            get
            {
                return typeof(Jamcast.Plugins.Murfie.UI.View.MasterView);
            }
        }

        public override Type SearchRendererType
        {
            get
            {
                return typeof(SearchRenderer);
            }
        }

        public override void OnPreRender()
        {
            base.OnPreRender();
            if (!API.IsLoggedIn())
                throw new PluginNotLoggedInException("Please log in to Murfie");
        }

        public override bool Startup()
        {
            PluginDataProvider.OnPluginDataChanged += PluginDataProvider_OnPluginDataChanged;
            doLogin();
            return true;
        }

        private void PluginDataProvider_OnPluginDataChanged(string key)
        {
            if (key.Equals(Configuration.CONFIGURATION_KEY))
            {
                Configuration.Refresh();
                doLogin();
            }
        }

        private void doLogin()
        {
            if (Configuration.Instance.Email != null)
            {
                API.Authenticate(Configuration.Instance.Email, Configuration.Instance.Password);
            }
            else
            {
                API.LogOut();
            }
        }
    }
}