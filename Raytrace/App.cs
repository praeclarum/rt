using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Raytrace
{
	public class App
	{
		public static int Main (string[] args)
		{
			int port = 8082;
			
			string serverUrl = "http://127.0.0.1:" + port + "/";
			
			bool runServer = false;
			int numClients = Environment.ProcessorCount;
			
			if (args.Length == 0) {
				runServer = true;
			}
			else if (args[0] == "serve") {				
				runServer = true;
				numClients = 0;
				if (args.Length > 1) {
					try {
						port = int.Parse (args[1]);
						serverUrl = "http://127.0.0.1:" + port + "/";
					} catch (Exception) {}
				}
			}
			else {
				if (args.Length > 0) {
					serverUrl = args[1];
				}				
			}
			
			if (runServer) {
				var scene = new Scene () {
					ImageWidth = 1920,
					ImageHeight = 1080,
				};
				scene.Camera.Position = new Vec (0, 0, 10);
				scene.Camera.Direction = new Vec (0, 0, -1);
				scene.Objects.Add (new SphereObj() {
					Center = new Vec (0, 0, 0),
					Radius = 5,
				});
				new Server (port, scene).Run ();
				try {
					Process.Start (new ProcessStartInfo() {
						Arguments = serverUrl,
						FileName = "open",
						UseShellExecute = true
					});
				} catch (Exception) {}
			}
			
			var threads = new List<Thread> ();
			for (var i = 0; i < numClients; i++) {
				var th = new Thread ((ThreadStart)delegate {
					new Client (serverUrl).Run ();
				});
				threads.Add (th);
				th.Start ();
			}
			
			Console.WriteLine ("Press Enter to stop...");
			Console.ReadLine ();
			foreach (var t in threads) t.Abort ();
			
			return 0;
		}
	}
}

