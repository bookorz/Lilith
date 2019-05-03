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
    public partial class FormOption : Form
    {
        public bool[] ProcessSlot;
        public FormOption(bool[] ProcessSlot)
        {
            InitializeComponent();
            this.ProcessSlot = ProcessSlot;
        }
        private void FormOption_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 25; i++)
            {
                (this.Controls.Find("slot" + (i + 1).ToString() + "_ck", true).FirstOrDefault() as CheckBox).Checked = ProcessSlot[i];
            }
        }
        private void select_all_btn_Click(object sender, EventArgs e)
        {
            for(int i = 1; i <= 25; i++)
            {
                (this.Controls.Find("slot"+ i.ToString() + "_ck", true).FirstOrDefault() as CheckBox).Checked = true;
            }
        }

        private void cancel_all_btn_Click(object sender, EventArgs e)
        {
            for (int i = 1; i <= 25; i++)
            {
                (this.Controls.Find("slot" + i.ToString() + "_ck", true).FirstOrDefault() as CheckBox).Checked = false;
            }
        }

        private void cancel_btn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void confirn_btn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 25; i++)
            {
                ProcessSlot[i] = (this.Controls.Find("slot" + (i+1).ToString() + "_ck", true).FirstOrDefault() as CheckBox).Checked;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

       
    }
}
