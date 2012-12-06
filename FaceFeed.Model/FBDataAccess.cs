using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Facebook;
using System.Collections.Generic;
using System.Text;
using System.IO.IsolatedStorage;
using System.WindowsPhone;
using System.IO;
using System.Threading;

namespace FaceFeed.Model
{
    public class FBDataAccess
    {
        const string APP_ID = "176110615799586";

        /// <summary>
        /// Gets the url of the authentication web page
        /// </summary>
        public static string GetOAuthUrl()
        {
            string[] extendedPermissions = new[] { "user_about_me", "offline_access",
"user_about_me", 
"user_activities", 
"user_birthday", 
"user_checkins", 
"user_education_history", 
"user_events", 
"user_groups", 
"user_hometown", 
"user_interests", 
"user_likes", 
"user_location", 
"user_notes", 
"user_online_presence", 
"user_photo_video_tags", 
"user_photos", 
"user_questions", 
"user_relationships", 
"user_relationship_details", 
"user_religion_politics", 
"user_status", 
"user_videos", 
"user_website", 
"user_work_history", 
"email", 
"friends_about_me", 
"friends_activities", 
"friends_birthday", 
"friends_checkins", 
"friends_education_history", 
"friends_events", 
"friends_groups", 
"friends_hometown", 
"friends_interests", 
"friends_likes", 
"friends_location", 
"friends_notes", 
"friends_online_presence", 
"friends_photo_video_tags", 
"friends_photos", 
"friends_questions", 
"friends_relationships", 
"friends_relationship_details", 
"friends_religion_politics", 
"friends_status", 
"friends_videos", 
"friends_website", 
"friends_work_history",
"read_friendlists",
"read_insights",
"read_mailbox",
"read_requests",
"read_stream",
"xmpp_login",
"ads_management",
"create_event",
"manage_friendlists",
"manage_notifications",
"offline_access",
"publish_checkins",
"publish_stream",
"rsvp_event",
"publish_actions",
"manage_pages"
}.Distinct().ToArray();

            var oauth = new FacebookOAuthClient { AppId = APP_ID };

            var parameters = new Dictionary<string, object>
                    {
                        { "response_type", "token" },
                        { "display", "popup" }
                    };

            if (extendedPermissions != null && extendedPermissions.Length > 0)
            {
                var scope = new StringBuilder();
                scope.Append(string.Join(",", extendedPermissions));
                parameters["scope"] = scope.ToString();
            }

            return oauth.GetLoginUrl(parameters).OriginalString;
        }

        public const string CACHE_FOLDER = "_cache";

        static FBDataAccess()
        {
            // create cachefolder
            if (IOHelper.Directory.Exists(CACHE_FOLDER) == false)
            {
                IOHelper.Directory.CreateDirectory(CACHE_FOLDER);
            }
        }
        FBDataAccess()
        {
            Token = (string)IsolatedStorageSettings.ApplicationSettings.TryGet("User.Token");
        }

        static FBDataAccess _Current;
        /// <summary>
        /// Current facebook data access object
        /// </summary>
        public static FBDataAccess Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new FBDataAccess();
                }
                return _Current;
            }
        }

        string Token { get; set; }
        void RegisterToken(string token)
        {
            IsolatedStorageSettings.ApplicationSettings["User.Token"] = token;
            IsolatedStorageSettings.ApplicationSettings.Save();

            // ensure reload
            _Current = null;
        }

        /// <summary>
        /// Tries to authorize user from the given URL.
        /// </summary>
        public bool Authorize(string responseUrl)
        {
            FacebookOAuthResult result;
            if (FacebookOAuthResult.TryParse(responseUrl, out result))
            {
                if (result.IsSuccess)
                {
                    RegisterToken(result.AccessToken);
                    return true;
                }
            }
            else
            {
            }

            return false;
        }

        /// <summary>
        /// Creates facebook client object for using internally
        /// </summary>
        /// <returns></returns>
        FacebookClient CreateClient()
        {
            return new FacebookClient(Token)
            {
            };
        }

        /// <summary>
        /// Validates the given response of facebook API for errors
        /// </summary>
        bool ValidateApiResponse(FacebookApiEventArgs args)
        {
            if (args.Cancelled)
            {
                //OnFailed("Action was cancelled.");
                return false;
            }
            if (args.Error != null)
            {
                if (args.Error is FacebookOAuthException)
                {
                    OnFailed(@"Oops! FaceFeed could not fetch data from your account.
Make sure you have not restricted FaceFeed in your account.
Please LOG-OUT (from home page menu) and LOG-IN again.");
                }
                else
                {
                    OnFailed(GENERIC_DATA_ERROR_MESSAGE);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Fired when a data access data fetch is failed
        /// </summary>
        public static event EventHandler<EventArgs<string>> Failed;
        static void OnFailed(string m)
        {
            var h = Failed;
            if (h != null) h(null, new EventArgs<string>(m));
        }

        /// <summary>
        /// List of current pending tasks
        /// </summary>
        Dictionary<string, FacebookClient> PendingTasks = new Dictionary<string, FacebookClient>();
        void AddToPendingTasks(string k, FacebookClient client)
        {
            lock (PendingTasks)
            {
                PendingTasks[k] = client;
            }
        }
        void CancelPendingTask(string k)
        {
            lock (PendingTasks)
            {
                if (PendingTasks.ContainsKey(k))
                {
                    PendingTasks[k].CancelAsync();
                    PendingTasks.Remove(k);
                }
            }
        }

        /// <summary>
        /// Cancels all current pending tasks
        /// </summary>
        public void CancelAllPendingTasks()
        {
            lock (PendingTasks)
            {
                foreach (var item in PendingTasks)
                {
                    item.Value.CancelAsync();
                }

                PendingTasks.Clear();
            }
        }

        /// <summary>
        /// Gets the facebook graph object of the given path with the given parameeters
        /// when ready the given callback is fired
        /// </summary>
        public void GetGraph(string path, Action<JsonObject> callback, IDictionary<string, object> parameters = null)
        {
            if (Token == null)
            {
                callback.Invoke(null);
                return;
            }

            // if any parameter is included (usually when paging)
            // no cache should be read or written
            var ignoreCacheTotally = parameters != null;

            if (!ignoreCacheTotally)
            {
                var cached = GetCache(path);
                if (cached != null)
                {
                    try
                    {
                        callback.Invoke(JsonSerializer.Current.DeserializeObject<JsonObject>(cached));
                        return;
                    }
                    catch { }
                }
            }

            // start task
            CancelPendingTask(path);

            var client = CreateClient();
            client.GetCompleted += (a, b) =>
            {
                // validate response
                if (!ValidateApiResponse(b)) return;

                // retrieve data
                var data = b.GetResultData();

                // check if any json data available
                var jsondata = data as JsonObject;
                if (jsondata == null)
                {
                    OnFailed(GENERIC_DATA_ERROR_MESSAGE);
                    return;
                }

                // store cache
                if (!ignoreCacheTotally)
                {
                    SetCache(path, JsonSerializer.Current.SerializeObject(data));
                }

                // invoke callback
                callback.Invoke(jsondata);
            };
            client.GetAsync(path, parameters);
            AddToPendingTasks(path, client);
        }

        /// <summary>
        /// Posts a message on the given path of facebook graph api, with the given parameters
        /// when done the given callback is called
        /// </summary>
        public void PostGraph(string path, Dictionary<string, object> parameters, Action callback)
        {
            var client = CreateClient();

            client.PostCompleted += (a, b) =>
            {
                callback.Invoke();
            };

            client.PostAsync(path, parameters);
        }

        Dictionary<string, WebDownloader> PendingImageDownloaders = new Dictionary<string, WebDownloader>();
        Dictionary<string, List<Action<byte[]>>> PendingImageCallbacks = new Dictionary<string, List<Action<byte[]>>>();
        void AddToPendingImageDownloaders(string k, WebDownloader downloader, Action<byte[]> callback)
        {
            lock (PendingImageDownloaders)
            {
                PendingImageDownloaders[k] = downloader;

                if (PendingImageCallbacks.ContainsKey(k))
                {
                    PendingImageCallbacks[k].Add(callback);
                }
                else
                {
                    PendingImageCallbacks[k] = new[] { callback }.ToList();
                }
            }
        }
        void CompletePendingImageDownloader(string k, byte[] data)
        {
            lock (PendingImageDownloaders)
            {
                if (PendingImageCallbacks.ContainsKey(k))
                {
                    foreach (var callback in PendingImageCallbacks[k])
                    {
                        callback.Invoke(data);
                    }
                    PendingImageCallbacks.Remove(k);
                }

                PendingImageDownloaders.Remove(k);
            }
        }

        /// <summary>
        /// Cancels all current pending image download tasks
        /// </summary>
        public void CancelAllPendingImageDownloaders()
        {
            lock (PendingImageDownloaders)
            {
                foreach (var item in PendingImageDownloaders)
                {
                    item.Value.Cancel();
                }
            }
        }

        /// <summary>
        /// Gets an image with the given path and given parameters
        /// when done the given callback is called
        /// </summary>
        public void GetImage(string path, Action<byte[]> callback, IDictionary<string, object> parameters = null)
        {
            // TODO: this must be more intelligent to avoid multiple download thread of the same image
            var cacheKey = path;

            if (parameters == null) parameters = new Dictionary<string, object>();
            foreach (var par in parameters) { cacheKey += "_{0}_{1}".FormatWith(par.Key, par.Value); }

            var cached = GetCache(cacheKey);
            if (cached != null)
            {
                callback.Invoke(Convert.FromBase64String(cached));
                return;
            }

            var client = CreateClient();

            parameters.Add("access_token", client.AccessToken);
            var url = client.ResolveUrl(path, parameters);

            var downloader = new WebDownloader
            {
                Url = url,
                ReadBinary = true
            };

            downloader.Completed += (a, b) =>
            {
                var bytes = downloader.DataBytes;

                SetCache(cacheKey, Convert.ToBase64String(bytes));

                //callback.Invoke(bytes);
                CompletePendingImageDownloader(cacheKey, bytes);
            };

            downloader.Failed += (a, b) =>
            {
                // nothing
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
            };

            downloader.Download();

            // add to pending list
            AddToPendingImageDownloaders(cacheKey, downloader, callback);
        }

        /// <summary>
        /// Get cache content for the given path
        /// </summary>
        static string GetCache(string path)
        {
            if (DataAccessDisableCacheContext.IgnoreCache) return null;

            var lines = IOHelper.File.ReadAllLines(GetCacheFileName(path));
            if (lines.None()) return null;

            var cachedOn = DateTime.MinValue;

            var first = lines.First();
            if (first.StartsWith("#"))
            {
                cachedOn = DateTime.FromOADate(double.Parse(first.TrimStart('#')));
                lines = lines.Skip(1).ToArray();
            }

            if (IsCacheExpired(path, cachedOn)) return null;

            return lines.ToString(Environment.NewLine);
        }

        static Dictionary<string, TimeSpan> CacheExpirationRules = new Dictionary<string, TimeSpan>
        {
            {"_default", TimeSpan.FromDays(1) },
            {"notes", TimeSpan.FromDays(7) },
            {"statuses", TimeSpan.FromHours(1)},
            {"albums", TimeSpan.FromDays(7) },
            {"photos", TimeSpan.FromDays(2) },
            {"notifications", TimeSpan.FromMinutes(10) },
            {"home", TimeSpan.FromMinutes(5) },
            {"likes", TimeSpan.FromMinutes(5) },
            {"comments", TimeSpan.FromMinutes(5) },
            {"picture", TimeSpan.FromDays(30)},
        };

        static bool IsCacheExpired(string path, DateTime cachedOn)
        {
            var t = path.Split('/').LastOrDefault().Or("");
            if (t.Contains('?')) t = t.Remove(t.IndexOf('?'));
            if (t.Contains('_')) t = t.Remove(t.IndexOf('_'));

            var timespan = CacheExpirationRules.ContainsKey(t) ? CacheExpirationRules[t] : CacheExpirationRules["_default"];

            return cachedOn.Add(timespan) <= DateTime.Now;
        }
        static void SetCache(string path, string value)
        {
            var content = @"#{0}
{1}".FormatWith(DateTime.Now.ToOADate(), value);

            IOHelper.File.WriteAllText(GetCacheFileName(path), content);
        }
        static string GetCacheFileName(string path)
        {
            return CACHE_FOLDER + "/" + GetCacheFriendlyKey(path);
        }
        static string GetCacheFriendlyKey(string path)
        {
            return path.Replace("/", "_").Replace("\\", "_").Replace("?", "_").Replace("=", "_") + ".dat";
        }

        /// <summary>
        /// Clears cache
        /// when done the given callback is fired
        /// </summary>
        public static void ClearCache(Action callback)
        {
            ThreadHelper.RunUniqueAsync(() =>
            {
                foreach (var file in IOHelper.Directory.GetFiles(CACHE_FOLDER))
                {
                    IOHelper.File.Delete(CACHE_FOLDER + "/" + file);
                }

                callback.Invoke();
            });
        }

        /// <summary>
        /// Logs the current user out
        /// When done the given callback is fired
        /// </summary>
        public void LogOut(Action callback)
        {
            ClearCache(() =>
            {
                Current.RegisterToken(null);

                callback.Invoke();
            });
        }

        const string GENERIC_DATA_ERROR_MESSAGE = @"An error occured while trying to read data.
This might be because you do not have permission to view this content.";

        /// <summary>
        /// Determines whether or not a user is logged in at the moment
        /// </summary>
        public bool IsLoggedIn()
        {
            return Token.HasValue();
        }

        public const int LIST_LIMIT_SIZE = 25;
    }

    public class DataAccessDisableCacheContext : IDisposable
    {
        public static bool IgnoreCache { get; private set; }

        public DataAccessDisableCacheContext()
        {
            IgnoreCache = true;
        }

        public void Dispose()
        {
            IgnoreCache = false;
        }
    }
}
