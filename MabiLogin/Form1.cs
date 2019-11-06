using BeanfunLogin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MabiLogin
{
    public partial class Form1 : Form
    {
        private MabiVersion mabiVersion;
        private MabiEXE mabiEXE;
        private LoginMethod loginMethod;
        private BeanfunLogin.BeanfunLogin _BfLogin;
        private BeanfunAccount beanfunAccount;
        private Dictionary<string, BeanfunLogin.BeanfunLogin.GameAccount> accountList;

        private void SetLoginMethod(LoginMethod method)
        {
            loginMethod = method;
            一般登入ToolStripMenuItem.BackColor = (method == LoginMethod.General) ? Color.SkyBlue : SystemColors.Control;
            qRCodeToolStripMenuItem.BackColor = (method == LoginMethod.QRCode) ? Color.SkyBlue : SystemColors.Control;

            btn_login.Enabled = (method != LoginMethod.QRCode);

            textBox_beanfunPassword.MaxLength = 100;
            textBox_beanfunPassword.Text = "";
        }

        public Form1()
        {
            InitializeComponent();
        }

        // ----------- GUI Function ----------- //
        private void Form1_Load(object sender, EventArgs e)
        {
            mabiVersion = new MabiVersion();
            mabiEXE = new MabiEXE(mabiVersion);
            SetLoginMethod(LoginMethod.General);

            launchExeToolStripMenuItem.DropDownItems.Add("Client").Click += LaunchClient_Click; ;
            launchExeToolStripMenuItem.DropDownItems.Add("Mabinogi").Click += LaunchMabinogi_Click;
            launchExeToolStripMenuItem.DropDownItems.Add("Open Directory").Click += OpenMabiDir_Click;

            beanfunAccount = new BeanfunAccount();
            LoadAccount();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            listView1.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_BfLogin != null)
            {
                _BfLogin.Dispose();
                _BfLogin = null;
            }
        }

        private void OpenMabiDir_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(mabiVersion.MabiDir))
                Process.Start(mabiVersion.MabiDir);
        }

        private void LaunchClient_Click(object sender, EventArgs e)
        {
            if (File.Exists(mabiVersion.MabiDir + "\\" + "Client.exe"))
                mabiEXE.StartClient(false);
        }

        private void LaunchMabinogi_Click(object sender, EventArgs e)
        {
            if (!File.Exists(mabiVersion.MabiDir + "\\" + "mabinogi.exe"))
                return;
            try
            {
                Process myProcess = Process.Start(
                new ProcessStartInfo
                {
                    WorkingDirectory = mabiVersion.MabiDir,
                    FileName = "mabinogi.exe"
                });
            }
            catch { }
        }

        private void 一般登入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLoginMethod(LoginMethod.General);
        }

        private void qRCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetLoginMethod(LoginMethod.QRCode);
            btn_login_ClickAsync(null, null);
        }

		public async void QuickStart(string username, string password, string accountName)
		{
			try
			{
				Enabled = false;
				var bf = new BeanfunLogin.BeanfunLogin();
				await bf.Login(username, password, LoginMethod.General);
				if (bf.accountList.Count > 0)
				{
					var account = bf.accountList.Find((a) => a.sacc == accountName);
					if (account == null) account = bf.accountList[0];
					var gamePassword = await bf.GetGamePasswordAsync(account);
					mabiEXE.StartClient(true, account.sacc, gamePassword);
				}
			}
			finally
			{
				Application.Exit();
			}
		}

        private async void btn_login_ClickAsync(object sender, EventArgs e)
        {
            if (accountList == null) accountList = new Dictionary<string, BeanfunLogin.BeanfunLogin.GameAccount>();
            accountList.Clear();
            listView1.Items.Clear();
            panel1.Enabled = false;

            if (_BfLogin != null)
            {
                _BfLogin.Dispose();
                _BfLogin = null;
            }
            _BfLogin = new BeanfunLogin.BeanfunLogin();
            _BfLogin.OnLoginCompleted += (s, args) =>
                {
                    if (checkBox_RememberAccount.Checked)
                        SaveAccount();

                    var login = s as BeanfunLogin.BeanfunLogin;
                    foreach (var account in login.accountList)
                    {
                        string[] row = { System.Net.WebUtility.HtmlDecode(account.sname), account.sacc };
                        var listViewItem = new ListViewItem(row);
                        listView1.Items.Add(listViewItem);
                        accountList.Add(account.sacc, account);
                        listView1.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.HeaderSize);
                    }

                    panel1.Enabled = true;
                    listView1.Focus();
                    listView1.Items[0].Selected = true;
                };
            try
            {
                await _BfLogin.Login(textBox_beanfunAccount.Text, textBox_beanfunPassword.Text, loginMethod);
            }
            catch (Exception error)
            {
                MessageBox.Show("登入失敗！ \n" + error.Message);
            }
            finally
            {
                panel1.Enabled = true;
            }
        }

        private void btn_launch_Click(object sender, EventArgs e)
        {
            mabiEXE.StartClient(true, textBox_gameAccount.Text, textBox_gamePassword.Text);
            btn_launch.Enabled = false;
        }

        private void btn_exeInfo_Click(object sender, EventArgs e)
        {
            if (mabiEXE.ExtractMabinogiProcess(out string account, out string password))
            {
                textBox_gameAccount.Text = account;
                textBox_gamePassword.Text = password;
                btn_launch.Enabled = true;
            }
            else
            {
                textBox_gameAccount.Text = "null";
                textBox_gamePassword.Text = "null";
                btn_launch.Enabled = false;
            }
        }

        private void textBox_beanfunAccount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SendKeys.Send("{TAB}");
        }

        private void textBox_beanfunPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
                btn_login.PerformClick();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            loginGameAsync();
        }

        private void listView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                loginGameAsync();
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (listView1.Sorting != SortOrder.Ascending)
            {
                listView1.Sorting = SortOrder.Ascending;
                listView1.Sort();
            }
            else
            {
                listView1.Sorting = SortOrder.None;
                listView1.Items.Clear();
                foreach (var account in accountList.Values)
                {
                    string[] row = { System.Net.WebUtility.HtmlDecode(account.sname), account.sacc };
                    var listViewItem = new ListViewItem(row);
                    listView1.Items.Add(listViewItem);
                }
            }
        }

        // ----------- GUI Function End----------- //

        private async void loginGameAsync()
        {
            if (listView1.SelectedItems.Count != 1) return;
            panel1.Enabled = false;

            var sacc = listView1.SelectedItems[0].SubItems[1].Text;
            if (accountList != null && accountList.ContainsKey(sacc))
            {
                try
                {
                    var account = accountList[sacc];
                    textBox_gameAccount.Text = account.sacc;
                    textBox_gamePassword.Text = await _BfLogin.GetGamePasswordAsync(account);
                }
                catch (Exception error)
                {
                    MessageBox.Show("獲取遊戲密碼失敗！ \n" + error.Message);
                }
                finally
                {
                    panel1.Enabled = true;
                    btn_launch.Enabled = !string.IsNullOrEmpty(textBox_gamePassword.Text);
                }
            }
            if (checkBox_clipboard.Checked)
                Clipboard.SetDataObject(textBox_gamePassword.Text, false, 5, 200);

            listView1.Focus();
        }

        private void SaveAccount()
        {
            if (beanfunAccount == null || beanfunAccount.list == null) return;
            beanfunAccount.AddAccount(new AccountInfo()
            {
                username = textBox_beanfunAccount.Text,
                password = textBox_beanfunPassword.Text,
                loginMethod = loginMethod
            });
            try
            {
                beanfunAccount.SaveAccount();
                LoadAccount();
            }
            catch (Exception error)
            {
                MessageBox.Show("SaveAccount Failed: \n" + error.Message);
            }
        }

        private void LoadAccount()
        {
            if (beanfunAccount == null) return;
            try
            {
                beanfunAccount.LoadAccount();
                if (beanfunAccount.list == null) return;
                一般登入ToolStripMenuItem.DropDownItems.Clear();
                playSafeToolStripMenuItem.DropDownItems.Clear();
                qRCodeToolStripMenuItem.DropDownItems.Clear();
                foreach (var info in beanfunAccount.list)
                {
                    if (info.loginMethod == LoginMethod.General)
                        一般登入ToolStripMenuItem.DropDownItems.Add(info.username.ToString())
                            .Click += (s, args) => AccountEnter(s, args, info);
                    else if (info.loginMethod == LoginMethod.QRCode)
                        qRCodeToolStripMenuItem.DropDownItems.Add(info.username.ToString())
                            .Click += (s, args) => AccountEnter(s, args, info);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("LoadAccount Failed: \n" + error.Message);
            }
        }

        private void AccountEnter(object sender, EventArgs e, AccountInfo info)
        {
            SetLoginMethod(info.loginMethod);
            textBox_beanfunAccount.Text = info.username.ToString();
            textBox_beanfunPassword.Text = info.password.ToString();
        }
    }
}
