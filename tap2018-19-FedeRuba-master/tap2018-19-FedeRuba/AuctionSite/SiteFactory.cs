using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Ninject;
using Ninject.Syntax;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;
using ISite = TAP2018_19.AuctionSite.Interfaces.ISite;

namespace AuctionSite
{
    public class SiteFactory : ISiteFactory
    {
        public void Setup(string connectionString)
        {
            if (connectionString is null)
                throw new ArgumentNullException();

            using (var context = new SiteContext(connectionString))
            {
                DropCreateDatabaseAlways<SiteContext> db = new DropCreateDatabaseAlways<SiteContext>();
                Database.SetInitializer<SiteContext>(null);
                try
                {
                    db.InitializeDatabase(context);
                }
                catch (SqlException)
                {
                    throw new UnavailableDbException();
                }
                
            }
        }

        public IEnumerable<string> GetSiteNames(string connectionString)
        {
            Utility.ConnectionString_Check(connectionString);

            using (var context = new SiteContext(connectionString))
            {
                if (!Database.Exists(connectionString))
                    throw new UnavailableDbException();
                return context.Sites.Select(a => a.SiteName).ToList();
            }
        }

        public void CreateSiteOnDb(string connectionString, string name, int timezone, int sessionExpirationTimeInSeconds,
            double minimumBidIncrement)
        {
            Utility.ConnectionString_Check(connectionString);
            Utility.Name_Check(name);
            Utility.RangeTimezone_Check(timezone);
            Utility.SessionExpirationTimeInSeconds_Check(sessionExpirationTimeInSeconds);
            Utility.MinimumBidIncrement_Check(minimumBidIncrement);

            var tupla = new SiteDb()
            {
                SiteName = name,
                Timezone = timezone,
                SessionExpirationTimeInSeconds = sessionExpirationTimeInSeconds,
                MinimumBidIncrement = minimumBidIncrement
            };

            using (var context = new SiteContext(connectionString))
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == name);
                if (!(site is null))
                    throw new NameAlreadyInUseException(name);
                context.Sites.Add(tupla);
                context.SaveChanges();
            }
        }

        public ISite LoadSite(string connectionString, string name, IAlarmClock alarmClock)
        {
            Utility.ConnectionString_Check(connectionString);
            Utility.Name_Check(name);
            Utility.AlarmClock_Check(alarmClock);
            if (alarmClock.Timezone != GetTheTimezoneOf(connectionString, name)) 
                throw  new ArgumentException();

            using (var context = new SiteContext(connectionString))
            {
                alarmClock.InstantiateAlarm(5 * 60 * 1000);
                var site = context.Sites.SingleOrDefault(a => a.SiteName == name);
                if (site is null)
                    throw new InexistentNameException(name);

                return new Site(connectionString, site.SiteName, site.Timezone, site.SessionExpirationTimeInSeconds, site.MinimumBidIncrement, alarmClock);
            }
        }

        public int GetTheTimezoneOf(string connectionString, string name)
        {
            Utility.ConnectionString_Check(connectionString);
            Utility.Name_Check(name);
            using (var context = new SiteContext(connectionString))
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == name);
                if (site is null)
                    throw new InexistentNameException(name);

                return site.Timezone;
            }
        }
    }
}
