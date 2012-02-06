﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Superiority.Model.Intellisense
{
    public enum AutoCompletionEntryType : byte
    {
        RecentChannel = 0,
        Command,
        FavoriteChannel
    }

    public class AutoCompletionEntry
    {
        public AutoCompletionEntryType EntryType
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string LiteralText
        {
            get;
            set;
        }

        public override string ToString()
        {
            return LiteralText;
        }
    }
}
