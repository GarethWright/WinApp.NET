using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinAppNET.AppCode;
using System.Threading;

namespace WinAppNET
{
    public partial class ContactsSelector : Form
    {
        protected Contact[] _initialContacts;
        protected BindingList<Contact> _matchingContacts = new BindingList<Contact>();
        public string SelectedJID = String.Empty;

        public ContactsSelector()
        {
            InitializeComponent();
            this._initialContacts = ContactStore.GetAllContacts();
            foreach(Contact c in this._initialContacts)
            {
                this._matchingContacts.Add(c);
            }
            this.listBox1.DataSource = this._matchingContacts;
            this.listBox1.DoubleClick += new EventHandler(listBox1_DoubleClick);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this._matchingContacts.Clear();
            foreach (Contact contact in this._initialContacts)
            {
                if(contact.ToString().ToLower().Contains(this.textBox1.Text.ToLower()))
                {
                    this._matchingContacts.Add(contact);
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem != null)
            {
                Contact item = (Contact)this.listBox1.SelectedItem;
                this.SelectedJID = item.jid;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void ContactsSelector_Activated(object sender, EventArgs e)
        {
            this.textBox1.Focus();
        }
    }
}
