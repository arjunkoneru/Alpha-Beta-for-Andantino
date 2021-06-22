using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andantino2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            /*Form2 form = new Form2();
            Player p1 = new HumanPlayer("Joe", ClonableBoard.Pieces.White, form);
            Player p2 = new HumanPlayer("Sai", ClonableBoard.Pieces.Black, form);
            Player p3 = new ComputerPlayer("c1", ClonableBoard.Pieces.Black,5);
            Player p4 = new ComputerPlayer("c2", ClonableBoard.Pieces.White,6);
            form.AddPlayer(p4);//White Player
            form.AddPlayer(p2);// Black Player
            Application.Run(form);*/
            Form1 form = new Form1();
            Application.Run(form);
        }
    }
}
