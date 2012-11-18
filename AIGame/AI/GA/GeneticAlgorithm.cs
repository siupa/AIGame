using System;
using System.Collections.Generic;

namespace AIGame.AI.GA
{
    class GenomePair
    {
        public string Id { get; set; }

        internal Genome Genome { get; set; }

        public GenomePair(string id, GA.Genome gen)
        {
            Id = id;
            Genome = gen;
        }

        public static Comparison<GenomePair> GenomePairComparison = new Comparison<GenomePair>(
            delegate(GenomePair g1, GenomePair g2)
            {
                if (g1.Genome.Fitness > g2.Genome.Fitness) return 1;
                else if (g1.Genome.Fitness == g2.Genome.Fitness) return 0;
                else return -1;
            });
    }

    class GeneticAlgorithm
    {
        #region Fields
        private List<GenomePair> _population;
        private int _populationSize;
        private int _populationCounter;
        //amount of weights per chromo
        private int _chromosomeLength;
        //total fitness of population
        private double _totalFitness;
        //total fitness of population for boltzmann scaling calculations
        private double _totalFitnessForChromoRoulette;
        //best fitness this population
        private double _bestFitness;
        //average fitness
        private double _averageFitness;
        //worst
        public double WorstFitness { get; private set; }

        public double Sigma { get; private set; }

        private AIObject _fittestObject;

        //probability that a chromosones bits will mutate.
        //Try figures around 0.05 to 0.3 ish
        private double _mutationRate;
        //probability of chromosones crossing over bits
        //0.7 is pretty good
        private double _crossoverRate;

        private List<int> _crossoverSplitPoints;

        //generation counter
        private int _generation;
        private double _boltzmannTemperature;
        
        #endregion

        #region Properties
        internal List<GenomePair> Population
        {
            get { return _population; }
        }

        public double BestFitness
        {
            get { return _bestFitness; }
            set { _bestFitness = value; }
        }

        public double AverageFitness
        {
            get { return _averageFitness; }
            set { _averageFitness = value; }
        }

        public AIObject FittestObject
        {
            get { return _fittestObject; }
            set { _fittestObject = value; }
        }
        #endregion

        #region Constructor
        public GeneticAlgorithm(int popsize, double mutationRate, double crossoverRate, int numWeights, List<int> splitPoints)
        {
            _population = new List<GenomePair>();
            _populationSize = popsize;
            _populationCounter = popsize;
            _mutationRate = mutationRate;
            _crossoverRate = crossoverRate;
            _chromosomeLength = numWeights;
            _totalFitness = 0;
            _generation = 0;
            _bestFitness = 0;
            WorstFitness = double.MaxValue;
            _averageFitness = 0;
            _boltzmannTemperature = AIParams.BoltzmannTemperature;
            _crossoverSplitPoints = splitPoints;

            //initialise population with chromosomes consisting of random
            //weights and all fitnesses set to zero
            for (int i = 0; i < _populationSize; ++i)
            {
                _population.Add(new GenomePair("", new Genome()));
                for (int j = 0; j < _chromosomeLength; ++j)
                    _population[i].Genome.Weights.Add(AIUtils.RandomClamped());
            }
        } 
        #endregion

        #region Private Methods
        private void CalculateFitnesses()
        {
            _totalFitness = 0;
            double highestSoFar = 0;
            double lowestSoFar = double.MaxValue;

            foreach (GenomePair genpair in _population)
            {
                if (genpair.Genome.Fitness > highestSoFar)
                {
                    highestSoFar = genpair.Genome.Fitness;
                    //_fittestGenome = i;
                    _bestFitness = highestSoFar;
                }

                if (genpair.Genome.Fitness < lowestSoFar)
                {
                    lowestSoFar = genpair.Genome.Fitness;
                    WorstFitness = lowestSoFar;
                }
                _totalFitness += genpair.Genome.Fitness;
            }
            _averageFitness = _totalFitness / _populationSize;
            _totalFitnessForChromoRoulette = _totalFitness;
        }
        private void ScaleFitnesses()
        {
            // scale fitness scores
            if (WorstFitness != _bestFitness)
            {
                _totalFitnessForChromoRoulette = 0;
                if (WorstFitness < 0)
                {
                    foreach (GenomePair genpair in _population)
                    {
                        genpair.Genome.Fitness += Math.Abs(WorstFitness);
                        _totalFitnessForChromoRoulette += genpair.Genome.Fitness;
                    }
                }
                else
                {
                    foreach (GenomePair genpair in _population)
                    {
                        genpair.Genome.Fitness -= WorstFitness;
                        _totalFitnessForChromoRoulette += genpair.Genome.Fitness;
                    }
                }
            }

            switch (AIParams.FitnessScalling)
            {
                case FitnessScallingType.Ranking:
                    ScaleFitnessRanking();
                    break;
                case FitnessScallingType.Boltzmann:
                    ScaleFitnessBoltzmann();
                    break;
                case FitnessScallingType.Sigma:
                    ScaleFitnessSigma();
                    break;
                default:
                    break;
            }
        }

        private void ScaleFitnessRanking()
        {
            _totalFitnessForChromoRoulette = 0;
            for (int i = 0; i < _population.Count; i++ )
            {
                _population[i].Genome.Fitness = i;
                _totalFitnessForChromoRoulette += _population[i].Genome.Fitness;
            }
        }

        private void ScaleFitnessBoltzmann()
        {
            _boltzmannTemperature -= AIParams.BoltzmannTemperatureStep;

            if (_boltzmannTemperature < AIParams.BoltzmannMinTemperature)
                _boltzmannTemperature = AIParams.BoltzmannMinTemperature;

            double temp_avg_fit = _totalFitnessForChromoRoulette / _populationSize;
            double divider = temp_avg_fit / _boltzmannTemperature;

            _totalFitnessForChromoRoulette = 0;
            foreach (GenomePair genpair in _population)
            {
                double oldFitness = genpair.Genome.Fitness;
                genpair.Genome.Fitness = (oldFitness / _boltzmannTemperature) / divider;
                _totalFitnessForChromoRoulette += genpair.Genome.Fitness;
            }
        }

        private void ScaleFitnessSigma()
        {
            double RunningTotal = 0;

            foreach (GenomePair genpair in _population)
            {
                RunningTotal += (genpair.Genome.Fitness - _averageFitness) *
                    (genpair.Genome.Fitness - _averageFitness);
            }
            double variance = RunningTotal / (double)_population.Count;
            Sigma = Math.Sqrt(variance);

            _totalFitnessForChromoRoulette = 0;
            foreach (GenomePair genpair in _population)
            {
                double OldFitness = genpair.Genome.Fitness;
                genpair.Genome.Fitness = (OldFitness - _averageFitness) / (2 * Sigma);
                _totalFitnessForChromoRoulette += genpair.Genome.Fitness;
            }

            // if fitnesses are lower then zero
            double LowestSoFar = double.MaxValue;
            foreach (GenomePair genpair in _population)
                if (genpair.Genome.Fitness < LowestSoFar)
                    LowestSoFar = genpair.Genome.Fitness;

            if (LowestSoFar < 0)
            {
                _totalFitnessForChromoRoulette = 0;
                foreach (GenomePair genpair in _population)
                {
                    genpair.Genome.Fitness += Math.Abs(LowestSoFar);
                    _totalFitnessForChromoRoulette += genpair.Genome.Fitness;
                }
            }

        }

        private GenomePair GetChromoRoulette()
        {
            //generate a random number between 0 & total fitness count
            double Slice = AIUtils.Random(_totalFitnessForChromoRoulette);
            //this will be set to the chosen chromosome
            GenomePair TheChosenOne = null;

            //go through the chromosones adding up the fitness so far
            double FitnessSoFar = 0;

            foreach (GenomePair genpair in _population)
            {
                FitnessSoFar += genpair.Genome.Fitness;
                if (FitnessSoFar >= Slice)
                {
                    TheChosenOne = genpair;
                    break;
                }
            }
            return TheChosenOne;
        }
        private void GrabNBest(int NBest, int NumCopies, ref List<GenomePair> Pop)
        {
            // !!! check if NBest count is correct
            while (NBest != 0)
            {
                for (int i = 0; i < NumCopies; ++i)
                    Pop.Add(_population[(_populationSize - 1) - NBest]);
                NBest--;
            }
        }
        private void Mutate(List<double> chromo)
        {
            for (int i = 0; i < chromo.Count; ++i)
            {
                if (AIUtils.Random(1.0) < _mutationRate)
                    chromo[i] += (AIUtils.RandomClamped() * AIParams.MaxPerturbation);
            }
        }
        private void Crossover(List<double> mum, List<double> dad, ref List<double> baby1, ref List<double> baby2)
        {
            if ((AIUtils.Random(1.0) > _crossoverRate) || (mum == dad))
            {
                baby1 = mum;
                baby2 = dad;
                return;
            }

            int cp = AIUtils.RandomInt(0, _chromosomeLength - 1);

            for (int i = 0; i < cp; ++i)
            {
                baby1.Add(mum[i]);
                baby2.Add(dad[i]);
            }

            for (int i = cp; i < mum.Count; ++i)
            {
                baby1.Add(dad[i]);
                baby2.Add(mum[i]);
            }
        }
        private void CrossoverSplitPoints(List<double> mum, List<double> dad, ref List<double> baby1, ref List<double> baby2)
        {
            if ((AIUtils.Random(1.0) > _crossoverRate) || (mum == dad))
            {
                baby1 = mum;
                baby2 = dad;
                return;
            }

            int i1 = AIUtils.RandomInt(0, _crossoverSplitPoints.Count - 2);
            int i2 = AIUtils.RandomInt(i1, _crossoverSplitPoints.Count - 1);

            int cp1 = _crossoverSplitPoints[i1];
            int cp2 = _crossoverSplitPoints[i2];

            for (int i = 0; i < mum.Count; ++i)
            {
                if ((i < cp1) || (i >= cp2))
                {
                    baby1.Add(mum[i]);
                    baby2.Add(dad[i]);
                }
                else
                {
                    baby1.Add(dad[i]);
                    baby2.Add(mum[i]);
                }
            }
        }
        #endregion

        #region Public Methods
        public List<GenomePair> Epoch(List<GenomePair> old_population)
        {
            _generation++;

            _population = old_population;
            _totalFitness = 0;
            _bestFitness = 0;
            WorstFitness = double.MaxValue;
            _averageFitness = 0;

            // this sort is requiered for further scale and fintess calculations!
            _population.Sort(GenomePair.GenomePairComparison);

            CalculateFitnesses();
            ScaleFitnesses();

            List<GenomePair> new_population = new List<GenomePair>();

            if (AIParams.NumCopiesElite * AIParams.NumElite % 2 == 0)
                GrabNBest(AIParams.NumElite, AIParams.NumCopiesElite, ref new_population);
            
            while (new_population.Count < _populationSize)
            {
                
                    GenomePair mum = GetChromoRoulette();
                    GenomePair dad = GetChromoRoulette();

                    if (mum == null)
                        mum = new_population[0];
                    if (dad == null)
                        dad = new_population[2];

                    List<double> baby1 = new List<double>();
                    List<double> baby2 = new List<double>();

                try
                {
                    if (AIParams.MultiPointCrossover)
                        CrossoverSplitPoints(mum.Genome.Weights, dad.Genome.Weights, ref baby1, ref baby2);
                    else
                        Crossover(mum.Genome.Weights, dad.Genome.Weights, ref baby1, ref baby2);

                    Mutate(baby1);
                    Mutate(baby2);

                    new_population.Add(new GenomePair("AIObjMut " + (++_populationCounter).ToString(), new Genome(baby1, 0)));
                    new_population.Add(new GenomePair("AIObjMut " + (++_populationCounter).ToString(), new Genome(baby2, 0)));
                }
                catch 
                {
                    new_population.Add(new_population[0]);
                    new_population.Add(new_population[2]);
                }
            }
            _population = new_population;

            return _population;
        }
        #endregion
    }
}
