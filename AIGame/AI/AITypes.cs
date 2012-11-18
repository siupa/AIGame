using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGame.AI
{
    public enum FitnessScallingType
    {
        Ranking,
        Sigma,
        Boltzmann,
        /// <summary>
        /// Scalling only when worst fitness is belowe zero, and it is adding worst value to all of the fitnesses.
        /// </summary>
        NoneUnsigned
    }
}
