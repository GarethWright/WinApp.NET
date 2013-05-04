using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAppNET.AppCode
{
    public class Contact
    {
        public int id;
        public string jid;
        public string status;
        public string nickname;
        public string given_name;
        public string family_name;

        public Contact(int id, string jid, string status, string nickname, string given_name, string family_name)
        {
            this.id = id;
            this.jid = jid.Replace("+", "").TrimStart('0');
            this.status = status;
            this.nickname = nickname;
            this.given_name = given_name;
            this.family_name = family_name;
        }

        public override string ToString()
        {
            string ret = "";
            if(!String.IsNullOrEmpty(this.given_name) || !String.IsNullOrEmpty(this.family_name))
            {
                ret =  this.given_name + " " + this.family_name;
            }
            else if(!String.IsNullOrEmpty(this.nickname))
            {
                ret = this.nickname;
            }
            string[] parts = this.jid.Split('@');
            if (!String.IsNullOrEmpty(ret))
            {
                return ret + " (" + parts[0] + ")";
            }
            else
            {
                return parts[0];
            }
        }
    }
}
