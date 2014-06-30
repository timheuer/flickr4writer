using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using FlickrNet;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    public partial class AuthForm : Form
    {
        private readonly FlickrNet.Flickr _proxy;
        public object Frob { get; private set; }

        public AuthForm(FlickrNet.Flickr flickrProxy)
        {
            InitializeComponent();
            _proxy = flickrProxy;
        }

        private void authBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var token = _proxy.OAuthGetRequestToken("oob");
            var authUrl = _proxy.OAuthCalculateAuthorizationUrl(token.Token, AuthLevel.Write);
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
