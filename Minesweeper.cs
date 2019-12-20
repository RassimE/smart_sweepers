using System;
using System.Drawing;

namespace SmartSweepers
{
	internal class Minesweeper
	{
		//the minesweeper's neural net
		private NeuralNet m_ItsBrain;

		//its position in the world
		private Vector2D m_vPosition;

		//direction sweeper is facing
		private Vector2D m_vLookAt;

		//its rotation (surprise surprise)
		private double m_dRotation;

		private double m_dSpeed;

		//to store output from the ANN
		private double m_lTrack, m_rTrack;

		//the sweeper's fitness score 
		private double m_dFitness;

		//the scale of the sweeper when drawn
		private double m_dScale;

		//index position of closest mine
		private int m_iClosestMine;

		public Minesweeper()
		{
			m_dRotation = Utils.RandFloat() * Params.dTwoPi;
			m_lTrack = 0.16;
			m_rTrack = 0.16;
			m_dFitness = 0;
			m_dScale = Params.iSweeperScale;
			m_iClosestMine = 0;

			m_ItsBrain = new NeuralNet();

			//create a random start position
			m_vPosition = Vector2D.GetRandom(Params.WindowWidth, Params.WindowHeight);
		}

		public static void Clamp(ref double arg, double min, double max)
		{
			if (arg < min) arg = min;
			if (arg > max) arg = max;
		}

		//updates the ANN with information from the sweepers enviroment
		//-------------------------------Update()--------------------------------
		//
		//	First we take sensor readings and feed these into the sweepers brain.
		//
		//	The inputs are:
		//	
		//	A vector to the closest mine (x, y)
		//	The sweepers 'look at' vector (x, y)
		//
		//	We receive two outputs from the brain.. lTrack & rTrack.
		//	So given a force for each track we calculate the resultant rotation 
		//	and acceleration and apply to current velocity vector.
		//
		//-----------------------------------------------------------------------
		public bool Update(ref Vector2D[] mines)
		{
			//get vector to closest mine
			Vector2D vClosestMine = GetClosestMine(ref mines);

			//normalise it
			vClosestMine.Normalize();

			//this will store all the inputs for the NN
			double[] inputs = new double[Params.iNumInputs];

			//add in vector to closest mine
			inputs[0] = vClosestMine.X;
			inputs[1] = vClosestMine.Y;

			//add in sweepers look at vector
			inputs[2] = m_vLookAt.X;
			inputs[3] = m_vLookAt.Y;

			//update the brain and get feedback
			double[] output = m_ItsBrain.Update(ref inputs);

			//make sure there were no errors in calculating the output
			if (output.Length < Params.iNumOutputs)
				return false;

			//assign the outputs to the sweepers left & right tracks
			m_lTrack = output[0];
			m_rTrack = output[1];

			//calculate steering forces
			double RotForce = m_lTrack - m_rTrack;

			//clamp rotation
			Clamp(ref RotForce, -Params.dMaxTurnRate, Params.dMaxTurnRate);

			m_dRotation += RotForce;

			m_dSpeed = (m_lTrack + m_rTrack);

			//update Look At 
			m_vLookAt.X = -Math.Sin(m_dRotation);
			m_vLookAt.Y = Math.Cos(m_dRotation);

			//update position
			m_vPosition += (m_vLookAt * m_dSpeed);

			//wrap around window limits
			if (m_vPosition.X >= Params.WindowWidth) m_vPosition.X = 0;
			if (m_vPosition.X < 0) m_vPosition.X = Params.WindowWidth;

			if (m_vPosition.Y >= Params.WindowHeight) m_vPosition.Y = 0;
			if (m_vPosition.Y < 0) m_vPosition.Y = Params.WindowHeight;

			return true;
		}

		//used to transform the sweepers vertices prior to rendering
		//---------------------WorldTransform--------------------------------
		//
		//	sets up a translation matrix for the sweeper according to its
		//  scale, rotation and position. Returns the transformed vertices.
		//-------------------------------------------------------------------
		public void WorldTransform(ref PointF[] sweeper)
		{
			//create the world transformation matrix
			Matrix2D matTransform = new Matrix2D(0);

			//scale
			matTransform.Scale(m_dScale, m_dScale);

			//rotate
			matTransform.Rotate(m_dRotation);

			//and translate
			matTransform.Translate(m_vPosition);

			//now transform the ships vertices
			matTransform.TransformPoints(ref sweeper);
		}

		//returns a vector to the closest mine
		//----------------------GetClosestObject()---------------------------------
		//
		//	returns the vector from the sweeper to the closest mine
		//
		//-----------------------------------------------------------------------
		public Vector2D GetClosestMine(ref Vector2D[] mines)
		{
			double closest_so_far = double.MaxValue;

			Vector2D vClosestObject = default(Vector2D);

			//cycle through mines to find closest
			for (int i = 0; i < mines.Length; i++)
			{
				double len_to_object = (mines[i] - m_vPosition).Length;

				if (len_to_object < closest_so_far)
				{
					closest_so_far = len_to_object;

					vClosestObject = m_vPosition - mines[i];

					m_iClosestMine = i;
				}
			}

			return vClosestObject;
		}

		//checks to see if the minesweeper has 'collected' a mine
		//----------------------------- CheckForMine -----------------------------
		//
		//  this function checks for collision with its closest mine (calculated
		//  earlier and stored in m_iClosestMine)
		//-----------------------------------------------------------------------
		public int CheckForMine(ref Vector2D[] mines, double size)
		{
			Vector2D DistToObject = m_vPosition - mines[m_iClosestMine];

			if (DistToObject.Length < size + 5)
				return m_iClosestMine;

			return -1;
		}

		//-------------------------------------------Reset()--------------------
		//
		//	Resets the sweepers position, fitness and rotation
		//
		//----------------------------------------------------------------------
		public void Reset()
		{
			//reset the sweepers positions
			m_vPosition = Vector2D.GetRandom(Params.WindowWidth, Params.WindowHeight);

			//and the fitness
			m_dFitness = 0;

			//and the rotation
			m_dRotation = Utils.RandFloat() * Params.dTwoPi;

			return;
		}

		//-------------------accessor functions
		public Vector2D Position() { return m_vPosition; }

		public double Rotation() { return m_dRotation; }

		public void IncrementFitness() { ++m_dFitness; }

		public double Fitness() { return m_dFitness; }

		public void PutWeights(ref double[] w) { m_ItsBrain.PutWeights(ref w); }

		public int GetNumberOfWeights() { return m_ItsBrain.GetNumberOfWeights(); }
	}
}
