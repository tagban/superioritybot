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
using System.Collections.Specialized;
using System.Collections;
using Superiority.Model.Bnet;

using GalaSoft.MvvmLight.Messaging;
using System.Globalization;
using Superiority.Model.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Superiority.Views.SLGlue
{
    public static class SpanInlinesSourceAttatchedProperty
    {
        public static readonly DependencyProperty InlinesSourceProperty =
            DependencyProperty.RegisterAttached("InlinesSource", typeof(INotifyCollectionChanged), typeof(SpanInlinesSourceAttatchedProperty), new PropertyMetadata(UpdateBinding));

        public static INotifyCollectionChanged GetInlinesSource(DependencyObject obj)
        {
            return (INotifyCollectionChanged)obj.GetValue(InlinesSourceProperty);
        }

        public static void SetInlinesSource(DependencyObject obj, INotifyCollectionChanged value)
        {
            obj.SetValue(InlinesSourceProperty, value);
        }

        private static WeakReference span;

        private static readonly object sync = new object();
        public static void UpdateBinding(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender == null)
                return;

            span = new WeakReference(sender);
            ((Span)span.Target).Inlines.Clear();

            INotifyCollectionChanged newSource = args.NewValue as INotifyCollectionChanged;
            INotifyCollectionChanged oldSource = args.OldValue as INotifyCollectionChanged;

            if (oldSource != null)
                oldSource.CollectionChanged -= OnSourceCollectionChanged;

            if (newSource == null)
                return;

            // Detach our View from the ViewModel source
            //EDIT: Bug fix to reflect PB7's new UI model.
            newSource.CollectionChanged -= OnSourceCollectionChanged;

            IEnumerable ns = newSource as IEnumerable;
            OnSourceCollectionChanged(ns, null);
            newSource.CollectionChanged += OnSourceCollectionChanged;
        }

        private static void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            lock (sync)
            {
                IEnumerable items;

                if (e != null)
                    if (e.NewItems != null)
                        items = (IEnumerable)e.NewItems;
                    else
                        items = (IEnumerable)sender;
                else
                    items = (IEnumerable)sender;

                if (items == null)
                    return;

                if (span.Target == null)
                    return;

                foreach (BnetEvent evt in items)
                {
                    Run usernameRun;
                    Run messageRun;
                    Run timestampRun;
                    Span overwhelmingSpan = new Span();

                    switch (evt.EventId)
                    {
                        case 0x02: // EID_JOIN                            
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());


                            messageRun.Text = string.Format("{0} has joined the channel.", evt.Username);
                            messageRun.Foreground = Application.Current.Resources["ChatJoinLeaveBrush"] as SolidColorBrush;
          
                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            
                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                        case 0x03: // EID_LEAVE
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();

                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            messageRun.Text = string.Format("{0} has left the channel.", evt.Username);
                            messageRun.Foreground = Application.Current.Resources["ChatJoinLeaveBrush"] as SolidColorBrush;

                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                        case 0x04: // EID_WHISPERFROM
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                            
                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            usernameRun.Foreground = Application.Current.Resources["ChatWhisperUserBrush"] as SolidColorBrush;
                            usernameRun.Text = string.Format("From {0}: ", evt.Username);

                            messageRun.Text = evt.Message;
                            messageRun.Foreground = Application.Current.Resources["ChatWhisperMessageBrush"] as SolidColorBrush;


                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(usernameRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                        case 0x05: // EID_TALK
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                            
                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            usernameRun.Foreground = Application.Current.Resources["ChatSpeakUserBrush"] as SolidColorBrush;
                            usernameRun.Text = string.Format("{0}: ", evt.Username);

                            messageRun.Text = evt.Message;
                            messageRun.Foreground = Application.Current.Resources["ChatSpeakMessageBrush"] as SolidColorBrush;

                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(usernameRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                        case 0x07: // EID_CHANNEL
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                            
                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            messageRun.Foreground = Application.Current.Resources["ChatJoinedChannelBrush"] as SolidColorBrush;
                            messageRun.Text = string.Format("Entered channel {0}", evt.Message);

                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                        case 0x12: // EID_INFO
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();

                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            messageRun.Text = evt.Message;
                            messageRun.Foreground = Application.Current.Resources["ChatInfoBrush"] as SolidColorBrush;

                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                        case 0x13: // EID_ERROR
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();

                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            messageRun.Text = evt.Message;
                            messageRun.Foreground = Application.Current.Resources["ChatErrorBrush"] as SolidColorBrush;

                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                        case 0x17: // EID_EMOTE
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                            
                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            usernameRun.Foreground = Application.Current.Resources["ChatSpeakUserBrush"] as SolidColorBrush;
                            usernameRun.Text = string.Format("{0} ", evt.Username);
                            usernameRun.FontWeight = FontWeights.Black;

                            messageRun.Text = evt.Message;
                            messageRun.Foreground = Application.Current.Resources["ChatSpeakUserBrush"] as SolidColorBrush;
                            messageRun.FontWeight = FontWeights.Black;


                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(usernameRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                        case 0x18: // EID_SELF
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                            
                            if (!evt.Message.Substring(0, 1).Contains("/"))
                            {
                                timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                                timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                                usernameRun.Foreground = Application.Current.Resources["ChatSelfUserBrush"] as SolidColorBrush;
                                usernameRun.Text = string.Format("{0}: ", evt.Username);
                                usernameRun.FontWeight = FontWeights.Black;

                                messageRun.Foreground = Application.Current.Resources["ChatSelfMessageBrush"] as SolidColorBrush;

                                overwhelmingSpan.Inlines.Add(timestampRun);
                                overwhelmingSpan.Inlines.Add(usernameRun);

                                string[] messages = evt.Message.Split(' ');
                                foreach (var msg in messages)
                                {
                                    if (msg.StartsWith("http://"))
                                    {
                                        if (!string.IsNullOrEmpty(messageRun.Text))
                                        {
                                            overwhelmingSpan.Inlines.Add(messageRun);
                                            messageRun = new Run();
                                            messageRun.Foreground = Application.Current.Resources["ChatSelfMessageBrush"] as SolidColorBrush;
                                        }
                                        
                                        Hyperlink hl = new Hyperlink();
                                        hl.Foreground = Application.Current.Resources["ChatSelfMessageBrush"] as SolidColorBrush;
                                        hl.NavigateUri = new Uri(msg);
                                        hl.Inlines.Add(new Run() { Text = msg });
                                        hl.TargetName = "_blank";

                                        overwhelmingSpan.Inlines.Add(hl);
                                        messageRun.Text += " ";
                                    }
                                    else
                                    {
                                        messageRun.Text += msg;
                                        messageRun.Text += " ";
                                    }
                                }

                                if (!string.IsNullOrEmpty(messageRun.Text))
                                {
                                    overwhelmingSpan.Inlines.Add(messageRun);
                                }
                                
                                ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                                ((Span)span.Target).Inlines.Add(new LineBreak());
                            }

                            break;
                        case 0x19: // EID_GOODMSG
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                           
                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            messageRun.Text = evt.Message;
                            messageRun.Foreground = Application.Current.Resources["ChatCompletedBrush"] as SolidColorBrush;


                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;

                        case 0x20: // EID_DISCONNECT
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                           
                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            messageRun.Text = "Disconnected from Battle.net.";
                            messageRun.Foreground = Application.Current.Resources["ChatErrorBrush"] as SolidColorBrush;


                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;

                        case 0x0A: // EID_WHISPERSENT
                            usernameRun = new Run();
                            messageRun = new Run();
                            timestampRun = new Run();
                            
                            timestampRun.Foreground = Application.Current.Resources["ChatTimeStampBrush"] as SolidColorBrush;
                            timestampRun.Text = string.Format("{0}{1} ", evt.TimeStamp.ToString("hh:mm", CultureInfo.InvariantCulture), evt.TimeStamp.ToString("tt", CultureInfo.InvariantCulture).ToLower());

                            usernameRun.Foreground = Application.Current.Resources["ChatWhisperUserBrush"] as SolidColorBrush;
                            usernameRun.Text = string.Format("To {0}: ", evt.Username);

                            messageRun.Text = evt.Message;
                            messageRun.Foreground = Application.Current.Resources["ChatWhisperMessageBrush"] as SolidColorBrush;


                            overwhelmingSpan.Inlines.Add(timestampRun);
                            overwhelmingSpan.Inlines.Add(usernameRun);
                            overwhelmingSpan.Inlines.Add(messageRun);

                            ((Span)span.Target).Inlines.Add(overwhelmingSpan);
                            ((Span)span.Target).Inlines.Add(new LineBreak());

                            break;
                    }
                }
            }
        }
    }
}
