namespace MstscVibe;

public class CommandLineOptions {
    public string? Server { get; set; }
    public bool Admin { get; set; }
    public bool FullScreen { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool Public { get; set; }
    public bool MultiMon { get; set; }
    public bool Prompt { get; set; }
    public int? ShadowSessionId { get; set; }
    public bool Control { get; set; }
    public bool NoConsentPrompt { get; set; }
    public bool ShowHelp { get; set; }
    public string? RdpFilePath { get; set; }

    public static CommandLineOptions Parse(string[] args) {
        var opts = new CommandLineOptions();

        for (int i = 0; i < args.Length; i++) {
            var arg = args[i];

            if (arg.Equals("/?") || arg.Equals("/help", StringComparison.OrdinalIgnoreCase)) {
                opts.ShowHelp = true;
                return opts;
            } else if (arg.StartsWith("/v:", StringComparison.OrdinalIgnoreCase)) {
                opts.Server = arg[3..];
            } else if (arg.Equals("/admin", StringComparison.OrdinalIgnoreCase)) {
                opts.Admin = true;
            } else if (arg.Equals("/f", StringComparison.OrdinalIgnoreCase)) {
                opts.FullScreen = true;
            } else if (arg.StartsWith("/w:", StringComparison.OrdinalIgnoreCase)) {
                if (int.TryParse(arg[3..], out var w)) opts.Width = w;
            } else if (arg.StartsWith("/h:", StringComparison.OrdinalIgnoreCase)) {
                if (int.TryParse(arg[3..], out var h)) opts.Height = h;
            } else if (arg.Equals("/public", StringComparison.OrdinalIgnoreCase)) {
                opts.Public = true;
            } else if (arg.Equals("/multimon", StringComparison.OrdinalIgnoreCase)) {
                opts.MultiMon = true;
            } else if (arg.Equals("/prompt", StringComparison.OrdinalIgnoreCase)) {
                opts.Prompt = true;
            } else if (arg.StartsWith("/shadow:", StringComparison.OrdinalIgnoreCase)) {
                if (int.TryParse(arg[8..], out var sid)) opts.ShadowSessionId = sid;
            } else if (arg.Equals("/control", StringComparison.OrdinalIgnoreCase)) {
                opts.Control = true;
            } else if (arg.Equals("/noConsentPrompt", StringComparison.OrdinalIgnoreCase)) {
                opts.NoConsentPrompt = true;
            } else if (arg.EndsWith(".rdp", StringComparison.OrdinalIgnoreCase) && File.Exists(arg)) {
                opts.RdpFilePath = arg;
            }
        }

        return opts;
    }

    public void ApplyTo(RdpFile rdp) {
        if (!string.IsNullOrEmpty(Server))
            rdp.FullAddress = Server;
        if (FullScreen)
            rdp.ScreenModeId = 2;
        if (Width.HasValue)
            rdp.DesktopWidth = Width.Value;
        if (Height.HasValue)
            rdp.DesktopHeight = Height.Value;
        if (Admin)
            rdp.AdminSession = true;
        if (Public)
            rdp.PublicMode = true;
        if (MultiMon)
            rdp.UseMultimon = true;
        if (Prompt)
            rdp.PromptForCredentials = true;
        if (ShadowSessionId.HasValue) {
            rdp.ShadowSessionId = ShadowSessionId.Value;
            rdp.ShadowControl = Control;
            rdp.ShadowNoConsent = NoConsentPrompt;
        }
    }

    public bool HasConnectionTarget => !string.IsNullOrEmpty(Server) || !string.IsNullOrEmpty(RdpFilePath);

    public static void ShowHelpMessage() {
        var help = """
            MstscVibe - Remote Desktop Connection

            Usage: MstscVibe [<file.rdp>] [options]

            Options:
              /v:<server[:port]>    Specifies the remote PC to connect to.
              /admin                Connects to the admin session.
              /f                    Starts Remote Desktop in full-screen mode.
              /w:<width>            Specifies the width of the Remote Desktop window.
              /h:<height>           Specifies the height of the Remote Desktop window.
              /public               Runs Remote Desktop in public mode (no caching).
              /multimon             Configures the session to use the client multi-monitor layout.
              /prompt               Prompts for credentials before connecting.
              /shadow:<sessionID>   Specifies the session ID to shadow.
              /control              Allows control of the session when shadowing.
              /noConsentPrompt      Allows shadowing without user consent.
              /?                    Displays this help message.
              /help                 Displays this help message.

            Examples:
              MstscVibe /v:myserver
              MstscVibe /v:myserver:3390 /admin /f
              MstscVibe /v:myserver /w:1920 /h:1080
              MstscVibe connection.rdp /f
              MstscVibe /shadow:2 /control /noConsentPrompt
            """;
        MessageBox.Show(help.Replace("            ", ""), "MstscVibe - Help",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
