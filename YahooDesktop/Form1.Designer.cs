namespace YahooDesktop
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
			this.btnLoginYahoo = new System.Windows.Forms.Button();
			this.textBoxOutput = new System.Windows.Forms.TextBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.txtURL = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnExecute = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnLoginYahoo
			// 
			this.btnLoginYahoo.Location = new System.Drawing.Point(13, 13);
			this.btnLoginYahoo.Margin = new System.Windows.Forms.Padding(4);
			this.btnLoginYahoo.Name = "btnLoginYahoo";
			this.btnLoginYahoo.Size = new System.Drawing.Size(100, 28);
			this.btnLoginYahoo.TabIndex = 0;
			this.btnLoginYahoo.Text = "Yahoo Login";
			this.btnLoginYahoo.UseVisualStyleBackColor = true;
			this.btnLoginYahoo.Click += new System.EventHandler(this.btnLoginYahoo_Click);
			// 
			// textBoxOutput
			// 
			this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxOutput.Location = new System.Drawing.Point(13, 111);
			this.textBoxOutput.Multiline = true;
			this.textBoxOutput.Name = "textBoxOutput";
			this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxOutput.Size = new System.Drawing.Size(739, 375);
			this.textBoxOutput.TabIndex = 1;
			// 
			// btnClear
			// 
			this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnClear.Location = new System.Drawing.Point(12, 77);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(740, 28);
			this.btnClear.TabIndex = 2;
			this.btnClear.Text = "Clear";
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// txtURL
			// 
			this.txtURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtURL.Location = new System.Drawing.Point(79, 48);
			this.txtURL.Name = "txtURL";
			this.txtURL.Size = new System.Drawing.Size(565, 23);
			this.txtURL.TabIndex = 3;
			this.txtURL.Text = "https://fantasysports.yahooapis.com/fantasy/v2/users;use_login=1/games";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 51);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(61, 17);
			this.label1.TabIndex = 4;
			this.label1.Text = "API URL";
			// 
			// btnExecute
			// 
			this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExecute.Location = new System.Drawing.Point(651, 45);
			this.btnExecute.Margin = new System.Windows.Forms.Padding(4);
			this.btnExecute.Name = "btnExecute";
			this.btnExecute.Size = new System.Drawing.Size(100, 28);
			this.btnExecute.TabIndex = 5;
			this.btnExecute.Text = "Execute";
			this.btnExecute.UseVisualStyleBackColor = true;
			this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(764, 498);
			this.Controls.Add(this.btnExecute);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtURL);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.textBoxOutput);
			this.Controls.Add(this.btnLoginYahoo);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnLoginYahoo;
		private System.Windows.Forms.TextBox textBoxOutput;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.TextBox txtURL;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnExecute;
	}
}

