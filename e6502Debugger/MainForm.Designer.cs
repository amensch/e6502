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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblA = new System.Windows.Forms.Label();
            this.lblX = new System.Windows.Forms.Label();
            this.lblY = new System.Windows.Forms.Label();
            this.lblSP = new System.Windows.Forms.Label();
            this.lblPC = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblFlags = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lstPC = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lstMemory = new System.Windows.Forms.ListBox();
            this.btnStep = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtBreakPoint = new System.Windows.Forms.TextBox();
            this.MainMenu.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.MainMenu.Size = new System.Drawing.Size(746, 24);
            this.MainMenu.TabIndex = 0;
            this.MainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.openToolStripMenuItem.Text = "Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(109, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(18, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Accumulator:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(15, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Index X:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(15, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 16);
            this.label3.TabIndex = 3;
            this.label3.Text = "Index Y:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(15, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "SP:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(15, 156);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 16);
            this.label5.TabIndex = 5;
            this.label5.Text = "PC:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblA
            // 
            this.lblA.BackColor = System.Drawing.SystemColors.Window;
            this.lblA.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblA.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblA.Location = new System.Drawing.Point(131, 16);
            this.lblA.Name = "lblA";
            this.lblA.Size = new System.Drawing.Size(43, 24);
            this.lblA.TabIndex = 6;
            this.lblA.Text = "00";
            this.lblA.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblX
            // 
            this.lblX.BackColor = System.Drawing.SystemColors.Window;
            this.lblX.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblX.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblX.Location = new System.Drawing.Point(131, 49);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(43, 24);
            this.lblX.TabIndex = 7;
            this.lblX.Text = "00";
            this.lblX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblY
            // 
            this.lblY.BackColor = System.Drawing.SystemColors.Window;
            this.lblY.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblY.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblY.Location = new System.Drawing.Point(131, 84);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(43, 24);
            this.lblY.TabIndex = 8;
            this.lblY.Text = "00";
            this.lblY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSP
            // 
            this.lblSP.BackColor = System.Drawing.SystemColors.Window;
            this.lblSP.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSP.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSP.Location = new System.Drawing.Point(131, 118);
            this.lblSP.Name = "lblSP";
            this.lblSP.Size = new System.Drawing.Size(43, 24);
            this.lblSP.TabIndex = 9;
            this.lblSP.Text = "00";
            this.lblSP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPC
            // 
            this.lblPC.BackColor = System.Drawing.SystemColors.Window;
            this.lblPC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblPC.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPC.Location = new System.Drawing.Point(131, 153);
            this.lblPC.Name = "lblPC";
            this.lblPC.Size = new System.Drawing.Size(43, 24);
            this.lblPC.TabIndex = 10;
            this.lblPC.Text = "0000";
            this.lblPC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblFlags);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.lblPC);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblSP);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblY);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.lblX);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.lblA);
            this.groupBox1.Location = new System.Drawing.Point(12, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 227);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Registers";
            // 
            // lblFlags
            // 
            this.lblFlags.BackColor = System.Drawing.SystemColors.Window;
            this.lblFlags.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblFlags.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFlags.Location = new System.Drawing.Point(98, 186);
            this.lblFlags.Name = "lblFlags";
            this.lblFlags.Size = new System.Drawing.Size(76, 24);
            this.lblFlags.TabIndex = 12;
            this.lblFlags.Text = "NV-BDIZC";
            this.lblFlags.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(36, 189);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 16);
            this.label7.TabIndex = 11;
            this.label7.Text = "Flags:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lstPC
            // 
            this.lstPC.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstPC.FormattingEnabled = true;
            this.lstPC.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "20"});
            this.lstPC.Location = new System.Drawing.Point(6, 23);
            this.lstPC.Name = "lstPC";
            this.lstPC.Size = new System.Drawing.Size(188, 277);
            this.lstPC.TabIndex = 12;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lstPC);
            this.groupBox2.Location = new System.Drawing.Point(12, 260);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 308);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Next Instructions";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lstMemory);
            this.groupBox3.Location = new System.Drawing.Point(219, 28);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(512, 540);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Memory";
            // 
            // lstMemory
            // 
            this.lstMemory.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstMemory.FormattingEnabled = true;
            this.lstMemory.Items.AddRange(new object[] {
            "$0000 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0010 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0020 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0030 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0040 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0050 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0060 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0070 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0080 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0090 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$00A0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$00B0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$00C0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$00D0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$00E0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$00F0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0100 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0110 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0120 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0130 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0140 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0150 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0160 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0170 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0180 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0190 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$01A0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$01B0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$01C0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$01D0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$01E0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$01F0 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0210 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0220 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0230 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0240 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0250 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F",
            "$0260 - 00 01 02 03 04 05 06 07   08 09 0A 0B 0C 0D 0E 0F"});
            this.lstMemory.Location = new System.Drawing.Point(6, 22);
            this.lstMemory.Name = "lstMemory";
            this.lstMemory.ScrollAlwaysVisible = true;
            this.lstMemory.Size = new System.Drawing.Size(491, 511);
            this.lstMemory.TabIndex = 0;
            // 
            // btnStep
            // 
            this.btnStep.Location = new System.Drawing.Point(621, 584);
            this.btnStep.Name = "btnStep";
            this.btnStep.Size = new System.Drawing.Size(95, 33);
            this.btnStep.TabIndex = 15;
            this.btnStep.Text = "Step";
            this.btnStep.UseVisualStyleBackColor = true;
            this.btnStep.Click += new System.EventHandler(this.btnStep_Click);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(15, 584);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 16);
            this.label6.TabIndex = 16;
            this.label6.Text = "Break Point:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtBreakPoint
            // 
            this.txtBreakPoint.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBreakPoint.Location = new System.Drawing.Point(124, 584);
            this.txtBreakPoint.Name = "txtBreakPoint";
            this.txtBreakPoint.Size = new System.Drawing.Size(62, 20);
            this.txtBreakPoint.TabIndex = 17;
            this.txtBreakPoint.Text = "0000";
            this.txtBreakPoint.TextChanged += new System.EventHandler(this.txtBreakPoint_TextChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 629);
            this.Controls.Add(this.txtBreakPoint);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnStep);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.MainMenu);
            this.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeyPreview = true;
            this.MainMenuStrip = this.MainMenu;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "6502 Debugger";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblA;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.Label lblSP;
        private System.Windows.Forms.Label lblPC;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lstPC;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox lstMemory;
        private System.Windows.Forms.Button btnStep;
        private System.Windows.Forms.Label lblFlags;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtBreakPoint;
    }
}