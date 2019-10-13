using System.Windows;

namespace TelegramClient.Controls.StartView
{
    internal abstract class InputCompletedArgs : InputBaseArgs
    {
        protected InputCompletedArgs(UIElement source, Point origin)
            : base(source, origin)
        {
        }

        public abstract Point TotalTranslation { get; }

        public abstract Point FinalLinearVelocity { get; }

        public abstract bool IsInertial { get; }
    }
}
