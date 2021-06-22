using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace Andantino2
{
    public class ClonableBoard : ICloneable
    {
        public Dictionary<Tuple<int, int>, int> boardStates = new Dictionary<Tuple<int, int>, int>();
        private int[][] jArray;
        private long[, ,] zorbistKey;
        public long hashValue;

        public enum Pieces { White, Black, Blank };

        public const int white = 1;
        public const int black = -1;
        public const int blank = 0;

        private int surroundCount = 0;
        private int movesMade ;
        private int tempRowEval = 0;
        private int tempSurround = 0;

        public int boardSurroundEval = 0;
        public int boardRowEval = 0;
        public int evalscoreID;
        public AndantinoMove evalMove;
        public AndantinoMove lastMove;

        private int qRowScore = 0;
        private int rRowScore = 0;
        private int sRowScore = 0;
        public int rowScore = 0;
        public Pieces winningPiece;

        public bool haveWinner = false;
        private bool whiteWon = false;
        private bool blackWon = false;

        List<Hex> neighborCheckHexes = new List<Hex>();
        List<Hex> insideHexes = new List<Hex>();
        List<Hex> whiteHexes = new List<Hex>();
        List<Hex> blackHexes = new List<Hex>();

        Tuple<int, int> lastCoord = new Tuple<int, int>(9, 9);

        public ClonableBoard(Dictionary<Tuple<int, int>, int> gameStates, long[,,] zorbist, long hashval, int movesNO)
        {
            zorbistKey = zorbist;
            hashValue = hashval;
            evalMove = new AndantinoMove();
            movesMade = movesNO;
            foreach (var state in gameStates)
            {
                boardStates.Add(state.Key, state.Value);
            }
        }

        public ClonableBoard(GameManager gameManager)
        {
            SetInitialGameDictionary();
            Tuple<int, int> center = new Tuple<int, int>(9, 9);
            Hex centerHex = new Hex(9, 9, -18);
            blackHexes.Add(centerHex);
            movesMade = 0;
            boardStates[center] = black;
            zorbistKey = new long[20,20,3];
            long result = 0;
            foreach (var state in boardStates)
            {
                zorbistKey[state.Key.Item1, state.Key.Item2, 0] = gameManager.NextInt64();                
                zorbistKey[state.Key.Item1, state.Key.Item2, 1] = gameManager.NextInt64();
                zorbistKey[state.Key.Item1, state.Key.Item2, 2] = gameManager.NextInt64();               
                if (state.Value == black)
                {
                    result ^= zorbistKey[state.Key.Item1, state.Key.Item2, 2];
                }
                if(state.Value == 0)
                {
                    result ^= zorbistKey[state.Key.Item1, state.Key.Item2, 0];
                }
                if (state.Value == white)
                {
                    result ^= zorbistKey[state.Key.Item1, state.Key.Item2, 1];
                }
            }
            hashValue = result;
        }

        // Functions on Handling the Game.
        public Pieces WinningPiece
        {
            get { return winningPiece; }
            set { winningPiece = value; }
        }

        public bool HaveWinner()
        {
            if (whiteWon) winningPiece = Pieces.White;
            if (blackWon) winningPiece = Pieces.Black;
            haveWinner = whiteWon || blackWon;
            return whiteWon || blackWon;
        }

        private List<Hex> GetWhiteHexes()
        {
            List<Hex> hices = new List<Hex>();
            foreach(var s in boardStates)
            {
                if (s.Value == white)
                {
                    hices.Add(new Hex(s.Key.Item1, s.Key.Item2, -(s.Key.Item1 + s.Key.Item2)));
                }
            }
            return hices;
        }

        private List<Hex> GetBlackHexes()
        {
            List<Hex> hices = new List<Hex>();
            foreach (var s in boardStates)
            {
                if (s.Value == black)
                {
                    hices.Add(new Hex(s.Key.Item1, s.Key.Item2, -(s.Key.Item1 + s.Key.Item2)));
                }
            }
            return hices;
        }

        protected int GetPieceValue(Pieces piece)
        {
            if (piece == Pieces.White)
            {
                return white;
            }
            else if (piece == Pieces.Black)
            {
                return black;
            }
            else
            {
                return blank;
            }
        }

        protected Pieces GetPieceAtPosition(Tuple<int,int> coord)
        {
            if(boardStates[coord] == white)
            {
                return Pieces.White;
            }
            else
            {
                return Pieces.Black;
            }
        }

        public bool IsGameOver(Tuple<int, int> coord, Pieces piece)
        {
           // HaveWinner();
            //return haveWinner = CheckWin(coord,piece) ;
            return HaveWinner();
        }

        public void SetInitialGameDictionary()
        {
            boardStates = new Dictionary<Tuple<int, int>, int>();
            jArray = new int[19][];
            int q, r, s = 0;
            for (r = 0; r < 19; r++)
            {
                for (int n = 0; n < jArray.Length; n++)
                {
                    jArray[n] = SetIndex(n);
                    for (int k = 0; k < jArray[n].Length; k++)
                    {
                        r = n;
                        q = jArray[n][k];
                        s = -(r + q);
                        Tuple<int, int> key = new Tuple<int, int>(q, r);
                        int value = 0;
                        boardStates.Add(key, value);
                    }
                }
            }
        }
        private int[] SetIndex(int n)
        {
            int[] returnarray;
            int i, k = 0;
            if (n == 9)
            {
                returnarray = new int[19];
                for (i = 0; i < 19; i++)
                {
                    returnarray[i] = i;
                }
                return returnarray;
            }
            else if (n > 9)
            {
                returnarray = new int[28 - n];
                for (i = 0; i < returnarray.Length; i++)
                {
                    returnarray[i] = i;
                }
                return returnarray;
            }
            else
            {
                returnarray = new int[10 + n];
                for (i = 9 - n; i < 19; i++)
                {
                    returnarray[k] = i;
                    k++;
                }
                return returnarray;
            }
        }

        public bool CheckWin(Tuple<int,int> coord, Pieces piece)
        {
            rowScore = 0;
            if (piece == Pieces.White)
            {
                Hex selectedHex = new Hex(coord.Item1, coord.Item2, -(coord.Item1 + coord.Item2));
                whiteHexes.Add(selectedHex);
                if (!whiteWon) whiteWon = CheckRow(selectedHex);
                if (!whiteWon)
                {
                    blackHexes = GetBlackHexes();
                    foreach (Hex blackHex in blackHexes)
                    {
                        if (CheckSurrounded(blackHex, black))
                        {
                            if (CalculateWin(neighborCheckHexes, black))
                            {
                                whiteWon = true;
                                insideHexes.Clear();
                                winningPiece = Pieces.White;
                                //neighborCheckHexes.Clear();
                                break;
                            }
                        }
                        else
                        {
                            whiteWon = false;
                            neighborCheckHexes.Clear();
                            insideHexes.Clear();
                        }
                    }
                }
            }
            else
            {
                Hex selectedHex = new Hex(coord.Item1, coord.Item2, -(coord.Item1 + coord.Item2));
                blackHexes.Add(selectedHex);
                //blackWon = CheckNeighbors(whiteHexes, black);
                if (!blackWon) blackWon = CheckRow(selectedHex);
                if (!blackWon)
                {
                    whiteHexes = GetWhiteHexes();
                    foreach (Hex whiteHex in whiteHexes)
                    {
                        if (CheckSurrounded(whiteHex, white))
                        {
                            if (CalculateWin(neighborCheckHexes, white))
                            {
                                blackWon = true;
                                //MessageBox.Show("b");
                                insideHexes.Clear();
                                neighborCheckHexes.Clear();
                                winningPiece = Pieces.Black;
                                break;
                            }
                        }
                        else
                        {
                            blackWon = false;
                            neighborCheckHexes.Clear();
                            insideHexes.Clear();
                        }
                    }
                }
            }
            if (whiteWon) WinningPiece = Pieces.White;
            if(blackWon) WinningPiece = Pieces.Black;
            return (haveWinner =blackWon || whiteWon );
        }

        public bool IsValidMove(Tuple<int, int> hexCoordinate, int movesMade)
        {
            if (boardStates.ContainsKey(hexCoordinate))
            {
                if (boardStates[hexCoordinate] == blank && CheckAdjacent(hexCoordinate, movesMade))
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckAdjacent(Tuple<int, int> hexCoordinate, int movesMade) // To check if there are two adjacent hexex
        {
            int count = 0;
            Hex selectedHex = new Hex(hexCoordinate.Item1, hexCoordinate.Item2, -(hexCoordinate.Item1 + hexCoordinate.Item2));
            List<Hex> neighbhors = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                neighbhors.Add(selectedHex.Neighbor(i));
            }
            foreach (Hex h in neighbhors)
            {
                Tuple<int, int> coord = new Tuple<int, int>(h.q, h.r);
                if (boardStates.ContainsKey(coord))
                {
                    if (boardStates[coord] != 0)
                    {
                        count++;
                    }
                }
            }
            if (movesMade == 0) //IF ITS THE FIRST MOVE
            {
                if (count == 1)
                {
                    return true;
                }
            }
            else
            {
                if (count >= 2)
                {
                    return true;
                }
            }
            return false;

        }

        public void MakeMove(Tuple<int, int> HexCoord, Pieces piece)
        {
            boardStates[HexCoord] = GetPieceValue(piece);
            HashMove(HexCoord, piece);
            movesMade++;
            lastMove = new AndantinoMove(HexCoord, piece);
            bool check =CheckWin(HexCoord, piece);
        }

        public static ClonableBoard.Pieces GetOponentPiece(ClonableBoard.Pieces yourPiece)
        {
            if (yourPiece == ClonableBoard.Pieces.White)
                return ClonableBoard.Pieces.Black;
            else if (yourPiece == ClonableBoard.Pieces.Black)
                return ClonableBoard.Pieces.White;
            else
                throw new Exception("Invalid Piece!");
        }

        public List<Tuple<int, int>> OpenMoves()
        {
            List<Tuple<int, int>> openCoord = new List<Tuple<int, int>>();
            foreach (var s in boardStates)
            {
                if (s.Value == blank)
                {
                    Tuple<int, int> tuple = new Tuple<int, int>(s.Key.Item1, s.Key.Item2);
                    if (CheckAdjacent(tuple, movesMade))
                    {
                        openCoord.Add(tuple);
                    }
                }
            }
            if (openCoord.Count == 0) MessageBox.Show("a");
            return openCoord;
        }

        // Functions for win Calculating Win

        private bool SearchLoop(Hex selectedHex, int lowerbound, int upperbound)
        {
            int value, count = 0;
            int templowerbound = lowerbound;
            int tempupperbound = upperbound;
            Tuple<int, int> coord = new Tuple<int, int>(selectedHex.q, selectedHex.r);
            value = boardStates[coord];
            if (lowerbound > upperbound)
            {
                int temp = lowerbound;
                lowerbound = upperbound;
                upperbound = temp;
            }
            if (lowerbound < 0) lowerbound = -lowerbound;

            for (int k = coord.Item1 - lowerbound; k <= coord.Item1 + upperbound; k++)
            {
                Tuple<int, int> tuple = new Tuple<int, int>(k, coord.Item2);
                if (boardStates.ContainsKey(coord) && boardStates.ContainsKey(tuple))
                {
                    if (boardStates[coord] == boardStates[tuple])
                    {
                        qRowScore++;
                        count++;
                    }
                }
                else
                {
                    qRowScore = 0;
                }
            }
            if (count >= 5) return true;
            count = 0;
            for (int j = coord.Item2 - lowerbound; j <= coord.Item2 + upperbound; j++)
            {
                Tuple<int, int> tuple = new Tuple<int, int>(coord.Item1, j);
                if (boardStates.ContainsKey(coord) && boardStates.ContainsKey(tuple))
                {
                    if (boardStates[coord] == boardStates[tuple])
                    {
                        rRowScore ++;
                        count++;
                    }
                }
                else
                {
                    rRowScore = 0;
                }
            }
            if (count >= 5)
            {
                return true;
            }
            count = 0;
            if (templowerbound < tempupperbound)
            {
                for (int i = templowerbound; i <= tempupperbound; i++)
                {
                    Tuple<int, int> temp = new Tuple<int, int>(coord.Item1 - i, coord.Item2 + i);
                    if (boardStates.ContainsKey(temp))
                    {
                        if (boardStates[temp] == value)
                        {
                            sRowScore++;
                            count++;
                        }
                    }
                    else
                    {
                        sRowScore = 0;
                    }
                }
                if (count >= 5)
                {
                    return true;
                }
            }
            else
            {
                for (int i = templowerbound; i <= tempupperbound; i++)
                {
                    Tuple<int, int> temp = new Tuple<int, int>(coord.Item1 + i, coord.Item2 - i);
                    if (boardStates.ContainsKey(temp))
                    {
                        if (boardStates[temp] == value)
                        {
                            sRowScore++;
                            count++;
                        }
                    }
                    else
                    {
                        sRowScore = 0;
                    }
                }
                if (count >= 5)
                {
                    return true;
                }
            }
           
            count = 0;
            return false;
        }
        private bool CheckRow(Hex selectedHex)
        {           
            bool won = false;
            Tuple<int, int>[] tuples = { Tuple.Create(-4, 0), Tuple.Create(-3,1), Tuple.Create(-2,2), Tuple.Create(-1,3),Tuple.Create(0,4),
            Tuple.Create(-1,3),Tuple.Create(2,-2), Tuple.Create(3,-1),Tuple.Create(4,0)};
            foreach (Tuple<int, int> t in tuples)
            {
                won = SearchLoop(selectedHex, t.Item1, t.Item2);
                rowScore += qRowScore + rRowScore + sRowScore;
                if (won)
                {
                    return won;
                }
                qRowScore = 0;
                rRowScore = 0;
                sRowScore = 0;
            }
            return false;

        }
        private bool CheckNeighbors(List<Hex> HexColor, int colorToCheck)
        {
            if (movesMade > 6)
            {
                int surroundCount = 0;
                foreach (Hex h in HexColor)
                {
                    List<Hex> neighbhors = new List<Hex>();
                    for (int i = 0; i < 6; i++)
                    {
                        neighbhors.Add(h.Neighbor(i));
                    }
                    foreach (Hex neighborHex in neighbhors)
                    {
                        Tuple<int, int> neighborCoord = new Tuple<int, int>(neighborHex.q, neighborHex.r);
                        if (boardStates.ContainsKey(neighborCoord) && boardStates[neighborCoord] == colorToCheck)
                        {
                            surroundCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (surroundCount == 6)
                    {
                        surroundCount = 0;
                        MessageBox.Show("surround");
                        return true;
                    }
                    return false;
                }
            }
            return false;

        }
        private bool CheckSurrounded(Hex selectedHex, int hexColor)
        {
           
            int count = 0;
            for (int i = 0; i < 6; i++)
            {
                if (!neighborCheckHexes.Contains(selectedHex.Neighbor(i)))
                {
                    if (RecursiveCheck(selectedHex, i, hexColor))
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    count++;
                }
            }
            if (count >= 6) return true;
            else return false;
        }

        private bool RecursiveCheck(Hex selectedHex, int direction, int hexColor)
        {
            int reachColor = 0;
            if (hexColor == white) reachColor = black;
            else reachColor = white;
            Hex neighborHex = new Hex();
            neighborHex = selectedHex.Neighbor(direction);
            Tuple<int, int> hexCoord = new Tuple<int, int>(neighborHex.q, neighborHex.r);
            if (!boardStates.ContainsKey(hexCoord)) return false;
            /* else if (gameStates[hexCoord] == 0)
             {
                 return false;
             }*/
            else if (boardStates[hexCoord] == reachColor) return true;
            else
            {
                neighborCheckHexes.Add(selectedHex.Neighbor(direction));
                return CheckSurrounded(selectedHex.Neighbor(direction), hexColor);
            }
        }

        private bool CalculateWin(List<Hex> hices, int hexcolor)
        {
            bool result = true;
            int length = hices.Count;
            if (length == 1)
            {
                return false;
            }
            for (int i = 0; i < length - 1; i++)
            {
                result = CheckSurrounded(hices[i], hexcolor) && CheckSurrounded(hices[i + 1], hexcolor) && result;
            }
            if (result)
            {
                return true;
            }
            else
            {
                //MessageBox.Show("I am here");
                foreach (Hex h in hices)
                {
                    //Tuple<int, int> coord = new Tuple<int, int>(h.q, h.r);
                    // MessageBox.Show(hices.Capacity.ToString());
                    // int maxDistance = movesMade - 1;
                    // if (maxDistance > 19) maxDistance = 19;
                    int maxDistance = 19;
                    result = CheckDistance(h, maxDistance, hexcolor, h.GetCoord(h));
                    // MessageBox.Show("check");
                    if (result == false) break;
                }

            }
            return result;
        }

        private bool CheckDistance(Hex hex, int distance, int hexColor, Tuple<int, int> coord)
        {
            if (!boardStates.ContainsKey(coord)) return false;

            if (distance == 0)
            {
                //MessageBox.Show("reached end");
                return true;
            }
            else
            {
                surroundCount = 0;
            }

            if (!insideHexes.Contains(hex))
            {
                for (int i = 0; i < 6; i++)
                {
                    //MessageBox.Show(" contain");
                    //Tuple<int, int> neighcoord = new Tuple<int, int>(hex.Neighbor(i).q, hex.Neighbor(i).r);
                    if (!insideHexes.Contains(hex))
                    {
                        if (boardStates.ContainsKey(hex.Neighbor(i).GetCoord(hex.Neighbor(i))))
                        {

                            if (boardStates[coord] == boardStates[hex.Neighbor(i).GetCoord(hex.Neighbor(i))])
                            {
                                if (CheckSurrounded(hex.Neighbor(i), hexColor))
                                {
                                    if (CheckDistance(hex.Neighbor(i), distance - 1, hexColor, hex.Neighbor(i).GetCoord(hex.Neighbor(i))))
                                    {
                                        surroundCount++;
                                    }
                                }
                            }
                        }
                        else return false;
                    }

                }
                insideHexes.Add(new Hex(coord.Item1, coord.Item2, -(coord.Item1) - (coord.Item2)));

            }
            if (surroundCount == 6)
            {
                return true;
            }
            return false;
        }

        public int EvaluateSurround(Pieces p)
        {
            List<Hex> trackHexes = new List<Hex>();
            int scoreSurroundWhite = 0;
            int scoreSurroundBlack = 0;
            if(p == Pieces.White)
            {
                foreach(Hex h in GetBlackHexes())
                {
                    scoreSurroundWhite += EvaluateHexSurround(h, black);
                }
            }
            else if(p == Pieces.Black)
            {
                foreach (Hex h in GetWhiteHexes())
                {
                    scoreSurroundBlack += EvaluateHexSurround(h, white);    
                }
            }
            if (p == Pieces.White)
                return scoreSurroundWhite - scoreSurroundBlack;
            else
                return scoreSurroundBlack - scoreSurroundWhite;
        }

        private int EvaluateHexSurround(Hex h, int colorValue)
        {
            int tempScore = 0;
            tempSurround = 0;
            if (colorValue == white) colorValue = black;
            else colorValue = white;
            for(int i = 0; i < 6; i++)
            {
                tempSurround = 0;
                tempScore += RecursiveEvaluateSurround(h, i, colorValue);
            }
            return tempScore;
        }

        private int RecursiveEvaluateSurround(Hex h,int direction, int colorValue)
        {
            Hex neighborHex = new Hex();
            neighborHex = h.Neighbor(direction);
            Tuple<int, int> hexCoord = new Tuple<int, int>(neighborHex.q, neighborHex.r);
            if (!boardStates.ContainsKey(hexCoord))
            {
                tempSurround = 0;
                return tempSurround;
            }
            else if (boardStates[hexCoord] == colorValue)
            {
                tempSurround += 1;
            }
            else
            {
                tempSurround+= RecursiveEvaluateSurround(neighborHex, direction, colorValue);
            }
            return tempSurround;
        }

        public void Eval(Pieces p)
        {
            if (HaveWinner())
            {
                if (WinningPiece == p)
                {
                    evalscoreID = int.MaxValue;
                }
                else
                {
                    evalscoreID = int.MinValue ;
                }
            }
            else
            {
                int x = EvaluateRow(p);
                int y = EvaluateSurround(p) / 6;
                evalscoreID = x+y;
            }
           
        }
        public int EvaluateRow(Pieces p)
        {
            int rowWhite2 = 0;
            int rowWhite3 = 0;
            int rowWhite4 = 0;
            int whiteWon = 0;
            int whitefasak = 0 ;
            List<Hex> row1 = new List<Hex>();
            List<Hex> row2Possibility = new List<Hex>();
            List<Hex> row3Possibility = new List<Hex>();
            List<Hex> row4Possibility = new List<Hex>();
            List<Point> white4 = new List<Point>();
            List<Point> white3 = new List<Point>();
            int rowBlack2 = 0;
            int rowBlack3 = 0;
            int rowBlack4 = 0;
            int blackWon = 0;
            int blackfask = 0;
            List<Hex> row1Black = new List<Hex>();
            List<Hex> row2BlackPossibility = new List<Hex>();
            List<Hex> row3BlackPossibility = new List<Hex>();
            List<Hex> row4BlackPossibility = new List<Hex>();
            List<Point> black4 = new List<Point>();
            List<Point> black3 = new List<Point>();
            whiteHexes = GetWhiteHexes();
            for (int i = 0; i < 6; i++)
            {
                foreach (Hex h in whiteHexes)
                {
                    Tuple<int, int> hexCoord = new Tuple<int, int>(h.q, h.r);
                    Hex neighHex = h.Neighbor(i);
                    row2Possibility.Add(h);
                    row3Possibility.Add(h);
                    row4Possibility.Add(h);
                    Tuple<int, int> neighCoord = new Tuple<int, int>(neighHex.q, neighHex.r);
                    if (boardStates.ContainsKey(neighCoord))
                    {
                        if (boardStates[hexCoord] == boardStates[neighCoord])
                        {
                            row2Possibility.Add(neighHex);
                            row3Possibility.Add(neighHex);
                            row4Possibility.Add(neighHex);
                            if (CheckRowPossible(row2Possibility, i, 3))
                            {
                                rowWhite2++;
                            }
                            Hex neigh2Hex = neighHex.Neighbor(i);
                            Tuple<int, int> neigh2Coord = new Tuple<int, int>(neigh2Hex.q, neigh2Hex.r);
                            if (boardStates.ContainsKey(neigh2Coord))
                            {
                                if (boardStates[neigh2Coord] == boardStates[neighCoord])
                                {
                                    row3Possibility.Add(neigh2Hex);
                                    row4Possibility.Add(neigh2Hex);
                                    if (CheckRowPossible(row3Possibility, i, 2))
                                    {
                                        white3.Add(new Point(neigh2Coord.Item1, neigh2Coord.Item2));
                                        rowWhite3++;
                                    }
                                    Hex neigh3Hex = neigh2Hex.Neighbor(i);
                                    Hex winningHex = neigh3Hex.Neighbor(i);
                                    Tuple<int, int> neigh3Coord = new Tuple<int, int>(neigh3Hex.q, neigh3Hex.r);
                                    if (boardStates.ContainsKey(neigh3Coord))
                                    {
                                        if (boardStates[neigh3Coord] == boardStates[neigh2Coord])
                                        {
                                            row4Possibility.Add(neigh3Hex);
                                            if (CheckRowPossible(row4Possibility, i, 1))
                                            {
                                                white4.Add(new Point(neigh3Coord.Item1, neigh3Coord.Item2));
                                                rowWhite4++;
                                                if (ValidWinningMove(winningHex, Pieces.White)) whiteWon = 10000;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
            whitefasak += CheckFeatureParallel(white3, white4);
            white4.Clear();
            white3.Clear();
            row2Possibility.Clear();
            row3Possibility.Clear();
            row4Possibility.Clear();
            blackHexes = GetBlackHexes();
            for (int i = 0; i < 6; i++)
            {
                foreach (Hex bh in blackHexes)
                {
                    Tuple<int, int> hexCoordB = new Tuple<int, int>(bh.q, bh.r);
                    Hex neighHex = bh.Neighbor(i);
                    row2Possibility.Add(bh);
                    row3Possibility.Add(bh);
                    row4Possibility.Add(bh);
                    Tuple<int, int> neighCoord = new Tuple<int, int>(neighHex.q, neighHex.r);
                    if (boardStates.ContainsKey(neighCoord))
                    {
                        if (boardStates[hexCoordB] == boardStates[neighCoord])
                        {
                            row2Possibility.Add(neighHex);
                            row3Possibility.Add(neighHex);
                            row4Possibility.Add(neighHex);
                            if (CheckRowPossible(row2Possibility, i, 3))
                            {
                                rowBlack2++;
                            }
                            Hex neigh2Hex = neighHex.Neighbor(i);
                            Tuple<int, int> neigh2Coord = new Tuple<int, int>(neigh2Hex.q, neigh2Hex.r);
                            if (boardStates.ContainsKey(neigh2Coord))
                            {
                                if (boardStates[neigh2Coord] == boardStates[neighCoord])
                                {
                                    row3Possibility.Add(neigh2Hex);
                                    row4Possibility.Add(neigh2Hex);
                                    if (CheckRowPossible(row3Possibility, i, 2))
                                    {
                                        black3.Add(new Point(neigh2Coord.Item1, neigh2Coord.Item2));
                                        rowBlack3++;
                                    }
                                    Hex neigh3Hex = neigh2Hex.Neighbor(i);
                                    Tuple<int, int> neigh3Coord = new Tuple<int, int>(neigh3Hex.q, neigh3Hex.r);
                                    Hex winningHex = neigh3Hex.Neighbor(i);
                                    if (boardStates.ContainsKey(neigh3Coord))
                                    {
                                        if (boardStates[neigh3Coord] == boardStates[neigh2Coord])
                                        {
                                            row4Possibility.Add(neigh3Hex);
                                            if (CheckRowPossible(row4Possibility, i, 1))
                                            {
                                                black4.Add(new Point(neigh3Coord.Item1, neigh3Coord.Item2));
                                                rowBlack4++;
                                                if (ValidWinningMove(winningHex, Pieces.Black)) blackWon = 10000;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
               
            }
            blackfask += CheckFeatureParallel(black3, black4);
            black4.Clear();
            black3.Clear();
            row2BlackPossibility.Clear();
            row3BlackPossibility.Clear();
            row4BlackPossibility.Clear();
            int scoreWhite = rowWhite2 + 2 * rowWhite3 + 4 * rowWhite4 + whiteWon + whitefasak * 1000;
            int scoreBlack = rowBlack2 + 2 * rowBlack3 + 4 * rowBlack4 + blackWon + blackfask * 1000;
            if (p == Pieces.White) return scoreWhite - scoreBlack;
            else return scoreBlack - scoreWhite;
        }

        private bool CheckRowPossible(List<Hex> hices, int direction, int distance)
        {
            Hex endHex = hices[hices.Count - 1];
            Tuple<int, int> startcoord = new Tuple<int, int>(endHex.q, endHex.r);
            for (int i = 0; i < distance; i++)
            {
                Hex neighHex = endHex.Neighbor(direction);
                Tuple<int, int> neighCoord = new Tuple<int, int>(neighHex.q, neighHex.r);
                if (boardStates.ContainsKey(neighCoord))
                {
                    if (boardStates[neighCoord] == 0)
                    {
                        endHex = neighHex;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
            return true;
        }

        private bool ValidWinningMove(Hex winningHex, Pieces p)
        {
            int count = 0;
            if(p==Pieces.White && movesMade % 2 != 0)
            {
                for(int i = 0; i < 6; i++)
                {
                    Tuple<int, int> coord = new Tuple<int, int>(winningHex.Neighbor(i).q, winningHex.Neighbor(i).r);
                    if (boardStates.ContainsKey(coord))
                    {
                        if (boardStates[coord] != 0)
                        {
                            count++;
                        }
                    }
                }
            }
            if (count >= 2) return true;
            if (p == Pieces.Black && movesMade % 2 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    Tuple<int, int> coord = new Tuple<int, int>(winningHex.Neighbor(i).q, winningHex.Neighbor(i).r);
                    if (boardStates.ContainsKey(coord))
                    {
                        if (boardStates[coord] != 0)
                        {
                            count++;
                        }
                    }
                }
            }
            if (count >= 2) return true;
            return false;
        }

        public int CheckFeatureParallel(List<Point> points3,List<Point> points4)
        {
            int count = 0;
            foreach (Point p in points4)
            {
                foreach (Point q in points3)
                {
                    if ((p.x == q.x) && ((p.y == q.y + 1) || (p.y == q.y - 1)))
                    {
                        count++;
                    }
                    if ((p.y == q.y) && ((p.x == q.x + 1) || (p.x == q.x - 1)))
                    {
                        count++;
                    }
                    if (-(p.y+p.x) == -(q.y+q.x) && ((p.x == q.x + 1) || (p.x == q.x - 1)))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        // Zorbist Hash Function

        public void HashMove(Tuple<int,int> movecoord,Pieces p)
        {
            if (p == Pieces.White)
            {
                hashValue ^= zorbistKey[movecoord.Item1, movecoord.Item2, 1];
            }                
            else
                hashValue ^= zorbistKey[movecoord.Item1, movecoord.Item2, 2];
           // MessageBox.Show("new hash value" + hashValue.ToString());
        }


        #region ICloneable Members
        public object Clone()
        {
            ClonableBoard b = new ClonableBoard(boardStates,zorbistKey,hashValue, movesMade);
            return b;
        }

        #endregion
    }

    public class InvalidMoveException : Exception
        {
            public InvalidMoveException()
                : base()
            {
            }

            public InvalidMoveException(string msg)
                : base(msg)
            {
            }
        }

        

        
}
    

