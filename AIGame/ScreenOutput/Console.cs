//======================================================================
// XNA Terrain AIGame
// Copyright (C) 2008 Eric Grossinger
// http://psycad007.spaces.live.com/
//======================================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AIGame.ScreenOutput
{
    public class Console
    {
        #region Fields and Properties
        private Vector2 _position = Vector2.Zero;
        private int _width = 0;
        private int _height = 80;
        private float _speed = 0.06f;
        private SpriteFont _font;
        private Color _fontColor = Color.White;
        private SpriteBatch _spriteBatch;
        private Texture2D _background;
        private bool _allowInput = false;
        private Line[] _line;
        public struct Line
        {
            public Line(string strText, Vector2 pos)
            {
                Text = strText;
                Position = pos;
            }
            public string Text;
            public Vector2 Position;
        }
        private List<string> _lineContent;
        private int _scrollPos = 0;
        private string _input = ">";
        private Keys[] _keyList = { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z, Keys.Space };
        private Boolean _keyExists = false;

        public ConsoleState State { get; set; }
        public enum ConsoleState
        {
            Closed,
            Closing,
            Opened,
            Opening
        }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Height
        {
            get { return _height; }
        }
        #endregion

        #region Constructor
        public Console()
        {
            _position.Y = -_height;

            if (_allowInput)
                _line = new Line[3];
            else
                _line = new Line[4];

            for (int i = 0; i < _line.Length; i++)
                _line[i] = new Line("", new Vector2(5f, 5f + (15f * i)));

            _font = AIGame.content.Load<SpriteFont>(@"Fonts\tahoma_small");
            _spriteBatch = new SpriteBatch(AIGame.graphics.GraphicsDevice);
            _width = AIGame.graphics.GraphicsDevice.Viewport.Width;
            _lineContent = new List<string>();
            State = ConsoleState.Closed;
            GenerateBackground();
        }
        #endregion

        #region Private and Public Methods
        private void GenerateBackground()
        {
            // NOTE: migration to XNA 4.0 background = new Texture2D(AIGame.graphics.GraphicsDevice, width, height, 1, TextureUsage.None, SurfaceFormat.Color);
            _background = new Texture2D(AIGame.graphics.GraphicsDevice, _width, _height, false, SurfaceFormat.Color);

            Color[] colors = new Color[_width * _height];
            int bitID = 0;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (y < _height - 1)
                        colors[x + y * _width] = new Color(new Vector4((float)x / _width, (float)y / _height, 0.1f, 0.5f));
                    else
                        colors[x + y * _width] = new Color(new Vector4((float)x / _width, (float)y - _height - 2 / _height, 0.4f, 0.5f));
                    bitID++;
                }
            }

            _background.SetData<Color>(colors);
        }

        public void Clear()
        {
            _lineContent.Clear();
        }

        public void Add(string text)
        {
            _lineContent.Add(text);
            ScrollDown(1);

            if (_lineContent.Count > 100)
                _lineContent.RemoveAt(0);

            //Console.WriteLine(text);
        }

        public void Update()
        {
            if (State == ConsoleState.Opening)
                Open();
            else if (State == ConsoleState.Closing)
                Close();

            CheckPCInput();
        }

        private void Open()
        {
            if (_position.Y + _height * _speed < 0f)
                _position.Y += _height * _speed;
            else
            {
                _position.Y = 0f;
                State = ConsoleState.Opened;
            }
        }

        private void Close()
        {
            if (_position.Y - _height * _speed > -_height)
                _position.Y -= _height * _speed;
            else
            {
                _position.Y = -_height;
                State = ConsoleState.Closed;
            }
        }

        bool bTabDown = false;
        bool bUpKey = false;
        bool bDownKey = false;
        bool bPgUp = false;
        bool bPgDn = false;
        bool bBackspace = false;
        bool bTyping = false;
        public void CheckPCInput()
        {
            KeyboardState ks = Keyboard.GetState();

            //if (AIGame.settings == null || !AIGame.settings.ContainsFocus)
            //{
            if (ks.IsKeyDown(Keys.Tab))
            {
                if (!bTabDown)
                {
                    bTabDown = true;
                    Toggle();
                }
            }
            else if (bTabDown)
                bTabDown = false;
            //}

            if (State == ConsoleState.Opened)
            {
                if (_allowInput)
                {
                    Keys[] currentKeys = ks.GetPressedKeys();

                    if (currentKeys.Length > 0)
                    {
                        _keyExists = false;
                        foreach (var k in _keyList)
                        {
                            if (k == currentKeys[0])
                                _keyExists = true;
                        }
                        //_keyExists = Array.Exists(_keyList, new Predicate<Keys>(delegate(Keys currentKey) { return currentKey == currentKeys[0]; }));
                    }
                    else
                        _keyExists = false;

                    if (_keyExists)
                    {
                        if (!bTyping)
                        {
                            bTyping = true;
                            _input += currentKeys[0].ToString();
                        }
                    }
                    else if (bTyping)
                        bTyping = false;

                    if (ks.IsKeyDown(Keys.Back))
                    {
                        if (!bBackspace)
                        {
                            bBackspace = true;
                            if (_input.Length > 0)
                                _input.Remove(_input.Length - 1, 1);
                        }
                    }
                    else if (bBackspace)
                        bBackspace = false;
                }

                if (ks.IsKeyDown(Keys.Up))
                {
                    if (!bUpKey)
                    {
                        bUpKey = true;
                        ScrollUp(1);
                    }
                }
                else if (bUpKey)
                    bUpKey = false;

                if (ks.IsKeyDown(Keys.Down))
                {
                    if (!bDownKey)
                    {
                        bDownKey = true;
                        ScrollDown(1);
                    }
                }
                else if (bDownKey)
                    bDownKey = false;

                if (ks.IsKeyDown(Keys.PageUp))
                {
                    if (!bPgUp)
                    {
                        bPgUp = true;
                        ScrollUp(_line.Length - 1);
                    }
                }
                else if (bPgUp)
                    bPgUp = false;

                if (ks.IsKeyDown(Keys.PageDown))
                {
                    if (!bPgDn)
                    {
                        bPgDn = true;
                        ScrollDown(_line.Length - 1);
                    }
                }
                else if (bPgDn)
                    bPgDn = false;
            }
        }

        public void Draw()
        {
            // NOTE: migration to XNA 4.0 spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            _spriteBatch.Draw(_background, _position, Color.White);
            DrawLines();
            _spriteBatch.End();
        }

        private void DrawLines()
        {
            for (int i = 0; i < _line.Length; i++)
                _spriteBatch.DrawString(_font, _line[i].Text, new Vector2(5f, 5f + (15f * i)) + _position, _fontColor);

            if (_allowInput)
                _spriteBatch.DrawString(_font, _input, new Vector2(5f, 80f) + _position, _fontColor);
        }

        private void Toggle()
        {
            if (State == ConsoleState.Opened || State == ConsoleState.Opening)
                Hide();
            else if (State == ConsoleState.Closed || State == ConsoleState.Closing)
                Show();
        }

        public void Show()
        {
            State = ConsoleState.Opening;
        }

        public void Hide()
        {
            State = ConsoleState.Closing;
        }

        private void ScrollUp(int amount)
        {
            if (_lineContent.Count > _line.Length)
            {
                if (_scrollPos - amount > 0)
                    _scrollPos -= amount;
                else
                    _scrollPos = 0;
            }

            SetVisibleText();
        }

        private void ScrollDown(int amount)
        {
            if (_lineContent.Count > _line.Length)
            {
                if (_scrollPos + amount < _lineContent.Count - _line.Length)
                    _scrollPos += amount;
                else
                    _scrollPos = _lineContent.Count - _line.Length;
            }

            SetVisibleText();
        }

        private void SetVisibleText()
        {
            if (_lineContent.Count > 0)
            {
                for (int i = 0; i < _line.Length; i++)
                {
                    if (_scrollPos + i < _lineContent.Count)
                    {
                        if (_lineContent[_scrollPos + i] != null)
                            _line[i].Text = _lineContent[_scrollPos + i];
                    }
                }
            }
        }
        #endregion
    }
}
