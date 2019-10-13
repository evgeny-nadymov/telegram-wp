using System.Windows;

namespace TelegramClient.Controls.StartView
{
    internal abstract class InputDeltaArgs : InputBaseArgs
    {
        protected InputDeltaArgs(UIElement source, Point origin)
            : base(source, origin)
        {
        }

        public abstract Point DeltaTranslation { get; }

        public abstract Point CumulativeTranslation { get; }

        public abstract Point ExpansionVelocity { get; }

        public abstract Point LinearVelocity { get; }
    }
}
