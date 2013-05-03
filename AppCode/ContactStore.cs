using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using Google.GData.Contacts;
using Google.GData.Extensions;
using WhatsAppApi.Helper;

namespace WinAppNET.AppCode
{
    class ContactStore
    {
        public static Contact GetContactByJid(string jid)
        {
            Contact contact;

            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=contacts.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Contacts where jid = @jid";
                cmd.Parameters.Add(new SQLiteParameter("@jid", jid));
                DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    int id = Int32.Parse(reader["id"].ToString());
                    string status = reader["status"].ToString();
                    string nickname = reader["nickname"].ToString();
                    string given_name = reader["given_name"].ToString();
                    string family_name = reader["family_name"].ToString();
                    contact = new Contact(id, jid, status, nickname, given_name, family_name);
                    return contact;
                }
                else
                {
                    return null;
                    //contact = new Contact(-1, jid, "", "", "", "");
                    //AddContact(contact);
                    //return contact;
                }
            }
        }

        public static void SyncWaContacts(string username, string password)
        {
            Contact[] contacts = ContactStore.GetAllContacts();
            List<string> input = new List<string>();
            foreach (Contact c in contacts)
            {
                input.Add(c.jid.Split('@').First());
            }
            ContactSync s = new ContactSync(username, password);
            ContactSyncResult[] res = s.Sync(input.ToArray());
            foreach (ContactSyncResult r in res)
            {
                if (r.w == 1)
                {
                    string jid = r.n + "@s.whatsapp.net";
                    Contact con = ContactStore.GetContactByJid(jid);
                    if (con != null && con.status != r.s)
                    {
                        //update status if changed
                        con.status = r.s;
                        ContactStore.UpdateStatus(con);
                    }
                }
                else
                {
                    //delete
                    Contact con = ContactStore.GetContactByJid(r.n + "@s.whatsapp.net");
                    if (con != null)
                    {
                        ContactStore.DeleteContact(con);
                    }
                }
            }
        }

        public static void CheckTable()
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=contacts.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS
'Contacts' (
'id' INTEGER PRIMARY KEY,
'jid' VARCHAR(64),
'status' VARCHAR(256),
'nickname' VARCHAR(128),
'given_name' VARCHAR(64),
'family_name' VARCHAR(64)
)";
                cmd.ExecuteNonQuery();
            }
        }

        public static Contact[] GetAllContacts()
        {
            List<Contact> contacts = new List<Contact>();

            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=contacts.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Contacts order by given_name asc";
                DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Int32.Parse(reader["id"].ToString());
                    string jid = reader["jid"].ToString();
                    string status = reader["status"].ToString();
                    string nickname = reader["nickname"].ToString();
                    string given_name = reader["given_name"].ToString();
                    string family_name = reader["family_name"].ToString();
                    Contact contact = new Contact(id, jid, status, nickname, given_name, family_name);
                    contacts.Add(contact);
                }
            }

            return contacts.ToArray();
        }

        public static void AddContact(Contact contact)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=contacts.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = @"INSERT INTO
'Contacts' 
(
'jid',
'status',
'nickname',
'given_name',
'family_name'
)
VALUES (
'" + contact.jid + @"',
'" + contact.status + @"',
'" + contact.nickname + @"',
'" + contact.given_name + @"',
'" + contact.family_name + @"'
)";
                cmd.ExecuteNonQuery();
            }
        }

        public static void SyncGoogleContacts(string username, string password)
        {
            ContactsService GContactService = new ContactsService("Contact Infomation");
            GContactService.setUserCredentials(username, password);

            ContactsQuery query = new ContactsQuery(ContactsQuery.
            CreateContactsUri("default"));

            ContactsFeed feed = GContactService.Query(query);

            int startIndex = 0;
            while(feed.Entries.Count > 0)
            {
                startIndex += feed.ItemsPerPage;
                query.StartIndex = startIndex;
                PhoneNumbers.PhoneNumberUtil util = PhoneNumbers.PhoneNumberUtil.GetInstance();
                foreach (ContactEntry entry in feed.Entries)
                {
                    if (entry.Phonenumbers.Count > 0)
                    {
                        foreach (PhoneNumber number in entry.Phonenumbers)
                        {
                            string numb = string.Empty;
                            try
                            {
                                PhoneNumbers.PhoneNumber num = util.Parse(number.Value, "NL");
                                numb = num.CountryCode.ToString() + num.NationalNumber.ToString();
                            }
                            catch (PhoneNumbers.NumberParseException e)
                            {
                                Console.WriteLine("NumberParseException was thrown: " + e.Message);
                                continue;
                            }
                            if (!_numberExists(numb + "@s.whatsapp.net"))
                            {
                                Contact contact = new Contact(0, numb + "@s.whatsapp.net", "", "", entry.Name.GivenName, entry.Name.FamilyName);
                                AddContact(contact);
                            }
                        }
                    }
                }
                feed = GContactService.Query(query);
            }
        }

        protected static bool _numberExists(string jid)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=contacts.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Contacts WHERE jid = @jid";
                cmd.Parameters.Add(new SQLiteParameter("@jid", jid));
                DbDataReader res = cmd.ExecuteReader();
                if (res.HasRows)
                    return true;
                else
                    return false;
            }
        }

        public static void UpdateNickname(Contact contact)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=contacts.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = @"UPDATE
'Contacts' 
SET
nickname = @nickname
WHERE jid = @gjid";
                cmd.Parameters.Add(new SQLiteParameter("@nickname", contact.nickname));
                cmd.Parameters.Add(new SQLiteParameter("@gjid", contact.jid));
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateStatus(Contact contact)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=contacts.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = @"UPDATE
'Contacts' 
SET
status = @status
WHERE jid = @gjid";
                cmd.Parameters.Add(new SQLiteParameter("@status", contact.status));
                cmd.Parameters.Add(new SQLiteParameter("@gjid", contact.jid));
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteContact(Contact contact)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=contacts.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = @"DELETE
FROM
'Contacts' 
WHERE jid = @gjid";
                cmd.Parameters.Add(new SQLiteParameter("@gjid", contact.jid));
                cmd.ExecuteNonQuery();
            }
        }
    }
}
