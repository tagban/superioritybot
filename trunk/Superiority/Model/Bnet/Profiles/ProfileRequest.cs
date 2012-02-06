using System;

namespace Superiority.Model.Bnet
{
    public abstract class ProfileRequest
    {
        public string Location
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public BnetUser User
        {
            get;
            set;
        }
    }
}
