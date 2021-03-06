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
using Jamcast.Extensibility.UI.Dialogs;

namespace Jamcast.Plugins.Murfie.UI.ViewModel
{
    internal class LoggedInViewModel : ObservableObject
    {
        public RelayCommand LogOutCommand { get; private set; }

        public event Action LoggedOut;
        public event Action ConfigurationChanged;

        public string Email
        {
            get
            {
                return Configuration.Instance.Email;
            }
        }

        public bool IsLosslessAvailable
        {
            get
            {
                return Configuration.Instance.IsLosslessAvailable;
            }
        }

        public bool IsLosslessEnabled
        {
            get
            {
                return Configuration.Instance.IsLosslessEnabled;
            }
            set
            {
                Configuration.Instance.IsLosslessEnabled = value;
                notifyConfigurationChanged();
                OnPropertyChanged("IsLosslessEnabled");
            }
        }

        public LoggedInViewModel()
        {
            this.LogOutCommand = new RelayCommand(logOut);
        }

        private void logOut(object param)
        {
            var dialog = JamcastDialog.CreateMessageDialog("Are you sure you want to log out? You'll no longer be able to access music in your Murfie collection.", "Confirm Log Out", DialogMode.YesNoCancel);
            dialog.Yes = new Action(() =>
            {
                Configuration.Instance.Email = null;
                Configuration.Instance.Password = null;
                Configuration.Instance.Save();
                this.LoggedOut();
            });
            dialog.Show();
        }

        private void notifyConfigurationChanged()
        {
            if (this.ConfigurationChanged != null)
                this.ConfigurationChanged();
        }
    }
}