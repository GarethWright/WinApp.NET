using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinAppNET.AppCode;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading;
using System.IO;
using WhatsAppApi.Helper;

namespace WinAppNET
{
    public partial class ContactsList : Form
    {
        public BindingList<Contact> contacts = new BindingList<Contact>();
        public Dictionary<string, ChatWindow> ChatWindows = new Dictionary<string, ChatWindow>();
        public ContactsSelector selector;
        protected string username;
        protected string password;

        public ContactsList()
        {
            InitializeComponent();
            this.FormClosing += this.ContactsList_OnClosing;
        }

        private void ContactsList_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ChatWindow w in this.ChatWindows.Values)
            {
                //close chat windows
                w.DoDispose();
            }
        }

        delegate void remoteDelegate();

        protected void _loadConversations()
        {
            if (this.InvokeRequired)
            {
                remoteDelegate r = new remoteDelegate(_loadConversations);
                this.Invoke(r);
            }
            else
            {
                DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
                using (DbConnection cnn = fact.CreateConnection())
                {
                    cnn.ConnectionString = MessageStore.ConnectionString;
                    cnn.Open();
                    DbCommand cmd = cnn.CreateCommand();
                    cmd = cnn.CreateCommand();
                    cmd.CommandText = "SELECT jid FROM Messages GROUP BY jid";
                    DbDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string jid = reader["jid"].ToString();
                        Contact contact = ContactStore.GetContactByJid(jid);
                        if (contact != null)
                        {
                            this.contacts.Add(contact);
                        }
                    }
                }

                //done
                this.label1.Hide();
                this.listBox1.Enabled = true;
            }
        }

        protected void SyncWaContactsAsync()
        {
            ContactStore.SyncWaContacts(this.username, this.password);
            this._loadConversations();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
             if (this.listBox1.SelectedItem != null)
             {
                 Contact item = (Contact)this.listBox1.SelectedItem;
                 Thread chat = new Thread(new ParameterizedThreadStart(OpenConversation));
                 chat.Start(item.jid);
             }
        }

        public void OpenConversation(object jid)
        {
            if (!this.ChatWindows.ContainsKey(jid.ToString()))
            {
                this.ChatWindows.Add(jid.ToString(), new ChatWindow(jid.ToString()));//create
            }
            else if (this.ChatWindows[jid.ToString()].IsDisposed)
            {
                this.ChatWindows[jid.ToString()] = new ChatWindow(jid.ToString());//renew
            }
            else
            {
                this.ChatWindows[jid.ToString()].DoActivate();
                return;
            }
            try
            {
                Application.Run(this.ChatWindows[jid.ToString()]);//.Show();
            }
            catch (Exception e)
            {

            }
        }

        public void OpenConversationThread(string jid)
        {
            try
            {
                Thread t = new Thread(new ParameterizedThreadStart(OpenConversation));
                t.IsBackground = true;
                t.Start(jid);
            }
            catch (Exception e)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.selector == null || this.selector.IsDisposed)
            {
                this.selector = new ContactsSelector();
            }
            DialogResult res = this.selector.ShowDialog(this);
            if (res == DialogResult.OK)
            {
                this.OpenConversationThread(this.selector.SelectedJID);
            }
            this.selector.Dispose();
        }

        protected ChatWindow getChat(string jid, bool forceOpen)
        {
            if (forceOpen)
            {
                Thread t = new Thread(new ParameterizedThreadStart(this.OpenConversation));
                t.Start(jid);
            }

            if (this.ChatWindows.ContainsKey(jid) && !this.ChatWindows[jid].IsDisposed)
            {
                return this.ChatWindows[jid];
            }

            while (forceOpen)
            {
                Thread.Sleep(100);
                if (this.ChatWindows.ContainsKey(jid) && !this.ChatWindows[jid].IsDisposed)
                {
                    return this.ChatWindows[jid];
                }
            }
            return null;
        }

        protected void ProcessMessages(ProtocolTreeNode[] nodes)
        {
            foreach (ProtocolTreeNode node in nodes)
            {
                if (node.tag.Equals("message"))
                {
                    ProtocolTreeNode body = node.GetChild("body");
                    ProtocolTreeNode paused = node.GetChild("paused");
                    ProtocolTreeNode composing = node.GetChild("composing");
                    string jid = node.GetAttribute("from");
                    if (body != null)
                    {
                        //extract and save nickname
                        if (node.GetChild("notify") != null && node.GetChild("notify").GetAttribute("name") != null)
                        {
                            string nick = node.GetChild("notify").GetAttribute("name");
                            Contact c = ContactStore.GetContactByJid(jid);
                            if (c != null)
                            {
                                c.nickname = nick;
                                ContactStore.UpdateNickname(c);
                            }
                        }

                        try
                        {
                            getChat(jid, true).AddMessage(node);
                        }
                        catch (Exception ex)
                        { }
                    }
                    if (paused != null)
                    {
                        try
                        {
                            getChat(jid, false).SetOnline();
                        }
                        catch (Exception e) { }
                    }
                    if (composing != null)
                    {
                        try
                        {
                            getChat(jid, false).SetTyping();
                        }
                        catch (Exception e) { }
                    }
                }
                else if (node.tag.Equals("presence"))
                {
                    string jid = node.GetAttribute("from");
                    if (node.GetAttribute("type") != null && node.GetAttribute("type").Equals("available"))
                    {
                        try
                        {
                            getChat(jid, false).SetOnline();
                        }
                        catch (Exception e) { }
                    }
                    if (node.GetAttribute("type") != null && node.GetAttribute("type").Equals("unavailable"))
                    {
                        try
                        {
                            getChat(jid, false).SetUnavailable();
                        }
                        catch (Exception e) { }
                    }
                }
                else if (node.tag.Equals("iq"))
                {
                    string jid = node.GetAttribute("from");

                    if (node.children.First().tag.Equals("query"))
                    {
                        DateTime lastseen = DateTime.Now;
                        int seconds = Int32.Parse(node.GetChild("query").GetAttribute("seconds"));
                        lastseen = lastseen.Subtract(new TimeSpan(0, 0, seconds));
                        try
                        {
                            getChat(jid, false).SetLastSeen(lastseen);
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    else if (node.children.First().tag.Equals("group"))
                    {
                        string subject = node.children.First().GetAttribute("subject");
                        Contact cont = ContactStore.GetContactByJid(jid);
                        if (cont != null)
                        {
                            cont.nickname = subject;
                            ContactStore.UpdateNickname(cont);
                        }
                        else
                        {

                        }
                    }
                    else if (node.children.First().tag.Equals("picture"))
                    {
                        string pjid = node.GetAttribute("from");
                        byte[] rawpicture = node.GetChild("picture").GetData();
                        Contact c = ContactStore.GetContactByJid(pjid);

                        Image img = null;
                        using (var ms = new MemoryStream(rawpicture))
                        {
                            try
                            {
                                img = Image.FromStream(ms);
                                string targetdir = Directory.GetCurrentDirectory() + "\\data\\profilecache";
                                if(!Directory.Exists(targetdir))
                                {
                                    Directory.CreateDirectory(targetdir);
                                }
                                img.Save(targetdir + "\\" + c.jid + ".jpg");
                                this.getChat(pjid, false).SetPicture(img);
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
            }
        }

        protected void Listen()
        {
            while (true)
            {
                try
                {
                    if (!WappSocket.Instance.HasMessages())
                    {
                        try
                        {
                            WappSocket.Instance.PollMessages();
                        }
                        catch (Exception ex)
                        {
                            if (ex.GetType().Name == "ConnectionException")
                            {
                                //throw new Exception("Socket timed out. Reconnect please. (TO BE IMPLEMENTED)");
                                WappSocket.Instance.Disconnect();
                                WappSocket.Instance.Connect();
                                WappSocket.Instance.Login();
                            }
                        }
                        Thread.Sleep(100);
                    }
                    else
                    {
                        ProtocolTreeNode[] nodes = WappSocket.Instance.GetAllMessages();
                        this.ProcessMessages(nodes);
                    }
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }

        private void ContactsList_Load(object sender, EventArgs e)
        {
            string nickname = "WhatsAPINet";
            this.username = System.Configuration.ConfigurationManager.AppSettings.Get("Username");
            this.password = System.Configuration.ConfigurationManager.AppSettings.Get("Password");

            if (string.IsNullOrEmpty(this.username) || string.IsNullOrEmpty(this.password))
            {
                throw new Exception("Please enter credentials!");
            }
            ContactStore.CheckTable();
            MessageStore.CheckTable();


            Thread t = new Thread(new ThreadStart(SyncWaContactsAsync));
            t.IsBackground = true;
            t.Start();

            this.listBox1.DoubleClick += new EventHandler(listBox1_DoubleClick);
            
            this.listBox1.DataSource = this.contacts;

            WappSocket.Create(this.username, this.password, nickname, true);
            WappSocket.Instance.Connect();
            WappSocket.Instance.Login();
            WappSocket.Instance.sendNickname(nickname);

            Thread listener = new Thread(new ThreadStart(Listen));
            listener.IsBackground = true;
            listener.Start();
        }

        private void btnGoogle_Click(object sender, EventArgs e)
        {
            //reset
            this.contacts.Clear();
            this.label1.Text = "Updating contacts...";
            this.label1.Show();

            //sync contacts
            Dialogs.frmGoogleSync gsync = new Dialogs.frmGoogleSync();
            gsync.ShowDialog(this);

            Thread t = new Thread(new ThreadStart(SyncWaContactsAsync));
            t.IsBackground = true;
            t.Start();
        }
    }
}
