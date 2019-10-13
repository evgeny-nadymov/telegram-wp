using System.Windows;
using System.Windows.Input;

namespace TelegramClient.Controls.StartView
{
    internal class ManipulationGestureHelper : GestureHelper
    {
        public ManipulationGestureHelper(UIElement target, bool shouldHandleAllDrags)
            : base(target, shouldHandleAllDrags)
        {
        }

        protected override void HookEvents()
        {
            Target.ManipulationStarted += Target_ManipulationStarted;
            Target.ManipulationDelta += Target_ManipulationDelta;
            Target.ManipulationCompleted += Target_ManipulationCompleted;
        }

        private void Target_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            NotifyDown(new ManipulationBaseArgs(e));
        }

        private void Target_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            NotifyMove(new ManipulationDeltaArgs(e));
        }

        private void Target_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            NotifyUp(new ManiulationCompletedArgs(e));
        }

        private class ManipulationBaseArgs : InputBaseArgs
        {
            public ManipulationBaseArgs(ManipulationStartedEventArgs args)
                : base(args.ManipulationContainer, args.ManipulationOrigin)
            {
            }
        }

        private class ManipulationDeltaArgs : InputDeltaArgs
        {
            private ManipulationDeltaEventArgs _args;

            public ManipulationDeltaArgs(ManipulationDeltaEventArgs args)
                : base(args.ManipulationContainer, args.ManipulationOrigin)
            {
                _args = args;
            }


            public override Point DeltaTranslation
            {
                get
                {
                    return _args.DeltaManipulation.Translation;
                }
            }

            public override Point CumulativeTranslation
            {
                get
                {
                    return _args.CumulativeManipulation.Translation;
                }
            }

            public override Point ExpansionVelocity
            {
                get
                {
                    return _args.Velocities.ExpansionVelocity;
                }
            }

            public override Point LinearVelocity
            {
                get
                {
                    return _args.Velocities.LinearVelocity;
                }
            }
        }

        private class ManiulationCompletedArgs : InputCompletedArgs
        {
            private ManipulationCompletedEventArgs _args;

            public ManiulationCompletedArgs(ManipulationCompletedEventArgs args)
                : base(args.ManipulationContainer, args.ManipulationOrigin)
            {
                _args = args;
            }

            public override Point TotalTranslation
            {
                get
                {
                    return _args.TotalManipulation.Translation;
                }
            }

            public override Point FinalLinearVelocity
            {
                get
                {
                    if (_args.FinalVelocities != null)
                        return _args.FinalVelocities.LinearVelocity;
                    else
                        return new Point(0, 0);
                }
            }

            public override bool IsInertial
            {
                get
                {
                    return _args.IsInertial;
                }
            }
        }
    }
}
