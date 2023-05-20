using System;

namespace SmartSweepers
{

	//-----------------------------------------------------------------------
	//
	//	the genetic algorithm class
	//-----------------------------------------------------------------------
	internal class GenAlg
	{
		/// <summary>
		///this holds the entire population of chromosomes
		/// </summary>
		private Genome[] m_vecPop;

		/// <summary>
		///size of population
		/// </summary>
		private readonly int m_iPopSize;

		/// <summary>
		///amount of weights per chromo
		/// </summary>
		private readonly int m_iChromoLength;

		/// <summary>
		///total fitness of population
		/// </summary>
		private double m_dTotalFitness;

		/// <summary>
		///best fitness this population
		/// </summary>
		private double m_dBestFitness;

		/// <summary>
		///average fitness
		/// </summary>
		//private double m_dAverageFitness;

		///<summary>
		///worst
		///</summary>
		//private double m_dWorstFitness;

		/// <summary>
		///keeps track of the best genome
		/// </summary>
		//private int m_iFittestGenome;

		/// <summary>
		///probability that a chromosones bits will mutate.
		///Try figures around 0.05 to 0.3 ish
		/// </summary>
		private readonly double m_dMutationRate;

		/// <summary>
		/// probability of chromosones crossing over bits
		///0.7 is pretty good
		/// </summary>
		private readonly double m_dCrossoverRate;

		/// <summary>
		///generation counter
		/// </summary>
		//private int m_cGeneration;

		/// <summary>
		///Crossover - 
		/// given parents and storage for the offspring this method performs
		///	crossover according to the GAs crossover rate
		/// </summary>
		private void Crossover(ref double[] mum, ref double[] dad, out double[] baby1, out double[] baby2)
		{
			int len = mum.Length;
			baby1 = new double[len];
			baby2 = new double[len];

			//just return parents as offspring dependent on the rate
			//or if parents are the same
			if ((mum == dad) || (Utils.RandFloat() > m_dCrossoverRate))
			{
				Array.Copy(mum, baby1, len);
				Array.Copy(dad, baby2, len);

				return;
			}

			//determine a crossover point
			int cp = Utils.RandInt(0, m_iChromoLength - 1);

			//create the offspring
			int i = 0;
			for (; i < cp; ++i)
			{
				baby1[i] = mum[i];
				baby2[i] = dad[i];
			}

			for (; i < len; ++i)
			{
				baby1[i] = dad[i];
				baby2[i] = mum[i];
			}
		}

		/// <summary>
		/// Mutate -
		///	mutates a chromosome by perturbing its weights by an amount not 
		///	greater than Params.dMaxPerturbation
		/// </summary>
		private void Mutate(ref double[] chromo)
		{
			//traverse the chromosome and mutate each weight dependent
			//on the mutation rate
			for (int i = 0; i < chromo.Length; ++i)
				//do we perturb this weight?
				if (Utils.RandFloat() < m_dMutationRate)
					//add or subtract a small value to the weight
					chromo[i] += Utils.RandomClamped() * Params.dMaxPerturbation;
		}

		/// <summary>
		/// GetChromoRoulette -
		///	returns a chromo based on roulette wheel sampling
		/// </summary>
		private Genome GetChromoRoulette()
		{
			//generate a random number between 0 & total fitness count
			double Slice = Utils.RandFloat() * m_dTotalFitness;

			//this will be set to the chosen chromosome
			Genome TheChosenOne = null;

			//go through the chromosones adding up the fitness so far
			double FitnessSoFar = 0;

			for (int i = 0; i < m_iPopSize; ++i)
			{
				FitnessSoFar += m_vecPop[i].dFitness;

				//if the fitness so far > random number return the chromo at 
				//this point
				if (FitnessSoFar >= Slice)
				{
					TheChosenOne = m_vecPop[i];
					break;
				}
			}

			return TheChosenOne;
		}

		///<summary>
		/// use to introduce elitism
		/// GrabNBest -
		///	This works like an advanced form of elitism by inserting NumCopies
		/// copies of the NBest most fittest genomes into a population vector
		///</summary>
		private void GrabNBest(int NBest, int NumCopies, ref Genome[] elitPop)
		{
			//add the required amount of copies of the n most fittest 
			//to the supplied vector
			int st = NBest - 1;
			while (NBest-- > 0)
				for (int i = 0; i < NumCopies; ++i)
					elitPop[(st - NBest) * NumCopies + i] = m_vecPop[st - NBest];
		}

		///<summary>
		/// CalculateBestWorstAvTot -
		///	calculates the fittest and weakest genome and the average/total 
		///	fitness scores
		///</summary>
		private void CalculateBestWorstAvTot()
		{
			//m_iFittestGenome = 0;
			m_dBestFitness = m_vecPop[0].dFitness;
			//m_dWorstFitness = m_vecPop[m_iPopSize - 1].dFitness;

			m_dTotalFitness = 0;

			int i = m_iPopSize - 1;
			while (i >= 0)
				m_dTotalFitness += m_vecPop[i--].dFitness;

			//m_dAverageFitness = m_dTotalFitness / m_iPopSize;
		}

		///<summary>
		/// Reset -
		///	resets all the relevant variables ready for a new generation
		///</summary>
		void Reset()
		{
			m_dTotalFitness = 0;
			m_dBestFitness = 0;
			//m_dWorstFitness = double.MaxValue;
			//m_dAverageFitness = 0;
		}

		/// <summary>
		/// constructor -
		///	sets up the population with random floats
		/// </summary>
		/// <param name="popsize"></param>
		/// <param name="MutRat"></param>
		/// <param name="CrossRat"></param>
		/// <param name="numweights"></param>
		public GenAlg(int popsize, double MutRat, double CrossRat, int numweights)
		{
			m_iPopSize = popsize;
			m_dMutationRate = MutRat;
			m_dCrossoverRate = CrossRat;
			m_iChromoLength = numweights;
			m_dTotalFitness = 0;
			///m_cGeneration = 0;
			//m_iFittestGenome = 0;
			m_dBestFitness = 0;
			//m_dWorstFitness = double.MaxValue;
			//m_dAverageFitness = 0;

			m_vecPop = new Genome[m_iPopSize];

			//initialise population with chromosomes consisting of random
			//weights and all fitnesses set to zero
			for (int i = 0; i < m_iPopSize; ++i)
				m_vecPop[i] = new Genome(m_iChromoLength);
		}

		/// <summary>
		/// Epoch -
		/// this runs the GA for one generation.
		///	takes a population of chromosones and runs the algorithm through one
		///	 cycle.
		/// </summary>
		/// <param name="old_pop"></param>
		///<returns>
		///	Returns a new population of chromosones.
		///</returns>
		public Genome[] Epoch(ref Genome[] old_pop)
		{
			//assign the given population to the classes population
			m_vecPop = old_pop;

			//reset the appropriate variables
			Reset();

			//sort the population (for scaling and elitism)
			Array.Sort(m_vecPop);

			//calculate best, worst, average and total fitness
			CalculateBestWorstAvTot();

			//create a temporary vector to store new chromosones
			Genome[] vecNewPop = new Genome[m_iPopSize];

			//Now to add a little elitism we shall add in some copies of the
			//fittest genomes. Make sure we add an EVEN number or the roulette
			//wheel sampling will crash
			if ((Params.iNumCopiesElite * Params.iNumElite % 2) == 0)
				GrabNBest(Params.iNumElite, Params.iNumCopiesElite, ref vecNewPop);

			//now we enter the GA loop

			//repeat until a new population is generated
			for (int i = Params.iNumCopiesElite* Params.iNumElite; i < m_iPopSize; i += 2)
			{
				//grab two chromosones
				Genome mum = GetChromoRoulette();
				Genome dad = GetChromoRoulette();

				//create some offspring via crossover
				Crossover(ref mum.vecWeights, ref dad.vecWeights, out double[] baby1, out double[] baby2);

				//now we mutate
				Mutate(ref baby1);
				Mutate(ref baby2);

				//now copy into vecNewPop population
				vecNewPop[i] = new Genome(baby1);
				if (i + 1 < m_iPopSize)
					vecNewPop[i + 1] = new Genome(baby2);
			}

			//finished so assign new pop back into m_vecPop
			m_vecPop = vecNewPop;

			return m_vecPop;
		}

		/// <summary>
		///  accessor methods
		/// </summary>
		/// <returns></returns>
		public Genome[] GetChromos()
		{
			return m_vecPop;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public double AverageFitness()
		{
			return m_dTotalFitness / m_iPopSize;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public double BestFitness()
		{
			return m_dBestFitness;
		}
	}
}

