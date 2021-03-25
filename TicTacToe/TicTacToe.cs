using System;

namespace Choker
{
    /// <summary>
    /// A simple and efficient implementation of an nxn Tic-tac-toe paper-and-pencil game
    /// https://en.wikipedia.org/wiki/Tic-tac-toe
    /// </summary>
    /// <date>13.02.2020</date>
    /// <author>Jalal Choker, jalal.choker@gmail.com</author>    
    public class TicTacToe
    {
        public int Size { get; }
        readonly BoardCell[,] board; // square board
        public TicTacToe(int size)
        {
            this.Size = (size < minimalSize) ? throw new ArgumentOutOfRangeException(nameof(size), $"Size cannot be less than {minimalSize}") : size;
            this.board = new BoardCell[size, size];
        }
        const int minimalSize = 3;

        public TicTacToePlayer Player1 { get; private set; }
        public TicTacToePlayer Player2 { get; private set; }
        public TicTacToePlayer Winner { get; private set; }
        public TicTacToePlayer LastPlayed { get; private set; }
        public bool HasGameEnded { get; private set; }

        public void RegisterPlayer(string name, BoardCell type)
        {
            if (Player1 == null) Player1 = TicTacToePlayer.Create(name, type);
            else if (Player2 == null)
            {
                if (Player1.Name == name) throw new InvalidOperationException("Name already registered.");
                if (Player1.Type == type) throw new InvalidOperationException("Type already registered.");
                Player2 = TicTacToePlayer.Create(name, type);
            }
            else throw new InvalidOperationException("Players already registered.");
        }

        int plays;
        public bool Play(string name, int row, int col) // play and check any winner
        {
            CheckGameRunning();
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            CheckPlayerRegistered(name);
            ValidateCell(row, col); // should be 1 based instead of 0 based for the player
            var p = CheckAndUpdateLastPlayed(name);

            this.board[row, col] = p.Type;
            plays++;

            var res = AnyWinner(row, col);
            if (res)
            {
                HasGameEnded = true;
                Winner = p;
            }
            else if (plays == (this.Size * this.Size)) HasGameEnded = true;

            return res;
        }
        void CheckGameRunning()
        {
            if (HasGameEnded) throw new InvalidOperationException("Game has ended");
        }
        void CheckPlayerRegistered(string name)
        {
            if (Player1?.Name != name && Player2?.Name != name) throw new ArgumentException($"Invalid player {name}", nameof(name));
        }
        TicTacToePlayer CheckAndUpdateLastPlayed(string nextPlayer)
        {
            if (LastPlayed?.Name == nextPlayer) throw new InvalidOperationException($"Not your turn {nextPlayer}");
            this.LastPlayed = (nextPlayer == Player1.Name) ? Player1 : Player2;
            return LastPlayed;
        }
        void ValidateCell(int row, int col)
        {
            if ((!IsValidCell(row, col))) throw new ArgumentOutOfRangeException($"Outside the boundaries of the board");
            if (board[row, col] != BoardCell.Empty) throw new InvalidOperationException("Cell already marked");
        }
        bool IsValidCell(int row, int col)
        {
            return IsValid(row) && IsValid(col);
            bool IsValid(int rowOrCol) => (rowOrCol >= 0 && rowOrCol < Size);
        }

        bool AnyWinner(int row, int col) // constant time
        {
            var t = board[row, col];

            // check for 3 X or O where the input cell is in the middle of them, 4 cases to check
            {
                // n, e, ne, se
                var rMove = new[] { -1, 0, -1, 1 };
                var cMove = new[] { 0, 1, 1, 1, };

                for (int i = 0; i < 4; i++)
                {
                    var r1 = row + rMove[i];
                    var c1 = col + cMove[i];

                    // symmetric node of the above w.r.t input cell
                    var r2 = row - rMove[i];
                    var c2 = col - cMove[i];

                    if (AreValidAndHaveSameValue((r1, c1), (r2, c2))) return true;
                }
            }

            // check for 3 X or O where the input cell is at the edge of the pattern, 8 cases to check
            {
                //r + [i] & r+ 2[i]

                // top north, clock-wise rotation
                var dirs = new int[][] { new []{ -1, 0 }, new[] { -1, 1 }, new[] { 0, 1 }, new[] { 1, 1 }, new[] { 1, 0 }, new[] { 1, -1 }, new[] { 0, -1 }, new[] { -1, -1 } } ;

                foreach (var dir in dirs) 
                {
                    var r1 = row + dir[0];
                    var c1 = col + dir[1];

                    var r2 = row + 2 * dir[0];
                    var c2 = col + 2 * dir[1];

                    if (AreValidAndHaveSameValue((r1, c1), (r2, c2))) return true;
                }
            }

            return false;

            #region Local
            bool AreValidAndHaveSameValue((int R, int C) c1, (int R, int C) c2) =>
                IsValidCell(c1.R, c1.C) && IsValidCell(c2.R, c2.C)
             && board[c1.R, c1.C] == t && board[c2.R, c2.C] == t;
            #endregion
        }

        public class TicTacToePlayer
        {
            private TicTacToePlayer(string name, BoardCell type)
            {
                this.Name = name ?? throw new ArgumentNullException(nameof(name));
                this.Type = type == BoardCell.Empty ? throw new ArgumentOutOfRangeException(nameof(type), "Cannot be empty") : type;
            }
            public string Name { get; }
            public BoardCell Type { get; }

            internal static TicTacToePlayer Create(string name, BoardCell type) => new TicTacToePlayer(name, type);
        }
    }

    public enum BoardCell
    {
        Empty = 0,
        Cross,
        Nought
    }
}