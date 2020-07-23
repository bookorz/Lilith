using Lilith.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransferControl.Management;

namespace Lilith.UI_Update.Manual
{
    class ManualRobotStatusUpdate
    {
        static ILog logger = LogManager.GetLogger(typeof(ManualRobotStatusUpdate));
        //delegate void ShowMessage(string str);
        delegate void UpdateGUI_D(Transaction txn, string name, string msg);
        delegate void UpdateStatus_D(string device);
        delegate void UpdateRobotStatus_D(string name, string status);

        public static void UpdateManual(string Key, string Val)
        {
            Form manual = Application.OpenForms["FormManual"];

            if (manual == null)
                return;
            Control ctrl = manual.Controls.Find(Key, true).FirstOrDefault() as Control;
            if (ctrl == null)
                return;
            if (ctrl.InvokeRequired)
            {
                UpdateRobotStatus_D ph = new UpdateRobotStatus_D(UpdateManual);
                ctrl.BeginInvoke(ph, Key, Val);
            }
            else
            {
               
                 ctrl.Text = Val;
                
            }
        }
        public static void UpdateCbx(string Key, string Val)
        {
            Form manual = Application.OpenForms["FormManual"];

            if (manual == null)
                return;
            ComboBox ctrl = manual.Controls.Find(Key, true).FirstOrDefault() as ComboBox;
            if (ctrl == null)
                return;
            if (ctrl.InvokeRequired)
            {
                UpdateRobotStatus_D ph = new UpdateRobotStatus_D(UpdateCbx);
                ctrl.BeginInvoke(ph, Key, Val);
            }
            else
            {

                ctrl.SelectedIndex = Convert.ToInt16(Val);

            }
        }
        
    }
}
