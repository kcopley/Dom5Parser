using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dom5Edit
{
    public partial class Startup: UserControl
    {
        public Startup()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        Importer i;

        private void startButton_Click(object sender, EventArgs e)
        {
            i = new Importer();
            i.Run(_folderPath.Text);
        }

        private void mergeButton_Click(object sender, EventArgs e)
        {
            if (i != null) i.Merge();
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (i != null) i.Export(_folderPath.Text);
        }
    }
}
