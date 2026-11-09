using System.Reflection;

namespace MstscVibe {
	public partial class OptionsForm : Form {
		public int ResolutionIndex { get; private set; }
		public int ColorDepthIndex { get; private set; }
		public bool FullScreen { get; private set; }
		public bool RedirectClipboard { get; private set; }
		public bool RedirectPrinters { get; private set; }
		public bool RedirectDrives { get; private set; }
		public bool RedirectSmartCards { get; private set; }
		public int AudioMode { get; private set; }
		public string ScreenshotPath { get; private set; } = "";

		// Parameterless constructor for the Windows Forms designer.
		public OptionsForm() {
			InitializeComponent();
		}

		public OptionsForm(RdpFile rdp) : this() {
			var resIdx = cmbResolution.Items.IndexOf($"{rdp.DesktopWidth} x {rdp.DesktopHeight}");
			cmbResolution.SelectedIndex = resIdx >= 0 ? resIdx : 2;
			cmbColorDepth.SelectedIndex = rdp.SessionBpp switch { 15 => 0, 16 => 1, 24 => 2, _ => 3 };
			chkFullScreen.Checked = rdp.ScreenModeId == 2;
			cmbResolution.Enabled = !chkFullScreen.Checked;

			chkClipboard.Checked = rdp.RedirectClipboard;
			chkPrinters.Checked = rdp.RedirectPrinters;
			chkDrives.Checked = rdp.RedirectDrives;
			chkSmartCards.Checked = rdp.RedirectSmartCards;
			cmbAudio.SelectedIndex = rdp.AudioMode is >= 0 and < 3 ? rdp.AudioMode : 0;

			txtScreenshotPath.Text = UserSettings.Load().ScreenshotPath;

			Text = $"MstscVibe - Options - {Assembly.GetExecutingAssembly().GetName().Version}";
		}

		private void ChkFullScreen_CheckedChanged(object? sender, EventArgs e) {
			cmbResolution.Enabled = !chkFullScreen.Checked;
		}

		private void BtnBrowseScreenshots_Click(object? sender, EventArgs e) {
			using (var folderDialog = new FolderBrowserDialog {
				Description = "Select folder for saving screenshots",
				SelectedPath = txtScreenshotPath.Text
			}) {
				if (folderDialog.ShowDialog() == DialogResult.OK) {
					txtScreenshotPath.Text = folderDialog.SelectedPath;
				}
			}
		}

		private void BtnOk_Click(object? sender, EventArgs e) {
			ResolutionIndex = cmbResolution.SelectedIndex;
			ColorDepthIndex = cmbColorDepth.SelectedIndex;
			FullScreen = chkFullScreen.Checked;
			RedirectClipboard = chkClipboard.Checked;
			RedirectPrinters = chkPrinters.Checked;
			RedirectDrives = chkDrives.Checked;
			RedirectSmartCards = chkSmartCards.Checked;
			AudioMode = cmbAudio.SelectedIndex;
			ScreenshotPath = txtScreenshotPath.Text;
		}

		private static readonly string[] Resolutions = [
			"640 x 480", "800 x 600", "1024 x 768", "1280 x 720", "1280 x 1024",
			"1366 x 768", "1600 x 900", "1920 x 1080", "2560 x 1440", "3840 x 2160"
		];

		public void ApplyTo(RdpFile rdp) {
			if (ResolutionIndex >= 0 && ResolutionIndex < Resolutions.Length) {
				var parts = Resolutions[ResolutionIndex].Split('x', StringSplitOptions.TrimEntries);
				if (parts.Length == 2 && int.TryParse(parts[0], out var w) && int.TryParse(parts[1], out var h)) {
					rdp.DesktopWidth = w;
					rdp.DesktopHeight = h;
				}
			}
			rdp.SessionBpp = ColorDepthIndex switch { 0 => 15, 1 => 16, 2 => 24, _ => 32 };
			rdp.ScreenModeId = FullScreen ? 2 : 1;
			rdp.RedirectClipboard = RedirectClipboard;
			rdp.RedirectPrinters = RedirectPrinters;
			rdp.RedirectDrives = RedirectDrives;
			rdp.RedirectSmartCards = RedirectSmartCards;
			rdp.AudioMode = AudioMode;
		}
	}
}
