namespace NSVRGui
{
	partial class VersionInfo
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VersionInfo));
			this.releasenotes = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.chimeraversion = new System.Windows.Forms.Label();
			this.serviceversion = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// releasenotes
			// 
			this.releasenotes.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.releasenotes.Location = new System.Drawing.Point(0, 131);
			this.releasenotes.Name = "releasenotes";
			this.releasenotes.Size = new System.Drawing.Size(294, 27);
			this.releasenotes.TabIndex = 0;
			this.releasenotes.Text = "View Release Notes";
			this.releasenotes.UseVisualStyleBackColor = true;
			this.releasenotes.Click += new System.EventHandler(this.button1_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Open Sans", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(13, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(189, 17);
			this.label1.TabIndex = 1;
			this.label1.Text = "NullSpace VR Runtime Service";
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Open Sans", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(13, 68);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(135, 17);
			this.label2.TabIndex = 2;
			this.label2.Text = "Chimera Distribution";
			this.label2.Click += new System.EventHandler(this.label2_Click);
			// 
			// chimeraversion
			// 
			this.chimeraversion.AutoSize = true;
			this.chimeraversion.Location = new System.Drawing.Point(25, 93);
			this.chimeraversion.Name = "chimeraversion";
			this.chimeraversion.Size = new System.Drawing.Size(87, 15);
			this.chimeraversion.TabIndex = 3;
			this.chimeraversion.Text = "Default Version";
			// 
			// serviceversion
			// 
			this.serviceversion.AutoSize = true;
			this.serviceversion.Location = new System.Drawing.Point(25, 40);
			this.serviceversion.Name = "serviceversion";
			this.serviceversion.Size = new System.Drawing.Size(87, 15);
			this.serviceversion.TabIndex = 4;
			this.serviceversion.Text = "Default Version";
			this.serviceversion.Click += new System.EventHandler(this.serviceversion_Click);
			// 
			// VersionInfo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(294, 158);
			this.Controls.Add(this.serviceversion);
			this.Controls.Add(this.chimeraversion);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.releasenotes);
			this.Font = new System.Drawing.Font("Open Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "VersionInfo";
			this.ShowIcon = false;
			this.Text = "Version Information";
			this.Load += new System.EventHandler(this.VersionInfo_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button releasenotes;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label chimeraversion;
		private System.Windows.Forms.Label serviceversion;
	}
}