using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WinAppNET.AppCode;
using System.Threading;
using WhatsAppApi.Helper;
using WhatsAppApi;
using System.Text;
using System.Drawing;
using System.IO;

namespace WinAppNET
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Helper.CreateFolderTree();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ContactsList());
        }
    }
}
