using System;
using System.Windows.Forms;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace keyListener {
    public partial class Form1 : Form {
        private bool allowVisible = false;
        private bool allowClose = false;

        public Form1() {
            InitializeComponent();
        }

        protected override void SetVisibleCore(bool value) {
            if (!allowVisible) {
                value = false;
                if (!IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            if (!allowClose) {
                Hide();
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        public void Act(int vkCode) {
            bool mediaPressed = true;
            string route = "";
            switch((Keys)vkCode) {
                case Keys.MediaPlayPause:
                    route = "PlayPause";
                    break;
                case Keys.MediaNextTrack:
                    route = "NextTrack";
                    break;
                case Keys.MediaPreviousTrack:
                    route = "PreviousTrack";
                    break;
                default:
                    mediaPressed = false;
                    break;
            }
            if(mediaPressed) {
                var request = (HttpWebRequest)WebRequest.Create("localhost:8000/" + route);

                var postData = "request=true";

                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream()) {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var respocseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e) {
            allowVisible = true;
            Show();
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e) {
            allowClose = true;
            Application.Exit();
        }
    }
}
