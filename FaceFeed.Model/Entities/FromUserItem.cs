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
    /// Represents abstraction of a facebook entity that is of "FromUser..." nature
    /// </summary>
    public abstract class FromUserItem : FBEntity
    {
        public string From { get; set; }
        public string From_Id { get; set; }

        byte[] _FromImageData;

        /// <summary>
        /// Profile picture data of the "From" user
        /// </summary>
        public byte[] FromImageData
        {
            get
            {
                if (_FromImageData == null)
                {
                    LoadImage();
                }
                return _FromImageData;
            }
        }

        void LoadImage()
        {
            FBDataAccess.Current.GetImage(From_Id + "/picture", data =>
            {
                _FromImageData = data;
                NotifyPropertyChanged("FromImageData");
            });
        }
    }
    
}
