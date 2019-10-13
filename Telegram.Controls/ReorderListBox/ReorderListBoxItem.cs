// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace ReorderListBox
{
    /// <summary>
    /// Extends ListBoxItem to support a styleable drag handle, drop-indicator spacing,
    /// and visual states and transitions for dragging/dropping and enabling/disabling the reorder capability.
    /// </summary>
    [TemplatePart(Name = ReorderListBoxItem.DragHandlePart, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = ReorderListBoxItem.DropBeforeSpacePart, Type = typeof(UIElement))]
    [TemplatePart(Name = ReorderListBoxItem.DropAfterSpacePart, Type = typeof(UIElement))]
    [TemplateVisualState(Name = ReorderListBoxItem.ReorderDisabledState, GroupName = ReorderListBoxItem.ReorderEnabledStateGroup)]
    [TemplateVisualState(Name = ReorderListBoxItem.ReorderEnabledState, GroupName = ReorderListBoxItem.ReorderEnabledStateGroup)]
    [TemplateVisualState(Name = ReorderListBoxItem.NotDraggingState, GroupName = ReorderListBoxItem.DraggingStateGroup)]
    [TemplateVisualState(Name = ReorderListBoxItem.DraggingState, GroupName = ReorderListBoxItem.DraggingStateGroup)]
    [TemplateVisualState(Name = ReorderListBoxItem.NoDropIndicatorState, GroupName = ReorderListBoxItem.DropIndicatorStateGroup)]
    [TemplateVisualState(Name = ReorderListBoxItem.DropBeforeIndicatorState, GroupName = ReorderListBoxItem.DropIndicatorStateGroup)]
    [TemplateVisualState(Name = ReorderListBoxItem.DropAfterIndicatorState, GroupName = ReorderListBoxItem.DropIndicatorStateGroup)]
    public class ReorderListBoxItem : ListBoxItem
    {
        #region Template part name constants

        public const string DragHandlePart = "DragHandle";
        public const string DropBeforeSpacePart = "DropBeforeSpace";
        public const string DropAfterSpacePart = "DropAfterSpace";

        #endregion

        #region Visual state name constants

        public const string ReorderEnabledStateGroup = "ReorderEnabledStates";
        public const string ReorderDisabledState = "ReorderDisabled";
        public const string ReorderEnabledState = "ReorderEnabled";

        public const string DraggingStateGroup = "DraggingStates";
        public const string NotDraggingState = "NotDragging";
        public const string DraggingState = "Dragging";

        public const string DropIndicatorStateGroup = "DropIndicatorStates";
        public const string NoDropIndicatorState = "NoDropIndicator";
        public const string DropBeforeIndicatorState = "DropBeforeIndicator";
        public const string DropAfterIndicatorState = "DropAfterIndicator";

        #endregion

        /// <summary>
        /// Creates a new ReorderListBoxItem and sets the default style key.
        /// The style key is used to locate the control template in Generic.xaml.
        /// </summary>
        public ReorderListBoxItem()
        {
            this.DefaultStyleKey = typeof(ReorderListBoxItem);
        }

        #region DropIndicatorHeight DependencyProperty

        public static readonly DependencyProperty DropIndicatorHeightProperty = DependencyProperty.Register(
            "DropIndicatorHeight", typeof(double), typeof(ReorderListBoxItem),
            new PropertyMetadata(0.0, (d, e) => ((ReorderListBoxItem)d).OnDropIndicatorHeightChanged(e)));

        /// <summary>
        /// Gets or sets the height of the drop-before and drop-after indicators.
        /// The drop-indicator visual states and transitions are automatically updated to use this height.
        /// </summary>
        public double DropIndicatorHeight
        {
            get
            {
                return (double)this.GetValue(ReorderListBoxItem.DropIndicatorHeightProperty);
            }
            set
            {
                this.SetValue(ReorderListBoxItem.DropIndicatorHeightProperty, value);
            }
        }

        /// <summary>
        /// Updates the drop-indicator height value for visual state and transition animations.
        /// </summary>
        /// <remarks>
        /// This is a workaround for the inability of visual states and transitions to do template binding
        /// in Silverlight 3. In SL4, they could bind directly to the DropIndicatorHeight property instead.
        /// </remarks>
        protected void OnDropIndicatorHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            Panel rootPanel = (Panel)VisualTreeHelper.GetChild(this, 0);
            VisualStateGroup vsg = ReorderListBoxItem.GetVisualStateGroup(
                rootPanel, ReorderListBoxItem.DropIndicatorStateGroup);
            if (vsg != null)
            {
                foreach (VisualState vs in vsg.States)
                {
                    foreach (Timeline animation in vs.Storyboard.Children)
                    {
                        this.UpdateDropIndicatorAnimationHeight((double)e.NewValue, animation);
                    }
                }
                foreach (VisualTransition vt in vsg.Transitions)
                {
                    foreach (Timeline animation in vt.Storyboard.Children)
                    {
                        this.UpdateDropIndicatorAnimationHeight((double)e.NewValue, animation);
                    }
                }
            }
        }

        /// <summary>
        /// Helper for the UpdateDropIndicatorAnimationHeight method.
        /// </summary>
        private void UpdateDropIndicatorAnimationHeight(double height, Timeline animation)
        {
            DoubleAnimation da = animation as DoubleAnimation;
            if (da != null)
            {
                string targetName = Storyboard.GetTargetName(da);
                PropertyPath targetPath = Storyboard.GetTargetProperty(da);
                if ((targetName == ReorderListBoxItem.DropBeforeSpacePart ||
                     targetName == ReorderListBoxItem.DropAfterSpacePart) &&
                    targetPath != null && targetPath.Path == "Height")
                {
                    if (da.From > 0 && da.From != height)
                    {
                        da.From = height;
                    }
                    if (da.To > 0 && da.To != height)
                    {
                        da.To = height;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a named VisualStateGroup for a framework element.
        /// </summary>
        private static VisualStateGroup GetVisualStateGroup(FrameworkElement element, string groupName)
        {
            VisualStateGroup result = null;
            System.Collections.IList groups = VisualStateManager.GetVisualStateGroups(element);
            if (groups != null)
            {
                foreach (VisualStateGroup group in groups)
                {
                    if (group.Name == groupName)
                    {
                        result = group;
                        break;
                    }
                }
            }
            return result;
        }

        #endregion

        #region IsReorderEnabled DependencyProperty

        public static readonly DependencyProperty IsReorderEnabledProperty = DependencyProperty.Register(
            "IsReorderEnabled", typeof(bool), typeof(ReorderListBoxItem),
            new PropertyMetadata(false, (d, e) => ((ReorderListBoxItem)d).OnIsReorderEnabledChanged(e)));

        /// <summary>
        /// Gets or sets a value indicating whether the drag handle should be shown.
        /// </summary>
        public bool IsReorderEnabled
        {
            get
            {
                return (bool)this.GetValue(ReorderListBoxItem.IsReorderEnabledProperty);
            }
            set
            {
                this.SetValue(ReorderListBoxItem.IsReorderEnabledProperty, value);
            }
        }

        protected void OnIsReorderEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            string visualState = (bool)e.NewValue ?
                ReorderListBoxItem.ReorderEnabledState : ReorderListBoxItem.ReorderDisabledState;
            VisualStateManager.GoToState(this, visualState, true);
        }

        #endregion

        #region DragHandleTemplate DependencyProperty

        public static readonly DependencyProperty DragHandleTemplateProperty = DependencyProperty.Register(
            "DragHandleTemplate", typeof(DataTemplate), typeof(ReorderListBoxItem), null);

        /// <summary>
        /// Gets or sets the template for the drag handle.
        /// </summary>
        public DataTemplate DragHandleTemplate
        {
            get
            {
                return (DataTemplate)this.GetValue(ReorderListBoxItem.DragHandleTemplateProperty);
            }
            set
            {
                this.SetValue(ReorderListBoxItem.DragHandleTemplateProperty, value);
            }
        }

        #endregion

        /// <summary>
        /// Gets the element (control template part) that serves as a handle for dragging the item. 
        /// </summary>
        public ContentPresenter DragHandle
        {
            get;
            private set;
        }

        /// <summary>
        /// Applies the control template, checks for required template parts, and initializes visual states.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.DragHandle = this.GetTemplateChild(ReorderListBoxItem.DragHandlePart) as ContentPresenter;

            if (this.DragHandle == null)
            {
                throw new InvalidOperationException("ReorderListBoxItem must have a DragHandle ContentPresenter part.");
            }

            VisualStateManager.GoToState(this, ReorderListBoxItem.ReorderDisabledState, false);
            VisualStateManager.GoToState(this, ReorderListBoxItem.NotDraggingState, false);
            VisualStateManager.GoToState(this, ReorderListBoxItem.NoDropIndicatorState, false);
        }
    }
}