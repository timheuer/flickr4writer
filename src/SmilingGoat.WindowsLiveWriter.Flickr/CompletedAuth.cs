using System;
using System.ComponentModel;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Threading;
using FlickrNet;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    public partial class CompletedAuth : Form
    {
        private object Frob { get; set; }
        public string AuthToken { get; private set; }
        public string AuthTokenSecret { get; private set; }
        private readonly FlickrNet.Flickr _proxy;
        private string _verifierCode;

        public CompletedAuth(FlickrNet.Flickr flickrProxy, object frob)
        {
            InitializeComponent();
            _proxy = flickrProxy;
            Frob = frob;
            Load += CompletedAuth_Load;
        }

        void CompletedAuth_Load(object sender, EventArgs e)
        {
            VerifierCode.Focus();
        }

        private void completedAuthWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                try
                {
                    var token = _proxy.OAuthGetAccessToken((OAuthRequestToken) Frob, _verifierCode);
                    
                    if (token == null)
                    {
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        e.Result = token;
                        break;
                    }
                }
                catch (FlickrApiException ex)
                {
                    if (ex.Code == 108)
                    {
                        
                    }
                }
            }
        }

        private void completedAuthWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AuthToken = ((OAuthAccessToken) e.Result).Token;
            AuthTokenSecret = ((OAuthAccessToken)e.Result).TokenSecret;
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _verifierCode = VerifierCode.Text;
            completedAuthWorker.RunWorkerAsync();
        }
    }
}