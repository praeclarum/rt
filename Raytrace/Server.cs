using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace Raytrace
{
	public class Server
	{
		HttpListener listener;
		int port;
		
		Scene scene;
		
		List<Work> work;
		
		public Server (int port, Scene sc)
		{
			this.port = port;
			
			finalBuffer = new PixelBuffer(sc.ImageWidth, sc.ImageHeight);
			work = new List<Work>();
			scene = sc;
			
			var workNumX = 10;
			var workNumY = 10;
			var workWidth = sc.ImageWidth / workNumX;
			var workHeight = sc.ImageHeight / workNumY;
			
			for (var yi = 0; yi < workNumY; yi++) {
				for (var xi = 0; xi < workNumX; xi++) {
					var x = xi * workWidth;
					var y = yi * workHeight;
					work.Add (new Work() {
						Id = work.Count,
						Finished = false,
						Scene = scene,
						Width = workWidth,
						Height = workHeight,
						X = x,
						Y = y,
					});
				}
			}
		}
		
		PixelBuffer finalBuffer;
		
		Random rand = new Random();
		
		public void Run ()
		{			
			listener = new HttpListener ();
			var p = "http://+:" + port + "/";
			listener.Prefixes.Add (p);
			listener.Start ();
			System.Console.WriteLine ("Serving {0}...", p);
			listener.BeginGetContext (OnContext, null);
		}
		
		DateTime? firstBlockTime;
	
		void OnContext (IAsyncResult ar) {
			try {
				var ctx = listener.EndGetContext (ar);
				var path = ctx.Request.Url.LocalPath;
				
				System.Console.WriteLine ("{0} {1}", ctx.Request.HttpMethod, path);
				
				if (ctx.Request.HttpMethod == "GET") {
					if (path == "/work") {
						var idx = rand.Next () % work.Count;
						var w = work[idx];
						if (w.Finished || w.HandedOut) {
							w = (from ww in work where !ww.Finished && !ww.HandedOut select ww).FirstOrDefault ();
						}
						
						if (w != null) {
							w.Finished = false;
							w.HandedOut = true;
							ctx.Response.StatusCode = 200;
							ctx.Response.ContentType = "text/xml";
							w.Save (ctx.Response.OutputStream);
							ctx.Response.Close ();
						}
						else {
							ctx.Response.StatusCode = 404;
							ctx.Response.Close ();
						}
					}
					else if (path == "/img" || path == "/favicon.ico") {
						ctx.Response.StatusCode = 200;
						ctx.Response.ContentType = "image/png";						
						finalBuffer.SavePng (ctx.Response.OutputStream);
						ctx.Response.Close ();
					}
					else if (path == "/") {
						ctx.Response.StatusCode = 200;
						ctx.Response.ContentType = "text/html";
						using (var w = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8)) {
							w.WriteLine ("<!DOCTYPE html>");
							w.WriteLine ("<html><head><title>Frank's Distributed Raytracer</title>");
							w.WriteLine ("</head><body style='font-family:sans-serif;'>");
							w.WriteLine ("<h1>Frank's Distributed Raytracer</h1>");
							w.WriteLine ("<img src='/img' style='max-width:640px'>");
							w.WriteLine ("<h2>Progress</h2>");
							var numFini = (from ww in work where ww.Finished select ww).Count();
							var p = (numFini * 100) / work.Count;
							w.WriteLine ("<p><strong>{0} of {1}</strong> blocks complete ({2}%)</p>", numFini, work.Count, p);
							var mins = 0.0;
							var pps = 0.0;
							var t = DateTime.UtcNow;
							if (firstBlockTime.HasValue) {
								var dt = t - firstBlockTime.Value;
								pps = (numFini-1) * work[0].Width*work[0].Height / dt.TotalSeconds;
								var tt = (dt.TotalMinutes * 100) / p;
								mins = tt - dt.TotalMinutes;
							}
							w.WriteLine ("<p><strong>{0}</strong> mins remaining ({1})</p>", (int)(mins + 0.5), DateTime.Now.AddMinutes (mins));
							w.WriteLine ("<p><strong>{0}</strong> pixels / sec</p>", (int)pps);
							w.WriteLine ("<h2>Scene</h2>");
							w.WriteLine ("<code><pre>");
							w.WriteLine (HtmlEscape (scene.SaveXml ()));
							w.WriteLine ("</pre></code>");
							w.WriteLine ("</body></html>");
						}
						ctx.Response.Close ();
					}
					else {
						ctx.Response.StatusCode = 404;
						ctx.Response.Close ();
					}
				}
				else {
					if (path.StartsWith ("/work/") && path.EndsWith ("/pixels")) {
						var wid = int.Parse (path.Split ('/')[2]);
						var w = work[wid];
						var pb = PixelBuffer.OpenBinary (ctx.Request.InputStream);						
						finalBuffer.SetPixels (w.X, w.Y, pb);
						w.Finished = true;
						if (!firstBlockTime.HasValue) {
							firstBlockTime = DateTime.UtcNow;
						}
						ctx.Response.StatusCode = 200;
						ctx.Response.ContentType = "text/plain";
						ctx.Response.Close ();
					}
					else {
						ctx.Response.StatusCode = 404;
						ctx.Response.Close ();
					}
				}
			}
			catch (Exception ex) {
				System.Console.WriteLine ("!! {0}: {1}", ex.GetType ().Name, ex.Message);
				throw;
			}
			finally {
				listener.BeginGetContext (OnContext, null);
			}
		}
		
		static string HtmlEscape (string s) {
			return s.Replace ("&", "&amp;").Replace ("<", "&lt;").Replace (">", "&gt;");
		}
	}
}

