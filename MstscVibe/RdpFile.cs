using System.Security.Cryptography;
using System.Text;

namespace MstscVibe;

public class RdpFile {

    public string FileName { get; set; } = "";
    public string FullAddress { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int ScreenModeId { get; set; } = 1; // 1=windowed, 2=fullscreen
    public int DesktopWidth { get; set; } = 1024;
    public int DesktopHeight { get; set; } = 768;
    public int SessionBpp { get; set; } = 32;
    public int AudioMode { get; set; } = 0;
    public bool RedirectClipboard { get; set; } = true;
    public bool RedirectPrinters { get; set; } = false;
    public bool RedirectSmartCards { get; set; } = false;
    public bool RedirectDrives { get; set; } = false;
    public int AuthenticationLevel { get; set; } = 2;
    public bool EnableCredSSPSupport { get; set; } = true;
    public int GatewayUsageMethod { get; set; } = 0;
    public string GatewayHostname { get; set; } = "";
    public bool AdminSession { get; set; }
    public bool PublicMode { get; set; }
    public bool UseMultimon { get; set; }
    public bool PromptForCredentials { get; set; }
    public int? ShadowSessionId { get; set; }
    public bool ShadowControl { get; set; }
    public bool ShadowNoConsent { get; set; }

    private readonly Dictionary<string, (string type, string value)> _raw = new(StringComparer.OrdinalIgnoreCase);

    public static RdpFile Parse(string filePath) {
        var rdp = new RdpFile();

        rdp.FileName = Path.GetFileName(filePath);

        foreach (var line in File.ReadAllLines(filePath)) {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Format: key:type:value
            var firstColon = trimmed.IndexOf(':');
            if (firstColon < 0) continue;
            var secondColon = trimmed.IndexOf(':', firstColon + 1);
            if (secondColon < 0) continue;

            var key = trimmed[..firstColon].Trim();
            var type = trimmed[(firstColon + 1)..secondColon].Trim();
            var value = trimmed[(secondColon + 1)..].Trim();

            rdp._raw[key] = (type, value);
        }

        rdp.ApplyKnownSettings();
        return rdp;
    }

    private void ApplyKnownSettings() {
        FullAddress = GetString("full address", FullAddress);
        Username = GetString("username", Username);
        Password = DecryptPassword();
        ScreenModeId = GetInt("screen mode id", ScreenModeId);
        DesktopWidth = GetInt("desktopwidth", DesktopWidth);
        DesktopHeight = GetInt("desktopheight", DesktopHeight);
        SessionBpp = GetInt("session bpp", SessionBpp);
        AudioMode = GetInt("audiomode", AudioMode);
        RedirectClipboard = GetInt("redirectclipboard", RedirectClipboard ? 1 : 0) == 1;
        RedirectPrinters = GetInt("redirectprinters", RedirectPrinters ? 1 : 0) == 1;
        RedirectSmartCards = GetInt("redirectsmartcards", RedirectSmartCards ? 1 : 0) == 1;
        RedirectDrives = GetInt("redirectdrives", RedirectDrives ? 1 : 0) == 1;
        AuthenticationLevel = GetInt("authentication level", AuthenticationLevel);
        EnableCredSSPSupport = GetInt("enablecredsspsupport", EnableCredSSPSupport ? 1 : 0) == 1;
        GatewayUsageMethod = GetInt("gatewayusagemethod", GatewayUsageMethod);
        GatewayHostname = GetString("gatewayhostname", GatewayHostname);
    }

    private string GetString(string key, string defaultValue) {
        return _raw.TryGetValue(key, out var entry) && entry.type == "s" ? entry.value : defaultValue;
    }

    private int GetInt(string key, int defaultValue) {
        return _raw.TryGetValue(key, out var entry) && entry.type == "i" && int.TryParse(entry.value, out var v) ? v : defaultValue;
    }

    private string DecryptPassword() {
        // RDP files store passwords as "password 51:b:<hex>" encrypted with DPAPI
        if (!_raw.TryGetValue("password 51", out var entry) || entry.type != "b")
            return Password;

        try {
            // Strip any whitespace and ensure even length (mstsc tolerates trailing chars)
            var hex = new string(entry.value.Where(c => Uri.IsHexDigit(c)).ToArray());
            if (hex.Length % 2 != 0)
                hex = hex[..^1];
            if (hex.Length == 0)
                return Password;

            var encrypted = Convert.FromHexString(hex);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            // RDP passwords are stored as UTF-16LE with a null terminator
            var password = Encoding.Unicode.GetString(decrypted);
            return password.TrimEnd('\0');
        } catch (Exception ex) {
            MessageBox.Show(
                $"Failed to decrypt password from RDP file:\n{ex.Message}",
                "Password Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return Password;
        }
    }

    public IReadOnlyDictionary<string, (string type, string value)> RawSettings => _raw;
}
