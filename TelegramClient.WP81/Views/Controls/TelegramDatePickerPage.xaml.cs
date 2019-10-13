// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Shell;

namespace TelegramClient.Views.Controls
{
    public partial class TelegramDatePickerPage
    {
        public TelegramDatePickerPage(YearDataSource yearDataSource)
        {
            InitializeComponent();
            PrimarySelector.DataSource = yearDataSource;
            SecondarySelector.DataSource = new MonthDataSource();
            TertiarySelector.DataSource = new DayDataSource();
            InitializeDateTimePickerPage(PrimarySelector, SecondarySelector, TertiarySelector);
        }

        protected override IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern()
        {
            string pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToUpperInvariant();
            if (DateShouldFlowRTL())
            {
                char[] charArray = pattern.ToCharArray();
                Array.Reverse(charArray);
                pattern = new string(charArray);
            }
            return DateTimePickerPageBase.GetSelectorsOrderedByCulturePattern(pattern, new[]
            {
                'Y',
                'M',
                'D'
            }, new []
            {
                PrimarySelector,
                SecondarySelector,
                TertiarySelector
            });
        }

        internal static bool DateShouldFlowRTL()
        {
            var letterIsoLanguageName = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (letterIsoLanguageName != "ar")
                return letterIsoLanguageName == "fa";
            return true;
        }

        public override void SetFlowDirection(FlowDirection flowDirection)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class DateTimePickerPageBase : PhoneApplicationPage, IDateTimePickerPage
    {
        private const string VisibilityGroupName = "VisibilityStates";
        private const string OpenVisibilityStateName = "Open";
        private const string ClosedVisibilityStateName = "Closed";
        private const string StateKey_Value = "DateTimePickerPageBase_State_Value";
        private LoopingSelector _primarySelectorPart;
        private LoopingSelector _secondarySelectorPart;
        private LoopingSelector _tertiarySelectorPart;
        private Storyboard _closedStoryboard;
        private DateTime? _value;

        public DateTime? Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                DateTimeWrapper dateTimeWrapper = new DateTimeWrapper(this._value.GetValueOrDefault(DateTime.Now));
                this._primarySelectorPart.DataSource.SelectedItem = (object)dateTimeWrapper;
                this._secondarySelectorPart.DataSource.SelectedItem = (object)dateTimeWrapper;
                this._tertiarySelectorPart.DataSource.SelectedItem = (object)dateTimeWrapper;
            }
        }

        protected void InitializeDateTimePickerPage(LoopingSelector primarySelector, LoopingSelector secondarySelector, LoopingSelector tertiarySelector)
        {
            if (primarySelector == null)
                throw new ArgumentNullException("primarySelector");
            if (secondarySelector == null)
                throw new ArgumentNullException("secondarySelector");
            if (tertiarySelector == null)
                throw new ArgumentNullException("tertiarySelector");
            this._primarySelectorPart = primarySelector;
            this._secondarySelectorPart = secondarySelector;
            this._tertiarySelectorPart = tertiarySelector;
            this._primarySelectorPart.DataSource.SelectionChanged += new EventHandler<SelectionChangedEventArgs>(this.OnDataSourceSelectionChanged);
            this._secondarySelectorPart.DataSource.SelectionChanged += new EventHandler<SelectionChangedEventArgs>(this.OnDataSourceSelectionChanged);
            this._tertiarySelectorPart.DataSource.SelectionChanged += new EventHandler<SelectionChangedEventArgs>(this.OnDataSourceSelectionChanged);
            this._primarySelectorPart.IsExpandedChanged += new DependencyPropertyChangedEventHandler(this.OnSelectorIsExpandedChanged);
            this._secondarySelectorPart.IsExpandedChanged += new DependencyPropertyChangedEventHandler(this.OnSelectorIsExpandedChanged);
            this._tertiarySelectorPart.IsExpandedChanged += new DependencyPropertyChangedEventHandler(this.OnSelectorIsExpandedChanged);
            this._primarySelectorPart.Visibility = Visibility.Collapsed;
            this._secondarySelectorPart.Visibility = Visibility.Collapsed;
            this._tertiarySelectorPart.Visibility = Visibility.Collapsed;
            int num = 0;
            foreach (LoopingSelector loopingSelector in this.GetSelectorsOrderedByCulturePattern())
            {
                Grid.SetColumn((FrameworkElement)loopingSelector, num);
                loopingSelector.Visibility = Visibility.Visible;
                ++num;
            }
            FrameworkElement child = VisualTreeHelper.GetChild((DependencyObject)this, 0) as FrameworkElement;
            if (child != null)
            {
                foreach (VisualStateGroup visualStateGroup in (IEnumerable)VisualStateManager.GetVisualStateGroups(child))
                {
                    if ("VisibilityStates" == visualStateGroup.Name)
                    {
                        foreach (VisualState state in (IEnumerable)visualStateGroup.States)
                        {
                            if ("Closed" == state.Name && state.Storyboard != null)
                            {
                                this._closedStoryboard = state.Storyboard;
                                this._closedStoryboard.Completed += new EventHandler(this.OnClosedStoryboardCompleted);
                            }
                        }
                    }
                }
            }
            if (this.ApplicationBar != null)
            {
                foreach (object button in (IEnumerable)this.ApplicationBar.Buttons)
                {
                    IApplicationBarIconButton applicationBarIconButton = button as IApplicationBarIconButton;
                    if (applicationBarIconButton != null)
                    {
                        if ("DONE" == applicationBarIconButton.Text)
                        {
                            applicationBarIconButton.Text = ControlResources.DateTimePickerDoneText;
                            applicationBarIconButton.Click += new EventHandler(this.OnDoneButtonClick);
                        }
                        else if ("CANCEL" == applicationBarIconButton.Text)
                        {
                            applicationBarIconButton.Text = ControlResources.DateTimePickerCancelText;
                            applicationBarIconButton.Click += new EventHandler(this.OnCancelButtonClick);
                        }
                    }
                }
            }
            VisualStateManager.GoToState((Control)this, "Open", true);
        }

        private void OnDataSourceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataSource dataSource = (DataSource)sender;
            this._primarySelectorPart.DataSource.SelectedItem = dataSource.SelectedItem;
            this._secondarySelectorPart.DataSource.SelectedItem = dataSource.SelectedItem;
            this._tertiarySelectorPart.DataSource.SelectedItem = dataSource.SelectedItem;
        }

        private void OnSelectorIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
                return;
            this._primarySelectorPart.IsExpanded = sender == this._primarySelectorPart;
            this._secondarySelectorPart.IsExpanded = sender == this._secondarySelectorPart;
            this._tertiarySelectorPart.IsExpanded = sender == this._tertiarySelectorPart;
        }

        private void OnDoneButtonClick(object sender, System.EventArgs e)
        {
            this._value = new DateTime?(((DateTimeWrapper)this._primarySelectorPart.DataSource.SelectedItem).DateTime);
            this.ClosePickerPage();
        }

        private void OnCancelButtonClick(object sender, System.EventArgs e)
        {
            this._value = new DateTime?();
            this.ClosePickerPage();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            e.Cancel = true;
            this.ClosePickerPage();
        }

        private void ClosePickerPage()
        {
            if (this._closedStoryboard != null)
                VisualStateManager.GoToState((Control)this, "Closed", true);
            else
                this.OnClosedStoryboardCompleted((object)null, (System.EventArgs)null);
        }

        private void OnClosedStoryboardCompleted(object sender, System.EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        protected abstract IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern();

        protected static IEnumerable<LoopingSelector> GetSelectorsOrderedByCulturePattern(string pattern, char[] patternCharacters, LoopingSelector[] selectors)
        {
            if (pattern == null)
                throw new ArgumentNullException("pattern");
            if (patternCharacters == null)
                throw new ArgumentNullException("patternCharacters");
            if (selectors == null)
                throw new ArgumentNullException("selectors");
            if (patternCharacters.Length != selectors.Length)
                throw new ArgumentException("Arrays must contain the same number of elements.");
            List<Tuple<int, LoopingSelector>> source = new List<Tuple<int, LoopingSelector>>(patternCharacters.Length);
            for (int index = 0; index < patternCharacters.Length; ++index)
                source.Add(new Tuple<int, LoopingSelector>(pattern.IndexOf(patternCharacters[index]), selectors[index]));
            return source.Where<Tuple<int, LoopingSelector>>((Func<Tuple<int, LoopingSelector>, bool>)(p => -1 != p.Item1)).OrderBy<Tuple<int, LoopingSelector>, int>((Func<Tuple<int, LoopingSelector>, int>)(p => p.Item1)).Select<Tuple<int, LoopingSelector>, LoopingSelector>((Func<Tuple<int, LoopingSelector>, LoopingSelector>)(p => p.Item2)).Where<LoopingSelector>((Func<LoopingSelector, bool>)(s => null != s));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            base.OnNavigatedFrom(e);
            if (!("app://external/" == e.Uri.ToString()))
                return;
            this.State["DateTimePickerPageBase_State_Value"] = (object)this.Value;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            base.OnNavigatedTo(e);
            if (!this.State.ContainsKey("DateTimePickerPageBase_State_Value"))
                return;
            this.Value = this.State["DateTimePickerPageBase_State_Value"] as DateTime?;
            if (!this.NavigationService.CanGoBack)
                return;
            this.NavigationService.GoBack();
        }

        public abstract void SetFlowDirection(FlowDirection flowDirection);
    }

    public abstract class DataSource : ILoopingSelectorDataSource
    {
        private DateTimeWrapper _selectedItem;

        public object SelectedItem
        {
            get
            {
                return (object)this._selectedItem;
            }
            set
            {
                if (value == this._selectedItem)
                    return;
                DateTimeWrapper dateTimeWrapper = (DateTimeWrapper)value;
                if (dateTimeWrapper != null && this._selectedItem != null && !(dateTimeWrapper.DateTime != this._selectedItem.DateTime))
                    return;
                object selectedItem = (object)this._selectedItem;
                this._selectedItem = dateTimeWrapper;
                EventHandler<SelectionChangedEventArgs> selectionChanged = this.SelectionChanged;
                if (selectionChanged == null)
                    return;
                selectionChanged((object)this, new SelectionChangedEventArgs((IList)new object[1]
        {
          selectedItem
        }, (IList)new object[1]
        {
          (object) this._selectedItem
        }));
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public object GetNext(object relativeTo)
        {
            DateTime? relativeTo1 = this.GetRelativeTo(((DateTimeWrapper)relativeTo).DateTime, 1);
            if (!relativeTo1.HasValue)
                return (object)null;
            return (object)new DateTimeWrapper(relativeTo1.Value);
        }

        public object GetPrevious(object relativeTo)
        {
            DateTime? relativeTo1 = this.GetRelativeTo(((DateTimeWrapper)relativeTo).DateTime, -1);
            if (!relativeTo1.HasValue)
                return (object)null;
            return (object)new DateTimeWrapper(relativeTo1.Value);
        }

        protected abstract DateTime? GetRelativeTo(DateTime relativeDate, int delta);
    }

    public class YearDataSource : DataSource
    {
        private int _maxYear = DateTime.Now.Year;

        public int MaxYear
        {
            get { return _maxYear; }
            set { _maxYear = value; }
        }

        private int _minYear = 1900;

        public int MinYear
        {
            get { return _minYear; }
            set { _minYear = value; }
        }

        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            if (MinYear == relativeDate.Year && delta < 0 || MaxYear == relativeDate.Year && delta > 0)
                return new DateTime?();
            int year = relativeDate.Year + delta;
            int day = Math.Min(relativeDate.Day, DateTime.DaysInMonth(year, relativeDate.Month));
            return new DateTime?(new DateTime(year, relativeDate.Month, day, relativeDate.Hour, relativeDate.Minute, relativeDate.Second));
        }
    }

    public class MonthDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            int num = 12;
            int month = (num + relativeDate.Month - 1 + delta) % num + 1;
            int day = Math.Min(relativeDate.Day, DateTime.DaysInMonth(relativeDate.Year, month));
            return new DateTime?(new DateTime(relativeDate.Year, month, day, relativeDate.Hour, relativeDate.Minute, relativeDate.Second));
        }
    }

    public class DayDataSource : DataSource
    {
        protected override DateTime? GetRelativeTo(DateTime relativeDate, int delta)
        {
            int num = DateTime.DaysInMonth(relativeDate.Year, relativeDate.Month);
            int day = (num + relativeDate.Day - 1 + delta) % num + 1;
            return new DateTime?(new DateTime(relativeDate.Year, relativeDate.Month, day, relativeDate.Hour, relativeDate.Minute, relativeDate.Second));
        }
    }
}