using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WhatsAppApi.Helper;

namespace WinAppNET.AppCode
{
    public class WappMessage
    {
        public int id;
        public String data;
        public bool from_me;
        public string jid;
        public DateTime timestamp;

        public WappMessage(string message, string jid)
        {
            this.data = message;
            this.timestamp = DateTime.UtcNow;
            this.from_me = true;
            this.jid = jid;
        }

        public WappMessage(int id, string data, bool from_me, string jid, DateTime timestamp)
        {
            this.id = id;
            this.data = data;
            this.from_me = from_me;
            this.jid = jid;
            this.timestamp = timestamp;
        }

        public WappMessage(ProtocolTreeNode node, string jid)
        {
            if (node.tag == "message")
            {
                if (node.GetAttribute("type") == "subject")
                {
                    Contact c = ContactStore.GetContactByJid(node.GetAttribute("author"));
                    this.data = c.ToString() + " changed subject to \"" + Encoding.ASCII.GetString(node.GetChild("body").GetData()) + "\"";
                }
                else
                {
                    this.data = Encoding.ASCII.GetString(node.GetChild("body").GetData());
                }
                this.from_me = false;
                this.timestamp = DateTime.UtcNow;
                this.jid = jid;
            }
        }

        public override String ToString()
        {
            if (this.from_me)
                return "Me: " + this.data;
            else
                return "Him: " + this.data;
        }
    }
}
