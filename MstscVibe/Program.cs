namespace MstscVibe {
	internal static class Program {
		[STAThread]
		static void Main(string[] args) {
			ApplicationConfiguration.Initialize();

			var opts = CommandLineOptions.Parse(args);

			if (opts.ShowHelp) {
				CommandLineOptions.ShowHelpMessage();
				return;
			}

			RdpFile? rdpFile = null;
			if (!string.IsNullOrEmpty(opts.RdpFilePath))
				rdpFile = RdpFile.Parse(opts.RdpFilePath);

			if (rdpFile != null || opts.HasConnectionTarget) {
				rdpFile ??= new RdpFile();
				opts.ApplyTo(rdpFile);

				if (opts.Prompt) {
					// Show the form to let the user enter credentials
					var form = new Form1(rdpFile);
					Application.Run(form);
				} else {
					// Direct connect
					var session = new SessionForm(rdpFile);
					Application.Run(session);
				}
			} else {
				Application.Run(new Form1());
			}
		}
	}
}