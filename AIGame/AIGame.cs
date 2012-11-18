using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using AIGame.AI;

namespace AIGame
{
    /// <summary>
    /// Sample showing how to use geometry that is programatically
    /// generated as part of the content pipeline build process.
    /// </summary>
    public class AIGame : Microsoft.Xna.Framework.Game
    {
        #region Fields
        private static AIContainer _aicontainer = new AIContainer();
        public static AIContainer AIContainer
        {
            get { return _aicontainer; }
            set { _aicontainer = value; }
        }

        public static KeyboardState currentKeyboardState;
        public static KeyboardState prevKeyboardState;

        public static MouseState currentMouseState;
        public static MouseState prevMouseState;

        public static GraphicsDeviceManager graphics;
        public static GraphicsDevice device;
        public static ContentManager content;

        public static CameraComponent camera;
        public static Matrix worldMatrix;

        private SpriteBatch spriteBatch;

        private int _windowWidth;
        private int _windowHeight;

        public World world;
        public static Terrain terrain;

        public GameTime gameTime;

        public static ScreenOutput.HUD HUD;
        private static ScreenOutput.Console _console;
        public static ScreenOutput.Console Console
        {
            get { return _console; }
            set { _console = value; }
        }

        public static Cursor cursor;

        public static Random random;

        public static BasicEffect basicEffect;


        #endregion

        #region Properties

        #endregion

        #region Initialization

        public AIGame()
        {
            Window.Title = "AIGame - Neural Networks and Genetic Algorithms";
            graphics = new GraphicsDeviceManager(this);
            content = new ContentManager(Services);
            content.RootDirectory = "Content";

            this.IsMouseVisible = true;
            IsFixedTimeStep = false;

            random = new Random();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            AIParams.SetUpAIParams(Path.Combine(Directory.GetCurrentDirectory(), "AIConfig.txt"));

            device = graphics.GraphicsDevice;
            spriteBatch = new SpriteBatch(device);

            world = new World(this);


            _windowWidth = 1000;// GraphicsDevice.DisplayMode.Width / 2;
            _windowHeight = 600;// GraphicsDevice.DisplayMode.Height / 2;

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = _windowWidth;
            graphics.PreferredBackBufferHeight = _windowHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();

            // Setup the initial input states.
            currentKeyboardState = Keyboard.GetState();

            // Setup Fog
            // NOTE: migration to XNA 4.0
            // NOTE: graphics.GraphicsDevice.RenderState.FogStart = Settings.FOG_START;
            // NOTE: graphics.GraphicsDevice.RenderState.FogEnd = Settings.FOG_END;
            // NOTE: graphics.GraphicsDevice.RenderState.FogDensity = Settings.FOG_DENSITY;
            // NOTE: graphics.GraphicsDevice.RenderState.FogColor = Settings.FOG_COLOR;
            // NOTE: graphics.GraphicsDevice.RenderState.FogTableMode = Settings.FOG_MODE;
            // NOTE: graphics.GraphicsDevice.RenderState.FogVertexMode = Settings.FOG_MODE;

            HUD = new ScreenOutput.HUD();
            _console = new ScreenOutput.Console();
            cursor = new Cursor(this, device, spriteBatch);
            Components.Add(cursor);
            camera = new CameraComponent(this);
            Components.Add(camera);

            // NOTE: migration to XNA 4.0 basicEffect = new BasicEffect(graphics.GraphicsDevice, null);
            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            world.Load(content);

            // Setup terrain.
            terrain = new Terrain(GraphicsDevice, content);
            terrain.Create(128, 20, 0.0f, 2048.0f, Terrain.HeightMapGenerationMethod.FromBitMap, AIParams.CONFIG_TERREIN_HEIGHT_PARAMETER);

            //terrain.AddRegion(-1000.0f, 2048.0f, @"Textures\region_grass_thin");

            terrain.AddRegion(-120.0f, -10.0f, @"Textures\region_soil_dry2");
            terrain.AddRegion(-10.0f, 500.0f, @"Textures\region_grass_thin");
            terrain.AddRegion(400.0f, 800.0f, @"Textures\region_rock1");
            //terrain.AddRegion(700.0f, 2048.0f, @"Textures\region_rock2");

            //terrain.AddRegion(-120.0f, 70.0f, @"Textures\region_soil_dry2");
            //terrain.AddRegion(20.0f, 700.0f, @"Textures\region_grass_thin");
            //terrain.AddRegion(700.0f, 1000.0f, @"Textures\region_rock1");
            //terrain.AddRegion(1001.0f, 2048.0f, @"Textures\region_rock2");

            terrain.SunlightDirection = Vector3.Down;
            terrain.SunlightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            terrain.TerrainAmbient = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
            terrain.TerrainDiffuse = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);

            // Setup camera.
            camera.CurrentBehavior = Camera.Behavior.FreeView;
            camera.Velocity = new Vector3(Settings.CAMERA_VELOCITY);
            camera.Acceleration = new Vector3(Settings.CAMERA_ACCELERATION);
            camera.Perspective(80, (float)_windowWidth / (float)_windowHeight, 0.1f, terrain.TerrainBoundingSphere.Radius * 4.0f);
            Vector3 cameraPos = new Vector3();
            cameraPos.X = terrain.TerrainCenter.X;
            cameraPos.Z = terrain.TerrainCenter.Z;
            cameraPos.Y = terrain.TerrainHeightMap.HeightAt(cameraPos.X, cameraPos.Z) * 1.5f + 2000;
            camera.LookAt(cameraPos, new Vector3(terrain.TerrainCenter.X + 1, terrain.TerrainHeightMap.HeightAt(cameraPos.X, cameraPos.Z), terrain.TerrainCenter.Z + 1), Vector3.Up);

            worldMatrix = Matrix.CreateWorld(cameraPos, Vector3.Forward, Vector3.Up);

            // good for the tests
            int ntroops = AIParams.CONFIG_NUMBER_AIOBJECTS_IN_TEAM;
            int nsnipers = (int)Math.Ceiling((double)ntroops / 9.0);
            int nsupports = (int)Math.Ceiling((double)ntroops / 7.0);
            ntroops = ntroops - nsnipers - nsupports - 1;
            _aicontainer.CreateTeam(AITeamType.Alpha, Color.Red, ntroops, nsnipers, nsupports, terrain.TerrainCenter, true);
            _aicontainer.CreateTeam(AITeamType.Bravo, Color.Blue, ntroops, nsnipers, nsupports, new Vector3(200), true);
            _aicontainer.CreateTeam(AITeamType.Charlie, Color.White, ntroops, nsnipers, nsupports, new Vector3(terrain.TerrainSize - 200), true);

            //aicontainer.CreateTeam(AITeamType.Alpha, Color.Red, 0, 0, 0, terrain.TerrainCenter, false);
            //aicontainer.CreateTeam(AITeamType.Bravo, Color.Blue, 0, 0, 0, new Vector3(200), false);
            //aicontainer.CreateTeam(AITeamType.Charlie, Color.White, 0, 0, 0, new Vector3(terrain.TerrainSize - 200), false);

            _aicontainer.AddMulitpleAIStatics(
                AIParams.CONFIG_NUMBER_COVERS,
                AIParams.CONFIG_NUMBER_MEDS,
                AIParams.CONFIG_NUMBER_BONUSES
                );

            // have to be done after building the teams and adding objects to the world
            _aicontainer.InitializeAI();
            // Load ai objects
            _aicontainer.LoadContent(content);
            _aicontainer.SpreadObjects();

            //aicontainer.SelectTeam(AITeamType.Alpha);
            //aicontainer.SelectedTeam.ToFormation(AIFormation.Row);
            //aicontainer.SelectedTeam.KeepFormation = false;

            //aicontainer.SelectTeam(AITeamType.Bravo);
            //aicontainer.SelectedTeam.ToFormation(AIFormation.Arrow);
            //aicontainer.SelectedTeam.KeepFormation = false;

            //aicontainer.SelectTeam(AITeamType.Charlie);
            //aicontainer.SelectedTeam.ToFormation(AIFormation.Spread);
            //aicontainer.SelectedTeam.KeepFormation = false;

            AIGame.Console.Show();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            this.gameTime = gameTime;

            _aicontainer.UpdateAll(gameTime);

            if (_console != null)
                _console.Update();

            base.Update(gameTime);

            ProcessKeyboard();
            UpdateCamera(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            if (gameTime == null) throw new ArgumentNullException("gameTime");

            // if in accelerated mode, draw only text on a screen
            if (Settings.ACCELERATED_MODE)
            {
                device.Clear(Color.DarkBlue);
                if (HUD != null)
                    HUD.Draw();

                if (_console != null && (_console.State == ScreenOutput.Console.ConsoleState.Opening || _console.State == ScreenOutput.Console.ConsoleState.Opened || _console.State == ScreenOutput.Console.ConsoleState.Closing))
                    _console.Draw();

                return;
            }

            if (!this.IsActive)
                return;

            // TODO: substitute with equivalents

            // NOTE: migration to XNA 4.0
            // NOTE: if (graphics.GraphicsDevice.RenderState.AlphaBlendEnable)
            // NOTE:     graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            // NOTE: if (!graphics.GraphicsDevice.RenderState.DepthBufferEnable)
            // NOTE:     graphics.GraphicsDevice.RenderState.DepthBufferEnable = true;
            // NOTE: if (!graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable)
            // NOTE:     graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            // NOTE: 
            // NOTE: if(Settings.FOG_ENABLED)
            // NOTE:     graphics.GraphicsDevice.RenderState.FogEnable = true;
            // NOTE: else
            // NOTE:     graphics.GraphicsDevice.RenderState.FogEnable = false;

            device.Clear(Settings.ENVIRONMENT_COLOR);

            terrain.Draw(Matrix.Identity, camera.ViewMatrix, camera.ProjectionMatrix);

            // world.DrawWorld(camera.ViewMatrix, camera.ProjectionMatrix);

            _aicontainer.DrawAll(gameTime, camera.ProjectionMatrix, camera.ViewMatrix);

            base.Draw(gameTime);

            if (HUD != null)
                HUD.Draw();

            if (_console != null && (_console.State == ScreenOutput.Console.ConsoleState.Opening || _console.State == ScreenOutput.Console.ConsoleState.Opened || _console.State == ScreenOutput.Console.ConsoleState.Closing))
                _console.Draw();
        }

        #endregion

        #region Handle Input

        public static bool KeyJustPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }
        public static bool LeftButtonJustPressed()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released;
        }
        public static bool RightButtonJustPressed()
        {
            return currentMouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton == ButtonState.Released;
        }

        private void ProcessKeyboard()
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            prevMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            if (KeyJustPressed(Keys.Escape))
                this.Exit();

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt))
                camera.Velocity *= 10.0f;
            if (currentKeyboardState.IsKeyUp(Keys.LeftAlt))
                camera.Velocity = new Vector3(Settings.CAMERA_VELOCITY);

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt) ||
                currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    ToggleFullScreen();
            }

            if (KeyJustPressed(Keys.D1))
            {
                camera.CurrentBehavior = Camera.Behavior.FirstPerson;
                camera.Velocity = new Vector3(Settings.CAMERA_VELOCITY);
                camera.Acceleration = new Vector3(Settings.CAMERA_ACCELERATION);
            }

            if (KeyJustPressed(Keys.D2))
            {
                camera.CurrentBehavior = Camera.Behavior.Spectator;
                camera.Velocity = new Vector3(Settings.CAMERA_VELOCITY) * 1.5f;
                camera.Acceleration = new Vector3(Settings.CAMERA_ACCELERATION) * 2.0f;
            }

            if (KeyJustPressed(Keys.D3))
            {
                camera.CurrentBehavior = Camera.Behavior.FreeView;
                camera.Velocity = new Vector3(Settings.CAMERA_VELOCITY) * 1.5f;
                camera.Acceleration = new Vector3(Settings.CAMERA_ACCELERATION) * 2.0f;
            }

            //if (KeyJustPressed(Keys.D4))
            //{
            //    camera.CurrentBehavior = Camera.Behavior.Orbit;
            //    camera.Velocity = new Vector3(Settings.CAMERA_VELOCITY) * 1.5f;
            //    camera.Acceleration = new Vector3(Settings.CAMERA_ACCELERATION) * 2.0f;
            //}

            if (KeyJustPressed(Keys.D4))
            {
                camera.CurrentBehavior = Camera.Behavior.Flight;
                camera.Velocity = new Vector3(Settings.CAMERA_VELOCITY) * 0.3f;
                camera.Acceleration = new Vector3(Settings.CAMERA_ACCELERATION) * 0.5f;
            }

            // SETTINGS WITH CONTROL
            if (currentKeyboardState.IsKeyDown(Keys.LeftControl))
            {
                if (KeyJustPressed(Keys.Z))
                    AI.AIParams.CONFIG_ENABLE_DEFAULT_LIGHTING = !AI.AIParams.CONFIG_ENABLE_DEFAULT_LIGHTING;

                if (KeyJustPressed(Keys.Space))
                    terrain.ToggleDrawingTechnique();

                if (KeyJustPressed(Keys.C))
                {
                    cursor.IsActivated = !cursor.IsActivated;
                    AIGame.terrain.ShowGroundCursor = !AIGame.terrain.ShowGroundCursor;
                }

                if (KeyJustPressed(Keys.F))
                    Settings.FOG_ENABLED = !Settings.FOG_ENABLED;

                if (KeyJustPressed(Keys.P))
                    camera.PerspectiveInitial();

                if (KeyJustPressed(Keys.B))
                    Settings.DRAW_BOUNDING_BOXES = !Settings.DRAW_BOUNDING_BOXES;

                if (KeyJustPressed(Keys.R))
                    Settings.DRAW_RANGE_CIRCLES = !Settings.DRAW_RANGE_CIRCLES;

                if (KeyJustPressed(Keys.A))
                    Settings.ACCELERATED_MODE = !Settings.ACCELERATED_MODE;

                if (KeyJustPressed(Keys.PageDown))
                {
                    Settings.GAME_SPEED -= 0.1f;
                    if (Settings.GAME_SPEED < 0.2f) Settings.GAME_SPEED = 0.2f;
                }
                if (KeyJustPressed(Keys.PageUp))
                    Settings.GAME_SPEED += 0.1f;
                if (KeyJustPressed(Keys.Home))
                    Settings.GAME_SPEED = 1f;

                if (KeyJustPressed(Keys.L))
                    Settings.DRAW_STATICS_LABELS = !Settings.DRAW_STATICS_LABELS;

                if (KeyJustPressed(Keys.H))
                    Settings.DRAW_GRAPH = !Settings.DRAW_GRAPH;


                if (currentKeyboardState.IsKeyDown(Keys.LeftShift) && KeyJustPressed(Keys.L))
                    Settings.DRAW_OBJECTS_LABELS = !Settings.DRAW_OBJECTS_LABELS;

                if (KeyJustPressed(Keys.G))
                    AIParams.CONFIG_DUMP_ANN_VECTORS = !AIParams.CONFIG_DUMP_ANN_VECTORS;
            }
            // SETTINGS WITHOUT CONTROL
            else
            {
                // Clear selection
                if (KeyJustPressed(Keys.U))
                    _aicontainer.ClearSelect();

                // FOLLOW FITTEST OBJECT
                if (KeyJustPressed(Keys.F) && _aicontainer.GenethicAlgorithm.FittestObject != null)
                {
                    if (camera.ObjectToFollow != null)
                        camera.ObjectToFollow = null;
                    else
                    {
                        camera.ObjectToFollow = _aicontainer.GenethicAlgorithm.FittestObject;
                        camera.Position = camera.ObjectToFollow.Position + Vector3.UnitY * 1000;
                        camera.LookAt(camera.ObjectToFollow.Position);
                        AIContainer.Select(camera.ObjectToFollow);
                    }
                }

                // process teams and objects movements
                if (camera.CurrentBehavior == Camera.Behavior.FreeView && _aicontainer.IsOneOfAIObjectSelected)
                {
                    // toggle objects status: walk, run, crawl, cover
                    if (KeyJustPressed(Keys.Z))
                        _aicontainer.SelectedObject.ToggleMoveStatus();

                    // toggle formation
                    if (KeyJustPressed(Keys.F))
                        _aicontainer.SelectedTeam.ToggleFormation();

                    // toggle oaau
                    if (KeyJustPressed(Keys.O))
                        _aicontainer.SelectedTeam.OperateAsAUnit = !_aicontainer.SelectedTeam.OperateAsAUnit;

                    // toggle kf
                    if (KeyJustPressed(Keys.K))
                        _aicontainer.SelectedTeam.KeepFormation = !_aicontainer.SelectedTeam.KeepFormation;

                    // go to cover
                    if (KeyJustPressed(Keys.C))
                        _aicontainer.SelectedObject.TakeCover();

                    // pick up static
                    if (KeyJustPressed(Keys.B))
                        _aicontainer.SelectedObject.PickUpBonus();

                    // pick up static
                    if (KeyJustPressed(Keys.M))
                        _aicontainer.SelectedObject.PickUpMedKit();
                }
            }
        }

        private void UpdateCamera(GameTime gameTime)
        {
            if (camera.ObjectToFollow != null)
            {
                if (camera.CurrentBehavior == Camera.Behavior.FreeView)
                    camera.Position = camera.ObjectToFollow.Position + Vector3.UnitY * 1000;
                if (camera.CurrentBehavior == Camera.Behavior.FirstPerson)
                {
                    camera.Position = camera.ObjectToFollow.Position - camera.ObjectToFollow.ViewDirection;
                    if (camera.ObjectToFollow.Target != null && camera.ObjectToFollow.IsShooting)
                        camera.LookAt(camera.ObjectToFollow.Target.Position);
                    else
                        camera.LookAt(camera.ObjectToFollow.Position + camera.ObjectToFollow.ViewDirection * 10);
                    camera.UndoRoll();
                }
            }

            Vector3 pos = camera.Position;
            float gridSpacing = terrain.TerrainHeightMap.GridSpacing;
            float size = terrain.TerrainHeightMap.Size * gridSpacing;
            float lowerBounds = 2.0f * gridSpacing;
            float upperBounds = size - (2.0f * gridSpacing);
            float height = terrain.TerrainHeightMap.HeightAt(pos.X, pos.Z) + 50.0f;

            if (pos.X < lowerBounds)
                pos.X = lowerBounds;

            if (pos.X > upperBounds)
                pos.X = upperBounds;

            switch (camera.CurrentBehavior)
            {
                case Camera.Behavior.FirstPerson:
                    pos.Y = height;
                    break;

                case Camera.Behavior.Spectator:
                case Camera.Behavior.Flight:
                    if (pos.Y < height)
                        pos.Y = height;

                    if (pos.Y > terrain.TerrainSize * 0.5f)
                        pos.Y = terrain.TerrainSize * 0.5f;
                    break;

                case Camera.Behavior.FreeView:
                    if (pos.Y < height)
                        pos.Y = height;
                    if (pos.Y > terrain.TerrainSize * 3)
                        pos.Y = terrain.TerrainSize * 3;
                    break;
                default:
                    break;
            }

            if (pos.Z < lowerBounds)
                pos.Z = lowerBounds;

            if (pos.Z > upperBounds)
                pos.Z = upperBounds;

            camera.Position = pos;
        }

        private void ToggleFullScreen()
        {
            int newWidth = 0;
            int newHeight = 0;

            graphics.IsFullScreen = !graphics.IsFullScreen;

            if (graphics.IsFullScreen)
            {
                newWidth = GraphicsDevice.DisplayMode.Width;
                newHeight = GraphicsDevice.DisplayMode.Height;
            }
            else
            {
                newWidth = _windowWidth;
                newHeight = _windowHeight;
            }

            graphics.PreferredBackBufferWidth = newWidth;
            graphics.PreferredBackBufferHeight = newHeight;
            graphics.ApplyChanges();
            Console = new ScreenOutput.Console();
            _console.Show();

        }

        #endregion
    }
}
