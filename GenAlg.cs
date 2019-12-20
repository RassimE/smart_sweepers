using System;

namespace SmartSweepers
{

	//-----------------------------------------------------------------------
	//
	//	the genetic algorithm class
	//-----------------------------------------------------------------------
	internal class GenAlg
	{
		//this holds the entire population of chromosomes
		private	Genome[] m_vecPop;

		//size of population
		private int m_iPopSize;

		//amount of weights per chromo
		private int m_iChromoLength;

		//total fitness of population
		private double m_dTotalFitness;

		//best fitness this population
		private double m_dBestFitness;

		//average fitness
		private double m_dAverageFitness;

		//worst
		private double m_dWorstFitness;

		//keeps track of the best genome
		private int m_iFittestGenome;

		//probability that a chromosones bits will mutate.
		//Try figures around 0.05 to 0.3 ish
		private double m_dMutationRate;

		//probability of chromosones crossing over bits
		//0.7 is pretty good
		private double m_dCrossoverRate;

		//generation counter
		//private int m_cGeneration;

		//-------------------------------------Crossover()-----------------------
		//	
		//  given parents and storage for the offspring this method performs
		//	crossover according to the GAs crossover rate
		//-----------------------------------------------------------------------
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

		//---------------------------------Mutate--------------------------------
		//
		//	mutates a chromosome by perturbing its weights by an amount not 
		//	greater than Params::dMaxPerturbation
		//-----------------------------------------------------------------------
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

		//----------------------------------GetChromoRoulette()------------------
		//
		//	returns a chromo based on roulette wheel sampling
		//
		//-----------------------------------------------------------------------
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

		//use to introduce elitism
		//-------------------------GrabNBest----------------------------------
		//
		//	This works like an advanced form of elitism by inserting NumCopies
		//  copies of the NBest most fittest genomes into a population vector
		//--------------------------------------------------------------------
		private void GrabNBest(int NBest, int NumCopies, ref Genome[] elitPop)
		{
			//add the required amount of copies of the n most fittest 
			//to the supplied vector
			int st = NBest - 1;
			while (NBest-- > 0)
				for (int i = 0; i < NumCopies; ++i)
					elitPop[(st - NBest) * NumCopies + i] = m_vecPop[st - NBest];
		}

		//-----------------------CalculateBestWorstAvTot-----------------------	
		//
		//	calculates the fittest and weakest genome and the average/total 
		//	fitness scores
		//---------------------------------------------------------------------
		private void CalculateBestWorstAvTot()
		{
			m_iFittestGenome = 0;
			m_dBestFitness = m_vecPop[0].dFitness;
			m_dWorstFitness = m_vecPop[m_iPopSize - 1].dFitness;

			m_dTotalFitness = 0;

			int i = m_iPopSize - 1;
			while (i >= 0)
				m_dTotalFitness += m_vecPop[i--].dFitness;

			m_dAverageFitness = m_dTotalFitness / m_iPopSize;
		}

		//-------------------------Reset()------------------------------
		//
		//	resets all the relevant variables ready for a new generation
		//--------------------------------------------------------------
		void Reset()
		{
			m_dTotalFitness = 0;
			m_dBestFitness = 0;
			m_dWorstFitness = double.MaxValue;
			m_dAverageFitness = 0;
		}
		//==========================
		//-----------------------------------constructor-------------------------
		//
		//	sets up the population with random floats
		//
		//-----------------------------------------------------------------------
		public GenAlg(int popsize, double MutRat, double CrossRat, int numweights)
		{
			m_iPopSize = popsize;
			m_dMutationRate = MutRat;
			m_dCrossoverRate = CrossRat;
			m_iChromoLength = numweights;
			m_dTotalFitness = 0;
			///m_cGeneration = 0;
			m_iFittestGenome = 0;
			m_dBestFitness = 0;
			m_dWorstFitness = double.MaxValue;
			m_dAverageFitness = 0;

			m_vecPop = new Genome[m_iPopSize];

			//initialise population with chromosomes consisting of random
			//weights and all fitnesses set to zero
			for (int i = 0; i < m_iPopSize; ++i)
				m_vecPop[i] = new Genome(m_iChromoLength);
		}

		//this runs the GA for one generation.
		//-----------------------------------Epoch()-----------------------------
		//
		//	takes a population of chromosones and runs the algorithm through one
		//	 cycle.
		//	Returns a new population of chromosones.
		//
		//-----------------------------------------------------------------------
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
				double[] baby1, baby2;

				Crossover(ref mum.vecWeights, ref dad.vecWeights, out baby1, out baby2);

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

		//-------------------accessor methods
		public Genome[] GetChromos()
		{
			return m_vecPop;
		}

		public double AverageFitness()
		{
			return m_dTotalFitness / m_iPopSize;
		}

		public double BestFitness()
		{
			return m_dBestFitness;
		}
	}
}

