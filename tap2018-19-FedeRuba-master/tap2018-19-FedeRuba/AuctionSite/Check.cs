using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public static class Check
    {
        public static void IsNull_Check(string s)
        {
            if (s is null)
                throw new ArgumentNullException();
        }
        public static void ConnectionString_Check(string connectionString)
        {
            IsNull_Check(connectionString);
            if (!Database.Exists(connectionString))
                throw new UnavailableDbException();
        }

        public static void Name_Check(string name)
        {
            IsNull_Check(name);
            if (name.Length < DomainConstraints.MinSiteName || name.Length > DomainConstraints.MaxSiteName)
                throw new ArgumentException();
        }

        public static void RangeTimezone_Check(int timezone)
        {
            if (timezone < DomainConstraints.MinTimeZone || timezone > DomainConstraints.MaxTimeZone)
                throw new ArgumentOutOfRangeException();
        }

        public static void SessionExpirationTimeInSeconds_Check(int sessionExpirationTimeInSeconds)
        {
            if (sessionExpirationTimeInSeconds < 0)
                throw new ArgumentOutOfRangeException();
        }

        public static void MinimumBidIncrement_Check(double minimumBidIncrement)
        {
            if (minimumBidIncrement < 0)
                throw new ArgumentOutOfRangeException();
        }

        public static void AlarmClock_Check(IAlarmClock alarmClock)
        {
            if (alarmClock is null)
                throw new ArgumentNullException();
        }
        public static void Username_Check(string username)
        {
            IsNull_Check(username);
            if (username.Length < DomainConstraints.MinUserName || username.Length > DomainConstraints.MaxUserName)
                throw new ArgumentException();
        }
        public static void Password_Check(string password)
        {
            IsNull_Check(password);

            if (password.Length < DomainConstraints.MinUserPassword)
                throw new ArgumentException();
        }
    }
}
