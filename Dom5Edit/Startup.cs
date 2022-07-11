using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Dom5Edit
{
    public partial class Startup : Form
    {
        public Startup()
        {
            InitializeComponent();
            _folderPath.Text = Path.GetFullPath(Path.Combine(Application.UserAppDataPath, @"..\..\..\Dominions5\mods"));
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        ModManager i;

        private void startButton_Click(object sender, EventArgs e)
        {
            if (i != null)
            {
                i._ModName = !string.IsNullOrEmpty(modFileName.Text) ? modFileName.Text : "merged-mod";
                i.DisabledNations.Clear();
                foreach (var c in eaNations.CheckedItems)
                {
                    i.DisabledNations.Add(c.ToString());
                }
                foreach (var c in maNations.CheckedItems)
                {
                    i.DisabledNations.Add(c.ToString());
                }
                foreach (var c in laNations.CheckedItems)
                {
                    i.DisabledNations.Add(c.ToString());
                }

                i.Merge();
                i.Export(_folderPath.Text);
                label7.Text = i._ModName + " successfully exported";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void vanillaNations_SelectedIndexChanged(object sender, EventArgs e)
        {
            string curItem = eaNations.SelectedItem.ToString();
            if (curItem.Equals("TOGGLE ALL"))
            {
                int index = eaNations.FindString("TOGGLE ALL");
                bool state = eaNations.GetItemChecked(index);
                for (int i = 0; i < eaNations.Items.Count; i++)
                {
                    if (i == index) continue;
                    eaNations.SetItemChecked(i, state);
                }
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void maNations_SelectedIndexChanged(object sender, EventArgs e)
        {
            string curItem = maNations.SelectedItem.ToString();
            if (curItem.Equals("TOGGLE ALL"))
            {
                int index = maNations.FindString("TOGGLE ALL");
                bool state = maNations.GetItemChecked(index);
                for (int i = 0; i < maNations.Items.Count; i++)
                {
                    if (i == index) continue;
                    maNations.SetItemChecked(i, state);
                }
            }
        }

        private void laNations_SelectedIndexChanged(object sender, EventArgs e)
        {
            string curItem = laNations.SelectedItem.ToString();
            if (curItem.Equals("TOGGLE ALL"))
            {
                int index = laNations.FindString("TOGGLE ALL");
                bool state = laNations.GetItemChecked(index);
                for (int i = 0; i < laNations.Items.Count; i++)
                {
                    if (i == index) continue;
                    laNations.SetItemChecked(i, state);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (i != null)
            {
                i.ExportMagicPaths(_folderPath.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            i = new ModManager();
            foreach (var c in eaNations.CheckedItems)
            {
                i.DisabledNations.Add(c.ToString());
            }
            foreach (var c in maNations.CheckedItems)
            {
                i.DisabledNations.Add(c.ToString());
            }
            foreach (var c in laNations.CheckedItems)
            {
                i.DisabledNations.Add(c.ToString());
            }
            i._ModName = !string.IsNullOrEmpty(modFileName.Text) ? modFileName.Text : "merged-mod";
            List<string> files = new List<string>();
            foreach (var modFile in modFiles.CheckedItems)
            {
                if (modFile.Equals("ALL")) continue;
                files.Add(modFile.ToString());
            }
            i.Import(_folderPath.Text, files, log);
            Mods.Items.Clear();
            foreach (var modFile in modFiles.CheckedItems)
            {
                if (modFile.Equals("ALL")) continue;
                Mods.Items.Add(modFile + " loaded");
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void modFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            string curItem = modFiles.SelectedItem.ToString();
            if (curItem.Equals("ALL"))
            {
                int index = modFiles.FindString("ALL");
                bool state = modFiles.GetItemChecked(index);
                for (int i = 0; i < modFiles.Items.Count; i++)
                {
                    if (i == index) continue;
                    modFiles.SetItemChecked(i, state);
                }
            }
        }

        private void Scan_Click(object sender, EventArgs e)
        {
            string[] dmFiles = Directory.GetFiles(_folderPath.Text, "*.dm");
            modFiles.Items.Clear();
            modFiles.Items.Add("ALL", true);
            foreach (string s in dmFiles)
            {
                var modfile = !string.IsNullOrEmpty(modFileName.Text) ? modFileName.Text : "merged-mod";
                if (Path.GetFileName(s).StartsWith(modfile)) continue;
                modFiles.Items.Add(Path.GetFileName(s), true);
            }
        }

        private void Mods_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private bool log = false;
        private void logging_CheckedChanged(object sender, EventArgs e)
        {
            log = logging.Checked;
        }
    }
}
