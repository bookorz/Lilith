using Lilith.UI_Update.Monitoring;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransferControl.Management;

namespace Lilith.UI_Update.Alarm
{
    class AlarmUpdate
    {
        static ILog logger = LogManager.GetLogger(typeof(AlarmUpdate));
        delegate void UpdateAlarm(List<AlarmManagement.Alarm> AlarmList);
        delegate void UpdateSignal(string Name, string Signal);
        delegate void UpdateMsg(string Msg);

       

        

        public static void UpdateAlarmList(List<AlarmManagement.Alarm> AlarmList)
        {
            try
            {
                logger.Debug("UpdateAlarmList_1");
                Form form = Application.OpenForms["FormAlarm"];
                DataGridView AlarmList_gv;
                
                                   
                if (form == null)
                    return;

                logger.Debug("UpdateAlarmList_2");
                AlarmList_gv = form.Controls.Find("AlarmList_gv", true).FirstOrDefault() as DataGridView;
                if (AlarmList_gv == null)
                    return;

                logger.Debug("UpdateAlarmList_3");
                if (AlarmList_gv.InvokeRequired)
                {
                    UpdateAlarm ph = new UpdateAlarm(UpdateAlarmList);

                    logger.Debug("UpdateAlarmList_4");
                    //AlarmList_gv.Invoke(ph, AlarmList);
                    AlarmList_gv.BeginInvoke(ph, AlarmList);

                }
                else
                {
                    logger.Debug("UpdateAlarmList_5");
                    logger.Debug("UpdateAlarmList_AlarmList count" + AlarmList.ToList().Count.ToString());

                    AlarmList_gv.DataSource = null;
                    AlarmList_gv.DataSource = AlarmList.ToList();

                    AlarmList_gv.ClearSelection();
                    AlarmList_gv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                    if (AlarmList.Count() != 0)
                    {
                        logger.Debug("UpdateAlarmList_7");
                        form.Visible = true;
                        form.WindowState = FormWindowState.Normal;
                        Application.OpenForms[form.Name].Focus();
                    }
                    else
                    {
                        logger.Debug("UpdateAlarmList_8");
                        form.Visible = false;
                    }
                }


            }
            catch (Exception e)
            {
                logger.Error("UpdateAlarmList: Update fail." + e.Message + "\n" + e.StackTrace);
            }

        }

        public static void UpdateAlarmHistory(List<AlarmManagement.Alarm> AlarmList)
        {
            try
            {
                Form form = Application.OpenForms["FormAlarmHis"];
                DataGridView AlarmList_gv;

                if (form == null)
                    return;


                AlarmList_gv = form.Controls.Find("dg1", true).FirstOrDefault() as DataGridView;
                if (AlarmList_gv == null)
                    return;

                if (AlarmList_gv.InvokeRequired)
                {
                    UpdateAlarm ph = new UpdateAlarm(UpdateAlarmHistory);
                    AlarmList_gv.BeginInvoke(ph, AlarmList);
                }
                else
                {

                    //JobList_gv.DataSource = null;
                    AlarmList_gv.DataSource = AlarmList;

                    //Conn_gv.Refresh();
                    AlarmList_gv.ClearSelection();
                    AlarmList_gv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                }


            }
            catch (Exception e)
            {
                logger.Error("UpdateAlarmHistory: Update fail." + e.Message + "\n" + e.StackTrace);
            }

        }
    }
}
