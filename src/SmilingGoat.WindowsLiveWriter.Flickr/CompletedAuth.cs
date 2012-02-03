using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using FlickrNet;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    public partial class CompletedAuth : Form
    {
        public string Frob { get; set; }
        public string AuthToken { get; set; }
        public string AuthUserId { get; set; }
        private FlickrNet.Flickr _proxy;

        public CompletedAuth(FlickrNet.Flickr flickrProxy, string frob)
        {
            InitializeComponent();
            _proxy = flickrProxy;
            this.Frob = frob;
            Load += new EventHandler(CompletedAuth_Load);
        }

        void CompletedAuth_Load(object sender, EventArgs e)
        {
            completedAuthWorker.RunWorkerAsync();
        }

        private void completedAuthWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                try
                {

                    FlickrNet.Auth token = _proxy.AuthGetToken(this.Frob);
                    if (token == null)
                    {
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        e.Result = token.Token;
                        break;
                    }
                }
                catch (FlickrNet.FlickrApiException ex)
                {
                    if (ex.Code == 108)
                    {
                        
                    }
                }
            }
        }

        private void completedAuthWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.AuthToken = (string)e.Result;
            base.DialogResult = DialogResult.OK;
        }
    }
}