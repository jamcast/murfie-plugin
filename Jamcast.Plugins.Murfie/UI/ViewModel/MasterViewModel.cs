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
namespace Jamcast.Plugins.Murfie.UI.ViewModel
{
    internal class MasterViewModel : ObservableObject
    {
        private object _currentView;
        private LoggedInViewModel _loggedInViewModel;
        private LoggedOutViewModel _loggedOutViewModel;

        public event Action OnNotifyFormIsDirty;

        public MasterViewModel()
        {
            _loggedInViewModel = new LoggedInViewModel();
            _loggedInViewModel.LoggedOut += _loggedInViewModel_LoggedOut;
            _loggedInViewModel.ConfigurationChanged += () =>
            {
                if (this.OnNotifyFormIsDirty != null)
                    this.OnNotifyFormIsDirty();
            };

            _loggedOutViewModel = new LoggedOutViewModel();
            _loggedOutViewModel.LoginSuccess += _loggedOutViewModel_LoginSuccess;

            this.CurrentView = Configuration.Instance.Email != null ? (object)_loggedInViewModel : _loggedOutViewModel;
        }

        private void _loggedInViewModel_LoggedOut()
        {
            _loggedOutViewModel.Reset();
            this.CurrentView = _loggedOutViewModel;
        }

        private void _loggedOutViewModel_LoginSuccess()
        {
            this.CurrentView = _loggedInViewModel;
        }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged("CurrentView");
            }
        }
    }
}