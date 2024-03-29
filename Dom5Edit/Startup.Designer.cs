﻿namespace Dom5Edit
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
            this.label1 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.eaNations = new System.Windows.Forms.CheckedListBox();
            this.maNations = new System.Windows.Forms.CheckedListBox();
            this.laNations = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.importButton = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.modFiles = new System.Windows.Forms.CheckedListBox();
            this.Scan = new System.Windows.Forms.Button();
            this.Mods = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.logging = new System.Windows.Forms.CheckBox();
            this._folderPath = new System.Windows.Forms.RichTextBox();
            this.modFileName = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(198, 459);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(180, 26);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Merge and Export";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path to Folder";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(772, 506);
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
            this.eaNations.IntegralHeight = false;
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
            this.eaNations.Location = new System.Drawing.Point(396, 68);
            this.eaNations.Name = "eaNations";
            this.eaNations.ScrollAlwaysVisible = true;
            this.eaNations.Size = new System.Drawing.Size(140, 385);
            this.eaNations.TabIndex = 9;
            this.eaNations.SelectedIndexChanged += new System.EventHandler(this.vanillaNations_SelectedIndexChanged);
            // 
            // maNations
            // 
            this.maNations.CheckOnClick = true;
            this.maNations.FormattingEnabled = true;
            this.maNations.IntegralHeight = false;
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
            this.maNations.Location = new System.Drawing.Point(542, 68);
            this.maNations.Name = "maNations";
            this.maNations.Size = new System.Drawing.Size(140, 385);
            this.maNations.TabIndex = 10;
            this.maNations.SelectedIndexChanged += new System.EventHandler(this.maNations_SelectedIndexChanged);
            // 
            // laNations
            // 
            this.laNations.CheckOnClick = true;
            this.laNations.FormattingEnabled = true;
            this.laNations.IntegralHeight = false;
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
            this.laNations.Location = new System.Drawing.Point(688, 68);
            this.laNations.Name = "laNations";
            this.laNations.Size = new System.Drawing.Size(140, 385);
            this.laNations.TabIndex = 11;
            this.laNations.SelectedIndexChanged += new System.EventHandler(this.laNations_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(393, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(317, 26);
            this.label2.TabIndex = 12;
            this.label2.Text = "Toggle to disable mages from these selected vanilla nations.\r\nRecommendation is d" +
    "isable mages from nations you are not using.";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(198, 488);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Merged Mod Name";
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(12, 459);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(180, 26);
            this.importButton.TabIndex = 15;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(688, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(141, 39);
            this.button2.TabIndex = 16;
            this.button2.Text = "Export Magic Paths CSV";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // modFiles
            // 
            this.modFiles.BackColor = System.Drawing.SystemColors.Window;
            this.modFiles.CheckOnClick = true;
            this.modFiles.FormattingEnabled = true;
            this.modFiles.IntegralHeight = false;
            this.modFiles.Items.AddRange(new object[] {
            "ALL"});
            this.modFiles.Location = new System.Drawing.Point(12, 68);
            this.modFiles.Name = "modFiles";
            this.modFiles.ScrollAlwaysVisible = true;
            this.modFiles.Size = new System.Drawing.Size(180, 385);
            this.modFiles.TabIndex = 19;
            this.modFiles.SelectedIndexChanged += new System.EventHandler(this.modFiles_SelectedIndexChanged);
            // 
            // Scan
            // 
            this.Scan.Image = ((System.Drawing.Image)(resources.GetObject("Scan.Image")));
            this.Scan.Location = new System.Drawing.Point(335, 25);
            this.Scan.Name = "Scan";
            this.Scan.Size = new System.Drawing.Size(22, 22);
            this.Scan.TabIndex = 20;
            this.Scan.UseVisualStyleBackColor = true;
            this.Scan.Click += new System.EventHandler(this.Scan_Click);
            // 
            // Mods
            // 
            this.Mods.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Mods.FormattingEnabled = true;
            this.Mods.IntegralHeight = false;
            this.Mods.Location = new System.Drawing.Point(198, 68);
            this.Mods.Name = "Mods";
            this.Mods.ScrollAlwaysVisible = true;
            this.Mods.Size = new System.Drawing.Size(180, 385);
            this.Mods.TabIndex = 21;
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
            this.logging.Location = new System.Drawing.Point(396, 504);
            this.logging.Name = "logging";
            this.logging.Size = new System.Drawing.Size(121, 17);
            this.logging.TabIndex = 23;
            this.logging.Text = "Log Errors on Import";
            this.logging.UseVisualStyleBackColor = true;
            this.logging.CheckedChanged += new System.EventHandler(this.logging_CheckedChanged);
            // 
            // _folderPath
            // 
            this._folderPath.Location = new System.Drawing.Point(12, 25);
            this._folderPath.Multiline = false;
            this._folderPath.Name = "_folderPath";
            this._folderPath.Size = new System.Drawing.Size(317, 22);
            this._folderPath.TabIndex = 24;
            this._folderPath.Text = "";
            this._folderPath.TextChanged += new System.EventHandler(this._folderPath_TextChanged);
            // 
            // modFileName
            // 
            this.modFileName.Location = new System.Drawing.Point(198, 504);
            this.modFileName.Multiline = false;
            this.modFileName.Name = "modFileName";
            this.modFileName.Size = new System.Drawing.Size(180, 21);
            this.modFileName.TabIndex = 25;
            this.modFileName.Text = "merged-mod";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Mods";
            this.label3.Click += new System.EventHandler(this.label3_Click_1);
            // 
            // button1
            // 
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.button1.Location = new System.Drawing.Point(356, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(22, 22);
            this.button1.TabIndex = 27;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 499);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(180, 26);
            this.button3.TabIndex = 28;
            this.button3.Text = "Verify";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "PickAMod";
            // 
            // Startup
            // 
            this.AccessibleName = "Dom5ModTools";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(845, 543);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.modFileName);
            this.Controls.Add(this._folderPath);
            this.Controls.Add(this.logging);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.Mods);
            this.Controls.Add(this.Scan);
            this.Controls.Add(this.modFiles);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.laNations);
            this.Controls.Add(this.maNations);
            this.Controls.Add(this.eaNations);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.startButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Startup";
            this.Text = "Dom5 Mod Tools";
            this.Load += new System.EventHandler(this.Startup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.CheckedListBox eaNations;
        private System.Windows.Forms.CheckedListBox maNations;
        private System.Windows.Forms.CheckedListBox laNations;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckedListBox modFiles;
        private System.Windows.Forms.Button Scan;
        private System.Windows.Forms.ListBox Mods;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox logging;
        private System.Windows.Forms.RichTextBox _folderPath;
        private System.Windows.Forms.RichTextBox modFileName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}
