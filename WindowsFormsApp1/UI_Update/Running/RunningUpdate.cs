
using Lilith.Util;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransferControl.Management;

namespace Lilith.UI_Update.Running
{
    class RunningUpdate
    {
        static ILog logger = LogManager.GetLogger(typeof(RunningUpdate));

        delegate void UpdatePortDest(string PortName, string Dest);
        delegate void UpdatePresent(string JobId);
        delegate void UpdatePortUsed(string PortName, bool Used);
        delegate void JobMoveUpdate(string Id, string FromPosition, string FromSlot, string ToPosition, string ToSlot);

        public static void UpdateModeStatus(string Status)
        {
            try
            {
                Form form = Application.OpenForms["FormRunningScreen"];
                Button Start_btn;
                if (form == null)
                    return;

                Start_btn = form.Controls.Find("Start_btn", true).FirstOrDefault() as Button;
                if (Start_btn == null)
                    return;

                if (Start_btn.InvokeRequired)
                {
                    UpdatePresent ph = new UpdatePresent(UpdateModeStatus);
                    Start_btn.BeginInvoke(ph, Status);
                }
                else
                {
                    Start_btn.Text = Status;
                    switch (Status)
                    {
                        case "Start Running":
                            Form formA = Application.OpenForms["FormMain"];

                            Button btn = formA.Controls.Find("Mode_btn", true).FirstOrDefault() as Button;
                            btn.Enabled = true;
                            Button btn2 = formA.Controls.Find("btnManual", true).FirstOrDefault() as Button;
                            btn2.Enabled = true;
                            break;
                        case "End Running":

                            break;
                    }
                }


            }
            catch
            {
                logger.Error("UpdateModeStatus: Update fail.");
            }
        }

        public static void UpdateRunningInfo(string Param, string Value)
        {
            Form form = Application.OpenForms["FormRunningScreen"];
            TextBox tb;
            if (form == null)
                return;

            tb = form.Controls.Find(Param + "_tb", true).FirstOrDefault() as TextBox;
            if (tb == null)
                return;

            if (tb.InvokeRequired)
            {
                UpdatePortDest ph = new UpdatePortDest(UpdateRunningInfo);
                tb.BeginInvoke(ph, Param, Value);
            }
            else
            {
                tb.Text = Value;
            }
        }

        public static void UpdateNodesJob(string NodeName)
        {
            try
            {
                Form form = Application.OpenForms["FormRunningScreen"];
                CheckBox tb;

                if (form == null)
                    return;

                tb = form.Controls.Find("use_loadport01_ck", true).FirstOrDefault() as CheckBox;

                if (tb == null)
                    return;

                if (tb.InvokeRequired)
                {
                    UpdatePresent ph = new UpdatePresent(UpdateNodesJob);
                    tb.BeginInvoke(ph, NodeName);
                }
                else
                {
                    Node node = NodeManagement.Get(NodeName);
                    
                    if (node.IsMapping)
                    {
                        for (int i = 1; i <= Tools.GetSlotCount(node.Type); i++)
                        {
                            Label present = form.Controls.Find(node.Name + "_Slot_" + i.ToString(), true).FirstOrDefault() as Label;
                            if (present != null)
                            {

                                Job tmp;
                                if ((tmp=JobManagement.Get(node.Name,i.ToString()))!=null)
                                {
                                    present.Text = tmp.Host_Job_Id;
                                    switch (present.Text)
                                    {
                                        case "No wafer":
                                            present.BackColor = Color.DimGray;
                                            present.ForeColor = Color.White;
                                            break;
                                        case "Crossed":
                                        case "Undefined":
                                        case "Double":
                                            present.BackColor = Color.Red;
                                            present.ForeColor = Color.White;
                                            break;
                                        default:
                                            present.BackColor = Color.Green;
                                            present.ForeColor = Color.White;
                                            break;
                                    }

                                }
                                else
                                {
                                    present.Text = "No wafer";
                                    present.BackColor = Color.DimGray;
                                    present.ForeColor = Color.White;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= Tools.GetSlotCount(node.Type); i++)
                        {
                            Label present = form.Controls.Find(node.Name + "_Slot_" + i.ToString(), true).FirstOrDefault() as Label;
                            if (present != null)
                            {
                                present.Text = "";
                                present.BackColor = Color.White;
                            }
                        }
                    }
                }


            }
            catch
            {
                logger.Error("UpdateNodesJob: Update fail.");
            }
        }
        public static void UpdateJobMove(string Id, string FromPosition, string FromSlot, string ToPosition, string ToSlot)
        {
            try
            {
                Form form = Application.OpenForms["FormRunningScreen"];
                CheckBox tb;

                if (form == null)
                    return;

                tb = form.Controls.Find("use_loadport01_ck", true).FirstOrDefault() as CheckBox;

                if (tb == null)
                    return;

                if (tb.InvokeRequired)
                {
                    JobMoveUpdate ph = new JobMoveUpdate(UpdateJobMove);
                    tb.BeginInvoke(ph, Id, FromPosition, FromSlot, ToPosition, ToSlot);
                }
                else
                {
                    Label fromposition = form.Controls.Find(FromPosition + "_Slot_" + FromSlot, true).FirstOrDefault() as Label;
                    if (fromposition != null)
                    {
                        fromposition.Text = "No wafer";
                        fromposition.BackColor = Color.DimGray;
                        fromposition.ForeColor = Color.White;

                    }

                    Label toposition = form.Controls.Find(ToPosition + "_Slot_" + ToSlot, true).FirstOrDefault() as Label;
                    if (toposition != null)
                    {
                        toposition.Text = Id;
                        toposition.BackColor = Color.Green;
                        toposition.ForeColor = Color.White;
                    }
                }
            }
            catch
            {
                logger.Error("UpdateJobMove: Update fail.");
            }


        }
        //public static void UpdateJobMove(string JobId)
        //{
        //    try
        //    {
        //        Form form = Application.OpenForms["FormRunningScreen"];
        //        CheckBox tb;

        //        if (form == null)
        //            return;

        //        tb = form.Controls.Find("use_loadport01_ck", true).FirstOrDefault() as CheckBox;

        //        if (tb == null)
        //            return;

        //        if (tb.InvokeRequired)
        //        {
        //            UpdatePresent ph = new UpdatePresent(UpdateJobMove);
        //            tb.BeginInvoke(ph, JobId);
        //        }
        //        else
        //        {
        //            Job Job = JobManagement.Get(JobId);
        //            if (Job != null)
        //            {
        //                Node LastNode = NodeManagement.Get(Job.LastNode);
        //                Node CurrentNode = NodeManagement.Get(Job.Position);
        //                if (LastNode != null)
        //                {

        //                    Label present = form.Controls.Find(Job.LastNode + "_Slot_" + Job.LastSlot, true).FirstOrDefault() as Label;
        //                    if (present != null)
        //                    {

        //                        present.Text = "No wafer";
        //                        present.BackColor = Color.DimGray;
        //                        present.ForeColor = Color.White;

        //                    }

        //                }
        //                if (CurrentNode != null)
        //                {
        //                    Label present = form.Controls.Find(Job.Position + "_Slot_" + Job.Slot, true).FirstOrDefault() as Label;
        //                    if (present != null)
        //                    {


        //                        present.Text = Job.Host_Job_Id;

        //                        present.BackColor = Color.Green;
        //                        present.ForeColor = Color.White;

        //                    }

        //                }
        //            }
        //        }


        //    }
        //    catch
        //    {
        //        logger.Error("UpdateJobMove: Update fail.");
        //    }
        //}



    }
}
