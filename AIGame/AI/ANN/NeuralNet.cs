using System;
using System.Collections.Generic;
using System.Text;

namespace AIGame.AI.ANN
{
    class NeuralNet
    {
        private int _numInputs;
        private int _numOutputs;
        private int _numHiddenLayers;
        private int _neuronsPerHiddenLayer;

        private List<double> _inputs;
        public List<double> Inputs
        {
            get { return _inputs; }
        }

        private List<double> _outputs;
        public List<double> Outputs
        {
            get { return _outputs; }
        }

        private List<double> _outputsBeforeSig = new List<double>();
        public List<double> OutputsBeforeSig
        {
            get { return _outputsBeforeSig; }
        }

        private List<NeuronLayer> _layers;
        internal List<NeuronLayer> Layers
        {
            get { return _layers; }
        }

        public NeuralNet(int numInputs, int numOutputs, int numHiddenLayers, int neuronsPerHiddenLayer) 
        {
            _numInputs = numInputs;
            _numOutputs = numOutputs;
            _numHiddenLayers = numHiddenLayers;
            _neuronsPerHiddenLayer = neuronsPerHiddenLayer;
            _layers = new List<NeuronLayer>();

            if (_numHiddenLayers > 0)
            {
                _layers.Add(new NeuronLayer(_neuronsPerHiddenLayer, _numInputs));

                for (int i = 0; i < _numHiddenLayers - 1; ++i)
                    _layers.Add(new NeuronLayer(_neuronsPerHiddenLayer, _neuronsPerHiddenLayer));

                _layers.Add(new NeuronLayer(_numOutputs, _neuronsPerHiddenLayer));
            }
            else
                _layers.Add(new NeuronLayer(_numOutputs, _numInputs));
        }

        public List<int> CalculateSplitPoints()
        {
            List<int> splitPoints = new List<int>();
            int weightCounter = 0;
            for (int i = 0; i < _numHiddenLayers + 1; i++)
            {
                for (int j = 0; j < _layers[i].NumOfNeurons; j++)
                {
                    for (int k = 0; k < _layers[i].Neurons[j].NumInputs; k++)
                    {
                        ++weightCounter;
                    }
                    splitPoints.Add(weightCounter - 1);
                }
            }
            return splitPoints;
        }

        public List<double> Update(List<double> inputs)
        {
            _inputs = inputs;

            List<double> outputs = new List<double>();
            int i = 0;

            if (inputs.Count != _numInputs)
                return outputs;

            for (i = 0; i < _numHiddenLayers + 1; i++)
            {
                if (i > 0)
                    inputs = new List<double>(outputs.ToArray());
                outputs.Clear();
                if (i == _numHiddenLayers)
                    _outputsBeforeSig.Clear();

                int weight = 0;

                for (int j = 0; j < _layers[i].NumOfNeurons; j++)
                {
                    double netinput = 0;
                    int numInputs = _layers[i].Neurons[j].NumInputs;

                    for (int k = 0; k < numInputs; k++)
                        netinput += _layers[i].Neurons[j].Weights[k] * inputs[weight++];

                    netinput += _layers[i].Neurons[j].Weights[numInputs - 1] * AIParams.Bias;

                    if (i == _numHiddenLayers)
                        _outputsBeforeSig.Add(netinput);
                    outputs.Add(Sigmoid(netinput, AIParams.ActivationResponse));

                    weight = 0;
                }
            }

            _outputs = outputs;

            return outputs;
        }

        public List<double> GetWeights()
        {
            List<double> weights = new List<double>();
        	
	        for (int i=0; i<_numHiddenLayers + 1; ++i)
                for (int j = 0; j < _layers[i].Neurons.Count; ++j)
                    for (int k = 0; k < _layers[i].Neurons[j].NumInputs; ++k)
                        weights.Add(_layers[i].Neurons[j].Weights[k]);

	        return weights;
        }

        public void PutWeights(List<double> weights)
        {
            int weight = 0;
            for (int i = 0; i < _numHiddenLayers + 1; ++i)
                for (int j = 0; j < _layers[i].Neurons.Count; ++j)
                    for (int k = 0; k < _layers[i].Neurons[j].NumInputs; ++k)
                        _layers[i].Neurons[j].Weights[k] = weights[weight++];
        }

        public int GetNumberOfWeights()
        {
	        int weights = 0;
        	
            for (int i = 0; i < _numHiddenLayers + 1; ++i)
                for (int j = 0; j < _layers[i].NumOfNeurons; ++j)
                    weights += _layers[i].Neurons[j].NumInputs; 

	        return weights;
        }

        private double Sigmoid(double netinput, double activationResponse)
        {
            return 1 / (1 + Math.Exp(-netinput / activationResponse));
        }

    }
}
