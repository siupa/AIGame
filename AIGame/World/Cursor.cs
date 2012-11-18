using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AIGame
{
    /// <summary>
    /// Cursor is a DrawableGameComponent that draws a cursor on the screen. It works
    /// differently on Xbox and Windows. On windows, this will be a cursor that is
    /// controlled using both the mouse and the gamepad. On Xbox, the cursor will be
    /// controlled using only the gamepad.
    /// </summary>
    public class Cursor : DrawableGameComponent
    {
        #region Constants

        const float CursorSpeed = 500.0f;

        #endregion

        #region Fields

        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private Texture2D _cursorTexture;
        private Vector2 _textureCenter;
        private Vector2 _position;
        private bool _show = false;
        private bool _activate = false;

        #endregion

        #region Properties
        public Vector2 Position
        {
            get { return _position; }
        }

        public bool IsActivated
        {
            get { return _activate; }
            set { _activate = value; }
        } 
        #endregion

        #region Methods
        public void Show()
        {
            if (_activate)
                this._show = true;
            else
                Game.IsMouseVisible = true;
        }

        public void Hide()
        {
            this._show = false;
            Game.IsMouseVisible = false;
        } 
        #endregion

        #region Creation and initialization

        public Cursor(Game game, GraphicsDevice graphics, SpriteBatch spriteBatch)
            : base(game) 
        {
            _activate = true;
            _show = true;
            _graphicsDevice = graphics;
            _spriteBatch = spriteBatch;
        }

        // LoadContent needs to load the cursor texture and find its center.
        // also, we need to create a SpriteBatch.
        protected override void LoadContent()
        {
            _cursorTexture = AIGame.content.Load<Texture2D>("Textures/cursor_dark");
            _textureCenter = new Vector2(
                _cursorTexture.Width / 2, _cursorTexture.Height / 2);

            base.LoadContent();
        }

        #endregion

        #region Draw

        // Draw is pretty straightforward: we'll Begin the SpriteBatch, Draw the cursor,
        // and then End.
        public override void Draw(GameTime gameTime)
        {
            if (!_show || !_activate)
                return;
            
            // NOTE: migration to XNA 4.0 
            // NOTE: _spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            // NOTE: _spriteBatch.GraphicsDevice.RenderState.FillMode = FillMode.Solid;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // use textureCenter as the origin of the sprite, so that the cursor is 
            // drawn centered around Position.
            _spriteBatch.Draw(_cursorTexture, Position, null, Color.White, 0.0f,
                _textureCenter, 1.0f, SpriteEffects.None, 0.0f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime)
        {
            if (!_show || !_activate)
                return;

            GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            // we'll create a vector2, called delta, which will store how much the
            // cursor position should change.
            Vector2 delta = currentState.ThumbSticks.Left;

            // down on the thumbstick is -1. however, in screen coordinates, values
            // increase as they go down the screen. so, we have to flip the sign of the
            // y component of delta.
            delta.Y *= -1;

            // normalize delta so that we know the cursor can't move faster than
            // CursorSpeed.
            if (delta != Vector2.Zero)
            {
                delta.Normalize();
            }

            MouseState mouseState = Mouse.GetState();
            _position.X = mouseState.X;
            _position.Y = mouseState.Y;

            if (Game.IsActive)
            {
                Viewport vp = _graphicsDevice.Viewport;
                if ((vp.X <= _position.X) && (_position.X <= (vp.X + vp.Width)) &&
                    (vp.Y <= _position.Y) && (_position.Y <= (vp.Y + vp.Height)))
                {
                    _position += delta * CursorSpeed *
                        (float)gameTime.ElapsedGameTime.TotalSeconds;
                    _position.X = MathHelper.Clamp(_position.X, vp.X, vp.X + vp.Width);
                    _position.Y = MathHelper.Clamp(_position.Y, vp.Y, vp.Y + vp.Height);
                }
                else if (delta.LengthSquared() > 0f)
                {
                    _position.X = vp.X + vp.Width / 2;
                    _position.Y = vp.Y + vp.Height / 2;
                }

                Mouse.SetPosition((int)_position.X, (int)_position.Y);
            }
            base.Update(gameTime);
        }

        #endregion
    }
}
