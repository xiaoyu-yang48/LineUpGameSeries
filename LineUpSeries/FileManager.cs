using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineUpSeries
{
    public class FileManager
    {
        public class CellSave
        {
            public int Owner { get; set; }
            public string Kind { get; set; } = "Ordinary";
        }

        public class GameSave
        {
            public int Rows { get; set; }
            public int Cols { get; set; }
            public int WinLen { get; set; }
            public int CurrentPlayerId { get; set; }
            public bool VsAI { get; set; }
            public CellSave[][] Cells { get; set; } = Array.Empty<CellSave[]>();
        }

        public static void Save(string path, Board board, int winLen, int currentPlayerId, bool vsAI)
        {
            var save = new GameSave
            {
                Rows = board.Rows,
                Cols = board.Cols,
                WinLen = winLen,
                CurrentPlayerId = currentPlayerId,
                VsAI = vsAI,
                Cells = new CellSave[board.Rows][]
            };

            for (int r = 0; r < board.Rows; r++)
            {
                save.Cells[r] = new CellSave[board.Cols];
                for (int c = 0; c < board.Cols; c++)
                {
                    var disc = board.Cells[r][c].Disc;
                    save.Cells[r][c] = new CellSave
                    {
                        Owner = disc?.PlayerId ?? 0,
                        Kind = disc?.Kind.ToString() ?? nameof(DiscKind.Ordinary)
                    };
                }
            }

            var json = System.Text.Json.JsonSerializer.Serialize(save, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            System.IO.File.WriteAllText(path, json);
        }

        public static GameSave Load(string path)
        {
            if (!System.IO.File.Exists(path)) throw new System.IO.FileNotFoundException(path);
            var json = System.IO.File.ReadAllText(path);
            var save = System.Text.Json.JsonSerializer.Deserialize<GameSave>(json);
            if (save == null) throw new InvalidOperationException("Failed to parse save file");
            return save;
        }

        public static void LoadInto(Board board, GameSave save)
        {
            if (board.Rows != save.Rows || board.Cols != save.Cols)
                throw new InvalidOperationException("Board size mismatch with save");

            for (int r = 0; r < board.Rows; r++)
            {
                for (int c = 0; c < board.Cols; c++)
                {
                    var cell = save.Cells[r][c];
                    if (cell.Owner == 0)
                    {
                        board.Cells[r][c].Disc = null;
                    }
                    else
                    {
                        board.Cells[r][c].Disc = CreateDisc(cell.Kind, cell.Owner);
                    }
                }
            }
            board.ApplyGravity();
        }

        public static Disc CreateDisc(string kind, int playerId)
        {
            if (Enum.TryParse<DiscKind>(kind, out var k))
            {
                return CreateDisc(k, playerId);
            }
            return new OrdinaryDisc(playerId);
        }

        public static Disc CreateDisc(DiscKind kind, int playerId)
        {
            return kind switch
            {
                DiscKind.Boring => new BoringDisc(playerId),
                DiscKind.Magnetic => new MagneticDisc(playerId),
                DiscKind.Explosive => new ExplosiveDisc(playerId),
                _ => new OrdinaryDisc(playerId)
            };
        }
    }
}
