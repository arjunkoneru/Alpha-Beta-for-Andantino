using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Andantino2
{
    class DeterministicRandomGenerator : System.Security.Cryptography.RandomNumberGenerator
    {
        Random thisRandom;
        public DeterministicRandomGenerator(int i)
        {
            Random r = new Random(i);
            thisRandom = r;
        }
        public override void GetBytes(byte[] data)
        {
            thisRandom.NextBytes(data);
        }
        public override void GetNonZeroBytes(byte[] data)
        {
            // simple implementation
            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)thisRandom.Next(1, 256);
        }
    }
    public class GameManager
    {
        public enum Players { Player1, Player2 };
        private static int seed = 0;
        public int movesMade = 0;

        public bool isDraw = false;
        public bool haveWinner;
        public bool gameOver = false;
        public ClonableBoard board;

        public Stack<AndantinoMove> moves;

        public Players currentTurn = Players.Player1;
        public Players winningPlayer;

        public ClonableBoard.Pieces player1Piece = ClonableBoard.Pieces.Black;
        public ClonableBoard.Pieces player2Piece = ClonableBoard.Pieces.White;
        public ClonableBoard.Pieces winningPiece = ClonableBoard.Pieces.Blank;

        public GameManager() : this(ClonableBoard.Pieces.Black, ClonableBoard.Pieces.White)
        {

        }

        public GameManager(ClonableBoard.Pieces player1Piece, ClonableBoard.Pieces player2Piece)
        {
            this.player1Piece = player1Piece;
            this.player2Piece = player2Piece;
            board = new ClonableBoard(this);
            moves = new Stack<AndantinoMove>();
        }

        public Int64 NextInt64()
        {
            var bytes = new byte[sizeof(Int64)];
            DeterministicRandomGenerator Gen = new DeterministicRandomGenerator(seed);
            Gen.GetBytes(bytes);
            seed++;
            return BitConverter.ToInt64(bytes, 0);
        }

        public ClonableBoard GameBoard
        {
            get { return board; }
        }

        public ClonableBoard.Pieces WinningPiece
        {
            get { return winningPiece; }
        }

        public bool IsGameOver(Tuple<int, int> coord, ClonableBoard.Pieces piece)
        {
            return board.IsGameOver(coord,piece);
        }

        public ClonableBoard.Pieces Player1Piece
        {
            get { return player1Piece; }
            set { player1Piece = value; }
        }

        public ClonableBoard.Pieces Player2Piece
        {
            get { return player2Piece; }
            set { player2Piece = value; }
        }

        public Players CurrentPlayerTurn
        {
            get { return this.currentTurn; }
        }

        public void MakeMove(AndantinoMove m)
        {
            MakeMove(m, GetPlayerWhoHasPiece(m.movingPiece));
        }

        public void MakeMove(AndantinoMove m, Players p)
        {

            if (currentTurn != p)
            {
                throw new InvalidMoveException("You went out of turn!");
            }
            board.MakeMove(m.movecoord,GetPlayersPiece(currentTurn));
            moves.Push(m);
            SwapTurns();
        }

        protected ClonableBoard.Pieces GetPlayersPiece(Players p)
        {
            if (p == Players.Player1)
                return player1Piece;
            else
                return player2Piece;
        }
        protected GameManager.Players GetPlayerWhoHasPiece(ClonableBoard.Pieces piece)
        {
            if (piece == player1Piece)
                return Players.Player1;
            else
                return Players.Player2;
        }

        private void SwapTurns()
        {
            if (currentTurn == Players.Player1)
                currentTurn = Players.Player2;

            else
                currentTurn = Players.Player1;
        }
    }

    public class AndantinoMove
    {
        public AndantinoMove()
        {
        }

        public AndantinoMove(Tuple<int,int> coord, ClonableBoard.Pieces piece)
        {
            this.movecoord = coord;
            this.movingPiece = piece;
        }

        public Tuple<int,int> movecoord { get; set; }
        public ClonableBoard.Pieces movingPiece { get; set; }
    }
}
