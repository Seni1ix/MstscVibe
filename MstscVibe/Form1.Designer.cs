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
			btnConnect = new Button();
			btnOptions = new Button();
			grpLogon = new GroupBox();
			menuStrip = new MenuStrip();
			fileMenu = new ToolStripMenuItem();
			openRdpMenuItem = new ToolStripMenuItem();
			exitMenuItem = new ToolStripMenuItem();

			// menuStrip
			menuStrip.Items.Add(fileMenu);
			menuStrip.Location = new Point(0, 0);
			menuStrip.Dock = DockStyle.Top;

			// fileMenu
			fullScreenMenuItem = new ToolStripMenuItem();

			fileMenu.Text = "&File";
			fileMenu.DropDownItems.AddRange([openRdpMenuItem, new ToolStripSeparator(), fullScreenMenuItem, new ToolStripSeparator(), exitMenuItem]);

			// fullScreenMenuItem
			fullScreenMenuItem.Text = "Connect &Full Screen";
			fullScreenMenuItem.ShortcutKeys = Keys.F11;
			fullScreenMenuItem.Click += FullScreenMenuItem_Click;

			// openRdpMenuItem
			openRdpMenuItem.Text = "&Open RDP File...";
			openRdpMenuItem.ShortcutKeys = Keys.Control | Keys.O;
			openRdpMenuItem.Click += OpenRdpMenuItem_Click;

			// exitMenuItem
			exitMenuItem.Text = "E&xit";
			exitMenuItem.Click += (s, e) => Close();

			// lblComputer
			lblComputer.Text = "Computer:";
			lblComputer.Location = new Point(16, 28);
			lblComputer.AutoSize = true;

			// txtComputer
			txtComputer.Location = new Point(120, 25);
			txtComputer.Size = new Size(220, 23);

			// lblUsername
			lblUsername.Text = "User name:";
			lblUsername.Location = new Point(16, 60);
			lblUsername.AutoSize = true;

			// txtUsername
			txtUsername.Location = new Point(120, 57);
			txtUsername.Size = new Size(220, 23);

			// lblPassword
			lblPassword = new Label();
			lblPassword.Text = "Password:";
			lblPassword.Location = new Point(16, 92);
			lblPassword.AutoSize = true;

			// txtPassword
			txtPassword = new TextBox();
			txtPassword.Location = new Point(120, 89);
			txtPassword.Size = new Size(220, 23);
			txtPassword.UseSystemPasswordChar = true;

			// chkRememberPassword
			chkRememberPassword = new CheckBox();
			chkRememberPassword.Text = "Remember password";
			chkRememberPassword.Location = new Point(120, 118);
			chkRememberPassword.AutoSize = true;

			// grpLogon
			grpLogon.Text = "Logon settings";
			grpLogon.Location = new Point(12, 32);
			grpLogon.Size = new Size(360, 150);
			grpLogon.Controls.AddRange([lblComputer, txtComputer, lblUsername, txtUsername, lblPassword, txtPassword, chkRememberPassword]);

			// btnOptions
			btnOptions.Text = "Options...";
			btnOptions.Location = new Point(12, 195);
			btnOptions.Size = new Size(100, 30);
			btnOptions.Click += BtnOptions_Click;

			// btnConnect
			btnConnect.Text = "Connect";
			btnConnect.Location = new Point(270, 195);
			btnConnect.Size = new Size(100, 30);
			btnConnect.Click += BtnConnect_Click;

			// Form1
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(388, 238);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MstscVibe - Remote Desktop Connection";
			MainMenuStrip = menuStrip;
			Controls.AddRange([menuStrip, grpLogon, btnOptions, btnConnect]);
			AcceptButton = btnConnect;
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
		private CheckBox chkRememberPassword;
		private MenuStrip menuStrip;
		private ToolStripMenuItem fileMenu;
		private ToolStripMenuItem openRdpMenuItem;
		private ToolStripMenuItem fullScreenMenuItem;
		private ToolStripMenuItem exitMenuItem;
	}
}
