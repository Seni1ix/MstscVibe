namespace MstscVibe {
	partial class OptionsForm {
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent() {
			grpDisplay = new GroupBox();
			lblResolution = new Label();
			cmbResolution = new ComboBox();
			lblColorDepth = new Label();
			cmbColorDepth = new ComboBox();
			chkFullScreen = new CheckBox();
			grpRedirection = new GroupBox();
			chkClipboard = new CheckBox();
			chkPrinters = new CheckBox();
			chkDrives = new CheckBox();
			chkSmartCards = new CheckBox();
			lblAudio = new Label();
			cmbAudio = new ComboBox();
			grpScreenshot = new GroupBox();
			lblScreenshotPath = new Label();
			txtScreenshotPath = new TextBox();
			btnBrowseScreenshots = new Button();
			btnOk = new Button();
			btnCancel = new Button();
			grpDisplay.SuspendLayout();
			grpRedirection.SuspendLayout();
			grpScreenshot.SuspendLayout();
			SuspendLayout();
			//
			// lblResolution
			//
			lblResolution.AutoSize = true;
			lblResolution.Location = new Point(16, 25);
			lblResolution.Name = "lblResolution";
			lblResolution.Text = "Resolution:";
			//
			// cmbResolution
			//
			cmbResolution.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbResolution.Items.AddRange(new object[] { "640 x 480", "800 x 600", "1024 x 768", "1280 x 720", "1280 x 1024", "1366 x 768", "1600 x 900", "1920 x 1080", "2560 x 1440", "3840 x 2160" });
			cmbResolution.Location = new Point(120, 22);
			cmbResolution.Name = "cmbResolution";
			cmbResolution.Size = new Size(220, 23);
			//
			// lblColorDepth
			//
			lblColorDepth.AutoSize = true;
			lblColorDepth.Location = new Point(16, 57);
			lblColorDepth.Name = "lblColorDepth";
			lblColorDepth.Text = "Color depth:";
			//
			// cmbColorDepth
			//
			cmbColorDepth.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbColorDepth.Items.AddRange(new object[] { "15 bit", "16 bit", "24 bit", "32 bit" });
			cmbColorDepth.Location = new Point(120, 54);
			cmbColorDepth.Name = "cmbColorDepth";
			cmbColorDepth.Size = new Size(220, 23);
			//
			// chkFullScreen
			//
			chkFullScreen.AutoSize = true;
			chkFullScreen.Location = new Point(16, 87);
			chkFullScreen.Name = "chkFullScreen";
			chkFullScreen.Text = "Full screen";
			chkFullScreen.CheckedChanged += ChkFullScreen_CheckedChanged;
			//
			// grpDisplay
			//
			grpDisplay.Controls.Add(lblResolution);
			grpDisplay.Controls.Add(cmbResolution);
			grpDisplay.Controls.Add(lblColorDepth);
			grpDisplay.Controls.Add(cmbColorDepth);
			grpDisplay.Controls.Add(chkFullScreen);
			grpDisplay.Location = new Point(12, 12);
			grpDisplay.Name = "grpDisplay";
			grpDisplay.Size = new Size(356, 120);
			grpDisplay.TabStop = false;
			grpDisplay.Text = "Display";
			//
			// chkClipboard
			//
			chkClipboard.AutoSize = true;
			chkClipboard.Location = new Point(16, 25);
			chkClipboard.Name = "chkClipboard";
			chkClipboard.Text = "Clipboard";
			//
			// chkPrinters
			//
			chkPrinters.AutoSize = true;
			chkPrinters.Location = new Point(16, 50);
			chkPrinters.Name = "chkPrinters";
			chkPrinters.Text = "Printers";
			//
			// chkDrives
			//
			chkDrives.AutoSize = true;
			chkDrives.Location = new Point(160, 25);
			chkDrives.Name = "chkDrives";
			chkDrives.Text = "Drives";
			//
			// chkSmartCards
			//
			chkSmartCards.AutoSize = true;
			chkSmartCards.Location = new Point(160, 50);
			chkSmartCards.Name = "chkSmartCards";
			chkSmartCards.Text = "Smart cards";
			//
			// lblAudio
			//
			lblAudio.AutoSize = true;
			lblAudio.Location = new Point(16, 80);
			lblAudio.Name = "lblAudio";
			lblAudio.Text = "Audio:";
			//
			// cmbAudio
			//
			cmbAudio.DropDownStyle = ComboBoxStyle.DropDownList;
			cmbAudio.Items.AddRange(new object[] { "Play on this computer", "Play on remote computer", "Do not play" });
			cmbAudio.Location = new Point(120, 77);
			cmbAudio.Name = "cmbAudio";
			cmbAudio.Size = new Size(220, 23);
			//
			// grpRedirection
			//
			grpRedirection.Controls.Add(chkClipboard);
			grpRedirection.Controls.Add(chkPrinters);
			grpRedirection.Controls.Add(chkDrives);
			grpRedirection.Controls.Add(chkSmartCards);
			grpRedirection.Controls.Add(lblAudio);
			grpRedirection.Controls.Add(cmbAudio);
			grpRedirection.Location = new Point(12, 140);
			grpRedirection.Name = "grpRedirection";
			grpRedirection.Size = new Size(356, 115);
			grpRedirection.TabStop = false;
			grpRedirection.Text = "Local resources";
			//
			// lblScreenshotPath
			//
			lblScreenshotPath.AutoSize = true;
			lblScreenshotPath.Location = new Point(16, 25);
			lblScreenshotPath.Name = "lblScreenshotPath";
			lblScreenshotPath.Text = "Save location:";
			//
			// txtScreenshotPath
			//
			txtScreenshotPath.Location = new Point(16, 50);
			txtScreenshotPath.Name = "txtScreenshotPath";
			txtScreenshotPath.Size = new Size(268, 23);
			//
			// btnBrowseScreenshots
			//
			btnBrowseScreenshots.Location = new Point(290, 50);
			btnBrowseScreenshots.Name = "btnBrowseScreenshots";
			btnBrowseScreenshots.Size = new Size(66, 23);
			btnBrowseScreenshots.Text = "Browse...";
			btnBrowseScreenshots.Click += BtnBrowseScreenshots_Click;
			//
			// grpScreenshot
			//
			grpScreenshot.Controls.Add(lblScreenshotPath);
			grpScreenshot.Controls.Add(txtScreenshotPath);
			grpScreenshot.Controls.Add(btnBrowseScreenshots);
			grpScreenshot.Location = new Point(12, 265);
			grpScreenshot.Name = "grpScreenshot";
			grpScreenshot.Size = new Size(356, 90);
			grpScreenshot.TabStop = false;
			grpScreenshot.Text = "Screenshots";
			//
			// btnOk
			//
			btnOk.DialogResult = DialogResult.OK;
			btnOk.Location = new Point(200, 368);
			btnOk.Name = "btnOk";
			btnOk.Size = new Size(80, 30);
			btnOk.Text = "OK";
			btnOk.Click += BtnOk_Click;
			//
			// btnCancel
			//
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.Location = new Point(288, 368);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(80, 30);
			btnCancel.Text = "Cancel";
			//
			// OptionsForm
			//
			AcceptButton = btnOk;
			CancelButton = btnCancel;
			ClientSize = new Size(380, 460);
			Controls.Add(grpDisplay);
			Controls.Add(grpRedirection);
			Controls.Add(grpScreenshot);
			Controls.Add(btnOk);
			Controls.Add(btnCancel);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "OptionsForm";
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Options";
			grpDisplay.ResumeLayout(false);
			grpDisplay.PerformLayout();
			grpRedirection.ResumeLayout(false);
			grpRedirection.PerformLayout();
			grpScreenshot.ResumeLayout(false);
			grpScreenshot.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private GroupBox grpDisplay;
		private Label lblResolution;
		private ComboBox cmbResolution;
		private Label lblColorDepth;
		private ComboBox cmbColorDepth;
		private CheckBox chkFullScreen;

		private GroupBox grpRedirection;
		private CheckBox chkClipboard;
		private CheckBox chkPrinters;
		private CheckBox chkDrives;
		private CheckBox chkSmartCards;
		private Label lblAudio;
		private ComboBox cmbAudio;

		private GroupBox grpScreenshot;
		private Label lblScreenshotPath;
		private TextBox txtScreenshotPath;
		private Button btnBrowseScreenshots;

		private Button btnOk;
		private Button btnCancel;
	}
}
