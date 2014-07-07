using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;
using WindowsLive.Writer.Api;
using FlickrNet;
using SmilingGoat.WindowsLiveWriter.Flickr.Properties;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    class SpecialFolderPatternConverter : log4net.Util.PatternConverter
    {
        protected override void Convert(TextWriter writer, object state)
        {
            Environment.SpecialFolder specialFolder =
                (Environment.SpecialFolder) Enum.Parse(typeof (Environment.SpecialFolder), base.Option, true);
            writer.Write(Environment.GetFolderPath(specialFolder));
        }
    }

    class FlickrPluginHelper
    {
        internal static void GotoUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                PluginDiagnostics.DisplayUnexpectedException(ex);
            }
        }

        internal static FlickrNet.Flickr GetFlickrProxy()
        {
            FlickrNet.Flickr.CacheDisabled = true;

            ResourceManager mgr = new ResourceManager(typeof(Resources));
            FlickrNet.Flickr fproxy = new FlickrNet.Flickr(mgr.GetString("ApiKey"), mgr.GetString("SharedSecret"));
            
            // Leverage proxy settings if they are there.
            WebProxy proxySettings = (WebProxy)PluginHttpRequest.GetWriterProxy();
            if (proxySettings != null)
            {
                fproxy.Proxy = proxySettings;
            }

            return fproxy;
        }

        internal static Image GetImageFromUrl(string url)
        {
            Image theImage;

            PluginHttpRequest http = new PluginHttpRequest(url);
            http.AllowAutoRedirect = true;
            http.CacheLevel = HttpRequestCacheLevel.BypassCache;

            using (Stream imageStream = http.GetResponse())
            {
                theImage = Image.FromStream(imageStream);
            }

            return theImage;
        }

        internal static string CleanTagFilter(string tagList)
        {
            // remove spaces
            string cleanedTagList = tagList.Replace(" ", string.Empty);

            // replace semicolons with commas
            cleanedTagList = cleanedTagList.Replace(";", ",");

            return cleanedTagList;
        }

        internal static string GenerateFlickrHtml(Photo selectedPhoto, string imageUrl, string cssClass, string border, string vSpace, string hSpace, string alignment, bool hyperLink, string userId)
        {
            StringBuilder imageTag = new StringBuilder();

            imageTag.Append("<img "); // begin tag
            imageTag.Append(string.Format("src=\"{0}\" ", HtmlServices.HtmlEncode(imageUrl)));
            imageTag.Append(string.Format("alt=\"{0}\" ", HtmlServices.HtmlEncode(selectedPhoto.Title))); // alt required for XHTML
            imageTag.Append(string.Format("border=\"{0}\" ", HtmlServices.HtmlEncode(border)));

            if (cssClass.Trim().Length > 0)
            {
                imageTag.Append(string.Format("class=\"{0}\" ", cssClass));
            }

            if (hSpace.Trim().Length > 0 && hSpace != "0")
            {
                imageTag.Append(string.Format("hspace=\"{0}\" ", hSpace));
            }

            if (vSpace.Trim().Length > 0 && vSpace != "0")
            {
                imageTag.Append(string.Format("vspace=\"{0}\" ", vSpace));
            }

            if (alignment.Trim().Length > 0 && alignment.ToLower() != "none")
            {
                imageTag.Append(string.Format("align=\"{0}\" ", alignment.ToLower()));
            }

            imageTag.Append("/>"); // end tag XHTML

            string imageHtml = imageTag.ToString();

            if (hyperLink)
            {
                imageHtml = string.Format("<a href=\"{0}\" title=\"{2}\">{1}</a>", HtmlServices.HtmlEncode(selectedPhoto.WebUrl), imageHtml, HtmlServices.HtmlEncode(selectedPhoto.Title));
                // TODO: Fix this temp hack in FlicrkNet library -- photoset photos array in FlickrAPI does not include owner attribute
                // thus the serialization lacks the Userid attribute to properly input the WebUrl
                // THIS IS A TEMP HACK TO FIX PHOTOSET ONLY
                imageHtml = FixPhotosetWebUrl(imageHtml, userId);

            }

            return imageHtml;
        }

        // TEMP SHIM FUNCTION
        internal static string FixPhotosetWebUrl(string url, string userId)
        {
            string photosetUrlPattern = "(?<PhotosetWebUrlHack>photos//)";
            return Regex.Replace(url, photosetUrlPattern, string.Format("photos/{0}/", userId));
        }
        
        internal static FoundUser FindUserByEmailOrName(FlickrNet.Flickr flickrProxy, string criteria)
        {
            FoundUser user;

            Regex rxp = new Regex("(?<user>[^@]+)@(?<host>.+)");

            if (rxp.IsMatch(criteria))
            {
                user = flickrProxy.PeopleFindByEmail(criteria);
            }
            else
            {
                user = flickrProxy.PeopleFindByUserName(criteria);
            }

            return user;
        }

        internal static Image ScaleToFixedSize(Image imgPhoto, int width, int height, int offset, Color backgroundColor)
        {
            int num1 = imgPhoto.Width;
            int num2 = imgPhoto.Height;
            int num3 = 0;
            int num4 = 0;
            int num5 = offset;
            int num6 = offset;
            float single1;
            float single2;
            float single3;
            single2 = width / ((float)num1);
            single3 = height / ((float)num2);
            if (single3 < single2)
            {
                single1 = single3;
                num5 = (int)((width - (num1 * single1)) / 2f);
            }
            else
            {
                single1 = single2;
                num6 = (int)((height - (num2 * single1)) / 2f);
            }
            int num7 = (int)(num1 * single1);
            int num8 = (int)(num2 * single1);
            Bitmap bitmap1 = new Bitmap(width, height);
            bitmap1.MakeTransparent(backgroundColor);
            bitmap1.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);
            Graphics graphics1 = Graphics.FromImage(bitmap1);
            graphics1.Clear(backgroundColor);
            graphics1.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics1.DrawImage(imgPhoto, new Rectangle(num5, num6, num7 - offset, num8 - offset), new Rectangle(num3, num4, num1, num2), GraphicsUnit.Pixel);
            graphics1.Dispose();
            return bitmap1;
        }
    }
}
