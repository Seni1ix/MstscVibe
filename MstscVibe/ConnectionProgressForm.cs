using System.ComponentModel;

namespace MstscVibe {
	public partial class ConnectionProgressForm : Form {
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
}
