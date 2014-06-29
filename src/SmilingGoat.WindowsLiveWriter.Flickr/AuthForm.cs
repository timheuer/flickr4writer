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
        private FlickrNet.Flickr _proxy;
        private string _tempFrob;

        public string Frob { get; set; }

        public AuthForm(FlickrNet.Flickr flickrProxy)
        {
            InitializeComponent();
            _proxy = flickrProxy;
        }

        private void authBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _tempFrob = _proxy.AuthGetFrob();
            string flickrUrl = _proxy.AuthCalcUrl(_tempFrob, AuthLevel.Write);
            Process.Start(flickrUrl);
            Thread.Sleep(2000);
            e.Result = _tempFrob;
        }

        private void authBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Frob = (string)e.Result;
                DialogResult = DialogResult.OK;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            authBackgroundWorker.RunWorkerAsync();
        }
    }
}
