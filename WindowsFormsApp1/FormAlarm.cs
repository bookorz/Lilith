//using SorterControl.Management;
using log4net;
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
        private static readonly ILog logger = LogManager.GetLogger(typeof(FormAlarm));

        public FormAlarm()
        {
            InitializeComponent();
        }

        private void ResetAll_bt_Click(object sender, EventArgs e)
        {
            Transaction Txn;

            //AlarmManagement.ClearALL();
            //AlarmUpdate.UpdateAlarmList(AlarmManagement.GetCurrent());
            logger.Debug("ResetAll_bt_Click_ClearALL_Start");
            AlarmManagement.ClearALL();
            logger.Debug("ResetAll_bt_Click_ClearALL_End");

            logger.Debug("ResetAll_bt_Click_Start");

            TaskFlowManagement.Excute(TaskFlowManagement.Command.RESET_ALL).Promise();



            //logger.Debug("ResetAll_bt_Click_UpdateAlarmList_Start");
            //AlarmUpdate.UpdateAlarmList(AlarmManagement.GetCurrent());
            //logger.Debug("ResetAll_bt_Click_UpdateAlarmList_End");
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
