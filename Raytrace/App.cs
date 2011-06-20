using System;
using System.Threading;

namespace Raytrace
{
	public class App
	{
		public static int Main (string[] args)
		{
			//var serverUrl = 
			//if (args.Length == 0 || args[0] == "serve") {
			{
				var scene = new Scene ();
				scene.Camera.Position = new Vec (0, 0, 10);
				scene.Camera.Direction = new Vec (0, 0, -1);
				scene.Objects.Add (new SphereObj() {
					Center = new Vec (0, 0, 0),
					Radius = 5,
				});
				new Server (scene, 1920, 1080).Run ();				
			}
			
			//else {
				new Client (args[0]).Run ();
			//}
			return 0;
		}
	}
}

