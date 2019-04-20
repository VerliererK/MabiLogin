namespace MabiLogin
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.一般登入ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playSafeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.qRCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.launchExeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btn_exeInfo = new System.Windows.Forms.Button();
            this.btn_launch = new System.Windows.Forms.Button();
            this.textBox_gameAccount = new System.Windows.Forms.TextBox();
            this.textBox_gamePassword = new System.Windows.Forms.TextBox();
            this.checkBox_clipboard = new System.Windows.Forms.CheckBox();
            this.label_account = new System.Windows.Forms.Label();
            this.label_password = new System.Windows.Forms.Label();
            this.textBox_beanfunAccount = new System.Windows.Forms.TextBox();
            this.textBox_beanfunPassword = new System.Windows.Forms.TextBox();
            this.btn_login = new System.Windows.Forms.Button();
            this.checkBox_RememberAccount = new System.Windows.Forms.CheckBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.AccountName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Account = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.一般登入ToolStripMenuItem,
            this.playSafeToolStripMenuItem,
            this.qRCodeToolStripMenuItem,
            this.launchExeToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(622, 38);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 一般登入ToolStripMenuItem
            // 
            this.一般登入ToolStripMenuItem.Name = "一般登入ToolStripMenuItem";
            this.一般登入ToolStripMenuItem.Size = new System.Drawing.Size(121, 34);
            this.一般登入ToolStripMenuItem.Text = "一般登入";
            this.一般登入ToolStripMenuItem.Click += new System.EventHandler(this.一般登入ToolStripMenuItem_Click);
            // 
            // playSafeToolStripMenuItem
            // 
            this.playSafeToolStripMenuItem.Name = "playSafeToolStripMenuItem";
            this.playSafeToolStripMenuItem.Size = new System.Drawing.Size(119, 34);
            this.playSafeToolStripMenuItem.Text = "PlaySafe";
            this.playSafeToolStripMenuItem.Click += new System.EventHandler(this.playSafeToolStripMenuItem_Click);
            // 
            // qRCodeToolStripMenuItem
            // 
            this.qRCodeToolStripMenuItem.Name = "qRCodeToolStripMenuItem";
            this.qRCodeToolStripMenuItem.Size = new System.Drawing.Size(120, 34);
            this.qRCodeToolStripMenuItem.Text = "QRCode";
            this.qRCodeToolStripMenuItem.Click += new System.EventHandler(this.qRCodeToolStripMenuItem_Click);
            // 
            // launchExeToolStripMenuItem
            // 
            this.launchExeToolStripMenuItem.Name = "launchExeToolStripMenuItem";
            this.launchExeToolStripMenuItem.Size = new System.Drawing.Size(148, 34);
            this.launchExeToolStripMenuItem.Text = "LaunchEXE";
            // 
            // btn_exeInfo
            // 
            this.btn_exeInfo.Location = new System.Drawing.Point(15, 60);
            this.btn_exeInfo.Name = "btn_exeInfo";
            this.btn_exeInfo.Size = new System.Drawing.Size(200, 35);
            this.btn_exeInfo.TabIndex = 0;
            this.btn_exeInfo.Text = "Account Info";
            this.btn_exeInfo.UseVisualStyleBackColor = true;
            this.btn_exeInfo.Click += new System.EventHandler(this.btn_exeInfo_Click);
            // 
            // btn_launch
            // 
            this.btn_launch.Enabled = false;
            this.btn_launch.Location = new System.Drawing.Point(15, 220);
            this.btn_launch.Name = "btn_launch";
            this.btn_launch.Size = new System.Drawing.Size(200, 35);
            this.btn_launch.TabIndex = 3;
            this.btn_launch.Text = "Start Game";
            this.btn_launch.UseVisualStyleBackColor = true;
            this.btn_launch.Click += new System.EventHandler(this.btn_launch_Click);
            // 
            // textBox_gameAccount
            // 
            this.textBox_gameAccount.Location = new System.Drawing.Point(15, 120);
            this.textBox_gameAccount.Name = "textBox_gameAccount";
            this.textBox_gameAccount.ReadOnly = true;
            this.textBox_gameAccount.Size = new System.Drawing.Size(200, 38);
            this.textBox_gameAccount.TabIndex = 4;
            // 
            // textBox_gamePassword
            // 
            this.textBox_gamePassword.Location = new System.Drawing.Point(15, 170);
            this.textBox_gamePassword.Name = "textBox_gamePassword";
            this.textBox_gamePassword.ReadOnly = true;
            this.textBox_gamePassword.Size = new System.Drawing.Size(200, 38);
            this.textBox_gamePassword.TabIndex = 5;
            // 
            // checkBox_clipboard
            // 
            this.checkBox_clipboard.AutoSize = true;
            this.checkBox_clipboard.Location = new System.Drawing.Point(15, 280);
            this.checkBox_clipboard.Name = "checkBox_clipboard";
            this.checkBox_clipboard.Size = new System.Drawing.Size(201, 34);
            this.checkBox_clipboard.TabIndex = 6;
            this.checkBox_clipboard.Text = "AutoClipboard";
            this.checkBox_clipboard.UseVisualStyleBackColor = true;
            // 
            // label_account
            // 
            this.label_account.AutoSize = true;
            this.label_account.Location = new System.Drawing.Point(230, 60);
            this.label_account.Name = "label_account";
            this.label_account.Size = new System.Drawing.Size(162, 30);
            this.label_account.TabIndex = 7;
            this.label_account.Text = "Beanfun 帳號";
            // 
            // label_password
            // 
            this.label_password.AutoSize = true;
            this.label_password.Location = new System.Drawing.Point(230, 110);
            this.label_password.Name = "label_password";
            this.label_password.Size = new System.Drawing.Size(162, 30);
            this.label_password.TabIndex = 8;
            this.label_password.Text = "Beanfun 密碼";
            // 
            // textBox_beanfunAccount
            // 
            this.textBox_beanfunAccount.Location = new System.Drawing.Point(398, 57);
            this.textBox_beanfunAccount.Name = "textBox_beanfunAccount";
            this.textBox_beanfunAccount.Size = new System.Drawing.Size(200, 38);
            this.textBox_beanfunAccount.TabIndex = 9;
            this.textBox_beanfunAccount.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_beanfunAccount_KeyDown);
            // 
            // textBox_beanfunPassword
            // 
            this.textBox_beanfunPassword.Location = new System.Drawing.Point(398, 107);
            this.textBox_beanfunPassword.Name = "textBox_beanfunPassword";
            this.textBox_beanfunPassword.PasswordChar = '*';
            this.textBox_beanfunPassword.Size = new System.Drawing.Size(200, 38);
            this.textBox_beanfunPassword.TabIndex = 10;
            this.textBox_beanfunPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_beanfunPassword_KeyDown);
            // 
            // btn_login
            // 
            this.btn_login.Location = new System.Drawing.Point(235, 153);
            this.btn_login.Name = "btn_login";
            this.btn_login.Size = new System.Drawing.Size(157, 35);
            this.btn_login.TabIndex = 11;
            this.btn_login.Text = "登入";
            this.btn_login.UseVisualStyleBackColor = true;
            this.btn_login.Click += new System.EventHandler(this.btn_login_ClickAsync);
            // 
            // checkBox_RememberAccount
            // 
            this.checkBox_RememberAccount.AutoSize = true;
            this.checkBox_RememberAccount.Location = new System.Drawing.Point(419, 154);
            this.checkBox_RememberAccount.Name = "checkBox_RememberAccount";
            this.checkBox_RememberAccount.Size = new System.Drawing.Size(179, 34);
            this.checkBox_RememberAccount.TabIndex = 12;
            this.checkBox_RememberAccount.Text = "記住帳號密碼";
            this.checkBox_RememberAccount.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.AccountName,
            this.Account});
            this.listView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(235, 194);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(375, 224);
            this.listView1.TabIndex = 13;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.listView1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listView1_KeyUp);
            // 
            // AccountName
            // 
            this.AccountName.Text = "帳號名稱";
            this.AccountName.Width = 158;
            // 
            // Account
            // 
            this.Account.Text = "遊戲帳號";
            this.Account.Width = 128;
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.listView1);
            this.panel1.Controls.Add(this.checkBox_RememberAccount);
            this.panel1.Controls.Add(this.btn_login);
            this.panel1.Controls.Add(this.textBox_beanfunAccount);
            this.panel1.Controls.Add(this.textBox_beanfunPassword);
            this.panel1.Controls.Add(this.label_password);
            this.panel1.Controls.Add(this.label_account);
            this.panel1.Controls.Add(this.checkBox_clipboard);
            this.panel1.Controls.Add(this.textBox_gamePassword);
            this.panel1.Controls.Add(this.textBox_gameAccount);
            this.panel1.Controls.Add(this.btn_exeInfo);
            this.panel1.Controls.Add(this.btn_launch);
            this.panel1.Controls.Add(this.menuStrip1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(622, 430);
            this.panel1.TabIndex = 14;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(622, 430);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("微軟正黑體", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "Form1";
            this.Text = "MabiLogin";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem playSafeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem launchExeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 一般登入ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem qRCodeToolStripMenuItem;
        private System.Windows.Forms.Button btn_exeInfo;
        private System.Windows.Forms.Button btn_launch;
        private System.Windows.Forms.TextBox textBox_gameAccount;
        private System.Windows.Forms.TextBox textBox_gamePassword;
        private System.Windows.Forms.CheckBox checkBox_clipboard;
        private System.Windows.Forms.Label label_account;
        private System.Windows.Forms.Label label_password;
        private System.Windows.Forms.TextBox textBox_beanfunAccount;
        private System.Windows.Forms.TextBox textBox_beanfunPassword;
        private System.Windows.Forms.Button btn_login;
        private System.Windows.Forms.CheckBox checkBox_RememberAccount;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader AccountName;
        private System.Windows.Forms.ColumnHeader Account;
        private System.Windows.Forms.Panel panel1;
    }
}

