using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace MabiLogin
{
    class MabiVersion
    {
        private string mabiDir;
        //電腦上瑪奇的版本
        private uint mabiUserVer = 0;

        public MabiVersion()
        {
            mabiDir = GetMabiDir();
            if (string.IsNullOrEmpty(mabiDir))
                FindMabiDir();

            mabiUserVer = GetMabiUserVer();
        }

        public string MabiDir
        {
            get { return mabiDir; }
        }

        public uint MabiVer
        {
            get { return mabiUserVer; }
        }

        public bool IsMabiExists()
        {
            if (!File.Exists(Path.Combine(mabiDir, "Client.exe")))
            {
                FindMabiDir();
                return false;
            }
            return true;
        }

        private string LoadMabiDirFromConfig()
        {
            try
            {
                if (!File.Exists("app.config"))
                    return null;
                var mabiDir = XDocument.Load("app.config");
                return mabiDir.Root.
                    Element("MabiInfo.Properties.Settings").
                    Element("mabiDir").Value;
            }
            catch
            {
                return null;
            }
        }

        private void SaveMabiDirToConfig()
        {
            XmlTextWriter XTW = new XmlTextWriter("app.config", Encoding.UTF8);
            XTW.WriteStartDocument();
            XTW.WriteStartElement("configuration");
            XTW.WriteStartElement("MabiInfo.Properties.Settings");
            XTW.WriteElementString("mabiDir", mabiDir);
            XTW.WriteElementString("Description", " 瑪奇安裝路徑");
            XTW.WriteEndElement();
            XTW.WriteEndElement();
            XTW.Flush();     //寫這行才會寫入檔案
            XTW.Close();
        }

        private string GetMabiDir()
        {
            string dir = LoadMabiDirFromConfig();
            if (!string.IsNullOrEmpty(dir) && File.Exists(dir + "\\" + "Client.exe"))
                return dir;

            //Get Mabinogi Directory from Registory
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Nexon\Mabinogi", false);
            if (regkey == null)
            {
                regkey = Registry.CurrentUser.OpenSubKey(@"Software\Nexon\Mabinogi_test", false);
                if (regkey == null)
                {
                    Registry.CurrentUser.OpenSubKey(@"Software\Nexon\Mabinogi_hangame", false);
                    if (regkey == null) return "C:\\Nexon\\Mabinogi";
                }
            }
            string reg = (string)regkey.GetValue("");	// Returns Mabinogi Directory
            regkey.Close();
            if (File.Exists(reg + "\\" + "Client.exe"))
                return reg;
            else
                return null;
        }

        private void FindMabiDir()
        {
            DialogResult result1 = MessageBox.Show(
                "沒有發現瑪奇安裝路徑，請問是否自行新增路徑？",
                "無瑪奇執行檔",
                MessageBoxButtons.YesNo);
            if (result1 == DialogResult.Yes)
            {
                FileFolderDialog folderDialog = new FileFolderDialog();
                folderDialog.onlyPath = true;
                if (folderDialog.ShowDialog() == DialogResult.OK)
                    if (File.Exists(folderDialog.SelectedPath + "\\" + "Client.exe"))
                    {
                        mabiDir = folderDialog.SelectedPath;
                        SaveMabiDirToConfig();
                    }
                    else
                        MessageBox.Show("該路徑非瑪奇安裝路徑");
            }
        }

        private uint GetMabiUserVer()
        {
            if (mabiDir == null)
                mabiDir = GetMabiDir();
            // Get Client Version from version.dat
            //string version_dat = MabiDir + "\\version.dat";
            string version_dat = "version.dat";
            if (File.Exists(version_dat))
            {
                byte[] data = File.ReadAllBytes(version_dat);
                return BitConverter.ToUInt32(data, 0);
            }
            else if (File.Exists(mabiDir + "\\" + version_dat))
            {
                byte[] data = File.ReadAllBytes(mabiDir + "\\" + version_dat);
                return BitConverter.ToUInt32(data, 0);
            }
            else
            {
                return 0;
            }
        }
    }
}
