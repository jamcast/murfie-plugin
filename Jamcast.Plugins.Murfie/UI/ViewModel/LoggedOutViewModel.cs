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
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jamcast.Plugins.Murfie.UI.ViewModel
{
    internal class LoggedOutViewModel : ObservableObject
    {
        private RelayCommand _logInCommand;
        private bool _isLoggingIn;
        private string _loginError;

        public string EmailOrUsername { get; set; }

        public event Action LoginFailed;

        public event Action LoginSuccess;

        public LoggedOutViewModel()
        {
            _logInCommand = new RelayCommand(logIn);
        }

        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set
            {
                _isLoggingIn = value;
                this.OnPropertyChanged("IsLoggingIn");
            }
        }

        public string LoginError
        {
            get { return _loginError; }
            set
            {
                _loginError = value;
                this.OnPropertyChanged("LoginError");
            }
        }

        public ICommand LogInCommand
        {
            get
            {
                return _logInCommand;
            }
        }

        public void Reset()
        {
            this.LoginError = null;
            this.IsLoggingIn = false;
            this.EmailOrUsername = null;
        }

        private void logIn(object password)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync(password);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var password = e.Argument as PasswordBox;
            this.IsLoggingIn = true;

            try
            {
                if (String.IsNullOrWhiteSpace(this.EmailOrUsername))
                    throw new ApplicationException("Email field cannot be left blank");
                string username = this.EmailOrUsername.Trim();
                var pw = password.Password;
                if (String.IsNullOrWhiteSpace(pw))
                    throw new ApplicationException("Password field cannot be left blank");
                
                if (API.Authenticate(username, pw))
                {
                    Configuration.Instance.Email = username;
                    Configuration.Instance.Password = pw;
                    Configuration.Instance.IsLosslessAvailable = API.LoggedInUser.capabilities.lossless != null;
                    Configuration.Instance.IsLosslessEnabled = Configuration.Instance.IsLosslessAvailable;
                    Configuration.Instance.Save();
                    this.LoginSuccess();
                }
                else
                {
                    this.LoginError = "Login failed, check please your credentials and try again";
                    return;
                }
            }
            catch (Exception ex)
            {
                this.LoginError = ex.Message;
                return;
            }
            finally
            {
                this.IsLoggingIn = false;
            }
        }
    }
}