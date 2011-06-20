using System;
using System.Net;

namespace Raytrace
{
	public class Client
	{
		string host;
		
		public Client (string host)
		{
			this.host = host;
		}
		
		public void Run ()
		{
			for (;;) {
				try {
					var w = default(Work);
					var req = WebRequest.Create (host + "work");
					using (var resp = req.GetResponse ()) {
						using (var s = resp.GetResponseStream ()) {
							w = Work.Open (s);
						}
					}
					
					System.Console.WriteLine ("Working on " + w.Id);
					
					var r = new Random ();
					var pb = new PixelBuffer (w.Width, w.Height);
					var col = new Vec(r.NextDouble (),r.NextDouble (),r.NextDouble ());
					for (var py = 0; py < w.Height; py++) {
						for (var px = 0; px < w.Width; px++) {
							pb.PutPixel (px, py, col);
						}
					}
					
					req = WebRequest.Create (host + "work/" + w.Id + "/pixels");
					req.Method = "POST";
					req.ContentType = "octet";
					using (var s = req.GetRequestStream ()) {
						pb.SaveBinary (s);
					}
					using (var resp = req.GetResponse ()) {
					}
				}
				catch (WebException ex) {
					if (ex.Status == WebExceptionStatus.ProtocolError && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound) {
						System.Console.WriteLine ("Server says we're finished");
						System.Threading.Thread.Sleep (2000);
					}
					if (ex.Status == WebExceptionStatus.ConnectFailure) {
						System.Console.WriteLine ("waiting for server {0}...", host);
						System.Threading.Thread.Sleep (2000);
					}
					else {
						System.Console.WriteLine ("! {0}: {1}", ex.GetType ().Name, ex.Message);
						System.Threading.Thread.Sleep (50);
					}
				}
			}
		}
	}
}

