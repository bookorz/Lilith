//using SorterControl.Management;
using Lilith.UI_Update.Alarm;
using Lilith.UI_Update.Monitoring;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransferControl.Management;

namespace Lilith
{
    public partial class FormAlarm : Form
    {
        public FormAlarm()
        {
            InitializeComponent();
        }

        private void ResetAll_bt_Click(object sender, EventArgs e)
        {
            Transaction Txn;

            //AlarmManagement.ClearALL();
            //AlarmUpdate.UpdateAlarmList(AlarmManagement.GetCurrent());

            TaskFlowManagement.Excute(TaskFlowManagement.Command.RESET_ALL).Promise();
            AlarmManagement.ClearALL();
            AlarmUpdate.UpdateAlarmList(AlarmManagement.GetCurrent());
        }

        private void AlarmFrom_Load(object sender, EventArgs e)
        {
            AlarmUpdate.UpdateAlarmList(AlarmManagement.GetCurrent());
        }

        private void AlarmFrom_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Visible = false;
        }
    }
}
