using System;

namespace SmartSweepers
{
	internal static class Utils
	{
		//----------------------------------------------------------------------------
		//	some random number functions.
		//----------------------------------------------------------------------------
		static Random rand = new Random(DateTime.Now.Millisecond);

		//returns a random integer between x and y
		public static int RandInt(int x, int y)
		{
			return rand.Next(x, y);
		}

		//returns a random float between zero and 1
		public static double RandFloat()
		{
			return rand.NextDouble();
		}

		//returns a random bool
		public static bool RandBool()
		{
			if (rand.Next(2) != 0)
				return true;
			return false;
		}

		//returns a random float in the range -1 < n < 1
		public static double RandomClamped()
		{
			return rand.NextDouble() - 0.5;
			//return 1.0 - 2.0 * rand.NextDouble();
			//return rand.NextDouble() - rand.NextDouble();
		}

		//=======================================================
		public static double RadToDeg(double x)
		{
			return x * 180.0 / Math.PI;
		}
	}
}
