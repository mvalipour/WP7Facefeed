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
    /// Represents a facebook news feed item
    /// </summary>
    public class FeedItem : FromUserItem
    {
        public string Message { get; set; }
        public string Story { get { return Message.Or(LinkName).Or(Description); } }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public string Description { get; set; }
        public string LinkName { get; set; }
        public string Link { get; set; }

        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal override void LoadData(JsonObject data)
        {
            base.LoadData(data);

            // details
            Message = Convert.ToString(data.TryGet("story") ?? data.TryGet("message"));
            Link = Convert.ToString(data.TryGet("link") ?? "");
            LinkName = Convert.ToString(data.TryGet("name") ?? "Link");
            Description = new[] { data.TryGet("caption"), data.TryGet("description") }.Where(a => a != null).Select(a => Convert.ToString(a)).ToString(Environment.NewLine);

            // from?
            var fromdata = data["from"] as JsonObject;
            From = Convert.ToString(fromdata["name"]);
            From_Id = Convert.ToString(fromdata["id"]);

            // like #
            var likesdata = data.TryGet("likes") as JsonObject;
            if (likesdata != null)
            {
                LikeCount = Convert.ToInt32(likesdata.TryGet("count") ?? 0);
            }

            // comment #
            var commentsdata = data.TryGet("comments") as JsonObject;
            if (commentsdata != null)
            {
                CommentCount = Convert.ToInt32(commentsdata.TryGet("count") ?? 0);
            }
        }
    }
    
}
