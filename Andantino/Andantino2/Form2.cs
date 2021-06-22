using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Linq.Expressions;


namespace Andantino2
{
    public partial class Form2 : Form
    {
        public event HexClickHandler HexClicked;
        private GameManager gameManager;
        private List<Player> players;
        private Thread mainThread = null;
        private Thread playerThread;
        private AndantinoMove lastMove = null;
        public static int movesMade = 0;
        private GFX engine;
        private bool gameOver = false;
        public static bool doOnce = false;
        public static Label turnLabel;
        public delegate void delUpdateLabel(string text);
        private bool computerThinking = false;

        public Form2()
        {
            InitializeComponent();
            this.FormClosed += new FormClosedEventHandler(Form2_FormClosed);
            movesMade = 0;
        }

        public void AddPlayer(Player p)
        {
            if (players == null)
                players = new List<Player>();

            if (this.players.Count > 2)
                throw new Exception("Must have only 2 players");

            if (players.Count == 1)
                if (players[0].PlayerPiece == p.PlayerPiece)
                    throw new Exception("Players Must have different board pieces");

            players.Add(p);
        }

        public void RemoveAllPlayers()
        {
            players.Clear();
        }

        void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            // clean up threads

            if (mainThread != null)
                mainThread.Abort();

            if (playerThread != null)
                playerThread.Abort();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics toPass = panel1.CreateGraphics();
            engine = new GFX(toPass);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LaunchGame();
        }

        private void LaunchGame()
        {
            if (players.Count != 2)
                throw new Exception("There must be two players for this game!");
            gameManager = new GameManager(players[0].PlayerPiece, players[1].PlayerPiece);
            this.Show();
            mainThread = new Thread(new ThreadStart(ProcessPlayerMoves));
            mainThread.Start();
        }

        private String Number2String(int number, bool isCaps)
        {
            Char c = (Char)((isCaps ? 65 : 97) + (number - 1));
            return c.ToString();
        }

        private void ProcessPlayerMoves()
        {
            while (!doOnce) ;
            while (!gameOver)
            {
                for (int i =0; i < players.Count; i++)
                {
                    Player p = players[i];
                    label1.Invoke((MethodInvoker)delegate {
                        // Running on the UI thread
                        label1.Text = p.PlayerPiece + " Turn";
                    });
                    Thread.Sleep(200);
                    if (p.GetType() == typeof(ComputerPlayer)) computerThinking = true;
                    AndantinoMove playerMove = GetMoveForPlayer(p);
                    gameManager.MakeMove(new AndantinoMove(playerMove.movecoord, p.PlayerPiece));
                    if (p.GetType() == typeof(ComputerPlayer)) computerThinking = false;
                    if (p.GetType() != typeof(HumanPlayer))
                    {
                        AndantinoBoardClickedEventArgs a = new AndantinoBoardClickedEventArgs(playerMove.movecoord);
                        if (HexClicked != null)
                        {

                            HexClicked(this, a);
                        }
                        Hex selectedHex = new Hex(playerMove.movecoord.Item1, playerMove.movecoord.Item2, -(playerMove.movecoord.Item1
                            + playerMove.movecoord.Item2));                       
                            if (playerMove.movingPiece == ClonableBoard.Pieces.Black) GFX.DrawBlack(GFX.lay.HexToPixel(selectedHex));
                            else GFX.DrawWhite(GFX.lay.HexToPixel(selectedHex));
                      /*  if (-playerMove.movecoord.Item1 - playerMove.movecoord.Item2 + 26 > 9)
                        {
                            int label = 9 - (-playerMove.movecoord.Item1 - playerMove.movecoord.Item2 + 26 - 9);
                            MessageBox.Show((Number2String(label, false)) + " " + (playerMove.movecoord.Item1 + 1).ToString());
                        }
                        else
                        {
                            int label = 9 + (9 - (-playerMove.movecoord.Item1 - playerMove.movecoord.Item2 + 26));
                            MessageBox.Show((Number2String(label, false)) + " " + (playerMove.movecoord.Item1 + 1).ToString());
                        }*/
                    }
                    movesMade++;
                    if (gameManager.IsGameOver(playerMove.movecoord,p.PlayerPiece))
                    {
                        gameOver = true;
                        MessageBox.Show(gameManager.board.winningPiece.ToString() + " Won");
                        FinishGame();
                    }
                }
            }
        }

        private void FinishGame()
        {
            Thread homethread = new Thread(new ThreadStart(GoHome));
            homethread.Start();
            this.Invoke((MethodInvoker)delegate () {
                this.Close();
            });
            mainThread.Abort();
            this.panel1.MouseClick -= this.panel1_MouseClick;
        }

        private void GoHome()
        {
            Form1 home = new Form1();
            Application.Run(home);
        }

        private AndantinoMove GetMoveForPlayer(Player p)
        {
            lastMove = null;
            playerThread = new Thread(p.Move);
            playerThread.Start(gameManager.GameBoard);
            p.PlayerMoved += new PlayerMovedHandler(player_PlayerMoved);
            while (lastMove == null) ;
            p.PlayerMoved -= player_PlayerMoved;
            playerThread.Abort();
            return p.CurrentMove;
        }

        private void player_PlayerMoved(object sender, PlayerMovedArgs args)
        {
            lastMove = args.Move; 
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!computerThinking)
            {
                Point loc = new Point(e.X, e.Y);
                FractionalHex fh = new FractionalHex();
                fh = GFX.lay.PixelToHex(loc);
                Hex selectedHex = new Hex();
                selectedHex = fh.HexRound();
                Tuple<int, int> hexCoordinate = new Tuple<int, int>(selectedHex.q, selectedHex.r);
                if (this.gameManager.board.IsValidMove(hexCoordinate, movesMade))
                {
                    AndantinoBoardClickedEventArgs a = new AndantinoBoardClickedEventArgs(hexCoordinate);
                    if (HexClicked != null)
                    {

                        HexClicked(this, a);
                    }
                    if (movesMade % 2 == 0)
                    {
                        GFX.DrawWhite(loc);

                    }
                    else GFX.DrawBlack(loc);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            doOnce = true;
            timer1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread homethread = new Thread(new ThreadStart(GoHome));
            this.Invoke((MethodInvoker)delegate () {
                this.Close();
            });
            homethread.Start();
            mainThread.Abort();
            this.panel1.MouseClick -= this.panel1_MouseClick;
        }
    }


    public delegate void HexClickHandler(object sender, AndantinoBoardClickedEventArgs args);

    public class AndantinoBoardClickedEventArgs : System.EventArgs
    {
        protected Tuple<int,int> boardPosition;

        public AndantinoBoardClickedEventArgs(Tuple<int,int> coord) : base()
        {
            boardPosition = coord;
        }

        public Tuple<int,int> BoardPosition
        {
            get { return boardPosition; }
        }
    }
}
