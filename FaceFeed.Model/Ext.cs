using System;
using System.Linq;
using System.Collections.Generic;

namespace System
{
    public static class FBNavigationHelper
    {
        public static Dictionary<string, string> TypePageMapping = new Dictionary<string, string>
        {
            {"PhotoAlbum", "Album"},
            {"Photo", "Photo"},
            {"FeedItem", "FeedItem"},
            {"User", "User"},
            {"Page", "User"},
            {"Group", "Group"},
            {"Note", "Note"},
            {"Comment", "Comment"}
        };

        public static string GetSubsectionUrl(string pagename, string id, string idcontainer = null)
        {
            return "/Pages/{0}.xaml?id={1}&container={2}".FormatWith(pagename, id, idcontainer);
        }

        public static string TranslateFacebookLink(string link)
        {
            try
            {
                var url = new Uri(link.ToLower().Trim());
                var qs = url.Query.Or("").TrimStart('?').Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries).ToDictionary(a => a.Split('=').First(), a => a.Split('=').Last());
                var parts = url.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (url.AbsolutePath == "/photo.php")
                {
                    if (qs.ContainsKey("fbid"))
                    {
                        return "/Pages/Photo.xaml?id=" + qs["fbid"];
                    }
                }
                if (url.AbsolutePath == "/permalink.php")
                {
                    if (qs.ContainsKey("story_fbid"))
                    {
                        return "/Pages/FeedItem.xaml?id=" + qs["story_fbid"];
                    }
                }
                if (parts.Contains("posts"))
                {
                    return "/Pages/FeedItem.xaml?id=" + parts.Last();
                }
                else
                {
                    if (parts.Length > 2)
                    {
                        var type = parts.First().TrimEnd('s');
                        var map = TypePageMapping.Keys.ToArray().FirstOrDefault(a => a.ToLower() == type);
                        if (map != null)
                        {
                            return GetSubsectionUrl(TypePageMapping[map], parts.Last());
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return null;
        }
    }
    public static class __Ext
    {
        public static int ToUnixTimestamp(this DateTime date)
        {
            var b = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var res = (int)Math.Ceiling((date.ToUniversalTime() - b).TotalSeconds);
            return res;
        }
    }
}
