using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace TelegramClient.Controls.StartView
{
    internal class DragEventArgs : GestureEventArgs
    {
        public DragEventArgs()
        {
        }

        public DragEventArgs(InputDeltaArgs args)
        {
            if (args != null)
            {
                CumulativeDistance = args.CumulativeTranslation;
                DeltaDistance = args.DeltaTranslation;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public bool IsTouchComplete { get; private set; }

        public Point DeltaDistance { get; private set; }

        public Point CumulativeDistance { get; internal set; }

        public void MarkAsFinalTouchManipulation()
        {
            IsTouchComplete = true;
        }
    }
}
