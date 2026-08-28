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
            btnAddSource = new Button();
            btnStartBackup = new Button();
            lblTitle = new Label();
            sourcePanel = new Panel();
            btnRemoveSource = new Button();
            listSourceFolders = new ListBox();
            lblSource = new Label();
            destinationPanel = new Panel();
            btnSelectDestination = new Button();
            txtDestinationPath = new TextBox();
            lblDestination = new Label();
            backupProgressPanel = new Panel();
            lblProgressPercent = new Label();
            lblStatus = new Label();
            progressBarBackup = new ProgressBar();
            lblBackupProgress = new Label();
            lblFilesScanned = new Label();
            lblFilesCopied = new Label();
            lblFilesSkipped = new Label();
            lblErrors = new Label();
            lblLastBackup = new Label();
            sourcePanel.SuspendLayout();
            destinationPanel.SuspendLayout();
            backupProgressPanel.SuspendLayout();
            SuspendLayout();
            // 
            // btnAddSource
            // 
            btnAddSource.Location = new Point(493, 44);
            btnAddSource.Name = "btnAddSource";
            btnAddSource.Size = new Size(112, 23);
            btnAddSource.TabIndex = 0;
            btnAddSource.Text = "Add Folder";
            btnAddSource.UseVisualStyleBackColor = true;
            btnAddSource.Click += btnAddSource_Click;
            // 
            // btnStartBackup
            // 
            btnStartBackup.BackColor = SystemColors.Desktop;
            btnStartBackup.FlatStyle = FlatStyle.Flat;
            btnStartBackup.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnStartBackup.ForeColor = SystemColors.Control;
            btnStartBackup.Location = new Point(644, 378);
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
            lblTitle.BackColor = Color.WhiteSmoke;
            lblTitle.BorderStyle = BorderStyle.FixedSingle;
            lblTitle.Font = new Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(608, 22);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(200, 39);
            lblTitle.TabIndex = 7;
            lblTitle.Text = "Backup Utility\r\n";
            // 
            // sourcePanel
            // 
            sourcePanel.BackColor = Color.DarkGray;
            sourcePanel.Controls.Add(btnRemoveSource);
            sourcePanel.Controls.Add(listSourceFolders);
            sourcePanel.Controls.Add(btnAddSource);
            sourcePanel.Controls.Add(lblSource);
            sourcePanel.Location = new Point(382, 96);
            sourcePanel.Name = "sourcePanel";
            sourcePanel.Size = new Size(652, 120);
            sourcePanel.TabIndex = 11;
            // 
            // btnRemoveSource
            // 
            btnRemoveSource.Location = new Point(493, 70);
            btnRemoveSource.Name = "btnRemoveSource";
            btnRemoveSource.Size = new Size(112, 23);
            btnRemoveSource.TabIndex = 15;
            btnRemoveSource.Text = "Remove Folder";
            btnRemoveSource.UseVisualStyleBackColor = true;
            btnRemoveSource.Click += btnRemoveSource_Click;
            // 
            // listSourceFolders
            // 
            listSourceFolders.FormattingEnabled = true;
            listSourceFolders.ItemHeight = 15;
            listSourceFolders.Location = new Point(25, 44);
            listSourceFolders.Name = "listSourceFolders";
            listSourceFolders.Size = new Size(418, 49);
            listSourceFolders.TabIndex = 14;
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.BackColor = SystemColors.ButtonHighlight;
            lblSource.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblSource.Location = new Point(25, 10);
            lblSource.Name = "lblSource";
            lblSource.Size = new Size(99, 19);
            lblSource.TabIndex = 9;
            lblSource.Text = "Source Folder:";
            // 
            // destinationPanel
            // 
            destinationPanel.BackColor = Color.DarkGray;
            destinationPanel.Controls.Add(btnSelectDestination);
            destinationPanel.Controls.Add(txtDestinationPath);
            destinationPanel.Controls.Add(lblDestination);
            destinationPanel.Location = new Point(382, 239);
            destinationPanel.Name = "destinationPanel";
            destinationPanel.Size = new Size(652, 120);
            destinationPanel.TabIndex = 12;
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
            btnSelectDestination.Click += btnSelectDestination_Click;
            // 
            // txtDestinationPath
            // 
            txtDestinationPath.Location = new Point(25, 49);
            txtDestinationPath.Name = "txtDestinationPath";
            txtDestinationPath.Size = new Size(418, 23);
            txtDestinationPath.TabIndex = 11;
            // 
            // lblDestination
            // 
            lblDestination.AutoSize = true;
            lblDestination.BackColor = SystemColors.ButtonHighlight;
            lblDestination.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDestination.Location = new Point(25, 17);
            lblDestination.Name = "lblDestination";
            lblDestination.Size = new Size(133, 19);
            lblDestination.TabIndex = 10;
            lblDestination.Text = "Backup Destination:";
            // 
            // backupProgressPanel
            // 
            backupProgressPanel.BackColor = SystemColors.ScrollBar;
            backupProgressPanel.Controls.Add(lblProgressPercent);
            backupProgressPanel.Controls.Add(lblStatus);
            backupProgressPanel.Controls.Add(progressBarBackup);
            backupProgressPanel.Controls.Add(lblBackupProgress);
            backupProgressPanel.Location = new Point(382, 586);
            backupProgressPanel.Name = "backupProgressPanel";
            backupProgressPanel.Size = new Size(652, 120);
            backupProgressPanel.TabIndex = 13;
            // 
            // lblProgressPercent
            // 
            lblProgressPercent.BackColor = SystemColors.ScrollBar;
            lblProgressPercent.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblProgressPercent.Location = new Point(466, 49);
            lblProgressPercent.Name = "lblProgressPercent";
            lblProgressPercent.Size = new Size(53, 23);
            lblProgressPercent.TabIndex = 14;
            lblProgressPercent.Text = "0%";
            lblProgressPercent.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.BackColor = SystemColors.Control;
            lblStatus.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblStatus.Location = new Point(25, 84);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(44, 17);
            lblStatus.TabIndex = 13;
            lblStatus.Text = "Ready";
            // 
            // progressBarBackup
            // 
            progressBarBackup.BackColor = Color.White;
            progressBarBackup.ForeColor = SystemColors.ButtonHighlight;
            progressBarBackup.Location = new Point(25, 49);
            progressBarBackup.Name = "progressBarBackup";
            progressBarBackup.Size = new Size(435, 23);
            progressBarBackup.TabIndex = 12;
            // 
            // lblBackupProgress
            // 
            lblBackupProgress.AutoSize = true;
            lblBackupProgress.BackColor = SystemColors.ButtonHighlight;
            lblBackupProgress.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblBackupProgress.Location = new Point(25, 14);
            lblBackupProgress.Name = "lblBackupProgress";
            lblBackupProgress.Size = new Size(109, 17);
            lblBackupProgress.TabIndex = 11;
            lblBackupProgress.Text = "Backup Progress";
            // 
            // lblFilesScanned
            // 
            lblFilesScanned.BackColor = SystemColors.ButtonHighlight;
            lblFilesScanned.Location = new Point(382, 439);
            lblFilesScanned.Name = "lblFilesScanned";
            lblFilesScanned.Size = new Size(158, 15);
            lblFilesScanned.TabIndex = 14;
            lblFilesScanned.Text = "Files scanned: 0";
            // 
            // lblFilesCopied
            // 
            lblFilesCopied.BackColor = SystemColors.ButtonHighlight;
            lblFilesCopied.Location = new Point(382, 478);
            lblFilesCopied.Name = "lblFilesCopied";
            lblFilesCopied.Size = new Size(158, 15);
            lblFilesCopied.TabIndex = 16;
            lblFilesCopied.Text = "Files copied: 0";
            // 
            // lblFilesSkipped
            // 
            lblFilesSkipped.BackColor = SystemColors.ButtonHighlight;
            lblFilesSkipped.Location = new Point(382, 516);
            lblFilesSkipped.Name = "lblFilesSkipped";
            lblFilesSkipped.Size = new Size(158, 15);
            lblFilesSkipped.TabIndex = 17;
            lblFilesSkipped.Text = "Files skipped: 0";
            // 
            // lblErrors
            // 
            lblErrors.BackColor = SystemColors.ButtonHighlight;
            lblErrors.Location = new Point(382, 554);
            lblErrors.Name = "lblErrors";
            lblErrors.Size = new Size(158, 15);
            lblErrors.TabIndex = 18;
            lblErrors.Text = "Errors: 0";
            // 
            // lblLastBackup
            // 
            lblLastBackup.BackColor = SystemColors.ButtonHighlight;
            lblLastBackup.Location = new Point(848, 554);
            lblLastBackup.Name = "lblLastBackup";
            lblLastBackup.Size = new Size(186, 15);
            lblLastBackup.TabIndex = 19;
            lblLastBackup.Text = "Last backup:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gainsboro;
            ClientSize = new Size(1426, 756);
            Controls.Add(lblLastBackup);
            Controls.Add(lblErrors);
            Controls.Add(lblFilesSkipped);
            Controls.Add(lblFilesCopied);
            Controls.Add(lblFilesScanned);
            Controls.Add(backupProgressPanel);
            Controls.Add(destinationPanel);
            Controls.Add(sourcePanel);
            Controls.Add(lblTitle);
            Controls.Add(btnStartBackup);
            Name = "MainForm";
            Text = "Backup Utility";
            Load += MainForm_Load;
            sourcePanel.ResumeLayout(false);
            sourcePanel.PerformLayout();
            destinationPanel.ResumeLayout(false);
            destinationPanel.PerformLayout();
            backupProgressPanel.ResumeLayout(false);
            backupProgressPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnAddSource;
        private Button btnStartBackup;
        private Label lblTitle;
        private Panel sourcePanel;
        private Label lblSource;
        private Panel destinationPanel;
        private Button btnSelectDestination;
        private TextBox txtDestinationPath;
        private Label lblDestination;
        private Panel backupProgressPanel;
        private Label lblStatus;
        private ProgressBar progressBarBackup;
        private Label lblBackupProgress;
        private ListBox listSourceFolders;
        private Button btnRemoveSource;
        private Label lblProgressPercent;
        private Label lblFilesScanned;
        private Label lblFilesCopied;
        private Label lblFilesSkipped;
        private Label lblErrors;
        private Label lblLastBackup;
    }
}
