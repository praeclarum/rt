using System;

using Prec = System.Double;

namespace Raytrace
{
	[Serializable]
	public struct Vec
	{
		[System.Xml.Serialization.XmlAttribute]
		public Prec X, Y, Z;

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

		public Prec Length {
			get { return (Prec)Math.Sqrt (X * X + Y * Y + Z * Z); }
		}

		public void Normalize ()
		{
			var len = (Prec)Math.Sqrt (X * X + Y * Y + Z * Z);
			X /= len;
			Y /= len;
			Z /= len;
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
}

