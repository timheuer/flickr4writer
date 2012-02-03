using System;
using WindowsLive;
using WindowsLive.Writer;
using WindowsLive.Writer.Api;
using System.Text;
using System.Text.RegularExpressions;
using FlickrNet;
using System.Windows.Forms;
using System.ComponentModel;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    [InsertableContentSource("Flickr Image"), 
    WriterPlugin("67d89376-1fb8-4939-a374-81ce8a6ebbaa", "Flickr Image Reference", "Images.FlickrOfficialIcon.png", 
        PublisherUrl = "http://www.flickr4writer.com", Description = "Plugin for retrieving Flickr images", HasEditableOptions=true)]

    [UrlContentSource(FlickrContentSource.PHOTO_REGEX_URL, 
        RequiresProgress=true, 
        ProgressCaption="Retrieving Image Details",
        ProgressMessage="Retrieving image details from image repository...")]

    public class FlickrContentSource : ContentSource
    {
        internal const string PHOTO_REGEX_URL = @"^http://(www\.)?flickr\.com/photos/[^/]+/(?<id>[\d]+)($|/)";

        internal string token;

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
                FlickrNet.Flickr flickrProxy = FlickrPluginHelper.GetFlickrProxy();

                FlickrNet.PhotoInfo photo = flickrProxy.PhotosGetInfo(photoId);

                title = photo.Title;
                newContent = string.Format("<p><a href=\"{0}\" title=\"{2}\"><img alt=\"{2}\" border=\"0\" src=\"{1}\"></a></p>", photo.WebUrl, photo.MediumUrl, HtmlServices.HtmlEncode(photo.Title));
            }
        }

        public string GetImageUrl(FlickrNet.Photo p, ImageSize imgsize)
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
        public override System.Windows.Forms.DialogResult CreateContent(System.Windows.Forms.IWin32Window dialogOwner, ref string newContent)
        {
            DialogResult result;
            DoWorkEventHandler handler = null;
            FlickrNet.Auth validAuthToken = null;
            FlickrContext context = new FlickrContext(base.Options);
            token = context.FlickrAuthToken;

            // we have a token saved already and 
            // need to verify it with Flickr
            if (!string.IsNullOrEmpty(token))
            {
                using (VerifyAuth vauth = new VerifyAuth())
                {
                    if (handler == null)
                    {
                        handler = delegate(object sender, DoWorkEventArgs args)
                        {
                            FlickrNet.Flickr fp = FlickrPluginHelper.GetFlickrProxy();
                            validAuthToken = fp.AuthCheckToken(token);
                            if (validAuthToken != null)
                            {
                                token = validAuthToken.Token;
                            }
                        };
                    }
                    vauth.DoWork += handler;
                    result = vauth.ShowDialog(dialogOwner);
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
            if (string.IsNullOrEmpty(token) || (validAuthToken == null))
            {
                token= AuthManager.Authenticate(dialogOwner, context);
            }

            if (string.IsNullOrEmpty(token))
            {
                return DialogResult.Cancel;
            }

            using (InsertFlickrImageForm flickr = new InsertFlickrImageForm(new FlickrContext(base.Options)))
            {
                System.Windows.Forms.DialogResult formResult = flickr.ShowDialog(dialogOwner);

                context.FlickrUserId = flickr.FlickrUserId.Trim();
                context.FlickrUserName = flickr.FlickrUserName.Trim();
                context.FlickrAuthUserId = flickr.FlickrAuthUserId.Trim();

                if (formResult == System.Windows.Forms.DialogResult.OK)
                {
                    ImageSize imgsize = flickr.SelectedImageSize;

                    foreach (FlickrNet.Photo photo in flickr.SelectedPhotos)
                    {
                        newContent += FlickrPluginHelper.GenerateFlickrHtml(photo, GetImageUrl(photo, imgsize), flickr.CssClass, flickr.BorderThickness, flickr.VerticalPadding, flickr.HorizontalPadding, flickr.Alignment, flickr.EnableHyperLink, flickr.FlickrUserId);
                    }
                }
                return formResult;
            }
        }

        public override void EditOptions(System.Windows.Forms.IWin32Window dialogOwner)
        {
            // authenticate as needed
            token = AuthManager.Authenticate(dialogOwner, new FlickrContext(base.Options));
        }
        #endregion
    }
}