using System;
using WindowsLive.Writer.Api;

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
        #endregion
    }
}
