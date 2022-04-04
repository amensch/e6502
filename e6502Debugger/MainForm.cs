using KDS.e6502;
using System.Text;

namespace e6502Debugger
{
    public partial class MainForm : Form
    {
        private CPU cpu;

        public MainForm()
        {
            InitializeComponent();
            ClearScreen();
        }

        private void ClearScreen()
        {
            lblA.Text = "";
            lblX.Text = "";
            lblY.Text = "";
            lblSP.Text = "";
            lblPC.Text = "";
            lblFlags.Text = "";
            lblNextInstruction.Text = "";
            txtMemory.Text = "";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                LoadFile(dialog.FileName);
            }
        }

        private void LoadFile(string file)
        {
            var bus = new BusDevice(File.ReadAllBytes(file), 0xf000);
            cpu = new CPU(bus, e6502Type.NMOS);
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            lblA.Text = $"{cpu.A:X2}";
            lblX.Text = $"{cpu.X:X2}";
            lblY.Text = $"{cpu.Y:X2}";
            lblSP.Text = $"{cpu.SP:X2}";
            lblPC.Text = $"{cpu.PC:X4}";

            var flags = new StringBuilder();

            if (cpu.NF)
                flags.Append("N");
            else
                flags.Append("-");
            if (cpu.VF)
                flags.Append("V");
            else
                flags.Append("-");
            flags.Append("--");
            if (cpu.DF)
                flags.Append("D");
            else
                flags.Append("-");
            if (cpu.IF)
                flags.Append("I");
            else
                flags.Append("-");
            if (cpu.ZF)
                flags.Append("Z");
            else
                flags.Append("-");
            if (cpu.CF)
                flags.Append("C");
            else
                flags.Append("-");

            lblFlags.Text = flags.ToString();

            lblNextInstruction.Text = $"${cpu.PC:X4}: {cpu.DasmNextInstruction()}";

            UpdateMemory();
        }

        private void btnStep_Click(object sender, EventArgs e)
        {
            ExecuteNextInstruction();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.F5)
            {
                ExecuteNextInstruction();
            }
        }

        private void ExecuteNextInstruction()
        {
            cpu.ExecuteNext();
            UpdateScreen();
        }

        private void UpdateMemory()
        {
            var low = int.Parse(txtLowRange.Text, System.Globalization.NumberStyles.HexNumber);
            var high = int.Parse(txtHighRange.Text, System.Globalization.NumberStyles.HexNumber);

            StringBuilder sb = new StringBuilder(1000);
            for (int pc = low; pc <= high; pc += 0x10)
            {
                sb.Append($"${pc:X4}: ");
                for (int ii = 0x00; ii <= 0x07; ii++)
                {
                    sb.Append($"{cpu.SystemBus.Read((ushort)(pc + ii)):X2} ");
                }
                sb.Append(" - ");
                for (int ii = 0x08; ii <= 0x0f; ii++)
                {
                    sb.Append($"{cpu.SystemBus.Read((ushort)(pc + ii)):X2} ");
                }
                sb.AppendLine();
            }
            txtMemory.Text = sb.ToString();
        }

        private void txtLowRange_Enter(object sender, EventArgs e)
        {
            txtLowRange.SelectAll();
        }

        private void txtHighRange_Enter(object sender, EventArgs e)
        {
            txtHighRange.SelectAll();
        }
    }
}
