using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AIGame
{
    public class ModelHandler
    {
        #region Fields
        // model
        private Model _model;

        // initial and basic transformations
        private Vector3 _position = Vector3.Zero;
        private float _scale = 1.0f;
        private float _scaleBase = 1.0f;

        public float ScaleBase
        {
            get { return _scaleBase; }
        }

       
        private Vector3 _rotation = Vector3.Zero;

        // elevation of the model related to his bounding box
        private float _feetElevation;

        // bounding box render variables.
        private VertexPositionColor[] _points;
        private short[] _index;
        private BoundingBox _boundingBox;
        private Vector3 _boundsRealMin;
        private Vector3 _boundsRealMax;
        private float _boundOffset = 100f;
        private float _boundsAreaCircle;

        // movement
        private Vector3 _viewDirection;
        private Vector3 _endPosition;
        private float _endDirection;
        public float EndDirection
        {
            get { return _endDirection; }
            set { _endDirection = value; }
        }
        private float _rotation2D;
        public float Direction2D
        {
            get { return _rotation2D; }
            set
            {
                _rotation2D = value;
                if (_rotation2D < 0)
                    _rotation2D += (float)(Math.PI * 2);
                else if (_rotation2D > Math.PI * 2)
                    _rotation2D -= (float)(Math.PI * 2);
            }
        }
        private float _movementSpeed;
        private float _moveOffset;
        private float _turningSpeed;
        private bool _isMoving = false;
        private bool _isMovingPrevStatus;
        private bool _isFarEnoughToRotateWhileMoving
        {
            get
            {
                if ((MathExtra.VectorOnXZAxis(_position) - MathExtra.VectorOnXZAxis(_endPosition)).Length() >= _movementSpeed / Math.Sin(_turningSpeed) + _moveOffset
                    || Direction2D == _endDirection)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        private Matrix[] _boneTransforms;
        private Matrix _worldTransformMatrix = Matrix.CreateTranslation(0f, 0f, 0f);

        private SpriteBatch _spriteBatch;
        private SpriteFont _textFont;
        private SpriteFont _textFont_bold;
        private SpriteFont _textFont_small;
        private Color _textFont_color;

        private string _modelText;
        private bool _bigText;
        private bool _smallText;


        private bool _selected = false;

        private Effect _shader;

        #endregion

        #region Properties
        /// <summary>
        /// Gets the model object.
        /// </summary>
        public Model Model
        {
            get { return _model; }
        }

        /// <summary>
        /// Gets or sets the actual position of the model. Overrides movement calculations.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set 
            { 
                _position = value;
                _endPosition = value;
            }
        }

        /// <summary>
        /// Gets the target position of the movement.
        /// </summary>
        public Vector3 EndPosition
        {
            get { return _endPosition; }
        }

        /// <summary>
        /// Gets and sets the scale of the model. When setting, scales bounding box and feet elevation as well.
        /// </summary>
        public float Scale
        {
            get { return _scale; }
            set 
            {
                _scale = value;
                _boundsRealMin *= _scale;
                _boundsRealMax *= _scale;
                _feetElevation *= _scale;
            }
        }

        /// <summary>
        /// Gets or sets the non-relative rotation of the model (not related with the movement).
        /// </summary>
        public Vector3 Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        /// <summary>
        /// Gets the actual viewing direction vector for the model.
        /// </summary>
        public Vector3 ViewDirection
        {
            get { return _viewDirection; }
        }

        public float FeetElevation
        {
            get { return _feetElevation; }
            set { _feetElevation = value; }
        }

        /// <summary>
        /// Gets or sets movement speed of the model.
        /// </summary>
        public float MovementSpeed
        {
            get { return _movementSpeed; }
            set { _movementSpeed = value; }
        }

        /// <summary>
        /// Sets or gets the text that is displayed on the screen along with the model.
        /// </summary>
        public string Text
        {
            get { return _modelText; }
            set { _modelText = value; }
        }

        /// <summary>
        /// Sets or gets value indicating if model text is displayed with the bolded and bigger font.
        /// </summary>
        public bool IsBigText
        {
            get { return _bigText; }
            set { _bigText = value; }
        }

        /// <summary>
        /// Sets or gets value indicating if model text is displayed with the smaller font.
        /// </summary>
        public bool IsSmallText
        {
            get { return _smallText; }
            set { _smallText = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating if model is selected or not.
        /// </summary>
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; }
        }

        /// <summary>
        /// Gets the biggest outside bounding box for the model. 
        /// Value with the offset used for collision detection.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }
        }

        /// <summary>
        /// Gets the real bounding box values for the lower left corner (Min).
        /// Not used for collision detection. Used to draw the bounding box.
        /// </summary>
        public Vector3 BoundsRealMin
        {
            get { return _boundsRealMin; }
        }

        /// <summary>
        /// Gets the real bounding box values for the upper right corner (Max).
        /// Not used for collision detection. Used to draw the bounding box.
        /// </summary>
        public Vector3 BoundsRealMax
        {
            get { return _boundsRealMax; }
        }

        /// <summary>
        /// Gets the size of the bounds area circle.
        /// </summary>
        public float BoundsAreaCircle
        {
            get { return _boundsAreaCircle; }
        }

        /// <summary>
        /// Checks if the model is currently moving. 
        /// </summary>
        public bool IsMoving
        {
            get { return _isMoving; }
        }

        /// <summary>
        /// Checks if the model has just stopped.
        /// </summary>
        public bool HasJustStopped
        {
            get { return _isMoving == false && _isMovingPrevStatus == true; }
        }
        #endregion

        #region Public Methods

        public void SetScale(float scale)
        {
            Scale = scale;
            _scaleBase = scale;
        }

        /// <summary>
        /// Initiate movement of the model to target position.
        /// </summary>
        /// <param name="target">Target position.</param>
        public virtual void MoveTo(Vector3 target)
        {
            target = NormalizeEndPosition(MathExtra.VectorOnXZAxis(target));
            // if target is same as position, do not move and rotate, if we move it will rotate character 180 degrees
            if (MathExtra.VectorOnXZAxis(_position) == target)
                return;
            _endPosition = target;
            TurnToTarget(target);
            _isMovingPrevStatus = _isMoving;
            _isMoving = true;
        }

        /// <summary>
        /// Initiate movement of the model to target position.
        /// </summary>
        /// <param name="x">Target X value.</param>
        /// <param name="z">Target Z value.</param>
        public virtual void MoveTo(float x, float z)
        {
            Vector3 target = MathExtra.VectorOnXZAxis(new Vector3(x, 0, z));
            MoveTo(target);
        }

        private Vector3 NormalizeEndPosition(Vector3 target_position)
        {
            float max = AIGame.terrain.TerrainHeightMap.RealSize;
            float min = 0.0f;

            if (target_position.X > max)
                target_position.X = max - 100;
            else if (target_position.X <= min)
                target_position.X = min + 100;

            if (target_position.Z > max)
                target_position.Z = max - 100;
            else if (target_position.Z <= min)
                target_position.Z = min + 100;

            return target_position;
        }

        /// <summary>
        /// Stops the model in current position.
        /// </summary>
        public virtual void Stop()
        {
            _endPosition = _position;
            _isMovingPrevStatus = _isMoving;
            _isMoving = false;
        } 
        #endregion

        #region Virtual Methods

        public virtual void Initialize(Model model)
        {
            _model = model;
            _movementSpeed = Settings.GameSpeed(_movementSpeed);//Settings.OBJECTS_MOVEMENT_SPEED);
            _turningSpeed = Settings.GameSpeed(Settings.OBJECTS_TURNING_SPEED);

            _moveOffset = _movementSpeed * 10;

            _boneTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_boneTransforms);

            _textFont = AIGame.content.Load<SpriteFont>(@"Fonts\tahoma");
            _textFont_bold = AIGame.content.Load<SpriteFont>(@"Fonts\tahoma_small_bold");
            _textFont_small = AIGame.content.Load<SpriteFont>(@"Fonts\tahoma_small");
            _textFont_color = Color.White;
            _spriteBatch = new SpriteBatch(AIGame.graphics.GraphicsDevice);

            _shader = AIGame.content.Load<Effect>(@"Shaders\Model");

            // NOTE: migration to XNA 4.0 _boundingBox = GetBoundingBoxFromModel(_model);
            _boundingBox = GetBoundingBoxFromModel(_model, AIGame.worldMatrix);

            _boundsRealMin = _boundingBox.Min;
            _boundsRealMax = _boundingBox.Max;
            _boundingBox = new BoundingBox(
                _boundsRealMin * new Vector3(_boundOffset) + _worldTransformMatrix.Translation,
                _boundsRealMax * new Vector3(_boundOffset) + _worldTransformMatrix.Translation
                );

            _boundsAreaCircle = (_boundingBox.Max.Y - _boundingBox.Min.Y) / 2 + _boundOffset;

            _feetElevation = (_boundingBox.Max.Y - _boundingBox.Min.Y) / 2;
            _feetElevation += _boundOffset;
            //_position = AIGame.terrain.TerrainCenter;
            //_endPosition = _position;

            // set up index buffers for drawing bounding box
            BuildBoxCorners();

            // set the shaders parameters
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (AI.AIParams.CONFIG_ENABLE_DEFAULT_LIGHTING)
                    {
                        effect.EnableDefaultLighting();
                        effect.SpecularColor = Settings.LIGHT_SPECULAR_COLOR;
                        effect.SpecularPower = Settings.LIGHT_SPECULAR_POWER_OBJECTS;
                        effect.LightingEnabled = true;
                    }
                    else
                    {
                        effect.LightingEnabled = false;
                    }
                    effect.FogEnabled = Settings.FOG_ENABLED;
                    effect.FogColor = Settings.FOG_COLOR.ToVector3();
                    effect.FogStart = Settings.FOG_START;
                    effect.FogEnd = Settings.FOG_END;
                }
            }
        }

        public virtual void Draw(GameTime gameTime, Matrix projection, Matrix view)
        {
            foreach (ModelMesh mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = _boneTransforms[mesh.ParentBone.Index] * _worldTransformMatrix;
                    if (AI.AIParams.CONFIG_ENABLE_DEFAULT_LIGHTING)
                    {
                        effect.EnableDefaultLighting();
                        effect.SpecularColor = Settings.LIGHT_SPECULAR_COLOR;
                        effect.SpecularPower = Settings.LIGHT_SPECULAR_POWER_OBJECTS;
                        effect.LightingEnabled = true;
                    }
                    else
                    {
                        effect.LightingEnabled = false;
                    }
                }
                mesh.Draw();
                if (Settings.DRAW_BOUNDING_BOXES)
                    if (
                       // mesh.Name.Contains("Torus") || 
                        mesh.Name.Contains("Sphere"))
                        AIGame.graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList, _points, 0, 8, _index, 0, 12);
            }

            
            #region draw text
            // Draw info text over the mesh
            if (!string.IsNullOrEmpty(_modelText) && (Settings.DRAW_OBJECTS_LABELS || _selected))
            {
                // check if the object is on a proper side of the view matrix
                Vector3 v1 = AIGame.camera.ViewDirection;
                Vector3 v2 = _position - AIGame.camera.Position;
                v1.Normalize();
                v2.Normalize();
                if (Vector3.Dot(v1, v2) < 0)
                    return;

                // check if object is selected
                SpriteFont temp_font = _textFont;
                Color temp_color = Color.White;
                if (_selected && _bigText)
                {
                    temp_font = _textFont_bold;
                    temp_color = Color.White;
                }
                else if (_selected)
                {
                    temp_font = _textFont;
                    temp_color = Color.White;
                }
                else
                {
                    if (_smallText)
                    {
                        if (!Settings.DRAW_STATICS_LABELS)
                            return;
                        temp_font = _textFont_small;
                        temp_color = Color.BlanchedAlmond;
                    }
                    else
                    {
                        temp_font = _textFont;
                        temp_color = Color.GreenYellow;
                    }
                }

                Vector3 pos2d = AIGame.graphics.GraphicsDevice.Viewport.Project(
                  _position,
                  AIGame.camera.ProjectionMatrix,
                  AIGame.camera.ViewMatrix,
                  Matrix.Identity);

                if (_smallText)
                    pos2d -= new Vector3(50);

                string[] lines = _modelText.Split('\n');

                // NOTE: migration to XNA 4.0
                // NOTE: _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                // NOTE: _spriteBatch.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                int i = 0;
                foreach (string line in lines)
                {
                    _spriteBatch.DrawString(temp_font, line, new Vector2(pos2d.X, pos2d.Y + 15 * i), temp_color);
                    i++;
                }
                _spriteBatch.End();
            } 
            #endregion
        }

        public virtual void Update(GameTime gameTime)
        {
            // update bounding box
            _boundingBox = new BoundingBox(
                _boundsRealMin * new Vector3(_boundOffset) + _worldTransformMatrix.Translation,
                _boundsRealMax * new Vector3(_boundOffset) + _worldTransformMatrix.Translation
                );

            if (Direction2D != _endDirection)
            {   
                    TurnTo();
            }
            if (_position * new Vector3(1, 0, 1) != _endPosition * new Vector3(1, 0, 1))
            {
                if (_isFarEnoughToRotateWhileMoving)
                    MoveTo();
            }

            _position.Y = AIGame.terrain.TerrainHeightMap.HeightAt(_position.X, _position.Z) +_feetElevation;
            Matrix positionMatrix =
                 Matrix.CreateRotationY(-Direction2D)
               * Matrix.CreateTranslation(_position / _scale)
               * Matrix.CreateScale(_scale);
            _worldTransformMatrix = positionMatrix;

            _viewDirection = positionMatrix.Forward;

            //System.Diagnostics.Debug.WriteLine("BBmax: " + _boundingBox.Max.X + ", " + _boundingBox.Max.Y + ", " + _boundingBox.Max.Z);
            //System.Diagnostics.Debug.WriteLine("BBmin: " + _boundingBox.Min.X + ", " + _boundingBox.Min.Y + ", " + _boundingBox.Min.Z);
            //System.Diagnostics.Debug.WriteLine("Translation V:" + _worldTransformMatrix.Translation.X + ", " + _worldTransformMatrix.Translation.Y + ", " + _worldTransformMatrix.Translation.Z);
        }
        #endregion

        #region Private Methods
        private BoundingBox GetBoundingBoxFromModel(Model model, Matrix worldTransform)
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), worldTransform);

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            return new BoundingBox(min, max);
        }

// NOTE: migration to XNA 4.0
/*
        private BoundingBox GetBoundingBoxFromModel(Model model)
        {
            BoundingBox boundingBox = new BoundingBox();
            for (int meshNumber = 0; meshNumber < model.Meshes.Count; meshNumber++)
            {
                ModelMesh mesh = model.Meshes[meshNumber];
                VertexPositionNormalTexture[] vertices =
                      new VertexPositionNormalTexture[mesh.VertexBuffer.SizeInBytes / VertexPositionNormalTexture.SizeInBytes];

                mesh.VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);

                Vector3[] vertexs = new Vector3[vertices.Length];

                for (int index = 0; index < vertexs.Length; index++)
                    vertexs[index] = vertices[index].Position;

                //BoundingBox Transforms
                boundingBox = BoundingBox.CreateMerged(boundingBox, BoundingBox.CreateFromPoints(vertexs));
                boundingBox = fixBoundingBoxData(boundingBox);
            }
            return boundingBox;
        }
*/
/*
        private BoundingBox fixBoundingBoxData(BoundingBox AABB)
        {
            Vector3 min = AABB.Min, max = AABB.Max;
            float tempData = 0f;
            if (min.X > max.X) { tempData = min.X; min.X = max.X; max.X = tempData; }
            if (min.Y > max.Y) { tempData = min.Y; min.Y = max.Y; max.Y = tempData; }
            if (min.Z > max.Z) { tempData = min.Z; min.Z = max.Z; max.Z = tempData; }
            AABB.Min = min;
            AABB.Max = max;
            return AABB;
        }
*/
/*
        private BoundingBox GetmodelBoundingBox(Model model)
        {
            Vector3 Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    int stride = part.VertexStride;
                    int numberv = part.NumVertices;
                    byte[] data = new byte[stride * numberv];
                    //System.Diagnostics.Debug.WriteLine("stride=" + stride.ToString() +
                    //                                           "numv  =" + numberv.ToString());
                    mesh.VertexBuffer.GetData<byte>(data);

                    for (int ndx = 0; ndx < data.Length; ndx += stride)
                    {
                        float floatvaluex = BitConverter.ToSingle(data, ndx);
                        float floatvaluey = BitConverter.ToSingle(data, ndx + 4);
                        float floatvaluez = BitConverter.ToSingle(data, ndx + 8);
                        if (floatvaluex < Min.X) Min.X = floatvaluex;
                        if (floatvaluex > Max.X) Max.X = floatvaluex;
                        if (floatvaluey < Min.Y) Min.Y = floatvaluey;
                        if (floatvaluey > Max.Y) Max.Y = floatvaluey;
                        if (floatvaluez < Min.Z) Min.Z = floatvaluez;
                        if (floatvaluez > Max.Z) Max.Z = floatvaluez;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine(" Min=" + Min.ToString());
            System.Diagnostics.Debug.WriteLine(" Max=" + Max.ToString());
            BoundingBox boundingbox = new BoundingBox(Min, Max); 
            return boundingbox;
        }
*/

        private void BuildBoxCorners()
        {
            _points = new VertexPositionColor[8];

            //Vector3[] corners = new BoundingBox(BoundingBox.Min / new Vector3(_boundOffset), BoundingBox.Max / new Vector3(_boundOffset)).GetCorners();
            Vector3[] corners = new BoundingBox(_boundsRealMin, _boundsRealMax).GetCorners();

            _points[0] = new VertexPositionColor(corners[1], Color.Green);
            _points[1] = new VertexPositionColor(corners[0], Color.Green);
            _points[2] = new VertexPositionColor(corners[2], Color.Green);
            _points[3] = new VertexPositionColor(corners[3], Color.Green);
            _points[4] = new VertexPositionColor(corners[5], Color.Green);
            _points[5] = new VertexPositionColor(corners[4], Color.Green);
            _points[6] = new VertexPositionColor(corners[6], Color.Green);
            _points[7] = new VertexPositionColor(corners[7], Color.Green);

            short[] inds = {
			    0, 1, 0, 2, 1, 3, 2, 3,
			    4, 5, 4, 6, 5, 7, 6, 7,
			    0, 4, 1, 5, 2, 6, 3, 7
		        };

            _index = inds;
        }

        private void MoveTo()
        {
            Vector3 diff = _position * new Vector3(1, 0, 1) - _endPosition * new Vector3(1, 0, 1);
            float distance = diff.Length();
            if (distance < _movementSpeed)
            {
                _position = _endPosition;
                _isMovingPrevStatus = _isMoving;
                _isMoving = false;
            }
            else
            {
                _worldTransformMatrix = Matrix.CreateTranslation(-_position.X, -_position.Y, -_position.Z);
                _worldTransformMatrix = Matrix.CreateRotationY(Direction2D);
                _worldTransformMatrix *= Matrix.CreateTranslation(0, 0, Settings.GameSpeed(-_movementSpeed));
                _worldTransformMatrix *= Matrix.CreateRotationY(-Direction2D);
                _worldTransformMatrix *= Matrix.CreateTranslation(_position.X, _position.Y, _position.Z);
                Vector3 dest = _worldTransformMatrix.Translation;
                _position.X = dest.X;
                _position.Z = dest.Z;
                if (_isFarEnoughToRotateWhileMoving)
                    TurnToTarget(_endPosition);

                _isMovingPrevStatus = _isMoving;
                _isMoving = true;
            }
        }
        private void TurnTo()
        {
            float d = _endDirection - Direction2D;
            if (d < 0 && -d < _turningSpeed || d > 0 && d < _turningSpeed)
                Direction2D = _endDirection;
            else
            {
                if (d > 0 && d > Math.PI)
                    Direction2D -= Settings.GameSpeed(_turningSpeed);
                else if (d < 0 && (-d < Math.PI))
                    Direction2D -= Settings.GameSpeed(_turningSpeed);
                else
                    Direction2D += Settings.GameSpeed(_turningSpeed);
            }
        }

        public void TurnToTarget(Vector3 target)
        {
            Vector3 d = target - _position;
            double a = 0;
            float m = 0.5f;

            if (d.X != 0 && d.Z != 0)
                a = Math.Atan(d.Z / d.X);
            else if (d.X == 0 && d.Z > 0)
                m = 0.0f;

            if (d.X < 0)
                a -= (Math.PI * m);
            else
                a += (Math.PI * m);

            if (a < 0)
                a += (Math.PI * 2);

            _endDirection = (float)a;
        }
        #endregion
    }
}
