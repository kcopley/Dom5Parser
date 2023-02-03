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
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit
{
    public partial class Startup : Form
    {
        public Startup()
        {
            InitializeComponent();
            _folderPath.Text = Path.GetFullPath(Path.Combine(Application.UserAppDataPath, @"..\..\..\Dominions5\testing"));
            Scan_Click(null, null);
            this.Name = "Dom5Merger";
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

        private ModManager temp;
        private void button1_Click(object sender, EventArgs e)
        {
            i = new ModManager();
            temp = i;
            temp.Logging = logging.Checked;
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
            if (!Directory.Exists(_folderPath?.Text)) return;
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

        private bool log = false;
        private void logging_CheckedChanged(object sender, EventArgs e)
        {
            log = logging.Checked;
            if (temp != null)
            {
                temp.Logging = log;
            }
        }

        private void Startup_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_folderPath?.Text) && Directory.Exists(_folderPath?.Text))
            {
                folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
                folderBrowserDialog1.SelectedPath = _folderPath.Text;
            }
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                _folderPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void _folderPath_TextChanged(object sender, EventArgs e)
        {
            Scan_Click(sender, e);
        }

        private void verify_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_folderPath?.Text) && Directory.Exists(_folderPath?.Text))
                openFileDialog1.InitialDirectory = _folderPath.Text;
            DialogResult result = openFileDialog1.ShowDialog();
            string file = "";
            if (result == DialogResult.OK)
            {
                file = openFileDialog1.FileName;
            }
            else { return; }
            i = new ModManager();
            i._ModName = !string.IsNullOrEmpty(modFileName.Text) ? modFileName.Text : "temp-mod";
            i.ImportToVerify(file, openFileDialog1.SafeFileName);
            Mods.Items.Clear();
            Mods.Items.Add("Mod verified successfully");
        }

        ModManager editor;
        Mod editedMod;

        private void loadToEdit_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_folderPath?.Text) && Directory.Exists(_folderPath?.Text))
                openFileDialog1.InitialDirectory = _folderPath.Text;
            DialogResult result = openFileDialog1.ShowDialog();
            string file = "";
            if (result == DialogResult.OK)
            {
                file = openFileDialog1.FileName;
            }
            else { return; }
            editor = new ModManager();
            editor._ModName = !string.IsNullOrEmpty(modFileName.Text) ? modFileName.Text : "temp-mod";

        }

        private void idSelectorDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel1.Controls.Clear();
            IDEntity entity = idSelectorDropDown.SelectedItem as IDEntity;
            int row = 0;
            Point baseSize = new Point(160, 20);
            foreach (var prop in entity.Properties)
            {
                var commandControl = new Label();
                commandControl.AutoSize = false;
                commandControl.Text = prop._command.ToString();
                commandControl.Size = new Size(baseSize);
                commandControl.Location = new Point(0, row);
                panel1.Controls.Add(commandControl);

                var newControl = prop.GetControl(baseSize);
                if (newControl != null)
                {
                    var size = newControl.Size;
                    if (size.Height < baseSize.Y) //resized text
                    {
                        int difference = (baseSize.Y - size.Height) / 2;
                        var location = new Point(commandControl.Size.Width, row - difference);
                        newControl.Location = location;
                    }
                    else
                    {
                        var location = new Point(commandControl.Size.Width, row);
                        newControl.Location = location;
                    }
                    

                    panel1.Controls.Add(newControl);
                    row += Math.Max(size.Height, baseSize.Y) + 2;
                }
                else
                {
                    row += commandControl.Size.Height;
                }
                
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
            editor.Save(editedMod);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
