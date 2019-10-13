// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Telegram.Controls.Profiling
{
    public static class ApplicationSpace
    {
        public static Frame RootFrame
        {
            get
            {
                return Application.Current.RootVisual as Frame;
            }
        }

        public static bool IsDesignMode
        {
            get
            {
                return DesignerProperties.IsInDesignTool;
            }
        }

        public static Dispatcher CurrentDispatcher
        {
            get
            {
                return Deployment.Current.Dispatcher;
            }
        }

        public static int ScaleFactor()
        {
            return 100;
        }
    }
}
