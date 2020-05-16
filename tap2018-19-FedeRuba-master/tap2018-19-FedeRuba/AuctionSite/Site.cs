using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Ninject.Infrastructure.Language;
using TAP2018_19.AlarmClock.Interfaces;
using TAP2018_19.AuctionSite.Interfaces;

namespace AuctionSite
{
    public class Site : ISite
    {
        private readonly string _connectionString;
        private readonly IAlarmClock _alarmClock;
        public string Name { get; }
        public int Timezone { get; }
        public int SessionExpirationInSeconds { get; }
        public double MinimumBidIncrement { get; }
        

        public Site(string connectionString, string name, int timezone, int sessionExpirationTimeInSeconds,
            double minimumBidIncrement, IAlarmClock alarmClock)
        {
            _connectionString = connectionString;
            Name = name;
            Timezone = timezone;
            SessionExpirationInSeconds = sessionExpirationTimeInSeconds;
            MinimumBidIncrement = minimumBidIncrement;
            _alarmClock = alarmClock;
            _alarmClock.InstantiateAlarm(5 * 60 * 1000).RingingEvent += CleanupSessions;
        }

        protected bool Equals(Site other)
        {
            return string.Equals(_connectionString, other._connectionString) && string.Equals(Name, other.Name) && Timezone == other.Timezone && SessionExpirationInSeconds == other.SessionExpirationInSeconds && MinimumBidIncrement.Equals(other.MinimumBidIncrement) && Equals(_alarmClock, other._alarmClock);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Site) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_connectionString != null ? _connectionString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timezone;
                hashCode = (hashCode * 397) ^ SessionExpirationInSeconds;
                hashCode = (hashCode * 397) ^ MinimumBidIncrement.GetHashCode();
                hashCode = (hashCode * 397) ^ (_alarmClock != null ? _alarmClock.GetHashCode() : 0);
                return hashCode;
            }
        }

        public IEnumerable<IUser> GetUsers()
        {
            List<IUser> listUsers = new List<IUser>();
            using (var context = new SiteContext(_connectionString))
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == Name); ;
                if ( site is null)
                    throw new InvalidOperationException();
                foreach (var curr in site.Users)
                {
                    listUsers.Add(new User(curr.Username, this, _connectionString, _alarmClock));
                }
            }
            return listUsers;
        }

        public IEnumerable<ISession> GetSessions()
        {
            List<Session> listSessions = new List<Session>();
            using (var context = new SiteContext(_connectionString))
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == Name);
                if (site is null)
                    throw new InvalidOperationException();
                foreach (var curr in site.Sessions)
                {
                    listSessions.Add(new Session(_connectionString, _alarmClock,curr.Id, curr.ValidUntil, new User(curr.User.Username, this, _connectionString, _alarmClock), this));
                }

                return listSessions;
            }
        }

        public IEnumerable<IAuction> GetAuctions(bool onlyNotEnded)
        {
            List<Auction> listAuction = new List<Auction>();
            using (var context = new SiteContext(_connectionString))
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == Name);
                if (site is null)
                    throw new InvalidOperationException();

                if (onlyNotEnded)
                {
                    foreach (var curr in site.Auctions)
                    {
                        if (curr.EndsOn > _alarmClock.Now)
                            listAuction.Add(new Auction(curr.Id,
                                new User(curr.Seller.Username, this, _connectionString, _alarmClock),
                                curr.Description, curr.EndsOn, curr.CurrentPrice, this, _connectionString, _alarmClock));
                    }
                }
                else
                {
                    foreach (var curr in site.Auctions)
                    {
                        listAuction.Add(new Auction(curr.Id,
                            new User(curr.Seller.Username, this, _connectionString, _alarmClock), curr.Description,
                            curr.EndsOn, curr.CurrentPrice,this, _connectionString, _alarmClock));
                    }
                }
            }

            return listAuction;
        }

        public ISession Login(string username, string password)
        {
            Utility.Username_Check(username);
            Utility.Password_Check(password);
            using (var context = new SiteContext(_connectionString))
            using (SHA256 sha256 = SHA256.Create())
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == Name);
                if (site is null)
                    throw new InvalidOperationException();
                var user = site.Users.SingleOrDefault(a => a.Username == username);
                if (user is null || user.Password != Utility.Hash(sha256,password))
                    return null;
                var session = site.Sessions.SingleOrDefault(a => a.User.Username == username);
                if (session is null)
                {
                    var id = Guid.NewGuid().ToString();
                    var tupla = new SessionDb()
                    {
                        Id = id,
                        ValidUntil = _alarmClock.Now.AddSeconds(site.SessionExpirationTimeInSeconds),
                        User = user,
                        SiteName = Name
                    };
                    context.Sessions.Add(tupla);
                    context.SaveChanges();
                    return new Session(_connectionString, _alarmClock, tupla.Id, tupla.ValidUntil, new User(username, this, _connectionString, _alarmClock), this);
                }

                session.ValidUntil = _alarmClock.Now.AddSeconds(site.SessionExpirationTimeInSeconds);
                context.SaveChanges();
                return GetSession(session.Id);
            }
        }

        public ISession GetSession(string sessionId)
        {
            Utility.IsNull_Check(sessionId);
            if (sessionId == "")
                return null;

            using (var context = new SiteContext(_connectionString))
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == Name);
                if (site is null)
                    throw new InvalidOperationException();
                var session = site.Sessions.SingleOrDefault(a => a.Id == sessionId);
                if (session is null)
                    return null;
               var iSession = new Session(_connectionString, _alarmClock, session.Id, session.ValidUntil, new User(session.User.Username, this, _connectionString, _alarmClock), this);
               if (iSession.IsValid())
                   return iSession;
               return null;
            }  
        }
        
        public void CreateUser(string username, string password)
        {
            Utility.IsNull_Check(username);
            Utility.IsNull_Check(password);
            if (username.Length < DomainConstraints.MinUserName || username.Length > DomainConstraints.MaxUserName || password.Length < DomainConstraints.MinUserPassword)
                throw new ArgumentException();


            using (var context = new SiteContext(_connectionString))
            using (SHA256 sha256 = SHA256.Create())
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == Name);
                if (site is null)
                    throw new InvalidOperationException();

                var tupla = new UserDb()
                {
                    Username = username,
                    Password = Utility.Hash(sha256, password),
                    SiteName = Name
                };

                if (!(site.Users.SingleOrDefault(a => a.Username == username) is null))
                    throw new NameAlreadyInUseException(username);

                context.Users.Add(tupla);
                context.SaveChanges();
            }
        }

        public void Delete()
        {
            using (var context = new SiteContext(_connectionString))
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == Name);
                if (site is null)
                    throw new InvalidOperationException();

                foreach (var curr in context.Users.Where(a => a.SiteName == Name))
                {
                    context.Users.Remove(curr);
                }
                foreach (var curr in context.Sessions.Where(a => a.SiteName == Name))
                {
                    context.Sessions.Remove(curr);
                }
                foreach (var curr in context.Auctions.Where(a => a.SiteName == Name))
                {
                    context.Auctions.Remove(curr);
                }
                context.Sites.Remove(site);
                context.SaveChanges();
            }
        }

        public void CleanupSessions()
        {
            using (var context = new SiteContext(_connectionString))
            {
                var site = context.Sites.SingleOrDefault(a => a.SiteName == Name);
                if (site is null)
                    throw new InvalidOperationException();
                var l = context.Sessions.Where(n => n.SiteName == Name).ToList();
                foreach (var curr in l)
                {
                    if (_alarmClock.Now > curr.ValidUntil)
                    {
                        context.Sessions.Remove(curr);
                        context.SaveChanges();
                    }
                }
            }
        }


    }
}
