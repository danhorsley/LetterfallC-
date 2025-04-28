using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using LetterFall.Models;
using LetterFall.GameComponents.Input;
using LetterFall.GameComponents.Words;
using System.IO;

namespace LetterFall
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        // Game components
        private LetterGrid _grid;
        private InputHandler _inputHandler;
        private WordDetector _wordDetector;
        
        // UI elements
        private SpriteFont _font;
        private Rectangle _gridArea;
        private float _cellSize;
        
        // Game state
        private int _score;
        private List<DetectedWord> _currentWords;
        private KeyboardState _previousKeyboardState;
        
        // Colors
        private Color _backgroundColor = new Color(50, 50, 60);
        private Color _gridColor = new Color(70, 70, 80);
        private Color _textColor = Color.White;
        private Color _highlightColor = new Color(100, 200, 100, 180);
        private Color _selectedRowColColor = new Color(80, 100, 120);

        //Combos
        private int _comboMultiplier = 1;
        private int _comboChainLength = 0;
        private bool _cascadeInProgress = false;
        private float _autoClearDelay = 0.5f; // Half a second delay
        private float _autoClearTimer = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // Increase window size to accommodate extended strips
            _graphics.PreferredBackBufferWidth = 1024;  // Increased from 800
            _graphics.PreferredBackBufferHeight = 900;  // Increased from 600
        }

        protected override void Initialize()
        {
        // Calculate grid area
        int gridSize = 400;
        int centerX = _graphics.PreferredBackBufferWidth / 2;
        int centerY = _graphics.PreferredBackBufferHeight / 2;
        _gridArea = new Rectangle(centerX - gridSize/2, centerY - gridSize/2, gridSize, gridSize);
        _cellSize = gridSize / 5f; // 5x5 grid
        
        // Create game components
        _grid = new LetterGrid();
        _inputHandler = new InputHandler(_grid, _gridArea);
        _wordDetector = new WordDetector(_grid,4,5);
        
        // Load external dictionary
        string wordListPath = "Content/wordlist.txt";
        if (File.Exists(wordListPath))
        {
            _wordDetector.LoadDictionary(wordListPath);
            System.Diagnostics.Debug.WriteLine("Loaded external word list");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Word list file not found: " + wordListPath);
        }
        
        _score = 0;
        _currentWords = new List<DetectedWord>();
        
        base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Load the font
            _font = Content.Load<SpriteFont>("Arial");
            
            // If the font doesn't exist yet, create this in the Content Pipeline
            // You'll need to add a SpriteFont file to your Content project
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit on Escape
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();
                
            // Handle debug commands
            if (keyboardState.IsKeyDown(Keys.R) && _previousKeyboardState.IsKeyUp(Keys.R))
                _grid.RandomizeLetters(); // Randomize grid on R key
            // In Update method of Game1.cs, add these before the existing keyboard checks:
            // Debug controls
            if (keyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left))
                _grid.ShiftRow(2, 1); // Shift middle row left
                
            if (keyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right))
                _grid.ShiftRow(2, -1); // Shift middle row right
                
            if (keyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up))
                _grid.ShiftColumn(2, 1); // Shift middle column up
                
            if (keyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down))
                _grid.ShiftColumn(2, -1); // Shift middle column down
            // Update input
            _inputHandler.Update(gameTime);
            
            // If not dragging, find words
            if (!_inputHandler.IsDragging)
            {
                _currentWords = _wordDetector.FindWords();
                _wordDetector.HighlightWords(_currentWords);
                
                // Clear with spacebar
                if (keyboardState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    ClearWords();
                }
            }
             if (_cascadeInProgress && _currentWords.Count > 0)
            {
                _autoClearTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                if (_autoClearTimer <= 0)
                {
                    // Automatically clear words when the timer expires
                    ClearWords();
                    
                    // Reset the timer for the next potential cascade
                    _autoClearTimer = _autoClearDelay;
                }
            }
            _previousKeyboardState = keyboardState;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_backgroundColor);
            
            _spriteBatch.Begin();
            
            // Draw grid background
            _spriteBatch.Draw(GetOrCreatePixelTexture(), _gridArea, _gridColor);
            
            // Draw current row/column highlight if dragging
            if (_inputHandler.IsDragging)
            {
                if (_inputHandler.CurrentDragDirection == InputHandler.DragDirection.Horizontal && _inputHandler.SelectedRow >= 0)
                {
                    // Highlight the selected row
                    Rectangle rowRect = new Rectangle(
                        _gridArea.X, 
                        _gridArea.Y + (int)(_inputHandler.SelectedRow * _cellSize),
                        _gridArea.Width,
                        (int)_cellSize);
                        
                    _spriteBatch.Draw(GetOrCreatePixelTexture(), rowRect, _selectedRowColColor);
                }
                else if (_inputHandler.CurrentDragDirection == InputHandler.DragDirection.Vertical && _inputHandler.SelectedColumn >= 0)
                {
                    // Highlight the selected column
                    Rectangle colRect = new Rectangle(
                        _gridArea.X + (int)(_inputHandler.SelectedColumn * _cellSize),
                        _gridArea.Y,
                        (int)_cellSize,
                        _gridArea.Height);
                        
                    _spriteBatch.Draw(GetOrCreatePixelTexture(), colRect, _selectedRowColColor);
                }
            }
            
            // Draw word highlights
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (_grid.IsSelected(row, col))
                    {
                        Rectangle cellRect = new Rectangle(
                            _gridArea.X + (int)(col * _cellSize),
                            _gridArea.Y + (int)(row * _cellSize),
                            (int)_cellSize,
                            (int)_cellSize);
                            
                        _spriteBatch.Draw(GetOrCreatePixelTexture(), cellRect, _highlightColor);
                    }
                }
            }
            
            // Draw grid lines
            for (int i = 0; i <= 5; i++)
            {
                // Horizontal lines
                _spriteBatch.Draw(
                    GetOrCreatePixelTexture(),
                    new Rectangle(_gridArea.X, _gridArea.Y + (int)(i * _cellSize), _gridArea.Width, 1),
                    Color.Black);
                    
                // Vertical lines
                _spriteBatch.Draw(
                    GetOrCreatePixelTexture(),
                    new Rectangle(_gridArea.X + (int)(i * _cellSize), _gridArea.Y, 1, _gridArea.Height),
                    Color.Black);
            }
            
            // Draw letters
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    char letter = _grid.GetLetter(row, col);
                    Vector2 textSize = _font.MeasureString(letter.ToString());
                    
                    // Center the letter in the cell
                    Vector2 position = new Vector2(
                        _gridArea.X + (col * _cellSize) + (_cellSize / 2) - (textSize.X / 2),
                        _gridArea.Y + (row * _cellSize) + (_cellSize / 2) - (textSize.Y / 2)
                    );
                    
                    _spriteBatch.DrawString(_font, letter.ToString(), position, _textColor);
                }
            }
            // After drawing the grid letters, add code to draw extended strips
            // First, draw the extended horizontal strips
            for (int row = 0; row < 5; row++)
            {
                // Get extended row letters (3 letters on each side)
                char[] extendedLetters = _grid.GetExtendedRowLetters(row, 3);
                
                // Draw letters to the left of the grid
                for (int i = 0; i < 3; i++)
                {
                    char letter = extendedLetters[i];
                    Vector2 textSize = _font.MeasureString(letter.ToString());
                    
                    // Position the letter to the left of the grid
                    Vector2 position = new Vector2(
                        _gridArea.X - (3 - i) * _cellSize + (_cellSize / 2) - (textSize.X / 2),
                        _gridArea.Y + (row * _cellSize) + (_cellSize / 2) - (textSize.Y / 2)
                    );
                    
                    // Draw with slightly reduced alpha
                    _spriteBatch.DrawString(_font, letter.ToString(), position, _textColor * 0.5f);
                }
                
                // Draw letters to the right of the grid
                for (int i = 0; i < 3; i++)
                {
                    char letter = extendedLetters[i + 5 + 3]; // Skip the 5 visible letters and the 3 left letters
                    Vector2 textSize = _font.MeasureString(letter.ToString());
                    
                    // Position the letter to the right of the grid
                    Vector2 position = new Vector2(
                        _gridArea.X + _gridArea.Width + (i * _cellSize) + (_cellSize / 2) - (textSize.X / 2),
                        _gridArea.Y + (row * _cellSize) + (_cellSize / 2) - (textSize.Y / 2)
                    );
                    
                    // Draw with slightly reduced alpha
                    _spriteBatch.DrawString(_font, letter.ToString(), position, _textColor * 0.5f);
                }
            }

            // Now, draw the extended vertical strips
            for (int col = 0; col < 5; col++)
            {
                // Get extended column letters (3 letters on each side)
                char[] extendedLetters = _grid.GetExtendedColumnLetters(col, 3);
                
                // Draw letters above the grid
                for (int i = 0; i < 3; i++)
                {
                    char letter = extendedLetters[i];
                    Vector2 textSize = _font.MeasureString(letter.ToString());
                    
                    // Position the letter above the grid
                    Vector2 position = new Vector2(
                        _gridArea.X + (col * _cellSize) + (_cellSize / 2) - (textSize.X / 2),
                        _gridArea.Y - (3 - i) * _cellSize + (_cellSize / 2) - (textSize.Y / 2)
                    );
                    
                    // Draw with slightly reduced alpha
                    _spriteBatch.DrawString(_font, letter.ToString(), position, _textColor * 0.5f);
                }
                
                // Draw letters below the grid
                for (int i = 0; i < 3; i++)
                {
                    char letter = extendedLetters[i + 5 + 3]; // Skip the 5 visible letters and the 3 top letters
                    Vector2 textSize = _font.MeasureString(letter.ToString());
                    
                    // Position the letter below the grid
                    Vector2 position = new Vector2(
                        _gridArea.X + (col * _cellSize) + (_cellSize / 2) - (textSize.X / 2),
                        _gridArea.Y + _gridArea.Height + (i * _cellSize) + (_cellSize / 2) - (textSize.Y / 2)
                    );
                    
                    // Draw with slightly reduced alpha
                    _spriteBatch.DrawString(_font, letter.ToString(), position, _textColor * 0.5f);
                }
            }
            // Draw score and words
            _spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(20, 20), _textColor);
            // draw combos
            if (_comboMultiplier > 1)
            {
                string comboText = $"COMBO x{_comboMultiplier} (Chain: {_comboChainLength})";
                Vector2 comboPosition = new Vector2(20, 90);
                _spriteBatch.DrawString(_font, comboText, comboPosition, Color.Yellow);
            }
            // Draw found words
            Vector2 wordPosition = new Vector2(20, 60);
            foreach (DetectedWord word in _currentWords)
            {
                _spriteBatch.DrawString(_font, word.Word, wordPosition, _textColor);
                wordPosition.Y += 30;
            }
            
            // Draw instructions
            string instructions = "Drag rows/columns to move letters\nPress SPACE to clear words\nPress R to randomize grid\nPress ESC to exit";
            Vector2 instructionPos = new Vector2(20, _graphics.PreferredBackBufferHeight - 100);
            _spriteBatch.DrawString(_font, instructions, instructionPos, _textColor * 0.7f);

            // Add the debug info here
            string debugInfo = $"Selected Row: {_inputHandler.SelectedRow}, Column: {_inputHandler.SelectedColumn}\n" + 
                            $"Drag Direction: {_inputHandler.CurrentDragDirection}";
            Vector2 debugPos = new Vector2(20, _graphics.PreferredBackBufferHeight - 140);
            _spriteBatch.DrawString(_font, debugInfo, debugPos, Color.Yellow);

            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Clears found words, calculates score, and applies gravity
        /// </summary>
        private void ClearWords()
        {
            if (_currentWords.Count == 0)
                return;
                
            // Count the initial number of words as a "base combo"
            int initialWordCount = _currentWords.Count;
            
            if (initialWordCount > 1)
            {
                // Start a new combo if we cleared multiple words at once
                _comboMultiplier = initialWordCount;
                _comboChainLength = 1;
                _cascadeInProgress = true;
            }
            else if (!_cascadeInProgress)
            {
                // Reset combo if this isn't part of a cascade and we only found one word
                _comboMultiplier = 1;
                _comboChainLength = 0;
            }
            else
            {
                // We're in a cascade, so increment the chain length
                _comboChainLength++;
            }
            
            // Add score for each word
            foreach (DetectedWord word in _currentWords)
            {
                // Score based on word length: 3 letters = 30 pts, 4 letters = 60 pts, 5 letters = 100 pts
                int wordScore = word.Length * word.Length * 10; // Quadratic scoring for longer words
                
                // Apply the combo multiplier
                wordScore *= _comboMultiplier;
                
                _score += wordScore;
            }
            
            // Remove the selected letters and apply gravity
            int lettersRemoved = _grid.RemoveSelectedLetters();
            
            // Find any new words that may have formed after applying gravity
            _currentWords = _wordDetector.FindWords();
            
            // If new words were formed, highlight them (but don't clear yet)
            if (_currentWords.Count > 0)
            {
                _wordDetector.HighlightWords(_currentWords);
                
                // If we're in a cascade, automatically clear words after a short delay
                if (_cascadeInProgress)
                {
                    // We'll need to add a timer system to handle this delay
                    _autoClearTimer = _autoClearDelay;
                }
            }
            else
            {
                // No more words to clear, end the cascade
                _cascadeInProgress = false;
            }
        }
        
        /// <summary>
        /// Gets or creates a 1x1 white pixel texture for drawing shapes
        /// </summary>
        private Texture2D _pixelTexture;
        private Texture2D GetOrCreatePixelTexture()
        {
            if (_pixelTexture == null)
            {
                _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }
            
            return _pixelTexture;
        }
    }
}