using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    class AuthManager
    {
        internal static string Authenticate(System.Windows.Forms.IWin32Window dialogOwner, FlickrContext context)
        {
            string frob;

            FlickrNet.Flickr.CacheDisabled = true;
            FlickrNet.Flickr flickrProxy = null;

            System.Resources.ResourceManager mgr = new System.Resources.ResourceManager(typeof(SmilingGoat.WindowsLiveWriter.Flickr.Properties.Resources));
            flickrProxy = new FlickrNet.Flickr(mgr.GetString("ApiKey"), mgr.GetString("SharedSecret"));

            // Leverage proxy settings if they are there.
            System.Net.WebProxy proxySettings = (System.Net.WebProxy)WindowsLive.Writer.Api.PluginHttpRequest.GetWriterProxy();
            if (proxySettings != null)
            {
                flickrProxy.Proxy = proxySettings;
            }

            using (AuthForm authf = new AuthForm(flickrProxy))
            {
                if (authf.ShowDialog(dialogOwner) != System.Windows.Forms.DialogResult.OK)
                {
                    return null;
                }

                frob = authf.Frob;
            }

            using (CompletedAuth complete = new CompletedAuth(flickrProxy, frob))
            {
                if (complete.ShowDialog(dialogOwner) != System.Windows.Forms.DialogResult.OK)
                {
                    // auth failed
                    MessageBox.Show("Authorization has failed.  Please try again", "Authorization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                context.FlickrAuthToken = complete.AuthToken;

                return complete.AuthToken;
            }
        }
    }
}
