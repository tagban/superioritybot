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
    public class AutoCompleteTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CommandTemplate
        {
            get;
            set;
        }
        
        protected override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            AutoCompletionEntry entry = item as AutoCompletionEntry;

            if (entry.EntryType == AutoCompletionEntryType.Command)
                return CommandTemplate;

            return null;
        }
    }
}
