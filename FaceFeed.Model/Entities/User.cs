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
    /// Represents a facebook user
    /// </summary>
    public class User : FBEntity
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        public string Name { get; set; }

        byte[] _ImageData;
        /// <summary>
        /// User image byte array
        /// </summary>
        public byte[] ImageData
        {
            get
            {
                if (_ImageData == null)
                {
                    LoadImage();
                }
                return _ImageData;
            }
        }

        /// <summary>
        /// Gets the user with the given id. When ready the given callback is fired
        /// </summary>
        public static void GetUser(string id, Action<User> callback)
        {
            FBDataAccess.Current.GetGraph(id, data =>
            {
                var user = FBEntity.Parse<User>(data);
                callback.Invoke(user);
            });
        }

        /// <summary>
        /// Gets the friends of this user, When ready the given callback is fired
        /// </summary>
        public void GetFriends(Action<VirtualizedCollection<User>> callback) { GetCollection<User>("friends", callback, a => a.Name); }

        /// <summary>
        /// Get the wall feed of this user until the given (optional) date time, When ready the given callback is fired
        /// </summary>
        public void GetWallFeed(Action<VirtualizedCollection<FeedItem>> callback, DateTime? until = null)
        {
            var pars = until.HasValue ? new Dictionary<string, object> { { "until", until.Value.ToUnixTimestamp() - 1 }, { "limit", FBDataAccess.LIST_LIMIT_SIZE } } : null;
            GetCollection<FeedItem>("feed", callback, parameters: pars);
        }

        /// <summary>
        /// Gets statuses of this user, When ready the given callback is fired
        /// </summary>
        public void GetStatuses(Action<VirtualizedCollection<FeedItem>> callback) { GetCollection<FeedItem>("statuses", callback); }

        /// <summary>
        /// Gets photos of this user, When ready the given callback is fired
        /// </summary>
        public void GetPhotos(Action<VirtualizedCollection<Photo>> callback) { GetCollection<Photo>("photos", callback); }

        /// <summary>
        /// Gets photoalbums of this user, When ready the given callback is fired
        /// </summary>
        public void GetPhotoAlbums(Action<VirtualizedCollection<PhotoAlbum>> callback) { GetCollection<PhotoAlbum>("albums", callback); }

        /// <summary>
        /// Gets notes of this user, When ready the given callback is fired
        /// </summary>
        public void GetNotes(Action<VirtualizedCollection<Note>> callback) { GetCollection<Note>("notes", callback); }

        /// <summary>
        /// Gets pages this user likes, When ready the given callback is fired
        /// </summary>
        public void GetPages(Action<VirtualizedCollection<Page>> callback) { GetCollection<Page>("likes", callback, a => a.Name); }

        /// <summary>
        /// Gets groups this user belongs to, When ready the given callback is fired
        /// </summary>
        public void GetGroups(Action<VirtualizedCollection<Group>> callback) { GetCollection<Group>("groups", callback, a => a.Name); }

        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal override void LoadData(JsonObject data)
        {
            base.LoadData(data);

            Name = Convert.ToString(data["name"]);
        }

        /// <summary>
        /// Loads the picture of this user asynchronously. When ready imageData property is updated
        /// </summary>
        void LoadImage()
        {
            FBDataAccess.Current.GetImage(Id + "/picture", data =>
            {
                _ImageData = data;
                NotifyPropertyChanged("ImageData");
            });
        }

        /// <summary>
        /// Gets large version of profile picture of this user. When ready the given callback is fired.
        /// </summary>
        public void GetLargeImage(Action<byte[]> callback)
        {
            FBDataAccess.Current.GetImage(Id + "/picture", callback, new Dictionary<string, object> { { "type", "large" } });
        }

        /// <summary>
        /// Post the given message on the wall of this user, When done the given callback is called
        /// </summary>
        public void PostOnWall(string message, Action callback)
        {
            FBDataAccess.Current.PostGraph(Id + "/feed", new Dictionary<string, object>
            {
                {"message", message}
            }, callback);
        }

        /// <summary>
        /// Checks to see whether or not this instance matches the given search keyword
        /// </summary>
        public override bool Matches(string f) { return Name.ToLower().Contains(f); }
    }

}
