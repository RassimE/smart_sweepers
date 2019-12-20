using System;

namespace SmartSweepers
{
	internal struct Vector2D
	{
		public double X, Y;

		public Vector2D(double a, double b = 0.0)
		{
			X = a;
			Y = b;
		}

		public Vector2D(Vector2D other)
		{
			X = other.X;
			Y = other.Y;
		}

		public static Vector2D GetRandom(double cx, double cy)
		{
			return new Vector2D(Utils.RandFloat() * cx, Utils.RandFloat() * cy);
		}

		//we need some overloaded operators
		public static Vector2D operator +(Vector2D th, Vector2D rhs)
		{
			th.X += rhs.X;
			th.Y += rhs.Y;

			return th;
		}

		public static Vector2D operator /(Vector2D th, double rhs)
		{
			th.X /= rhs;
			th.Y /= rhs;

			return th;
		}

		//overload the * operator
		public static Vector2D operator *(Vector2D lhs, double rhs)
		{
			Vector2D result = new Vector2D(lhs);
			result.X *= rhs;
			result.Y *= rhs;

			return result;
		}

		public static Vector2D operator *(double lhs, Vector2D rhs)
		{
			Vector2D result = new Vector2D(rhs);
			result.X *= lhs;
			result.Y *= lhs;
			return result;
		}

		//overload the - operator
		public static Vector2D operator -(Vector2D lhs, Vector2D rhs)
		{
			Vector2D result = new Vector2D(lhs);
			result.X -= rhs.X;
			result.Y -= rhs.Y;
			return result;
		}
		//------------------------- Vec2DLength -----------------------------
		//
		//	returns the length of a 2D vector
		//--------------------------------------------------------------------
		//public double Length()
		//{
		//	return Math.Sqrt(X * X + Y * Y);
		//}
		public double Length
		{
			get
			{
				return Math.Sqrt(X * X + Y * Y);
			}
		}

		//public static double Length(Vector2D v)
		//{
		//	return Math.Sqrt(v.X * v.X + v.Y * v.Y);
		//}

		//------------------------- Vec2DNormalize -----------------------------
		//
		//	normalizes a 2D Vector
		//--------------------------------------------------------------------
		public void Normalize()
		{
			double vector_length = Length;
			//this /= vector_length;
			X /= vector_length;
			Y /= vector_length;
		}

		public static void Normalize(ref Vector2D v)
		{
			double vector_length = v.Length;

			v.X = v.X / vector_length;
			v.Y = v.Y / vector_length;
		}

		//------------------------- Vec2DDot --------------------------
		//
		//	calculates the dot product
		//--------------------------------------------------------------------
		public double Dot(Vector2D v)
		{
			return X * v.X + Y * v.Y;
		}

		public static double Dot(Vector2D v1, Vector2D v2)
		{
			return v1.X * v2.X + v1.Y * v2.Y;
		}

		//------------------------ Vec2DSign --------------------------------
		//
		//  returns positive if v2 is clockwise of v1, minus if anticlockwise
		//-------------------------------------------------------------------
		public int Sign(Vector2D v)
		{
			if (Y * v.X > X * v.Y)
				return 1;
			else //if (y * v.x < x * v.y)
				return -1;

			//return 0;
		}

		public static int Sign(Vector2D v1, Vector2D v2)
		{
			if (v1.Y * v2.X > v1.X * v2.Y)
				return 1;
			else //if (v1.y*v2.x < v1.x*v2.y)
				return -1;

			//return 0;
		}
	}
}
