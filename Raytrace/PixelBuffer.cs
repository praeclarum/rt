using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Serialization;

namespace Raytrace
{
	[Serializable]
	public class PixelBuffer
	{
		public int Width;
		public int Height;
		public Vec[] Buffer;
		
		public PixelBuffer ()
		{
		}
		
		public PixelBuffer (int w, int h)
		{
			Width = w;
			Height = h;
			Buffer = new Vec[w * h];
		}
		
		public Vec Get (int x, int y)
		{
			return Buffer [(Height - 1 - y)*Width + x];
		}

		public void PutPixel (int x, int y, Vec color)
		{
			Buffer [(Height - 1 - y)*Width + x] = color;
		}
		
		public void SetPixels (int pbx, int pby, PixelBuffer pb)
		{
			//Buffer [(Height - 1 - y)*Width + x] = color;
			for (var y = 0; y < pb.Height; y++) {
				for (var x = 0; x < pb.Width; x++) {
					Buffer [(Height - 1 - (y+pby))*Width + x + pbx] = pb.Get (x, y);
				}
			}
		}

		public void SavePng (string path = "/Users/fak/Desktop/Raytrace.png")
		{
			using (var s = File.OpenWrite (path)) {
				SavePng (s);
			}
		}
		public void SavePng (Stream stream)
		{
			using (var bmp = new Bitmap (Width, Height)) {
				var l = bmp.LockBits (new Rectangle (0,0,Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				var s0 = l.Scan0;
				for (var y = 0; y < Height; y++) {
					var s = new IntPtr ( s0.ToInt64 () + y * l.Stride);
					unsafe {
						var ar = (byte*)s;
						for (var x = 0; x < Width; x++) {
							var c = Buffer [y*Width + x];
							var r = 255 * c.Z;
							if (r > 255)
								r = 255;
							var g = 255 * c.Y;
							if (g > 255.0)
								g = 255;
							var b = 255 * c.X;
							if (b > 255)
								b = 255;
							var a = 255;// * c.W;
							if (a > 255)
								a = 255;
							*
							ar++ = (byte)r;
							*
							ar++ = (byte)g;
							*
							ar++ = (byte)b;
							*
							ar++ = (byte)a;
						}
					}
				}
				bmp.UnlockBits (l);
	
				bmp.Save (stream, ImageFormat.Png);
			}
		}
		
		public void SaveBinary (Stream s) {
			using (var w = new BinaryWriter (s)) {
				w.Write (Width);
				w.Write (Height);
				for (var i = 0; i < Width*Height; i++) {
					w.Write (Buffer[i].X);
					w.Write (Buffer[i].Y);
					w.Write (Buffer[i].Z);
				}
			}
		}
		
		public static PixelBuffer OpenBinary (Stream s) {
			using (var w = new BinaryReader (s)) {
				var width = w.ReadInt32 ();
				var height = w.ReadInt32 ();
				var pb = new PixelBuffer (width, height);
				for (var i = 0; i < width*height; i++) {
					pb.Buffer [i].X = w.ReadDouble ();
					pb.Buffer [i].Y = w.ReadDouble ();
					pb.Buffer [i].Z = w.ReadDouble ();
				}
				return pb;
			}
		}

		public void SaveXml (Stream w) {
			var s = new XmlSerializer(typeof(PixelBuffer));
			s.Serialize (w, this);
		}
		
		public static PixelBuffer OpenXml (Stream r) {
			var s = new XmlSerializer(typeof(PixelBuffer));
			return (PixelBuffer)s.Deserialize (r);
		}
	}
}

