using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SmartSweepers
{
	internal class Params
	{
		//------------------------------------general parameters
		public const double dPi = Math.PI;
		public const double dHalfPi = Math.PI / 2;
		public const double dTwoPi = Math.PI * 2;

		public static int WindowWidth = 400;
		public static int WindowHeight = 400;

		public static bool bFullScreen = false;
		public static int iFramesPerSecond = 60;

		//-------------------------------------used for the neural network
		public static int iNumInputs = 4;
		public static int iNumHidden = 1;
		public static int iNeuronsPerHiddenLayer = 6;
		public static int iNumOutputs = 2;

		//for tweeking the sigmoid function
		public static double dActivationResponse = 1;
		//bias value
		public static double dBias = -1;

		//--------------------------------------used to define the sweepers

		//limits how fast the sweepers can turn
		public static double dMaxTurnRate = 0.3;
		public static double dMaxSpeed = 2;

		//for controlling the size
		public static int iSweeperScale = 5;

		//--------------------------------------controller parameters
		public static int iNumSweepers = 30;
		public static int iNumMines = 40;

		//number of time steps we allow for each generation to live
		public static int iNumTicks = 2000;

		//scaling factor for mines
		public static double dMineScale = 2;

		//---------------------------------------GA parameters
		public static double dCrossoverRate = 0.7;
		public static double dMutationRate = 0.1;

		//the maximum amount the ga may mutate each weight by
		public static double dMaxPerturbation = 0.3;

		//used for elitism
		public static int iNumElite = 4;
		public static int iNumCopiesElite = 1;
		public static int iRenderer = 1;
		private Dictionary<string, object> paramsDictionary;

		//ctor
		public Params()
		{
			fillMapDefaultValies();

			string path = Path.GetDirectoryName(Application.ExecutablePath);
			if (!LoadInParameters(path + "\\params.ini"))
			{
				System.Windows.Forms.MessageBox.Show("Cannot find ini file!\r\nThe default values apply!", "Warning", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
			}
		}

		void fillMapDefaultValies()
		{
			paramsDictionary = new Dictionary<string, object>();
			paramsDictionary["iFramesPerSecond"] = 60;
			paramsDictionary["iNumInputs"] = 4;
			paramsDictionary["iNumHidden"] = 1;
			paramsDictionary["iNeuronsPerHiddenLayer"] = 6;
			paramsDictionary["iNumOutputs"] = 2;
			paramsDictionary["dActivationResponse"] = 1;
			paramsDictionary["dBias"] = -1;
			paramsDictionary["dMaxTurnRate"] = 0.3;
			paramsDictionary["dMaxSpeed"] = 2;
			paramsDictionary["iSweeperScale"] = 5;
			paramsDictionary["iNumMines"] = 40;
			paramsDictionary["iNumSweepers"] = 30;
			paramsDictionary["iNumTicks"] = 2000;
			paramsDictionary["dMineScale"] = 2;
			paramsDictionary["dCrossoverRate"] = 0.7;
			paramsDictionary["dMutationRate"] = 0.1;
			paramsDictionary["dMaxPerturbation"] = 0.3;
			paramsDictionary["iNumElite"] = 4;
			paramsDictionary["iNumCopiesElite"] = 1;
			paramsDictionary["iRenderer"] = 1;
			paramsDictionary["iFullScreen"] = false;
		}

		void LoadMapValiesFromFile(string FileName)
		{
			using (StreamReader sr = File.OpenText(FileName))
			{
				string str = "";

				while ((str = sr.ReadLine()) != null)
				{
					try
					{
						string[] ParamString = str.Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);

						if (ParamString.Length < 2)
							continue;

						if (int.TryParse(ParamString[1], out int iVal))
							paramsDictionary[ParamString[0]] = iVal;
						else if (double.TryParse(ParamString[1], out double dVal))
							paramsDictionary[ParamString[0]] = dVal;
					}
					catch
					{
						continue;
					}
				}
			}

		}

		bool LoadInParameters(string FileName)
		{
			try
			{
				LoadMapValiesFromFile(FileName);

				iFramesPerSecond = System.Convert.ToInt32(paramsDictionary["iFramesPerSecond"]);
				iNumInputs = System.Convert.ToInt32(paramsDictionary["iNumInputs"]);
				iNumHidden = System.Convert.ToInt32(paramsDictionary["iNumHidden"]);
				iNeuronsPerHiddenLayer = System.Convert.ToInt32(paramsDictionary["iNeuronsPerHiddenLayer"]);
				iNumOutputs = System.Convert.ToInt32(paramsDictionary["iNumOutputs"]);

				dActivationResponse = System.Convert.ToDouble(paramsDictionary["dActivationResponse"]);
				dBias  = System.Convert.ToDouble(paramsDictionary["dBias"]);
				dMaxTurnRate = System.Convert.ToDouble(paramsDictionary["dMaxTurnRate"]);
				dMaxSpeed = System.Convert.ToDouble(paramsDictionary["dMaxSpeed"]);

				iSweeperScale = System.Convert.ToInt32(paramsDictionary["iSweeperScale"]);
				iNumMines  = System.Convert.ToInt32(paramsDictionary["iNumMines"]);
				iNumSweepers = System.Convert.ToInt32(paramsDictionary["iNumSweepers"]);
				iNumTicks = System.Convert.ToInt32(paramsDictionary["iNumTicks"]);

				dMineScale = System.Convert.ToDouble(paramsDictionary["dMineScale"]);
				dCrossoverRate = System.Convert.ToDouble(paramsDictionary["dCrossoverRate"]);
				dMutationRate = System.Convert.ToDouble(paramsDictionary["dMutationRate"]);
				dMaxPerturbation = System.Convert.ToDouble(paramsDictionary["dMaxPerturbation"]);

				iNumElite = System.Convert.ToInt32(paramsDictionary["iNumElite"]);
				iNumCopiesElite = System.Convert.ToInt32(paramsDictionary["iNumCopiesElite"]);
				iRenderer = System.Convert.ToInt32(paramsDictionary["iRenderer"]);
				bFullScreen = System.Convert.ToBoolean(paramsDictionary["iFullScreen"]);

				return true;
			}
			catch
			{
				return false;
			}
		}

		bool LoadInParameters_(string FileName)
		{
			using (FileStream fs = new FileStream(FileName, FileMode.Open))
			{
				try
				{
					//FileStream fs = new FileStream(FileName, FileMode.Open);
					TextReader tr = new StreamReader(fs);

					//load in from the file

					iFramesPerSecond = getIntValue(tr.ReadLine(), 60);

					iNumInputs = getIntValue(tr.ReadLine(), 4);

					iNumHidden = getIntValue(tr.ReadLine(), 1);

					iNeuronsPerHiddenLayer = getIntValue(tr.ReadLine(), 6);

					iNumOutputs = getIntValue(tr.ReadLine(), 2);

					dActivationResponse = getDoubleValue(tr.ReadLine(), 1);

					dBias = getDoubleValue(tr.ReadLine(), -1);

					dMaxTurnRate = getDoubleValue(tr.ReadLine(), 0.3);

					dMaxSpeed = getDoubleValue(tr.ReadLine(), 2);

					iSweeperScale = getIntValue(tr.ReadLine(), 5);

					iNumMines = getIntValue(tr.ReadLine(), 20);

					iNumSweepers = getIntValue(tr.ReadLine(), 30);

					iNumTicks = getIntValue(tr.ReadLine(), 2000);

					dMineScale = getDoubleValue(tr.ReadLine(), 2);

					dCrossoverRate = getDoubleValue(tr.ReadLine(), 0.7);

					dMutationRate = getDoubleValue(tr.ReadLine(), 0.1);

					dMaxPerturbation = getDoubleValue(tr.ReadLine(), 0.3);

					iNumElite = getIntValue(tr.ReadLine(), 4);

					iNumCopiesElite = getIntValue(tr.ReadLine(), 1);

					iRenderer = getIntValue(tr.ReadLine(), 0);

					fs.Close();
					return true;
				}
				catch
				{
					fs.Close();
					return false;
				}
			}
		}

		int getIntValue(string str, int defValue)
		{
			try
			{
				string[] ParamString = str.Split(new char[]{ ' ', '='}, StringSplitOptions.RemoveEmptyEntries);
				return int.Parse(ParamString[1]);
			}
			catch
			{
				return defValue;
			}
		}

		double getDoubleValue(string str, double defValue)
		{
			try
			{
				string[] ParamString = str.Split(new char[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
				return double.Parse(ParamString[1]);
			}
			catch
			{
				return defValue;
			}
		}

	}
}
