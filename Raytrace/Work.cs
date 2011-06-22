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
		
		public void Save (Stream w)
		{
			var s = new XmlSerializer (typeof(Work));
			s.Serialize (w, this);
		}
		
		public static Work Open (Stream r)
		{
			var s = new XmlSerializer (typeof(Work));
			return (Work)s.Deserialize (r);
		}
		
		Vec Radiance (Ray ray, int depth, Random rand)
		{
			Prec t;
			SceneObject obj;
			if (!Scene.Intersect (ray, out t, out obj)) return Vec.Zero;
			
			var x = ray.Origin + ray.Direction * t;
			var n = obj.GetNormal (x);
			var nl = n.Dot (ray.Direction) < 0 ? n : n*-1;
			var f = obj.Color;
			var p = (f.X > f.Y && f.X > f.Z) ? f.X : ((f.Y > f.Z) ? f.Y : f.Z);
			if (++depth > 5) {
				if (rand.NextDouble () < p) {
					f = f * (1.0 / p);
				}
				else {
					return obj.Emission;
				}
			}
			if (obj.Material == MaterialType.Diffuse) {
				var r1 = 2*Math.PI*rand.NextDouble ();
				var r2 = rand.NextDouble ();
				var r2s = Math.Sqrt (r2);
				var w = nl;
				var u = ((Math.Abs (w.X)>0.1?new Vec(0,1):new Vec(1)).Cross (w)).Norm;
				var v = w.Cross (u);
				var d = (u*Math.Cos (r1)*r2s + v*Math.Sin (r1)*r2s + w*Math.Sqrt (1-r2)).Norm;
				return obj.Emission + f.Mult (Radiance (new Ray(x, d), depth, rand));
			}			
			else if (obj.Material == MaterialType.Specular) {
				return obj.Emission + f.Mult (Radiance (new Ray(x, ray.Direction - n*2*n.Dot (ray.Direction)), depth, rand));
			}
			
			var reflRay = new Ray (x, ray.Direction - n*2*n.Dot (ray.Direction));
			var intoo = n.Dot (nl) > 0;
			Prec nc = 1.0, nt = 1.5;
			var nnt = intoo?nc/nt:nt/nc;
			var ddn = ray.Direction.Dot (nl);
			var cos2t = 1 - nnt*nnt*(1-ddn*ddn);
			if (cos2t < 0) {
				return obj.Emission + f.Mult (Radiance (reflRay, depth, rand));
			}
			
			var tdir = (ray.Direction*nnt - n*((intoo?1:-1)*(ddn*nnt+Math.Sqrt (cos2t)))).Norm;
			
			Prec a = nt - nc, b = nt + nc;
			var R0 = a*a / (b*b);
			var c = 1 - (intoo ? -ddn : tdir.Dot(n));
			
			var Re = R0 + (1 - R0)*c*c*c*c*c;
			var Tr = 1 - Re;
			var P = 0.25 + 0.5*Re;
			var RP = Re/P;
			var TP = Tr/(1-P);
			
			return obj.Emission + f.Mult (depth > 2 ? (rand.NextDouble () < P ?
				Radiance (reflRay, depth, rand)*RP:Radiance (new Ray(x,tdir),depth,rand)*TP) :
				Radiance (reflRay, depth, rand)*Re+Radiance (new Ray(x,tdir),depth,rand)*Tr);
		}
		
		public void Execute (Random rand, PixelBuffer pb)
		{
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
								
								Prec r1 = 2 * rand.NextDouble (), dx=r1 < 1 ? Math.Sqrt (r1) - 1 : 1 - Math.Sqrt (2 - r1);
								Prec r2 = 2 * rand.NextDouble (), dy=r2 < 1 ? Math.Sqrt (r2) - 1 : 1 - Math.Sqrt (2 - r2);
								Vec d = cx * (((sx + .5 + dx) / 2 + x) / w - .5) +
				                    cy * (((sy + .5 + dy) / 2 + y) / h - .5) + cam.Direction;
						
								r += Radiance (new Ray (cam.Origin + d * 140, d.Norm), 0, rand) * (1.0 / samps);
						
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

