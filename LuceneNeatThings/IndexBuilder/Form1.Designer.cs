namespace IndexBuilder
{
	partial class Form1
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.Label label2;
			this.panel1 = new System.Windows.Forms.Panel();
			this.txbCorpusPath = new System.Windows.Forms.TextBox();
			this.btnSelectCorpusWikiDump = new System.Windows.Forms.Button();
			this.btnSelectCorpusPath = new System.Windows.Forms.Button();
			this.lblStatus = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.btnExecute = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			label2 = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(1, 1);
			label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(89, 17);
			label2.TabIndex = 15;
			label2.Text = "Corpus path:";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(label2);
			this.panel1.Controls.Add(this.txbCorpusPath);
			this.panel1.Controls.Add(this.btnSelectCorpusWikiDump);
			this.panel1.Controls.Add(this.btnSelectCorpusPath);
			this.panel1.Location = new System.Drawing.Point(20, 13);
			this.panel1.Margin = new System.Windows.Forms.Padding(4);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(656, 51);
			this.panel1.TabIndex = 15;
			// 
			// txbCorpusPath
			// 
			this.txbCorpusPath.Location = new System.Drawing.Point(0, 21);
			this.txbCorpusPath.Margin = new System.Windows.Forms.Padding(4);
			this.txbCorpusPath.Name = "txbCorpusPath";
			this.txbCorpusPath.Size = new System.Drawing.Size(399, 22);
			this.txbCorpusPath.TabIndex = 14;
			// 
			// btnSelectCorpusWikiDump
			// 
			this.btnSelectCorpusWikiDump.Location = new System.Drawing.Point(408, 17);
			this.btnSelectCorpusWikiDump.Margin = new System.Windows.Forms.Padding(4);
			this.btnSelectCorpusWikiDump.Name = "btnSelectCorpusWikiDump";
			this.btnSelectCorpusWikiDump.Size = new System.Drawing.Size(120, 28);
			this.btnSelectCorpusWikiDump.TabIndex = 10;
			this.btnSelectCorpusWikiDump.Text = "Wiki-dump...";
			this.btnSelectCorpusWikiDump.UseVisualStyleBackColor = true;
			this.btnSelectCorpusWikiDump.Click += new System.EventHandler(this.btnSelectCorpusWikiDump_Click);
			// 
			// btnSelectCorpusPath
			// 
			this.btnSelectCorpusPath.Enabled = false;
			this.btnSelectCorpusPath.Location = new System.Drawing.Point(536, 17);
			this.btnSelectCorpusPath.Margin = new System.Windows.Forms.Padding(4);
			this.btnSelectCorpusPath.Name = "btnSelectCorpusPath";
			this.btnSelectCorpusPath.Size = new System.Drawing.Size(120, 28);
			this.btnSelectCorpusPath.TabIndex = 11;
			this.btnSelectCorpusPath.Text = "Select folder...";
			this.btnSelectCorpusPath.UseVisualStyleBackColor = true;
			// 
			// lblStatus
			// 
			this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(16, 89);
			this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(48, 17);
			this.lblStatus.TabIndex = 12;
			this.lblStatus.Text = "Status";
			this.lblStatus.Visible = false;
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(20, 109);
			this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(465, 32);
			this.progressBar1.TabIndex = 14;
			this.progressBar1.Visible = false;
			// 
			// btnExecute
			// 
			this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExecute.Location = new System.Drawing.Point(588, 109);
			this.btnExecute.Margin = new System.Windows.Forms.Padding(4);
			this.btnExecute.Name = "btnExecute";
			this.btnExecute.Size = new System.Drawing.Size(88, 32);
			this.btnExecute.TabIndex = 13;
			this.btnExecute.Text = "Start";
			this.btnExecute.UseVisualStyleBackColor = true;
			this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(493, 109);
			this.button1.Margin = new System.Windows.Forms.Padding(4);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(87, 32);
			this.button1.TabIndex = 11;
			this.button1.Text = "Pause";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(695, 154);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.btnExecute);
			this.Name = "Form1";
			this.Text = "Form1";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TextBox txbCorpusPath;
		private System.Windows.Forms.Button btnSelectCorpusWikiDump;
		private System.Windows.Forms.Button btnSelectCorpusPath;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button btnExecute;
		private System.Windows.Forms.Button button1;
	}
}

