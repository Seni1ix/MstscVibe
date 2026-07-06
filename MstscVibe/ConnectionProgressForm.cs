using System.ComponentModel;

namespace MstscVibe;

public class ConnectionProgressForm : Form {
    private Label _statusLabel = null!;
    private ProgressBar _progressBar = null!;
    private Label _titleLabel = null!;
    private Panel _contentPanel = null!;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Status {
        get => _statusLabel.Text;
        set {
            if (InvokeRequired) {
                Invoke(() => _statusLabel.Text = value);
            } else {
                _statusLabel.Text = value;
            }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Progress {
        get => _progressBar.Value;
        set {
            if (InvokeRequired) {
                Invoke(() => _progressBar.Value = Math.Min(value, 100));
            } else {
                _progressBar.Value = Math.Min(value, 100);
            }
        }
    }

    public ConnectionProgressForm(string serverAddress) {
        InitializeComponent();
        _titleLabel.Text = $"Connecting to {serverAddress}...";
        _statusLabel.Text = "Initializing connection...";
    }

    private void InitializeComponent() {
        // Form settings
        AutoScaleDimensions = new SizeF(6F, 13F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 200);
        ControlBox = false;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MstscVibe - Connecting";
        TopMost = true;
        BackColor = SystemColors.Control;

        // Content panel
        _contentPanel = new Panel {
            Dock = DockStyle.Fill,
            Padding = new Padding(20)
        };

        // Title label
        _titleLabel = new Label {
            AutoSize = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Location = new Point(20, 20),
            Text = "Connecting..."
        };

        // Status label
        _statusLabel = new Label {
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            Location = new Point(20, 60),
            Text = "Initializing connection...",
            ForeColor = SystemColors.ControlDarkDark
        };

        // Progress bar
        _progressBar = new ProgressBar {
            Location = new Point(20, 100),
            Size = new Size(360, 20),
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 30
        };

        _contentPanel.Controls.Add(_titleLabel);
        _contentPanel.Controls.Add(_statusLabel);
        _contentPanel.Controls.Add(_progressBar);

        Controls.Add(_contentPanel);
    }

    public void SetDeterminateProgress(int value) {
        if (InvokeRequired) {
            Invoke(() => {
                if (_progressBar.Style != ProgressBarStyle.Continuous) {
                    _progressBar.Style = ProgressBarStyle.Continuous;
                }
                _progressBar.Value = Math.Min(value, 100);
            });
        } else {
            if (_progressBar.Style != ProgressBarStyle.Continuous) {
                _progressBar.Style = ProgressBarStyle.Continuous;
            }
            _progressBar.Value = Math.Min(value, 100);
        }
    }

    public void SetIndeterminateProgress() {
        if (InvokeRequired) {
            Invoke(() => _progressBar.Style = ProgressBarStyle.Marquee);
        } else {
            _progressBar.Style = ProgressBarStyle.Marquee;
        }
    }

	protected override void OnFormClosing(FormClosingEventArgs e) {
		System.Threading.Thread.Sleep(1500); // Wait for 1.5 seconds before closing to ensure the user sees the final status
		_progressBar?.Dispose();
		_statusLabel?.Dispose();
		_titleLabel?.Dispose();
		_contentPanel?.Dispose();
		base.OnFormClosing(e);
	}
}
