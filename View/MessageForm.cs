using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeoParser.View
{
    public partial class MessageForm : Form
    {
        public MessageForm(string content, string title)
        {
            InitializeComponent();
            this.Text = title;
            label1.Text = content;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
