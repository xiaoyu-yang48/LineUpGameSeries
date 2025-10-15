using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LineUpSeries
{
    // Serializable data structures for saving/loading game state

    public class GameSaveData
    {
        public string GameName { get; set; } = "";
        public int WinLen { get; set; }
        public SnapshotData CurrentState { get; set; } = new SnapshotData();
        public PlayerData Player1 { get; set; } = new PlayerData();
        public PlayerData Player2 { get; set; } = new PlayerData();
        public List<SnapshotData> UndoStack { get; set; } = new List<SnapshotData>();
        public List<SnapshotData> RedoStack { get; set; } = new List<SnapshotData>();
    }

    public class PlayerData
    {
        public int PlayerId { get; set; }
        public bool IsComputer { get; set; }
        public string AIStrategyType { get; set; } = "";
        public Dictionary<DiscKind, int> Inventory { get; set; } = new Dictionary<DiscKind, int>();
    }

    public class SnapshotData
    {
        public BoardData Board { get; set; } = new BoardData();
        public Dictionary<int, Dictionary<DiscKind, int>> PlayerInventories { get; set; } = new Dictionary<int, Dictionary<DiscKind, int>>();
        public int CurrentPlayerId { get; set; }
        public int TurnNumber { get; set; }
        public bool Player1Win { get; set; }
        public bool Player2Win { get; set; }
        public bool GameOver { get; set; }
    }

    public class BoardData
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public List<List<CellData>> Cells { get; set; } = new List<List<CellData>>();
    }

    public class CellData
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public DiscData? Disc { get; set; }
    }

    public class DiscData
    {
        public DiscKind Kind { get; set; }
        public int PlayerId { get; set; }
    }
}
