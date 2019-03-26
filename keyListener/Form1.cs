using System;
using System.Windows.Forms;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace keyListener {
    public partial class Form1 : Form {
        private bool allowVisible = false;
        private bool allowClose = false;
        private string token = null;

        Dictionary<string, string> config;

        public Form1() {
            string path = Path.Combine(Environment.CurrentDirectory, "config.json");
            config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path, Encoding.UTF8));
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
                if(config.Count > 0 && token != null) {
                    string URL = "http://" + config["server_ip"] + ":" + config["server_port"] + "/api/" + route;
                    var request = (HttpWebRequest)WebRequest.Create(URL);

                    var postData = "request=true";

                    var data = Encoding.ASCII.GetBytes(postData);

                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Headers.Add("Authorization", token);
                    request.ContentLength = data.Length;

                    try {
                        using (var stream = request.GetRequestStream()) {
                            stream.Write(data, 0, data.Length);
                        }

                        var response = (HttpWebResponse)request.GetResponse();
                        var respocseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    } catch (Exception ex) {

                    }
                }
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

        private void getTokenToolStripMenuItem_Click(object sender, EventArgs e) {
            if (config.Count > 0) {
                string URL = "http://" + config["server_ip"] + ":" + config["server_port"] + "/auth/login";
                var request = (HttpWebRequest)WebRequest.Create(URL);

                var postData = "email="+config["email"] + "&password=" + config["password"];
                var data = Encoding.ASCII.GetBytes(postData);

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                try {
                    using (var stream = request.GetRequestStream()) {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = (HttpWebResponse)request.GetResponse();
                    var respocseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    Response jsonResp = JsonConvert.DeserializeObject<Response>(respocseString.ToString());

                    if (jsonResp.success == true) {
                        token = jsonResp.token;
                    }
                } catch (Exception ex) {
                }
            }
        }
        public class Response {
            public class User {
                [JsonProperty("name")]
                public string name { get; set; }
            }

            [JsonProperty("success")]
            public bool success { get; set; }

            [JsonProperty("message")]
            public string message { get; set; }

            [JsonProperty("token")]
            public string token { get; set; }
            
            [JsonProperty("gameserver")]    
            public User user { get; set; }
        }
    }
}
