using System;
using WindowsLive;
using WindowsLive.Writer;
using WindowsLive.Writer.Api;
using System.Text;
using System.Text.RegularExpressions;
using FlickrNet;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    [InsertableContentSource("Flickr Image"), 
    WriterPlugin("67d89376-1fb8-4939-a374-81ce8a6ebbaa", "Flickr Image Reference", "Images.FFWTrans.png", 
        PublisherUrl = "http://www.flickr4writer.com", Description = "Plugin for retrieving Flickr images")]

    [UrlContentSource(FlickrContentSource.PHOTO_REGEX_URL, 
        RequiresProgress=true, 
        ProgressCaption="Retrieving Image Details",
        ProgressMessage="Retrieving image details from image repository...")]

    public class FlickrContentSource : ContentSource
    {
        internal const string PHOTO_REGEX_URL = @"^http://(www\.)?flickr\.com/photos/[^/]+/(?<id>[\d]+)($|/)";

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
        
        #region WLW Overrides
        public override System.Windows.Forms.DialogResult CreateContent(System.Windows.Forms.IWin32Window dialogOwner, ref string newContent)
        {
            using (InsertFlickrImageForm flickr = new InsertFlickrImageForm(new FlickrContext(base.Options)))
            {
                System.Windows.Forms.DialogResult formResult = flickr.ShowDialog(dialogOwner);

                FlickrContext context = new FlickrContext(base.Options);
                context.FlickrUserId = flickr.FlickrUserId.Trim();
                context.FlickrUserName = flickr.FlickrUserName.Trim();

                if (formResult == System.Windows.Forms.DialogResult.OK)
                {
                    newContent = FlickrPluginHelper.GenerateFlickrHtml(flickr.SelectedPhoto, flickr.ImageSourceUrl, flickr.CssClass, flickr.BorderThickness, flickr.VerticalPadding, flickr.HorizontalPadding, flickr.Alignment, flickr.EnableHyperLink, flickr.FlickrUserId);
                }
                return formResult;
            }
        }
        #endregion
    }
}
