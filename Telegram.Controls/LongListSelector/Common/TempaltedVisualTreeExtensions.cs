// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Telegram.Controls.Extensions;

namespace Telegram.Controls.LongListSelector.Common
{
    /// <summary>
    /// A static class providing methods for working with the visual tree using generics.  
    /// </summary>
    public static class TemplatedVisualTreeExtensions
    {

        #region GetFirstLogicalChildByType<T>(...)
        /// <summary>
        /// Retrieves the first logical child of a specified type using a 
        /// breadth-first search.  A visual element is assumed to be a logical 
        /// child of another visual element if they are in the same namescope.
        /// For performance reasons this method manually manages the queue 
        /// instead of using recursion.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <param name="applyTemplates">Specifies whether to apply templates on the traversed framework elements</param>
        /// <returns>The first logical child of the framework element of the specified type.</returns>
        internal static T GetFirstLogicalChildByType<T>(this FrameworkElement parent, bool applyTemplates)
            where T : FrameworkElement
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            Queue<FrameworkElement> queue = new Queue<FrameworkElement>();
            queue.Enqueue(parent);

            while (queue.Count > 0)
            {
                FrameworkElement element = queue.Dequeue();
                var elementAsControl = element as Control;
                if (applyTemplates && elementAsControl != null)
                {
                    elementAsControl.ApplyTemplate();
                }

                if (element is T && element != parent)
                {
                    return (T)element;
                }

                foreach (FrameworkElement visualChild in element.GetVisualChildren().OfType<FrameworkElement>())
                {
                    queue.Enqueue(visualChild);
                }
            }

            return null;
        }
        #endregion

        #region GetLogicalChildrenByType<T>(...)
        /// <summary>
        /// Retrieves all the logical children of a specified type using a 
        /// breadth-first search.  A visual element is assumed to be a logical 
        /// child of another visual element if they are in the same namescope.
        /// For performance reasons this method manually manages the queue 
        /// instead of using recursion.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <param name="applyTemplates">Specifies whether to apply templates on the traversed framework elements</param>
        /// <returns>The logical children of the framework element of the specified type.</returns>
        internal static IEnumerable<T> GetLogicalChildrenByType<T>(this FrameworkElement parent, bool applyTemplates)
                where T : FrameworkElement
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            if (applyTemplates && parent is Control)
            {
                ((Control)parent).ApplyTemplate();
            }

            Queue<FrameworkElement> queue =
               new Queue<FrameworkElement>(parent.GetVisualChildren().OfType<FrameworkElement>());

            while (queue.Count > 0)
            {
                FrameworkElement element = queue.Dequeue();
                if (applyTemplates && element is Control)
                {
                    ((Control)element).ApplyTemplate();
                }

                if (element is T)
                {
                    yield return (T)element;
                }

                foreach (FrameworkElement visualChild in element.GetVisualChildren().OfType<FrameworkElement>())
                {
                    queue.Enqueue(visualChild);
                }
            }
        }
        #endregion

    }
}
