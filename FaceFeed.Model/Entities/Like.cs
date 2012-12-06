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
    /// Represents a facebook like
    /// </summary>
    public class Like : FromUserItem
    {
        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal override void LoadData(JsonObject data)
        {
            //base.LoadData(data);

            // some randome id (will never be used)
            // need it otherwise the item will be treated as invalid
            Id = Guid.NewGuid().ToString();

            From = Convert.ToString(data["name"]);
            From_Id = Convert.ToString(data["id"]);
        }
    }
    
}
