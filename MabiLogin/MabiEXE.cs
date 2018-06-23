using System;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;

namespace MabiLogin
{
    class MabiEXE
    {
        private MabiVersion mabiVersion;

        public MabiEXE(MabiVersion mabiVersion)
        {
            this.mabiVersion = mabiVersion;
        }

        public void StartClient(Boolean userLogin, string gameAccount = "", string gamePassword = "")
        {
            if (!mabiVersion.IsMabiExists()) return;

            ProcessStartInfo _processStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = mabiVersion.MabiDir,
                FileName = "Client.exe"
            };
            String cmdline = @"code:1622 ver:" + mabiVersion.MabiVer + @" logip:210.208.80.6 logport:11000 chatip:210.208.80.10 chatport:8004 setting:""file://data/features.xml=Regular, Taiwan""";
            if (userLogin)
                cmdline = cmdline + " /N:" + gameAccount + " /V:" + gamePassword + " /T:gamania";
            _processStartInfo.Arguments = cmdline;
            Process myProcess = Process.Start(_processStartInfo);
        }

        public bool ExtractMabinogiProcess(out string account, out string password)
        {
            Process[] process = Process.GetProcessesByName("mabinogi");
            account = string.Empty;
            password = string.Empty;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process[0].Id);
                ManagementOperationObserver watcher = new ManagementOperationObserver();
                string commandline;
                searcher.Get(watcher);

                foreach (var @object in searcher.Get())
                {
                    commandline = @object["CommandLine"].ToString();
                    int beg = commandline.IndexOf("/V:");
                    int end = commandline.IndexOf("/T:");
                    password = commandline.Substring(beg + 3, end - beg - 4);

                    beg = commandline.IndexOf("/N:");
                    end = commandline.IndexOf("/V:");
                    account = commandline.Substring(beg + 3, end - beg - 4);

                    Clipboard.SetText(password);
                    process[0].Kill();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
