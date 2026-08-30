namespace BackupUtility
{
    partial class BackupHistoryForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            historyGrid = new DataGridView();
            colCompleted = new DataGridViewTextBoxColumn();
            colScanned = new DataGridViewTextBoxColumn();
            colCopied = new DataGridViewTextBoxColumn();
            colSkipped = new DataGridViewTextBoxColumn();
            colErrors = new DataGridViewTextBoxColumn();
            lblDetails = new Label();
            txtDetails = new TextBox();
            btnClose = new Button();
            ((System.ComponentModel.ISupportInitialize)historyGrid).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(20, 18);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(165, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Backup History";
            // 
            // historyGrid
            // 
            historyGrid.AllowUserToAddRows = false;
            historyGrid.AllowUserToDeleteRows = false;
            historyGrid.AllowUserToResizeRows = false;
            historyGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            historyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            historyGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            historyGrid.Columns.AddRange(new DataGridViewColumn[] { colCompleted, colScanned, colCopied, colSkipped, colErrors });
            historyGrid.Location = new Point(24, 64);
            historyGrid.MultiSelect = false;
            historyGrid.Name = "historyGrid";
            historyGrid.ReadOnly = true;
            historyGrid.RowHeadersVisible = false;
            historyGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            historyGrid.Size = new Size(712, 248);
            historyGrid.TabIndex = 1;
            historyGrid.SelectionChanged += historyGrid_SelectionChanged;
            // 
            // colCompleted
            // 
            colCompleted.FillWeight = 160F;
            colCompleted.HeaderText = "Completed";
            colCompleted.Name = "colCompleted";
            colCompleted.ReadOnly = true;
            // 
            // colScanned
            // 
            colScanned.HeaderText = "Scanned";
            colScanned.Name = "colScanned";
            colScanned.ReadOnly = true;
            // 
            // colCopied
            // 
            colCopied.HeaderText = "Copied";
            colCopied.Name = "colCopied";
            colCopied.ReadOnly = true;
            // 
            // colSkipped
            // 
            colSkipped.HeaderText = "Skipped";
            colSkipped.Name = "colSkipped";
            colSkipped.ReadOnly = true;
            // 
            // colErrors
            // 
            colErrors.HeaderText = "Errors";
            colErrors.Name = "colErrors";
            colErrors.ReadOnly = true;
            // 
            // lblDetails
            // 
            lblDetails.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblDetails.AutoSize = true;
            lblDetails.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDetails.Location = new Point(24, 327);
            lblDetails.Name = "lblDetails";
            lblDetails.Size = new Size(125, 19);
            lblDetails.TabIndex = 2;
            lblDetails.Text = "Selected Run Details";
            // 
            // txtDetails
            // 
            txtDetails.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtDetails.BackColor = SystemColors.Window;
            txtDetails.Location = new Point(24, 352);
            txtDetails.Multiline = true;
            txtDetails.Name = "txtDetails";
            txtDetails.ReadOnly = true;
            txtDetails.ScrollBars = ScrollBars.Both;
            txtDetails.Size = new Size(712, 181);
            txtDetails.TabIndex = 3;
            txtDetails.WordWrap = false;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(636, 550);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 32);
            btnClose.TabIndex = 4;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // BackupHistoryForm
            // 
            AcceptButton = btnClose;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 603);
            Controls.Add(btnClose);
            Controls.Add(txtDetails);
            Controls.Add(lblDetails);
            Controls.Add(historyGrid);
            Controls.Add(lblTitle);
            MinimizeBox = false;
            MinimumSize = new Size(620, 500);
            Name = "BackupHistoryForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Backup History";
            ((System.ComponentModel.ISupportInitialize)historyGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private DataGridView historyGrid;
        private DataGridViewTextBoxColumn colCompleted;
        private DataGridViewTextBoxColumn colScanned;
        private DataGridViewTextBoxColumn colCopied;
        private DataGridViewTextBoxColumn colSkipped;
        private DataGridViewTextBoxColumn colErrors;
        private Label lblDetails;
        private TextBox txtDetails;
        private Button btnClose;
    }
}
