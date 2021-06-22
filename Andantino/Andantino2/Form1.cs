using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andantino2
{
    public partial class Form1 : Form
    {
       
        public Form1()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            Player p1 = new HumanPlayer("Joe", ClonableBoard.Pieces.White, form);
            Player p2 = new ComputerPlayer("c1", ClonableBoard.Pieces.Black, 7);
            form.AddPlayer(p1);
            form.AddPlayer(p2);
            this.Hide();
            form.ShowDialog();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            Player p1 = new HumanPlayer("Joe", ClonableBoard.Pieces.Black, form);
            Player p2 = new ComputerPlayer("c1", ClonableBoard.Pieces.White, 8);
            form.AddPlayer(p2);
            form.AddPlayer(p1);
            this.Hide();
            form.ShowDialog();
            this.Close();
        }
    }
}
