using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    public partial class AuthForm : Form
    {
        private FlickrNet.Flickr _proxy;
        private string tempFrob;

        public string Frob { get; set; }

        public AuthForm(FlickrNet.Flickr flickrProxy)
        {
            InitializeComponent();
            _proxy = flickrProxy;
        }

        private void authBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            tempFrob = _proxy.AuthGetFrob();
            string flickrUrl = _proxy.AuthCalcUrl(tempFrob, FlickrNet.AuthLevel.Write);
            System.Diagnostics.Process.Start(flickrUrl);
            Thread.Sleep(2000);
            e.Result = tempFrob;
        }

        private void authBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Frob = (string)e.Result;
                base.DialogResult = DialogResult.OK;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            authBackgroundWorker.RunWorkerAsync();
        }
    }
}
