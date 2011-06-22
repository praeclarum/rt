using System;

using Prec = System.Double;

namespace Raytrace
{
	/// <summary>
	/// A 3D vector.
	/// </summary>
	[Serializable]
	public struct Vec
	{
		[System.Xml.Serialization.XmlAttribute]
		public Prec X, Y, Z;
		
		public static readonly Vec Zero = new Vec (0, 0, 0);

		public Vec (Prec x=0, Prec y=0, Prec z=0)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Prec Dot (Vec b)
		{
			return X * b.X + Y * b.Y + Z * b.Z;
		}
		
		public Vec Cross (Vec b)
		{
			return new Vec (
				Y*b.Z - Z*b.Y,
				Z*b.X - X*b.Z,
				X*b.Y - Y*b.X
			);
		}

		public Prec Length {
			get { return (Prec)Math.Sqrt (X * X + Y * Y + Z * Z); }
		}
		
		public Vec Norm
		{
			get {
				var other = this;
				other.Normalize ();
				return other;
			}
		}
		
		public void Normalize ()
		{
			var len = (Prec)Math.Sqrt (X * X + Y * Y + Z * Z);
			if (len == 0) return;
			var r = ((Prec)1) / len;
			X *= r;
			Y *= r;
			Z *= r;
		}

		public Vec NormalTo (Vec dest)
		{
			var v = dest - this;
			v.Normalize ();
			return v;
		}

		public static Vec operator + (Vec a, Vec b)
		{
			return new Vec (a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Vec operator - (Vec a, Vec b)
		{
			return new Vec (a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Vec operator * (Vec a, Prec b)
		{
			return new Vec (a.X * b, a.Y * b, a.Z * b);
		}

		public override string ToString ()
		{
			return string.Format ("[{0}, {1}, {2}]", X, Y, Z);
		}
	}
	
	/// <summary>
	/// This is a ray. A point and a normalized direction.
	/// Make sure it's normalized!
	/// </summary>
	[Serializable]
	public struct Ray
	{
		public Vec Origin;
		public Vec Direction;

		public Ray (Vec orig, Vec dir)
		{
			Origin = orig;
			Direction = dir;
		}
	}
}

