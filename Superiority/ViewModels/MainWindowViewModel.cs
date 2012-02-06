using System;
using System.Collections.Generic;
using System.Windows.Threading;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;

using Superiority.Messages;
using Superiority.Model;
using Superiority.Model.Bnet;
using Superiority.Model.Accounts;
using System.Collections.ObjectModel;
using Superiority.Messages.Bnet;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using Superiority.ViewModels.Bnet;

namespace Superiority.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDataErrorInfo
    {
        public static Guid CommunicationToken = Guid.NewGuid();

        #region Account related properties
        /// <summary>
        /// Current account, ChatView's current DataContext
        /// </summary>
        private AccountViewModel currentAccount;
        public AccountViewModel CurrentAccount
        {
            get
            {
                return currentAccount;
            }
            set
            {
                currentAccount = value;
                VerifyPropertyName("CurrentAccount");
                RaisePropertyChanged("CurrentAccount");
            }
        }

        private RelayCommand<AccountViewModel> changeCurrentEditingAccountCommand;
        public RelayCommand<AccountViewModel> ChangeCurrentEditingAccountCommand
        {
            get
            {
                if (changeCurrentEditingAccountCommand == null)
                    changeCurrentEditingAccountCommand = new RelayCommand<AccountViewModel>((AccountViewModel account) =>
                    {
                        if (account == null)
                            return;

                        CurrentEditingAccount = account;
                    });

                return changeCurrentEditingAccountCommand;
            }
        }

        private AccountViewModel currentEditingAccount;
        public AccountViewModel CurrentEditingAccount
        {
            get
            {
                return currentEditingAccount;
            }
            set
            {
                currentEditingAccount = value;
                VerifyPropertyName("CurrentEditingAccount");
                RaisePropertyChanged("CurrentEditingAccount");
            }
        }

        public string AccountNameCandidate
        {
            get;
            set;
        }

        private RelayCommand createAccountCommand;
        public RelayCommand CreateAccountCommand
        {
            get
            {
                if (createAccountCommand == null)
                {
                    createAccountCommand = new RelayCommand(() =>
                    {
                        if (this["AccountNameCandidate"] == null)
                        {
                            BnetAccount account = new BnetAccount() { AccountName = AccountNameCandidate, Product = BnetProduct.Starcraft, HomeChannel = "Op {{}}", Server = BnetServer.UsEast };
                            AccountViewModel avm = new AccountViewModel(account);

                            if (CurrentEditingAccount == null)
                                CurrentEditingAccount = avm;

                            AccountsModel.BotAccounts.Add(account);
                            Accounts.Add(avm);

                            AccountNameCandidate = string.Empty;

                            SaveSettings();
                        }
                    });
                }

                return createAccountCommand;
            }
            set
            {
            }
        }

        public Accounts AccountsModel
        {
            get;
            set;
        }

        public ObservableCollection<AccountViewModel> Accounts
        {
            get;
            set;
        }

        #region Validation
        public string Error
        {
            get
            {
                return string.Empty;
            }
        }

        public string this[string propertyName]
        {
            get
            {
                string error = null;

                switch (propertyName)
                {
                    case "AccountNameCandidate":
                        if (string.IsNullOrEmpty(AccountNameCandidate))
                            error = "You must provide an account name.";
                        else if (AccountNameCandidate.Length > 20)
                            error = "Your account name cannot be longer than 20 characters.";

                        foreach (var acc in AccountsModel.BotAccounts)
                        {
                            if (acc.AccountName.ToLower() == AccountNameCandidate.ToLower())
                            {
                                error = "An account by this name already exists.";

                                break;
                            }
                        }
                        break;
                }

                return error;
            }
        }
        #endregion
        #endregion

        public void SaveSettings()
        {
            Superiority.Model.Accounts.Accounts.Serialize(AccountsModel);
        }

        public MainWindowViewModel()
        {
            if (IsInDesignMode)
            {
                // Setup a mock CurrentAccount, TODO: Mock Notifications
              //  CurrentAccount = new AccountViewModel(new BnetAccount() { AccountName = "Hello", Product = BnetProduct.Warcraft3, Username = "Warrior" });
            }
            else
            {
                // Load Accounts from disk
                Accounts = new ObservableCollection<AccountViewModel>();
                AccountsModel = Superiority.Model.Accounts.Accounts.Deserialize();
                AccountNameCandidate = string.Empty;

                foreach (var account in AccountsModel.BotAccounts)
                    Accounts.Add(new AccountViewModel((BnetAccount)account));

                if (Accounts.Count > 0)
                {
                    CurrentAccount = Accounts[0];
                    CurrentEditingAccount = Accounts[0];
                }
                else
                {
                    // Treat this as if it were our first run
                }

                // Setup our communication channels
                Messenger.Default.Register<ScrollDownMessage>(this, (ScrollDownMessage msg) =>
                {
                    if (CurrentAccount != null)
                    {
                        if (msg.Source == CurrentAccount.Bnet)
                            Messenger.Default.Send<ScrollDownMessage>(msg, CommunicationToken);
                    }
                });

                Messenger.Default.Register<BnetSwitchAccountMessage>(this, (BnetSwitchAccountMessage msg) =>
                {
                    if (msg.ViewModel != null)
                        CurrentAccount = msg.ViewModel;
                });
            }
        }
    }
}
