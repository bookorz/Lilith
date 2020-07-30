using Lilith.UI_Update.Monitoring;
using Lilith.UI_Update.Running;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransferControl.Engine;
using TransferControl.Management;

namespace Lilith.Menu.RunningScreen
{
    public partial class FormRunningScreen : Lilith.Menu.FormFrame
    {

        bool[] ProcessSlotList = new bool[25];
        bool Run = false;
        bool CycleStop = false;
        bool LotEnd = false;
        bool ThreadEnd = false;
        string LL = "";
        int TransCount = 0;
        string SpeedSet = "";

        List<Node> SelectLoadports = new List<Node>();

        public FormRunningScreen()
        {
            InitializeComponent();

        }
        private void FormRunningScreen_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < ProcessSlotList.Length; i++)
            {
                ProcessSlotList[i] = true;
            }
            SelectLoadports.Clear();
        }
        private void Start_btn_Click(object sender, EventArgs e)
        {
            if (Start_btn.Text.Equals("Start Running"))
            {
                if (SelectLoadports.Count != 2)
                {
                    MessageBox.Show("請選擇兩個Loadport!");
                    return;
                }
                if(LL_cb.Text.Equals("BF1")|| LL_cb.Text.Equals("BF2"))
                {
                    LL = LL_cb.Text;
                }
                else
                {
                    LL = "";
                }
                TransCount = Convert.ToInt32(TransCount_tb.Text);
                SpeedSet = RunningSpeed_cb.Text.Replace("%","");
                SpeedSet = SpeedSet.Equals("100") ? "0" : SpeedSet;

                Form form = Application.OpenForms["FormMain"];
                
                Button btn = form.Controls.Find("Mode_btn", true).FirstOrDefault() as Button;
                btn.Enabled = false;
                Button btn2 = form.Controls.Find("btnManual", true).FirstOrDefault() as Button;
                btn2.Enabled = false;

                Start_btn.Text = "End Running";
                Run = true;
                CycleStop = false;
                LotEnd = false;
                ThreadEnd = false;
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateLapsedTime));
                ThreadPool.QueueUserWorkItem(new WaitCallback(Transfer));
            }
            else
            {
                using (var form = new FormEndOption())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        switch (form.Option)
                        {
                            case FormEndOption.EndOption.CycleStop:
                                CycleStop = true;
                                break;
                            case FormEndOption.EndOption.LotEnd:
                                LotEnd = true;
                                break;
                        }
                        SpinWait.SpinUntil(() => ThreadEnd, 99999999);
                        Start_btn.Text = "Start Running";
                        Form formA = Application.OpenForms["FormMain"];
                        
                        Button btn = formA.Controls.Find("Mode_btn", true).FirstOrDefault() as Button;
                        btn.Enabled = true;
                    }

                }

            }

        }
        private void UpdateLapsedTime(object obj)
        {
            DateTime StartTime = DateTime.Now;
            while (!ThreadEnd)
            {
                SpinWait.SpinUntil(() => false, 1000);
                TimeSpan timeDiff = DateTime.Now - StartTime;
                RunningUpdate.UpdateRunningInfo("LapsedTime", timeDiff.ToString(@"hh\:mm\:ss"));
            }
        }
        private void Transfer(object obj)
        {
            int LapsedWfCount = 0;
            int LapsedLotCount = 0;
            DateTime StartTime = DateTime.Now;

           
            string Message = "";
            string TaskName = "";
            Dictionary<string, string> param = new Dictionary<string, string>();
            Node LD = SelectLoadports[1];
            Node ULD = SelectLoadports[0];
            //init
           
            if (!TaskFlowManagement.Excute( TaskFlowManagement.Command.ALL_INIT).Promise())
            {
                ThreadEnd = true;
                RunningUpdate.UpdateModeStatus("Start Running");

                return;
            }
            if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ALL_ORGSH).Promise())
            {
                ThreadEnd = true;
                RunningUpdate.UpdateModeStatus("Start Running");

                return;
            }

            //set speed
            
            param.Clear();
            param.Add("@Target","ROBOT01");
            param.Add("@Value",SpeedSet);
            if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_SPEED,param).Promise())
            {
                ThreadEnd = true;
                RunningUpdate.UpdateModeStatus("Start Running");
                return;
            }



            while (!LotEnd)
            {
                Node swap = LD;
                LD = ULD;
                ULD = swap;
                //loadport open
                
                param.Clear();
                param.Add("@Target", LD.Name);
                
              
                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_OPEN, param).Promise())
                {
                    ThreadEnd = true;
                    RunningUpdate.UpdateModeStatus("Start Running");
                    return;
                }
               
                param.Clear();
                param.Add("@Target", ULD.Name);
                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_OPEN, param).Promise())
                {
                    ThreadEnd = true;
                    RunningUpdate.UpdateModeStatus("Start Running");
                    return;
                }

                for (int i = 0; i < ProcessSlotList.Length; i++)
                {
                    TimeSpan timeDiff = DateTime.Now - StartTime;
                    RunningUpdate.UpdateRunningInfo("LapsedTime", timeDiff.ToString(@"hh\:mm\:ss"));


                    if (TransCount == 0)
                    {
                        ThreadEnd = true;
                        RunningUpdate.UpdateModeStatus("Start Running");
                        return;
                    }
                    int slotNo = i + 1;
                    bool needProcess = ProcessSlotList[i];
                    if (needProcess)
                    {
                        Job FromSlot = JobManagement.Get(LD.Name, slotNo.ToString());
                        Job ToSlot  = JobManagement.Get(ULD.Name, slotNo.ToString());

                        if (FromSlot == null || ToSlot!=null)
                        {
                            continue;
                        }
                        if (FromSlot.MapFlag && !FromSlot.ErrPosition && ToSlot == null)//check slot status by from and to 
                        {
                            if (!LL.Equals(""))
                            {//LL use
                                //TaskName = "LOAD";
                                param.Clear();
                                param.Add("@Target", "ROBOT01");
                                param.Add("@Position", LD.Name);
                                param.Add("@Slot", slotNo.ToString());
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_GET, param).Promise())
                                {
                                   
                                    ThreadEnd = true;
                                    RunningUpdate.UpdateModeStatus("Start Running");
                                    return;
                                }
                                //TaskName = "UNLOAD";
                                param.Clear();
                                param.Add("@Target", "ROBOT01");
                                param.Add("@Position", LL);
                                param.Add("@Slot", "1");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_PUT, param).Promise())
                                {
                                    ThreadEnd = true;
                                    RunningUpdate.UpdateModeStatus("Start Running");
                                    return;
                                }
                                //TaskName = "LOAD";
                                param.Clear();
                                param.Add("@Target", "ROBOT01");
                                param.Add("@Position", LL);
                                param.Add("@Slot", "1");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_GET, param).Promise())
                                {
                                    ThreadEnd = true;
                                    RunningUpdate.UpdateModeStatus("Start Running");
                                    return;
                                }
                                //TaskName = "UNLOAD";
                                param.Clear();
                                param.Add("@Target", "ROBOT01");
                                param.Add("@Position", ULD.Name);
                                param.Add("@Slot", slotNo.ToString());
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_PUT, param).Promise())
                                {
                                    ThreadEnd = true;
                                    RunningUpdate.UpdateModeStatus("Start Running");
                                    return;
                                }
                                TransCount--;
                                RunningUpdate.UpdateRunningInfo("TransCount", TransCount.ToString());
                                LapsedWfCount++;
                                RunningUpdate.UpdateRunningInfo("LapsedWfCount", LapsedWfCount.ToString());
                                if (CycleStop)
                                {
                                    break;
                                }
                            }
                            else
                            {//LL not use
                                //TaskName = "LOAD";
                                param.Clear();
                                param.Add("@Target", "ROBOT01");
                                param.Add("@Position", LD.Name);
                                param.Add("@Slot", slotNo.ToString());
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_GET, param).Promise())
                                {
                                    ThreadEnd = true;
                                    RunningUpdate.UpdateModeStatus("Start Running");
                                    return;
                                }
                                //TaskName = "UNLOAD";
                                param.Clear();
                                param.Add("@Target", "ROBOT01");
                                param.Add("@Position", ULD.Name);
                                param.Add("@Slot", slotNo.ToString());
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_PUT, param).Promise())
                                {
                                    ThreadEnd = true;
                                    RunningUpdate.UpdateModeStatus("Start Running");
                                    return;
                                }
                                TransCount--;
                                RunningUpdate.UpdateRunningInfo("TransCount", TransCount.ToString());
                                LapsedWfCount++;
                                RunningUpdate.UpdateRunningInfo("LapsedWfCount", LapsedWfCount.ToString());
                                if (CycleStop)
                                {
                                    break;
                                }
                            }
                            LapsedLotCount++;
                            RunningUpdate.UpdateRunningInfo("LapsedLotCount", LapsedLotCount.ToString());


                        }
                    }
                }
                if (CycleStop)
                {
                    break;
                }
                LapsedLotCount++;
                RunningUpdate.UpdateRunningInfo("LapsedLotCount", LapsedLotCount.ToString());
                //TaskName = "CLOSE";
                param.Clear();
                param.Add("@Target", LD.Name);
                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_CLOSE, param).Promise())
                {
                    ThreadEnd = true;
                    RunningUpdate.UpdateModeStatus("Start Running");
                    return;
                }
                //TaskName = "CLOSE";
                param.Clear();
                param.Add("@Target", ULD.Name);
                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_CLOSE, param).Promise())
                {
                    ThreadEnd = true;
                    RunningUpdate.UpdateModeStatus("Start Running");
                    return;
                }
                if (LotEnd)
                {
                    break;
                }
            }
            ThreadEnd = true;
        }

        private void RunningSpeed_cb_TextChanged(object sender, EventArgs e)
        {
            //string strMsg = "確定要修改整機速度?";
            //if (MessageBox.Show(strMsg, "ChangeSpeed", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            //{
            //    ChangeSpeed();
               
            //}
        }

        //private void ChangeSpeed()
        //{
        //    string sp = RunningSpeed_cb.Text.Replace("%", "");
        //    if (sp.Equals("100"))
        //    {
        //        sp = "0";
        //    }
        //    foreach (Node node in NodeManagement.GetList())
        //    {
        //        string Message = "";
        //        if (node.Type.Equals("ROBOT"))
        //        {
        //            Transaction txn = new Transaction();
        //            txn.Method = Transaction.Command.RobotType.Speed;
        //            txn.Value = sp;
        //            txn.FormName = "Running";
        //            node.SendCommand(txn, out Message);
        //        }
        //        else
        //        if (node.Type.Equals("ALIGNER"))
        //        {
        //            Transaction txn = new Transaction();
        //            txn.Method = Transaction.Command.AlignerType.Speed;
        //            txn.Value = sp;
        //            txn.FormName = "Running";
        //            node.SendCommand(txn, out Message);
        //        }
        //    }
        //}



        private void option_btn_Click(object sender, EventArgs e)
        {
            using (var form = new FormOption(ProcessSlotList))
            {
                var result = form.ShowDialog();
                //if (result == DialogResult.OK)
                //{

                //}
            }
        }


        private void use_loadport_ck_CheckedChanged(object sender, EventArgs e)
        {
            string LoadportName = ((CheckBox)sender).Name.Replace("use_", "").Replace("_ck", "").ToUpper();
            Node port = NodeManagement.Get(LoadportName);
            if (((CheckBox)sender).Checked)
            {
                SelectLoadports.Add(port);
                if (SelectLoadports.Count >= 2)
                {
                    if (!use_loadport01_ck.Checked) { use_loadport01_ck.Enabled = false; }
                    if (!use_loadport02_ck.Checked) { use_loadport02_ck.Enabled = false; }
                    if (!use_loadport03_ck.Checked) { use_loadport03_ck.Enabled = false; }
                    if (!use_loadport04_ck.Checked) { use_loadport04_ck.Enabled = false; }
                }
            }
            else
            {
                SelectLoadports.Remove(port);
                if (SelectLoadports.Count < 2)
                {
                    if (!use_loadport01_ck.Checked) { use_loadport01_ck.Enabled = true; }
                    if (!use_loadport02_ck.Checked) { use_loadport02_ck.Enabled = true; }
                    if (!use_loadport03_ck.Checked) { use_loadport03_ck.Enabled = true; }
                    if (!use_loadport04_ck.Checked) { use_loadport04_ck.Enabled = true; }
                }
            }

        }

        private void FormRunningScreen_FormClosing(object sender, FormClosingEventArgs e)
        {
            Run = false;
        }
    }
}
