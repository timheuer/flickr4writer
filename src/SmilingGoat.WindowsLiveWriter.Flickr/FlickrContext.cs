using System;
using WindowsLive.Writer.Api;
using System.ComponentModel;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    public class FlickrContext
    {
        #region Constructors
        public FlickrContext(IProperties pluginSettings)
        {
            _pluginSettings = pluginSettings;
        }
        #endregion

        #region Member Variables
        private IProperties _pluginSettings;
        #endregion

        #region Public Variables
        public string FlickrAuthUserId
        {
            get { return _pluginSettings.GetString("FlickrAuthUserId", string.Empty); }
            set { _pluginSettings.SetString("FlickrAuthUserId", value); }
        }
        public string FlickrAuthUserName
        {
            get { return _pluginSettings.GetString("FlickrAuthUserName", string.Empty); }
            set { _pluginSettings.SetString("FlickrAuthUserName", value); }
        }
        public string FlickrUserName
        {
            get { return _pluginSettings.GetString("FlickrUserName", string.Empty); }
            set { _pluginSettings.SetString("FlickrUserName", value); }
        }

        public string FlickrTags
        {
            get { return _pluginSettings.GetString("FlickrTags", string.Empty); }
            set { _pluginSettings.SetString("FlickrTags", value); }
        }

        public string FlickrUserId
        {
            get { return _pluginSettings.GetString("FlickrUserId", string.Empty); }
            set { _pluginSettings.SetString("FlickrUserId", value); }
        }

        public string FlickrAuthToken
        {
            get { return _pluginSettings.GetString("FlickrAuthToken", string.Empty); }
            set { _pluginSettings.SetString("FlickrAuthToken", value); }
        }

        public int DefaultImageSize
        {
            get { return _pluginSettings.GetInt("DefaultImageSize", (int)ImageSize.Medium); }
            set { _pluginSettings.SetInt("DefaultImageSize", value); }
        }
        #endregion
    }
}