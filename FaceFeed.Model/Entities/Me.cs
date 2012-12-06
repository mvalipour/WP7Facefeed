using System;
using System.Linq;
using System.Collections.Generic;
using System.WindowsPhone;
using System.IO;
using Facebook;
using System.ComponentModel;
using System.Collections;
using System.Text.RegularExpressions;

namespace FaceFeed.Model
{
    /// <summary>
    /// Represents facebook profile of logged in person
    /// </summary>
    public class Me : User
    {
        public override string Id
        {
            get
            {
                return "me";
            }
            set
            {
                // nothing!
            }
        }

        static Me _Current;

        /// <summary>
        /// Loads the profile of the current user to a ME object, when ready the given callback is fired
        /// </summary>
        public static void GetCurrent(Action<Me> callback)
        {
            if (_Current == null)
            {
                FBDataAccess.Current.GetGraph("me", data =>
                {
                    _Current = FBEntity.Parse<Me>(data);

                    callback.Invoke(_Current);
                });
            }
            else
            {
                callback.Invoke(_Current);
            }
        }

        /// <summary>
        /// Gets the home feed of "me"
        /// </summary>
        public void GetHomeFeed(Action<VirtualizedCollection<FeedItem>> callback, DateTime? until = null)
        {
            var pars = until.HasValue ? new Dictionary<string, object> { { "until", until.Value.ToUnixTimestamp() - 1 }, { "limit", FBDataAccess.LIST_LIMIT_SIZE } } : null;
            GetCollection<FeedItem>("home", callback, parameters: pars);
        }

        /// <summary>
        /// Gets list of notifications of "me", when ready the given callback is fired
        /// </summary>
        public void GetNotifications(Action<VirtualizedCollection<Notification>> callback) { GetCollection<Notification>("notifications?include_read=1", callback); }

        /// <summary>
        /// Gets mutual friends of "me" with the given user, when ready the given callback is fired
        /// </summary>
        /// <param name="with"></param>
        /// <param name="callback"></param>
        public void GetMutualFriends(User with, Action<VirtualizedCollection<User>> callback) { GetCollection<User>("mutualfriends/" + with.Id, callback, a => a.Name); }

        /// <summary>
        /// Shares the given news feed item on my wall, when done the given callback is fired.
        /// </summary>
        public void ShareFeed(FeedItem item, Action callback)
        {
            FBDataAccess.Current.PostGraph(Id + "/links", new Dictionary<string, object>
            {
                {"link", item.Link.Or("www.facebook.com/{0}".FormatWith(item.Id.Replace("_", "/posts/")))},
                /*TODO: input message!*/
            }, callback);
        }
    }
    
}
