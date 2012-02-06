using System;
using System.Net;

namespace Superiority.Model.Filters
{
    public enum ChatFilterTypes : byte
    {
        MessageFilter = 1,
        UsernameFilter
    }

    public struct ChatFilter
    {
        public ChatFilterTypes FilterType { get; set; }
        public string FilterContent { get; set; }
    }
}
