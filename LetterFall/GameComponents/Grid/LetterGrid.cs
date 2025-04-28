using System;
using System.Collections.Generic;

namespace LetterFall.Models
{
    /// <summary>
    /// Manages the 5x5 grid of letters and the interactions between strips
    /// </summary>
    public class LetterGrid
    {
        // Size of the grid (both width and height)
        private readonly int _gridSize;
        
        // Size of each strip (total number of letters)
        private readonly int _stripSize;
        
        // Horizontal strips (rows)
        private LetterStrip[] _horizontalStrips;
        
        // Vertical strips (columns)
        private LetterStrip[] _verticalStrips;
        
        // Matrix to keep track of which letters are currently selected
        private bool[,] _selectedLetters;

        /// <summary>
        /// Creates a new letter grid
        /// </summary>
        /// <param name="gridSize">Size of the grid (width/height)</param>
        /// <param name="stripSize">Size of each strip</param>
        public LetterGrid(int gridSize = 5, int stripSize = 10)
        {
            _gridSize = gridSize;
            _stripSize = stripSize;
            
            // Initialize strips
            _horizontalStrips = new LetterStrip[gridSize];
            _verticalStrips = new LetterStrip[gridSize];
            
            for (int i = 0; i < gridSize; i++)
            {
                _horizontalStrips[i] = new LetterStrip(stripSize, gridSize);
                _verticalStrips[i] = new LetterStrip(stripSize, gridSize);
            }
            
            _selectedLetters = new bool[gridSize, gridSize];
            
            // Initialize with random letters
            RandomizeLetters();
        }
        
        /// <summary>
        /// Randomizes all letters in the grid
        /// </summary>
        public void RandomizeLetters()
        {
            Random random = new Random();
            
            // Common English letter frequencies for more natural word creation
            string commonLetters = "EEEEEEEEEEAAAAAAAARRRRRRRIIIIIIIOOOOOOOTTTTTTTNNNNNNNSSSSSSLLLLLCCCCUUUUDDDPPPMMM";
            
            // Fill horizontal strips with random letters
            for (int i = 0; i < _gridSize; i++)
            {
                char[] letters = new char[_stripSize];
                for (int j = 0; j < _stripSize; j++)
                {
                    // Get a random common letter
                    letters[j] = commonLetters[random.Next(commonLetters.Length)];
                }
                _horizontalStrips[i].SetLetters(letters);
            }
            
            // Synchronize vertical strips based on current intersections
            SynchronizeVerticalStrips();
        }
        
        /// <summary>
/// Updates vertical strips based on the current state of horizontal strips
/// </summary>
        private void SynchronizeVerticalStrips()
        {
            // For each vertical strip
            for (int col = 0; col < _gridSize; col++)
            {
                char[] verticalLetters = new char[_stripSize];
                
                // For each position in the visible part of the vertical strip
                for (int row = 0; row < _gridSize; row++)
                {
                    // Get the letter from the horizontal strip at this intersection
                    char letter = _horizontalStrips[row].GetVisibleLetter(col);
                    
                    // Calculate where in the vertical strip this position maps to
                    int verticalPos = (_verticalStrips[col].Offset + row) % _stripSize;
                    
                    // Update the vertical strip
                    verticalLetters[verticalPos] = letter;
                }
                
                // Fill the rest of the vertical strip with random letters
                Random random = new Random();
                string commonLetters = "EEEEEEEEEEAAAAAAAARRRRRRRIIIIIIIOOOOOOOTTTTTTTNNNNNNNSSSSSSLLLLLCCCCUUUUDDDPPPMMM";
                
                for (int i = 0; i < _stripSize; i++)
                {
                    // Skip positions that were already set from the horizontal strips
                    bool isSet = false;
                    for (int row = 0; row < _gridSize; row++)
                    {
                        int verticalPos = (_verticalStrips[col].Offset + row) % _stripSize;
                        if (verticalPos == i)
                        {
                            isSet = true;
                            break;
                        }
                    }
                    
                    // If this position wasn't set from horizontal strips, set a random letter
                    if (!isSet)
                    {
                        verticalLetters[i] = commonLetters[random.Next(commonLetters.Length)];
                    }
                }
                
                // Apply the updated letters to the vertical strip
                _verticalStrips[col].SetLetters(verticalLetters);
            }
        }
        
        /// <summary>
        /// Updates horizontal strips based on the current state of vertical strips
        /// </summary>
        private void SynchronizeHorizontalStrips()
        {
            // For each horizontal strip
            for (int row = 0; row < _gridSize; row++)
            {
                char[] horizontalLetters = _horizontalStrips[row].GetAllLetters();
                
                // For each position in the visible part of the horizontal strip
                for (int col = 0; col < _gridSize; col++)
                {
                    // Get the letter from the vertical strip at this intersection
                    char letter = _verticalStrips[col].GetVisibleLetter(row);
                    
                    // Calculate where in the horizontal strip this position maps to
                    int horizontalPos = (_horizontalStrips[row].Offset + col) % _stripSize;
                    
                    // Update the horizontal strip
                    horizontalLetters[horizontalPos] = letter;
                }
                
                // Apply the updated letters to the horizontal strip
                _horizontalStrips[row].SetLetters(horizontalLetters);
            }
        }
        
        /// <summary>
        /// Shifts a horizontal strip (row) by a number of positions
        /// </summary>
        /// <param name="rowIndex">Index of the row to shift</param>
        /// <param name="positions">Positions to shift (positive = right, negative = left)</param>
        public void ShiftRow(int rowIndex, int positions)
        {
            if (rowIndex < 0 || rowIndex >= _gridSize)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
                
            // Shift the horizontal strip
            _horizontalStrips[rowIndex].Shift(positions);
            
            // Update vertical strips to match the new horizontal strip positions
            SynchronizeVerticalStrips();
            
            // Clear selections
            ClearSelections();
        }
        
        /// <summary>
        /// Shifts a vertical strip (column) by a number of positions
        /// </summary>
        /// <param name="colIndex">Index of the column to shift</param>
        /// <param name="positions">Positions to shift (positive = down, negative = up)</param>
        public void ShiftColumn(int colIndex, int positions)
        {
            if (colIndex < 0 || colIndex >= _gridSize)
                throw new ArgumentOutOfRangeException(nameof(colIndex));
                
            // Shift the vertical strip
            _verticalStrips[colIndex].Shift(positions);
            
            // Update horizontal strips to match the new vertical strip positions
            SynchronizeHorizontalStrips();
            
            // Clear selections
            ClearSelections();
        }
        
        /// <summary>
        /// Gets the letter at a specific position in the grid
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="col">Column index</param>
        /// <returns>The letter at the specified position</returns>
        public char GetLetter(int row, int col)
        {
            if (row < 0 || row >= _gridSize || col < 0 || col >= _gridSize)
                throw new ArgumentOutOfRangeException();
                
            return _horizontalStrips[row].GetVisibleLetter(col);
        }
        
        /// <summary>
        /// Gets whether a specific position is currently selected
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="col">Column index</param>
        /// <returns>True if the position is selected</returns>
        public bool IsSelected(int row, int col)
        {
            if (row < 0 || row >= _gridSize || col < 0 || col >= _gridSize)
                throw new ArgumentOutOfRangeException();
                
            return _selectedLetters[row, col];
        }
        
        /// <summary>
        /// Sets whether a specific position is selected
        /// </summary>
        /// <param name="row">Row index</param>
        /// <param name="col">Column index</param>
        /// <param name="selected">Selection state</param>
        public void SetSelected(int row, int col, bool selected)
        {
            if (row < 0 || row >= _gridSize || col < 0 || col >= _gridSize)
                throw new ArgumentOutOfRangeException();
                
            _selectedLetters[row, col] = selected;
        }
        
        /// <summary>
        /// Clears all selections
        /// </summary>
        public void ClearSelections()
        {
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    _selectedLetters[row, col] = false;
                }
            }
        }
        
        /// <summary>
        /// Gets all letters in a specific row
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <returns>Array of letters in the row</returns>
        public char[] GetRowLetters(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _gridSize)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
                
            return _horizontalStrips[rowIndex].GetVisibleLetters();
        }
        
        /// <summary>
        /// Gets all letters in a specific column
        /// </summary>
        /// <param name="colIndex">Column index</param>
        /// <returns>Array of letters in the column</returns>
        public char[] GetColumnLetters(int colIndex)
        {
            if (colIndex < 0 || colIndex >= _gridSize)
                throw new ArgumentOutOfRangeException(nameof(colIndex));
                
            return _verticalStrips[colIndex].GetVisibleLetters();
        }
        
        /// <summary>
        /// Gets the extended row display (for UI, showing some letters before and after)
        /// </summary>
        /// <param name="rowIndex">Row index</param>
        /// <param name="paddingSize">How many letters to show on each side</param>
        /// <returns>Extended array of letters</returns>
        public char[] GetExtendedRowLetters(int rowIndex, int paddingSize)
        {
            if (rowIndex < 0 || rowIndex >= _gridSize)
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
                
            return _horizontalStrips[rowIndex].GetExtendedVisibleLetters(paddingSize);
        }
        
        /// <summary>
        /// Gets the extended column display (for UI, showing some letters before and after)
        /// </summary>
        /// <param name="colIndex">Column index</param>
        /// <param name="paddingSize">How many letters to show on each side</param>
        /// <returns>Extended array of letters</returns>
        public char[] GetExtendedColumnLetters(int colIndex, int paddingSize)
        {
            if (colIndex < 0 || colIndex >= _gridSize)
                throw new ArgumentOutOfRangeException(nameof(colIndex));
                
            return _verticalStrips[colIndex].GetExtendedVisibleLetters(paddingSize);
        }
                /// <summary>
        /// Removes all selected letters (matched words) and applies gravity
        /// </summary>
        /// <returns>Number of letters removed</returns>
        public int RemoveSelectedLetters()
        {
            int removedCount = 0;
            bool[,] removedPositions = new bool[_gridSize, _gridSize];
            
            // Create animations for selected letters that will be removed
            AnimatingLetters.Clear();
            float baseAnimationDelay = 0.5f; // Half second animation
            
            // First, mark which positions need to be removed
            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    if (_selectedLetters[row, col])
                    {
                        removedPositions[row, col] = true;
                        removedCount++;
                        
                        // Add an animating letter
                        char letter = GetLetter(row, col);
                        AnimatingLetter anim = new AnimatingLetter(row, col, letter, baseAnimationDelay);
                        AnimatingLetters.Add(anim);
                    }
                }
            }
            
            // Apply gravity to each column
            for (int col = 0; col < _gridSize; col++)
            {
                ApplyGravityToColumn(col, removedPositions);
            }
            
            // Clear selections since we've processed them
            ClearSelections();
            
            return removedCount;
        }
        
        /// <summary>
        /// Applies gravity to a column, moving letters down to fill empty spaces
        /// </summary>
        /// <param name="col">Column index</param>
        /// <param name="removedPositions">Grid of positions to remove</param>
        private void ApplyGravityToColumn(int col, bool[,] removedPositions)
        {
            // Get all letters in this column's vertical strip
            char[] verticalLetters = _verticalStrips[col].GetAllLetters();
            
            // Count letters to remove in this column
            int removeCount = 0;
            for (int row = 0; row < _gridSize; row++)
            {
                if (removedPositions[row, col])
                {
                    removeCount++;
                }
            }
            
            if (removeCount == 0)
                return; // Nothing to do for this column
                
            // Create a new array for the updated letters
            char[] newVerticalLetters = new char[_stripSize];
            
            // Create a mapping from visible grid positions to vertical strip positions
            int[] verticalPositions = new int[_gridSize];
            for (int row = 0; row < _gridSize; row++)
            {
                verticalPositions[row] = (_verticalStrips[col].Offset + row) % _stripSize;
            }
            
            // First, copy letters that aren't removed
            int destIndex = 0;
            for (int i = 0; i < _stripSize; i++)
            {
                // Check if this position is visible and marked for removal
                bool isRemoved = false;
                for (int row = 0; row < _gridSize; row++)
                {
                    if (verticalPositions[row] == i && removedPositions[row, col])
                    {
                        isRemoved = true;
                        break;
                    }
                }
                
                if (!isRemoved)
                {
                    newVerticalLetters[destIndex] = verticalLetters[i];
                    destIndex++;
                }
            }
            
            // Add new random letters at the top
            Random random = new Random();
            string commonLetters = "EEEEEEEEEEAAAAAAAARRRRRRRIIIIIIIOOOOOOOTTTTTTTNNNNNNNSSSSSSLLLLLCCCCUUUUDDDPPPMMM";
            
            for (int i = 0; i < removeCount; i++)
            {
                newVerticalLetters[destIndex] = commonLetters[random.Next(commonLetters.Length)];
                destIndex++;
            }
            
            // Apply the updated letters to the vertical strip
            _verticalStrips[col].SetLetters(newVerticalLetters);
            
            // Synchronize horizontal strips based on new vertical strip values
            SynchronizeHorizontalStrips();
        }
               
        public class AnimatingLetter
        {
            public int Row { get; }
            public int Column { get; }
            public char Letter { get; }
            public float Timer { get; set; }
            
            public AnimatingLetter(int row, int column, char letter, float timer)
            {
                Row = row;
                Column = column;
                Letter = letter;
                Timer = timer;
            }
        }

        // Add this property to the LetterGrid class
        public List<AnimatingLetter> AnimatingLetters { get; private set; } = new List<AnimatingLetter>();


        // Add a new method to update the animations
        public void UpdateAnimations(float deltaTime)
        {
            for (int i = AnimatingLetters.Count - 1; i >= 0; i--)
            {
                AnimatingLetters[i].Timer -= deltaTime;
                
                if (AnimatingLetters[i].Timer <= 0)
                {
                    AnimatingLetters.RemoveAt(i);
                }
            }
        }
    }
}