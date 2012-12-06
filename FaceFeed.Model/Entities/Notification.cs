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
    /// Represents a facebook notification
    /// </summary>
    public class Notification : FromUserItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public bool Unread { get; set; }

        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal override void LoadData(JsonObject data)
        {
            base.LoadData(data);

            Title = Convert.ToString(data["title"]);

            var fromdata = data["from"] as JsonObject;
            From = Convert.ToString(fromdata["name"]);
            From_Id = Convert.ToString(fromdata["id"]);

            Link = Convert.ToString(data.TryGet("link") ?? "");
            Unread = Convert.ToBoolean(data.TryGet("unread") ?? "false");
        }

        /// <summary>
        /// Marks this notification as read
        /// </summary>
        public void MarkAsRead()
        {
            if (Unread)
            {
                Unread = false;

                FBDataAccess.Current.PostGraph(Id, new Dictionary<string, object> { { "unread", "0" } }, () => { });
            }
        }
    }
    
}
