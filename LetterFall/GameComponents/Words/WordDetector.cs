using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LetterFall.GameComponents.Words
{
    /// <summary>
    /// Detects valid words in the letter grid
    /// </summary>
    public class WordDetector
    {
        // Reference to the letter grid
        private Models.LetterGrid _grid;
        
        // Dictionary of valid words
        private HashSet<string> _validWords;
        
        // Minimum word length to consider
        private int _minWordLength;
        
        // Maximum word length to consider
        private int _maxWordLength;

        /// <summary>
        /// Creates a new word detector
        /// </summary>
        /// <param name="grid">Reference to the letter grid</param>
        /// <param name="minWordLength">Minimum length of valid words</param>
        /// <param name="maxWordLength">Maximum length of valid words</param>
        public WordDetector(Models.LetterGrid grid, int minWordLength = 3, int maxWordLength = 5)
        {
            _grid = grid;
            _minWordLength = minWordLength;
            _maxWordLength = maxWordLength;
            _validWords = new HashSet<string>();
            
            // Load a simple dictionary for now
            LoadSimpleDictionary();
        }

        /// <summary>
        /// Loads a simple built-in dictionary of common words
        /// </summary>
        private void LoadSimpleDictionary()
        {
            // Common 3-5 letter English words 
            string[] commonWords = new string[]
            {
                // 3-letter words
                "THE", "AND", "FOR", "ARE", "BUT", "NOT", "YOU", "ALL", "ANY", "CAN", 
                "HAD", "HER", "WAS", "ONE", "OUR", "OUT", "DAY", "GET", "HAS", "HIM", 
                "HIS", "HOW", "MAN", "NEW", "NOW", "OLD", "SEE", "TWO", "WAY", "WHO", 
                "BOY", "DID", "ITS", "LET", "PUT", "SAY", "SHE", "TOO", "USE", "CAT",
                "DOG", "RUN", "SIT", "TOP", "CAR", "BUS", "BAT", "HAT", "MAP", "PEN",
                
                // 4-letter words
                "THAT", "WITH", "HAVE", "THIS", "WILL", "YOUR", "FROM", "THEY", "KNOW", 
                "WANT", "BEEN", "GOOD", "MUCH", "SOME", "TIME", "VERY", "WHEN", "COME", 
                "HERE", "JUST", "LIKE", "LONG", "MAKE", "MANY", "MORE", "ONLY", "OVER", 
                "SUCH", "TAKE", "THAN", "THEM", "WELL", "WERE", "WHAT", "BOOK", "FOOD",
                "ROOM", "LOVE", "BIRD", "FISH", "GAME", "PLAY", "JUMP", "WALK", "TALK",
                
                // 5-letter words
                "ABOUT", "COULD", "THEIR", "THERE", "OTHER", "THESE", "WHICH", "WOULD", 
                "WRITE", "FIRST", "WATER", "AFTER", "WHERE", "RIGHT", "THINK", "THREE", 
                "HOUSE", "WORLD", "BELOW", "ABOVE", "NEVER", "MONEY", "LEARN", "MUSIC", 
                "COLOR", "APPLE", "TABLE", "HAPPY", "SMILE", "SLEEP", "DREAM", "GREEN", 
                "BLACK", "WHITE", "EARTH", "CHILD", "NIGHT", "GREAT", "PAPER", "PHONE"
            };
            
            // Add all words to the dictionary
            foreach (string word in commonWords)
            {
                _validWords.Add(word);
            }
        }

        /// <summary>
        /// Loads a dictionary from a text file
        /// </summary>
        /// <param name="filePath">Path to the dictionary file</param>
        public void LoadDictionary(string filePath)
        {
            try
            {
                string[] words = File.ReadAllLines(filePath);
                
                _validWords.Clear();
                
                foreach (string word in words)
                {
                    // Only add words within our length constraints
                    if (word.Length >= _minWordLength && word.Length <= _maxWordLength)
                    {
                        _validWords.Add(word.ToUpper());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dictionary: {ex.Message}");
                // Fallback to the simple dictionary
                LoadSimpleDictionary();
            }
        }

        /// <summary>
        /// Finds all valid words in rows and columns
        /// </summary>
        /// <returns>List of detected words with their positions</returns>
        public List<DetectedWord> FindWords()
        {
            List<DetectedWord> foundWords = new List<DetectedWord>();
            
            // Check rows for words
            for (int row = 0; row < 5; row++)
            {
                // Get all letters in this row
                char[] rowLetters = _grid.GetRowLetters(row);
                string rowString = new string(rowLetters);
                
                // Find words in this row
                List<DetectedWord> rowWords = FindWordsInString(rowString, row, true);
                foundWords.AddRange(rowWords);
            }
            
            // Check columns for words
            for (int col = 0; col < 5; col++)
            {
                // Get all letters in this column
                char[] colLetters = _grid.GetColumnLetters(col);
                string colString = new string(colLetters);
                
                // Find words in this column
                List<DetectedWord> colWords = FindWordsInString(colString, col, false);
                foundWords.AddRange(colWords);
            }
            
            return foundWords;
        }

        /// <summary>
        /// Finds valid words in a string of letters
        /// </summary>
        /// <param name="letters">String of letters to check</param>
        /// <param name="index">Row or column index</param>
        /// <param name="isRow">Whether this is a row (true) or column (false)</param>
        /// <returns>List of detected words</returns>
        private List<DetectedWord> FindWordsInString(string letters, int index, bool isRow)
        {
            List<DetectedWord> words = new List<DetectedWord>();
            
            // Check all possible substrings of appropriate length
            for (int length = _minWordLength; length <= Math.Min(_maxWordLength, letters.Length); length++)
            {
                for (int start = 0; start <= letters.Length - length; start++)
                {
                    // Extract the substring
                    string candidate = letters.Substring(start, length);
                    
                    // Check if it's a valid word
                    if (_validWords.Contains(candidate))
                    {
                        // Create positions for this word
                        List<Position> positions = new List<Position>();
                        
                        for (int i = 0; i < length; i++)
                        {
                            if (isRow)
                            {
                                positions.Add(new Position(index, start + i));
                            }
                            else
                            {
                                positions.Add(new Position(start + i, index));
                            }
                        }
                        
                        // Add this word to the list
                        words.Add(new DetectedWord(candidate, positions));
                    }
                }
            }
            
            return words;
        }

        /// <summary>
        /// Highlights all found words in the grid
        /// </summary>
        /// <param name="words">List of detected words</param>
        public void HighlightWords(List<DetectedWord> words)
        {
            // First clear any previous selections
            _grid.ClearSelections();
            
            // Then highlight all positions for found words
            foreach (DetectedWord word in words)
            {
                foreach (Position pos in word.Positions)
                {
                    _grid.SetSelected(pos.Row, pos.Column, true);
                }
            }
        }
    }

    /// <summary>
    /// Represents a position in the grid
    /// </summary>
    public struct Position
    {
        public int Row { get; }
        public int Column { get; }
        
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }

    /// <summary>
    /// Represents a detected word in the grid
    /// </summary>
    public class DetectedWord
    {
        public string Word { get; }
        public List<Position> Positions { get; }
        
        public DetectedWord(string word, List<Position> positions)
        {
            Word = word;
            Positions = positions;
        }
        
        public int Length => Word.Length;
    }
}