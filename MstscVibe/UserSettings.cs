using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MstscVibe;

public class UserSettings {
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MstscVibe", "settings.json");

    public string LastComputer { get; set; } = "";
    public string LastUsername { get; set; } = "";
    public bool RememberPassword { get; set; }
    public string ProtectedPassword { get; set; } = "";
    public int DesktopWidth { get; set; } = 1024;
    public int DesktopHeight { get; set; } = 768;
    public int SessionBpp { get; set; } = 32;
    public int ScreenModeId { get; set; } = 1;
    public int AudioMode { get; set; } = 0;
    public bool RedirectClipboard { get; set; } = true;
    public bool RedirectPrinters { get; set; }
    public bool RedirectDrives { get; set; }
    public bool RedirectSmartCards { get; set; }

    public static UserSettings Load() {
        try {
            if (File.Exists(SettingsPath)) {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
            }
        } catch {
            // Fall through to defaults
        }
        return new UserSettings();
    }

    public void Save() {
        try {
            var dir = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        } catch {
            // Best effort
        }
    }

    public void ApplyTo(RdpFile rdp) {
        rdp.DesktopWidth = DesktopWidth;
        rdp.DesktopHeight = DesktopHeight;
        rdp.SessionBpp = SessionBpp;
        rdp.ScreenModeId = ScreenModeId;
        rdp.AudioMode = AudioMode;
        rdp.RedirectClipboard = RedirectClipboard;
        rdp.RedirectPrinters = RedirectPrinters;
        rdp.RedirectDrives = RedirectDrives;
        rdp.RedirectSmartCards = RedirectSmartCards;
    }

    public void CopyFrom(RdpFile rdp) {
        DesktopWidth = rdp.DesktopWidth;
        DesktopHeight = rdp.DesktopHeight;
        SessionBpp = rdp.SessionBpp;
        ScreenModeId = rdp.ScreenModeId;
        AudioMode = rdp.AudioMode;
        RedirectClipboard = rdp.RedirectClipboard;
        RedirectPrinters = rdp.RedirectPrinters;
        RedirectDrives = rdp.RedirectDrives;
        RedirectSmartCards = rdp.RedirectSmartCards;
    }

    public string GetPassword() {
        if (string.IsNullOrEmpty(ProtectedPassword)) return "";
        try {
            var encrypted = Convert.FromBase64String(ProtectedPassword);
            var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        } catch {
            return "";
        }
    }

    public void SetPassword(string password) {
        if (string.IsNullOrEmpty(password)) {
            ProtectedPassword = "";
            return;
        }
        try {
            var bytes = Encoding.UTF8.GetBytes(password);
            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            ProtectedPassword = Convert.ToBase64String(encrypted);
        } catch {
            ProtectedPassword = "";
        }
    }
}
