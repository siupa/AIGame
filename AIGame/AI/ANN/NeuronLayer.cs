using System;
using System.Collections.Generic;
using System.Text;

namespace AIGame.AI.ANN
{
    class NeuronLayer
    {
        private List<Neuron> _neurons;

        public int NumOfNeurons
        {
            get { return _neurons.Count; }
        }

        internal List<Neuron> Neurons
        {
            get { return _neurons; }
        }

        public NeuronLayer(int numOfNeurons, int numInputs)
        {
            _neurons = new List<Neuron>();
            for (int i = 0; i < numOfNeurons; i++)
                _neurons.Add(new Neuron(numInputs));
        }
    }
}
