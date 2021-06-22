using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Andantino2
{
    class GFX
    { 
        public static Graphics gObject;
        private Pen p;

        public Dictionary<Tuple<int, int>, int> initialGameDictionary;

        public static Layout lay;
        private Hex singleHex;
        private List<Point> corners;
        private Orientation pointy;
        private Point size;
        private Point center;
        private int[][] jArray;
        private static FractionalHex fh;

        public GFX(Graphics g)
        {
            gObject = g;
            p = new Pen(Color.Brown, 5);
            DrawHexagonalHex(p);
        }

        public void DrawHexagonalHex(Pen p)
        {
            initialGameDictionary = new Dictionary<Tuple<int, int>, int>();
            jArray = new int[19][];
            int q, r, s = 0;
            pointy = new Orientation(Math.Sqrt(3.0), Math.Sqrt(3.0) / 2.0, 0.0, 3.0 / 2.0, Math.Sqrt(3.0) / 3.0, -1.0 / 3.0, 0.0, 2.0 / 3.0, 0.5);

            size = new Point(20, 20);
            center = new Point(100, 100);
            lay = new Layout(pointy, size, center);
            //MessageBox.Show(corners[1].x.ToString());
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
                        Tuple<int,int> key = new Tuple<int, int>(q, r);
                        int value = 0;
                        initialGameDictionary.Add(key, value);
                        singleHex = new Hex(q, r, s);
                        corners = lay.PolygonCorners(singleHex);
                        PointF[] cornerPoints = new PointF[6];
                        cornerPoints = ConvertCorner(corners);
                        gObject.DrawPolygon(p, cornerPoints);
                        if(q==9 && r == 9)
                        {
                            DrawPlayerMove(lay.HexToPixel(singleHex), Brushes.Black);
                        }
                    }
                }
            }
        }

        private static PointF[] ConvertCorner(List<Point> points)
        {
            PointF[] cornerPoints = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                cornerPoints[i].X = (float)points[i].x;
                cornerPoints[i].Y = (float)points[i].y;
            }
            return cornerPoints;
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

        public void HandleMouseClick(Point p)
        {
            fh = lay.PixelToHex(p);
            Hex selectedHex = new Hex();
            selectedHex = fh.HexRound();
            int checkRow = selectedHex.q;
            int checkCol = selectedHex.r;
            Tuple<int, int> checkState = new Tuple<int, int>(checkRow, checkCol);
            if (initialGameDictionary.ContainsKey(checkState))
            {
                MessageBox.Show(selectedHex.q.ToString());
            }
        }
        
        public static void DrawWhite(Point loc)
        {
            DrawPlayerMove(loc, Brushes.DarkBlue);
        }

        public static void DrawBlack(Point loc)
        {
            DrawPlayerMove(loc, Brushes.Black);
        }

        public static void DrawPlayerMove(Point loc, Brush brush)
        {
            lock (gObject)
            {
                List<Point> polygonPoints = new List<Point>();
                fh = lay.PixelToHex(loc);
                Hex selectedHex = new Hex();
                selectedHex = fh.HexRound();
                polygonPoints = lay.PolygonCorners(selectedHex);
                PointF[] cornerPoints = new PointF[6];
                cornerPoints = ConvertCorner(polygonPoints);
                PointF[] boundary = new PointF[6];
                boundary = HexToColorPoints(cornerPoints);
                gObject.FillPolygon(brush, cornerPoints);
            }
           
        }

        public static PointF[] HexToColorPoints(PointF[] points)
        {
            points[0].X -= 2.5f;
            points[1].X -= 2.5f;
            points[2].Y += 2.5f;
            points[3].X += 2.5f;
            points[4].X += 2.5f;
            points[5].Y -= 2.5f;
            return new PointF[]{
                points[0],points[1],points[2],points[3],points[4],points[5] };
        }
    }
}
