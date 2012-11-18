using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;

namespace AIGame.AI
{
    public class AIParams
    {
        #region Params
        // ANN
        public static double Bias = -1;
        public static double ActivationResponse = 1;
        public static int NumOutputs = 1;
        public static int NumInputs = 8;
        public static int NumHidden = 1;
        public static int NeuronsPerHiddenLayer = 10;

        // GA
        public static double MaxPerturbation = 0.3;
        public static int NumElite = 5;
        public static int NumCopiesElite = 2;
        public static double CrossoverRate = 0.7;
        public static bool MultiPointCrossover = false;
        public static double MutationRate = 0.1;

        public static FitnessScallingType FitnessScalling = FitnessScallingType.NoneUnsigned;

        public static double BoltzmannTemperature = 120.0;
        public static double BoltzmannTemperatureStep = 1;
        public static double BoltzmannMinTemperature = 1;

        // how long last one epoch in evolution (in frames)
        public static bool UPDATE_EVOLUTION_ENABLED = true;
        public static int UPDATE_EVOLUTION_TIME_PERIOD_FRAMES = 2000;
        public static bool UPDATE_LIFE_POINTS = true;

        public static double FITNESS_EXPERIENCE_MULTIPLICATION_FACTOR = 10.0;


        public static double FITNESS_INCREASE_MED = 2.0f;
        public static double FITNESS_DECREASE_MED = -1.0f;
        public static double FITNESS_INCREASE_BONUS = 3.0f;
        public static double FITNESS_DECREASE_BONUS = -1.0f;
        public static double FITNESS_INCREASE_GET_TO_COVER = 2.0f;
        public static double FITNESS_INCREASE_KILL = 5.0f;
        public static double FITNESS_INCREASE_KILL_CONCIDENCE = 1.0f;
        public static double FITNESS_INCREASE_DIE = -20.0f;
        public static double FITNESS_INCREASE_DIE_STRONG = -5.0f;
        public static double FITNESS_INCREASE_HIT = 0.01f;
        public static double FITNESS_INCREASE_GOT_HIT = -0.01f;

        public static bool FITNESS_INCREASE_WITH_TIME_ENABLE = true;
        public static int FITNESS_INCREASE_WITH_TIME_PERIOD_FRAMES = 700;
        public static double FITNESS_INCREASE_WITH_TIME = 20.0;

        public static float OBJECT_RANGE_LEADER = 700f;
        public static float OBJECT_RANGE_SNIPER = 700f;
        public static float OBJECT_RANGE_INFANTRY = 500f;
        public static float OBJECT_RANGE_SUPPORT = 450f;

        // AI Settings
        public static string PATH_APPLICATION = Directory.GetCurrentDirectory();

        public static bool CONFIG_DUMP_ALL_POPULATIONS = true;
        public static string CONFIG_DUMP_ALL_POPULATIONS_FILE_PATH = Path.Combine(PATH_APPLICATION, "data_populations.txt");
        public static bool CONFIG_DUMP_LAST_POPULATION = true;
        public static string CONFIG_DUMP_LAST_POPULATION_FILE_PATH = Path.Combine(PATH_APPLICATION, "data_last_population.txt");
        public static bool CONFIG_DUMP_ANN_VECTORS = false;
        public static int CONFIG_DUMP_ANN_VECTORS_NUM_PROBES = 10000;
        public static string CONFIG_DUMP_ANN_VECTORS_FILE_PATH = Path.Combine(PATH_APPLICATION, "data_ann_vectors.txt");

        public static bool CONFIG_GET_GENERATION_FROM_FILE = false;
        public static bool CONFIG_GET_GENERATION_FROM_FILE_ONLY_ELITE = false;
        public static string CONFIG_GET_GENERATION_FROM_FILE_PATH = Path.Combine(PATH_APPLICATION, "data_last_population.txt");

        public static bool CONFIG_LOW_RENDER = false;
        public static bool CONFIG_ENABLE_DEFAULT_LIGHTING = true;
        public static float CONFIG_TERREIN_HEIGHT_PARAMETER = 3;

        public static int CONFIG_NUMBER_AIOBJECTS_IN_TEAM = 20;

        public static int CONFIG_NUMBER_COVERS = 80;
        public static int CONFIG_NUMBER_MEDS = 80;
        public static int CONFIG_NUMBER_BONUSES = 80;

        public static float CONFIG_INCREASE_EXP_BONUS = 5.0f;
        public static float CONFIG_INCREASE_EXP_KILL = 5.0f;
        public static float CONFIG_INCREASE_MED = 0.2f;

        public static float CONFIG_DECREASE_OF_LIFE_WHEN_HIT = 0.005f;
        public static float CONFIG_COVER_ADD_POINTS = 0.003f;
        #endregion

        #region Methods

        private static string ReadParam(string paramName, string fileName)
        {
            using (TextReader tr = new StreamReader(fileName))
            {
                string line = tr.ReadLine();
                while (line != null)
                {
                    List<string> data = new List<string>();
                    string[] temp = line.Split(new char[] { ' ', '\t' });
                    foreach (string s in temp)
                        if (!string.IsNullOrEmpty(s))
                            data.Add(s);
                    if (data.Count < 2)
                    {
                        line = tr.ReadLine();
                        continue;
                    }

                    if (data[0].Equals(paramName))
                        return data[1];

                    line = tr.ReadLine();
                }
                throw new Exception("Failed set up AI params! Param '" + paramName + "' not found.");
            }
        }
        public static void SetUpAIParams(string fileName)
        {
            // set the temporary culture for importing
            string temp_culture = Thread.CurrentThread.CurrentCulture.Name;
            Thread.CurrentThread.CurrentCulture
                = new CultureInfo("en-US");

            //[ANN]
            Bias = double.Parse(ReadParam("Bias", fileName));
            ActivationResponse = double.Parse(ReadParam("ActivationResponse", fileName));
            NumOutputs = int.Parse(ReadParam("NumOutputs", fileName));
            NumInputs = int.Parse(ReadParam("NumInputs", fileName));
            NumHidden = int.Parse(ReadParam("NumHidden", fileName));
            NeuronsPerHiddenLayer = int.Parse(ReadParam("NeuronsPerHiddenLayer", fileName));

            //[GA]
            MaxPerturbation = double.Parse(ReadParam("MaxPerturbation", fileName));
            NumElite = int.Parse(ReadParam("NumElite", fileName));
            NumCopiesElite = int.Parse(ReadParam("NumCopiesElite", fileName));
            CrossoverRate = double.Parse(ReadParam("CrossoverRate", fileName));
            MultiPointCrossover = bool.Parse(ReadParam("MultiPointCrossover", fileName));
            MutationRate = double.Parse(ReadParam("MutationRate", fileName));

            switch (ReadParam("FitnessScalling", fileName))
            {
                case "Ranking":
                    FitnessScalling = FitnessScallingType.Ranking;
                    break;
                case "Boltzmann":
                    FitnessScalling = FitnessScallingType.Boltzmann;
                    break;
                case "Sigma":
                    FitnessScalling = FitnessScallingType.Sigma;
                    break;
                case "NoneUnsigned":
                    FitnessScalling = FitnessScallingType.NoneUnsigned;
                    break;
                default:
                    FitnessScalling = FitnessScallingType.NoneUnsigned;
                    break;
            }

            BoltzmannTemperature = double.Parse(ReadParam("BoltzmannTemperature", fileName));
            BoltzmannTemperatureStep = double.Parse(ReadParam("BoltzmannTemperatureStep", fileName));
            BoltzmannMinTemperature = double.Parse(ReadParam("BoltzmannMinTemperature", fileName));

            //[how long last one epoch in evolution (in rames)]

            UPDATE_EVOLUTION_ENABLED = bool.Parse(ReadParam("UPDATE_EVOLUTION_ENABLED", fileName));
            UPDATE_EVOLUTION_TIME_PERIOD_FRAMES = int.Parse(ReadParam("UPDATE_EVOLUTION_TIME_PERIOD_FRAMES", fileName));
            UPDATE_LIFE_POINTS = bool.Parse(ReadParam("UPDATE_LIFE_POINTS", fileName));

            FITNESS_EXPERIENCE_MULTIPLICATION_FACTOR = double.Parse(ReadParam("FITNESS_EXPERIENCE_MULTIPLICATION_FACTOR", fileName));


            FITNESS_INCREASE_MED = double.Parse(ReadParam("FITNESS_INCREASE_MED", fileName));
            FITNESS_DECREASE_MED = double.Parse(ReadParam("FITNESS_DECREASE_MED", fileName));
            FITNESS_INCREASE_BONUS = double.Parse(ReadParam("FITNESS_INCREASE_BONUS", fileName));
            FITNESS_DECREASE_BONUS = double.Parse(ReadParam("FITNESS_DECREASE_BONUS", fileName));
            FITNESS_INCREASE_GET_TO_COVER = double.Parse(ReadParam("FITNESS_INCREASE_GET_TO_COVER", fileName));
            FITNESS_INCREASE_KILL = double.Parse(ReadParam("FITNESS_INCREASE_KILL_CONCIDENCE", fileName));
            FITNESS_INCREASE_KILL_CONCIDENCE = double.Parse(ReadParam("FITNESS_INCREASE_KILL", fileName));
            FITNESS_INCREASE_DIE = double.Parse(ReadParam("FITNESS_INCREASE_DIE", fileName));
            FITNESS_INCREASE_DIE_STRONG = double.Parse(ReadParam("FITNESS_INCREASE_DIE_STRONG", fileName));
            FITNESS_INCREASE_HIT = double.Parse(ReadParam("FITNESS_INCREASE_HIT", fileName));
            FITNESS_INCREASE_GOT_HIT = double.Parse(ReadParam("FITNESS_INCREASE_GOT_HIT", fileName));
            FITNESS_INCREASE_WITH_TIME_ENABLE = bool.Parse(ReadParam("FITNESS_INCREASE_WITH_TIME_ENABLE", fileName));
            FITNESS_INCREASE_WITH_TIME_PERIOD_FRAMES = int.Parse(ReadParam("FITNESS_INCREASE_WITH_TIME_PERIOD_FRAMES", fileName));
            FITNESS_INCREASE_WITH_TIME = double.Parse(ReadParam("FITNESS_INCREASE_WITH_TIME", fileName));

            OBJECT_RANGE_LEADER = float.Parse(ReadParam("OBJECT_RANGE_LEADER", fileName));
            OBJECT_RANGE_SNIPER = float.Parse(ReadParam("OBJECT_RANGE_SNIPER", fileName));
            OBJECT_RANGE_INFANTRY = float.Parse(ReadParam("OBJECT_RANGE_INFANTRY", fileName));
            OBJECT_RANGE_SUPPORT = float.Parse(ReadParam("OBJECT_RANGE_SUPPORT", fileName));

            // [test configuration]
            CONFIG_DUMP_ALL_POPULATIONS = bool.Parse(ReadParam("CONFIG_DUMP_ALL_POPULATIONS", fileName));
            CONFIG_DUMP_ALL_POPULATIONS_FILE_PATH = Path.Combine(PATH_APPLICATION, ReadParam("CONFIG_DUMP_ALL_POPULATIONS_FILE_PATH", fileName));
            CONFIG_DUMP_LAST_POPULATION = bool.Parse(ReadParam("CONFIG_DUMP_LAST_POPULATION", fileName));
            CONFIG_DUMP_LAST_POPULATION_FILE_PATH = Path.Combine(PATH_APPLICATION, ReadParam("CONFIG_DUMP_LAST_POPULATION_FILE_PATH", fileName));
            CONFIG_DUMP_ANN_VECTORS = bool.Parse(ReadParam("CONFIG_DUMP_ANN_VECTORS", fileName));
            CONFIG_DUMP_ANN_VECTORS_NUM_PROBES = int.Parse(ReadParam("CONFIG_DUMP_ANN_VECTORS_NUM_PROBES", fileName));
            CONFIG_DUMP_ANN_VECTORS_FILE_PATH = Path.Combine(PATH_APPLICATION, ReadParam("CONFIG_DUMP_ANN_VECTORS_FILE_PATH", fileName));

            CONFIG_GET_GENERATION_FROM_FILE = bool.Parse(ReadParam("CONFIG_GET_GENERATION_FROM_FILE", fileName));
            CONFIG_GET_GENERATION_FROM_FILE_ONLY_ELITE = bool.Parse(ReadParam("CONFIG_GET_GENERATION_FROM_FILE_ONLY_ELITE", fileName));
            CONFIG_GET_GENERATION_FROM_FILE_PATH = Path.Combine(PATH_APPLICATION, ReadParam("CONFIG_GET_GENERATION_FROM_FILE_PATH", fileName));
            CONFIG_LOW_RENDER = bool.Parse(ReadParam("CONFIG_LOW_RENDER", fileName));
            CONFIG_ENABLE_DEFAULT_LIGHTING = bool.Parse(ReadParam("CONFIG_ENABLE_DEFAULT_LIGHTING", fileName));
            CONFIG_TERREIN_HEIGHT_PARAMETER = float.Parse(ReadParam("CONFIG_TERREIN_HEIGHT_PARAMETER", fileName));

            CONFIG_NUMBER_COVERS = int.Parse(ReadParam("CONFIG_NUMBER_COVERS", fileName));
            CONFIG_NUMBER_MEDS = int.Parse(ReadParam("CONFIG_NUMBER_MEDS", fileName));
            CONFIG_NUMBER_BONUSES = int.Parse(ReadParam("CONFIG_NUMBER_BONUSES", fileName));
            CONFIG_NUMBER_AIOBJECTS_IN_TEAM = int.Parse(ReadParam("CONFIG_NUMBER_AIOBJECTS_IN_TEAM", fileName));

            CONFIG_INCREASE_EXP_BONUS = float.Parse(ReadParam("CONFIG_INCREASE_EXP_BONUS", fileName));
            CONFIG_INCREASE_EXP_KILL = float.Parse(ReadParam("CONFIG_INCREASE_EXP_KILL", fileName));
            CONFIG_INCREASE_MED = float.Parse(ReadParam("CONFIG_INCREASE_MED", fileName));

            CONFIG_DECREASE_OF_LIFE_WHEN_HIT = float.Parse(ReadParam("CONFIG_DECREASE_OF_LIFE_WHEN_HIT", fileName));
            CONFIG_COVER_ADD_POINTS = float.Parse(ReadParam("CONFIG_COVER_ADD_POINTS", fileName));

            // restore current culture after importing
            Thread.CurrentThread.CurrentCulture = new CultureInfo(temp_culture);
        }

        #endregion
    }
}
