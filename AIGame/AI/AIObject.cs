using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIGame.AI
{
    public class AIObject : ModelHandler
    {
        #region AI
        private bool _isBot = true;
        /// <summary>
        /// Indicates if AIObject is controled by ANN or by player.
        /// </summary>
        public bool IsBot
        {
            get { return _isBot; }
            set { _isBot = value; }
        }

        private ANN.NeuralNet _aiNeuralBrain
            = new global::AIGame.AI.ANN.NeuralNet(
                AIParams.NumInputs,
                AIParams.NumOutputs,
                AIParams.NumHidden,
                AIParams.NeuronsPerHiddenLayer
                );
        internal ANN.NeuralNet AINeuralBrain
        {
            get { return _aiNeuralBrain; }
        }
        private double _aiFitness = 0;
        private double _aiFitnessCustom = 0;

        public double AIFitnessCustom
        {
            get { return _aiFitnessCustom; }
        }
        private double _aiFitnessLife = 0;
        public double AIFitnessLife
        {
            get { return _aiFitnessLife; }
            set { _aiFitnessLife = value; }
        }
        private double _aiFitnessExp = 0;
        public double AIFitnessExp
        {
            get { return _aiFitnessExp; }
            set { _aiFitnessExp = value; }
        }
        private List<double> _aiFitnessesListLife = new List<double>();
        public List<double> AIFitnessesListLife
        {
            get { return _aiFitnessesListLife; }
        }
        private List<double> _aiFitnessesListExp = new List<double>();
        public List<double> AIFitnessesListExp
        {
            get { return _aiFitnessesListExp; }
        }
        private List<double> _aiFitnessesListPenalty = new List<double>();
        public List<double> AIFitnessesListPenalty
        {
            get { return _aiFitnessesListPenalty; }
        }

        public double AIFitness
        {
            get { return _aiFitness; }
            set { _aiFitness = value; }
        }

        private double _aiFitnessPenaltyCounter = 0;
        public double AIFitnessPenaltyCounter
        {
            get { return _aiFitnessPenaltyCounter; }
        }

        private Dictionary<AIAction, int> _actionCounter = new Dictionary<AIAction, int>();
        public Dictionary<AIAction, int> ActionCounter
        {
            get { return _actionCounter; }
        }

        private bool _isTheFittest = false;
        public bool IsTheFittest
        {
            get { return _isTheFittest; }
            set { _isTheFittest = value; }
        }
        #endregion

        #region Fields
        private string _identifier;
        private AITeam _team;
        private AIRoleType _role;
        /// <summary>
        /// Do not use to assign status!!!
        /// </summary>
        private AIMovementStatus _status;
        private bool _isLeader = false;
        private bool _isDead = false;
        private List<SeenAIObject> _sightList = new List<SeenAIObject>();
        public struct SeenAIObject
        {
            public float distance;
            public AIObject aiobj;
            public SeenAIObject(float dist, AIObject obj)
            {
                distance = dist;
                aiobj = obj;
            }
        }
        private float _sightRange = 500.0f;
        private bool _isShootAt = false;
        
        private bool _inCover = false;
        private AIStatic _cover;
        private List<AIStatic> _coversBanned = new List<AIStatic>();

        private AIStatic _pickUpStatic = null;
        private Vector3 _fireDirection;
        private AIObject _target = null;
        private float _targetDistance = 0;
        private bool _isFollowing = false;
        private bool _holdFire = false;
        private bool _isShooting = false;

        private bool _isOccupied = false;
        public bool IsOccupied
        {
            get { return _isOccupied; }
        }
        private AIAction _actualAction;
        public AIAction ActualAction
        {
            get { return _actualAction; }
        }
        private void OccupyBy(AIAction action)
        {
            _isOccupied = true;
            _actualAction = action;
        }
        private void SetFree()
        {
            _isOccupied = false;
            _actualAction = AIAction.Free;
            Status = AIMovementStatus.Walk;
            //CancelPreviousActions();
        }

        private float _life = 1.0f;
        private float _experience = 0.0f;

        private int _delay = 0;
        #endregion

        #region Properties
        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }
        public AITeam Team
        {
            get { return _team; }
            set { _team = value; }
        }
        public AIRoleType Role
        {
            get { return _role; }
            //set { _role = value; }
        }
        public bool IsLeader
        {
            get { return _isLeader; }
        }
        public AIMovementStatus Status
        {
            get { return _status; }
            set 
            { 
                switch(value)
                {
                    case AIMovementStatus.Walk:
                        MovementSpeed = Settings.GameSpeed(Settings.OBJECTS_MOVEMENT_SPEED * 0.5f);
                        break;
                    case AIMovementStatus.Run:
                        MovementSpeed = Settings.GameSpeed(Settings.OBJECTS_MOVEMENT_SPEED);
                        break;
                    case AIMovementStatus.Crawl:
                        MovementSpeed = Settings.GameSpeed(Settings.OBJECTS_MOVEMENT_SPEED * 0.25f);
                        break;
                    case AIMovementStatus.Cover:
                        MovementSpeed = Settings.GameSpeed(Settings.OBJECTS_MOVEMENT_SPEED * 0.4f);
                        break;
                }
                _status = value;

                if (_isLeader && _team != null && _team.OperateAsAUnit)
                    foreach (AIObject aiobj in _team.Values)
                    {
                        if (aiobj != this)
                            aiobj.Status = _status;
                    }
            }
        }
        public AIObject Target
        {
            get { return _target; }
            set 
            { 
                _target = value;

                if(value != null)
                    _targetDistance = (_target.Position - Position).Length();
                if (value != null && !IsMoving)
                    TurnToTarget(_target.Position);
            }
        }
        public bool IsFollowing
        {
            get { return _isFollowing; }
            set { _isFollowing = value; }
        }
        public bool HoldFire
        {
            get { return _holdFire; }
            set { _holdFire = value; }
        }
        public bool IsShooting
        {
            get { return _isShooting; }
        }
        public bool InCover
        {
            get { return _inCover; }
        }
        public float Life
        {
            get { return _life; }
            set { _life = value; }
        }
        public float Experience
        {
            get { return _experience; }
            set { _experience = value; }
        }
        /// <summary>
        /// Gets the list of all the aiobjects that are seen.
        /// </summary>
        public List<SeenAIObject> SightList
        {
            get { return _sightList; }
        }
        /// <summary>
        /// Gets the sight range for the ai object.
        /// </summary>
        public float SightRange
        {
            get { return _sightRange; }
            set { _sightRange = value; }
        }
        public bool IsDead
        {
            get { return _isDead; }
            set { _isDead = value; }
        }
        #endregion

        #region Constructors and Setup

        public AIObject(AIRoleType r, Vector3 p, bool isBot)
            : base()
        {
            _team = null;
            _isBot = isBot;
            if (r == AIRoleType.TeamLeader)
            {
                //r = AIRoleType.Infantry;
                _isLeader = true;
            }
            _role = r;
            Status = AIMovementStatus.Walk;
            Position = p;
        }

        public void SetUpProperties()
        {
            switch (this._role)
            {
                case AIRoleType.Infantry:
                    _sightRange = AIParams.OBJECT_RANGE_INFANTRY;
                    break;
                case AIRoleType.Sniper:
                    _sightRange = AIParams.OBJECT_RANGE_SNIPER;
                    break;
                case AIRoleType.Support:
                    _sightRange = AIParams.OBJECT_RANGE_SUPPORT;
                    break;
                case AIRoleType.TeamLeader:
                    _sightRange = AIParams.OBJECT_RANGE_LEADER;
                    break;
                default:
                    break;
            }
            _actionCounter.Clear();
            _actionCounter.Add(AIAction.Attack, 0);
            _actionCounter.Add(AIAction.PUBonus, 0);
            _actionCounter.Add(AIAction.PUMed, 0);
            _actionCounter.Add(AIAction.TakeCover, 0);
            _actionCounter.Add(AIAction.Wander, 0);
        }

        #endregion

        #region Action Methods
        public void Reset()
        {
            _isDead = false;
            _life = 1;
            _experience = 0;

            _aiFitness = 0;
            _aiFitnessCustom = 0;
            _aiFitnessLife = 0;
            _aiFitnessExp = 0;
            _aiFitnessesListExp.Clear();
            _aiFitnessesListLife.Clear();

            SetUpProperties();
            Position = new Vector3(
                    (float)AIGame.random.NextDouble() * AIGame.terrain.TerrainHeightMap.RealSize,
                    0,
                    (float)AIGame.random.NextDouble() * AIGame.terrain.TerrainHeightMap.RealSize
                    );
            SetFree();
            CeaseFire();
        }
        public bool Sees(AIObject seen_aiobj)
        {
            foreach (SeenAIObject obj in _sightList)
            {
                if (obj.aiobj == seen_aiobj)
                    return true;
            }
            return false;
        }
        public void Follow(AIObject aiobj_to_follow)
        {
            _target = aiobj_to_follow;
            _isFollowing = true;
        }
        public void Attack(AIObject attack_aiobj)
        {
            this.Status = AIMovementStatus.Run;
            _target = attack_aiobj;
            _holdFire = false;
            Follow(attack_aiobj);
            Shoot(attack_aiobj);
            _actualAction = AIAction.Attack;
            OccupyBy(AIAction.Attack);
        }
        private void Shoot(AIObject attack_aiobj)
        {
            if (_target != null)
            {
                _isShooting = true;
            }
        }
        private void RunFromFire()
        {
            if (_fireDirection != null)
            {
                this.MoveTo(Position + new Vector3(_sightRange - _sightRange * 0.1f) * -_fireDirection);
            }
        }
        public void TakeCover()
        {
            AIStatic cover = AIGame.AIContainer.GetNearestStatic(this.Position, AIStaticType.Cover);
            if (cover == _cover)
                return;

            if (cover.IsAllowedToEnter(this))
            {
                this.MoveTo(cover.Position);
                _cover = cover;
                if (_isShootAt)
                    UpdateFitness(AIParams.FITNESS_INCREASE_GET_TO_COVER);
            }
            else
            {
                _coversBanned.Add(cover);
                TakeCover(AIGame.AIContainer.GetOtherNearestStatic(this.Position, AIStaticType.Cover, _coversBanned));
            }
            OccupyBy(AIAction.TakeCover);
        }
        public void TakeCover(AIStatic cover)
        {
            if (cover == null)
            {
                SetFree();
                _coversBanned.Clear();
                return;
            }

            if (cover.IsAllowedToEnter(this))
            {
                this.MoveTo(cover.Position);
                _cover = cover;
            }
            else
            {
                _coversBanned.Add(cover);
                TakeCover(AIGame.AIContainer.GetOtherNearestStatic(this.Position, AIStaticType.Cover, _coversBanned));
            }
            OccupyBy(AIAction.TakeCover);
        }
        public void PickUpMedKit()
        {
            if (this._life == 1)
            {
                UpdateFitness(AIParams.FITNESS_DECREASE_MED);
                SetFree();
                return;
            }

            AIStatic medKit = AIGame.AIContainer.GetNearestStatic(this.Position, AIStaticType.FirstAidKit);
            _pickUpStatic = medKit;
            this.MoveTo(medKit.Position);
            medKit.AIObjects.Add(this);
            OccupyBy(AIAction.PUMed);
            
        }
        public void PickUpBonus()
        {
            if (_experience == 100)
            {
                UpdateFitness(AIParams.FITNESS_DECREASE_BONUS);
                SetFree();
                return;
            }

            AIStatic bonus = AIGame.AIContainer.GetNearestStatic(this.Position, AIStaticType.Bonus);
            _pickUpStatic = bonus;
            this.MoveTo(bonus.Position);
            bonus.AIObjects.Add(this);

            OccupyBy(AIAction.PUBonus);
          
        }
        private void WanderTo(Vector3 target)
        {
            MoveTo(target);
            OccupyBy(AIAction.Wander);
        }
        private void Wander()
        {
            if (!_isShootAt)
                MoveTo(RandomPosition());
            else
                RunFromFire();

            OccupyBy(AIAction.Wander);
        }
        private void CeaseFire()
        {
            _isShooting = false;
            _target = null;
            _sightList.Clear();
            _isFollowing = false;
        }
        private bool IsInRange(AIObject aiobj)
        {
            // check if the object is on a proper side of the view matrix
            Vector3 v1 = ViewDirection;
            Vector3 v2 = Target.Position - Position;
            v1.Normalize();
            v2.Normalize();
            if (Vector3.Dot(v1, v2) < 0)
                return false;

            return 
                (aiobj.Position - Position).Length() <= _sightRange &&
                Sees(aiobj);
        }
        private void BoostOrDecreaseExperience(float value)
        {
            this._experience += value;
            if (_experience < 0)
                _experience = 0;
            else if (_experience > 100)
                _experience = 100;
        }
        private void HealOrHarm(float value)
        {
            this._life += value;
            if (_life > 1)
                _life = 1;
        }
        /// <summary>
        /// Cancels all the posible actions undertaken before. 
        /// Needs to be updated everytime actions logic is changed or new action added.
        /// </summary>
        public void CancelPreviousActions()
        {
            _pickUpStatic = null;
            SetFree();
        }
        public void ToggleMoveStatus()
        {
            if (_status == AIMovementStatus.Walk)
                Status = AIMovementStatus.Run;
            else if (_status == AIMovementStatus.Run)
                Status = AIMovementStatus.Crawl;
            else if (_status == AIMovementStatus.Crawl)
                Status = AIMovementStatus.Cover;
            else if (_status == AIMovementStatus.Cover)
                Status = AIMovementStatus.Walk;
        }

        private AIStatic GetNearestAvailableStatic(AIStaticType type)
        {
            AIStatic nearest = null;
            float min_dist = float.MaxValue;
            foreach (AIStatic aistatic in AIGame.AIContainer.AIStatics.FindAll(delegate(AIStatic ais) { return ais.Type == type;}))
            {
                float real_dist = (aistatic.Position - Position).Length();
                if (real_dist < min_dist)
                {
                    nearest = aistatic;
                    min_dist = real_dist;
                }
            }
            return nearest;
        }
        #endregion

        #region Update and Draw Methods
        private void UpdateSightList()
        {
            _sightList.Clear();

            // add new objects that can be seen
            foreach (AIObject o_aiobj in AIContainer.Objects)
            {
                // if its itself - continue with others
                if (o_aiobj == this)
                    continue;

                // is distance ok?
                float dist = (o_aiobj.Position - this.Position).Length();
                if (dist > _sightRange)
                    continue;

                bool is_blocked = false;
                // check obstacles and terrain
                if (this.Team != o_aiobj.Team)
                {
                    // method less precise but faster
                    Vector3 point_in_between = (Position + o_aiobj.Position) / 2;
                    float diff = AIGame.terrain.TerrainHeightMap.HeightAt(point_in_between.X, point_in_between.Z) - point_in_between.Y - 5;
                    if (diff > 0) is_blocked = true;
                    Vector3 point_in_quarter = (Position + point_in_between) / 2;
                    float diff_1q = AIGame.terrain.TerrainHeightMap.HeightAt(point_in_quarter.X, point_in_quarter.Z) - point_in_quarter.Y - 5;
                    if (diff_1q > 0) is_blocked = true;
                    Vector3 point_in_three_quarters = (point_in_between + o_aiobj.Position) / 2;
                    float diff_3q = AIGame.terrain.TerrainHeightMap.HeightAt(point_in_three_quarters.X, point_in_three_quarters.Z) - point_in_three_quarters.Y - 5;
                    if (diff_3q > 0) is_blocked = true;

                    #region method extremely slow but precise, checks the intersection with the height map and objects bounding boxes
                    //Vector3 dir = this.Position - o_aiobj.Position;
                    //dir.Normalize();
                    //Ray ray = new Ray(this.Position, dir);
                    //Vector3? intersection_point = AIGame.terrain.IsIntersected(ray);
                    //if (intersection_point != null)
                    //{
                    //    if ((this.Position - o_aiobj.Position).Length() > (this.Position - intersection_point.Value).Length())
                    //    {
                    //        is_blocked = true;
                    //    }
                    //} 
                    #endregion
                }

                // object can be seen, so we add it to the sight list
                if (dist <= _sightRange && !is_blocked)
                {
                    _sightList.Add(new SeenAIObject(dist, o_aiobj));

                    //if (o_aiobj.Sees(this))
                    //    this._isShootAt = true;
                    //else
                    //    this._isShootAt = false;
                }
            }
        }
        private void UpdateTakeCover()
        {
            if (_cover != null)
            {
                if (_cover.IsObjectLocatedInside(this) && !_inCover)
                {
                    if (!_cover.IsAllowedToEnter(this))
                    {
                        _coversBanned.Add(_cover);
                        _cover = AIGame.AIContainer.GetOtherNearestStatic(this.Position, AIStaticType.Cover, _coversBanned);
                        TakeCover(_cover);
                        return;
                    }

                    _inCover = true;
                    _coversBanned.Clear();
                    if (!_cover.AIObjects.Contains(this))
                    {
                        _cover.AIObjects.CopyTo(_cover.AIObjects_Prev);
                        _cover.AIObjects.Add(this);
                        _cover.TeamInside = this.Team.TeamType;
                        SetFree();
                    }
                }
                
                if(!_cover.IsObjectLocatedInside(this) && _inCover)
                {
                    _inCover = false;
                    if (_cover.AIObjects.Contains(this))
                    {
                        _cover.AIObjects.CopyTo(_cover.AIObjects_Prev);
                        _cover.AIObjects.Remove(this);
                        if (_cover.AIObjects.Count == 0)
                            _cover.TeamInside = AITeamType.NULL;
                        _cover = null;
                    }
                }
            }
        }
        private void UpdatePickUpStatic()
        {
            if (_pickUpStatic != null)
            {
                if ((_pickUpStatic.Position - this.Position).Length() <= 30f)
                {
                    switch (_pickUpStatic.Type)
                    {
                        case AIStaticType.Bonus:
                            if (_life > 0.5)
                                BoostOrDecreaseExperience(AIParams.CONFIG_INCREASE_EXP_BONUS);

                            if (_life > 0.5)
                                UpdateFitness(AIParams.FITNESS_INCREASE_BONUS);
                            break;
                        case AIStaticType.FirstAidKit:
                            HealOrHarm(AIParams.CONFIG_INCREASE_MED);
                            if (_life < 0.5)
                                UpdateFitness(AIParams.FITNESS_INCREASE_MED);
                            break;
                        default:
                            break;
                    }
                    _pickUpStatic.Position = new Vector3((float)AIGame.random.NextDouble() * AIGame.terrain.TerrainHeightMap.RealSize, 0, (float)AIGame.random.NextDouble() * AIGame.terrain.TerrainHeightMap.RealSize);
                    //_pickUpStatic.IsGone = true;

                    // free rest of the objects picking up this static
                    foreach (AIObject aiobj in _pickUpStatic.AIObjects)
                    {
                        if (!aiobj.Equals(this))
                        {
                            aiobj._pickUpStatic = null;
                            aiobj.SetFree();
                        }
                    }

                    if(_pickUpStatic.AIObjects.Count > 0)
                        _pickUpStatic.AIObjects.Clear();

                    _pickUpStatic = null;
                    SetFree();
                }
            }
        }
        private void UpdateWander()
        {
            if (_actualAction == AIAction.Wander &&
                EndPosition.X == Position.X &&
                EndPosition.Z == Position.Z)
                SetFree();
        }
        private void UpdateAction()
        {
            // if has a target already set
            if (_isFollowing)
            {
                if (_target == null || _target._isDead)
                {
                    CeaseFire();
                    if(_actualAction == AIAction.Attack)
                        SetFree();
                    return;
                }
                Vector3 dir = (Position - _target.Position);
                dir.Normalize();
                if ((_target.Position - Position).Length() > _sightRange - _sightRange * 0.1f)
                    MoveTo(_target.Position + new Vector3(_sightRange - _sightRange * 0.2f) * dir);
                this.Target = _target;
            }
            // if is not following particular target than can attack all the nearest targets
            else if (!_isFollowing)
            {
                // sort list of seen aiobjects from the closest one to the furthest one.
                _sightList.Sort(new Comparison<SeenAIObject>(
                    delegate(SeenAIObject o1, SeenAIObject o2)
                    {
                        if (o1.distance > o2.distance) return 1;
                        else if (o1.distance == o2.distance) return 0;
                        else return -1;
                    }));

                // go through the list and see what we can do.
                foreach (SeenAIObject seen_aiobj in _sightList)
                {
                    if (seen_aiobj.aiobj.Team != this.Team)
                    {
                        // if is not following a certain target, update _target to the nearest target
                        this.Target = seen_aiobj.aiobj;
                        break;
                    }
                    // there are not targets in a sight range
                    this.Target = null;
                }
            }

            UpdatePickUpStatic();
            UpdateTakeCover();
            //UpdateWander();

            // shoot or not
            if (_target != null && _target.Team != this.Team && IsInRange(_target) && !_holdFire)
            {
                _isShooting = true;

                // make target be aware that he is being shoot at
                _target._isShootAt = true;
                Vector3 f_dir = (_target.Position - Position);
                f_dir.Normalize();
                _target._fireDirection = f_dir;

                if(AIParams.UPDATE_LIFE_POINTS)
                    UpdateTargetLife();
            }
            else
            {
                _isShooting = false;
                if(_target != null)
                    _target._isShootAt = false;
            }
        }
        private void UpdateTargetLife()
        {
            if (_target == null || !_isShooting)
                return;

            float r = (float)AIGame.random.NextDouble();
            int num_of_params = 3;
            float range = 1 - ((_sightRange - (_target.Position - Position).Length()) / _sightRange);
            
            float height = this.Position.Y - _target.Position.Y;
            if (height < 0) height = 25f;
            else height = 75f;
            if (Math.Abs(height) < 200)
            {
                height = 0;
                num_of_params = 2;
            }

            float paramaters = (range + _experience / 100f + height / 100f) / num_of_params;
            
            // target hit
            if (r > paramaters)
            {
                float lost_of_life = 0;
                lost_of_life += AIParams.CONFIG_DECREASE_OF_LIFE_WHEN_HIT;

                #region diversyfication of life loses by weapon and object characteristics
                //// depending the weapon and skills
                //switch (this._role)
                //{
                //    case AIRoleType.Infantry:
                //        lost_of_life += 0.002f;
                //        break;
                //    case AIRoleType.Support:
                //        lost_of_life += 0.005f;
                //        break;
                //    case AIRoleType.Sniper:
                //        lost_of_life += 0.004f;
                //        break;
                //    default:
                //        lost_of_life += 0.002f;
                //        break;
                //}
                //// depending the movement status
                //switch (_target._status)
                //{
                //    case AIMovementStatus.Walk:
                //        lost_of_life += 0.003f;
                //        break;
                //    case AIMovementStatus.Run:
                //        lost_of_life += 0.001f;
                //        break;
                //    case AIMovementStatus.Crawl:
                //        lost_of_life += 0.0005f;
                //        break;
                //    case AIMovementStatus.Cover:
                //        lost_of_life += 0.0001f;
                //        break;
                //    default:
                //        //throw new Exception("Updating target life: Movement Status error! Status does not exist!");
                //        break;
                //}
                //// leaders are always more tough :)
                //if (_target.IsLeader)
                //    lost_of_life -= 0.001f; 
                #endregion

                // in cover we are laughing
                if (_target._inCover)
                    lost_of_life -= AIParams.CONFIG_COVER_ADD_POINTS;

                // update life
                _target.HealOrHarm(-lost_of_life);

                if(_actualAction == AIAction.Attack)
                    UpdateFitness(AIParams.FITNESS_INCREASE_HIT);
                if(!_target.InCover)
                    _target.UpdateFitness(AIParams.FITNESS_INCREASE_GOT_HIT);
            }

            // TARGET IS DEAD
            if(_target.Life <= 0)
            {
                //Update Fitness
                _target.CalculateActualFitness(AIGame.AIContainer.FramesCounterForEvolution, _target._experience);

                // reset and respawn dead objects
                _target._isDead = true;
                _target.Life = 1;
                _target.Experience = 0;
                _target.Position = new Vector3((float)AIGame.random.NextDouble() * AIGame.terrain.TerrainHeightMap.RealSize, 0, (float)AIGame.random.NextDouble() * AIGame.terrain.TerrainHeightMap.RealSize);
                
                // update target fitness
                if (!_target.InCover)
                {
                    if (_target.AIFitness < AIGame.AIContainer.GenethicAlgorithm.AverageFitness)
                        _target.UpdateFitness(AIParams.FITNESS_INCREASE_DIE);
                    else
                        _target.UpdateFitness(AIParams.FITNESS_INCREASE_DIE_STRONG);
                }

                // update target cover
                if (_target._cover != null && _target._inCover)
                    _target._cover.AIObjects.Remove(_target);
                _target.SetFree();
                _target.CeaseFire();

                // update experiance
                if (_actualAction == AIAction.Attack)
                    BoostOrDecreaseExperience(AIParams.CONFIG_INCREASE_EXP_KILL);
                
                // update fitness
                if (_actualAction == AIAction.Attack && !_isShootAt && _life > 0.3)
                    UpdateFitness(AIParams.FITNESS_INCREASE_KILL);
                else
                    UpdateFitness(AIParams.FITNESS_INCREASE_KILL_CONCIDENCE);
                
                // cease fire
                CeaseFire();
                if (_actualAction == AIAction.Attack)
                    SetFree();
            }
        }
        private void UpdateProperties()
        {
            // objects get tired while running
            if (Status == AIMovementStatus.Run)
            {
                if (IsMoving)
                    MovementSpeed -= Settings.GameSpeed(0.001f);
                else
                    MovementSpeed += Settings.GameSpeed(0.002f);
                if (MovementSpeed > Settings.OBJECTS_MOVEMENT_SPEED)
                    MovementSpeed = Settings.OBJECTS_MOVEMENT_SPEED;
                else if (MovementSpeed <= Settings.OBJECTS_MOVEMENT_SPEED * 0.1)
                    MovementSpeed += Settings.GameSpeed(0.005f);
            }
            // adjust movement Speed to the life thats left.
            //MovementSpeed *= _life;
        }
        private void UpdateText()
        {
            StringBuilder text = new StringBuilder();
            if (!_isBot)
                text.AppendLine("-=PLAYER=-");
            if (Selected)
            {
                if (this.IsLeader)
                    IsBigText = true;
                
                text.AppendLine(this.Identifier + " <" + this.Role.ToString() + " (" + this._sightRange + ")>" +
                                " L: " + Math.Round(_life * 100, 0).ToString() + "%" +
                                " E: " + string.Format("{0}", this._experience) + " pt");
                text.AppendLine("Status: " + "<" + this.Status.ToString() + " (" + Math.Round(this.MovementSpeed, 0) + ")>" +
                                " Target: " + (_target == null ? "none" : _target.Identifier.ToString()) +
                                " See: " + _sightList.Count + " HF: " + _holdFire.ToString());
                text.AppendLine(Math.Round(Position.X, 0) + ", " + Math.Round(Position.Y, 0) + ", " + Math.Round(Position.Z, 0));
                text.AppendLine(_inCover == true ? "In Cover!" : "");
            }
            else
            {
                text.AppendLine(Math.Round(_life * 100, 0).ToString() + "%" +
                   ", " + string.Format("{0}", this._experience) + " pt");
                text.AppendLine("Is: " +Math.Round(Position.X, 0) + ", " + Math.Round(Position.Y, 0) + ", " + Math.Round(Position.Z, 0));
                text.AppendLine("To: " + Math.Round(EndPosition.X, 0) + ", " + Math.Round(EndPosition.Y, 0) + ", " + Math.Round(EndPosition.Z, 0));
                text.AppendLine(_inCover == true ? "In Cover!" : "");
            }
            text.AppendLine("Fit: " + Math.Round(_aiFitness, 1));

            this.Text = text.ToString();
        }

        public void CalculateActualFitness(double framesCounter, double exp)
        {
            _aiFitnessesListLife.Add(framesCounter);
            _aiFitnessesListPenalty.Add(_aiFitnessPenaltyCounter);
            _aiFitnessesListExp.Add(exp);

            double prev_time = 0;
            double total_life_fitness = 0;
            double total_exp_fitness = 0;
            for (int i = 0; i < _aiFitnessesListLife.Count; i++)
            {
                double life_fitness = _aiFitnessesListLife[i] - prev_time;
                total_life_fitness += life_fitness - _aiFitnessesListPenalty[i];
                prev_time = _aiFitnessesListLife[i];

                total_exp_fitness += _aiFitnessesListExp[i];
            }
            _aiFitnessLife = total_life_fitness / _aiFitnessesListLife.Count;
            _aiFitnessExp = (total_exp_fitness / _aiFitnessesListExp.Count) * AIParams.FITNESS_EXPERIENCE_MULTIPLICATION_FACTOR;
            _aiFitness = _aiFitnessLife + _aiFitnessExp + _aiFitnessCustom;
            _aiFitnessPenaltyCounter = 0;
        }

        private int RangeIndex(double value, int numRanges)
        {
            double range = 1.0 / (double)numRanges;
            for (int i = 0; i < numRanges; i++)
            {
                if (range * i <= value && range * (i + 1) > value)
                    return i;
            }
            return (int)Math.Floor(AIGame.random.NextDouble() * (numRanges - 1.0));
        }
        private void UpdateAI()
        {
            // exceptions
            if (_actualAction == AIAction.Attack && _life < 0.5f || _actualAction == AIAction.Attack && _isShootAt)
                SetFree();

            if (_isOccupied && _actualAction != AIAction.Wander) 
                return;

            List<double> inputs = new List<double>();

            // target distance and elevation difference factors
            if (_target == null)
                inputs.Add(0.0);
            else
                inputs.Add(AIUtils.Clamp(1 - _targetDistance / _sightRange + Position.Y / _target.Position.Y / 200));
            // cover
            inputs.Add(_inCover ? 1.0 : AIUtils.Clamp(1 - (GetNearestAvailableStatic(AIStaticType.Cover).Position - Position).Length() / _sightRange));
            // bonus
            inputs.Add(AIUtils.Clamp(1 - (GetNearestAvailableStatic(AIStaticType.Bonus).Position - Position).Length() / _sightRange));
            // med
            inputs.Add(AIUtils.Clamp(1 - (GetNearestAvailableStatic(AIStaticType.FirstAidKit).Position - Position).Length() / _sightRange));
            // life
            inputs.Add((1 - _life));
            // experience
            inputs.Add(_experience / 100);
            // speed
            //inputs.Add(MovementSpeed / Settings.OBJECTS_MOVEMENT_SPEED);
            // is shoot at
            inputs.Add(_isShootAt ? 1.0 : 0.0);

            List<double> output = _aiNeuralBrain.Update(inputs);
            if (output.Count < AIParams.NumOutputs)
                throw new Exception("Brain mulfunction for " + this.Identifier + "!");

            switch (RangeIndex(output[0], 5))
            {
                case 0:
                    if (_target != null)
                    {
                        Attack(_target);
                        _actionCounter[AIAction.Attack]++;
                        Status = AIMovementStatus.Run;
                    }
                    else
                    {
                        PickUpBonus();
                        _actionCounter[AIAction.PUBonus]++;
                        Status = AIMovementStatus.Run;
                    }
                    break;
                case 1:
                    PickUpBonus();
                    _actionCounter[AIAction.PUBonus]++;
                    Status = AIMovementStatus.Run;
                    break;
                case 2:
                    Wander();
                    _actionCounter[AIAction.Wander]++;
                    Status = AIMovementStatus.Walk;
                    break;
                case 3:
                    TakeCover();
                    _actionCounter[AIAction.TakeCover]++;
                    Status = AIMovementStatus.Run;
                    break;
                case 4:
                    PickUpMedKit();
                    _actionCounter[AIAction.PUMed]++;
                    Status = AIMovementStatus.Run;
                    break;
                default:
                    throw new Exception("Not valid number of ANN outputs!");
            }
            #region Old calculations with 5 outputs
            ////assign the outputs to the aiobject physics
            //double max = double.MinValue;
            //int max_index = 0;
            //for (int i = 0; i < output.Count; i++)
            //{
            //    if (output[i] > max)
            //    {
            //        max = output[i];
            //        max_index = i;
            //    }
            //}

            //switch (max_index)
            //{
            //    case 0:
            //        if (_target != null)
            //        {
            //            Attack(_target);
            //            _actionCounter[AIAction.Attack]++;
            //            Status = AIMovementStatus.Run;
            //        }
            //        else
            //        {
            //            PickUpBonus();
            //            _actionCounter[AIAction.PickUpBonus]++;
            //            Status = AIMovementStatus.Run;
            //        }
            //        break;
            //    case 1:
            //        TakeCover();
            //        _actionCounter[AIAction.TakeCover]++;
                  
            //        Status = AIMovementStatus.Run;
            //        break;
            //    case 2:
            //        PickUpMedKit();
            //        _actionCounter[AIAction.PickUpMed]++;
            //        Status = AIMovementStatus.Run;
            //        break;
            //    case 3:
            //        Wander();
            //        _actionCounter[AIAction.Wander]++;
            //        Status = AIMovementStatus.Walk;
            //        break;
            //    default:
            //        throw new Exception("Wrong number of outputs from ANN!");
            //        //Wander();
            //        //_actionCounter[AIAction.Wander]++;
            //        //Status = AIMovementStatus.Walk;
            //        //SetFree();
            //        break;
            //} 
            #endregion
        }
        
        private Vector3 RandomPosition()
        {
            return Position + new Vector3(
                (float)AIUtils.RandomInt(-100, 100),
                (float)AIUtils.RandomInt(-100, 100),
                (float)AIUtils.RandomInt(-100, 100)
                );
        }
        public void UpdateFitness(double value)
        {
            _aiFitnessCustom += value;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateText();
            UpdateSightList();
            UpdateAction();
            //UpdateProperties();
            if(_isBot)
                UpdateAI();

            if (_actualAction == AIAction.Free && (_life < 0.7 || _experience < 70))
                _aiFitnessPenaltyCounter++;

            base.Update(gameTime);
        }

        public void DrawLine(Vector3 point1, Vector3 point2, Color colorStart, Color colorEnd)
        {
            VertexPositionColor[] line = new VertexPositionColor[] 
            {        
                new VertexPositionColor(point1, colorStart),        
                new VertexPositionColor(point2, colorEnd)    
            };
            AIGame.basicEffect.VertexColorEnabled = true;
            AIGame.basicEffect.View = AIGame.camera.ViewMatrix;
            AIGame.basicEffect.Projection = AIGame.camera.ProjectionMatrix;

            // NOTE: migration to XNA 4.0
            // NOTE: AIGame.basicEffect.Begin();
            // NOTE: AIGame.basicEffect.CurrentTechnique.Passes[0].Begin();
            // NOTE: AIGame.graphics.GraphicsDevice.VertexDeclaration = new VertexDeclaration(AIGame.graphics.GraphicsDevice, VertexPositionColor.VertexElements);
            // NOTE: AIGame.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, line, 0, 1);
            // NOTE: AIGame.basicEffect.CurrentTechnique.Passes[0].End();
            // NOTE: AIGame.basicEffect.End();
            AIGame.basicEffect.CurrentTechnique.Passes[0].Apply();
            AIGame.device.DrawUserPrimitives(PrimitiveType.LineList, line, 0, 1);
        }

        public void DrawPoint(Vector3 point, float size, Color color)
        {
            VertexPositionColor[] p = new VertexPositionColor[] 
            {        
                new VertexPositionColor(point, color)     
            };
            AIGame.basicEffect.VertexColorEnabled = true;
            AIGame.basicEffect.View = AIGame.camera.ViewMatrix;
            AIGame.basicEffect.Projection = AIGame.camera.ProjectionMatrix;

            // NOTE: migration to XNA 4.0
            // NOTE: AIGame.basicEffect.Begin();
            // NOTE: AIGame.basicEffect.CurrentTechnique.Passes[0].Begin();
            // NOTE: AIGame.graphics.GraphicsDevice.VertexDeclaration = new VertexDeclaration(AIGame.graphics.GraphicsDevice, VertexPositionColor.VertexElements);
            // NOTE: AIGame.graphics.GraphicsDevice.RenderState.PointSize = size;
            // NOTE: AIGame.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.PointList, p, 0, 1);
            // NOTE: AIGame.basicEffect.CurrentTechnique.Passes[0].End();
            // NOTE: AIGame.basicEffect.End();
            AIGame.basicEffect.CurrentTechnique.Passes[0].Apply();
            AIGame.device.DrawUserPrimitives(PrimitiveType.LineList, p, 0, 1);
        }
        public override void Draw(GameTime gameTime, Matrix projection, Matrix view)
        {
            base.Draw(gameTime, projection, view);

            #region draw range circle
            if (Settings.DRAW_RANGE_CIRCLES)
            {
                List<Vector3> vectors = new List<Vector3>();
                float max = 2 * (float)Math.PI;
                float step = max / (float)36;
                for (float theta = 0; theta < max; theta += step)
                {
                    vectors.Add(
                        new Vector3(
                            Position.X + _sightRange * (float)Math.Sin((double)theta),
                            Position.Y,
                            Position.Z + _sightRange * (float)Math.Cos((double)theta)
                            )
                        );
                }
                vectors.Add(
                    new Vector3(
                        Position.X + _sightRange * (float)Math.Sin(0),
                        Position.Y,
                        Position.Z + _sightRange * (float)Math.Cos(0)
                        )
                    );

                //for (int i = 0; i < vectors.Count; i ++)
                //    DrawPoint(vectors[i], 5, this.Team.TeamColor);
                for (int i = 0; i < vectors.Count; i += 2)
                    DrawLine(vectors[i], vectors[i + 1], Color.White, this.Team.TeamColor);
            }
            #endregion

            #region draw shooting lines
            _delay++;

            if (_delay > 100)
                _delay = 0;
            if (IsShooting && Target != null)
            {
                int delay = 0;
                float miss_shoot_x = ((float)AIGame.random.NextDouble() - 0.5f) * 50;
                float miss_shoot_y = ((float)AIGame.random.NextDouble() - 0.5f) * 50;
                float miss_shoot_z = ((float)AIGame.random.NextDouble() - 0.5f) * 50;
                switch (_role)
                {
                    case AIRoleType.Sniper: miss_shoot_x = miss_shoot_y = miss_shoot_z = 0; delay = 20;  break;
                    case AIRoleType.Support: miss_shoot_x *= 2; miss_shoot_y *= 2; miss_shoot_z *= 2; delay = 3;  break;
                    case AIRoleType.TeamLeader: 
                    case AIRoleType.Infantry: miss_shoot_x /= 2; miss_shoot_y /= 2; miss_shoot_z /= 2; delay = 2; break;
                    default: break;
                }
                if (_delay % delay != 0)
                    return;

                // different color of a fire if it is a fittest one
                Color color1 = Color.Yellow;
                Color color2 = Color.OrangeRed;
                if(_delay % 3 == 0)
                {
                    color1 = Color.LightGoldenrodYellow;
                    color2 = Color.Orange;
                }

                if (AIGame.AIContainer.GenethicAlgorithm.FittestObject != null &&
                    this == AIGame.AIContainer.GenethicAlgorithm.FittestObject)
                {
                    color1 = Color.White;
                    color2 = Color.White;
                }
                DrawLine(Position, Target.Position + new Vector3(miss_shoot_x, miss_shoot_y, miss_shoot_z),
                    color1,
                    color2
                );
            }
            #endregion

            #region draw middle points between fighting objects
            //if (_target != null && Settings.DRAW_STATICS_LABELS && Settings.DRAW_OBJECTS_LABELS)
            //{
            //    Vector3 point_in_between = (Position + _target.Position) / 2;
            //    AIGame.hud.PrintTextInLocation(Math.Round(point_in_between.X, 0) + ", " + Math.Round(point_in_between.Y, 0) + ", " + Math.Round(point_in_between.Z, 0), point_in_between, Color.Honeydew);
            //    point_in_between.Y = AIGame.terrain.TerrainHeightMap.HeightAt(point_in_between.X, point_in_between.Z);
            //    AIGame.hud.PrintTextInLocation("Surface: " + Math.Round(point_in_between.Y, 0), point_in_between, Color.YellowGreen);

            //    Vector3 point_in_quarter = (Position + point_in_between) / 2;
            //    AIGame.hud.PrintTextInLocation(Math.Round(point_in_quarter.X, 0) + ", " + Math.Round(point_in_quarter.Y, 0) + ", " + Math.Round(point_in_quarter.Z, 0), point_in_quarter, Color.Honeydew);
            //    point_in_quarter.Y = AIGame.terrain.TerrainHeightMap.HeightAt(point_in_quarter.X, point_in_quarter.Z);
            //    AIGame.hud.PrintTextInLocation("Surface: " + Math.Round(point_in_quarter.Y, 0), point_in_quarter, Color.YellowGreen);

            //    Vector3 point_in_three_quarters = (point_in_between + _target.Position) / 2;
            //    AIGame.hud.PrintTextInLocation(Math.Round(point_in_three_quarters.X, 0) + ", " + Math.Round(point_in_three_quarters.Y, 0) + ", " + Math.Round(point_in_three_quarters.Z, 0), point_in_three_quarters, Color.Honeydew);
            //    point_in_three_quarters.Y = AIGame.terrain.TerrainHeightMap.HeightAt(point_in_three_quarters.X, point_in_three_quarters.Z);
            //    AIGame.hud.PrintTextInLocation("Surface: " + Math.Round(point_in_three_quarters.Y, 0), point_in_three_quarters, Color.YellowGreen);
            //} 
            #endregion
        }
        #endregion
    }
}
