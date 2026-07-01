using System.Reflection;

namespace MstscVibe;

public class OptionsForm : Form {
    private readonly GroupBox grpDisplay;
    private readonly Label lblResolution;
    private readonly ComboBox cmbResolution;
    private readonly Label lblColorDepth;
    private readonly ComboBox cmbColorDepth;
    private readonly CheckBox chkFullScreen;

    private readonly GroupBox grpRedirection;
    private readonly CheckBox chkClipboard;
    private readonly CheckBox chkPrinters;
    private readonly CheckBox chkDrives;
    private readonly CheckBox chkSmartCards;
    private readonly Label lblAudio;
    private readonly ComboBox cmbAudio;

    private readonly GroupBox grpScreenshot;
    private readonly Label lblScreenshotPath;
    private readonly TextBox txtScreenshotPath;
    private readonly Button btnBrowseScreenshots;

    private readonly Button btnOk;
    private readonly Button btnCancel;

    public int ResolutionIndex { get; private set; }
    public int ColorDepthIndex { get; private set; }
    public bool FullScreen { get; private set; }
    public bool RedirectClipboard { get; private set; }
    public bool RedirectPrinters { get; private set; }
    public bool RedirectDrives { get; private set; }
    public bool RedirectSmartCards { get; private set; }
    public int AudioMode { get; private set; }
    public string ScreenshotPath { get; private set; } = "";

    public OptionsForm(RdpFile rdp) {
        Text = "Options";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(380, 460);
        ShowInTaskbar = false;

        // --- Display group ---
        lblResolution = new Label { Text = "Resolution:", Location = new Point(16, 25), AutoSize = true };
        cmbResolution = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(120, 22),
            Size = new Size(220, 23)
        };
        cmbResolution.Items.AddRange(["640 x 480", "800 x 600", "1024 x 768", "1280 x 720", "1280 x 1024",
            "1366 x 768", "1600 x 900", "1920 x 1080", "2560 x 1440", "3840 x 2160"]);

        var resText = $"{rdp.DesktopWidth} x {rdp.DesktopHeight}";
        var resIdx = cmbResolution.Items.IndexOf(resText);
        cmbResolution.SelectedIndex = resIdx >= 0 ? resIdx : 2;

        lblColorDepth = new Label { Text = "Color depth:", Location = new Point(16, 57), AutoSize = true };
        cmbColorDepth = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(120, 54),
            Size = new Size(220, 23)
        };
        cmbColorDepth.Items.AddRange(["15 bit", "16 bit", "24 bit", "32 bit"]);
        cmbColorDepth.SelectedIndex = rdp.SessionBpp switch { 15 => 0, 16 => 1, 24 => 2, _ => 3 };

        chkFullScreen = new CheckBox {
            Text = "Full screen",
            Location = new Point(16, 87),
            AutoSize = true,
            Checked = rdp.ScreenModeId == 2
        };
        chkFullScreen.CheckedChanged += (s, e) => cmbResolution.Enabled = !chkFullScreen.Checked;
        cmbResolution.Enabled = !chkFullScreen.Checked;

        grpDisplay = new GroupBox {
            Text = "Display",
            Location = new Point(12, 12),
            Size = new Size(356, 120)
        };
        grpDisplay.Controls.AddRange([lblResolution, cmbResolution, lblColorDepth, cmbColorDepth, chkFullScreen]);

        // --- Redirection group ---
        chkClipboard = new CheckBox { Text = "Clipboard", Location = new Point(16, 25), AutoSize = true, Checked = rdp.RedirectClipboard };
        chkPrinters = new CheckBox { Text = "Printers", Location = new Point(16, 50), AutoSize = true, Checked = rdp.RedirectPrinters };
        chkDrives = new CheckBox { Text = "Drives", Location = new Point(160, 25), AutoSize = true, Checked = rdp.RedirectDrives };
        chkSmartCards = new CheckBox { Text = "Smart cards", Location = new Point(160, 50), AutoSize = true, Checked = rdp.RedirectSmartCards };

        lblAudio = new Label { Text = "Audio:", Location = new Point(16, 80), AutoSize = true };
        cmbAudio = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(120, 77),
            Size = new Size(220, 23)
        };
        cmbAudio.Items.AddRange(["Play on this computer", "Play on remote computer", "Do not play"]);
        cmbAudio.SelectedIndex = rdp.AudioMode is >= 0 and < 3 ? rdp.AudioMode : 0;

        grpRedirection = new GroupBox {
            Text = "Local resources",
            Location = new Point(12, 140),
            Size = new Size(356, 115)
        };
        grpRedirection.Controls.AddRange([chkClipboard, chkPrinters, chkDrives, chkSmartCards, lblAudio, cmbAudio]);

        // --- Screenshot group ---
        lblScreenshotPath = new Label { Text = "Save location:", Location = new Point(16, 25), AutoSize = true };
        txtScreenshotPath = new TextBox {
            Location = new Point(16, 50),
            Size = new Size(268, 23),
            Text = UserSettings.Load().ScreenshotPath,
            ReadOnly = false
        };
        btnBrowseScreenshots = new Button {
            Text = "Browse...",
            Location = new Point(290, 50),
            Size = new Size(66, 23)
        };
        btnBrowseScreenshots.Click += BtnBrowseScreenshots_Click;

        grpScreenshot = new GroupBox {
            Text = "Screenshots",
            Location = new Point(12, 265),
            Size = new Size(356, 90)
        };
        grpScreenshot.Controls.AddRange([lblScreenshotPath, txtScreenshotPath, btnBrowseScreenshots]);

        // --- Buttons ---
        btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(200, 368), Size = new Size(80, 30) };
        btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(288, 368), Size = new Size(80, 30) };
        btnOk.Click += BtnOk_Click;

        AcceptButton = btnOk;
        CancelButton = btnCancel;
        Controls.AddRange([grpDisplay, grpRedirection, grpScreenshot, btnOk, btnCancel]);

        Text = $"MstscVibe - Options - {Assembly.GetExecutingAssembly().GetName().Version}";
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
