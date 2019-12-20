using System;

namespace SmartSweepers
{
	//-----------------------------------------------------------------------
	//
	//	create a structure to hold each genome
	//-----------------------------------------------------------------------
	internal class Genome : IComparable
	{
		public double[] vecWeights;
		public double dFitness;

		public Genome(int chromoLength)
		{
			vecWeights = new double[chromoLength];
			for (int j = 0; j < chromoLength; ++j)
				vecWeights[j] = Utils.RandomClamped();

			dFitness = 0;
		}

		public Genome(double[] w)
		{
			int n = w.Length;
			vecWeights = new double[n];
			Array.Copy(w, vecWeights, n);

			dFitness = 0.0;
		}

		//=================================================

		// Implements System.IComparable.CompareTo
		//used for sorting
		public int CompareTo(object obj)
		{
			Genome a = (Genome)obj;
			if (a.dFitness < dFitness) return -1;
			if (a.dFitness > dFitness) return 1;

			return 0;
		}

	}
}
