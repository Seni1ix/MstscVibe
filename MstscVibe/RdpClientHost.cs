using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MstscVibe;

public class RdpClientHost : AxHost {
    // MsRdpClient9NotSafeForScripting CLSID
    private const string RDP_CLIENT_CLSID = "{8B918B82-7985-4C24-89DF-C33AD2BBFBCD}";

    public RdpClientHost() : base(RDP_CLIENT_CLSID) { }

    private dynamic? Ocx => GetOcx() as dynamic;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Server { get => Ocx?.Server ?? ""; set { if (Ocx != null) Ocx.Server = value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string UserName { get => Ocx?.UserName ?? ""; set { if (Ocx != null) Ocx.UserName = value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int DesktopWidth { get => Ocx?.DesktopWidth ?? 1024; set { if (Ocx != null) Ocx.DesktopWidth = value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int DesktopHeight { get => Ocx?.DesktopHeight ?? 768; set { if (Ocx != null) Ocx.DesktopHeight = value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int ColorDepth { get => Ocx?.ColorDepth ?? 32; set { if (Ocx != null) Ocx.ColorDepth = value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FullScreen { get => Ocx?.FullScreen ?? false; set { if (Ocx != null) Ocx.FullScreen = value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Connected => Ocx?.Connected ?? 0;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public dynamic? AdvancedSettings => Ocx?.AdvancedSettings9;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public dynamic? SecuredSettings => Ocx?.SecuredSettings3;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public dynamic? TransportSettings => Ocx?.TransportSettings3;

    public void Connect() => Ocx?.Connect();
    public void Disconnect() => Ocx?.Disconnect();

    public void Reconnect(int width, int height) {
        try {
            // UpdateSessionDisplaySettings resizes without disconnecting (RDP 8+)
            Ocx?.UpdateSessionDisplaySettings((uint)width, (uint)height, (uint)width, (uint)height, 0u, 100u, 100u);
        } catch {
            // Fall back to full reconnect on older servers
            try {
                Ocx?.Reconnect((uint)width, (uint)height);
            } catch {
                // Not supported; SmartSizing will handle scaling
            }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool SmartSizing {
        get => Ocx?.AdvancedSettings9?.SmartSizing ?? false;
        set { var adv = Ocx?.AdvancedSettings9; if (adv != null) adv.SmartSizing = value; }
    }

    protected override void AttachInterfaces() { }
    protected override void CreateSink() { }
    protected override void DetachSink() { }
}
