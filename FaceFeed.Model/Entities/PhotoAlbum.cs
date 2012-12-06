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
    /// Represents a facebook photo album
    /// </summary>
    public class PhotoAlbum : FBEntity
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }

        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal override void LoadData(JsonObject data)
        {
            base.LoadData(data);

            Name = Convert.ToString(data.TryGet("name") ?? "untitled");
            Location = Convert.ToString(data.TryGet("location") ?? "");
            Description = Convert.ToString(data.TryGet("description") ?? "");
            Count = Convert.ToInt32(data.TryGet("count") ?? 0);
        }

        /// <summary>
        /// Gets photos of this album, Whenn ready the given callback is fird.
        /// </summary>
        public void GetPhotos(Action<VirtualizedCollection<Photo>> callback) { GetCollection<Photo>("photos", callback); }
    }
    
}
