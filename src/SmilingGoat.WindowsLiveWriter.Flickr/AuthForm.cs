using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using FlickrNet;
using log4net;
using log4net.Config;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    public partial class AuthForm : Form
    {
        private readonly FlickrNet.Flickr _proxy;
        public object Frob { get; private set; }
        private static readonly ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static AuthForm()
        {
            FileInfo logConfig =
                new FileInfo(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "log4net.config"));

            XmlConfigurator.ConfigureAndWatch(logConfig);
        }

        public AuthForm(FlickrNet.Flickr flickrProxy)
        {
            InitializeComponent();
            _proxy = flickrProxy;
        }

        private void authBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var token = _proxy.OAuthGetRequestToken("oob");
            var authUrl = _proxy.OAuthCalculateAuthorizationUrl(token.Token, AuthLevel.Write);
            Logger.InfoFormat("Flickr OAuth URL: {0}", authUrl);
            Process.Start(authUrl);
            Thread.Sleep(2000);
            e.Result = token;
        }

        private void authBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled) return;
            Frob = e.Result;
            DialogResult = DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            authBackgroundWorker.RunWorkerAsync();
        }
    }
}
