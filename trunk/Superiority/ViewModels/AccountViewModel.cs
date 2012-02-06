using System;

using Superiority.Model.Bnet;
using Superiority.ViewModels.Bnet;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;

namespace Superiority.ViewModels
{
    /// <summary>
    /// The ViewModel for the specified Account. There is one of these spawned for every Account on the Bot. 
    /// </summary>
    public sealed class AccountViewModel : ViewModelBase
    {
        #region Battle.net related fields
        private BnetAccount bnetAccount;
        private BnetViewModel bnetViewModel;
        #endregion

        public BnetAccount Account
        {
            get
            {
                return bnetAccount;
            }
            set
            {
                bnetAccount = value;
                VerifyPropertyName("Account");
                RaisePropertyChanged("Account");
            }
        }

        public BnetViewModel Bnet
        {
            get
            {
                return bnetViewModel;
            }
            set
            {
                bnetViewModel = value;
                VerifyPropertyName("Bnet");
                RaisePropertyChanged("Bnet");
            }
        }

        private RelayCommand<AccountViewModel> changeCurrentAccountCommand;
        public RelayCommand<AccountViewModel> ChangeCurrentAccountCommand
        {
            get
            {
                if (changeCurrentAccountCommand == null)
                    changeCurrentAccountCommand = new RelayCommand<AccountViewModel>((AccountViewModel account) =>
                    {
                        if (account == null)
                            return;

                        Messenger.Default.Send<BnetSwitchAccountMessage>(new BnetSwitchAccountMessage() { ViewModel = account });
                    });

                return changeCurrentAccountCommand;
            }
        }

        /// AccountViewModel Constructor
        /// <summary>
        /// </summary>
        /// <param name="account">The Account passed by the MainWindowViewModel</param>
        public AccountViewModel(BnetAccount account)
        {
            bnetAccount = account;

            // Initialize our BnetViewModel with the BnetAccount
            // Note: Strong VM coupling is discouraged, but it makes sense here
            bnetViewModel = new BnetViewModel(bnetAccount);
        }
    }
}
