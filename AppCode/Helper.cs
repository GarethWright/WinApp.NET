using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WinAppNET.AppCode
{
    class Helper
    {
        public static void CreateFolderTree()
        {
            string targetdir = Directory.GetCurrentDirectory() + "\\data";
            if (!Directory.Exists(targetdir))
            {
                Directory.CreateDirectory(targetdir);
            }
            string foo = targetdir + "\\profilecache";
            if (!Directory.Exists(foo))
            {
                Directory.CreateDirectory(foo);
            }
            foo = targetdir + "\\sqlite";
            if (!Directory.Exists(foo))
            {
                Directory.CreateDirectory(foo);
            }
        }
    }
}
