using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WhatsAppApi.Helper;
using System.Threading;
using WinAppNET.AppCode;
using System.IO;
using System.Runtime.InteropServices;

namespace WinAppNET
{
    public partial class ChatWindow : Form
    {
        public BindingList<WappMessage> messages = new BindingList<WappMessage>();
        public string target;
        ClientState state = ClientState.ONLINE;
        public bool IsGroup = false;
        public bool stealFocus = false;

        enum ClientState
        {
            ONLINE,
            TYPING
        }

        void ProcessChat()
        {
            this.Text = "Chat with " + ContactStore.GetContactByJid(this.target).ToString();
            WappSocket.Instance.WhatsSendHandler.SendQueryLastOnline(this.target);
            WappSocket.Instance.WhatsSendHandler.SendPresenceSubscriptionRequest(this.target);

            //load image
            string filepath = this.getCacheImagePath();
            if (File.Exists(filepath))
            {
                try
                {
                    Image img = Image.FromFile(filepath);
                    this.pictureBox1.Image = img;
                }
                catch (Exception ex)
                {
                    this.GetImageAsync();
                }

            }
            else
            {
                this.GetImageAsync();
            }
        }

        protected string getCacheImagePath()
        {
            return Directory.GetCurrentDirectory() + "\\data\\profilecache\\" + this.target + ".jpg";
        }

        public void GetImageAsync()
        {
            WappSocket.Instance.WhatsSendHandler.SendGetPhoto(this.target, false);
        }

        void ProcessGroupChat()
        {
            this.Text = "Group chat " + ContactStore.GetContactByJid(this.target).ToString();
            WappSocket.Instance.WhatsSendHandler.SendGetGroupInfo(this.target);
        }

        public ChatWindow(string target, bool stealFocus)
        {
            this.stealFocus = stealFocus;
            if (stealFocus)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
            }
            if(target.Contains("-"))
                this.IsGroup = true;
            this.target = target;
            InitializeComponent();

            Contact con = ContactStore.GetContactByJid(target);

            if (con == null)
            {
                con = new Contact(-1, target, "", "", "", "");
                ContactStore.AddContact(con);
            }

            this.lblNick.Text = con.nickname;
            this.lblUserStatus.Text = con.status;

            listBox1.DataSource = messages;

            if (this.IsGroup)
                this.ProcessGroupChat();
            else
                this.ProcessChat();

            WappMessage[] oldmessages = MessageStore.GetAllMessagesForContact(target);
            foreach (WappMessage msg in oldmessages)
            {
                this.messages.Add(msg);
            }
        }

        delegate void ParentDelegate();

        public void DoActivate()
        {
            if (this.InvokeRequired)
            {
                ParentDelegate f = new ParentDelegate(DoActivate);
                this.Invoke(f);
            }
            else
            {
                FlashWindow(this.Handle, true);
            }
        }

        [DllImport("user32.dll")]
        static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        public void DoDispose()
        {
            if (this.InvokeRequired)
            {
                ParentDelegate f = new ParentDelegate(DoDispose);
                this.Invoke(f);
            }
            else
            {
                this.Dispose();
            }

        }

        delegate void AddMessageCallback(string data);

        delegate void AddMessageCallbackNode(ProtocolTreeNode node);
        delegate void SetOnlineCallback();
        delegate void SetLastSeenCallback(DateTime time);
        delegate void SetPictureCallback(Image img);

        public void SetPicture(Image img)
        {
            if (this.label1.InvokeRequired)
            {
                SetPictureCallback t = new SetPictureCallback(SetPicture);
                this.Invoke(t, img);
            }
            else
            {
                this.pictureBox1.Image = img;
            }
        }

        public void SetLastSeen(DateTime time)
        {
            if (this.label1.InvokeRequired)
            {
                SetLastSeenCallback t = new SetLastSeenCallback(SetLastSeen);
                this.Invoke(t, time);
            }
            else
            {
                this.label1.Text = "Last seen on " + time.ToString();
            }
        }

        public void SetOnline()
        {
            if (this.label1.InvokeRequired)
            {
                SetOnlineCallback t = new SetOnlineCallback(SetOnline);
                this.Invoke(t, null);
            }
            else
            {
                this.label1.Text = "Online";
            }
        }

        public void SetUnavailable()
        {
            if (this.label1.InvokeRequired)
            {
                SetOnlineCallback t = new SetOnlineCallback(SetUnavailable);
                this.Invoke(t, null);
            }
            else
            {
                this.label1.Text = "Unavailable";
            }
        }

        public void SetTyping()
        {
            if (this.label1.InvokeRequired)
            {
                SetOnlineCallback t = new SetOnlineCallback(SetTyping);
                this.Invoke(t, null);
            }
            else
            {
                this.label1.Text = "Typing...";
            }
        }

        public void AddMessage(string message)
        {
            if (this.listBox1.InvokeRequired)
            {
                AddMessageCallback r = new AddMessageCallback(AddMessage);
                this.Invoke(r, new object[] {message});
            }
            else
            {
                WappMessage msg = new WappMessage(message, this.target);
                this.messages.Add(msg);
                MessageStore.AddMessage(msg);
            }
        }

        public void AddMessage(ProtocolTreeNode node)
        {
            if (this.listBox1.InvokeRequired)
            {
                AddMessageCallbackNode r = new AddMessageCallbackNode(AddMessage);
                this.Invoke(r, new object[] { node });
            }
            else
            {
                WappMessage msg = new WappMessage(node, this.target);
                this.messages.Add(msg);
                MessageStore.AddMessage(msg);
                
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.textBox1.Text))
            {
                if (this.state == ClientState.ONLINE)
                {
                    this.state = ClientState.TYPING;
                    this.button1.Enabled = true;
                    WappSocket.Instance.WhatsSendHandler.SendComposing(this.target);
                }
            }
            else
            {
                if (state == ClientState.TYPING)
                {
                    this.state = ClientState.ONLINE;
                    this.button1.Enabled = false;
                    WappSocket.Instance.WhatsSendHandler.SendPaused(this.target);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.textBox1.Text))
            {
                WappSocket.Instance.Message(this.target, textBox1.Text);
                this.AddMessage(this.textBox1.Text);
                this.textBox1.Clear();
            }
        }

        private void ChatWindow_Load(object sender, EventArgs e)
        {
            this.textBox1.Focus();
            if (!this.stealFocus)
            {
                this.DoActivate();
            }
            this.stealFocus = false;//do not steal focus on incoming messages
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //redownload image
            this.pictureBox1.Image = null;
            this.GetImageAsync();
        }

        protected override bool ShowWithoutActivation
        {
            get
            {
                return !this.stealFocus;
            }
        }
    }
}
