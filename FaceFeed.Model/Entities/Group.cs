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
    /// Represents a facebook group
    /// </summary>
    public class Group : User
    {
        /// <summary>
        /// Gets members of this group, when ready the given callbackc is fired
        /// </summary>
        public void GetMembers(Action<VirtualizedCollection<User>> callback) { GetCollection<User>("members", callback, a => a.Name); }
    }
    
}
