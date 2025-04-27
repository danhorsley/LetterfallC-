using System;
using System.Collections.Generic;

namespace LetterFall.Models
{
    /// <summary>
    /// Represents a circular strip of letters that can be shifted
    /// </summary>
    public class LetterStrip
    {
        // The actual letters in the strip (circular buffer)
        private char[] _letters;
        
        // Current offset in the strip (where the first visible letter is)
        private int _offset;
        
        // Size of the full strip
        private readonly int _stripSize;
        
        // Size of the visible section in the grid
        private readonly int _visibleSize;

        /// <summary>
        /// Creates a new letter strip
        /// </summary>
        /// <param name="stripSize">Total size of the strip (e.g., 10)</param>
        /// <param name="visibleSize">Number of visible letters in the grid (e.g., 5)</param>
        public LetterStrip(int stripSize, int visibleSize)
        {
            _stripSize = stripSize;
            _visibleSize = visibleSize;
            _letters = new char[stripSize];
            _offset = 0;
            
            // Fill with placeholder letters
            for (int i = 0; i < stripSize; i++)
            {
                _letters[i] = (char)('A' + (i % 26));
            }
        }

        /// <summary>
        /// Gets a letter at a specific position in the visible grid
        /// </summary>
        /// <param name="visiblePosition">Position (0 to visibleSize-1)</param>
        /// <returns>The letter at that position</returns>
        public char GetVisibleLetter(int visiblePosition)
        {
            if (visiblePosition < 0 || visiblePosition >= _visibleSize)
                throw new ArgumentOutOfRangeException(nameof(visiblePosition));
                
            // Calculate the actual index in the circular buffer
            int actualIndex = (_offset + visiblePosition) % _stripSize;
            return _letters[actualIndex];
        }
        
        /// <summary>
        /// Sets a letter at a specific position in the visible grid
        /// </summary>
        /// <param name="visiblePosition">Position (0 to visibleSize-1)</param>
        /// <param name="letter">The letter to set</param>
        public void SetVisibleLetter(int visiblePosition, char letter)
        {
            if (visiblePosition < 0 || visiblePosition >= _visibleSize)
                throw new ArgumentOutOfRangeException(nameof(visiblePosition));
                
            // Calculate the actual index in the circular buffer
            int actualIndex = (_offset + visiblePosition) % _stripSize;
            _letters[actualIndex] = letter;
        }

        /// <summary>
        /// Gets the full array of letters from the strip
        /// </summary>
        public char[] GetAllLetters()
        {
            return (char[])_letters.Clone();
        }

        /// <summary>
        /// Sets the letters in the strip
        /// </summary>
        public void SetLetters(char[] letters)
        {
            if (letters.Length != _stripSize)
                throw new ArgumentException($"Letters array must be of size {_stripSize}");
                
            _letters = (char[])letters.Clone();
        }

        /// <summary>
        /// Shifts the strip by a specified number of positions
        /// </summary>
        /// <param name="positions">Positions to shift (positive = right/down, negative = left/up)</param>
        public void Shift(int positions)
        {
            // Handle the offset in a circular way
            _offset = (_offset - positions) % _stripSize;
            
            // Ensure positive modulo result
            if (_offset < 0)
                _offset += _stripSize;
        }
        
        /// <summary>
        /// Gets all currently visible letters (what's shown in the grid)
        /// </summary>
        /// <returns>Array of visible letters</returns>
        public char[] GetVisibleLetters()
        {
            char[] visible = new char[_visibleSize];
            for (int i = 0; i < _visibleSize; i++)
            {
                visible[i] = GetVisibleLetter(i);
            }
            return visible;
        }
        
        /// <summary>
        /// Gets extended visible letters (including some before and after for UI)
        /// </summary>
        /// <param name="paddingSize">How many letters to show on each side</param>
        /// <returns>Extended array of letters for display</returns>
        public char[] GetExtendedVisibleLetters(int paddingSize)
        {
            int totalSize = _visibleSize + (paddingSize * 2);
            char[] extended = new char[totalSize];
            
            for (int i = 0; i < totalSize; i++)
            {
                // Convert to position relative to the visible window's start
                int relativePos = i - paddingSize;
                
                // Handle positions before or after the visible window
                int actualPos = (_offset + relativePos) % _stripSize;
                while (actualPos < 0) actualPos += _stripSize;
                
                extended[i] = _letters[actualPos];
            }
            
            return extended;
        }
        
        /// <summary>
        /// Gets the current offset
        /// </summary>
        public int Offset => _offset;
    }
}