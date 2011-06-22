using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Raytrace
{
	/// <summary>
	/// This app is a distributed path tracer based on
	/// <a href="http://www.kevinbeason.com/smallpt/">smallpt</a>.
	/// </summary>
	public class App
	{
		public static int Main (string[] args)
		{
			//
			// Configure
			//
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
			
			//
			// If we are a server,
			//   create a scene
			//   start the server
			//   show the browser so people can watch
			//
			if (runServer) {
				var scene = new Scene () {
					ImageWidth = 1920,
					ImageHeight = 1080,
					SamplesPerPixel = 8,
				};
				scene.Camera.Origin = new Vec (50, 52, 295.6);
				scene.Camera.Direction = new Vec (0, -0.042612, -1).Norm;
				scene.Objects.AddRange (new Sphere[] {
					new Sphere (1e5, new Vec( 1e5+1,40.8,81.6), Vec.Zero,new Vec(.75,.25,.25),MaterialType.Diffuse),//Left
					new Sphere (1e5, new Vec(-1e5+99,40.8,81.6),Vec.Zero,new Vec(.25,.25,.75),MaterialType.Diffuse),//Rght
					new Sphere (1e5, new Vec(50,40.8, 1e5),     Vec.Zero,new Vec(.75,.75,.75),MaterialType.Diffuse),//Back
					new Sphere (1e5, new Vec(50,40.8,-1e5+170), Vec.Zero,new Vec(),           MaterialType.Diffuse),//Frnt
					new Sphere (1e5, new Vec(50, 1e5, 81.6),    Vec.Zero,new Vec(.75,.75,.75),MaterialType.Diffuse),//Botm
					new Sphere (1e5, new Vec(50,-1e5+81.6,81.6),Vec.Zero,new Vec(.75,.75,.75),MaterialType.Diffuse),//Top
					new Sphere (16.5,new Vec(27,16.5,47),       Vec.Zero,new Vec(1,1,1)*.999, MaterialType.Specular),//Mirr
					new Sphere (16.5,new Vec(73,16.5,78),       Vec.Zero,new Vec(1,1,1)*.999, MaterialType.Transmissive),//Glas
					new Sphere (600, new Vec(50,681.6-.27,81.6),new Vec(12,12,12),  Vec.Zero, MaterialType.Diffuse) //Lite
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
			
			//
			// Create clients on their own threads
			//
			var threads = new List<Thread> ();
			for (var i = 0; i < numClients; i++) {
				var th = new Thread ((ThreadStart)delegate {
					new Client (serverUrl).Run ();
				});
				threads.Add (th);
				th.Start ();
			}
			
			//
			// Twiddle thumbs while server + clients do the work
			//
			Console.WriteLine ("Press Enter to stop...");
			Console.ReadLine ();
			foreach (var t in threads) t.Abort ();
			
			return 0;
		}
	}
}

