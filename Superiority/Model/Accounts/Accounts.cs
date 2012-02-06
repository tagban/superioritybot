using System;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Superiority.Model.Bnet;
using System.Collections.Generic;

namespace Superiority.Model.Accounts
{
    [KnownType(typeof(BnetAccount))]
    public class Accounts
    {
        public ObservableCollection<IAccount> BotAccounts { get; set; }
        public Dictionary<BnetProduct, byte> VersionBytes { get; set; }

        public Accounts()
        {
            BotAccounts = new ObservableCollection<IAccount>();
            VersionBytes = new Dictionary<BnetProduct, byte>();
        }

        public static void Serialize(Accounts serObj)
        {
            using (IsolatedStorageFileStream fs = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("accounts.xml", System.IO.FileMode.Create))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(Accounts));
                ser.WriteObject(fs, serObj);
            }
        }

        public static Accounts Deserialize()
        {
            if (!IsolatedStorageFile.GetUserStoreForApplication().FileExists("accounts.xml"))
                return new Accounts();

            try
            {
                using (IsolatedStorageFileStream fs = IsolatedStorageFile.GetUserStoreForApplication().OpenFile("accounts.xml", System.IO.FileMode.OpenOrCreate))
                {
                    // TODO: Catch Serialization exceptions and return a new account object, notify MVVM that the accounts file was corrupted.
                    DataContractSerializer ser = new DataContractSerializer(typeof(Accounts));
                    return (Accounts)ser.ReadObject(fs);
                }
            }
            catch (SerializationException)
            {
                // TODO: Show UX notifying the user his config file was destroyed.
                IsolatedStorageFile.GetUserStoreForApplication().DeleteFile("accounts.xml");
                return new Accounts();
            }
        }
    }
}
