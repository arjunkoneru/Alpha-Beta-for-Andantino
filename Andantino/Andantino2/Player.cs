using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace Andantino2
{
    public delegate void PlayerMovedHandler(object sender, PlayerMovedArgs args);
    public abstract class Player
    {
        public event PlayerMovedHandler PlayerMoved;
        protected AndantinoMove currentMove;

        public Player(string name, ClonableBoard.Pieces p)
        {
            this.Name = name;
            this.PlayerPiece = p;
        }

        public ClonableBoard.Pieces PlayerPiece { get; set; }

        public string Name { get; set; }


        public abstract void Move(object gameBoard);


        public AndantinoMove CurrentMove
        {
            get { return currentMove; }
        }

        public virtual void OnPlayerMoved()
        {
            if (PlayerMoved != null)
                PlayerMoved(this, new PlayerMovedArgs(currentMove, this));
        }

    }

    public class HumanPlayer: Player
    {
        protected Form2 form;
        protected bool alreadymoved = false;

        public HumanPlayer(string name, ClonableBoard.Pieces p, Form2 f) : base(name,p)
        {
            this.form = f;
        }

        public override void Move(object gameBoard)
        {
            form.HexClicked += new HexClickHandler(HexClicked);
            while (!alreadymoved)
                ;
            alreadymoved = false;
            OnPlayerMoved();
        }

        void HexClicked(Object sender, AndantinoBoardClickedEventArgs a)
        {
            form.HexClicked -= HexClicked;
            currentMove = new AndantinoMove(a.BoardPosition, this.PlayerPiece);
            alreadymoved = true;
        }
    }

    public class ComputerPlayer : Player
    {
        public const int Default_Search_Depth = 2;
        public int nodescount = 0;
        private TranspositionTable tt;
        MiniMax miniMax;
        bool timeOver = false;
        AndantinoMove temp;
        System.Timers.Timer timer1 = new System.Timers.Timer(10000);

        public ComputerPlayer(string name, ClonableBoard.Pieces p) : this(name, p, Default_Search_Depth)
        {
           
        }

        public ComputerPlayer(string name, ClonableBoard.Pieces p, int searchDepth) : base(name, p)
        {
            this.SearchDepth = searchDepth;
            tt = new TranspositionTable();
            miniMax = new MiniMax(this.PlayerPiece, this, tt);
        }

        public int SearchDepth { get; set; }

        public override void Move(object gameBoard)
        {
            ClonableBoard b = (ClonableBoard)gameBoard;
            timer1.Elapsed += new ElapsedEventHandler(timer1_Tick);
            timer1.Enabled = true; 
            timer1.Start();
            timeOver = false;
            /*currentMove = miniMax.MiniMaxSearch(b, SearchDepth, int.MinValue, int.MaxValue, true, 0, this.PlayerPiece).Item2;
            OnPlayerMoved();*/
             for(int i = 1;i<=SearchDepth&& !timeOver; i++)//ID code
             {
                 temp = miniMax.MiniMaxSearch(b, i, int.MinValue, int.MaxValue, true, 0, this.PlayerPiece).Item2;
                 if (i == SearchDepth)
                 {
                     currentMove = temp;
                     OnPlayerMoved();
                 }
             }
            
            timer1.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timeOver = true;
            currentMove = temp;
            OnPlayerMoved();            
        }
    }

    public class PlayerMovedArgs : System.EventArgs
    {
        protected AndantinoMove move;
        protected Player player;

        public PlayerMovedArgs(AndantinoMove m, Player p) : base()
        {
            this.player = p;
            move = m;
        }

        public AndantinoMove Move
        {
            get { return move; }
        }

        public Player Player
        {
            get { return player; }
        }
    }


}
