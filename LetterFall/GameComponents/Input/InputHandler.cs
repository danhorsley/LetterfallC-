using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace LetterFall.GameComponents.Input
{
    /// <summary>
    /// Handles dragging input for rows and columns
    /// </summary>
    public class InputHandler
    {
        // Constants for drag detection
        private const float MIN_DRAG_DISTANCE = 10.0f;
        private const float DRAG_THRESHOLD = 15.0f;
        
        // References to game components
        private Models.LetterGrid _grid;
        
        // State tracking
        private bool _isDragging;
        private Vector2 _dragStartPosition;
        private Vector2 _currentPosition;
        private DragDirection _dragDirection;
        private int _selectedRow;
        private int _selectedColumn;
        private float _accumulatedDrag;
        
        // Grid rendering information
        private Rectangle _gridBounds;
        private float _cellSize;
        
        /// <summary>
        /// Direction of drag (horizontal or vertical)
        /// </summary>
        public enum DragDirection
        {
            None,
            Horizontal,
            Vertical
        }

        /// <summary>
        /// Creates a new input handler
        /// </summary>
        /// <param name="grid">Reference to the letter grid</param>
        /// <param name="gridBounds">Rectangle representing the grid's position and size on screen</param>
        public InputHandler(Models.LetterGrid grid, Rectangle gridBounds)
        {
            _grid = grid;
            _gridBounds = gridBounds;
            _cellSize = gridBounds.Width / 5.0f; // Assuming 5x5 grid
            
            Reset();
        }

        /// <summary>
        /// Resets the drag state
        /// </summary>
        public void Reset()
        {
            _isDragging = false;
            _dragDirection = DragDirection.None;
            _selectedRow = -1;
            _selectedColumn = -1;
            _accumulatedDrag = 0;
        }

        /// <summary>
        /// Updates the input state
        /// </summary>
        /// <param name="gameTime">Current game time</param>
        public void Update(GameTime gameTime)
{
        MouseState mouseState = Mouse.GetState();
        Vector2 newPosition = new Vector2(mouseState.X, mouseState.Y);
        
        // Handle starting a drag
        if (mouseState.LeftButton == ButtonState.Pressed && !_isDragging)
        {
            // Check if the click is within grid bounds
            if (_gridBounds.Contains(mouseState.Position))
            {
                StartDrag(mouseState.Position);
            }
        }
        // Handle continuing a drag
        else if (mouseState.LeftButton == ButtonState.Pressed && _isDragging)
        {
            ContinueDrag(mouseState.Position, newPosition);
        }
        // Handle ending a drag
        else if (mouseState.LeftButton == ButtonState.Released && _isDragging)
        {
            EndDrag();
        }
        
        // Update current position at the END of the method
        _currentPosition = newPosition;
        }

        /// <summary>
        /// Starts a drag operation
        /// </summary>
        /// <param name="position">Mouse position</param>
        private void StartDrag(Point position)
        {
            _isDragging = true;
            _dragStartPosition = new Vector2(position.X, position.Y);
            _currentPosition = _dragStartPosition;
            
            // Calculate which row/column was clicked
            float relativeX = position.X - _gridBounds.X;
            float relativeY = position.Y - _gridBounds.Y;
            
            _selectedRow = (int)(relativeY / _cellSize);
            _selectedColumn = (int)(relativeX / _cellSize);
            
            // Clamp to grid boundaries
            _selectedRow = Math.Clamp(_selectedRow, 0, 4);
            _selectedColumn = Math.Clamp(_selectedColumn, 0, 4);
            
            _dragDirection = DragDirection.None;
            _accumulatedDrag = 0;
        }

        /// <summary>
        /// Continues a drag operation
        /// </summary>
        /// <param name="position">Current mouse position</param>
        private void ContinueDrag(Point position, Vector2 newPosition)
        {
        // Calculate drag vector from original start position
        Vector2 dragVector = new Vector2(position.X, position.Y) - _dragStartPosition;
        Console.WriteLine($"Drag vector: {dragVector} Length: {dragVector.Length()} MinDist: {MIN_DRAG_DISTANCE}");
        // Determine drag direction if not already set
        if (_dragDirection == DragDirection.None)
        {
        if (dragVector.Length() >= MIN_DRAG_DISTANCE)
        {
            // Determine primary direction of drag
            if (Math.Abs(dragVector.X) > Math.Abs(dragVector.Y))
            {
                _dragDirection = DragDirection.Horizontal;
                System.Diagnostics.Debug.WriteLine($"Started horizontal drag on row {_selectedRow}");
            }
            else
            {
                _dragDirection = DragDirection.Vertical;
                System.Diagnostics.Debug.WriteLine($"Started vertical drag on column {_selectedColumn}");
            }
            
            // Reset the drag start position after determining direction
            _dragStartPosition = new Vector2(position.X, position.Y);
            _accumulatedDrag = 0;
        }
    }
    
    // Process drag based on direction
    if (_dragDirection == DragDirection.Horizontal && _selectedRow >= 0)
    {
        // Calculate change from last position to new position
        float change = newPosition.X - _currentPosition.X;
        
        // Accumulate horizontal drag
        _accumulatedDrag += change;
        
        Console.WriteLine($"Horizontal drag: New X: {newPosition.X} Previous X: {_currentPosition.X} Change: {change}");
        Console.WriteLine($"Accumulated drag: {_accumulatedDrag} (Threshold: {DRAG_THRESHOLD})");
        
        // If we've dragged enough, shift the row
        if (Math.Abs(_accumulatedDrag) >= DRAG_THRESHOLD)
        {
            int shifts = (int)(_accumulatedDrag / DRAG_THRESHOLD);
            
            Console.WriteLine($"Shifting row {_selectedRow} by {-shifts} positions");
            
            // Negative shifts = right drag (letters move left)
            // Positive shifts = left drag (letters move right)
            _grid.ShiftRow(_selectedRow, -shifts);
            
            // Remove the processed drag amount
            _accumulatedDrag -= shifts * DRAG_THRESHOLD;
            
            Console.WriteLine($"After shift, accumulated drag: {_accumulatedDrag}");
        }
}
    else if (_dragDirection == DragDirection.Vertical && _selectedColumn >= 0)
    {
        // Calculate drag delta relative to current position
        float dragDelta = position.Y - _currentPosition.Y;
        _accumulatedDrag += dragDelta;
        
        // Check if we've exceeded the threshold in either direction
        int cellsMoved = (int)(_accumulatedDrag / DRAG_THRESHOLD);
        
        if (cellsMoved != 0)
        {
            // Apply the shift
            _grid.ShiftColumn(_selectedColumn, -cellsMoved);
            
            // Adjust the accumulated drag and reset the drag start point
            _accumulatedDrag -= cellsMoved * DRAG_THRESHOLD;
            System.Diagnostics.Debug.WriteLine($"Vertical shift: {cellsMoved}, new accum: {_accumulatedDrag}");
        }
    }
    

}

        /// <summary>
        /// Ends a drag operation
        /// </summary>
        private void EndDrag()
        {
            Reset();
        }

        /// <summary>
        /// Gets whether a drag is in progress
        /// </summary>
        public bool IsDragging => _isDragging;
        
        /// <summary>
        /// Gets the currently selected row (or -1 if none)
        /// </summary>
        public int SelectedRow => _selectedRow;
        
        /// <summary>
        /// Gets the currently selected column (or -1 if none)
        /// </summary>
        public int SelectedColumn => _selectedColumn;
        
        /// <summary>
        /// Gets the current drag direction
        /// </summary>
        public DragDirection CurrentDragDirection => _dragDirection;
    }
}