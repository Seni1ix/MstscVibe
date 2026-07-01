using System.Runtime.InteropServices;
using System.Security;
using System.Diagnostics;

namespace MstscVibe;

public class SessionForm : Form {
    private const int WM_SYSCOMMAND = 0x0112;
    private const int WM_KEYDOWN = 0x0100;
    private const int MF_SEPARATOR = 0x800;
    private const int MF_STRING = 0x0;
    private const int SC_FULLSCREEN = 0xF100;
    private const int SC_DISCONNECT = 0xF110;
    private const int SC_TYPE_PASSWORD = 0xF130;
    private const int SC_MINIMIZE_WIN = 0xF020;
    private const int SC_RESTORE_WIN = 0xF120;
    private const int SC_TAKE_SCREENSHOT = 0xF131;
    private const int VK_P = 0x50;

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
    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);
    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private IntPtr _keyboardHookId = IntPtr.Zero;
    private LowLevelKeyboardProc? _keyboardProc;
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN_HOOK = 0x0100;
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
    private ConnectionProgressForm? _progressForm;
    private System.Windows.Forms.Timer? _connectionProgressTimer;

    public SessionForm(RdpFile rdpFile) {
        _rdpFile = rdpFile;
        string filename = Path.GetFileNameWithoutExtension(_rdpFile.FileName);

        Text = $"MstscVibe - {filename} - {rdpFile.FullAddress}";
        StartPosition = FormStartPosition.CenterScreen;

        _rdpClient = new RdpClientHost { Dock = DockStyle.Fill };
        Controls.Add(_rdpClient);

        _rdpClient.RequestMinimize += (s, e) => {
            if (_isFullScreen) WindowState = FormWindowState.Minimized;
        };
        _rdpClient.RequestLeaveFullScreen += (s, e) => {
            if (_isFullScreen) ExitFullScreen();
        };
        _rdpClient.Disconnected += (s, reason) => {
            _disconnectTimer.Stop();
            ShowDisconnectReason(reason);
            Close();
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
        AppendMenu(sysMenu, MF_STRING, SC_TAKE_SCREENSHOT, "Take Screenshot\tCtrl+Shift+P");
        AppendMenu(sysMenu, MF_STRING, SC_TYPE_PASSWORD, "Type Password");
        AppendMenu(sysMenu, MF_STRING, SC_DISCONNECT, "Disconnect");
    }

    private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam) {
        if (nCode >= 0 && (int)wParam == WM_KEYDOWN_HOOK) {
            // Only trigger if this form is the foreground window
            if (GetForegroundWindow() == Handle) {
                int vkCode = Marshal.ReadInt32(lParam);
                // Check for Ctrl+Shift+P
                if (vkCode == VK_P && (GetKeyState(0x11) & 0x8000) != 0 && (GetKeyState(0x10) & 0x8000) != 0) {
                    TakeScreenshot();
                    return (IntPtr)1; // Return 1 to prevent further processing
                }
            }
        }
        return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
    }

    private void SetupKeyboardHook() {
        _keyboardProc = KeyboardHookProc;
        using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule) {
            _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private void RemoveKeyboardHook() {
        if (_keyboardHookId != IntPtr.Zero) {
            UnhookWindowsHookEx(_keyboardHookId);
            _keyboardHookId = IntPtr.Zero;
        }
    }

    protected override void WndProc(ref Message m) {
        if (m.Msg == WM_SYSCOMMAND) {
            int cmd = (int)(m.WParam.ToInt64());
            if ((cmd & 0xFFF0) == SC_FULLSCREEN) {
                if (_isFullScreen) ExitFullScreen(); else EnterFullScreen();
                return;
            }
            if (cmd == SC_TAKE_SCREENSHOT) {
                TakeScreenshot();
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
            if ((cmd & 0xFFF0) == SC_MINIMIZE_WIN && _isFullScreen) {
                WindowState = FormWindowState.Minimized;
                return;
            }
            if ((cmd & 0xFFF0) == SC_RESTORE_WIN && _isFullScreen) {
                ExitFullScreen();
                return;
            }
        }
        base.WndProc(ref m);
    }

    private void SessionForm_Load(object? sender, EventArgs e) {
        try {
            SetupKeyboardHook();
            ShowConnectionProgress();
            ConfigureClient();
            _rdpClient.Connect();
            _rdpClient.SmartSizing = true;
            _lastClientSize = _rdpClient.Size;
            _disconnectTimer.Start();
            UpdateProgress(80, "Connected, waiting for session...");
        } catch (Exception ex) {
            CloseProgressForm();
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
                CloseProgressForm();
                Close();
                return;
            }

            // Close progress form once fully connected
            if (_progressForm != null && _rdpClient.Connected != 0) {
                CloseProgressForm();
            }

            // Detect when the native connection bar's restore button exits fullscreen
            if (_isFullScreen && !_rdpClient.FullScreen) {
                ExitFullScreen();
            }
        } catch {
            _disconnectTimer.Stop();
            CloseProgressForm();
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
                if (scanCode == 0) continue;  // Skip characters without valid scan codes

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
        RemoveKeyboardHook();
        _resizeTimer.Stop();
        _disconnectTimer.Stop();
        _connectionProgressTimer?.Stop();
        CloseProgressForm();
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

    //https://learn.microsoft.com/en-us/windows/win32/termserv/imstscaxevents-ondisconnected
    private static string GetDisconnectReasonText(int reason) => reason switch {
        // Standard disconnect codes (0-3)
        0 => "No information available",
        1 => "Local disconnection",
        2 => "Remote disconnection by user",
        3 => "Remote disconnection by server",

        // Legacy codes (4-30) - kept for backward compatibility
        4 => "Insufficient client license",
        5 => "Client license expired",
        6 => "Replace license",
        7 => "Host not found",
        8 => "Out of memory",
        9 => "Connection refused",
        10 => "Logon failed",
        11 => "Logon failure",
        12 => "Wrong password",
        13 => "Access denied",
        14 => "Unknown error",
        15 => "Unsupported version",
        16 => "Idle timeout",
        17 => "Logon timeout",
        18 => "Disconnect by other instance",
        19 => "Out of memory (server)",
        20 => "Server denied connection",
        21 => "Callback disconnect",
        22 => "Account disabled",
        23 => "Account expired",
        24 => "Account locked out",
        25 => "Account restricted",
        26 => "Session collision",
        27 => "License quota exceeded",
        28 => "Account expired - must change password",
        29 => "Unsupported encryption level",
        30 => "Account locked - multiple failed logon attempts",

        // Win32 socket errors
        53 => "Network path not found",

        // Winsock/Network errors (256+)
        260 => "DNS name lookup failure",
        261 => "TCP connect failed",
        262 => "Out of memory",
        263 => "Logon failed",
        264 => "Connection timed out",
        516 => "Windows Sockets connect failed",
        518 => "Out of memory",
        520 => "Host not found error",
        772 => "Windows Sockets send call failed",
        774 => "Out of memory",
        776 => "The IP address specified is not valid",
        1028 => "Windows Sockets recv call failed",

        // Security/Encryption errors
        1030 => "Security data is not valid",
        1032 => "Internal error",
        1286 => "The encryption method specified is not valid",
        1288 => "DNS lookup failed",
        1540 => "Windows Sockets gethostbyname call failed",
        1542 => "Server security data is not valid",
        1544 => "Internal timer error",
        1796 => "Time-out occurred",
        1798 => "Failed to unpack server certificate",
        2052 => "Bad IP address specified",

        // SSL/TLS Authentication errors
        2055 => "Login failed",
        2056 => "License negotiation failed",
        2308 => "Socket closed",
        2310 => "Internal security error",
        2312 => "Licensing time-out",
        2566 => "Internal security error",
        2567 => "The specified user has no account",
        2822 => "Encryption error",
        2823 => "The account is disabled",

        // Advanced security errors
        3078 => "Decryption error",
        3079 => "The account is restricted",
        3080 => "Decompression error",
        3335 => "The account is locked out",
        3591 => "The account is expired",
        3847 => "The password is expired",
        4615 => "The user password must be changed before logging on for the first time",
        5639 => "The policy does not support delegation of credentials to the target server",
        5895 => "Delegation of credentials to the target server is not allowed unless mutual authentication has been achieved",
        6151 => "No authority could be contacted for authentication. The domain name of the authenticating party could be wrong, the domain could be unreachable, or there might have been a trust relationship failure",
        6919 => "The received certificate is expired",
        7175 => "An incorrect PIN was presented to the smart card",
        8455 => "The server authentication policy does not allow connection requests using saved credentials. The user must enter new credentials",
        8711 => "The smart card is blocked",

        // Unknown codes
        _ => $"Unknown reason (code {reason})"
    };

    private void ShowDisconnectReason(int reason) {
        if(reason == 7943) return;

        string reasonText = GetDisconnectReasonText(reason);
        string hexCode = $"0x{reason:X}";
        MessageBox.Show(
            $"RDP session disconnected.\n\nReason: {reasonText}\nCode: {reason} ({hexCode})",
            "Disconnected",
            MessageBoxButtons.OK,
            reason < 10 ? MessageBoxIcon.Information : MessageBoxIcon.Warning
        );
    }

    private void ShowConnectionProgress() {
        _progressForm = new ConnectionProgressForm(ParseHost(_rdpFile.FullAddress)) {
            Owner = this
        };
        _progressForm.Show();
        _progressForm.Refresh();

        _connectionProgressTimer = new System.Windows.Forms.Timer { Interval = 500 };
        _connectionProgressTimer.Tick += ConnectionProgressTimer_Tick;
        _connectionProgressTimer.Start();
    }

    private void ConnectionProgressTimer_Tick(object? sender, EventArgs e) {
        if (_progressForm == null || _progressForm.IsDisposed)
            return;

        if (_rdpClient.Connected != 0) {
            _connectionProgressTimer?.Stop();
            CloseProgressForm();
        } else {
            UpdateProgress(50, "Establishing connection...");
        }
    }

    private void UpdateProgress(int percentage, string status) {
        if (_progressForm != null && !_progressForm.IsDisposed) {
            _progressForm.SetDeterminateProgress(percentage);
            _progressForm.Status = status;
            _progressForm.Refresh();
        }
    }

    private void CloseProgressForm() {
        if (_connectionProgressTimer != null) {
            _connectionProgressTimer.Stop();
            _connectionProgressTimer.Dispose();
            _connectionProgressTimer = null;
        }

        if (_progressForm != null && !_progressForm.IsDisposed) {
            _progressForm.Close();
            _progressForm.Dispose();
            _progressForm = null;
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);
    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, uint rop);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);
    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
    private const uint SRCCOPY = 0x00CC0020;

    private void TakeScreenshot() {
        try {
            if (_rdpClient.Connected == 0) {
                MessageBox.Show("RDP session is not connected.", "Screenshot",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var settings = UserSettings.Load();
            var screenshotPath = settings.ScreenshotPath;

            // Create directory if it doesn't exist
            if (!Directory.Exists(screenshotPath)) {
                Directory.CreateDirectory(screenshotPath);
            }

            // Capture the RDP client control using BitBlt (works with ActiveX controls)
            IntPtr srcDC = GetWindowDC(_rdpClient.Handle);
            IntPtr memDC = CreateCompatibleDC(srcDC);
            IntPtr hBitmap = CreateCompatibleBitmap(srcDC, _rdpClient.Width, _rdpClient.Height);
            IntPtr hOldBitmap = SelectObject(memDC, hBitmap);

            BitBlt(memDC, 0, 0, _rdpClient.Width, _rdpClient.Height, srcDC, 0, 0, SRCCOPY);

            SelectObject(memDC, hOldBitmap);
            Bitmap bitmap = Image.FromHbitmap(hBitmap);

            DeleteObject(hBitmap);
            DeleteDC(memDC);
            ReleaseDC(_rdpClient.Handle, srcDC);

            // Generate filename with timestamp
            var rdpFileName = Path.GetFileNameWithoutExtension(_rdpFile.FileName);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var filename = Path.Combine(screenshotPath, $"{rdpFileName}_{timestamp}.png");

            // Save the screenshot
            bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            bitmap.Dispose();

            MessageBox.Show($"Screenshot saved successfully.\n\nLocation: {filename}",
                "Screenshot", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } catch (Exception ex) {
            MessageBox.Show($"Failed to save screenshot:\n{ex.Message}",
                "Screenshot Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
