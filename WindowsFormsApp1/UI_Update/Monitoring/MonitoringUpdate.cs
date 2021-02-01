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

namespace Lilith.UI_Update.Monitoring
{
    public class MonitoringUpdate
    {
        static ILog logger = LogManager.GetLogger(typeof(MonitoringUpdate));

        delegate void UpdatePortUsed(string PortName, bool Used);
        delegate void UpdateNode(string JobId);
        delegate void UpdateEnable(bool Enable);
        delegate void UpdateLog(string msg);
        delegate void UpdateCSTMode(string PortName, string Mode);
        delegate void JobMoveUpdate(string Id, string FromPosition, string FromSlot, string ToPosition, string ToSlot);


        public static void EventUpdate(string Name,bool Checked)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                CheckBox W;
                if (form == null)
                    return;

                W = form.Controls.Find(Name+"_ck", true).FirstOrDefault() as CheckBox;
                if (W == null)
                    return;

                if (W.InvokeRequired)
                {
                    UpdatePortUsed ph = new UpdatePortUsed(EventUpdate);
                    W.BeginInvoke(ph, Name, Checked);
                }
                else
                {
                    W.Checked = Checked;

                }
            }
            catch
            {

            }
        }

        public static void ConnectUpdate(string state)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                Label W;
                if (form == null)
                    return;

                W = form.Controls.Find("ConnectStatus_lb", true).FirstOrDefault() as Label;
                if (W == null)
                    return;

                if (W.InvokeRequired)
                {
                    UpdateLog ph = new UpdateLog(ConnectUpdate);
                    W.BeginInvoke(ph, state);
                }
                else
                {
                    switch (state.ToUpper())
                    {
                        case "CONNECTED":
                            W.BackColor = Color.Lime;
                            break;
                        case "DISCONNECTED":
                            W.BackColor = Color.Red;
                            break;
                        case "CONNECTING":
                            W.BackColor = Color.Yellow;
                            break;
                    }

                }
            }
            catch
            {

            }
        }

        public static void LogUpdate(string msg)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                RichTextBox W;
                if (form == null)
                    return;

                W = form.Controls.Find("Log_rt", true).FirstOrDefault() as RichTextBox;
                if (W == null)
                    return;

                if (W.InvokeRequired)
                {
                    UpdateLog ph = new UpdateLog(LogUpdate);
                    W.BeginInvoke(ph, msg);
                }
                else
                {
                   

                    if (W.Text.Length > 13000)
                    {
                        W.Text = W.Text.Substring(W.Text.Length - 7000);
                    }
                    W.SelectionStart = W.TextLength;
                    W.SelectionLength = 0;


                    if (msg.ToUpper().Contains("ACK"))
                    {
                        W.SelectionColor = Color.Blue;
                    }
                    else if (msg.ToUpper().Contains("INF"))
                    {
                        W.SelectionColor = Color.Green;
                    }
                    else if (msg.ToUpper().Contains("ABS"))
                    {
                        W.SelectionColor = Color.Red;
                    }
                    else if (msg.ToUpper().Contains("CAN"))
                    {
                        W.SelectionColor = Color.Orange;
                    }
                    else
                    {
                        W.SelectionColor = Color.Black;
                    }
                    W.AppendText(msg + "\n");
                    W.ScrollToCaret();


                    //EventUpdate("MAPDT", FormMain.HostControl.Events.MAPDT);
                    //EventUpdate("PORT", FormMain.HostControl.Events.PORT);
                    //EventUpdate("PRS", FormMain.HostControl.Events.PRS);
                    //EventUpdate("SYSTEM", FormMain.HostControl.Events.SYSTEM);
                    //EventUpdate("TRANSREQ", FormMain.HostControl.Events.TRANSREQ);
                    //EventUpdate("FFU", FormMain.HostControl.Events.FFU);
                    //EventUpdate("BF1_BYPASS", FormMain.HostControl.Events.BF1_BYPASS);
                    //NodeManagement.Get("BF1").ByPassCheck = FormMain.HostControl.Events.BF1_BYPASS;
                    //EventUpdate("BF2_BYPASS", FormMain.HostControl.Events.BF2_BYPASS);
                    //NodeManagement.Get("BF2").ByPassCheck = FormMain.HostControl.Events.BF2_BYPASS;
                }
            }
            catch(Exception e)
            {

            }
        }

        public static void UpdateStatus(string Status)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                Button W;
                if (form == null)
                    return;

                W = form.Controls.Find("RunSwitch", true).FirstOrDefault() as Button;
                if (W == null)
                    return;

                if (W.InvokeRequired)
                {
                    UpdateNode ph = new UpdateNode(UpdateStatus);
                    W.BeginInvoke(ph, Status);
                }
                else
                {
                    if (Status.Equals("Start"))
                    {

                        W.Enabled = true;
                        W.Text = "Stop";
                        W.BackColor = Color.OrangeRed;

                    }
                    else
                    {
                        W.Text = "Start";
                        W.Enabled = false;
                        W.BackColor = Color.Lime;

                    }
                }
            }
            catch
            {
                logger.Error("UpdateStatus: Update fail.");
            }
        }

        public static void UpdateInitialButton(bool Enable)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                Button W;
                if (form == null)
                    return;

                W = form.Controls.Find("Initial_btn", true).FirstOrDefault() as Button;
                if (W == null)
                    return;

                if (W.InvokeRequired)
                {
                    UpdateEnable ph = new UpdateEnable(UpdateInitialButton);
                    W.BeginInvoke(ph, Enable);
                }
                else
                {
                    W.Enabled = Enable;
                    
                }
            }
            catch
            {
                logger.Error("UpdateInitialButton: Update fail.");
            }
        }

        public static void UpdateStartButton(bool Enable)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                Button W;
                if (form == null)
                    return;

                W = form.Controls.Find("RunSwitch", true).FirstOrDefault() as Button;
                if (W == null)
                    return;

                if (W.InvokeRequired)
                {
                    UpdateEnable ph = new UpdateEnable(UpdateStartButton);
                    W.BeginInvoke(ph, Enable);
                }
                else
                {
                    W.Enabled = Enable;
                    CheckBox AutoReverse = form.Controls.Find("AutoReverse_ck", true).FirstOrDefault() as CheckBox;
                    if (AutoReverse != null)
                    {
                        AutoReverse.Enabled = Enable;
                    }
                }
            }
            catch
            {
                logger.Error("UpdateStartButton: Update fail.");
            }
        }

        public static void UpdateWPH(string WPH)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                Label W;
                if (form == null)
                    return;

                W = form.Controls.Find("WPH", true).FirstOrDefault() as Label;
                if (W == null)
                    return;

                if (W.InvokeRequired)
                {
                    UpdateNode ph = new UpdateNode(UpdateWPH);
                    W.BeginInvoke(ph, WPH);
                }
                else
                {
                    W.Text = WPH;

                }
            }
            catch
            {
                logger.Error("UpdateWPH: Update fail.");
            }
        }

        public static void UpdateUseState(string PortName, bool used)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                TextBox Used;
                if (form == null)
                    return;

                Used = form.Controls.Find(PortName + "_FID", true).FirstOrDefault() as TextBox;
                if (Used == null)
                    return;

                if (Used.InvokeRequired)
                {
                    UpdatePortUsed ph = new UpdatePortUsed(UpdateUseState);
                    Used.BeginInvoke(ph, PortName, used);
                }
                else
                {
                    if (used)
                    {
                        //Used.Text = "Used";
                        Used.BackColor = Color.Green;
                        Used.ForeColor = Color.White;
                    }
                    else
                    {
                        //Used.Text = "Not Used";
                        Used.BackColor = Color.White;
                        Used.ForeColor = Color.Black;
                    }

                }
            }
            catch
            {
                logger.Error("UpdateUseState: Update fail.");
            }
        }
        public static void UpdateNodesJob(string NodeName)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                TextBox tb;

                if (form == null)
                    return;

                tb = form.Controls.Find("LoadPort01_State", true).FirstOrDefault() as TextBox;

                if (tb == null)
                    return;

                if (tb.InvokeRequired)
                {
                    UpdateNode ph = new UpdateNode(UpdateNodesJob);
                    tb.BeginInvoke(ph, NodeName);
                }
                else
                {
                    Node node = NodeManagement.Get(NodeName);
                    Label Mode = form.Controls.Find(NodeName + "_Mode", true).FirstOrDefault() as Label;
                    Mode.Text = node.Mode;
                    if (node.IsMapping)
                    {
                        for (int i = 1; i <= Tools.GetSlotCount(node.Type); i++)
                        {
                            Label present = form.Controls.Find(node.Name + "_Slot_" + i.ToString(), true).FirstOrDefault() as Label;
                            if (present != null)
                            {

                                Job tmp;
                                if ((tmp = JobManagement.Get(node.Name, i.ToString())) != null)
                                {
                                    ///20201202 Pingchung
                                    switch (tmp.Status)
                                    {
                                        case Job.MapStatus.Undefined:
                                            tmp.Host_Job_Id = "?";
                                            break;

                                        case Job.MapStatus.Double:
                                            tmp.Host_Job_Id = "W";
                                            break;

                                        case Job.MapStatus.Crossed:
                                            tmp.Host_Job_Id = "E";
                                            break;
                                        default:
                                            break;
                                    }

                                    present.Text = tmp.Host_Job_Id;
                                    switch (present.Text)
                                    {
                                        case "No wafer":
                                            present.BackColor = Color.DimGray;
                                            present.ForeColor = Color.White;
                                            break;
                                        case "E":
                                        case "W":
                                        case "?":
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
        public static void UpdateJobMove(string Id, string FromPosition, string FromSlot ,string ToPosition, string ToSlot)
        {
            try
            {
                Form form = Application.OpenForms["FormMonitoring"];
                TextBox tb;

                if (form == null)
                    return;

                tb = form.Controls.Find("LoadPort01_State", true).FirstOrDefault() as TextBox;

                if (tb == null)
                    return;

                if (tb.InvokeRequired)
                {
                    JobMoveUpdate ph = new JobMoveUpdate(UpdateJobMove);
                    tb.BeginInvoke(ph, Id, FromPosition, FromSlot, ToPosition, ToSlot);
                }
                else
                {
                    Label  fromposition = form.Controls.Find(FromPosition + "_Slot_" + FromSlot, true).FirstOrDefault() as Label;
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
        //        Form form = Application.OpenForms["FormMonitoring"];
        //        TextBox tb;

        //        if (form == null)
        //            return;

        //        tb = form.Controls.Find("LoadPort01_State", true).FirstOrDefault() as TextBox;

        //        if (tb == null)
        //            return;

        //        if (tb.InvokeRequired)
        //        {
        //            UpdateNode ph = new UpdateNode(UpdateJobMove);
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
        public static void CSTModeUpdate(string PortName, string Mode)
        {
            Form form = Application.OpenForms["FormMonitoring"];
            TextBox Port;
            if (form == null)
                return;

            if (form == null)
                return;

            Port = form.Controls.Find(PortName + "_CSTMode", true).FirstOrDefault() as TextBox;
            if (Port == null)
                return;

            if (Port.InvokeRequired)
            {
                UpdateCSTMode ph = new UpdateCSTMode(CSTModeUpdate);
                Port.BeginInvoke(ph, PortName, Mode);
            }
            else
            {
                if(Mode.Equals("-1"))
                {
                    Port.Visible = false;
                }
                else
                {
                    Port.Visible = true;

                    if (Mode.Equals("0"))
                    {
                        Port.Text = "No CST";
                    }
                    else if (Mode.Equals("1"))
                    {
                        Port.Text = "Pod";
                    }
                    else if (Mode.Equals("2"))
                    {
                        Port.Text = "Open CST";
                    }
                    else if(Mode.Equals("3"))
                    {
                        Port.Text = "Adapter";
                    }
                    else
                    {
                        Port.Text = "Unknow";
                    }

                }
            }

        }


    }
}
