using System.Runtime.InteropServices;
using System.Security;

namespace MstscVibe;

public class SessionForm : Form {
    private const int WM_SYSCOMMAND = 0x0112;
    private const int MF_SEPARATOR = 0x800;
    private const int MF_STRING = 0x0;
    private const int SC_FULLSCREEN = 0xF100;
    private const int SC_DISCONNECT = 0xF110;
    private const int SC_TYPE_PASSWORD = 0xF130;
    private const int SC_MINIMIZE_WIN = 0xF020;
    private const int SC_RESTORE_WIN = 0xF120;

    [DllImport("user32.dll")]
    private static extern short VkKeyScanW(char ch);
    [DllImport("user32.dll")]
    private static extern uint MapVirtualKeyW(uint uCode, uint uMapType);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    [DllImport("user32.dll")]
    private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);
    [DllImport("user32.dll")]
    private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);
    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);
    private readonly RdpClientHost _rdpClient;
    private readonly RdpFile _rdpFile;
    private readonly System.Windows.Forms.Timer _disconnectTimer;
    private readonly System.Windows.Forms.Timer _resizeTimer;
    private readonly SecureString? _securePassword;
    private Size _lastClientSize;
    private bool _isFullScreen;
    private FormBorderStyle _savedBorderStyle;
    private FormWindowState _savedWindowState;
    private Size _savedClientSize;

    public SessionForm(RdpFile rdpFile) {
        _rdpFile = rdpFile;

        Text = $"MstscVibe - {rdpFile.FullAddress}";
        StartPosition = FormStartPosition.CenterScreen;

        _rdpClient = new RdpClientHost { Dock = DockStyle.Fill };
        Controls.Add(_rdpClient);

        _rdpClient.RequestMinimize += (s, e) => {
            if (_isFullScreen) WindowState = FormWindowState.Minimized;
        };
        _rdpClient.RequestLeaveFullScreen += (s, e) => {
            if (_isFullScreen) ExitFullScreen();
        };

        if (!string.IsNullOrEmpty(rdpFile.Password)) {
            _securePassword = new SecureString();
            foreach (var c in rdpFile.Password)
                _securePassword.AppendChar(c);
            _securePassword.MakeReadOnly();
        }

        if (rdpFile.ScreenModeId == 2) {
            EnterFullScreen();
        } else {
            ClientSize = new Size(rdpFile.DesktopWidth, rdpFile.DesktopHeight);
            MinimumSize = new Size(200, 200);
        }

        ResizeEnd += SessionForm_ResizeEnd;

        _resizeTimer = new System.Windows.Forms.Timer { Interval = 300 };
        _resizeTimer.Tick += ResizeTimer_Tick;

        _disconnectTimer = new System.Windows.Forms.Timer { Interval = 500 };
        _disconnectTimer.Tick += DisconnectTimer_Tick;

        Load += SessionForm_Load;
        FormClosing += SessionForm_FormClosing;
    }

    protected override void OnHandleCreated(EventArgs e) {
        base.OnHandleCreated(e);
        var sysMenu = GetSystemMenu(Handle, false);
        AppendMenu(sysMenu, MF_SEPARATOR, 0, string.Empty);
        AppendMenu(sysMenu, MF_STRING, SC_FULLSCREEN, "Full Screen\tCtrl+Alt+Break");
        AppendMenu(sysMenu, MF_STRING, SC_TYPE_PASSWORD, "Type Password");
        AppendMenu(sysMenu, MF_STRING, SC_DISCONNECT, "Disconnect");
    }

    protected override void WndProc(ref Message m) {
        if (m.Msg == WM_SYSCOMMAND) {
            int cmd = (int)(m.WParam.ToInt64() & 0xFFF0);
            if (cmd == SC_FULLSCREEN) {
                if (_isFullScreen) ExitFullScreen(); else EnterFullScreen();
                return;
            }
            if (cmd == SC_TYPE_PASSWORD) {
                TypePasswordIntoSession();
                return;
            }
            if (cmd == SC_DISCONNECT) {
                Close();
                return;
            }
            if (cmd == SC_MINIMIZE_WIN && _isFullScreen) {
                WindowState = FormWindowState.Minimized;
                return;
            }
            if (cmd == SC_RESTORE_WIN && _isFullScreen) {
                ExitFullScreen();
                return;
            }
        }
        base.WndProc(ref m);
    }

    private void SessionForm_Load(object? sender, EventArgs e) {
        try {
            ConfigureClient();
            _rdpClient.Connect();
            _rdpClient.SmartSizing = true;
            _lastClientSize = _rdpClient.Size;
            _disconnectTimer.Start();
        } catch (Exception ex) {
            MessageBox.Show($"Failed to start RDP connection:\n{ex.Message}", "Connection Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void ConfigureClient() {
        _rdpClient.Server = ParseHost(_rdpFile.FullAddress);

        var adv = _rdpClient.AdvancedSettings;
        if (adv != null) {
            adv.RDPPort = ParsePort(_rdpFile.FullAddress);
            adv.EnableCredSspSupport = _rdpFile.EnableCredSSPSupport;
            adv.AuthenticationLevel = 0; // No authentication warning popup
            adv.RedirectClipboard = _rdpFile.RedirectClipboard;
            adv.RedirectPrinters = _rdpFile.RedirectPrinters;
            adv.RedirectSmartCards = _rdpFile.RedirectSmartCards;
            adv.RedirectDrives = _rdpFile.RedirectDrives;
        }

        if (!string.IsNullOrWhiteSpace(_rdpFile.Username))
            _rdpClient.UserName = _rdpFile.Username;

        if (!string.IsNullOrEmpty(_rdpFile.Password)) {
            var adv2 = _rdpClient.AdvancedSettings;
            if (adv2 != null)
                adv2.ClearTextPassword = _rdpFile.Password;
        }

        _rdpClient.DesktopWidth = _rdpFile.DesktopWidth;
        _rdpClient.DesktopHeight = _rdpFile.DesktopHeight;
        _rdpClient.ColorDepth = _rdpFile.SessionBpp;

        // Audio
        var sec = _rdpClient.SecuredSettings;
        if (sec != null)
            sec.AudioRedirectionMode = _rdpFile.AudioMode;

        // Gateway
        if (_rdpFile.GatewayUsageMethod > 0 && !string.IsNullOrWhiteSpace(_rdpFile.GatewayHostname)) {
            var gw = _rdpClient.TransportSettings;
            if (gw != null) {
                gw.GatewayUsageMethod = (uint)_rdpFile.GatewayUsageMethod;
                gw.GatewayHostname = _rdpFile.GatewayHostname;
                gw.GatewayCredsSource = 0;
            }
        }

        if (_rdpFile.ScreenModeId == 2)
            _rdpClient.FullScreen = true;

        // Admin session
        if (_rdpFile.AdminSession)
            adv?.ConnectToAdministerServer = true;

        // Public mode — disable persistent bitmap caching and auto-reconnect
        if (_rdpFile.PublicMode && adv != null) {
            adv.BitmapPersistence = 0;
            adv.EnableAutoReconnect = false;
        }

        // Multi-monitor
        if (_rdpFile.UseMultimon)
            adv?.UseMultimon = true;

        // Prompt for credentials
        if (_rdpFile.PromptForCredentials)
            adv?.EnableCredSspSupport = true;

        // Shadow session
        if (_rdpFile.ShadowSessionId.HasValue) {
            try {
                var ocx = _rdpClient.AdvancedSettings;
                // Shadow is set via RDP property
            } catch {
                // Shadow not supported on all versions
            }
        }

        // Enable the native fullscreen connection bar
        if (adv != null) {
            adv.ConnectionBarShowMinimizeButton = true;
            adv.ConnectionBarShowRestoreButton = true;
            adv.ConnectionBarShowPinButton = true;
            adv.PinConnectionBar = false;
            adv.ContainerHandledFullScreen = 1;
        }
    }

    private void DisconnectTimer_Tick(object? sender, EventArgs e) {
        try {
            if (_rdpClient.Connected == 0) {
                _disconnectTimer.Stop();
                Close();
                return;
            }

            // Detect when the native connection bar's restore button exits fullscreen
            if (_isFullScreen && !_rdpClient.FullScreen) {
                ExitFullScreen();
            }
        } catch {
            _disconnectTimer.Stop();
            Close();
        }
    }

    private void SessionForm_ResizeEnd(object? sender, EventArgs e) {
        ApplyResize();
    }

    protected override void OnClientSizeChanged(EventArgs e) {
        base.OnClientSizeChanged(e);

        // Re-enter fullscreen when restored from minimized
        if (_isFullScreen && WindowState == FormWindowState.Maximized && _rdpClient.Connected != 0 && !_rdpClient.FullScreen) {
            _rdpClient.FullScreen = true;
        }

        // Debounce: restart timer on every size change, only fire once settled
        if (_rdpClient.Connected != 0) {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }
    }

    private void ResizeTimer_Tick(object? sender, EventArgs e) {
        _resizeTimer.Stop();
        ApplyResize();
    }

    private void ApplyResize() {
        if (_rdpClient.Connected == 0 || WindowState == FormWindowState.Minimized)
            return;

        var currentSize = _rdpClient.Size;
        if (currentSize == _lastClientSize)
            return;

        _lastClientSize = currentSize;
        if (currentSize.Width > 0 && currentSize.Height > 0)
            _rdpClient.Reconnect(currentSize.Width, currentSize.Height);
    }

    private void EnterFullScreen() {
        if (_isFullScreen) return;
        _savedBorderStyle = FormBorderStyle;
        _savedWindowState = WindowState;
        _savedClientSize = ClientSize;

        WindowState = FormWindowState.Normal;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        _isFullScreen = true;

        if (_rdpClient.Connected != 0)
            _rdpClient.FullScreen = true;
    }

    private void ExitFullScreen() {
        if (!_isFullScreen) return;
        _isFullScreen = false;

        if (_rdpClient.Connected != 0 && _rdpClient.FullScreen)
            _rdpClient.FullScreen = false;

        WindowState = FormWindowState.Normal;
        FormBorderStyle = _savedBorderStyle != FormBorderStyle.None ? _savedBorderStyle : FormBorderStyle.Sizable;
        ClientSize = _savedClientSize.Width > 0 ? _savedClientSize : new Size(_rdpFile.DesktopWidth, _rdpFile.DesktopHeight);
        MinimumSize = new Size(200, 200);

        // Reset remote resolution to match the restored window
        _lastClientSize = Size.Empty;
        ApplyResize();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
        if (keyData == (Keys.Control | Keys.Alt | Keys.Cancel) || keyData == (Keys.Control | Keys.Alt | Keys.Pause)) {
            if (_isFullScreen)
                ExitFullScreen();
            else
                EnterFullScreen();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void TypePasswordIntoSession() {
        if (_securePassword == null || _securePassword.Length == 0) {
            MessageBox.Show("No password stored for this session.", "Type Password",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_rdpClient.Connected == 0) return;

        var ptr = Marshal.SecureStringToGlobalAllocUnicode(_securePassword);
        try {
            var password = Marshal.PtrToStringUni(ptr) ?? string.Empty;
            var keyUpList = new List<bool>();
            var scanCodeList = new List<int>();

            foreach (char c in password) {
                short vk = VkKeyScanW(c);
                if (vk == -1) continue;
                byte virtualKey = (byte)(vk & 0xFF);
                byte shiftState = (byte)((vk >> 8) & 0xFF);
                uint scanCode = MapVirtualKeyW(virtualKey, 0);

                bool needShift = (shiftState & 1) != 0;
                bool needCtrl = (shiftState & 2) != 0;
                bool needAlt = (shiftState & 4) != 0;

                if (needShift) { keyUpList.Add(false); scanCodeList.Add(0x2A); }
                if (needCtrl) { keyUpList.Add(false); scanCodeList.Add(0x1D); }
                if (needAlt) { keyUpList.Add(false); scanCodeList.Add(0x38); }

                keyUpList.Add(false); scanCodeList.Add((int)scanCode);
                keyUpList.Add(true); scanCodeList.Add((int)scanCode);

                if (needAlt) { keyUpList.Add(true); scanCodeList.Add(0x38); }
                if (needCtrl) { keyUpList.Add(true); scanCodeList.Add(0x1D); }
                if (needShift) { keyUpList.Add(true); scanCodeList.Add(0x2A); }
            }

            if (keyUpList.Count > 0) {
                _rdpClient.SendKeys(keyUpList.Count, keyUpList.ToArray(), scanCodeList.ToArray());
            }
        } finally {
            Marshal.ZeroFreeGlobalAllocUnicode(ptr);
        }
    }

    private void SessionForm_FormClosing(object? sender, FormClosingEventArgs e) {
        _resizeTimer.Stop();
        _disconnectTimer.Stop();
        _securePassword?.Dispose();
        try {
            if (_rdpClient.Connected != 0)
                _rdpClient.Disconnect();
        } catch {
            // Ignore errors during teardown
        }
    }

    private static string ParseHost(string fullAddress) {
        var idx = fullAddress.LastIndexOf(':');
        if (idx > 0 && int.TryParse(fullAddress[(idx + 1)..], out _))
            return fullAddress[..idx];
        return fullAddress;
    }

    private static int ParsePort(string fullAddress) {
        var idx = fullAddress.LastIndexOf(':');
        if (idx > 0 && int.TryParse(fullAddress[(idx + 1)..], out var port))
            return port;
        return 3389;
    }
}
