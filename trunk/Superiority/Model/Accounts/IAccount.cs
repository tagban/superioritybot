using System;
using System.Collections.ObjectModel;

using Superiority.Model.Filters;

namespace Superiority.Model.Accounts
{
    public enum AccountTypes : byte
    {
        Bnet = 0,
        Irc
    }

    public interface IAccount
    {
        string AccountName { get; set; }
        AccountTypes AccountType { get; }

        ObservableCollection<ChatFilter> AccountFilters { get; }
    }
}
