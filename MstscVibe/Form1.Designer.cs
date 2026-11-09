namespace MstscVibe {
	partial class Form1 {
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent() {
			lblComputer = new Label();
			txtComputer = new ComboBox();
			lblUsername = new Label();
			txtUsername = new TextBox();
			lblPassword = new Label();
			txtPassword = new TextBox();
			btnRevealPassword = new Button();
			chkRememberPassword = new CheckBox();
			btnConnect = new Button();
			btnOptions = new Button();
			grpLogon = new GroupBox();
			menuStrip = new MenuStrip();
			fileMenu = new ToolStripMenuItem();
			openRdpMenuItem = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			fullScreenMenuItem = new ToolStripMenuItem();
			toolStripSeparator2 = new ToolStripSeparator();
			exitMenuItem = new ToolStripMenuItem();
			grpLogon.SuspendLayout();
			menuStrip.SuspendLayout();
			SuspendLayout();
			//
			// lblComputer
			//
			lblComputer.AutoSize = true;
			lblComputer.Location = new Point(16, 28);
			lblComputer.Name = "lblComputer";
			lblComputer.Text = "Computer:";
			//
			// txtComputer
			//
			txtComputer.Location = new Point(120, 25);
			txtComputer.Name = "txtComputer";
			txtComputer.Size = new Size(220, 23);
			txtComputer.TabIndex = 0;
			//
			// lblUsername
			//
			lblUsername.AutoSize = true;
			lblUsername.Location = new Point(16, 60);
			lblUsername.Name = "lblUsername";
			lblUsername.Text = "User name:";
			//
			// txtUsername
			//
			txtUsername.Location = new Point(120, 57);
			txtUsername.Name = "txtUsername";
			txtUsername.Size = new Size(220, 23);
			txtUsername.TabIndex = 1;
			//
			// lblPassword
			//
			lblPassword.AutoSize = true;
			lblPassword.Location = new Point(16, 92);
			lblPassword.Name = "lblPassword";
			lblPassword.Text = "Password:";
			//
			// txtPassword
			//
			txtPassword.Location = new Point(120, 89);
			txtPassword.Name = "txtPassword";
			txtPassword.Size = new Size(194, 23);
			txtPassword.TabIndex = 2;
			txtPassword.UseSystemPasswordChar = true;
			//
			// btnRevealPassword (press and hold to show the password)
			//
			btnRevealPassword.Cursor = Cursors.Hand;
			btnRevealPassword.FlatAppearance.BorderSize = 0;
			btnRevealPassword.FlatStyle = FlatStyle.Flat;
			btnRevealPassword.Location = new Point(316, 89);
			btnRevealPassword.Name = "btnRevealPassword";
			btnRevealPassword.Size = new Size(24, 23);
			btnRevealPassword.TabStop = false;
			btnRevealPassword.MouseCaptureChanged += BtnRevealPassword_MouseCaptureChanged;
			btnRevealPassword.MouseDown += BtnRevealPassword_MouseDown;
			btnRevealPassword.MouseUp += BtnRevealPassword_MouseUp;
			//
			// chkRememberPassword
			//
			chkRememberPassword.AutoSize = true;
			chkRememberPassword.Location = new Point(120, 118);
			chkRememberPassword.Name = "chkRememberPassword";
			chkRememberPassword.TabIndex = 3;
			chkRememberPassword.Text = "Remember password";
			//
			// grpLogon
			//
			grpLogon.Controls.Add(lblComputer);
			grpLogon.Controls.Add(txtComputer);
			grpLogon.Controls.Add(lblUsername);
			grpLogon.Controls.Add(txtUsername);
			grpLogon.Controls.Add(lblPassword);
			grpLogon.Controls.Add(txtPassword);
			grpLogon.Controls.Add(btnRevealPassword);
			grpLogon.Controls.Add(chkRememberPassword);
			grpLogon.Location = new Point(12, 32);
			grpLogon.Name = "grpLogon";
			grpLogon.Size = new Size(360, 150);
			grpLogon.TabIndex = 1;
			grpLogon.TabStop = false;
			grpLogon.Text = "Logon settings";
			//
			// btnOptions
			//
			btnOptions.Location = new Point(12, 195);
			btnOptions.Name = "btnOptions";
			btnOptions.Size = new Size(100, 30);
			btnOptions.TabIndex = 2;
			btnOptions.Text = "Options...";
			btnOptions.Click += BtnOptions_Click;
			//
			// btnConnect
			//
			btnConnect.Location = new Point(270, 195);
			btnConnect.Name = "btnConnect";
			btnConnect.Size = new Size(100, 30);
			btnConnect.TabIndex = 3;
			btnConnect.Text = "Connect";
			btnConnect.Click += BtnConnect_Click;
			//
			// menuStrip
			//
			menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu });
			menuStrip.Location = new Point(0, 0);
			menuStrip.Name = "menuStrip";
			menuStrip.TabIndex = 0;
			//
			// fileMenu
			//
			fileMenu.DropDownItems.AddRange(new ToolStripItem[] { openRdpMenuItem, toolStripSeparator1, fullScreenMenuItem, toolStripSeparator2, exitMenuItem });
			fileMenu.Name = "fileMenu";
			fileMenu.Text = "&File";
			//
			// openRdpMenuItem
			//
			openRdpMenuItem.Name = "openRdpMenuItem";
			openRdpMenuItem.ShortcutKeys = Keys.Control | Keys.O;
			openRdpMenuItem.Text = "&Open RDP File...";
			openRdpMenuItem.Click += OpenRdpMenuItem_Click;
			//
			// toolStripSeparator1
			//
			toolStripSeparator1.Name = "toolStripSeparator1";
			//
			// fullScreenMenuItem
			//
			fullScreenMenuItem.Name = "fullScreenMenuItem";
			fullScreenMenuItem.ShortcutKeys = Keys.F11;
			fullScreenMenuItem.Text = "Connect &Full Screen";
			fullScreenMenuItem.Click += FullScreenMenuItem_Click;
			//
			// toolStripSeparator2
			//
			toolStripSeparator2.Name = "toolStripSeparator2";
			//
			// exitMenuItem
			//
			exitMenuItem.Name = "exitMenuItem";
			exitMenuItem.Text = "E&xit";
			exitMenuItem.Click += ExitMenuItem_Click;
			//
			// Form1
			//
			AcceptButton = btnConnect;
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(388, 238);
			Controls.Add(menuStrip);
			Controls.Add(grpLogon);
			Controls.Add(btnOptions);
			Controls.Add(btnConnect);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MainMenuStrip = menuStrip;
			MaximizeBox = false;
			Name = "Form1";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MstscVibe - Remote Desktop Connection";
			grpLogon.ResumeLayout(false);
			grpLogon.PerformLayout();
			menuStrip.ResumeLayout(false);
			menuStrip.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label lblComputer;
		private ComboBox txtComputer;
		private Label lblUsername;
		private TextBox txtUsername;
		private Button btnConnect;
		private Button btnOptions;
		private GroupBox grpLogon;
		private Label lblPassword;
		private TextBox txtPassword;
		private Button btnRevealPassword;
		private CheckBox chkRememberPassword;
		private MenuStrip menuStrip;
		private ToolStripMenuItem fileMenu;
		private ToolStripMenuItem openRdpMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem fullScreenMenuItem;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripMenuItem exitMenuItem;
	}
}
