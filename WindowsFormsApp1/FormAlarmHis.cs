using Lilith.UI_Update.Alarm;
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
    public partial class FormAlarmHis : Form
    {
        public FormAlarmHis()
        {
            InitializeComponent();

            
        }

        private void FormAlarmHis_Load(object sender, EventArgs e)
        {
            From.Value = DateTime.Now.AddDays(-1);
            To.Value = DateTime.Now;
            AlarmUpdate.UpdateAlarmHistory((from almHis in AlarmManagement.GetHistory()
                                           where almHis.TimeStamp.CompareTo(From.Value)>=0 && almHis.TimeStamp.CompareTo(To.Value) <= 0
                                           select almHis).ToList());
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            AlarmUpdate.UpdateAlarmHistory((from almHis in AlarmManagement.GetHistory()
                                            where almHis.TimeStamp.CompareTo(From.Value) >= 0 && almHis.TimeStamp.CompareTo(To.Value) <= 0
                                            select almHis).ToList());
        }
    }
}
