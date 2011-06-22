using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using Prec = System.Double;

namespace Raytrace
{
	[Serializable]
	public class Scene
	{
		[XmlAttribute]
		public int ImageWidth;
		
		[XmlAttribute]
		public int ImageHeight;
		
		[XmlAttribute]
		public int SamplesPerPixel;
		
		public Ray Camera;
		
		public List<SceneObject> Objects;
		
		public Scene ()
		{
			Objects = new List<SceneObject> ();
		}
		
		public bool Intersect (Ray ray, out Prec t, out SceneObject obj)
		{
			var inf = 1.0e20;
			var tt = inf;
			var oo = default(SceneObject);
			foreach (var o in Objects) {
				var d = o.Intersect (ray);
				if (d != 0 && d < tt) {
					tt = d;
					oo = o;
				}
			}
			t = tt;
			obj = oo;
			return tt < inf;
		}
		
		public void SaveXml (Stream s) {
			using (var w = new StreamWriter (s, Encoding.UTF8)) {
				SaveXml (w);
			}
		}
		
		public void SaveXml (TextWriter w) {
			var s = new XmlSerializer(typeof(Scene));
			s.Serialize (w, this);
		}
		
		public string SaveXml () {
			var w = new StringWriter ();
			SaveXml (w);
			return w.ToString ();
		}
		
		public static Scene Open (Stream r) {
			var s = new XmlSerializer(typeof(Scene));
			return (Scene)s.Deserialize (r);
		}
	}
	
	[Serializable]
	public enum MaterialType {
		Diffuse,
		Specular,
		Transmissive,
	}
	
	[Serializable]
	[XmlInclude(typeof(Sphere))]
	public abstract class SceneObject
	{
		public Vec Emission;
		public Vec Color;
		public MaterialType Material;
		
		public SceneObject () {}
		
		public SceneObject (Vec emission, Vec color, MaterialType mat)
		{
			Emission = emission;
			Color = color;
			Material = mat;
		}
		
		public abstract double Intersect (Ray ray);
		public abstract Vec GetNormal (Vec p);
	}
	
	[Serializable]
	public class Sphere : SceneObject
	{
		public Vec Position;
		public Prec Radius;
		
		public Sphere () {}
		
		public Sphere (Prec radius, Vec position, Vec emission, Vec color, MaterialType mt)
			: base (emission, color, mt)
		{
			Radius = radius;
			Position = position;
		}
		
		public override double Intersect (Ray ray)
		{
			var op = Position - ray.Origin;
			var eps = 1.0e-4;
			var b = op.Dot (ray.Direction);
			var det = b*b - op.Dot (op) + Radius*Radius;
			if (det < 0) return 0;
			det = Math.Sqrt (det);
			var t = b - det;
			if (t > eps) return t;
			t = b + det;
			return (t > eps) ? t : 0;
		}
		
		public override Vec GetNormal (Vec p)
		{
			return (p - Position).Norm;
		}
	}
}

