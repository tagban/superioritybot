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
using GalaSoft.MvvmLight.Command;

namespace Superiority.Views
{
    public partial class Home : Page
    {
#if DEBUG
        public static WeakReference wr;
#endif
        public Home()
        {
            InitializeComponent();
#if DEBUG
            if (wr == null)
                wr = new WeakReference(this);
            else
            {
                GC.Collect();
                if (wr.IsAlive)
                    System.Diagnostics.Debug.WriteLine("Account Screen is alive");

                wr.Target = this;
            }
#endif
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateAccountDialog d = new CreateAccountDialog();
            d.DataContext = this.DataContext;
            d.Show();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            foreach (var item in toDispose)
            {
                var triggers = System.Windows.Interactivity.Interaction.GetTriggers(item);
                foreach (var trigger in triggers)
                {
                    foreach (var action in trigger.Actions)
                    {
                        var act = action as EventToCommand;
                        act.Command = null;
                        action.Detach();
                    }

                    trigger.Detach();
                }

                item.Loaded-= Root_Loaded;
            }

            DataContext = null;

            base.OnNavigatedFrom(e);
        }

        private List<Border> toDispose = new List<Border>();
        private int idx = 0;
        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            toDispose.Add((Border)sender);

            ItemsControl ic = (ItemsControl)AccountsList;

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
