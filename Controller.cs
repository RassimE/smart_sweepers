#define UseSweeperWorldTransform

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SmartSweepers
{
	internal class Controller
	{
		//these hold the geometry of the sweepers and the mines
		const int NumSweeperVerts = 16;
		const int NumMineVerts = 4;

		private readonly PointF[] sweeper = {
		new PointF(-1, -1),
		new PointF(-1, 1),
		new PointF(-0.5f, 1),
		new PointF(-0.5f, -1),

		new PointF(1, -1),
		new PointF(1, 1),
		new PointF(0.5f, 1),
		new PointF(0.5f, -1),

		new PointF(-0.5f, -0.5f),
		new PointF(0.5f, -0.5f),

		new PointF(-0.5f, 0.5f),
		new PointF(-0.25f, 0.5f),
		new PointF(-0.25f, 1.75f),
		new PointF(0.25f, 1.75f),
		new PointF(0.25f, 0.5f),
		new PointF(0.5f, 0.5f) };

		private readonly PointF[] mine = {
		//new PointF(0,  -2),
		new PointF(-1, -1),
		new PointF(-1,  1),
		new PointF(1,  1),
		new PointF(1, -1) };

		/// <summary>
		/// storage for the population of genomes
		/// </summary>
		private Genome[] m_ThePopulation;

		/// <summary>
		/// and the minesweepers
		/// </summary>
		private readonly Minesweeper[] m_Sweepers;

		/// <summary>
		/// and the mines
		/// </summary>
		private Vector2D[] m_Mines;

		/// <summary>
		/// pointer to the GA
		/// </summary>
		private readonly GenAlg m_pGA;
		private readonly int m_NumSweepers;
		private int m_NumMines;
		private readonly int m_NumWeightsInNN;

		/// <summary>
		/// vertex buffer for the sweeper shape's vertices
		/// </summary>
		private readonly PointF[] m_SweeperVB;

		/// <summary>
		/// vertex buffer for the mine shape's vertices
		/// </summary>
		private readonly PointF[] m_MineVB;

		/// <summary>
		/// stores the average fitness per generation for use in graphing.
		/// </summary>
		private readonly List<double> m_vecAvFitness;

		/// <summary>
		/// stores the best fitness per generation
		/// </summary>
		private readonly List<double> m_vecBestFitness;

		/// <summary>
		/// pens we use for the stats
		/// </summary>
		private readonly Pen m_fittPen;          // fittest
		private readonly Pen m_sweepPen;         // normal
		private readonly Pen m_minePen;          // mine

		/// <summary>
		/// handle to the application window
		/// </summary>
		private Control m_Main;

		/// <summary>
		/// toggles the speed at which the simulation runs
		/// </summary>
		private bool m_bFastRender;

		/// <summary>
		/// cycles per generation
		/// </summary>
		private int m_iTicks;

		/// <summary>
		/// generation counter
		/// </summary>
		private int m_iGenerations;

		/// <summary>
		/// window dimensions
		/// </summary>
		private int cxClient, cyClient;

		private double m_dBestInHistory;
		/// <summary>
		/// constructor -
		///	initilaize the sweepers, their brains and the GA factory
		/// </summary>
		/// <param name="frmMain"></param>
		public Controller(Control frmMain)
		{
			m_dBestInHistory = double.MinValue;
			m_NumSweepers = Params.iNumSweepers;
			m_pGA = null;
			m_bFastRender = false;
			m_iTicks = 0;
			m_NumMines = Params.iNumMines;
			m_iGenerations = 0;

			m_Main = frmMain;
			//cxClient = Params.WindowWidth;
			//cyClient = Params.WindowHeight;
			cxClient = frmMain.ClientSize.Width;
			cyClient = frmMain.ClientSize.Height;

			m_vecAvFitness = new List<double>();
			m_vecBestFitness = new List<double>();

			//let's create the mine sweepers
			m_Sweepers = new Minesweeper[m_NumSweepers];
			for (int i = 0; i < m_NumSweepers; ++i)
				m_Sweepers[i] = new Minesweeper();

			//get the total number of weights used in the sweepers
			//NN so we can initialise the GA
			m_NumWeightsInNN = m_Sweepers[0].GetNumberOfWeights();

			//initialize the Genetic Algorithm class
			m_pGA = new GenAlg(m_NumSweepers, Params.dMutationRate, Params.dCrossoverRate, m_NumWeightsInNN);

			//Get the weights from the GA and insert into the sweepers brains
			m_ThePopulation = m_pGA.GetChromos();

			for (int i = 0; i < m_NumSweepers; i++)
				m_Sweepers[i].PutWeights(ref m_ThePopulation[i].vecWeights);

			//initialize mines in random positions within the application window
			m_Mines = new Vector2D[m_NumMines];
			for (int i = 0; i < m_NumMines; ++i)
				m_Mines[i] = Vector2D.GetRandom(cxClient, cyClient);

			//create a pen for the graph drawing
			m_fittPen = Pens.Red;
			m_sweepPen = Pens.Black;
			m_minePen = Pens.Green; //CreatePen(PS_SOLID, 1, RGB(0, 150, 0));

			//fill the vertex buffers
			m_SweeperVB = new PointF[NumSweeperVerts];
			Array.Copy(sweeper, m_SweeperVB, NumSweeperVerts);

			m_MineVB = new PointF[NumMineVerts];
			Array.Copy(mine, m_MineVB, NumMineVerts);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="frmMain"></param>
		public void ControlChangedSize(Control frmMain)
		{
			m_Main = frmMain;

			cxClient = frmMain.ClientSize.Width;
			cyClient = frmMain.ClientSize.Height;
			Params.WindowWidth = cxClient;
			Params.WindowHeight = cyClient;

			for (int i = 0; i < m_NumMines; ++i)
			{
				while (m_Mines[i].X >= cxClient)			m_Mines[i].X -= cxClient;
				while (m_Mines[i].Y >= cyClient)			m_Mines[i].Y -= cyClient;
			}

			int oldNum = m_NumMines;
			m_NumMines = (int)(Math.Pow(cxClient * cyClient, 0.75) / 200.0);

			Array.Resize(ref m_Mines, m_NumMines);
			
			for (int i = oldNum; i < m_NumMines; ++i)
				m_Mines[i] = Vector2D.GetRandom(cxClient, cyClient);
		}

		//public ~Controller() { }

		/// <summary>
		/// WorldTransform -
		///	sets up the translation matrices for the mines and applies the
		///	world transform to each vertex in the vertex buffer passed to this
		///	method.
		/// </summary>
		/// <param name="VBuffer"></param>
		/// <param name="vPos"></param>
		public void WorldTransform(ref PointF[] VBuffer, Vector2D vPos)
		{
			//create the world transformation matrix
			Matrix2D matTransform = new Matrix2D();

			//scale
			matTransform.Scale(Params.dMineScale, Params.dMineScale);

			//translate
			matTransform.Translate(vPos);

			//transform the ships vertices
			matTransform.TransformPoints(ref VBuffer);
		}

		/// <summary>
		/// Update -
		///	This is the main workhorse. The entire simulation is controlled from here.
		///	The comments should explain what is going on adequately.
		/// </summary>
		/// <returns></returns>
		public bool Update()
		{
			//run the sweepers through Params::iNumTicks amount of cycles. During
			//this loop each sweepers NN is constantly updated with the appropriate
			//information from its surroundings. The output from the NN is obtained
			//and the sweeper is moved. If it encounters a mine its fitness is
			//updated appropriately,

			if (m_iTicks++ < Params.iNumTicks)
			{
				for (int i = 0; i < m_NumSweepers; ++i)
				{
					//update the NN and position
					if (!m_Sweepers[i].Update(ref m_Mines))
					{
						//error in processing the neural net
						MessageBox.Show(m_Main, "Wrong amount of NN inputs!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return false;
					}

					//see if it's found a mine
					int GrabHit = m_Sweepers[i].CheckForMine(ref m_Mines, Params.dMineScale);

					if (GrabHit >= 0)
					{
						//we have discovered a mine so increase fitness
						m_Sweepers[i].IncrementFitness();

						//mine found so replace the mine with another at a random position
						m_Mines[GrabHit] = Vector2D.GetRandom(cxClient, cyClient);
						//	Vector2D(RandFloat() * cxClient, RandFloat() * cyClient);
					}

					//update the chromos fitness score
					m_ThePopulation[i].dFitness = m_Sweepers[i].Fitness();
				}
			}
			//Another generation has been completed.
			//Time to run the GA and update the sweepers with their new NNs
			else
			{
				//update the stats to be used in our stat window
				m_vecAvFitness.Add(m_pGA.AverageFitness());
				m_vecBestFitness.Add(m_pGA.BestFitness());

				//increment the generation counter
				++m_iGenerations;

				//reset cycles
				m_iTicks = 0;

				//run the GA to create a new population
				m_ThePopulation = m_pGA.Epoch(ref m_ThePopulation);

				//insert the new (hopefully)improved brains back into the sweepers
				//and reset their positions etc
				for (int i = 0; i < m_NumSweepers; ++i)
				{
					m_Sweepers[i].PutWeights(ref m_ThePopulation[i].vecWeights);
					m_Sweepers[i].Reset();
				}
			}

			return true;
		}

		static int step = 0;

		/// <summary>
		/// Render 
		/// </summary>
		/// <param name="surface"></param>
		public void Render(Graphics surface)
		{
			Font font = new Font(FontFamily.GenericMonospace, 11f, FontStyle.Bold);
			//Font font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold);
			//render the stats
			surface.DrawString("Generation:      " + m_iGenerations, font, Brushes.Black, 5, 0);
			surface.DrawString("Best Fitness:    " + m_pGA.BestFitness().ToString("0.####"), font, Brushes.Black, 5, 20);
			surface.DrawString("Average Fitness: " + m_pGA.AverageFitness().ToString("0.####"), font, Brushes.Black, 5, 40);

			//do not render if running at accelerated speed
			if (m_bFastRender)
			{
				PlotStats(surface);
				return;
			}

			if (Params.iRenderer == 1)
			{
				int x, y;
				//render the mines
				for (int i = 0; i < m_NumMines; ++i)
				{
					x = (int)(m_Mines[i].X) - Properties.Resources.Syrup.Width / 2;
					y = (int)(m_Mines[i].Y) - Properties.Resources.Syrup.Height / 2;

					surface.DrawImage(Properties.Resources.Syrup, x, y, Properties.Resources.Syrup.Width, Properties.Resources.Syrup.Height);
				}

				ImageAttributes imageAttributes = new ImageAttributes();
				imageAttributes.SetColorKey(Color.FromArgb(250, 250, 250, 250), Color.White);

				//we want the fittest displayed in red
				Bitmap ant = Properties.Resources.bestAnt;
#if !UseSweeperWorldTransform

				int w;
				if ((side & 2) != 0)
				{
					w = ant.Width;
					x = -ant.Width / 2;
				}
				else
				{
					w = -ant.Width;
					x = ant.Width / 2;
				}

				y = -ant.Height / 2;
				Rectangle rect = new Rectangle(x, y, w, ant.Height);

				//render the sweepers
				for (int i = 0; i < m_NumSweepers; i++)
				{
					if (i == Params.iNumElite)
						ant = Properties.Resources.Ant;

					Vector2D pos = m_Sweepers[i].Position();
					float rot = (float)(m_Sweepers[i].Rotation() * 180 / Math.PI);

					//draw ant image
					surface.TranslateTransform((float)pos.X, (float)pos.Y);
					surface.RotateTransform(rot);
					surface.DrawImage(ant, rect, 0, 0, ant.Width, ant.Height, GraphicsUnit.Pixel, imageAttributes);
					surface.ResetTransform();
				}
#else
				PointF[] sweeper3 = new PointF[3];
				float w = (float)ant.Width / Params.iSweeperScale;
				float h = (float)ant.Height / Params.iSweeperScale;

				if ((step & 2) != 0)
				{
					sweeper3[0] = new PointF(-0.5f * w, -0.5f * h);
					sweeper3[1] = new PointF(0.5f * w, -0.5f * h);
					sweeper3[2] = new PointF(-0.5f * w, 0.5f * h);
				}
				else
				{
					sweeper3[0] = new PointF(0.5f * w, -0.5f * h);
					sweeper3[1] = new PointF(-0.5f * w, -0.5f * h);
					sweeper3[2] = new PointF(0.5f * w, 0.5f * h);
				}

				Rectangle rect = new Rectangle(0, 0, ant.Width, ant.Height);

				PointF[] sweeperVB = new PointF[3];

				for (int i = 0; i < m_NumSweepers; i++)
				{
					if (i == Params.iNumElite)
						ant = Properties.Resources.Ant;

					Array.Copy(sweeper3, sweeperVB, sweeper3.Length);

					//transform the vertex buffer
					m_Sweepers[i].WorldTransform(ref sweeperVB);

					//draw ant image
					surface.DrawImage(ant, sweeperVB, rect, GraphicsUnit.Pixel, imageAttributes);
					//Application.DoEvents();
				}
#endif
				step++;
			}
			else
			{
				//render the mines
				PointF[] mineVB = new PointF[m_MineVB.Length];
				for (int i = 0; i < m_NumMines; ++i)
				{
					//grab the vertices for the mine shape
					Array.Copy(m_MineVB, mineVB, m_MineVB.Length);

					WorldTransform(ref mineVB, m_Mines[i]);

					//draw the mines
					PointF pt0 = mineVB[0];
					for (int vert = 1; vert < mineVB.Length; ++vert)
					{
						surface.DrawLine(m_minePen, pt0, mineVB[vert]);
						pt0 = mineVB[vert];
					}
					surface.DrawLine(m_minePen, pt0, mineVB[0]);
				}

				//we want the fittest displayed in red
				Pen swPen = m_fittPen;

				//render the sweepers
				PointF[] sweeperVB = new PointF[m_SweeperVB.Length];
				for (int i = 0; i < m_NumSweepers; i++)
				{
					if (i == Params.iNumElite)
						swPen = m_sweepPen;

					//grab the sweeper vertices
					Array.Copy(m_SweeperVB, sweeperVB, m_SweeperVB.Length);

					//transform the vertex buffer
					m_Sweepers[i].WorldTransform(ref sweeperVB);

					//draw the sweeper left track
					for (int vert = 1; vert < 4; ++vert)
						surface.DrawLine(swPen, sweeperVB[vert - 1], sweeperVB[vert]);

					surface.DrawLine(swPen, sweeperVB[3], sweeperVB[0]);

					//draw the sweeper right track
					for (int vert = 5; vert < 8; ++vert)
						surface.DrawLine(swPen, sweeperVB[vert - 1], sweeperVB[vert]);
					surface.DrawLine(swPen, sweeperVB[7], sweeperVB[4]);
					//================================================================
					surface.DrawLine(swPen, sweeperVB[8], sweeperVB[9]);

					for (int vert = 11; vert < 16; ++vert)
						surface.DrawLine(swPen, sweeperVB[vert - 1], sweeperVB[vert]);
					surface.DrawLine(swPen, sweeperVB[15], sweeperVB[10]);
				}
			}
		}

		///<summary>
		/// PlotStats -
		/// this function plots a graph of the average and best fitnesses
		/// over the course of a run
		///  Given a surface to draw on this function displays stats and a crude
		///  graph showing best and average fitness
		///</summary>
		/// <param name="surface"></param>
		private void PlotStats(Graphics surface)
		{
			double bestFit = m_pGA.BestFitness();
			//Font font = new Font(FontFamily.GenericMonospace, 11f, FontStyle.Bold);

			//string s = "Best Fitness:        " + bestFit;
			//surface.DrawString(s, font, Brushes.Black, 5, 20);
			//s = "Average Fitness:     " + m_pGA.AverageFitness();
			//surface.DrawString(s, font, Brushes.Black, 5, 40);

			if (m_dBestInHistory < bestFit)
				m_dBestInHistory = bestFit;

			//render the graph
			float HSlice = (float)(cxClient / (m_iGenerations + 1.0));
			float VSlice = (float)(cyClient / ((m_dBestInHistory + 1.0) * 1.5));

			//plot the graph for the best fitness
			float x = 0;

			PointF pt0 = default, pt1 = default;
			pt0.X = 0;
			pt0.Y = cyClient;

			for (int i = 0; i < m_vecBestFitness.Count; ++i)
			{
				pt1.X = x;
				pt1.Y = (float)(cyClient - VSlice * m_vecBestFitness[i]);
				surface.DrawLine(Pens.Red, pt0, pt1);

				pt0 = pt1;
				x += HSlice;
			}

			//plot the graph for the average fitness
			x = 0;
			pt0.X = 0;
			pt0.Y = cyClient;

			for (int i = 0; i < m_vecAvFitness.Count; ++i)
			{
				pt1.X = x;
				pt1.Y = (float)(cyClient - VSlice * m_vecAvFitness[i]);

				surface.DrawLine(Pens.Blue, pt0, pt1);

				pt0 = pt1;
				x += HSlice;
			}
		}

		//accessor methods
		public bool FastRender
		{
			get { return m_bFastRender; }
			set { m_bFastRender = value; }
		}

		public void RendererToggle() { Params.iRenderer = 1 - Params.iRenderer; }
		public void FastRenderToggle() { m_bFastRender = !m_bFastRender; }
	}
}
