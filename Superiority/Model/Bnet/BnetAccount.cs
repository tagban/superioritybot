using System;

using Superiority.Model.Accounts;
using Superiority.Model.Filters;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Collections.Generic;

using GalaSoft.MvvmLight;
using System.ComponentModel;

namespace Superiority.Model.Bnet
{
    public class BnetAccount : IDataErrorInfo, IAccount, INotifyPropertyChanged
    {
        public string AccountName { get; set; }

        public AccountTypes AccountType
        {
            get
            {
                return AccountTypes.Bnet;
            }
        }

        private string username;
        public string Username 
        {
            get
            {
                return username;
            }
            set
            {
                username = value;

                OnPropertyChanged("Username");
            }
        }

        public string Password { get; set; }

        public string CdKey
        {
            get;
            set;
        }

        public string HomeChannel { get; set; }

        public BnetServer Server { get; set; }

        private BnetProduct product;
        public BnetProduct Product 
        {
            get
            {
                return product;
            }
            set
            {
                product = value;
                OnPropertyChanged("Product");
            }
        }

        private List<string> productList;
        [IgnoreDataMember]
        public List<string> ProductList
        {
            get
            {
                if (productList == null)
                {
                    productList = new List<string>();
                    productList.Add("Starcraft");
                    productList.Add("Starcraft: Brood War");
                    productList.Add("Warcraft II: BNE");
                    productList.Add("Diablo II");
                    productList.Add("Warcraft III: Reign of Chaos");
                }

                return productList;
            }
        }

        [IgnoreDataMember]
        public string ProductFriendly
        {
            get
            {
                if (Product == BnetProduct.Starcraft)
                    return "Starcraft";

                if (Product == BnetProduct.BroodWar)
                    return "Starcraft: Brood War";

                if (Product == BnetProduct.Warcraft2)
                    return "Warcraft II: BNE";

                if (Product == BnetProduct.Diablo2)
                    return "Diablo II";

                if (Product == BnetProduct.Warcraft3)
                    return "Warcraft III: Reign of Chaos";

                return "Starcraft";
            }
            set
            {
                if (value == "Starcraft")
                    Product = BnetProduct.Starcraft;

                if (value == "Starcraft: Brood War")
                    Product = BnetProduct.BroodWar;

                if (value == "Warcraft II: BNE")
                    Product = BnetProduct.Warcraft2;

                if (value == "Diablo II")
                    Product = BnetProduct.Diablo2;

                if (value == "Warcraft III: Reign of Chaos")
                    Product = BnetProduct.Warcraft3;
            }
        }

        private List<string> serverList;
        [IgnoreDataMember]
        public List<string> ServerList
        {
            get
            {
                if (serverList == null)
                {
                    serverList = new List<string>();
                    serverList.Add("USEast");
                    serverList.Add("USWest");
                    serverList.Add("Asia");
                    serverList.Add("Europe");
                }

                return serverList;
            }
        }

        [IgnoreDataMember]
        public string ServerFriendly
        {
            get
            {
                if (Server == BnetServer.UsEast)
                    return "USEast";

                if (Server == BnetServer.UsWest)
                    return "USWest";

                if (Server == BnetServer.Asia)
                    return "Asia";

                if (Server == BnetServer.Europe)
                    return "Europe";

                return "USEast";
            }
            set
            {
                if (value == "USEast")
                    Server = BnetServer.UsEast;

                if (value == "USWest")
                    Server = BnetServer.UsWest;

                if (value == "Asia")
                    Server = BnetServer.Asia;

                if (value == "Europe")
                    Server = BnetServer.Europe;
            }
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
                    case "Username":
                        if (string.IsNullOrEmpty(Username))
                            error = "You must provide a username.";
                        else if (Username.Length > 15)
                            error = "Your username cannot be longer than 15 characters.";

                        break;
                    case "Password":
                        if (string.IsNullOrEmpty(Password))
                            error = "You must provide a password.";

                        break;
                    case "CdKey":
                        if (string.IsNullOrEmpty(CdKey))
                            error = "You must provide a cdkey.";
                        else if (CdKey.Contains("-"))
                            error = "Your cdkey cannot contain any dashes.";

                        break;
                    case "HomeChannel":
                        if (string.IsNullOrEmpty(HomeChannel))
                            error = "You must provide a home channel.";
                        else if (HomeChannel.Length > 15)
                            error = "Your home channel cannot be longer than 15 characters.";
                        
                        break;
                }

                return error;
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public ObservableCollection<ChatFilter> AccountFilters
        {
            get;
            set;
        }
    }
}
