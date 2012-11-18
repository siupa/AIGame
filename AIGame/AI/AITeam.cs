using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace AIGame.AI
{
    public class AITeam : Dictionary<string, AIObject>
    {
        #region Fields 
        private AITeamType _teamType;
        private AIObject _selectedObject;
        private AIObject _leader;
        private bool _operateAsAUnit = false;
        private AIFormation _formation;
        private AIObjective _objective;
        private bool _keepFormation;
        private Color _color;
        #endregion

        #region Properties
        public AITeamType TeamType
        {
            get { return _teamType; }
        }
        public AIObject SelectedObject
        {
            get { return _selectedObject; }
        }
        public AIObject Leader
        {
            get { return _leader; }
        }
        public bool OperateAsAUnit
        {
            get { return _operateAsAUnit; }
            set { _operateAsAUnit = value; }
        }
        public AIFormation Formation
        {
            get { return _formation; }
        }
        public AIObjective Objective
        {
            get { return _objective; }
            set { _objective = value; }
        }
        public bool KeepFormation
        {
            get { return _keepFormation; }
            set 
            {
                if (_formation == AIFormation.Spread)
                    _keepFormation = false;
                else
                    _keepFormation = value; 
            }
        }
        public Color TeamColor
        {
            get { return _color; }
        }
        
        #endregion

        #region Constructors
        public AITeam(AITeamType type, Color color)
        {
            _teamType = type;
            _selectedObject = null;
            _leader = null;
            _formation = AIFormation.Row;
            _objective = AIObjective.Hold;
            _color = color;
        }
        public AITeam(AITeamType type, Color color, AIFormation formation, AIObjective objective)
        {
            _teamType = type;
            _selectedObject = null;
            _leader = null;
            _formation = formation;
            _objective = objective;
            _color = color;
        } 
        #endregion

        #region Public Methods
        public void Add(AIObject item)
        {
            if (string.IsNullOrEmpty(item.Identifier))
                item.Identifier = AIContainer.GetUniqueIdentifier();
            item.Team = this;
            if (item.IsLeader)
                _leader = item;

            AIContainer.Objects.Add(item);
            base.Add(item.Identifier, item);
        }

        public void ToStatus(AIMovementStatus status)
        {
            foreach (AIObject aiobj in this.Values)
            {
                aiobj.Status = status;
            }
        }

        public void ToggleStatus()
        {
            if (_leader.Status == AIMovementStatus.Walk)
                ToStatus(AIMovementStatus.Run);
            else if (_leader.Status == AIMovementStatus.Run)
                ToStatus(AIMovementStatus.Crawl);
            else if (_leader.Status == AIMovementStatus.Crawl)
                ToStatus(AIMovementStatus.Cover);
            else if (_leader.Status == AIMovementStatus.Cover)
                ToStatus(AIMovementStatus.Walk);
        }

        /// <summary>
        /// Move the selected team to the formation.
        /// </summary>
        /// <param name="formation">Type of the formation</param>
        public void ToFormation(AIFormation formation)
        {
            _formation = formation;
            if (_leader == null)
            {
                AIGame.Console.Show();
                AIGame.Console.Add("Team '" + _teamType + "' does not have a leader. Can not form '" + _formation + "'.");
                return;
            }
            switch (formation)
            {
                case AIFormation.Row:
                    _keepFormation = true;
                    FormRow();
                    break;
                case AIFormation.Arrow:
                    _keepFormation = true;
                    FormArrow();
                    break;
                case AIFormation.Spread:
                    _keepFormation = false;
                    FormSpread();
                    break;
                default:
                    break;
            }
        }

        public void ToggleFormation()
        {
            if (_formation == AIFormation.Row)
                ToFormation(AIFormation.Arrow);
            else if (_formation == AIFormation.Arrow)
                ToFormation(AIFormation.Spread);
            else if (_formation == AIFormation.Spread)
                ToFormation(AIFormation.Row);
        } 
        #endregion

        #region Private Methods
        private void FormRow()
        {
            float side = 1;
            float row_num = 1;
            foreach (AIObject aiobj in this.Values)
            {
                float offset = aiobj.BoundsAreaCircle;

                if (!aiobj.IsLeader)
                {
                    float angle = _leader.EndDirection;
                    float xprim = _leader.EndPosition.X + ((float)Math.Cos(angle)) * offset * side * (float)Math.Floor(row_num);
                    float zprim = _leader.EndPosition.Z + ((float)Math.Sin(angle)) * offset * side * (float)Math.Floor(row_num);
                    //System.Diagnostics.Debug.WriteLine("Math.Cos(angle): " + Math.Cos(l.Direction2D).ToString());
                    //System.Diagnostics.Debug.WriteLine("Math.Sin(angle): " + Math.Sin(l.Direction2D).ToString());
                    aiobj.MoveTo(new Vector3(xprim, 0, zprim));
                    if (aiobj.HasJustStopped && aiobj.Team.KeepFormation) aiobj.EndDirection = _leader.EndDirection;
                    side *= -1;
                    row_num += 0.5f;
                }
            }
        }

        private void FormArrow()
        {
            float side = 1;
            float row_num = 1;
            foreach (AIObject aiobj in this.Values)
            {
                if (!aiobj.IsLeader)
                {
                    float offset = aiobj.BoundsAreaCircle;

                    float angle = _leader.EndDirection;
                    if (side > 0)
                        angle += ((float)Math.PI / 4) * 3;
                    else
                        angle += ((float)Math.PI / 4);
                    float xprim = _leader.EndPosition.X + ((float)Math.Cos(angle)) * offset * (float)Math.Floor(row_num);
                    float zprim = _leader.EndPosition.Z + ((float)Math.Sin(angle)) * offset * (float)Math.Floor(row_num);
                    aiobj.MoveTo(new Vector3(xprim, 0, zprim));
                    if (aiobj.HasJustStopped && aiobj.Team.KeepFormation) aiobj.EndDirection = _leader.EndDirection;
                    row_num += 0.5f;
                    side *= -1;
                }

            }
        }

        private void FormSpread()
        {
            Random r = new Random();
            float side = 1;
            float row_num = 1;
            foreach (AIObject aiobj in this.Values)
            {
                if (!aiobj.IsLeader)
                {
                    float offset = aiobj.BoundsAreaCircle * (float)(r.NextDouble() + 3.0);
                    float angle = _leader.EndDirection;
                    if (side > 0)
                        angle += (float)((Math.PI / 4) * 3 + r.NextDouble() * Math.PI * 2);
                    else
                        angle += (float)((Math.PI / 4) + r.NextDouble() * Math.PI * 2);

                    float xprim = _leader.EndPosition.X + ((float)Math.Cos(angle)) * offset * (float)Math.Floor(row_num);
                    float zprim = _leader.EndPosition.Z + ((float)Math.Sin(angle)) * offset * (float)Math.Floor(row_num);
                    aiobj.MoveTo(new Vector3(xprim, 0, zprim));
                    row_num += 0.5f;
                    side *= -1;
                }

            }
        } 
        #endregion
    }
}
