using System;

namespace SmartSweepers
{
	//-------------------------------------------------------------------
	//	define neuron struct
	//-------------------------------------------------------------------
	internal struct Neuron
	{
		//the number of inputs into the neuron
		public int m_NumInputs;

		//the weights for each input
		public double[] m_vecWeight;

		//public double Output /???

		//ctor
		public Neuron(int NumInputs)
		{
			m_NumInputs = NumInputs + 1;
			m_vecWeight = new double[m_NumInputs];

			//set up the weights with an initial random value
			for (int i = 0; i < m_NumInputs; ++i)
				m_vecWeight[i] = Utils.RandomClamped();
		}
	}

	//---------------------------------------------------------------------
	//	struct to hold a layer of neurons.
	//---------------------------------------------------------------------

	internal struct NeuronLayer
	{
		//the number of neurons in this layer
		public int m_NumNeurons;

		//the layer of neurons
		public Neuron[] m_Neurons;

		public NeuronLayer(int NumNeurons, int NumInputsPerNeuron)
		{
			m_NumNeurons = NumNeurons;
			m_Neurons = new Neuron[NumNeurons];

			for (int i = 0; i < NumNeurons; i++)
				m_Neurons[i] = new Neuron(NumInputsPerNeuron);
		}
	}

	//----------------------------------------------------------------------
	//	neural net class
	//----------------------------------------------------------------------

	internal class NeuralNet
	{
		private int m_NumInputs;
		private int m_NumOutputs;
		private int m_NumHiddenLayers;
		private int m_NeuronsPerHiddenLyr;

		//storage for each layer of neurons including the output layer
		private NeuronLayer[] m_Layers;

		//------------------------------default ctor ----------------------------
		//
		//	creates a ANN based on the default values in params.ini
		//-----------------------------------------------------------------------
		public NeuralNet()
		{
			m_NumInputs = Params.iNumInputs;
			m_NumOutputs = Params.iNumOutputs;
			m_NumHiddenLayers = Params.iNumHidden;
			m_NeuronsPerHiddenLyr = Params.iNeuronsPerHiddenLayer;

			m_Layers = new NeuronLayer[m_NumHiddenLayers + 1];
			CreateNet();
		}

		//------------------------------createNet()------------------------------
		//
		//	this method builds the ANN. The weights are all initially set to 
		//	random values -1 < w < 1
		//------------------------------------------------------------------------
		public void CreateNet()
		{
			//create the layers of the network
			if (m_NumHiddenLayers > 0)
			{
				//create first hidden layer
				m_Layers[0] = new NeuronLayer(m_NeuronsPerHiddenLyr, m_NumInputs);

				for (int i = 1; i < m_NumHiddenLayers; ++i)
					m_Layers[i] = new NeuronLayer(m_NeuronsPerHiddenLyr, m_NeuronsPerHiddenLyr);

				//create output layer
				m_Layers[m_NumHiddenLayers] = new NeuronLayer(m_NumOutputs, m_NeuronsPerHiddenLyr);
			}
			else
				//create output layer
				m_Layers[m_NumHiddenLayers] = new NeuronLayer(m_NumOutputs, m_NumInputs);
		}

		//gets the weights from the NN
		//---------------------------------GetWeights-----------------------------
		//
		//	returns a vector containing the weights
		//
		//------------------------------------------------------------------------
		public double[] GetWeights()
		{
			//this will hold the weights
			double[] weights = new double[m_NumInputs + m_NumHiddenLayers * m_NeuronsPerHiddenLyr];

			int c = 0;
			//for each layer
			for (int i = 0; i < m_NumHiddenLayers + 1; ++i)
				//for each neuron
				for (int j = 0; j < m_Layers[i].m_NumNeurons; ++j)
					//for each weight
					for (int k = 0; k < m_Layers[i].m_Neurons[j].m_NumInputs; ++k)
						weights[c++] = m_Layers[i].m_Neurons[j].m_vecWeight[k];

			return weights;
		}

		//returns total number of weights in net
		//---------------------------------GetNumberOfWeights---------------------
		//
		//	returns the total number of weights needed for the net
		//
		//------------------------------------------------------------------------
		public int GetNumberOfWeights()
		{
			int weights = 0;

			//for each layer
			for (int i = 0; i < m_NumHiddenLayers + 1; ++i)
				//for each neuron
				for (int j = 0; j < m_Layers[i].m_NumNeurons; ++j)
					//for each weight
					weights += m_Layers[i].m_Neurons[j].m_NumInputs;

			return weights;
		}

		//replaces the weights with new ones
		//-----------------------------------PutWeights---------------------------
		//
		//	given a vector of doubles this function replaces the weights in the NN
		//  with the new values
		//
		//------------------------------------------------------------------------
		public void PutWeights(ref double[] weights)
		{
			int cWeight = 0;

			//for each layer
			for (int i = 0; i < m_NumHiddenLayers + 1; ++i)
				//for each neuron
				for (int j = 0; j < m_Layers[i].m_NumNeurons; ++j)
					//for each weight
					for (int k = 0; k < m_Layers[i].m_Neurons[j].m_NumInputs; ++k)
						m_Layers[i].m_Neurons[j].m_vecWeight[k] = weights[cWeight++];

			return;
		}

		//calculates the outputs from a set of inputs
		//-------------------------------Update-----------------------------------
		//
		//	given an input vector this function calculates the output vector
		//
		//------------------------------------------------------------------------
		public double[] Update(ref double[] input)
		{
			//stores the resultant outputs from each layer

			//first check that we have the correct amount of inputs
			if (input.Length != m_NumInputs)
				//just return an empty vector if incorrect.
				return new double[0];

			double[] inputs = input;
			double[] outputs = null;

			if (m_NumHiddenLayers > 0)
				outputs = new double[m_NeuronsPerHiddenLyr];
			//else
			//	outputs = new double[m_NumOutputs];

			//int cWeight = 0;
			//For each layer....
			for (int i = 0; i < m_NumHiddenLayers + 1; ++i)
			{
				if (i > 0)
				{
					inputs = outputs;
					if (i < m_NumHiddenLayers)
						outputs = new double[m_NeuronsPerHiddenLyr];
				}

				if (i >= m_NumHiddenLayers)
					outputs = new double[m_NumOutputs];

				//for each neuron sum the (inputs * corresponding weights).Throw 
				//the total at our sigmoid function to get the output.
				for (int j = 0; j < m_Layers[i].m_NumNeurons; ++j)
				{
					double netinput = 0;

					int NumInputs = m_Layers[i].m_Neurons[j].m_NumInputs;

					//for each weight
					for (int k = 0; k < NumInputs - 1; ++k)
						//sum the weights x inputs
						netinput += m_Layers[i].m_Neurons[j].m_vecWeight[k] * inputs[k];

					//add in the bias
					netinput += m_Layers[i].m_Neurons[j].m_vecWeight[NumInputs - 1] * Params.dBias;

					//we can store the outputs from each layer as we generate them. 
					//The combined activation is first filtered through the sigmoid 
					//function
					outputs[j] = Sigmoid(netinput, Params.dActivationResponse);
				}
			}

			return outputs;
		}

		//sigmoid response curve
		public double Sigmoid(double activation, double response)
		{
			return (1.0 / (1.0 + Math.Exp(-activation / response)));
		}
	}
}

