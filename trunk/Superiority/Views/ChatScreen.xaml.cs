using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Messaging;
using Superiority.Messages.Bnet;
using Superiority.Views.SLGlue;
using Superiority.Model.Intellisense;
using System.Windows.Data;
using Superiority.ViewModels;

namespace Superiority.Views
{
    public partial class ChatScreen : Page
    {
#if DEBUG
        public static WeakReference wr;
#endif
        public ChatScreen()
        {
            InitializeComponent();

#if DEBUG
            if (wr == null)
                wr = new WeakReference(this);
            else
            {
                GC.Collect();
                if (wr.IsAlive)
                    System.Diagnostics.Debug.WriteLine("Chat Screen is alive");

                wr.Target = this;
            }
#endif

            Messenger.Default.Register<ScrollDownMessage>(this, Superiority.ViewModels.MainWindowViewModel.CommunicationToken, 
            (ScrollDownMessage msg) =>
            {
               ChatBox.Selection.Select(ChatBox.ContentEnd, ChatBox.ContentEnd);
            });

            ChatBox.Loaded += ChatBox_Loaded;
            SendBox.ItemFilter += SearchEntries;
            SendBox.DropDownClosed += SendBox_DropDownClosed;
        }

        void SendBox_DropDownClosed(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            SendBox.ItemsSource = null;
            SendBox.ItemsSource = ((AccountViewModel)DataContext).Bnet.AutoCompletionEntries;
        }

        bool SearchEntries(string search, object o)
        {
            AutoCompletionEntry entry = o as AutoCompletionEntry;

            if (entry != null)
                if (entry.LiteralText.ToLower().StartsWith(search.ToLower()))
                    return true;

            return false;
        }

        void ChatBox_Loaded(object sender, RoutedEventArgs e)
        {
            ChatBox.Selection.Select(ChatBox.ContentEnd, ChatBox.ContentEnd);
            SendBox.ItemsSource = ((AccountViewModel)DataContext).Bnet.AutoCompletionEntries;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ItemsControl ic = (ItemsControl)UserList.Content;

            foreach (var item in unregisterList)
                item.Loaded -= Border_Loaded;

            DataContext = null;

            SendBox.DropDownClosed -= SendBox_DropDownClosed;
            ChatBox.Loaded -= ChatBox_Loaded;
            SendBox.ItemFilter -= SearchEntries;
            LayoutRoot.Background = null;
            Messenger.Default.Unregister<ScrollDownMessage>(this);

            base.OnNavigatingFrom(e);
        }

        private int idx = 0;
        private List<Border> unregisterList = new List<Border>();
        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            unregisterList.Add((Border)sender);

            ItemsControl ic = (ItemsControl)UserList.Content;

            var sb = new Storyboard();
            var db = new DoubleAnimation();
            var db1 = new DoubleAnimation();
            var db2 = new DoubleAnimation();

            db.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
            db.From = 0;
            db.To = 1;
            db.BeginTime = new TimeSpan(0, 0, 0, 0, idx * 100);
            Storyboard.SetTargetProperty(db, new PropertyPath(Border.OpacityProperty));

            db1.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            db1.To = 1;
            db1.BeginTime = new TimeSpan(0, 0, 0, 0, idx * 100);
            Storyboard.SetTargetProperty(db1, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

            db2.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            db2.To = 1;
            db2.BeginTime = new TimeSpan(0, 0, 0, 0, idx * 100);
            Storyboard.SetTargetProperty(db2, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            sb.Children.Add(db);
            sb.Children.Add(db1);
            sb.Children.Add(db2);

            Storyboard.SetTarget(sb, (DependencyObject)sender);
            sb.Begin();

            idx++;
            if (idx >= ic.Items.Count)
                idx = 0;
        }
    }
}