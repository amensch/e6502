namespace e6502Debugger
{
    partial class MainForm
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.lblA = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblFlags = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblPC = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblSP = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblY = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblX = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblNextInstruction = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMemory = new System.Windows.Forms.TextBox();
            this.btnStep = new System.Windows.Forms.Button();
            this.btnRestart = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLowRange = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtHighRange = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(53, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "A";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblA
            // 
            this.lblA.BackColor = System.Drawing.SystemColors.Window;
            this.lblA.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblA.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblA.Location = new System.Drawing.Point(84, 85);
            this.lblA.Name = "lblA";
            this.lblA.Size = new System.Drawing.Size(37, 37);
            this.lblA.TabIndex = 1;
            this.lblA.Text = "2F";
            this.lblA.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblFlags);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.lblPC);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.lblSP);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lblY);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.lblX);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblA);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.groupBox1.Location = new System.Drawing.Point(12, 31);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(186, 297);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Registers";
            // 
            // lblFlags
            // 
            this.lblFlags.BackColor = System.Drawing.SystemColors.Window;
            this.lblFlags.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblFlags.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblFlags.Location = new System.Drawing.Point(84, 247);
            this.lblFlags.Name = "lblFlags";
            this.lblFlags.Size = new System.Drawing.Size(83, 37);
            this.lblFlags.TabIndex = 11;
            this.lblFlags.Text = "NVDIZC";
            this.lblFlags.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label7.Location = new System.Drawing.Point(21, 251);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 28);
            this.label7.TabIndex = 10;
            this.label7.Text = "Flags";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPC
            // 
            this.lblPC.BackColor = System.Drawing.SystemColors.Window;
            this.lblPC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPC.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPC.Location = new System.Drawing.Point(84, 30);
            this.lblPC.Name = "lblPC";
            this.lblPC.Size = new System.Drawing.Size(63, 37);
            this.lblPC.TabIndex = 9;
            this.lblPC.Text = "2D3F";
            this.lblPC.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label6.Location = new System.Drawing.Point(53, 34);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 28);
            this.label6.TabIndex = 8;
            this.label6.Text = "PC";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSP
            // 
            this.lblSP.BackColor = System.Drawing.SystemColors.Window;
            this.lblSP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSP.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblSP.Location = new System.Drawing.Point(84, 196);
            this.lblSP.Name = "lblSP";
            this.lblSP.Size = new System.Drawing.Size(37, 37);
            this.lblSP.TabIndex = 7;
            this.lblSP.Text = "2F";
            this.lblSP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label5.Location = new System.Drawing.Point(53, 200);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 28);
            this.label5.TabIndex = 6;
            this.label5.Text = "SP";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblY
            // 
            this.lblY.BackColor = System.Drawing.SystemColors.Window;
            this.lblY.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblY.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblY.Location = new System.Drawing.Point(84, 159);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(37, 37);
            this.lblY.TabIndex = 5;
            this.lblY.Text = "2F";
            this.lblY.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(53, 163);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 28);
            this.label4.TabIndex = 4;
            this.label4.Text = "Y";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblX
            // 
            this.lblX.BackColor = System.Drawing.SystemColors.Window;
            this.lblX.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblX.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblX.Location = new System.Drawing.Point(84, 122);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(37, 37);
            this.lblX.TabIndex = 3;
            this.lblX.Text = "2F";
            this.lblX.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(53, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 28);
            this.label3.TabIndex = 2;
            this.label3.Text = "X";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNextInstruction
            // 
            this.lblNextInstruction.BackColor = System.Drawing.SystemColors.Window;
            this.lblNextInstruction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblNextInstruction.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblNextInstruction.Location = new System.Drawing.Point(12, 383);
            this.lblNextInstruction.Name = "lblNextInstruction";
            this.lblNextInstruction.Size = new System.Drawing.Size(186, 37);
            this.lblNextInstruction.TabIndex = 13;
            this.lblNextInstruction.Text = "$0000: BRK";
            this.lblNextInstruction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(12, 343);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(151, 28);
            this.label8.TabIndex = 12;
            this.label8.Text = "Next Instruction";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(221, 31);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(86, 28);
            this.label9.TabIndex = 14;
            this.label9.Text = "Memory";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMemory
            // 
            this.txtMemory.Location = new System.Drawing.Point(221, 116);
            this.txtMemory.Multiline = true;
            this.txtMemory.Name = "txtMemory";
            this.txtMemory.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMemory.Size = new System.Drawing.Size(473, 752);
            this.txtMemory.TabIndex = 15;
            // 
            // btnStep
            // 
            this.btnStep.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnStep.Location = new System.Drawing.Point(600, 897);
            this.btnStep.Name = "btnStep";
            this.btnStep.Size = new System.Drawing.Size(94, 29);
            this.btnStep.TabIndex = 16;
            this.btnStep.Text = "Step (F5)";
            this.btnStep.UseVisualStyleBackColor = true;
            this.btnStep.Click += new System.EventHandler(this.btnStep_Click);
            // 
            // btnRestart
            // 
            this.btnRestart.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnRestart.Location = new System.Drawing.Point(474, 897);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(94, 29);
            this.btnRestart.TabIndex = 17;
            this.btnRestart.Text = "Restart";
            this.btnRestart.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(718, 28);
            this.menuStrip1.TabIndex = 18;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(128, 26);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(128, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(286, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 23);
            this.label2.TabIndex = 19;
            this.label2.Text = "Filter:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLowRange
            // 
            this.txtLowRange.Location = new System.Drawing.Point(343, 74);
            this.txtLowRange.Name = "txtLowRange";
            this.txtLowRange.Size = new System.Drawing.Size(43, 27);
            this.txtLowRange.TabIndex = 20;
            this.txtLowRange.Text = "0000";
            this.txtLowRange.Enter += new System.EventHandler(this.txtLowRange_Enter);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label10.Location = new System.Drawing.Point(392, 78);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(26, 23);
            this.label10.TabIndex = 21;
            this.label10.Text = "to";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtHighRange
            // 
            this.txtHighRange.Location = new System.Drawing.Point(424, 75);
            this.txtHighRange.Name = "txtHighRange";
            this.txtHighRange.Size = new System.Drawing.Size(43, 27);
            this.txtHighRange.TabIndex = 22;
            this.txtHighRange.Text = "FFFF";
            this.txtHighRange.Enter += new System.EventHandler(this.txtHighRange_Enter);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(718, 953);
            this.Controls.Add(this.txtHighRange);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtLowRange);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.btnStep);
            this.Controls.Add(this.txtMemory);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.lblNextInstruction);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Debugger";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private Label label1;
        private Label lblA;
        private GroupBox groupBox1;
        private Label lblFlags;
        private Label label7;
        private Label lblPC;
        private Label label6;
        private Label lblSP;
        private Label label5;
        private Label lblY;
        private Label label4;
        private Label lblX;
        private Label label3;
        private Label lblNextInstruction;
        private Label label8;
        private Label label9;
        private TextBox txtMemory;
        private Button btnStep;
        private Button btnRestart;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private Label label2;
        private TextBox txtLowRange;
        private Label label10;
        private TextBox txtHighRange;
    }
}