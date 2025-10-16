using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    // Implement Redo and Undo functionalities with Singleton Pattern
    public class MoveManager
    {
        private static MoveManager? _instance;

        private Stack<GameStateSnapshot> _undoStack;
        private Stack<GameStateSnapshot> _redoStack;
        private const int MAX_HISTORY = 100; // Prevent excessive memory usage

        // Private constructor to prevent external instantiation
        private MoveManager()
        {
            _undoStack = new Stack<GameStateSnapshot>();
            _redoStack = new Stack<GameStateSnapshot>();
        }

        /// Gets the singleton instance of MoveManager
        public static MoveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MoveManager();
                }
                return _instance;
            }
        }

        /// Saves the current game state before a move is executed
        public void SaveState(GameStateSnapshot snapshot)
        {
            _undoStack.Push(snapshot);
            // Clear redo stack when a new move is made
            _redoStack.Clear();

            // Enforce max history limit
            if (_undoStack.Count > MAX_HISTORY)
            {
                // Remove oldest entry (at the bottom of the stack)
                var temp = new Stack<GameStateSnapshot>(_undoStack.Reverse().Skip(1));
                _undoStack = new Stack<GameStateSnapshot>(temp.Reverse());
            }
        }

        /// Checks if undo operation is possible
        public bool CanUndo()
        {
            return _undoStack.Count > 0;
        }

        /// Checks if redo operation is possible for the specified number of turns
        public bool CanRedo()
        {
            return _redoStack.Count > 0;
        }

        // undo back previous move
        public GameStateSnapshot? Undo()
        {
            if (!CanUndo())
            {
                throw new InvalidOperationException("No moves to undo");
            }
            Console.WriteLine($"undostacks size:{_undoStack.Count()}");
            // Pop the current state and push to redo stack
            var currentState = _undoStack.Pop();
            _redoStack.Push(currentState);
            // Return the previous state (now at top of undo stack)
            // If undo stack is empty, we're at the initial state
            if (_undoStack.Count > 0)
            {
                return _undoStack.Peek();
            }

            return null;
        }

        /// Redoes previous undone action
        public GameStateSnapshot Redo()
        {
            if (!CanRedo())
            {
                throw new InvalidOperationException($"Cannot redo previous turn. Only {_redoStack.Count} turns available.");
            }

            var state = _redoStack.Pop();
            _undoStack.Push(state);
            Console.WriteLine($"Pop current state from redo stack: playerID:{state.CurrentPlayerId}, turn: {state.TurnNumber}");
            var top = _undoStack.Peek();
            Console.WriteLine($"Pop current state from redo stack: playerID:{top.CurrentPlayerId}, turn: {top.TurnNumber}");

            // Return the state we just restored
            return _undoStack.Peek();
        }

        /// Gets the number of available undo operations
        public int GetUndoCount()
        {
            return _undoStack.Count;
        }

        /// Gets the number of available redo operations
        public int GetRedoCount()
        {
            return _redoStack.Count;
        }

        /// Clears all history (for starting a new game)
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        /// Gets the undo stack for saving
        public Stack<GameStateSnapshot> GetUndoStackForSave()
        {
            return new Stack<GameStateSnapshot>(_undoStack);
        }


        /// Restores the undo and redo stacks from saved data
        public void RestoreStacks(List<GameStateSnapshot> undoList, int turnNumber)
        {
            _undoStack.Clear();
            _redoStack.Clear();

            // Restore undo stack (reverse the list to get correct stack order)
            foreach (var snapshot in undoList.AsEnumerable().Reverse())
            {
                _undoStack.Push(snapshot);
            }
        }
    }
}
