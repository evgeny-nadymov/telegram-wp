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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using TelegramClient.Animation.LinqToVisualTree;

namespace TelegramClient.Animation
{
  public static class MetroInMotion
  {


      public static int GetPivotIndex(DependencyObject obj)
      {
          return (int)obj.GetValue(PivotIndexProperty);
      }

      public static void SetPivotIndex(DependencyObject obj, int value)
      {
          obj.SetValue(PivotIndexProperty, value);
      }


      public static readonly DependencyProperty PivotIndexProperty =
          DependencyProperty.RegisterAttached("PivotIndex", typeof(int),
          typeof(MetroInMotion), new PropertyMetadata(-1));

    #region AnimationLevel

    public static int GetAnimationLevel(DependencyObject obj)
    {
      return (int)obj.GetValue(AnimationLevelProperty);
    }

    public static void SetAnimationLevel(DependencyObject obj, int value)
    {
      obj.SetValue(AnimationLevelProperty, value);
    }


    public static readonly DependencyProperty AnimationLevelProperty =
        DependencyProperty.RegisterAttached("AnimationLevel", typeof(int),
        typeof(MetroInMotion), new PropertyMetadata(-1));

    #endregion

    #region IsPivotAnimated

    public static bool GetIsPivotAnimated(DependencyObject obj)
    {
      return (bool)obj.GetValue(IsPivotAnimatedProperty);
    }

    public static void SetIsPivotAnimated(DependencyObject obj, bool value)
    {
      obj.SetValue(IsPivotAnimatedProperty, value);
    }

    public static readonly DependencyProperty IsPivotAnimatedProperty =
        DependencyProperty.RegisterAttached("IsPivotAnimated", typeof(bool),
        typeof(MetroInMotion), new PropertyMetadata(false, OnIsPivotAnimatedChanged));

    private static void OnIsPivotAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
    {
      ItemsControl list = d as ItemsControl;

      list.Loaded += (s2, e2) =>
      {

          // locate the pivot control that this list is within
          Pivot pivot = list.Ancestors<Pivot>().Single() as Pivot;

            var pivotIndex = GetPivotIndex(list);
            if (pivotIndex == -1)
            {


                var pivotItem = list.Ancestors<PivotItem>().Single();

                // and its index within the pivot
                pivotIndex = pivot.Items.IndexOf(pivotItem);
                                 
            }

          bool selectionChanged = false;

          pivot.SelectionChanged += (s3, e3) =>
            {
              selectionChanged = true;
            };

          // handle manipulation events which occur when the user
          // moves between pivot items
          pivot.ManipulationCompleted += (s, e) =>
            {
              if (!selectionChanged)
                return;

              selectionChanged = false;

              if (pivotIndex != pivot.SelectedIndex)
                return;
              
              // determine which direction this tab will be scrolling in from
              bool fromRight = e.TotalManipulation.Translation.X <= 0;

                            
              // iterate over each of the items in view
              var items = list.GetItemsInView().ToList();
              for (int index = 0; index < items.Count; index++ )
              {
                var lbi = items[index];

                list.Dispatcher.BeginInvoke(() =>
                {
                  var animationTargets = lbi.Descendants()
                                         .Where(p => MetroInMotion.GetAnimationLevel(p) > -1);
                  foreach (FrameworkElement target in animationTargets)
                  {
                    // trigger the required animation
                    GetSlideAnimation(target, fromRight).Begin();
                  }
                });
              };
             
            };
        };
    }


    #endregion

    /// <summary>
    /// Animates each element in order, creating a 'peel' effect. The supplied action
    /// is invoked when the animation ends.
    /// </summary>
    public static void Peel(this IEnumerable<FrameworkElement> elements, Action endAction)
    {
      var elementList = elements.ToList();
      var lastElement = elementList.Last();

      // iterate over all the elements, animating each of them
      double delay = 0;
      foreach (FrameworkElement element in elementList)
      {
        var sb = GetPeelAnimation(element, delay);

        // add a Completed event handler to the last element
        if (element.Equals(lastElement))
        {
          sb.Completed += (s, e) =>
            {
              endAction();
            };
        }

        sb.Begin();
        delay += 50;
      }
    }


    /// <summary>
    /// Enumerates all the items that are currently visible in am ItemsControl. This implementation assumes
    /// that a VirtualizingStackPanel is being used as the ItemsPanel.
    /// </summary>
    public static IEnumerable<FrameworkElement> GetItemsInView(this ItemsControl itemsControl)
    {
       // locate the stack panel that hosts the items
      VirtualizingStackPanel vsp = itemsControl.Descendants<VirtualizingStackPanel>().First() as VirtualizingStackPanel;

      // iterate over each of the items in view
      int firstVisibleItem = (int)vsp.VerticalOffset;
      int visibleItemCount = (int)vsp.ViewportHeight;
      for (int index = firstVisibleItem; index <= firstVisibleItem + visibleItemCount + 1; index++)
      {
        var item = itemsControl.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
        if (item == null)
          continue;

        yield return item;
      }
    }

    /// <summary>
    /// Creates a PlaneProjection and associates it with the given element, returning
    /// a Storyboard which will animate the PlaneProjection to 'peel' the item
    /// from the screen.
    /// </summary>
    private static Storyboard GetPeelAnimation(FrameworkElement element, double delay)
    {
      Storyboard sb;

      var projection = new PlaneProjection()
      {
        CenterOfRotationX = -0.1
      };
      element.Projection = projection;

      // compute the angle of rotation required to make this element appear
      // at a 90 degree angle at the edge of the screen.
      var width = element.ActualWidth;
      var targetAngle = Math.Atan(1000 / (width / 2));
      targetAngle = targetAngle * 180 / Math.PI;

      // animate the projection
      sb = new Storyboard();
      sb.BeginTime = TimeSpan.FromMilliseconds(delay);      
      sb.Children.Add(CreateAnimation(0, -(180 - targetAngle), 0.3, "RotationY", projection));
      sb.Children.Add(CreateAnimation(0, 23, 0.3, "RotationZ", projection));
      sb.Children.Add(CreateAnimation(0, -23, 0.3, "GlobalOffsetZ", projection));      
      return sb;
    }

    private static DoubleAnimation CreateAnimation(double from, double to, double duration,
      string targetProperty, DependencyObject target)
    {
      var db = new DoubleAnimation();
      db.To = to;
      db.From = from;
      db.EasingFunction = new SineEase();
      db.Duration = TimeSpan.FromSeconds(duration);
      Storyboard.SetTarget(db, target);
      Storyboard.SetTargetProperty(db, new PropertyPath(targetProperty));
      return db;
    }

    /// <summary>
    /// Creates a TranslateTransform and associates it with the given element, returning
    /// a Storyboard which will animate the TranslateTransform with a SineEase function
    /// </summary>
    private static Storyboard  GetSlideAnimation(FrameworkElement element, bool fromRight)
    {
      double from = fromRight ? 40 : -40;
      
      Storyboard sb;
      double delay = (GetAnimationLevel(element)) * 0.1 + 0.1;

      var trans = new TranslateTransform { X = from };
      element.RenderTransform = trans;

      sb = new Storyboard();
      sb.BeginTime = TimeSpan.FromSeconds(delay);
      sb.Children.Add(CreateAnimation(from, 0, 0.3, "X", trans));      
      return sb;
    }

  }
}
