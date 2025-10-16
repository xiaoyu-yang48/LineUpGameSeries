using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LineUpSeries
{
    public class FileManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// Saves the current game state to a JSON file
        public static void SaveGame(string filePath, Game game)
        {
            try
            {
                var saveData = new GameSaveData
                {
                    GameName = game.Name,
                    WinLen = game.WinLen,
                    TurnNumber = game.TurnNumber,
                    CurrentState = ConvertSnapshotToData(game.CaptureCurrentSnapshot()),
                    Player1 = ConvertPlayerToData(game.Player1),
                    Player2 = ConvertPlayerToData(game.Player2),
                    UndoStack = ConvertStackToDataList(game.GetUndoStack()),
                };

                string jsonString = JsonSerializer.Serialize(saveData, JsonOptions);
                File.WriteAllText(filePath, jsonString);
                Console.WriteLine($"Game saved successfully to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving game: {ex.Message}");
                throw;
            }
        }

        /// Loads game state from a JSON file
        public static GameSaveData LoadGame(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Save file not found: {filePath}");

            try
            {
                string jsonString = File.ReadAllText(filePath);
                var saveData = JsonSerializer.Deserialize<GameSaveData>(jsonString, JsonOptions);

                if (saveData == null)
                    throw new InvalidOperationException("Failed to deserialize save data");

                // Validate the loaded data
                ValidateSaveData(saveData);

                Console.WriteLine($"Game loaded successfully from {filePath}");
                return saveData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading game: {ex.Message}");
                throw;
            }
        }

        /// Converts a Player object to serializable PlayerData
        private static PlayerData ConvertPlayerToData(Player player)
        {
            var playerData = new PlayerData
            {
                PlayerId = player.playerId,
                IsComputer = player.IsComputer,
                Inventory = new Dictionary<DiscKind, int>(player.Inventory)
            };

            if (player is ComputerPlayer cp)
            {
                playerData.AIStrategyType = cp.Strategy.GetType().Name;
            }

            return playerData;
        }

        /// Converts a GameStateSnapshot to serializable SnapshotData
        private static SnapshotData ConvertSnapshotToData(GameStateSnapshot snapshot)
        {
            return new SnapshotData
            {
                Board = ConvertBoardToData(snapshot.BoardClone),
                PlayerInventories = new Dictionary<int, Dictionary<DiscKind, int>>(snapshot.PlayerInventories),
                CurrentPlayerId = snapshot.CurrentPlayerId,
                Player1Win = snapshot.Player1Win,
                Player2Win = snapshot.Player2Win,
                GameOver = snapshot.GameOver,
                TurnNumber = snapshot.TurnNumber
            };
        }

        /// Converts a Board to serializable BoardData
        private static BoardData ConvertBoardToData(Board board)
        {
            var boardData = new BoardData
            {
                Rows = board.Rows,
                Cols = board.Cols,
                Cells = new List<List<CellData>>()
            };

            for (int r = 0; r < board.Rows; r++)
            {
                var rowData = new List<CellData>();
                for (int c = 0; c < board.Cols; c++)
                {
                    var cell = board.Cells[r][c];
                    var cellData = new CellData
                    {
                        Row = r,
                        Col = c,
                        Disc = cell.Disc != null ? new DiscData
                        {
                            Kind = cell.Disc.Kind,
                            PlayerId = cell.Disc.PlayerId
                        } : null
                    };
                    rowData.Add(cellData);
                }
                boardData.Cells.Add(rowData);
            }

            return boardData;
        }

        /// Converts a stack of snapshots to a list of serializable data
        private static List<SnapshotData> ConvertStackToDataList(Stack<GameStateSnapshot> stack)
        {
            // Convert stack to list (reversing to maintain order)
            return stack.Reverse().Select(ConvertSnapshotToData).ToList();
        }

        /// Reconstructs a Player object from PlayerData
        public static Player ConvertDataToPlayer(PlayerData data, IWinRule winRule)
        {
            Player player;

            if (data.IsComputer)
            {
                // Recreate AI strategy based on type name
                IAIStrategy strategy = data.AIStrategyType switch
                {
                    "ImmeWinElseRandom" => new ImmeWinElseRandom(winRule, data.PlayerId),
                    _ => new ImmeWinElseRandom(winRule, data.PlayerId) // Default strategy
                };
                player = new ComputerPlayer(strategy, data.PlayerId);
            }
            else
            {
                player = new HumanPlayer(data.PlayerId);
            }

            // Restore inventory
            foreach (var kvp in data.Inventory)
            {
                player.Inventory[kvp.Key] = kvp.Value;
            }

            return player;
        }

        /// Reconstructs a GameStateSnapshot from SnapshotData
        public static GameStateSnapshot ConvertDataToSnapshot(SnapshotData data, Player player1, Player player2)
        {
            var board = ConvertDataToBoard(data.Board, player1, player2);
            var currentPlayer = data.CurrentPlayerId == 1 ? player1 : player2;

            return new GameStateSnapshot(
                board,
                player1,
                player2,
                data.CurrentPlayerId,
                data.TurnNumber,
                data.Player1Win,
                data.Player2Win,
                data.GameOver
            );
        }

        /// Reconstructs a Board from BoardData
        public static Board ConvertDataToBoard(BoardData data, Player player1, Player player2)
        {
            var board = new Board(data.Rows, data.Cols);

            for (int r = 0; r < data.Rows; r++)
            {
                for (int c = 0; c < data.Cols; c++)
                {
                    var cellData = data.Cells[r][c];
                    if (cellData.Disc != null)
                    {
                        var owner = cellData.Disc.PlayerId == 1 ? player1 : player2;
                        board.Cells[r][c].Disc = DiscFactory.Create(cellData.Disc.Kind, owner);
                    }
                }
            }

            return board;
        }

        /// Validates the loaded save data
        private static void ValidateSaveData(GameSaveData data)
        {
            if (data.Player1 == null || data.Player2 == null)
                throw new InvalidOperationException("Invalid save data: missing player information");

            if (data.Player1.PlayerId != 1 || data.Player2.PlayerId != 2)
                throw new InvalidOperationException("Invalid save data: incorrect player IDs");

            if (data.CurrentState == null)
                throw new InvalidOperationException("Invalid save data: missing current state");

            if (data.CurrentState.Board == null)
                throw new InvalidOperationException("Invalid save data: missing board data");
        }
    }
}
