using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace Andantino2
{
    
    class MiniMax
    {
        ClonableBoard.Pieces rootPiece;
        private ComputerPlayer maxPlayer;
        public AndantinoMove bestMove;
        private TranspositionTable tt;
        private List<AndantinoMove> killerMoves;

        public MiniMax(ClonableBoard.Pieces p, ComputerPlayer computer, TranspositionTable table)
        {
            bestMove = new AndantinoMove();
            this.rootPiece = p;
            this.maxPlayer = computer;
            this.tt = table;
            killerMoves = new List<AndantinoMove>();
        }
        public Tuple<int,AndantinoMove> MiniMaxSearch(ClonableBoard b, int depth , int alpha ,int beta, bool maximisingPlayer,int treedepth,ClonableBoard.Pieces p)// To sort out eval function
        {
             int olda = alpha;
             int oldb = beta;
             TableEnrtry retrievedEntry = tt.GetEntry(b);
              if (retrievedEntry.depth >= depth)
              {
                if (retrievedEntry.flag == TableEnrtry.flags.exact) return new Tuple<int, AndantinoMove>(retrievedEntry.eval,retrievedEntry.ttMove);
                  else if(retrievedEntry.flag == TableEnrtry.flags.upperBound)
                  {
                      if (alpha < retrievedEntry.eval) alpha = retrievedEntry.eval;
                  }
                  else if(retrievedEntry.flag == TableEnrtry.flags.lowerBound)
                  {
                      if (beta > retrievedEntry.eval) beta = retrievedEntry.eval;
                  }
                  if (alpha >= beta)
                  {
                      return new Tuple<int, AndantinoMove>(retrievedEntry.eval, retrievedEntry.ttMove);
                  }
              }
            if (b.haveWinner)
            {
                if (b.WinningPiece == maxPlayer.PlayerPiece)
                {
                    return new Tuple<int, AndantinoMove>(int.MaxValue- treedepth, null);
                }
                else
                {
                    return new Tuple<int, AndantinoMove>(int.MinValue+ treedepth,null);
                }
            }
            else
            {
                if (depth == 0)
                {
                    if(maxPlayer.PlayerPiece == ClonableBoard.Pieces.White) b.Eval(p);
                    return new Tuple<int, AndantinoMove>(b.evalscoreID, null);
                }
                else
                {
                    TableEnrtry.flags storeFlag;
                    if (maximisingPlayer)
                    {
                        int value = int.MinValue;
                        AndantinoMove ttbestMove = new AndantinoMove();
                        List<Tuple<int, int>> openPositions = b.OpenMoves();
                        List<ClonableBoard> children = new List<ClonableBoard>();
                        foreach (Tuple<int, int> i in openPositions)
                        {
                            maxPlayer.nodescount++;
                            ClonableBoard clonedboard = (ClonableBoard)b.Clone();
                            clonedboard.MakeMove(i, p);
                            clonedboard.evalMove.movecoord = i;
                            clonedboard.evalMove.movingPiece = p;
                            children.Add(clonedboard);
                        }
                        children = MoveOrdering(children,true,p,b,depth,treedepth);
                        ttbestMove.movecoord = children[0].evalMove.movecoord;
                        foreach (ClonableBoard board in children)
                        {
                            int newvalue = MiniMaxSearch(board, depth - 1, alpha, beta, false, treedepth + 1, ClonableBoard.GetOponentPiece(p)).Item1;
                            if (treedepth == 0)
                            {
                                if (newvalue > value)
                                {
                                    bestMove.movecoord = board.evalMove.movecoord;
                                    bestMove.movingPiece = p;
                                }
                            }
                            if (newvalue > value)
                            {
                                ttbestMove.movecoord = board.evalMove.movecoord;
                                ttbestMove.movingPiece = p;
                                value = newvalue;
                            }
                            if (value > alpha)
                            {
                                alpha = value;
                            }
                            if (alpha >= beta)
                            {
                                killerMoves.Add(ttbestMove);
                                break;
                            }
                        }                           
                        if (value <= olda) storeFlag = TableEnrtry.flags.upperBound;
                        else if (value >= beta) storeFlag = TableEnrtry.flags.lowerBound;
                        else storeFlag = TableEnrtry.flags.exact;
                        tt.StoreEntry(new TableEnrtry(b.hashValue, depth, value, storeFlag,ttbestMove));
                        return new Tuple<int, AndantinoMove>(value,ttbestMove);
                    }
                    else
                    {
                        int value = int.MaxValue;
                        List<Tuple<int, int>> openPositions = b.OpenMoves();
                        AndantinoMove ttbestMove = new AndantinoMove();
                        List<ClonableBoard> children = new List<ClonableBoard>();
                        foreach (Tuple<int, int> i in openPositions)
                        {
                            maxPlayer.nodescount++;
                            ClonableBoard clonedboard = (ClonableBoard)b.Clone();
                            clonedboard.MakeMove(i, p);
                            clonedboard.evalMove.movecoord = i;
                            clonedboard.evalMove.movingPiece = p;
                            children.Add(clonedboard);
                        }
                        //children = SortBoardMin(children, p);
                        children = MoveOrdering(children,false,p,b,depth,treedepth);
                        ttbestMove.movecoord = children[0].evalMove.movecoord;
                        foreach (ClonableBoard board in children)
                        {
                            maxPlayer.nodescount++;
                            int newvalue = MiniMaxSearch(board, depth - 1, alpha, beta, true, treedepth + 1, ClonableBoard.GetOponentPiece(p)).Item1;
                            if (newvalue < value)
                            {
                                value = newvalue;
                                ttbestMove.movecoord = board.evalMove.movecoord;
                                ttbestMove.movingPiece = p;
                            }
                            if (beta > value) beta = value;
                            if (alpha >= beta)
                            {
                                killerMoves.Add(ttbestMove);
                                break;
                            }
                        }
                        if (value <= olda) storeFlag = TableEnrtry.flags.upperBound;
                        else if (value >= beta) storeFlag = TableEnrtry.flags.lowerBound;
                        else storeFlag = TableEnrtry.flags.exact;
                        tt.StoreEntry(new TableEnrtry(b.hashValue, depth, value, storeFlag,ttbestMove));
                        return new Tuple<int, AndantinoMove>(value, ttbestMove);
                    }

                }
            }
        }

        private List<ClonableBoard> SortBoardMax(List<ClonableBoard> boards, ClonableBoard.Pieces p)
        {
            List<ClonableBoard> sortedBoards = new List<ClonableBoard>();
            foreach(ClonableBoard b in boards)
            {
                b.Eval(p);
            }
            sortedBoards = boards.OrderByDescending(n => n.evalscoreID).ToList();
            return sortedBoards;
        }
        private List<ClonableBoard> SortBoardMin(List<ClonableBoard> boards, ClonableBoard.Pieces p)
        {
            List<ClonableBoard> sortedBoards = new List<ClonableBoard>();
            foreach (ClonableBoard b in boards)
            {
                b.Eval(p);
            }
            sortedBoards = boards.OrderBy(n => n.evalscoreID).ToList();
            return sortedBoards;
        }

        private List<ClonableBoard> MoveOrdering(List<ClonableBoard> boards,bool MaxNode, ClonableBoard.Pieces p,ClonableBoard board,int depth,int treeDepth)
        {
            List<ClonableBoard> temp = new List<ClonableBoard>(); // Human Knowledge Moves
            List<ClonableBoard> sortedBoards = new List<ClonableBoard>();// Transposition Table moves
            List<ClonableBoard> winBoards = new List<ClonableBoard>(); //  Won Boards
            List<ClonableBoard> killerMove = new List<ClonableBoard>(); // Killer Move Boards
            temp.AddRange(boards); 
            foreach (ClonableBoard b in boards)
            {
                if (b.haveWinner)
                {
                    winBoards.Add(b);
                }
                else
                {
                    if (tt.CheckBoardInTable(b))
                    {
                        sortedBoards.Add(b);
                        temp.Remove(b);
                    }
                    else if (killerMoves.Contains(b.lastMove))
                    {
                        killerMove.Add(b);
                        temp.Remove(b);
                    }
                }
                
            }
            sortedBoards = SortBoardMax(sortedBoards, p);
            sortedBoards.AddRange(killerMove);
            sortedBoards.AddRange(temp);
            winBoards.AddRange(sortedBoards);
            return winBoards;
        }

 
    }

   public struct TableEnrtry
    {
        public TableEnrtry(long hashval,int depth,int eval, flags flag, AndantinoMove ttmove)
        {
            this.hashval = hashval;
            this.depth = depth;
            this.eval = eval;
            this.flag = flag;
            this.ttMove = ttmove;
        }
        public long hashval;
        public int depth;
        public int eval ;
        public enum flags
        {
            exact,lowerBound,upperBound
        };
        public flags flag;
        public AndantinoMove ttMove;
    };

    public class TranspositionTable
    {
        private static int size = (int)Math.Pow(2, 12);
        Hashtable hashtable = new Hashtable(size);
        public void StoreEntry(TableEnrtry entry)
        {
            int hashkey = (int)entry.hashval % size;
            hashtable[hashkey] = entry;
        }

        public TableEnrtry GetEntry(ClonableBoard board)
        {
            int hashkey = (int)board.hashValue % size;
            if(hashtable[hashkey] == null)
            {
                return new TableEnrtry(board.hashValue,-1,-99,TableEnrtry.flags.exact, new AndantinoMove());
            }
            else
            {
                return (TableEnrtry)hashtable[hashkey];
            }
        }

        public bool CheckBoardInTable(ClonableBoard board)
        {
            int hashkey = (int)board.hashValue % size;
            if (hashtable[hashkey] == null) return false;
            else return true;
        }
    }

}
