using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using e6502CPU;

namespace e6502Debugger
{
    public partial class MainForm : Form
    {
        public const int MEMORY_LIST_SIZE = 38;
        public const int DASM_LIST_SIZE = 20;

        private e6502 cpu;

        public MainForm()
        {
            InitializeComponent();

            lblA.Text = "";
            lblX.Text = "";
            lblY.Text = "";
            lblSP.Text = "";
            lblPC.Text = "";
            lblFlags.Text = "";
            lstMemory.Items.Clear();
            lstPC.Items.Clear();
            txtBreakPoint.Text = "";

            LoadTestProgram();
        }

        private void LoadTestProgram()
        {
            cpu = new e6502();

            // instead of using file-open, be lazy and automatically load the file

            // stuck: $3509
            byte[] program;

            if (System.Environment.MachineName.StartsWith("US"))
            {
                program = File.ReadAllBytes(@"C:\Users\menschas\Source\6502_65C02_functional_tests\bin_files\6502_functional_test.bin");
            }
            else
            {
                program = File.ReadAllBytes(@"C:\Users\adam\Documents\My Projects\6502_65C02_functional_tests\bin_files\6502_functional_test.bin");
            }
            cpu.LoadProgram(0x0000, program);
            cpu.PC = 0x0400;
            UpdateScreen();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;

            if(dlg.ShowDialog() == DialogResult.OK)
            {
                byte[] program = File.ReadAllBytes(dlg.FileName);
                cpu.LoadProgram(0x0000, program);

                // this test program is supposed to start at 0x0400;
                cpu.PC = 0x0400;
            }

            UpdateScreen();
        }

        private void UpdateScreen()
        {
            lblA.Text = cpu.A.ToString("X2");
            lblX.Text = cpu.X.ToString("X2");
            lblY.Text = cpu.Y.ToString("X2");
            lblSP.Text = cpu.SP.ToString("X2");
            lblPC.Text = cpu.PC.ToString("X4");

            string flags = "";

            if (cpu.NF)
                flags += "N";
            else
                flags += "-";
            if (cpu.VF)
                flags += "V";
            else
                flags += "-";
            flags += "-";
            if (cpu.BF)
                flags += "B";
            else
                flags += "-";
            if (cpu.DF)
                flags += "D";
            else
                flags += "-";
            if (cpu.IF)
                flags += "I";
            else
                flags += "-";
            if (cpu.ZF)
                flags += "Z";
            else
                flags += "-";
            if (cpu.CF)
                flags += "C";
            else
                flags += "-";

            lblFlags.Text = flags;

            lstPC.Items.Clear();
            lstPC.Items.Add("$" + cpu.PC.ToString("X4") + ": " + cpu.DasmNextInstruction());

            UpdateMemoryWindow();

        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            ushort bp;
            ushort prev_pc;
            lblHalted.Visible = false;
            if (txtBreakPoint.Text.Length == 4)
            {
                if (ushort.TryParse(txtBreakPoint.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out bp))
                {
                    do
                    {
                        prev_pc = cpu.PC;
                        cpu.ExecuteNext();
                    } while ( (cpu.PC != bp) && ( prev_pc != cpu.PC ));
                    if (prev_pc == cpu.PC)
                        lblHalted.Visible = true;
                }
                else
                {
                    cpu.ExecuteNext();
                }
            }
            else
            {
                cpu.ExecuteNext();
            }
            UpdateScreen();
        }

        private void UpdateMemoryWindow()
        {
            int ii;
            int idx = 0;
            bool emptyList = lstMemory.Items.Count == 0;
            int topIndex = 0;

            if(lstMemory.Items.Count > 0)
            {
                topIndex = lstMemory.TopIndex;
            }

            StringBuilder sb = new StringBuilder(100);
            for (int pc = 0x0000; pc <= 0xffff; pc += 0x10)
            {
                sb.Clear();
                sb.Append("$" + pc.ToString("X4") + ": ");
                for (ii = 0x00; ii <= 0x07; ii++)
                {
                    sb.Append(cpu.memory[pc + ii].ToString("X2") + " ");
                }
                sb.Append(" - ");
                for (; ii <= 0x0f; ii++)
                {
                    sb.Append(cpu.memory[pc + ii].ToString("X2") + " ");
                }
                sb.AppendLine();
                if (emptyList)
                    lstMemory.Items.Add(sb.ToString());
                else
                    lstMemory.Items[idx] = sb.ToString();
                idx++;
            }
            lstMemory.TopIndex = topIndex;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.KeyCode == Keys.F10)
            {
                btnStep.PerformClick();
            }
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            LoadTestProgram();
            lblHalted.Visible = false;
        }
    }
}
