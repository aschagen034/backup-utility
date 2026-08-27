namespace BackupUtility
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSelectSource = new Button();
            txtSourcePath = new TextBox();
            btnSelectDestination = new Button();
            txtDestinationPath = new TextBox();
            btnStartBackup = new Button();
            SuspendLayout();
            // 
            // btnSelectSource
            // 
            btnSelectSource.Location = new Point(398, 348);
            btnSelectSource.Name = "btnSelectSource";
            btnSelectSource.Size = new Size(112, 27);
            btnSelectSource.TabIndex = 0;
            btnSelectSource.Text = "Select Folder";
            btnSelectSource.UseVisualStyleBackColor = true;
            btnSelectSource.Click += btnSelectSource_Click;
            // 
            // txtSourcePath
            // 
            txtSourcePath.Location = new Point(170, 279);
            txtSourcePath.Name = "txtSourcePath";
            txtSourcePath.Size = new Size(340, 23);
            txtSourcePath.TabIndex = 1;
            // 
            // btnSelectDestination
            // 
            btnSelectDestination.Location = new Point(731, 352);
            btnSelectDestination.Name = "btnSelectDestination";
            btnSelectDestination.RightToLeft = RightToLeft.Yes;
            btnSelectDestination.Size = new Size(119, 23);
            btnSelectDestination.TabIndex = 2;
            btnSelectDestination.Text = "Select Destination";
            btnSelectDestination.UseVisualStyleBackColor = true;
            btnSelectDestination.Click += btnSelectDestination_Click;
            // 
            // txtDestinationPath
            // 
            txtDestinationPath.Location = new Point(731, 279);
            txtDestinationPath.Name = "txtDestinationPath";
            txtDestinationPath.Size = new Size(324, 23);
            txtDestinationPath.TabIndex = 3;
            // 
            // btnStartBackup
            // 
            btnStartBackup.Location = new Point(551, 428);
            btnStartBackup.Name = "btnStartBackup";
            btnStartBackup.Size = new Size(131, 26);
            btnStartBackup.TabIndex = 4;
            btnStartBackup.Text = "Start Backup";
            btnStartBackup.UseVisualStyleBackColor = true;
            btnStartBackup.Click += btnStartBackup_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1236, 577);
            Controls.Add(btnStartBackup);
            Controls.Add(txtDestinationPath);
            Controls.Add(btnSelectDestination);
            Controls.Add(txtSourcePath);
            Controls.Add(btnSelectSource);
            Name = "MainForm";
            Text = "Form1";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelectSource;
        private TextBox txtSourcePath;
        private Button btnSelectDestination;
        private TextBox txtDestinationPath;
        private Button btnStartBackup;
    }
}
