using System;
using System.Collections.Generic;
using System.Text;

namespace AIGame.AI.GA
{
    class Genome
    {
        private List<double> _weights;
        private double _fitness;

        public List<double> Weights
        {
            get { return _weights; }
            set { _weights = value; }
        }
        public double Fitness
        {
            get { return _fitness; }
            set { _fitness = value; }
        }

        public Genome()
        {
            _weights = new List<double>();
            _fitness = 0;
        }
        public Genome(List<double> weights, double fitness)
        {
            _weights = weights;
            _fitness = fitness;
        }

        public static Comparison<Genome> GenomeComparison = new Comparison<Genome>(
            delegate(Genome g1, Genome g2)
            {
                if (g1._fitness > g2._fitness) return 1;
                else if (g1._fitness == g2._fitness) return 0;
                else return -1;
            });
    }
}
