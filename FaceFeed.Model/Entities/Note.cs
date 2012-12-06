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
    /// Represents a facebook note
    /// </summary>
    public class Note : FBEntity
    {
        public string Subject { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal override void LoadData(JsonObject data)
        {
            base.LoadData(data);

            Subject = Convert.ToString(data["subject"]);
            Message = GetHtmlContent(Convert.ToString(data["message"]));
        }

        static string GetHtmlContent(string html)
        {
            var res = html;

            res = Regex.Replace(res, @"<[^\/>]+>", ""); // starting tag e.g. <p>
            res = Regex.Replace(res, @"<\/[^\/>]+>", Environment.NewLine); // ending tag e.g. </p>
            res = Regex.Replace(res, @"<[^\/>]+\/>", Environment.NewLine); // empty tag e.g. <div />

            res = res.Split(Environment.NewLine).Select(a => a.Trim()).Where(a => a.HasValue()).ToString(Environment.NewLine);

            return res;
        }
    }
    
}
