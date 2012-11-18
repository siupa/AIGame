using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace AIGame.AI
{
    #region Enums
    public enum AIAction
    {
        Attack,
        PUMed,
        PUBonus,
        TakeCover,
        Wander,
        Free
    }
    public enum AIRoleType
    {
        TeamLeader,
        Infantry,
        Support,
        Sniper
    }

    public enum AITeamType
    {
        Alpha,
        Bravo,
        Charlie,
        /// <summary>
        /// Can not be assigned, identificator of the empty, not assigned team
        /// </summary>
        NULL
    }

    public enum AIFormation
    {
        Row,
        Arrow,
        Spread
    }

    public enum AIObjective
    {
        SeekAndDestroy,
        Capture,
        Hold
    }

    public enum AIMovementStatus
    {
        Walk,
        Run,
        Crawl,
        Cover
    }
   
    #endregion

    public class AIContainer
    {
        #region Fields
        private int _framesCounterForEvolution = 0;
        public int FramesCounterForEvolution
        {
            get { return _framesCounterForEvolution; }
        }
        private int _framesCounterForFitness = 0;
        public int FramesCounterForFitness
        {
            get { return _framesCounterForFitness; }
        }
        private int _numberAIObjStillAlive = 0;

        public int NumberAIObjStillAlive
        {
            get { return _numberAIObjStillAlive; }
        }

        private bool _updateEvolution = false;
        public bool UpdateEvolution
        {
            get { return _updateEvolution; }
        }
        private bool _updateFitnessWithTime = false;

        private int _generation = 0;
        public int Generation
        {
            get { return _generation; }
        }
        private GA.GeneticAlgorithm _gaGenethicAlgorithm;

        internal GA.GeneticAlgorithm GenethicAlgorithm
        {
            get { return _gaGenethicAlgorithm; }
        }

        
        private List<GA.GenomePair> _gaPopulation;

        internal List<GA.GenomePair> GaPopulation
        {
            get { return _gaPopulation; }
        }

        private List<double> _fintessListMax = new List<double>();
        public List<double> FitnessListMax
        {
            get { return _fintessListMax; }
        }
        private List<double> _fintessListAvg = new List<double>();
        public List<double> FitnessListAvg
        {
            get { return _fintessListAvg; }
        }

		private AIObject _selectedObject;
        private AITeam _selectedTeam;
        private static List<AITeam> _teams = new List<AITeam>();
        private static List<AIObject> _aiobjects = new List<AIObject>();
        private static List<AIStatic> _aistatics = new List<AIStatic>();
        
	    #endregion

        #region Properties

        public bool IsOneOfAIObjectSelected
        {
            get { return _selectedObject != null && _selectedTeam != null; }
        }

        /// <summary>
        /// Gets selected AI object.
        /// </summary>
        public AIObject SelectedObject
        {
            get { return _selectedObject; }
        }

        /// <summary>
        /// Gets identifier of the selected team.
        /// </summary>
        public AITeam SelectedTeam
        {
            get { return _selectedTeam; }
        }

        /// <summary>
        /// Gets list of the teams.
        /// </summary>
        public List<AITeam> Teams
        {
            get { return _teams; }
        }

        /// <summary>
        /// Gets the global list of all the AI objects.
        /// </summary>
        public static List<AIObject> Objects
        {
            get { return _aiobjects; }
        }

        /// <summary>
        /// Gets the global list of the AI Static objects.
        /// </summary>
        public List<AIStatic> AIStatics
        {
            get { return AIContainer._aistatics; }
        }

        #endregion

        #region Public Methods
        public void CreateTeam(AITeamType type, Color color, int numInfantry, int numSniper, int numSupport, Vector3 position, bool areBots)
        {
            AITeam team = new AITeam(type, color);
            team.Add(new AIObject(AIRoleType.TeamLeader, position, areBots));
            for (int i = 0; i < numInfantry; i++)
                team.Add(new AIObject(AIRoleType.Infantry, position, areBots));
            for (int i = 0; i < numSniper; i++)
                team.Add(new AIObject(AIRoleType.Sniper, position, areBots));
            for (int i = 0; i < numSupport; i++)
                team.Add(new AIObject(AIRoleType.Support, position, areBots));
            _teams.Add(team);
        }
        public void SpreadObjects()
        {
            foreach (AIObject aiobj in _aiobjects)
                aiobj.Position = new Vector3((float)AIGame.random.NextDouble() * AIGame.terrain.TerrainHeightMap.RealSize, 0, (float)AIGame.random.NextDouble() * AIGame.terrain.TerrainHeightMap.RealSize);
        }
        public void AddAIStatic(string id, AIStaticType type)
        {
            _aistatics.Add(new AIStatic(id, type));
        }
        public void AddMulitpleAIStatics(int numCovers, int numMeds, int numBonuses)
        {
            for (int i = 0; i < numCovers; i++)
            {
                AddAIStatic("Cover " + (i + 1), AIStaticType.Cover);
            }
            for (int i = 0; i < numMeds; i++)
            {
                AddAIStatic("First Aid Kit " + (i + 1), AIStaticType.FirstAidKit);
            }
            for (int i = 0; i < numBonuses; i++)
            {
                AddAIStatic("Bonus " + (i + 1), AIStaticType.Bonus);
            }
        }
        
        public AIStatic GetNearestStatic(Vector3 position, AIStaticType type)
        {
            AIStatic nearest = null;
            float min_dist = float.MaxValue;
            foreach (AIStatic aistatic in _aistatics)
            {
                if (aistatic.Type == type)
                {
                    float real_dist = (aistatic.Position - position).Length();
                    if (real_dist < min_dist)
                    {
                        nearest = aistatic;
                        min_dist = real_dist;
                    }
                }
            }
            return nearest;
        }
        public AIStatic GetOtherNearestStatic(Vector3 position, AIStaticType type, List<AIStatic> not_this_ones)
        {
            AIStatic nearest = null;
            float min_dist = float.MaxValue;
            foreach (AIStatic aistatic in _aistatics)
            {
                if (not_this_ones.Contains(aistatic))
                    continue;
                if (aistatic.Type == type)
                {
                    float real_dist = (aistatic.Position - position).Length();
                    if (real_dist < min_dist)
                    {
                        nearest = aistatic;
                        min_dist = real_dist;
                    }
                }
            }
            return nearest;
        }
        public AIStatic GetActualCover(AIObject aiobj)
        {
            AIStatic actual = GetNearestStatic(aiobj.Position, AIStaticType.Cover);
            if (actual.IsObjectLocatedInside(aiobj))
                return actual;
            else
                return null;
        }

        public void ClearSelect()
        {
            foreach (AIObject aiobj in _aiobjects)
                aiobj.Selected = false;
            _selectedObject = null;
            _selectedTeam = null;
        }
        public void Select(AIObject item)
        {
            ClearSelect();
            _selectedObject = item;
            _selectedTeam = item.Team;
            item.Selected = true;
        }
        public void SelectTeam(AITeamType team)
        {
            ClearSelect();
            foreach (AITeam t in _teams)
            {
                if(t.TeamType == team)
                    _selectedTeam = t;
            }
            foreach (AIObject aiobj in _aiobjects)
            {
                if (aiobj.Team == _selectedTeam)
                {
                    aiobj.Selected = true;

                    if (aiobj.IsLeader)
                    {
                        _selectedObject = aiobj;
                        aiobj.Selected = true;
                    }
                }
            }
        }

        public void LoadContent(ContentManager content)
        {
            // set up ai objects
            foreach (AIObject aiobj in _aiobjects)
            {
                switch (aiobj.Team.TeamType)
                {
                    case AITeamType.Alpha:
                        if (aiobj.IsLeader)
                            aiobj.Initialize(content.Load<Model>("Models/actor_attack_leader"));
                        else
                        {
                            switch (aiobj.Role)
                            {
                                case AIRoleType.Infantry:
                                    aiobj.Initialize(content.Load<Model>("Models/actor_attack_inf"));
                                    break;
                                case AIRoleType.Sniper:
                                    aiobj.Initialize(content.Load<Model>("Models/actor_attack_sniper"));
                                    break;
                                case AIRoleType.Support:
                                    aiobj.Initialize(content.Load<Model>("Models/actor_attack_support"));
                                    break;
                                default:
                                    aiobj.Initialize(content.Load<Model>("Models/actor_attack_inf"));
                                    break;

                            }
                        }
                        break;
                    case AITeamType.Bravo:
                        if (aiobj.IsLeader)
                            aiobj.Initialize(content.Load<Model>("Models/actor_def_leader"));
                        else
                        {
                            switch (aiobj.Role)
                            {
                                case AIRoleType.Infantry:
                                    aiobj.Initialize(content.Load<Model>("Models/actor_def_inf"));
                                    break;
                                case AIRoleType.Sniper:
                                    aiobj.Initialize(content.Load<Model>("Models/actor_def_sniper"));
                                    break;
                                case AIRoleType.Support:
                                    aiobj.Initialize(content.Load<Model>("Models/actor_def_support"));
                                    break;
                                default:
                                    aiobj.Initialize(content.Load<Model>("Models/actor_def_inf"));
                                    break;
                            }
                        }
                        break;
                    default:
                        if (aiobj.IsLeader)
                        {
                            aiobj.Initialize(content.Load<Model>("Models/actor_civil_light"));
                        }
                        else
                        {
                            aiobj.Initialize(content.Load<Model>("Models/actor_civil"));
                        }
                        break;
                }
                aiobj.SetScale(0.2f);

                aiobj.SetUpProperties();
            }

            // set up ai statics
            foreach (AIStatic aistat in _aistatics)
            {
                
                float height = 110;
                //while (height > 80 && height <= 120)
                //{
                aistat.Position = new Vector3((float)AIGame.random.NextDouble() * (AIGame.terrain.TerrainHeightMap.RealSize - 200) + 100, 0, (float)AIGame.random.NextDouble() * (AIGame.terrain.TerrainHeightMap.RealSize - 200) + 100);
                height = AIGame.terrain.TerrainHeightMap.HeightAt(aistat.Position.X, aistat.Position.Z);
                //}

                if (aistat.Type == AIStaticType.Cover)
                {
                    bool success = false;
                    while (!success)
                    {
                        List<AIStatic> covs = _aistatics.FindAll(delegate(AIStatic ais) { return ais.Type == AIStaticType.Cover && !ais.Equals(aistat); });
                    StartLoop:
                        foreach (AIStatic cov in covs)
                        {
                            float dist = (aistat.Position - cov.Position).Length();
                            if (dist < 70.0f)
                            {
                                success = false;
                                aistat.Position = new Vector3((float)AIGame.random.NextDouble() * (AIGame.terrain.TerrainHeightMap.RealSize - 200) + 100, 0, (float)AIGame.random.NextDouble() * (AIGame.terrain.TerrainHeightMap.RealSize - 200) + 100);
                                height = AIGame.terrain.TerrainHeightMap.HeightAt(aistat.Position.X, aistat.Position.Z);
                                goto StartLoop;
                            }
                        }
                        success = true;
                    }
                }
                
                switch (aistat.Type)
                {
                    case AIStaticType.Cover:
                        // desert stuff
                        if (height < 80)
                        {
                            if (AIGame.random.NextDouble() > 0.7)
                            {
                                aistat.Initialize(content.Load<Model>("Models/static_cactus_small"));
                                if(AI.AIParams.CONFIG_LOW_RENDER)aistat.Initialize(content.Load<Model>("Models/static_cover"));
                                aistat.SetScale(0.7f);
                                aistat.FeetElevation = 0f;
                            }
                            else
                            {
                                aistat.Initialize(content.Load<Model>("Models/static_rock_small"));
                                if (AI.AIParams.CONFIG_LOW_RENDER) aistat.Initialize(content.Load<Model>("Models/static_cover"));
                                aistat.SetScale(0.7f);
                                aistat.FeetElevation = 0f;
                            }
                        }
                        //green stuff
                        else
                        {
                            if (AIGame.random.NextDouble() > 0.9)
                            {
                                aistat.Initialize(content.Load<Model>("Models/static_grass_tree"));
                                if (AI.AIParams.CONFIG_LOW_RENDER) aistat.Initialize(content.Load<Model>("Models/static_cover"));
                                aistat.SetScale(0.7f);
                                aistat.FeetElevation = -5f;
                            }
                            else
                            {
                                aistat.Initialize(content.Load<Model>("Models/static_grass_small"));
                                if (AI.AIParams.CONFIG_LOW_RENDER) aistat.Initialize(content.Load<Model>("Models/static_cover"));
                                aistat.SetScale(0.7f);
                                aistat.FeetElevation = 0f;
                            }
                        }
                        break;
                    case AIStaticType.FirstAidKit:
                        aistat.Initialize(content.Load<Model>("Models/static_med"));
                        aistat.SetScale(0.2f);
                        aistat.FeetElevation = 20f;
                        break;
                    case AIStaticType.Bonus:
                        aistat.Initialize(content.Load<Model>("Models/static_bonus"));
                        aistat.SetScale(0.2f);
                        aistat.FeetElevation = 20f;
                        break;
                    default:
                        break;
                }
                aistat.EndDirection = (float)(AIGame.random.NextDouble() * Math.PI * 2.0);
            }
        }

        public void InitializeAI()
        {
            _gaPopulation = new List<global::AIGame.AI.GA.GenomePair>();
            _gaGenethicAlgorithm = new global::AIGame.AI.GA.GeneticAlgorithm(
                    _aiobjects.Count,
                    AIParams.MutationRate,
                    AIParams.CrossoverRate,
                    _aiobjects[0].AINeuralBrain.GetNumberOfWeights(),
                    _aiobjects[0].AINeuralBrain.CalculateSplitPoints()
                );

            #region read from dump file
            try
            {
                if (AIParams.CONFIG_GET_GENERATION_FROM_FILE)
                {
                    if (!File.Exists(AIParams.CONFIG_GET_GENERATION_FROM_FILE_PATH))
                        throw new Exception("Attempt to read start generation from file failed! File does not exist!");

                    //// set the temporary culture for importing
                    //string temp_culture = Thread.CurrentThread.CurrentCulture.Name;
                    //Thread.CurrentThread.CurrentCulture
                    //    = new CultureInfo("en-US");

                    _gaGenethicAlgorithm.Population.Clear();

                    TextReader tr = new StreamReader(AIParams.CONFIG_GET_GENERATION_FROM_FILE_PATH);
                    List<double> weights = new List<double>();
                    string line = tr.ReadLine();
                    double fitness = 0;
                    while (line != null)
                    {
                        if (line.Contains("Generation:"))
                        {
                            string[] temp_gen = line.Split(' ');
                            _generation = int.Parse(temp_gen[1]);
                            _gaGenethicAlgorithm.BestFitness = double.Parse(temp_gen[3]);
                            _gaGenethicAlgorithm.AverageFitness = double.Parse(temp_gen[5]);
                        }
                        if (line.Contains("Fitness:"))
                        {
                            fitness = double.Parse(line.Split(' ')[1]);
                        }
                        if (line.Contains("Weights:"))
                        {
                            weights.Clear();
                            line = tr.ReadLine().TrimStart().TrimEnd();
                            string[] s_w = line.Split(' ');
                            foreach (string s in s_w)
                                weights.Add(double.Parse(s));

                            double[] temp = weights.ToArray();
                            _gaGenethicAlgorithm.Population.Add(new GA.GenomePair("",new GA.Genome(new List<double>(temp), fitness)));
                        }
                        line = tr.ReadLine();
                    }

                    if (AIParams.CONFIG_GET_GENERATION_FROM_FILE_ONLY_ELITE)
                    {
                        _gaGenethicAlgorithm.Population.Sort(GA.GenomePair.GenomePairComparison);
                        _gaGenethicAlgorithm.Population.Reverse();
                        //GA.GenomePair[] temp = new global::AIGame.AI.GA.Genome[_gaGenethicAlgorithm.Population.Count];
                        //_gaGenethicAlgorithm.Population.CopyTo(temp);
                        int e = 0;
                        for (int i = 0; i < _gaGenethicAlgorithm.Population.Count; i++)
                        {
                            if (i < AIParams.NumElite * AIParams.NumCopiesElite)
                                continue;
                            _gaGenethicAlgorithm.Population[i] = _gaGenethicAlgorithm.Population[e];
                            e++;
                        }
                    }


                    //// restore current culture after importing
                    //Thread.CurrentThread.CurrentCulture = new CultureInfo(temp_culture);
                }
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error reading first generation from file! Exception: " + ex.Message);
            }
            #endregion

            int j = 0;
            foreach (AIObject aiobj in _aiobjects)
            {
                _gaGenethicAlgorithm.Population[j].Genome.Fitness = 0.0;
                aiobj.AINeuralBrain.PutWeights(_gaGenethicAlgorithm.Population[j].Genome.Weights);
                _gaPopulation.Add(_gaGenethicAlgorithm.Population[j]);
                j++;
            }
        }

        public void DrawAll(GameTime gameTime, Matrix proj, Matrix view)
        {
            foreach (AIStatic aistat in _aistatics)
                aistat.Draw(gameTime, proj, view);

            foreach (AIObject aiobj in _aiobjects)
                aiobj.Draw(gameTime, proj, view);
        }

        private GA.GenomePair GetGenomePairById(List<GA.GenomePair> list, string id)
        {
            foreach (GA.GenomePair genpair in list)
            {
                if (genpair.Id.Equals(id))
                    return genpair;
            }
            return null;
        }

        public void UpdateAll(GameTime gameTime)
        {
            #region Remove dead units or gone statics
            //// remove dead units
            //List<AIObject> to_remove = new List<AIObject>();
            //foreach (AIObject aiobj in _aiobjects)
            //    if (aiobj.IsDead)
            //        to_remove.Add(aiobj);
            //foreach (AIObject aiobj_to_remove in to_remove)
            //    _aiobjects.Remove(aiobj_to_remove);

            //// remove gone statics
            //List<AIStatic> to_remove_st = new List<AIStatic>();
            //foreach (AIStatic aistat in _aistatics)
            //    if (aistat.IsGone)
            //        to_remove_st.Add(aistat);
            //foreach (AIStatic aistat_to_remove in to_remove_st)
            //    _aistatics.Remove(aistat_to_remove); 
            #endregion

            List<GA.GenomePair> temp_population = null;
            if (_updateEvolution && AIParams.UPDATE_EVOLUTION_ENABLED)
            {
                //System.Threading.Thread.Sleep(60000);
                // proceed with the epoch
                _generation++;

                //GA.Genome[] genoms_array = new GA.Genome[_gaPopulation.Values.Count];
                //_gaPopulation.Values.CopyTo(genoms_array, 0);
                //List<GA.Genome> genoms = new List<global::AIGame.AI.GA.Genome>(genoms_array);

                temp_population = _gaGenethicAlgorithm.Epoch(_gaPopulation);

                CultureInfo temp_culture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", true);

                _fintessListMax.Add(_gaGenethicAlgorithm.BestFitness);
                _fintessListAvg.Add(_gaGenethicAlgorithm.AverageFitness);

                // dump the populatiuon into the txt file
                if (AIParams.CONFIG_DUMP_ALL_POPULATIONS)
                {
                    TextWriter tw = new StreamWriter(AIParams.CONFIG_DUMP_ALL_POPULATIONS_FILE_PATH, true);
                    tw.WriteLine("\nGeneration: " + Generation.ToString() + " BestFit: " + _gaGenethicAlgorithm.BestFitness + " AvgFit: " + _gaGenethicAlgorithm.AverageFitness);
                    foreach (GA.GenomePair genpair in _gaPopulation)
                    {
                        tw.WriteLine("\nFitness: " + genpair.Genome.Fitness);
                        tw.WriteLine("Weights: ");
                        foreach (double weight in genpair.Genome.Weights)
                            tw.Write(" " + weight);
                    }
                    tw.Close();
                    tw.Dispose();
                }
                if (AIParams.CONFIG_DUMP_LAST_POPULATION)
                {
                    // dump the last generation into the txt file
                    TextWriter tw = new StreamWriter(AIParams.CONFIG_DUMP_LAST_POPULATION_FILE_PATH, false);
                    tw.WriteLine("\nGeneration: " + Generation.ToString() + " BestFit: " + _gaGenethicAlgorithm.BestFitness + " AvgFit: " + _gaGenethicAlgorithm.AverageFitness);
                    foreach (GA.GenomePair genpair in _gaPopulation)
                    {
                        tw.WriteLine("\nFitness: " + genpair.Genome.Fitness);
                        tw.WriteLine("Weights: ");
                        foreach (double weight in genpair.Genome.Weights)
                            tw.Write(" " + weight);
                    }
                    tw.Close();
                    tw.Dispose();
                }
                Thread.CurrentThread.CurrentCulture = temp_culture;

                _gaPopulation.Clear();
                //Select(_gaGenethicAlgorithm.FittestObject);
            }

            // update all alive
            double max = double.MinValue;
            int i = 0;
            foreach (AIObject aiobj in _aiobjects)
            {
                if (_updateEvolution && AIParams.UPDATE_EVOLUTION_ENABLED)
                {
                    _gaPopulation.Add(temp_population[i]);
                    aiobj.Identifier = temp_population[i].Id;
                    aiobj.AINeuralBrain.PutWeights(temp_population[i].Genome.Weights);
                    aiobj.Reset();
                    i++;
                }
                else
                {
                    aiobj.Update(gameTime);

                    if (!aiobj.IsDead)
                    {
                        aiobj.AIFitnessLife = _framesCounterForEvolution - aiobj.AIFitnessPenaltyCounter;
                        aiobj.AIFitnessExp = aiobj.Experience * AIParams.FITNESS_EXPERIENCE_MULTIPLICATION_FACTOR;
                        aiobj.AIFitness = aiobj.AIFitnessLife + aiobj.AIFitnessExp + aiobj.AIFitnessCustom;
                    }

                    if (aiobj.AIFitness > max)
                    {
                        max = aiobj.AIFitness;
                        _gaGenethicAlgorithm.FittestObject = aiobj;
                    }

                    if (_updateFitnessWithTime)
                    {
                        if (!aiobj.IsDead)
                        {
                            aiobj.UpdateFitness(AIParams.FITNESS_INCREASE_WITH_TIME);
                            _numberAIObjStillAlive++;
                        }
                        //aiobj.IsDead = false;
                    }
                }
            }
            if (_updateEvolution && AIParams.UPDATE_EVOLUTION_ENABLED)
            {
                _updateEvolution = false;
                AIParams.FITNESS_INCREASE_WITH_TIME_ENABLE = true;
            }
            if (_updateFitnessWithTime)
                _updateFitnessWithTime = false;
            
            // frame timers
            if (AIParams.UPDATE_EVOLUTION_ENABLED)
            {
                _framesCounterForEvolution++;
                if (_framesCounterForEvolution > AIParams.UPDATE_EVOLUTION_TIME_PERIOD_FRAMES)
                {
                    _updateEvolution = true;
                    _framesCounterForEvolution = 0;

                    // update population list
                    int g = 0;
                    foreach (AIObject aio in _aiobjects)
                    {
                        _gaPopulation[g].Id = aio.Identifier;
                        _gaPopulation[g].Genome = new GA.Genome(aio.AINeuralBrain.GetWeights(), aio.AIFitness);
                        g++;
                    }
                }
            }

            if (AIParams.FITNESS_INCREASE_WITH_TIME_ENABLE && AIParams.UPDATE_EVOLUTION_ENABLED)
            {
                _framesCounterForFitness++;
                if (_framesCounterForFitness > AIParams.FITNESS_INCREASE_WITH_TIME_PERIOD_FRAMES)
                {
                    _updateFitnessWithTime = true;
                    AIParams.FITNESS_INCREASE_WITH_TIME_ENABLE = false;
                    _framesCounterForFitness = 0;
                    _numberAIObjStillAlive = 0;
                }
            }

            // update all aistatics, if there is anything to update
            foreach (AIStatic aistat in _aistatics)
                aistat.Update(gameTime);

            UpdateInput();
        }

        private void UpdateInput()
        {
            MouseState mouseState = Mouse.GetState();
            
            // SELECT
            if (AIGame.LeftButtonJustPressed())
            {
                bool was_selected = false;
                foreach (AIObject aiobj in _aiobjects)
                {
                    Ray pickRay = MathExtra.CalculateCursorRay(AIGame.camera.ProjectionMatrix, AIGame.camera.ViewMatrix);
                    float? rayLength
                        = aiobj.BoundingBox.Intersects(pickRay);

                    if (rayLength != null)
                    {
                        if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                        {
                            SelectTeam(aiobj.Team.TeamType);
                            aiobj.Team.OperateAsAUnit = true;
                        }
                        else
                            Select(aiobj);
                        was_selected = true;
                    }
                }
                if (!was_selected)
                    ClearSelect();
            }
            
            // MOVE OR ATTACK
            if (AIGame.RightButtonJustPressed() && AIGame.camera.CurrentBehavior == Camera.Behavior.FreeView)
            {
                if (_selectedObject == null)
                    return;

                AIObject temp_aiobj = null;
                float? rayLength;

                foreach (AIObject aiobj in _aiobjects)
                {
                    Ray pickRay = MathExtra.CalculateCursorRay(AIGame.camera.ProjectionMatrix, AIGame.camera.ViewMatrix);
                    rayLength = aiobj.BoundingBox.Intersects(pickRay);
                    if(rayLength != null)
                        temp_aiobj = aiobj;
                }

                _selectedObject.CancelPreviousActions();
                // only move
                if (temp_aiobj == null)
                {
                    _selectedObject.MoveTo(AIGame.terrain.groundCursorPosition * Settings.TERRAIN_TEXTURE_SIZE);
                    if (_selectedObject.IsLeader &&
                        _selectedTeam.KeepFormation && 
                        _selectedTeam.Formation != AIFormation.Spread)
                    {
                        _selectedTeam.ToFormation(_selectedTeam.Formation);
                    }

                    if (_selectedTeam.KeepFormation &&
                        _selectedObject == _selectedTeam.Leader)
                    {
                        if (_selectedTeam.Leader.HasJustStopped)
                            _selectedTeam.ToFormation(_selectedTeam.Formation);
                    }
                    _selectedObject.IsFollowing = false;
                    _selectedObject.Target = null;
                }
                // attack
                else
                {
                    if (_selectedObject.Team != temp_aiobj.Team)
                        _selectedObject.Attack(temp_aiobj);
                    else
                        _selectedObject.Follow(temp_aiobj);
                }
            }
        }

	    #endregion

        #region Private Methods
        public static string GetUniqueIdentifier()
        {
            if (_aiobjects.Count == 0)
                return "AIObject 1";

            string ret = "";
            int index = 1;
            bool found_unique = false;
            while (!found_unique)
            {
                ret = "AIObject " + index.ToString();
                index++;
                foreach (AIObject aiobj in _aiobjects)
                {
                    if (aiobj.Identifier == ret)
                    {
                        found_unique = false;
                        break;
                    }
                    found_unique = true;
                }
            }
            return ret;
        }

        #endregion
    }



}
