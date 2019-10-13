// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.Threading;

namespace Telegram.EmojiPanel.Controls.Utilites
{
    public class DelayedExecutor
    {
        class ExecutionInfo
        {
            public Action Action { get; set; }
            public DateTime Timestamp { get; set; }
        }

        ExecutionInfo m_executionInfo;
        Timer m_timer;
        int m_delay;
        bool m_timerIsActive;
        object m_lockObj = new object();

        public DelayedExecutor(int delay) // TO DO : add IDateTimeProvider dependency to remove dependency on DateTime
        {
            m_delay = delay;
            m_timer = new Timer(TimerCallback);
        }

        public void AddToDelayedExecution(Action action)
        {
            lock (m_lockObj)
            {
                m_executionInfo = new ExecutionInfo() { Action = action, Timestamp = DateTime.Now };
            }

            ChangeTimer(true);
        }

        private void TimerCallback(object state)
        {
            Action executeAction = null;

            lock (m_lockObj)
            {
                if (m_executionInfo != null)
                {
                    if (DateTime.Now - m_executionInfo.Timestamp >= TimeSpan.FromMilliseconds(m_delay))
                    {
                        Debug.WriteLine("Action is set to be executed.");
                        executeAction = m_executionInfo.Action;
                        m_executionInfo = null;
                        ChangeTimer(false);
                    }
                }
            }
            if (executeAction != null)
            {
                try
                {
                    executeAction();
                }
                catch (Exception exc)
                {
                    //Logger.Instance.Error("Exeption during delayed execution", exc);
                }
            }
        }

        private void ChangeTimer(bool activate)
        {
            if (activate && !m_timerIsActive)
            {
                lock (m_timer)
                {
                    m_timerIsActive = true;
                    m_timer.Change(m_delay, m_delay);
                }
            }
            else if (!activate && m_timerIsActive)
            {
                lock (m_timer)
                {
                    m_timerIsActive = false;
                    m_timer.Change(Timeout.Infinite, 0);
                }
            }
        }
    }
}
