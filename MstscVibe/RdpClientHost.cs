using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MstscVibe;

public class RdpClientHost : AxHost {
    private const string RDP_CLIENT_CLSID = "{8B918B82-7985-4C24-89DF-C33AD2BBFBCD}";

    public RdpClientHost() : base(RDP_CLIENT_CLSID) { }

    private dynamic? Ocx => GetOcx() as dynamic;

    public event EventHandler? RequestMinimize;
    public event EventHandler? RequestLeaveFullScreen;
    public event EventHandler<int>? Disconnected;

    private AxHost.ConnectionPointCookie? _sinkCookie;
    private RdpEventSink? _sink;

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

    public void SendScanCode(uint scanCode, bool keyUp) {
        var ocx = GetOcx();
        if (ocx == null) return;
        var nonScriptable = (IMsRdpClientNonScriptable)ocx;
        if(nonScriptable == null) 
            return;
        nonScriptable.SendKeys(1, new bool[] { keyUp }, new int[] { (int)scanCode });
    }

    public void SendKeys(int numKeys, bool[] keyUp, int[] scanCodes) {
        const int MAX_KEYS = 256; // Adjust based on testing/documentation

        if(numKeys > MAX_KEYS)
            throw new ArgumentException($"Maximum {MAX_KEYS} keys per call", nameof(numKeys));

        if(keyUp.Length != numKeys || scanCodes.Length != numKeys)
            throw new ArgumentException("Array lengths must match numKeys");

        var ocx = GetOcx();
        if(ocx == null) return;

        var nonScriptable = (IMsRdpClientNonScriptable)ocx;
        if(nonScriptable == null) return;

        nonScriptable.SendKeys(numKeys, keyUp, scanCodes);
    }

    public void Reconnect(int width, int height) {
        try {
            Ocx?.UpdateSessionDisplaySettings((uint)width, (uint)height, (uint)width, (uint)height, 0u, 100u, 100u);
        } catch {
            try { Ocx?.Reconnect((uint)width, (uint)height); } catch { }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool SmartSizing {
        get => Ocx?.AdvancedSettings9?.SmartSizing ?? false;
        set { var adv = Ocx?.AdvancedSettings9; if (adv != null) adv.SmartSizing = value; }
    }

    protected override void AttachInterfaces() { }

    protected override void CreateSink() {
        try {
            _sink = new RdpEventSink(this);
			Object? ocx = GetOcx();
            if(ocx == null) return;
			_sinkCookie = new ConnectionPointCookie(ocx, _sink, typeof(IMsTscAxEvents));
        } catch { }
    }

    protected override void DetachSink() {
        try {
            _sinkCookie?.Disconnect();
        } catch { }
        _sinkCookie = null;
        _sink = null;
    }

    internal void FireRequestMinimize() => RequestMinimize?.Invoke(this, EventArgs.Empty);
    internal void FireRequestLeaveFullScreen() => RequestLeaveFullScreen?.Invoke(this, EventArgs.Empty);
    internal void FireDisconnected(int reason) => Disconnected?.Invoke(this, reason);

    // The source event interface for the RDP ActiveX control
    [ComImport, Guid("336D5562-EFA8-482E-8CB3-C5C0FC7A7DB6"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
    public interface IMsTscAxEvents {
        [DispId(1)] void OnConnecting();
        [DispId(2)] void OnConnected();
        [DispId(3)] void OnLoginComplete();
        [DispId(4)] void OnDisconnected([In] int discReason);
        [DispId(5)] void OnEnterFullScreenMode();
        [DispId(6)] void OnLeaveFullScreenMode();
        [DispId(7)] void OnChannelReceivedData([In, MarshalAs(UnmanagedType.BStr)] string chanName, [In, MarshalAs(UnmanagedType.BStr)] string data);
        [DispId(8)] void OnRequestGoFullScreen();
        [DispId(9)] void OnRequestLeaveFullScreen();
        [DispId(10)] void OnFatalError([In] int errorCode);
        [DispId(11)] void OnWarning([In] int warningCode);
        [DispId(12)] void OnRemoteDesktopSizeChange([In] int width, [In] int height);
        [DispId(13)] void OnIdleTimeoutNotification();
        [DispId(14)] void OnRequestContainerMinimize();
        [DispId(15)] void OnConfirmClose([Out, MarshalAs(UnmanagedType.VariantBool)] out bool pfAllowClose);
        [DispId(16)] void OnReceivedTSPublicKey([In, MarshalAs(UnmanagedType.BStr)] string publicKey, [Out, MarshalAs(UnmanagedType.VariantBool)] out bool pfContinueLogon);
        [DispId(17)] void OnAutoReconnecting([In] int disconnectReason, [In] int attemptCount, [Out] out int pContinueStatus);
        [DispId(18)] void OnAuthenticationWarningDisplayed();
        [DispId(19)] void OnAuthenticationWarningDismissed();
        [DispId(20)] void OnRemoteProgramResult([In, MarshalAs(UnmanagedType.BStr)] string bstrRemoteProgram, [In] int lError, [In, MarshalAs(UnmanagedType.VariantBool)] bool vbIsExecutable);
        [DispId(21)] void OnRemoteProgramDisplayed([In, MarshalAs(UnmanagedType.VariantBool)] bool vbDisplayed, [In] int uDisplayInformation);
        [DispId(22)] void OnRemoteWindowDisplayed([In, MarshalAs(UnmanagedType.VariantBool)] bool vbDisplayed, [In] IntPtr hwnd, [In] int windowAttribute);
        [DispId(23)] void OnLogonError([In] int lError);
        [DispId(24)] void OnFocusReleased([In] int iDirection);
        [DispId(25)] void OnUserNameAcquired([In, MarshalAs(UnmanagedType.BStr)] string bstrUserName);
        [DispId(26)] void OnMouseInputModeChanged([In, MarshalAs(UnmanagedType.VariantBool)] bool fMouseModeRelative);
        [DispId(27)] void OnServiceMessageReceived([In, MarshalAs(UnmanagedType.BStr)] string serviceMessage);
        [DispId(28)] void OnConnectionBarPullDown();
        [DispId(29)] void OnNetworkStatusChanged([In] int qualityLevel, [In] int bandwidth, [In] int rtt);
        [DispId(30)] void OnDevicesButtonPressed();
        [DispId(31)] void OnAutoReconnected();
        [DispId(32)] void OnAutoReconnecting2([In] int disconnectReason, [In, MarshalAs(UnmanagedType.VariantBool)] bool networkAvailable, [In] int attemptCount, [In] int maxAttemptCount);
    }

    [ClassInterface(ClassInterfaceType.None)]
    private class RdpEventSink : IMsTscAxEvents {
        private readonly RdpClientHost _host;
        public RdpEventSink(RdpClientHost host) => _host = host;

        public void OnConnecting() { }
        public void OnConnected() { }
        public void OnLoginComplete() { }
        public void OnDisconnected(int discReason) => _host.FireDisconnected(discReason);
        public void OnEnterFullScreenMode() { }
        public void OnLeaveFullScreenMode() { }
        public void OnChannelReceivedData(string chanName, string data) { }
        public void OnRequestGoFullScreen() { }
        public void OnRequestLeaveFullScreen() => _host.FireRequestLeaveFullScreen();
        public void OnFatalError(int errorCode) { }
        public void OnWarning(int warningCode) { }
        public void OnRemoteDesktopSizeChange(int width, int height) { }
        public void OnIdleTimeoutNotification() { }
        public void OnRequestContainerMinimize() => _host.FireRequestMinimize();
        public void OnConfirmClose(out bool pfAllowClose) { pfAllowClose = true; }
        public void OnReceivedTSPublicKey(string publicKey, out bool pfContinueLogon) { pfContinueLogon = true; }
        public void OnAutoReconnecting(int disconnectReason, int attemptCount, out int pContinueStatus) { pContinueStatus = 0; }
        public void OnAuthenticationWarningDisplayed() { }
        public void OnAuthenticationWarningDismissed() { }
        public void OnRemoteProgramResult(string bstrRemoteProgram, int lError, bool vbIsExecutable) { }
        public void OnRemoteProgramDisplayed(bool vbDisplayed, int uDisplayInformation) { }
        public void OnRemoteWindowDisplayed(bool vbDisplayed, IntPtr hwnd, int windowAttribute) { }
        public void OnLogonError(int lError) { }
        public void OnFocusReleased(int iDirection) { }
        public void OnUserNameAcquired(string bstrUserName) { }
        public void OnMouseInputModeChanged(bool fMouseModeRelative) { }
        public void OnServiceMessageReceived(string serviceMessage) { }
        public void OnConnectionBarPullDown() { }
        public void OnNetworkStatusChanged(int qualityLevel, int bandwidth, int rtt) { }
        public void OnDevicesButtonPressed() { }
        public void OnAutoReconnected() { }
        public void OnAutoReconnecting2(int disconnectReason, bool networkAvailable, int attemptCount, int maxAttemptCount) { }
    }

    [ComImport, Guid("2F079C4C-87B2-4AFD-97AB-20CDB43038AE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMsRdpClientNonScriptable {
        // IMsTscNonScriptable methods
        void put_ClearTextPassword([MarshalAs(UnmanagedType.BStr)] string clearTextPassword);
        void put_PortablePassword([MarshalAs(UnmanagedType.BStr)] string portablePassword);
        void get_PortablePassword([MarshalAs(UnmanagedType.BStr)] out string portablePassword);
        void put_PortableSalt([MarshalAs(UnmanagedType.BStr)] string portableSalt);
        void get_PortableSalt([MarshalAs(UnmanagedType.BStr)] out string portableSalt);
        void put_BinaryPassword([MarshalAs(UnmanagedType.BStr)] string binaryPassword);
        void get_BinaryPassword([MarshalAs(UnmanagedType.BStr)] out string binaryPassword);
        void put_BinarySalt([MarshalAs(UnmanagedType.BStr)] string binarySalt);
        void get_BinarySalt([MarshalAs(UnmanagedType.BStr)] out string binarySalt);
        void ResetPassword();
        // IMsRdpClientNonScriptable methods
        void NotifyRedirectDeviceChange(IntPtr wParam, IntPtr lParam);
        void SendKeys(int numKeys, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.VariantBool)] bool[] pbArrayKeyUp, [MarshalAs(UnmanagedType.LPArray)] int[] plKeyData);
    }
}
