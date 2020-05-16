using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ISiteFactory>().To<SiteFactory>();
            Bind<IUser>().To<User>();
            Bind<ISite>().To<Site>();
            Bind<ISession>().To<Session>();
            Bind<IAuction>().To<Auction>();
        }
    }
}
