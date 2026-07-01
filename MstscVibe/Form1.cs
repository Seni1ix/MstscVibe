using System.Reflection;

namespace MstscVibe {
	public partial class Form1 : Form {
		private RdpFile? _loadedRdp;
		private readonly UserSettings _settings;

		public Form1() {
			InitializeComponent();
			_settings = UserSettings.Load();
			ApplySettings();
		}

		public Form1(RdpFile rdpFile) : this() {
			_loadedRdp = rdpFile;
			ApplyRdpFile(rdpFile);

			Text = $"MstscVibe - {Assembly.GetExecutingAssembly().GetName().Version}";
        }

		private void ApplySettings() {
			txtComputer.Text = _settings.LastComputer;
			txtUsername.Text = _settings.LastUsername;
			chkRememberPassword.Checked = _settings.RememberPassword;
			if (_settings.RememberPassword)
				txtPassword.Text = _settings.GetPassword();
			// Apply saved options to a default RdpFile so they carry through
			var rdp = new RdpFile();
			_settings.ApplyTo(rdp);
			_loadedRdp = rdp;
		}

		private RdpFile GetCurrentRdp() {
			var rdp = _loadedRdp ?? new RdpFile();
			if (!string.IsNullOrWhiteSpace(txtComputer.Text))
				rdp.FullAddress = txtComputer.Text.Trim();
			if (!string.IsNullOrWhiteSpace(txtUsername.Text))
				rdp.Username = txtUsername.Text.Trim();
			return rdp;
		}

		private void ApplyRdpFile(RdpFile rdp) {
			txtComputer.Text = rdp.FullAddress;
			txtUsername.Text = rdp.Username;
			if (!string.IsNullOrEmpty(rdp.Password))
				txtPassword.Text = rdp.Password;
		}

		private void OpenRdpMenuItem_Click(object? sender, EventArgs e) {
			using var dlg = new OpenFileDialog {
				Filter = "Remote Desktop Files (*.rdp)|*.rdp|All Files (*.*)|*.*",
				Title = "Open RDP File"
			};
			if (dlg.ShowDialog() == DialogResult.OK) {
				try {
					_loadedRdp = RdpFile.Parse(dlg.FileName);
					ApplyRdpFile(_loadedRdp);
				} catch (Exception ex) {
					MessageBox.Show($"Failed to read RDP file:\n{ex.Message}", "Error",
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void BtnOptions_Click(object? sender, EventArgs e) {
			var rdp = GetCurrentRdp();
			using var optionsForm = new OptionsForm(rdp);
			if (optionsForm.ShowDialog(this) == DialogResult.OK) {
				optionsForm.ApplyTo(rdp);
				_loadedRdp = rdp;
				_settings.CopyFrom(rdp);
				_settings.Save();
			}
		}

		private void FullScreenMenuItem_Click(object? sender, EventArgs e) {
			var server = txtComputer.Text.Trim();
			if (string.IsNullOrEmpty(server)) {
				MessageBox.Show("Please enter a computer name or IP address.", "MstscVibe",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var rdp = GetCurrentRdp();
			rdp.Password = txtPassword.Text;
			rdp.ScreenModeId = 2;

			_settings.LastComputer = rdp.FullAddress;
			_settings.LastUsername = rdp.Username;
			_settings.RememberPassword = chkRememberPassword.Checked;
			if (chkRememberPassword.Checked)
				_settings.SetPassword(txtPassword.Text);
			else
				_settings.SetPassword("");
			_settings.CopyFrom(rdp);
			_settings.Save();

			var session = new SessionForm(rdp);
			session.FormClosed += (_, _) => Show();
			Hide();
			session.Show();
		}

		private void BtnConnect_Click(object? sender, EventArgs e) {
			var server = txtComputer.Text.Trim();
			if (string.IsNullOrEmpty(server)) {
				MessageBox.Show("Please enter a computer name or IP address.", "MstscVibe",
					MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			var rdp = GetCurrentRdp();
			rdp.Password = txtPassword.Text;

			_settings.LastComputer = rdp.FullAddress;
			_settings.LastUsername = rdp.Username;
			_settings.RememberPassword = chkRememberPassword.Checked;
			if (chkRememberPassword.Checked)
				_settings.SetPassword(txtPassword.Text);
			else
				_settings.SetPassword("");
			_settings.CopyFrom(rdp);
			_settings.Save();

			var session = new SessionForm(rdp);
			session.FormClosed += (_, _) => Show();
			Hide();
			session.Show();
		}
	}
}
