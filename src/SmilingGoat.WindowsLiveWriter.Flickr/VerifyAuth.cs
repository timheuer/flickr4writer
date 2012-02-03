using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SmilingGoat.WindowsLiveWriter.Flickr
{
    public partial class VerifyAuth : Form
    {
        public VerifyAuth()
        {
            InitializeComponent();
        }

        public event DoWorkEventHandler DoWork;

        private void verifyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (this.DoWork != null)
            {
                this.DoWork(sender, e);
            }
        }

        private void verifyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                base.DialogResult = DialogResult.Cancel;
            }
            else
            {
                if (e.Error != null)
                {
                    throw e.Error;
                }
                base.DialogResult = DialogResult.OK;
            }
        }

        private void waitTimer_Tick(object sender, EventArgs e)
        {
            this.waitTimer.Stop();
            this.verifyWorker.RunWorkerAsync();
        }
    }
}
