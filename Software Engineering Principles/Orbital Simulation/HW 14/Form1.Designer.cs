namespace HW_14
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
      this.components = new System.ComponentModel.Container();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.titleLabel = new System.Windows.Forms.Label();
      this.instrLabel = new System.Windows.Forms.Label();
      this.timer = new System.Windows.Forms.Timer(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // pictureBox
      // 
      this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pictureBox.Location = new System.Drawing.Point(13, 33);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(495, 480);
      this.pictureBox.TabIndex = 0;
      this.pictureBox.TabStop = false;
      this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
      // 
      // titleLabel
      // 
      this.titleLabel.AutoSize = true;
      this.titleLabel.Location = new System.Drawing.Point(194, 9);
      this.titleLabel.Name = "titleLabel";
      this.titleLabel.Size = new System.Drawing.Size(135, 13);
      this.titleLabel.TabIndex = 1;
      this.titleLabel.Text = "CptS 322 Orbital Simulation";
      // 
      // instrLabel
      // 
      this.instrLabel.AutoSize = true;
      this.instrLabel.Location = new System.Drawing.Point(104, 520);
      this.instrLabel.Name = "instrLabel";
      this.instrLabel.Size = new System.Drawing.Size(315, 13);
      this.instrLabel.TabIndex = 2;
      this.instrLabel.Text = "Left-click to create planets / Right-click to create centers-of-mass";
      // 
      // timer
      // 
      this.timer.Enabled = true;
      this.timer.Interval = 18;
      this.timer.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(520, 542);
      this.Controls.Add(this.instrLabel);
      this.Controls.Add(this.titleLabel);
      this.Controls.Add(this.pictureBox);
      this.Name = "Form1";
      this.Text = "Form1";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox pictureBox;
    private System.Windows.Forms.Label titleLabel;
    private System.Windows.Forms.Label instrLabel;
    private System.Windows.Forms.Timer timer;
  }
}

