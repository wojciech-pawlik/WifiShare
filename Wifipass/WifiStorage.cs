using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Collections.ObjectModel;

namespace Wifipass
{
    public class WifiStorage
    {
        private static readonly string TEMPORARY_FOLDER = "credentials";

        public List<string> Names { get; set; }
        public List<string> Passwords { get; set; }

        public WifiStorage()
        {
            Names = new List<string>();
            Passwords = new List<string>();
        }

        public void GetNamesAndPasswords()
        {
            // 1. create temporary folder(name, current directory -> if not exists, create)
            Directory.CreateDirectory(TEMPORARY_FOLDER);

            // 2. create XML files of actual wifi saved content (with names ['name'] and passwords ['keyMaterial'])
            string showProfile = "netsh wlan export profile folder=" + Directory.GetCurrentDirectory() + "\\" + TEMPORARY_FOLDER + " key=clear";
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(showProfile);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());

            // 3. load values from XML files to arrays
            var files = Directory.GetFiles(TEMPORARY_FOLDER, "*.xml", SearchOption.AllDirectories);

            Names = new List<string>();
            Passwords = new List<string>();
            foreach (var file in files)
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(file);
                Names.Add(xmlDocument.GetElementsByTagName("name")[0].InnerText);
                Passwords.Add(xmlDocument.GetElementsByTagName("keyMaterial")[0].InnerText);
            }

            // 4. delete temporary folder
            Directory.Delete(TEMPORARY_FOLDER, true);
            cmd.Close();
        }
    }

    public class WifiEntity
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
