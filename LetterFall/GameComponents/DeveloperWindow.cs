using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace LetterFall.GameComponents
{
    /// <summary>
    /// A simple developer window for adjusting game parameters
    /// </summary>
    public class DeveloperWindow
    {
        // Window state
        private bool _isVisible = false;
        private Rectangle _windowBounds;
        private SpriteFont _font;
        
        // Game parameters that can be adjusted
        private Dictionary<string, float> _numericParameters;
        private Dictionary<string, bool> _booleanParameters;
        
        // Input tracking
        private KeyboardState _previousKeyboardState;
        private int _selectedParameterIndex = 0;
        
        /// <summary>
        /// Creates a new developer window
        /// </summary>
        /// <param name="bounds">Rectangle representing the window's position and size</param>
        /// <param name="font">Font to use for text</param>
        public DeveloperWindow(Rectangle bounds, SpriteFont font)
        {
            _windowBounds = bounds;
            _font = font;
            
            // Initialize parameters with default values
            _numericParameters = new Dictionary<string, float>
            {
                { "Word Score Multiplier", 1.0f },
                { "Auto Clear Delay", 0.5f },
                { "Base Score 3-letter", 30f },
                { "Base Score 4-letter", 60f },
                { "Base Score 5-letter", 100f },
                { "Cascade Bonus", 0.2f }
            };
            
            _booleanParameters = new Dictionary<string, bool>
            {
                { "Auto Clear Enabled", true },
                { "Show Debug Info", true },
                { "Use Letter Frequency", true }
            };
        }
        
        /// <summary>
        /// Toggles the visibility of the developer window
        /// </summary>
        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
        }
        
        /// <summary>
        /// Updates the developer window
        /// </summary>
        /// <param name="gameTime">Current game time</param>
        public void Update(GameTime gameTime)
        {
            if (!_isVisible)
                return;
                
            KeyboardState keyboardState = Keyboard.GetState();
            
            // Navigate parameters
            if (keyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up))
            {
                _selectedParameterIndex = Math.Max(0, _selectedParameterIndex - 1);
            }
            
            if (keyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down))
            {
                int totalParams = _numericParameters.Count + _booleanParameters.Count;
                _selectedParameterIndex = Math.Min(totalParams - 1, _selectedParameterIndex + 1);
            }
            
            // Adjust selected parameter
            if (_selectedParameterIndex < _numericParameters.Count)
            {
                // Numeric parameter
                string key = new List<string>(_numericParameters.Keys)[_selectedParameterIndex];
                
                if (keyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left))
                {
                    _numericParameters[key] = Math.Max(0, _numericParameters[key] - 0.1f);
                }
                
                if (keyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right))
                {
                    _numericParameters[key] += 0.1f;
                }
            }
            else
            {
                // Boolean parameter
                int boolIndex = _selectedParameterIndex - _numericParameters.Count;
                string key = new List<string>(_booleanParameters.Keys)[boolIndex];
                
                if ((keyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left)) ||
                    (keyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right)))
                {
                    _booleanParameters[key] = !_booleanParameters[key];
                }
            }
            
            _previousKeyboardState = keyboardState;
        }
        
        /// <summary>
        /// Draws the developer window
        /// </summary>
        /// <param name="spriteBatch">Sprite batch for drawing</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isVisible)
                return;
                
            // Draw window background
            Texture2D pixel = GetOrCreatePixelTexture(spriteBatch.GraphicsDevice);
            spriteBatch.Draw(pixel, _windowBounds, new Color(30, 30, 30, 200));
            
            // Draw window title
            string title = "Developer Window (F1 to toggle)";
            Vector2 titlePos = new Vector2(_windowBounds.X + 10, _windowBounds.Y + 10);
            spriteBatch.DrawString(_font, title, titlePos, Color.White);
            
            // Draw parameters
            float yPos = _windowBounds.Y + 40;
            int index = 0;
            
            // Draw numeric parameters
            foreach (var param in _numericParameters)
            {
                bool isSelected = index == _selectedParameterIndex;
                Color textColor = isSelected ? Color.Yellow : Color.White;
                
                string text = $"{param.Key}: {param.Value:0.0}";
                Vector2 pos = new Vector2(_windowBounds.X + 20, yPos);
                
                spriteBatch.DrawString(_font, text, pos, textColor);
                
                yPos += 30;
                index++;
            }
            
            // Draw boolean parameters
            foreach (var param in _booleanParameters)
            {
                bool isSelected = index == _selectedParameterIndex;
                Color textColor = isSelected ? Color.Yellow : Color.White;
                
                string text = $"{param.Key}: {(param.Value ? "On" : "Off")}";
                Vector2 pos = new Vector2(_windowBounds.X + 20, yPos);
                
                spriteBatch.DrawString(_font, text, pos, textColor);
                
                yPos += 30;
                index++;
            }
            
            // Draw instructions
            string instructions = "Up/Down: Select parameter\nLeft/Right: Adjust value";
            Vector2 instrPos = new Vector2(_windowBounds.X + 10, _windowBounds.Bottom - 60);
            spriteBatch.DrawString(_font, instructions, instrPos, Color.LightGray);
        }
        
        /// <summary>
        /// Gets a numeric parameter value
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <returns>Parameter value</returns>
        public float GetNumericParameter(string paramName)
        {
            if (_numericParameters.ContainsKey(paramName))
                return _numericParameters[paramName];
                
            return 0f;
        }
        
        /// <summary>
        /// Gets a boolean parameter value
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <returns>Parameter value</returns>
        public bool GetBooleanParameter(string paramName)
        {
            if (_booleanParameters.ContainsKey(paramName))
                return _booleanParameters[paramName];
                
            return false;
        }
        
        /// <summary>
        /// Sets a numeric parameter value
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="value">New value</param>
        public void SetNumericParameter(string paramName, float value)
        {
            if (_numericParameters.ContainsKey(paramName))
                _numericParameters[paramName] = value;
        }
        
        /// <summary>
        /// Sets a boolean parameter value
        /// </summary>
        /// <param name="paramName">Parameter name</param>
        /// <param name="value">New value</param>
        public void SetBooleanParameter(string paramName, bool value)
        {
            if (_booleanParameters.ContainsKey(paramName))
                _booleanParameters[paramName] = value;
        }
        
        /// <summary>
        /// Gets or creates a 1x1 white pixel texture for drawing shapes
        /// </summary>
        private Texture2D _pixelTexture;
        private Texture2D GetOrCreatePixelTexture(GraphicsDevice graphicsDevice)
        {
            if (_pixelTexture == null)
            {
                _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
            
            return _pixelTexture;
        }
    }
}