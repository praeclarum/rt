using System;
using System.IO;
using System.Xml.Serialization;

using Prec = System.Double;

namespace Raytrace
{
	[Serializable]
	public class Work
	{
		[XmlAttribute]
		public int Id;
		
		[XmlAttribute]
		public bool Finished;
		
		[XmlAttribute]
		public bool HandedOut;		
		
		[XmlAttribute]
		public int X, Y, Width, Height;
		
		public Scene Scene;		
		
		public Work ()
		{
		}
		
		public void Save (Stream w) {
			var s = new XmlSerializer(typeof(Work));
			s.Serialize (w, this);
		}
		
		public static Work Open (Stream r) {
			var s = new XmlSerializer(typeof(Work));
			return (Work)s.Deserialize (r);
		}
		
		Vec Radiance (Ray ray, int depth, Random rand) {
			return new Vec(0.5, 0, 0);
		}
		
		public void Execute (Random rand, PixelBuffer pb) {
			
			var w = Scene.ImageWidth;
			var h = Scene.ImageHeight;
			var samps = Scene.SamplesPerPixel;
			var cam = Scene.Camera;
			var cx = new Vec (w * 0.5135 / h);
			var cy = cx.Cross (cam.Direction).Norm * 0.5135;
			
			for (var py = 0; py < Height; py++) {
				for (var px = 0; px < Width; px++) {
					var x = X + px;
					var y = Y + py;
					
					var c = new Vec ();

					for (var sy = 0; sy < 2; sy++) {
						for (var sx = 0; sx < 2; sx++) {
							var r = new Vec ();
							for (var s = 0; s < samps; s++) {
								
					            Prec r1=2*rand.NextDouble (), dx=r1<1 ? Math.Sqrt(r1)-1: 1-Math.Sqrt(2-r1);
					            Prec r2=2*rand.NextDouble (), dy=r2<1 ? Math.Sqrt(r2)-1: 1-Math.Sqrt(2-r2);
					            Vec d = cx*( ( (sx+.5 + dx)/2 + x)/w - .5) +
				                    cy*( ( (sy+.5 + dy)/2 + y)/h - .5) + cam.Direction;
						
								r += Radiance (new Ray (cam.Origin + d*140, d.Norm), 0, rand) * (1.0 / samps);
						
							}
							c += r * 0.25;
						}
					}
					
					pb.PutPixel (px, py, c);
				}
			}			
		}
	}
}

