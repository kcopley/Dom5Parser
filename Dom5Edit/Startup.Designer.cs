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
            this.startButton = new System.Windows.Forms.Button();
            this._folderPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.mergeButton = new System.Windows.Forms.Button();
            this.exportButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(317, 202);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(234, 64);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // _folderPath
            // 
            this._folderPath.Location = new System.Drawing.Point(222, 162);
            this._folderPath.Name = "_folderPath";
            this._folderPath.Size = new System.Drawing.Size(414, 20);
            this._folderPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(219, 146);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path to Folder";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // mergeButton
            // 
            this.mergeButton.Location = new System.Drawing.Point(317, 272);
            this.mergeButton.Name = "mergeButton";
            this.mergeButton.Size = new System.Drawing.Size(234, 58);
            this.mergeButton.TabIndex = 3;
            this.mergeButton.Text = "Merge";
            this.mergeButton.UseVisualStyleBackColor = true;
            this.mergeButton.Click += new System.EventHandler(this.mergeButton_Click);
            // 
            // exportButton
            // 
            this.exportButton.Location = new System.Drawing.Point(317, 336);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(234, 56);
            this.exportButton.TabIndex = 4;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // Startup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.mergeButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._folderPath);
            this.Controls.Add(this.startButton);
            this.Name = "Startup";
            this.Size = new System.Drawing.Size(891, 554);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox _folderPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button mergeButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button exportButton;
    }
}
