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
    /// Represents a facebook photo
    /// </summary>
    public class Photo : FromUserItem
    {
        public string Name { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }
        public List<User> TaggedUsers { get; set; }

        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal override void LoadData(JsonObject data)
        {
            base.LoadData(data);

            // from?
            var fromdata = data["from"] as JsonObject;
            From = Convert.ToString(fromdata["name"]);
            From_Id = Convert.ToString(fromdata["id"]);

            Name = Convert.ToString(data.TryGet("name") ?? "untitled");
            ImageHeight = Convert.ToInt32(data["height"]);
            ImageWidth = Convert.ToInt32(data["width"]);

            // tags
            var tagsdata = data.TryGet("tags") as JsonObject;
            if (tagsdata != null)
            {
                TaggedUsers = (tagsdata["data"] as List<object>).Select(o => FBEntity.Parse<User>(o as JsonObject)).ToList();
            }
            else
            {
                TaggedUsers = new List<User>();
            }
        }

        /// <summary>
        /// Gets the image of this photo, when ready the given callback is fired
        /// </summary>
        public void GetImage(Action<byte[]> callback)
        {
            FBDataAccess.Current.GetImage(Id + "/picture", callback);
        }
    }
    
}
