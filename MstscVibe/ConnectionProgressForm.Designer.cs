namespace MstscVibe {
	partial class ConnectionProgressForm {
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent() {
			_contentPanel = new Panel();
			_titleLabel = new Label();
			_statusLabel = new Label();
			_progressBar = new ProgressBar();
			_contentPanel.SuspendLayout();
			SuspendLayout();
			//
			// _contentPanel
			//
			_contentPanel.Controls.Add(_titleLabel);
			_contentPanel.Controls.Add(_statusLabel);
			_contentPanel.Controls.Add(_progressBar);
			_contentPanel.Dock = DockStyle.Fill;
			_contentPanel.Location = new Point(0, 0);
			_contentPanel.Name = "_contentPanel";
			_contentPanel.Padding = new Padding(20);
			_contentPanel.Size = new Size(400, 200);
			//
			// _titleLabel
			//
			_titleLabel.AutoSize = true;
			_titleLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
			_titleLabel.Location = new Point(20, 20);
			_titleLabel.Name = "_titleLabel";
			_titleLabel.Text = "Connecting...";
			//
			// _statusLabel
			//
			_statusLabel.AutoSize = true;
			_statusLabel.Font = new Font("Segoe UI", 10F);
			_statusLabel.ForeColor = SystemColors.ControlDarkDark;
			_statusLabel.Location = new Point(20, 60);
			_statusLabel.Name = "_statusLabel";
			_statusLabel.Text = "Initializing connection...";
			//
			// _progressBar
			//
			_progressBar.Location = new Point(20, 100);
			_progressBar.MarqueeAnimationSpeed = 30;
			_progressBar.Name = "_progressBar";
			_progressBar.Size = new Size(360, 20);
			_progressBar.Style = ProgressBarStyle.Marquee;
			//
			// ConnectionProgressForm
			//
			AutoScaleDimensions = new SizeF(6F, 13F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = SystemColors.Control;
			ClientSize = new Size(400, 200);
			ControlBox = false;
			Controls.Add(_contentPanel);
			FormBorderStyle = FormBorderStyle.FixedSingle;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ConnectionProgressForm";
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterScreen;
			Text = "MstscVibe - Connecting";
			TopMost = true;
			_contentPanel.ResumeLayout(false);
			_contentPanel.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Panel _contentPanel;
		private Label _titleLabel;
		private Label _statusLabel;
		private ProgressBar _progressBar;
	}
}
