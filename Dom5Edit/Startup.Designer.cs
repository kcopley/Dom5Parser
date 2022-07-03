namespace Dom5Edit
{
    partial class Startup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Startup));
            this.startButton = new System.Windows.Forms.Button();
            this._folderPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.eaNations = new System.Windows.Forms.CheckedListBox();
            this.maNations = new System.Windows.Forms.CheckedListBox();
            this.laNations = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.modFileName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.importButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.modFiles = new System.Windows.Forms.CheckedListBox();
            this.Scan = new System.Windows.Forms.Button();
            this.Mods = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.logging = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(222, 166);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(207, 46);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Merge and Export";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // _folderPath
            // 
            this._folderPath.Location = new System.Drawing.Point(12, 36);
            this._folderPath.Name = "_folderPath";
            this._folderPath.Size = new System.Drawing.Size(414, 20);
            this._folderPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path to Folder";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(11, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(174, 24);
            this.label3.TabIndex = 4;
            this.label3.Text = "Pluto\'s Mod Merger";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 363);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(439, 182);
            this.label4.TabIndex = 6;
            this.label4.Text = resources.GetString("label4.Text");
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(991, 605);
            this.button5.Margin = new System.Windows.Forms.Padding(2);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(56, 19);
            this.button5.TabIndex = 8;
            this.button5.Text = "Exit";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // eaNations
            // 
            this.eaNations.CheckOnClick = true;
            this.eaNations.FormattingEnabled = true;
            this.eaNations.Items.AddRange(new object[] {
            "TOGGLE ALL",
            "EA Arcosephale",
            "EA Ermor",
            "EA Ulm",
            "EA Marverni",
            "EA Sauromatia",
            "EA T\'ien Ch\'i",
            "EA Machaka",
            "EA Mictlan",
            "EA Abysia",
            "EA Caelum",
            "EA C\'tis",
            "EA Pangaea",
            "EA Agartha",
            "EA Tir na n\'Og",
            "EA Fomoria",
            "EA Vanheim",
            "EA Helheim",
            "EA Niefelheim",
            "EA Rus",
            "EA Kailasa",
            "EA Lanka",
            "EA Yomi",
            "EA Hinnom",
            "EA Ur",
            "EA Berytos",
            "EA Xibalba",
            "EA Mekone",
            "EA Ubar",
            "EA Atlantis",
            "EA R\'lyeh",
            "EA Pelagia",
            "EA Oceania",
            "EA Therodos"});
            this.eaNations.Location = new System.Drawing.Point(660, 82);
            this.eaNations.Name = "eaNations";
            this.eaNations.ScrollAlwaysVisible = true;
            this.eaNations.Size = new System.Drawing.Size(134, 394);
            this.eaNations.TabIndex = 9;
            this.eaNations.SelectedIndexChanged += new System.EventHandler(this.vanillaNations_SelectedIndexChanged);
            // 
            // maNations
            // 
            this.maNations.CheckOnClick = true;
            this.maNations.FormattingEnabled = true;
            this.maNations.Items.AddRange(new object[] {
            "TOGGLE ALL",
            "MA Arcosephale",
            "MA Ermor",
            "MA Sceleria",
            "MA Pythium",
            "MA Man",
            "MA Eriu",
            "MA Ulm",
            "MA Marignon",
            "MA Mictlan",
            "MA T\'ien Ch\'i",
            "MA Machaka",
            "MA Agartha",
            "MA Abysia",
            "MA Caelum",
            "MA C\'tis",
            "MA Pangaea",
            "MA Asphodel",
            "MA Vanheim",
            "MA Jotunheim",
            "MA Vanarus",
            "MA Bandar Log",
            "MA Shinuyama",
            "MA Ashdod",
            "MA Uruk",
            "MA Nazca",
            "MA Xibalba",
            "MA Phlegra",
            "MA Phaecia",
            "MA Ind",
            "MA Na\'Ba",
            "MA Atlantis",
            "MA R\'lyeh",
            "MA Pelagia",
            "MA Oceania",
            "MA Ys"});
            this.maNations.Location = new System.Drawing.Point(800, 82);
            this.maNations.Name = "maNations";
            this.maNations.Size = new System.Drawing.Size(120, 394);
            this.maNations.TabIndex = 10;
            this.maNations.SelectedIndexChanged += new System.EventHandler(this.maNations_SelectedIndexChanged);
            // 
            // laNations
            // 
            this.laNations.CheckOnClick = true;
            this.laNations.FormattingEnabled = true;
            this.laNations.Items.AddRange(new object[] {
            "TOGGLE ALL",
            "LA Arcosephale",
            "LA Pythium",
            "LA Lemuria",
            "LA Man",
            "LA Ulm",
            "LA Marignon",
            "LA Mictlan",
            "LA T\'ien Ch\'i",
            "LA Jomon",
            "LA Agartha",
            "LA Abysia",
            "LA Caelum",
            "LA C\'tis",
            "LA Pangaea",
            "LA Midgard",
            "LA Utgard",
            "LA Bogarus",
            "LA Patala",
            "LA Gath",
            "LA Ragha",
            "LA Xibalba",
            "LA Phlegra",
            "LA Vaettiheim",
            "LA Atlantis",
            "LA R\'lyeh",
            "LA Erytheia"});
            this.laNations.Location = new System.Drawing.Point(926, 82);
            this.laNations.Name = "laNations";
            this.laNations.Size = new System.Drawing.Size(120, 394);
            this.laNations.TabIndex = 11;
            this.laNations.SelectedIndexChanged += new System.EventHandler(this.laNations_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(657, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(347, 39);
            this.label2.TabIndex = 12;
            this.label2.Text = "Toggle to disable mages from these vanilla mages.\r\nUse this if you have mages pos" +
    "t-merge that do not have paths.\r\nStandard recommendation is disable all vanilla " +
    "nations you are not using.";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // modFileName
            // 
            this.modFileName.Location = new System.Drawing.Point(12, 75);
            this.modFileName.Name = "modFileName";
            this.modFileName.Size = new System.Drawing.Size(414, 20);
            this.modFileName.TabIndex = 13;
            this.modFileName.Text = "merged-mod";
            this.modFileName.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 98);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Merged Mod Name";
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(12, 166);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(204, 46);
            this.importButton.TabIndex = 15;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 561);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(141, 39);
            this.button2.TabIndex = 16;
            this.button2.Text = "Export Magic Paths CSV";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 603);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(122, 26);
            this.label6.TabIndex = 17;
            this.label6.Text = "Utility for nation analysis.\r\nMust be post-import.";
            this.label6.Click += new System.EventHandler(this.label6_Click);
            // 
            // modFiles
            // 
            this.modFiles.CheckOnClick = true;
            this.modFiles.FormattingEnabled = true;
            this.modFiles.Items.AddRange(new object[] {
            "ALL"});
            this.modFiles.Location = new System.Drawing.Point(432, 66);
            this.modFiles.Name = "modFiles";
            this.modFiles.ScrollAlwaysVisible = true;
            this.modFiles.Size = new System.Drawing.Size(219, 334);
            this.modFiles.TabIndex = 19;
            this.modFiles.SelectedIndexChanged += new System.EventHandler(this.modFiles_SelectedIndexChanged);
            // 
            // Scan
            // 
            this.Scan.Location = new System.Drawing.Point(12, 114);
            this.Scan.Name = "Scan";
            this.Scan.Size = new System.Drawing.Size(204, 46);
            this.Scan.TabIndex = 20;
            this.Scan.Text = "Scan Folder";
            this.Scan.UseVisualStyleBackColor = true;
            this.Scan.Click += new System.EventHandler(this.Scan_Click);
            // 
            // Mods
            // 
            this.Mods.FormattingEnabled = true;
            this.Mods.Location = new System.Drawing.Point(12, 218);
            this.Mods.Name = "Mods";
            this.Mods.Size = new System.Drawing.Size(204, 108);
            this.Mods.TabIndex = 21;
            this.Mods.SelectedIndexChanged += new System.EventHandler(this.Mods_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(219, 265);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 13);
            this.label7.TabIndex = 22;
            // 
            // logging
            // 
            this.logging.AutoSize = true;
            this.logging.Location = new System.Drawing.Point(159, 573);
            this.logging.Name = "logging";
            this.logging.Size = new System.Drawing.Size(98, 17);
            this.logging.TabIndex = 23;
            this.logging.Text = "Log Mod Errors";
            this.logging.UseVisualStyleBackColor = true;
            this.logging.CheckedChanged += new System.EventHandler(this.logging_CheckedChanged);
            // 
            // Startup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1058, 635);
            this.Controls.Add(this.logging);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.Mods);
            this.Controls.Add(this.Scan);
            this.Controls.Add(this.modFiles);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.modFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.laNations);
            this.Controls.Add(this.maNations);
            this.Controls.Add(this.eaNations);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._folderPath);
            this.Controls.Add(this.startButton);
            this.Name = "Startup";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox _folderPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.CheckedListBox eaNations;
        private System.Windows.Forms.CheckedListBox maNations;
        private System.Windows.Forms.CheckedListBox laNations;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox modFileName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckedListBox modFiles;
        private System.Windows.Forms.Button Scan;
        private System.Windows.Forms.ListBox Mods;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox logging;
    }
}
