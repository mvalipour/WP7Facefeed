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
    /// Represents a facebook comment
    /// </summary>
    public class Comment : FromUserItem
    {
        public string Story { get; set; }
        public int LikeCount { get; set; }

        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal override void LoadData(JsonObject data)
        {
            base.LoadData(data);

            Story = Convert.ToString(data["message"]);

            var fromdata = data["from"] as JsonObject;
            From = Convert.ToString(fromdata["name"]);
            From_Id = Convert.ToString(fromdata["id"]);

            LikeCount = Convert.ToInt32(data.TryGet("likes") ?? 0);
        }
    }
}
