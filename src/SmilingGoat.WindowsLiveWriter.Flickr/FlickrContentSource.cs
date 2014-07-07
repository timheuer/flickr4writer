using FlickrNet;
using log4net;
using log4net.Config;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WindowsLive.Writer.Api;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    [WriterPlugin("67d89376-1fb8-4939-a374-81ce8a6ebbaa", "Flickr Image Reference", Description = "Plugin for retrieving Flickr images", HasEditableOptions = true, ImagePath = "Images.FlickrOfficialIcon.png", PublisherUrl = "http://timheuer.com")]
    [InsertableContentSource("Flickr Image")]
    [UrlContentSource(PHOTO_REGEX_URL, 
        RequiresProgress=true, 
        ProgressCaption="Retrieving Image Details",
        ProgressMessage="Retrieving image details from image repository...")]
    public class FlickrContentSource : ContentSource
    {
        private const string PHOTO_REGEX_URL = @"^http://(www\.)?flickr\.com/photos/[^/]+/(?<id>[\d]+)($|/)";

        private string _token;

        private static readonly ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static FlickrContentSource()
        {
            FileInfo logConfig =
                new FileInfo(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "log4net.config"));

            XmlConfigurator.ConfigureAndWatch(logConfig);
        }

        public override void CreateContentFromUrl(string url, ref string title, ref string newContent)
        {
            Match m = Regex.Match(url, PHOTO_REGEX_URL);

            if (!m.Success)
            {
                base.CreateContentFromUrl(url, ref title, ref newContent);
            }
            else
            {
                string photoId = m.Groups["id"].Value;

                // get photo
                FlickrContext context = new FlickrContext(Options);
                FlickrNet.Flickr flickrProxy = FlickrPluginHelper.GetFlickrProxy();
                flickrProxy.OAuthAccessToken = context.FlickrAuthToken;
                flickrProxy.OAuthAccessTokenSecret = context.FlickrAuthTokenSecret;

                PhotoInfo photo = flickrProxy.PhotosGetInfo(photoId);

                title = photo.Title;
                newContent = string.Format("<p><a href=\"{0}\" title=\"{2}\"><img alt=\"{2}\" border=\"0\" src=\"{1}\"></a></p>", photo.WebUrl, photo.MediumUrl, HtmlServices.HtmlEncode(photo.Title));
            }
        }

        public string GetImageUrl(Photo p, ImageSize imgsize)
        {
            string url = null;

            switch (imgsize)
            {
                case ImageSize.Thumbnail:
                    url = p.ThumbnailUrl;
                    break;

                case ImageSize.Small:
                    url = p.SmallUrl;
                    break;

                case ImageSize.Medium:
                    url = p.MediumUrl;
                    break;

                case ImageSize.Large:
                    url = p.LargeUrl;
                    break;
            }

            return url;
        }

        #region WLW Overrides
        public override DialogResult CreateContent(IWin32Window dialogOwner, ref string newContent)
        {
            Auth validAuthToken = null;
            FlickrContext context = new FlickrContext(Options);
            _token = context.FlickrAuthToken;

            // we have a token saved already and 
            // need to verify it with Flickr
            if (!string.IsNullOrEmpty(_token))
            {
                using (VerifyAuth vauth = new VerifyAuth())
                {
                    DoWorkEventHandler handler = delegate
                    {
                        FlickrNet.Flickr fp = FlickrPluginHelper.GetFlickrProxy();
                        fp.OAuthAccessTokenSecret = context.FlickrAuthTokenSecret;
                        fp.OAuthAccessToken = context.FlickrAuthToken;

                        Logger.DebugFormat("Auth Token: {0}", context.FlickrAuthToken);
                        Logger.DebugFormat("Auth Secret: {0}", context.FlickrAuthTokenSecret);

                        try
                        {
                            validAuthToken = fp.AuthOAuthCheckToken();
                        }
                        catch (FlickrNet.Exceptions.LoginFailedInvalidTokenException ex)
                        {
                            // token that was stored was bad -- re-auth
                            _token = AuthManager.Authenticate(dialogOwner, context);
                            fp.OAuthAccessToken = context.FlickrAuthToken;
                            fp.OAuthAccessTokenSecret = context.FlickrAuthTokenSecret;
                            validAuthToken = fp.AuthOAuthCheckToken();

                            PluginDiagnostics.LogException(ex, "Failure in Auth Verification");
                        }

                        if (validAuthToken != null)
                        {
                            _token = validAuthToken.Token;
                        }
                    };
                    vauth.DoWork += handler;
                    DialogResult result = vauth.ShowDialog(dialogOwner);
                    if (result != DialogResult.OK)
                    {
                        return result;
                    }
                }
            }

            /* we didn't get a valid auth token
             * it might have expired or is just invalid/revoked
             * prompt the user to re-auth
             * OR
             * we don't have a saved token and know
             * we need to get one first so show the auth process
             */
            if (string.IsNullOrEmpty(_token) || (validAuthToken == null))
            {
                _token= AuthManager.Authenticate(dialogOwner, context);
            }

            if (string.IsNullOrEmpty(_token))
            {
                return DialogResult.Cancel;
            }

            using (InsertFlickrImageForm flickr = new InsertFlickrImageForm(new FlickrContext(Options)))
            {
                DialogResult formResult = flickr.ShowDialog(dialogOwner);

                context.FlickrUserId = flickr.FlickrUserId.Trim();
                context.FlickrUserName = flickr.FlickrUserName.Trim();
                context.FlickrAuthUserId = flickr.FlickrAuthUserId.Trim();

                if (formResult == DialogResult.OK)
                {
                    ImageSize imgsize = flickr.SelectedImageSize;

                    foreach (Photo photo in flickr.SelectedPhotos)
                    {
                        newContent += FlickrPluginHelper.GenerateFlickrHtml(photo, GetImageUrl(photo, imgsize), flickr.CssClass, flickr.BorderThickness, flickr.VerticalPadding, flickr.HorizontalPadding, flickr.Alignment, flickr.EnableHyperLink, flickr.FlickrUserId);
                    }
                }
                return formResult;
            }
        }

        public override void EditOptions(IWin32Window dialogOwner)
        {
            // authenticate as needed
            _token = AuthManager.Authenticate(dialogOwner, new FlickrContext(Options));
        }
        #endregion
    }
}