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
using Superiority.Model.Intellisense;

namespace Superiority.Views.SLGlue
{
    public class UserListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DetailedTemplate
        {
            get;
            set;
        }

        public DataTemplate SimpleTemplate
        {
            get;
            set;
        }

        protected override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var obj = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(container))))));

            if (((Grid)obj).ColumnDefinitions[0].ActualWidth < 253)
                return SimpleTemplate;
            
            return DetailedTemplate;
        }
    }
}
