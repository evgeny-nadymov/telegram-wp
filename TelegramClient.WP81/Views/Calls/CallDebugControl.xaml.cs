// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.Phone.Networking.Voip;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using TelegramClient.Services;

namespace TelegramClient.Views.Calls
{
    public partial class CallDebugControl : UserControl
    {
        private DispatcherTimer _debugTimer = new DispatcherTimer{ Interval = TimeSpan.FromSeconds(0.5) };

        public CallDebugControl()
        {
            InitializeComponent();

            _debugTimer.Tick += DebugTimer_Tick;
        }

        private void DebugTimer_Tick(object sender, System.EventArgs eventArgs)
        {
            Debug.Text = IoC.Get<IVoIPService>().GetDebugString();
        }

        public void Start()
        {
            _debugTimer.Start();
        }

        public void Stop()
        {
            _debugTimer.Stop();
        }
    }
}
