using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SQLite;

namespace WinAppNET.AppCode
{
    class MessageStore
    {
        public static void CheckTable()
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=messages.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = @"CREATE TABLE IF NOT EXISTS
'Messages' (
'id' INTEGER PRIMARY KEY,
'jid' VARCHAR(64),
'from_me' INTEGER,
'read' INTEGER,
'data' TEXT,
'timestamp' VARCHAR(64)
)";
                cmd.ExecuteNonQuery();
            }
        }

        public static WappMessage[] GetAllMessagesForContact(string jid)
        {
            List<WappMessage> messages = new List<WappMessage>();

            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=messages.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd = cnn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Messages where jid = @jid";
                cmd.Parameters.Add(new SQLiteParameter("@jid", jid));
                DbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = Int32.Parse(reader["id"].ToString());
                    bool from_me = (Int32.Parse(reader["from_me"].ToString()) == 1 ? true : false);
                    string data = (string)reader["data"];
                    DateTime timestamp = DateTime.Parse(reader["timestamp"].ToString());
                    WappMessage message = new WappMessage(id, data, from_me, jid, timestamp);
                    messages.Add(message);
                }
            }

            return messages.ToArray();
        }

        public static void AddMessage(WappMessage message)
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            using (DbConnection cnn = fact.CreateConnection())
            {
                cnn.ConnectionString = "Data Source=messages.db3";
                cnn.Open();
                DbCommand cmd = cnn.CreateCommand();
                cmd.CommandText = @"INSERT INTO
'Messages' 
(
'jid',
'from_me',
'data',
'timestamp'
)
VALUES (
'" + message.jid + @"',
'" + (message.from_me ? "1" : "0") + @"',
@data,
'" + message.timestamp.ToString() + @"'
)";
                cmd.Parameters.Add(new SQLiteParameter("@data", message.data));
                cmd.ExecuteNonQuery();
            }
        }
    }
}
