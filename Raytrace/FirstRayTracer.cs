using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

using Prec = System.Double;

namespace Raytrace.FirstRayTracer
{
	class Ray
	{
		public Vec Origin;
		public Vec Normal;

		public Ray ()
		{
		}

		public Ray (Vec orig, Vec dest)
		{
			Origin = orig;
			Normal = dest - orig;
			Normal.Normalize ();
		}
	}

	class Intersection
	{
		public SceneObject Object;
		public Ray Ray;
		public Prec RayT;
		public Vec Point;
		public Vec Normal;
		public Material Material;

		public void Set (Prec t, Vec n, Material mat)
		{
			RayT = t;
			Point = Ray.Origin + Ray.Normal * t;
			Normal = n;
			Material = mat;
		}

		public override string ToString ()
		{
			return string.Format ("[t={0}]", RayT);
		}
	}

	class Material
	{
		public Vec AmbientColor = new Vec ((Prec)0.01, (Prec)0.01, (Prec)0.01);
		public Vec DiffuseColor;
		public Vec SpecularColor = new Vec (1, 1, 1);
		public Prec DiffuseCoefficient = 1;
		public Prec SpecularCoefficient = 0;
		public Prec TransmissionCoefficient = 0;
		public static Material Red = new Material () {
			DiffuseColor = new Vec (1, 0, 0)
		};
		public static Material Green = new Material () {
			DiffuseColor = new Vec (0, 1, 0)
		};
		public static Material Blue = new Material () {
			DiffuseColor = new Vec (0, 0, 1)
		};
		public static Material Pink = new Material () {
			DiffuseColor = new Vec (1, (Prec)0.75, (Prec)0.75)
		};
	}

	abstract class SceneObject
	{
		public Material Material;

		public abstract bool Intersect (Ray ray, Intersection isec);
	}

	class Plane : SceneObject
	{
		public Vec Normal;
		public Vec Position;

		public override bool Intersect (Ray ray, Intersection isec)
		{
			var d = ray.Normal.Dot (Normal);
			if (d == 0)
				return false;
			var t = (Position - ray.Origin).Dot (Normal) / d;
			isec.Set (t, Normal, Material);
			return true;
		}
	}

	class Sphere : SceneObject
	{
		public Vec Center;
		public double Radius;

		public override bool Intersect (Ray ray, Intersection isec)
		{
			var a = 1;
			var b = 2 * ray.Normal.Dot (ray.Origin - Center);
			var c = Center.Dot (Center) + ray.Origin.Dot (ray.Origin) - 2 * (Center.Dot (ray.Origin)) - Radius * Radius;

			var d = b * b - 4 * a * c;

			if (d < 0)
				return false;

			var sqrtd = (Prec)Math.Sqrt (d);

			var t1 = (-b - sqrtd) / 2;//(2*a);
			var t2 = (-b + sqrtd) / 2;//(2*a);

			var t = t1;
			if (t1 < 0) {
				if (t2 < 0)
					return false;
				t = t2;
			} else {
				if (t2 > 0 && t2 < t1)
					t = t2;
			}

			var p = ray.Origin + ray.Normal * t;
			var n = (p - Center);
			n.Normalize ();

			isec.Set (t, n, Material);

			return true;
		}
	}

	class Light
	{
		public Vec Position;
	}

	class MainClass
	{
		Light[] Lights = new Light[] {
			new Light () {
				Position = new Vec(4, 4, 40),
			},
			new Light () {
				Position = new Vec(-4, -4, -2),
			},
			new Light () {
				Position = new Vec(4, 4, 2),
			},
		};
		SceneObject[] Objects = new SceneObject[] {
			new Plane () {
				Normal = new Vec(0, 0, 1),
				Position = new Vec (0, 0, -5),
				Material = Material.Red
			},
			new Plane () {
				Normal = new Vec(1, 0, 0),
				Position = new Vec (-5, 0, 0),
				Material = Material.Green
			},
			new Plane () {
				Normal = new Vec(-1, 0, 0),
				Position = new Vec (5, 0, 0),
				Material = Material.Green
			},
			new Plane () {
				Normal = new Vec(0, 1, 0),
				Position = new Vec (0, -5, 0),
				Material = Material.Blue
			},
			new Plane () {
				Normal = new Vec(0, -1, 0),
				Position = new Vec (0, 5, 0),
				Material = Material.Blue
			},
			new Sphere () {
				Center = new Vec(2, 2, -3),
				Radius = 3,
				Material = Material.Pink
			},
		};

		public Prec ShadowCoefficient (Vec orig, Vec light)
		{
			//
			// INSERSECT ALL
			//
			var d = (light - orig).Length;
			var ray = new Ray (orig, light);
			var i = new Intersection ();
			foreach (var o in Objects) {
				i.Ray = ray;
				if (o.Intersect (ray, i)) {
					if (i.RayT > 0.0000001 && i.RayT < d) {
						return 0;
					}
				}
			}
			return 1;
		}

		public Vec Trace (Ray ray, int depth)
		{
			//
			// INSERSECT ALL
			//
			var icount = 0;
			foreach (var o in Objects) {
				var i = ints [icount];
				i.Ray = ray;
				if (o.Intersect (ray, i)) {
					if (i.RayT > 0) {
						i.Object = o;
						icount++;
					}
				}
			}

			//
			// FIND CLOSEST
			//
			if (icount > 0) {

				var closeT = ints [0].RayT;
				var closeI = 0;
				for (var i = 1; i < icount; i++) {
					if (ints [i].RayT < closeT) {
						closeI = i;
						closeT = ints [i].RayT;
					}
				}

				var ii = ints [closeI];

				return Shade (ray, ii, depth);

			} else {
				return new Vec (0, 0, 0);
			}

		}

		Vec Shade (Ray ray, Intersection isec, int depth)
		{
			var mat = isec.Material;
			var color = mat.DiffuseColor * (Prec)0.01;

			foreach (var light in Lights) {
				var sray = new Ray (isec.Point, light.Position);
				var l = isec.Point.NormalTo (light.Position);
				var v = isec.Point.NormalTo (CameraCenter);
				var r = isec.Normal;
				if (isec.Normal.Dot (sray.Normal) > 0) {
					var s = ShadowCoefficient (isec.Point, light.Position);
					var fatt = (Prec)1;
					color += (mat.DiffuseColor * (mat.DiffuseCoefficient * isec.Normal.Dot (l)) + 
						mat.SpecularColor * (mat.SpecularCoefficient * r.Dot (v))) * (s * fatt);
				}
			}

			if (depth < MaxDepth) {

			}

			color *= ((Prec)10) / isec.RayT;

			return color;
		}

		const int MaxDepth = 6;
		Intersection[] ints;
		Vec CameraCenter = new Vec (0, 0, 10);

		public void Run ()
		{
			ints = new Intersection[Objects.Length];
			for (var i = 0; i < ints.Length; i++) {
				ints [i] = new Intersection ();
			}

			var bmp = new PixelBuffer (1920, 1080);

			var ss = 10 / (Prec)bmp.Width;

			var startT = DateTime.Now;

			var ray = new Ray ();

			var ns = 32;

			var rand = new Random ();

			for (var y = 0; y < bmp.Height; y++) {
				for (var x = 0; x < bmp.Width; x++) {

					var col = new Vec (0,0,0);

					for (var samp = 0; samp < ns; samp++) {
						var r0 = CameraCenter;
						var r1 = new Vec (
							(Prec)(x + (ns > 1 ? rand.NextDouble () : 0.0) - bmp.Width / 2) * ss,
							(Prec)(y + (ns > 1 ? rand.NextDouble () : 0.0) - bmp.Height / 2) * ss,
							6);

						var v = r1 - r0;
						v.Normalize ();

						ray.Origin = r0;
						ray.Normal = v;

						col += Trace (ray, 1);
					}

					col *= ((Prec)1)/ns;

					bmp.PutPixel (x, y, col);
				}
			}

			var endT = DateTime.Now;
			var time = (endT - startT).TotalSeconds;

			System.Console.WriteLine ("Rays/sec = {0}, T = {1}", bmp.Width * bmp.Height / time, time);

			bmp.SavePng ();
		}

		/*public static void Main (string[] args)
		{
			new MainClass ().Run ();
		}*/
	}
}
