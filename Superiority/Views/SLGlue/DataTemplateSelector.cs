using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Superiority.Views
{
    // Need to implement this because Silverlight lacks DataTemplateSelectors like in Wpf. This is a hackish way around that.
    // Deriving classes must explicitly make space for the DataTemplates they want. It's annoying, but since I'll only be using 
    // them in a few places (Notifications mainly.. maybe Chat), it's not that bad.
    public abstract class DataTemplateSelector : ContentControl
    {
        protected virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ContentTemplate = SelectTemplate(Content, this);
            return base.ArrangeOverride(finalSize);
        }
    }
}
