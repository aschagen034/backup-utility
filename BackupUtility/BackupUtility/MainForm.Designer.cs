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
            btnStartBackup = new Button();
            lblTitle = new Label();
            sourcePanel = new Panel();
            lblSource = new Label();
            backupPanel = new Panel();
            lblDestination = new Label();
            txtDestinationPath = new TextBox();
            btnSelectDestination = new Button();
            panel1 = new Panel();
            lblBackupProgress = new Label();
            progressBarBackup = new ProgressBar();
            lblStatus = new Label();
            sourcePanel.SuspendLayout();
            backupPanel.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSelectSource
            // 
            btnSelectSource.Location = new Point(493, 55);
            btnSelectSource.Name = "btnSelectSource";
            btnSelectSource.Size = new Size(112, 23);
            btnSelectSource.TabIndex = 0;
            btnSelectSource.Text = "Select Source";
            btnSelectSource.UseVisualStyleBackColor = true;
            btnSelectSource.Click += btnSelectSource_Click;
            // 
            // txtSourcePath
            // 
            txtSourcePath.Location = new Point(25, 55);
            txtSourcePath.Name = "txtSourcePath";
            txtSourcePath.Size = new Size(435, 23);
            txtSourcePath.TabIndex = 1;
            // 
            // btnStartBackup
            // 
            btnStartBackup.BackColor = SystemColors.ControlDarkDark;
            btnStartBackup.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnStartBackup.ForeColor = SystemColors.Control;
            btnStartBackup.Location = new Point(554, 378);
            btnStartBackup.Name = "btnStartBackup";
            btnStartBackup.Size = new Size(129, 40);
            btnStartBackup.TabIndex = 4;
            btnStartBackup.Text = "Start Backup";
            btnStartBackup.UseVisualStyleBackColor = false;
            btnStartBackup.Click += btnStartBackup_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = SystemColors.ButtonHighlight;
            lblTitle.BorderStyle = BorderStyle.FixedSingle;
            lblTitle.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(518, 22);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 39);
            lblTitle.TabIndex = 7;
            lblTitle.Text = "Backup Utility\r\n";
            // 
            // sourcePanel
            // 
            sourcePanel.Controls.Add(btnSelectSource);
            sourcePanel.Controls.Add(txtSourcePath);
            sourcePanel.Controls.Add(lblSource);
            sourcePanel.Location = new Point(292, 96);
            sourcePanel.Name = "sourcePanel";
            sourcePanel.Size = new Size(652, 96);
            sourcePanel.TabIndex = 11;
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.BackColor = SystemColors.ButtonHighlight;
            lblSource.Location = new Point(25, 18);
            lblSource.Name = "lblSource";
            lblSource.Size = new Size(79, 15);
            lblSource.TabIndex = 9;
            lblSource.Text = "Source Folder";
            // 
            // backupPanel
            // 
            backupPanel.Controls.Add(btnSelectDestination);
            backupPanel.Controls.Add(txtDestinationPath);
            backupPanel.Controls.Add(lblDestination);
            backupPanel.Location = new Point(292, 242);
            backupPanel.Name = "backupPanel";
            backupPanel.Size = new Size(652, 99);
            backupPanel.TabIndex = 12;
            // 
            // lblDestination
            // 
            lblDestination.AutoSize = true;
            lblDestination.BackColor = SystemColors.ButtonHighlight;
            lblDestination.Location = new Point(25, 17);
            lblDestination.Name = "lblDestination";
            lblDestination.Size = new Size(109, 15);
            lblDestination.TabIndex = 10;
            lblDestination.Text = "Backup Destination";
            // 
            // txtDestinationPath
            // 
            txtDestinationPath.Location = new Point(20, 47);
            txtDestinationPath.Name = "txtDestinationPath";
            txtDestinationPath.Size = new Size(440, 23);
            txtDestinationPath.TabIndex = 11;
            // 
            // btnSelectDestination
            // 
            btnSelectDestination.Location = new Point(493, 47);
            btnSelectDestination.Name = "btnSelectDestination";
            btnSelectDestination.RightToLeft = RightToLeft.Yes;
            btnSelectDestination.Size = new Size(112, 24);
            btnSelectDestination.TabIndex = 12;
            btnSelectDestination.Text = "Select Destination";
            btnSelectDestination.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.Controls.Add(lblStatus);
            panel1.Controls.Add(progressBarBackup);
            panel1.Controls.Add(lblBackupProgress);
            panel1.Location = new Point(292, 442);
            panel1.Name = "panel1";
            panel1.Size = new Size(652, 110);
            panel1.TabIndex = 13;
            // 
            // lblBackupProgress
            // 
            lblBackupProgress.AutoSize = true;
            lblBackupProgress.BackColor = SystemColors.ButtonHighlight;
            lblBackupProgress.Location = new Point(25, 14);
            lblBackupProgress.Name = "lblBackupProgress";
            lblBackupProgress.Size = new Size(94, 15);
            lblBackupProgress.TabIndex = 11;
            lblBackupProgress.Text = "Backup Progress";
            
            // 
            // progressBarBackup
            // 
            progressBarBackup.Location = new Point(25, 49);
            progressBarBackup.Name = "progressBarBackup";
            progressBarBackup.Size = new Size(435, 23);
            progressBarBackup.TabIndex = 12;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.BackColor = SystemColors.Control;
            lblStatus.Location = new Point(25, 84);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 13;
            lblStatus.Text = "Ready";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(1236, 577);
            Controls.Add(panel1);
            Controls.Add(backupPanel);
            Controls.Add(sourcePanel);
            Controls.Add(lblTitle);
            Controls.Add(btnStartBackup);
            Name = "MainForm";
            Text = "Backup Utility";
            Load += MainForm_Load;
            sourcePanel.ResumeLayout(false);
            sourcePanel.PerformLayout();
            backupPanel.ResumeLayout(false);
            backupPanel.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSelectSource;
        private TextBox txtSourcePath;
        private Button btnStartBackup;
        private Label lblTitle;
        private Panel sourcePanel;
        private Label lblSource;
        private Panel backupPanel;
        private Button btnSelectDestination;
        private TextBox txtDestinationPath;
        private Label lblDestination;
        private Panel panel1;
        private Label lblStatus;
        private ProgressBar progressBarBackup;
        private Label lblBackupProgress;
    }
}
