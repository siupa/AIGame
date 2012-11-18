using System;
using System.Collections.Generic;
using System.Text;

namespace AIGame.AI.ANN
{
    class Neuron
    {
        private int _numInputs;
        private List<double> _weights;

        public int NumInputs
        {
            get { return _numInputs; }
        }
        public List<double> Weights
        {
            get { return _weights; }
        }

        public Neuron(int numInputs)
        {
            _numInputs = numInputs;
            _weights = new List<double>();
            for (int i = 0; i < numInputs + 1; i++)
                _weights.Add(AIUtils.RandomClamped());
        }

        
    }
}
