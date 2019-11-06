using System;
using System.Windows.Forms;

namespace MabiLogin
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main(string[] _args)
        {
            if (Environment.OSVersion.Version.Major >= 6)
                NativeMethods.SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

			var form = new Form1();

			if (_args.Length > 1)
				form.QuickStart(_args[0], _args[1], _args.Length > 2 ? _args[2] : "");

			Application.Run(form);
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool SetProcessDPIAware();
    }
}
