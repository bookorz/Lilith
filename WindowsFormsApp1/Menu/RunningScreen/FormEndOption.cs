using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lilith.Menu.RunningScreen
{
    public partial class FormEndOption : Form
    {
        public EndOption Option = EndOption.CycleStop;
        public enum EndOption
        {
            CycleStop,
            LotEnd
        }
        public FormEndOption()
        {
            InitializeComponent();
        }

        private void cancel_btn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ok_btn_Click(object sender, EventArgs e)
        {
            if (cycle_stop_rb.Checked)
            {
                Option = EndOption.CycleStop;
            }
            else if (lot_end_rb.Checked)
            {
                Option = EndOption.LotEnd;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
