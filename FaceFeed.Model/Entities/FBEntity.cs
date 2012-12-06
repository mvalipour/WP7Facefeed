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
    /// Represents abstraction of a facebook entity
    /// </summary>
    public abstract class FBEntity : INotifyPropertyChanged
    {
        public virtual string Id { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        /// Loads data fields of this instance from the given json object
        /// </summary>
        internal virtual void LoadData(JsonObject data)
        {
            Id = Convert.ToString(data["id"]);

            var datefield = data.TryGet("created_time") ?? data.TryGet("updated_time");
            if (datefield != null)
            {
                Date = Convert.ToDateTime(datefield);
            }
        }

        /// <summary>
        /// Parses the given json data to facebook entity T.
        /// </summary>
        protected static T Parse<T>(JsonObject data) where T : FBEntity, new()
        {
            if (data == null) return null;

            try
            {
                var res = new T();
                res.LoadData(data);

                if (res.Id.HasValue() == false || res.Id.StartsWith("0_")) return null;

                return res;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Fetches a collection of facebook entity T from facebook graph
        /// with the given list of parameters
        /// sorting them by the given criteria
        /// </summary>
        protected void GetCollection<T>(string cat, Action<VirtualizedCollection<T>> callback, Func<T, string> sortCriteria = null, IDictionary<string, object> parameters = null) where T : FBEntity, new()
        {
            FBDataAccess.Current.GetGraph(Id + "/" + cat, data =>
            {
                var res = (data["data"] as List<object>).Select(o => FBEntity.Parse<T>(o as JsonObject)).ExceptNulls();

                if (sortCriteria != null)
                {
                    res = res.OrderBy(sortCriteria);
                }

                callback.Invoke(new VirtualizedCollection<T>(res));
            }, parameters);
        }

        /// <summary>
        /// Event handler that get's fired when a property is changed on this instance
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners of the change in the given property of this instance
        /// </summary>
        public virtual void NotifyPropertyChanged(string info)
        {
            var h = PropertyChanged;
            if (h != null)
            {
                System.Threading.ThreadHelper.RunOnUI(() =>
                {
                    h(this, new PropertyChangedEventArgs(info));
                });
            }
        }

        /// <summary>
        /// Gets comments on this facebook entity, when ready the given callback is fired
        /// </summary>
        public void GetComments(Action<VirtualizedCollection<Comment>> callback, string lastid = null, int offset = 0)
        {
            var pars = lastid.HasValue() ? new Dictionary<string, object> { { "__after_id", lastid }, { "limit", FBDataAccess.LIST_LIMIT_SIZE }, { "offset", offset } } : null;
            GetCollection<Comment>("comments", callback, parameters: pars);
        }

        /// <summary>
        /// Gets likes on this facebook entity, when ready the given callback is fired
        /// </summary>
        public void GetLikes(Action<VirtualizedCollection<Like>> callback) { GetCollection<Like>("likes", callback); }

        /// <summary>
        /// Posts the given message as a comment on this facebook entity, when done the given callback is fired
        /// </summary>
        public void PostComment(string message, Action callback)
        {
            FBDataAccess.Current.PostGraph(Id + "/comments", new Dictionary<string, object>
            {
                {"message", message}
            }, callback);
        }

        /// <summary>
        /// Likes this facebook entity, when done the given callback is fired
        /// </summary>
        public void PostLike(Action callback)
        {
            FBDataAccess.Current.PostGraph(Id + "/likes", new Dictionary<string, object>
            {
            }, callback);
        }

        /// <summary>
        /// Finds a fecebook entity of type T with the given ID, when done the given callback is fired
        /// </summary>
        public static void FindByID<T>(string id, Action<T> callback) where T : FBEntity, new()
        {
            FBDataAccess.Current.GetGraph(id, data =>
            {
                callback.Invoke(Parse<T>(data));
            });
        }

        public virtual bool Matches(string f) { return false; }
    }

}
