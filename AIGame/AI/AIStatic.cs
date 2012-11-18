using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIGame.AI
{
    public enum AIStaticType
    {
        Cover,
        FirstAidKit,
        Bonus
    }
    public class AIStatic : ModelHandler
    {
        #region Fields
        private string _identifier;
        private AIStaticType _type;
        private float _radius;
        private List<AIObject> _aiobjects = new List<AIObject>();
        private AIObject[] _aiobjects_prev;
        private bool _isGone = false;

        private AITeamType _teamInside = AITeamType.NULL;

        public AITeamType TeamInside
        {
            get { return _teamInside; }
            set { _teamInside = value; }
        }
        #endregion

        #region Properties
        // public
        public string Identifier
        {
            get { return _identifier; }
        }
        public AIStaticType Type
        {
            get { return _type; }
        }
        public List<AIObject> AIObjects
        {
            get { return _aiobjects; }
        }
        public AIObject[] AIObjects_Prev
        {
            get { return _aiobjects_prev; }
            set { _aiobjects_prev = value; }
        }
        // private
        private bool IsEmpty
        {
            get { return _aiobjects.Count == 0; }
        }
        
        public bool IsGone
        {
            get { return _isGone; }
            set { _isGone = value; }
        }
        #endregion

        #region Constructor
        public AIStatic(string id, AIStaticType type)
            : base()
        {
            _identifier = id;
            _type = type;
            _radius = 60;
            _aiobjects = new List<AIObject>();
            _aiobjects_prev = new AIObject[100];
            IsSmallText = true;
        } 
        #endregion

        #region Public Methods
        public bool IsAllowedToEnter(AIObject aiobj)
        {
            if (IsEmpty)
                return true;
            else
                //    return aiobj.Team.TeamType == TeamInside;
                return false;
        }

        public bool ObjectJustArrived(AIObject aiobj)
        {
            return !PreviousStateContains(aiobj) && _aiobjects.Contains(aiobj);
        }

        public bool ObjectJustLeft(AIObject aiobj)
        {
            return PreviousStateContains(aiobj) && !_aiobjects.Contains(aiobj);
        }

        private bool PreviousStateContains(AIObject aiobj)
        {
            foreach (AIObject obj in _aiobjects_prev)
            {
                if (obj == aiobj)
                    return true;
            }
            return false;
        }

        public bool IsObjectLocatedInside(AIObject aiobj)
        {
            return (this.Position - aiobj.Position).Length() <= _radius;
        } 
        #endregion

        #region Update and Draw Methods
        private void UpdateText()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine(_identifier);
            if(this._type == AIStaticType.Cover)
                text.AppendLine((TeamInside == AITeamType.NULL ? "empty" : TeamInside.ToString()) + " (" + _aiobjects.Count + ")");
            text.AppendLine(Math.Round(Position.X, 0) + ", " + Math.Round(Position.Y, 0) + ", " + Math.Round(Position.Z, 0));
            this.Text = text.ToString();
        }
        public override void Update(GameTime gameTime)
        {
            UpdateText();
            base.Update(gameTime);
        } 
        #endregion
    }
}
