using System.Windows.Forms;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    static class AuthManager
    {
        internal static string Authenticate(IWin32Window dialogOwner, FlickrContext context)
        {
            object frob;

            FlickrNet.Flickr.CacheDisabled = true;

            System.Resources.ResourceManager mgr = new System.Resources.ResourceManager(typeof(Properties.Resources));
            FlickrNet.Flickr flickrProxy = new FlickrNet.Flickr(mgr.GetString("ApiKey"), mgr.GetString("SharedSecret"));

            // Leverage proxy settings if they are there.
            System.Net.WebProxy proxySettings = (System.Net.WebProxy)WindowsLive.Writer.Api.PluginHttpRequest.GetWriterProxy();
            if (proxySettings != null)
            {
                flickrProxy.Proxy = proxySettings;
            }

            using (AuthForm authf = new AuthForm(flickrProxy))
            {
                if (authf.ShowDialog(dialogOwner) != DialogResult.OK)
                {
                    return null;
                }

                frob = authf.Frob;
            }

            using (CompletedAuth complete = new CompletedAuth(flickrProxy, frob))
            {
                if (complete.ShowDialog(dialogOwner) != DialogResult.OK)
                {
                    // auth failed
                    MessageBox.Show(@"Authorization has failed.  Please try again", @"Authorization Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                context.FlickrAuthToken = complete.AuthToken;
                context.FlickrAuthTokenSecret = complete.AuthTokenSecret;

                return complete.AuthToken;
            }
        }
    }
}
