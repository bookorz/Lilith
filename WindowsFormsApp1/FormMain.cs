using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TransferControl.Engine;
using TransferControl.Management;
using TransferControl.Parser;
using log4net.Config;
using Lilith.UI_Update.Monitoring;
using log4net;
using Lilith.UI_Update.Manual;

using System.Threading;
using Lilith.UI_Update.Layout;
using Lilith.UI_Update.Alarm;
using GUI;
using Lilith.UI_Update.Running;
using System.Linq;
using System.Collections.Concurrent;
using Lilith.Util;
using EFEMInterface.MessageInterface;
using EFEMInterface;
using static EFEMInterface.MessageInterface.RorzeInterface;
using System.Security.Cryptography;
using System.Text;
using TransferControl.CommandConvert;
using TransferControl.Config;

namespace Lilith
{
    public partial class FormMain : Form, IUserInterfaceReport
    {
        public static TaskFlowManagement TaskFlowCtrl;
        public static RorzeInterface HostControl;
        private static readonly ILog logger = LogManager.GetLogger(typeof(FormMain));

        public static bool AutoReverse = true;

        FormAlarm alarmFrom = new FormAlarm();
        private Menu.Monitoring.FormMonitoring formMonitoring = new Menu.Monitoring.FormMonitoring();

        private Menu.RunningScreen.FormRunningScreen formTestMode = new Menu.RunningScreen.FormRunningScreen();

        public static GUI.FormManual formManual = null;

        public FormMain()
        {
            InitializeComponent();

            //讀取log4net相關參數
            XmlConfigurator.Configure();

            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(-200, 0);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Int32 oldWidth = this.Width;
            Int32 oldHeight = this.Height;

            this.WindowState = FormWindowState.Normal;
            this.Width = 1;
            this.Height = 1;

            Control[] ctrlForm = new Control[] { formMonitoring, formTestMode };

            try
            {
                for (int i = 0; i < ctrlForm.Length; i++)
                {
                    ((Form)ctrlForm[i]).TopLevel = false;
                    tbcMian.TabPages[i].Controls.Add(((Form)ctrlForm[i]));
                    ((Form)ctrlForm[i]).Show();
                    tbcMian.SelectTab(i);
                }

                tbcMian.SelectTab(0);

                alarmFrom.Show();
                //alarmFrom.SendToBack();
                alarmFrom.Hide();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            //Thread.Sleep(2000);

            if (SplashScreen.Instance != null)
            {
                SplashScreen.Instance.BeginInvoke(new MethodInvoker(SplashScreen.Instance.Dispose));
                SplashScreen.Instance = null;
            }
            this.Width = oldWidth;
            this.Height = oldHeight;
            this.WindowState = FormWindowState.Maximized;

            //建立與上位的通訊的Interface
            HostControl = new RorzeInterface(this);

            //任務流程管理
            //1.依照不同的機型搭配不同的工作流程(TaskFlow)
            //2.相關硬體的初始化與參數檔讀取(MainControl)
            TaskFlowCtrl = new TaskFlowManagement(HostControl);

            this.Width = oldWidth;
            this.Height = oldHeight;
            this.WindowState = FormWindowState.Maximized;
            HostControl.Events = new ReportEvent();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("RED", "False");
            param.Add("ORANGE", "False");
            param.Add("GREEN", "False");
            param.Add("BLUE", "False");
            param.Add("BUZZER1", "False");
            param.Add("BUZZER2", "False");
            TaskFlowCtrl.Ctrl.DIO.SetIO(param);


            ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateCheckBox));

        }

        private void UpdateCheckBox(object input)
        {
            FormMain.HostControl.Events.Load();
            MonitoringUpdate.EventUpdate("MAPDT", FormMain.HostControl.Events.MAPDT);
            MonitoringUpdate.EventUpdate("PORT", FormMain.HostControl.Events.PORT);
            MonitoringUpdate.EventUpdate("PRS", FormMain.HostControl.Events.PRS);
            MonitoringUpdate.EventUpdate("SYSTEM", FormMain.HostControl.Events.SYSTEM);
            MonitoringUpdate.EventUpdate("TRANSREQ", FormMain.HostControl.Events.TRANSREQ);
            MonitoringUpdate.EventUpdate("FFU", FormMain.HostControl.Events.FFU);
            MonitoringUpdate.EventUpdate("BF1_BYPASS", NodeManagement.Get("BF1").ByPassCheck);
    
            MonitoringUpdate.EventUpdate("BF2_BYPASS", NodeManagement.Get("BF2").ByPassCheck);
          
            DIOUpdate.UpdateDIOStatus("RED", "False");
            DIOUpdate.UpdateDIOStatus("ORANGE", "False");
            DIOUpdate.UpdateDIOStatus("GREEN", "False");
            DIOUpdate.UpdateDIOStatus("BLUE", "False");
            DIOUpdate.UpdateDIOStatus("BUZZER1", "False");
            DIOUpdate.UpdateDIOStatus("BUZZER2", "False");

            foreach (Node node in NodeManagement.GetList())
            {
                MonitoringUpdate.EventUpdate(node.Name + "_Enable", node.Enable);
            }
        }

        private void LoadPort01_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ((DataGridView)sender).ClearSelection();
        }

        private void btnLogInOut_Click(object sender, EventArgs e)
        {
            switch (btnLogInOut.Text)
            {
                case "Login":
                    GUI.FormLogin formLogin = new GUI.FormLogin();
                    formLogin.ShowDialog();
                    break;
                case "Logout":

                    break;
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FormAlarmHis form4 = new FormAlarmHis();
            form4.Text = "Message History";
            form4.label21.Text = "Message History";
            form4.Show();
        }

        private void bBBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAlarmHis form4 = new FormAlarmHis();
            form4.Show();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            string strMsg = "This equipment performs the initialization and origin search OK?\r\n" + "This equipment will be initalized, each axis will return to home position.\r\n" + "Check the condition of the wafer.";
            if (MessageBox.Show(strMsg, "Initialize", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                foreach (Node each in NodeManagement.GetList())
                {
                    string Message = "";
                    switch (each.Type)
                    {
                        case "ALIGNER":
                            each.ErrorMsg = "";
                            //each.ExcuteScript("AlignerStateGet", "GetStatsBeforeInit", out Message);
                            break;
                        case "ROBOT":
                            each.ErrorMsg = "";
                            //each.ExcuteScript("RobotStateGet", "GetStatsBeforeInit", out Message);
                            break;
                    }
                }
            }
        }


        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            //string strMsg = "Move to Home position. OK?";
            //if (MessageBox.Show(strMsg, "Org.Back", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            //{
            //    string Message = "";
            //    Transaction txn = new Transaction();
            //    txn.Method = Transaction.Command.RobotType.Home;
            //    NodeManagement.Get("Robot01").SendCommand(txn);
            //    txn = new Transaction();
            //    txn.Method = Transaction.Command.RobotType.Home;
            //    NodeManagement.Get("Robot02").SendCommand(txn);
            //}
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            string strMsg = "Switching to manual mode.\r\n" + "In this mode, your operation may damage the equipment.\r\n" + "Suffcient cautions are required for your operation.";
            //if (MessageBox.Show(strMsg, "Manual", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification) == DialogResult.OK)
            //{
            GUI.FormManual formManual = new GUI.FormManual();
            formManual.Show();
            //}
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            FormUnitCtrlData form2 = new FormUnitCtrlData();
            form2.ShowDialog();
        }

        private void toolStripMenuItem10_Click(object sender, EventArgs e)
        {
            FormTransTest form3 = new FormTransTest();
            form3.ShowDialog();
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            UI_TEST.IO iO = new UI_TEST.IO();
            iO.ShowDialog();
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            UI_TEST.LLSetting lLSetting = new UI_TEST.LLSetting();
            lLSetting.ShowDialog();
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UI_TEST.Setting setting = new UI_TEST.Setting();
            setting.ShowDialog();
        }

        private void terminalToolStripMenuItem_Click_1(object sender, EventArgs e)
        {

        }

        private void btnTeach_Click(object sender, EventArgs e)
        {
            UI_TEST.Teaching teaching = new UI_TEST.Teaching();
            teaching.ShowDialog();
        }

        private void btnVersion_Click(object sender, EventArgs e)
        {
            GUI.FormVersion formVersion = new GUI.FormVersion();
            formVersion.ShowDialog();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            GUI.FormLogSave formLogSave = new GUI.FormLogSave();
            formLogSave.ShowDialog();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormAlarm alarmFrom = new FormAlarm();
            alarmFrom.Text = "MessageFrom";
            alarmFrom.BackColor = Color.Blue;
            alarmFrom.ResetAll_bt.Enabled = false;
            alarmFrom.ShowDialog();
        }

        private void aAAToolStripMenuItem_Click(object sender, EventArgs e)
        {

            alarmFrom.Visible = true;
        }

        public void On_Command_Excuted(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {
            logger.Debug("On_Command_Excuted");
            string Message = "";

            Transaction SendTxn = new Transaction();

            if (Txn.Method == Transaction.Command.LoadPortType.Reset)
            {
                AlarmUpdate.UpdateAlarmList(AlarmManagement.GetCurrent());
            }



            switch (Node.Type)
            {
                case "LOADPORT":

                    ManualPortStatusUpdate.UpdateLog(Node.Name, Msg.Command + " Excuted");

                    switch (Txn.Method)
                    {
                        //case Transaction.Command.LoadPortType.GetMapping:
                        case Transaction.Command.LoadPortType.Unload:
                        case Transaction.Command.LoadPortType.MappingUnload:
                        case Transaction.Command.LoadPortType.DoorUp:
                        case Transaction.Command.LoadPortType.InitialPos:
                        case Transaction.Command.LoadPortType.ForceInitialPos:

                            MonitoringUpdate.UpdateNodesJob(Node.Name);
                            RunningUpdate.UpdateNodesJob(Node.Name);
                            break;
                        //case Transaction.Command.LoadPortType.GetCassetteSize:
                        //    ManualPortStatusUpdate.UpdateParameter("CASSETTE_SIZE_tb", Msg.Value);
                        //    break;
                        //case Transaction.Command.LoadPortType.GetSlotOffset:
                        //    ManualPortStatusUpdate.UpdateParameter("SLOT_OFFSET_tb", Msg.Value);
                        //    break;
                        //case Transaction.Command.LoadPortType.GetWaferOffset:
                        //    ManualPortStatusUpdate.UpdateParameter("WAFER_OFFSET_tb", Msg.Value);
                        //    break;
                        //case Transaction.Command.LoadPortType.GetTweekDistance:
                        //    ManualPortStatusUpdate.UpdateParameter("TWEEK_tb", Msg.Value);
                        //    break;
                        //case Transaction.Command.LoadPortType.GetSlotPitch:
                        //    ManualPortStatusUpdate.UpdateParameter("SLOT_PITCH_tb", Msg.Value);
                        //    break;
                        //case Transaction.Command.LoadPortType.ReadVersion:
                        //    ManualPortStatusUpdate.UpdateVersion(Node.Name, Msg.Value);
                        //    break;
                        //case Transaction.Command.LoadPortType.GetLED:
                        //    ManualPortStatusUpdate.UpdateLED(Node.Name, Msg.Value);
                        //    break;
                        //case Transaction.Command.LoadPortType.ReadStatus:
                        //    ManualPortStatusUpdate.UpdateSmifStatus(Node.Name, Msg.Value);
                        //    break;
                        //case Transaction.Command.LoadPortType.GetCount:

                        //    break;
                        case Transaction.Command.LoadPortType.GetMapping:
                        case Transaction.Command.LoadPortType.GetMappingDummy:
                            ManualPortStatusUpdate.UpdateMapping(Node.Name, Msg.Value);
                            MonitoringUpdate.UpdateNodesJob(Node.Name);
                            RunningUpdate.UpdateNodesJob(Node.Name);
                            break;
                    }
                    break;
                case "ROBOT":
                    switch (Txn.Method)
                    {
                        case Transaction.Command.RobotType.GetMapping:

                            MonitoringUpdate.UpdateNodesJob(Node.CurrentPosition);
                            RunningUpdate.UpdateNodesJob(Node.CurrentPosition);
                            break;
                        case Transaction.Command.RobotType.GetStatus:
                            ManualRobotStatusUpdate.UpdateManual("tbRServo",Node.Servo);//update 手動功能畫面
                            break;
                        case Transaction.Command.RobotType.GetSpeed:
                            ManualRobotStatusUpdate.UpdateManual("nudRSpeed", Node.Speed);//update 手動功能畫面
                            break;
                        case Transaction.Command.RobotType.GetRIO:
                            ManualRobotStatusUpdate.UpdateManual("tbRRVacuSolenoid", Node.R_Vacuum_Solenoid);//update 手動功能畫面
                            break;
                        case Transaction.Command.RobotType.GetError:
                            ManualRobotStatusUpdate.UpdateManual("tbRError", Node.LastError);//update 手動功能畫面
                            break;
                        case Transaction.Command.RobotType.GetMode:
                            ManualRobotStatusUpdate.UpdateCbx("cbRMode", Node.Mode);//update 手動功能畫面
                            break;
                        //case Transaction.Command.RobotType.GetStatus:
                        case Transaction.Command.RobotType.GetSV:
                           // ManualRobotStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 手動功能畫面
                            break;
                        case Transaction.Command.RobotType.GetCombineStatus:
                            //ManualRobotStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Command);//update 手動功能畫面
                            break;
                    }
                    break;
            }   
        }

        public void On_Command_Error(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {
            switch (Txn.TaskObj.Id)
            {
                case "FormManual":
                    switch (Node.Type)
                    {
                        case "LOADPORT":
                            //ManualPortStatusUpdate.LockUI(false);
                            break;

                    }
                    break;
            }
            logger.Debug("On_Command_Error");


        }

        public void On_Command_Finished(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {
            logger.Debug("On_Command_Finished");
            //Transaction txn = new Transaction();
            switch (Node.Type)
            {
                case "SMARTTAG":
                    switch (Txn.Method)
                    {
                        case Transaction.Command.SmartTagType.GetLCDData:
                            ManualPortStatusUpdate.UpdateID(Msg.Value);
                            break;
                    }
                    ManualPortStatusUpdate.LockUI(false);
                    break;
                case "LOADPORT":

                    ManualPortStatusUpdate.UpdateLog(Node.Name, Msg.Command + " Finished");
                    //ManualPortStatusUpdate.LockUI(false);
                    switch (Txn.Method)
                    {
                        case Transaction.Command.LoadPortType.GetMapping:
                            ManualPortStatusUpdate.UpdateMapping(Node.Name, Node.MappingResult);
                            MonitoringUpdate.UpdateNodesJob(Node.Name);
                            RunningUpdate.UpdateNodesJob(Node.Name);
                            break;
                    }
                    break;

                case "ROBOT":
                    //ManualRobotStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 手動功能畫面
                    break;
                case "ALIGNER":
                   // ManualAlignerStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 手動功能畫面
                    break;
            }
        }

        public void On_Command_TimeOut(Node Node, Transaction Txn)
        {
            logger.Debug("On_Command_TimeOut");

        }

        public void On_Event_Trigger(Node Node, CommandReturnMessage Msg)
        {
            logger.Debug("On_Event_Trigger");

            try
            {
                Transaction txn = new Transaction();
                switch (Node.Type)
                {
                    case "LOADPORT":

                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace, e);
            }

        }

        public void On_Node_State_Changed(Node Node, string Status)
        {
            logger.Debug("On_Node_State_Changed");
            NodeStatusUpdate.UpdateNodeState(Node.Name, Status);
            switch (Node.Name)
            {
                case "ROBOT01":
                    ManualRobotStatusUpdate.UpdateManual("tbRStatus", Status);//update 手動功能畫面
                    break;
                
            }
        }

        public void On_Eqp_State_Changed(string OldStatus, string NewStatus)
        {
            // NodeStatusUpdate.UpdateCurrentState(NewStatus);
            //StateRecord.EqpStateUpdate("Sorter", OldStatus, NewStatus);
        }

        public void On_Controller_State_Changed(string Device_ID, string Status)
        {

            ConnectionStatusUpdate.UpdateControllerStatus(Device_ID, Status);

            //if (Status.Equals("Connected"))
            //{
            //    //當Loadport連線成功，檢查狀態，進行燈號顯示
            //    // var findPort = from port in NodeManagement.GetLoadPortList()
            //    //               where port.Controller.Equals(Device_ID) && !port.ByPass && port.Type.Equals("LOADPORT")
            //    //               select port;

            //    //foreach (Node port in findPort)
            //    //{
            //    //    port.ExcuteScript("LoadPortFoupOut", "LoadPortFoup", "", true);
            //    //}
            //    CommunicationsUpdate.UpdateConnection(Device_ID, true);
            //}
            //else
            //{
            //    CommunicationsUpdate.UpdateConnection(Device_ID, false);
            //}
            switch (Status)
            {
                case "Connected":

                    break;
                case "Connection_Error":

                    break;
            }


            logger.Debug("On_Controller_State_Changed");
        }







        public void On_Mode_Changed(string Mode)
        {
            logger.Debug("On_Mode_Changed");

            ConnectionStatusUpdate.UpdateModeStatus(Mode);
            RunningUpdate.UpdateModeStatus(Mode);
            MonitoringUpdate.UpdateStatus(Mode);


        }

        public void On_Job_Location_Changed(Job Job)
        {
            logger.Debug("On_Job_Location_Changed");
            MonitoringUpdate.UpdateJobMove(Job.Uid);
            RunningUpdate.UpdateJobMove(Job.Uid);

        }

        public void On_CST_Mode_Changed(Node node)
        {
            logger.Debug("On_CST_Mode_Changed");
            MonitoringUpdate.CSTModeUpdate(node.Name, node.Workpiece.ToString());
        }

        public void On_Connection_Status_Report(string DIOName, string Status)
        {
            ConnectionStatusUpdate.UpdateControllerStatus(DIOName, Status);
        }


       
        public void On_Alarm_Happen(AlarmManagement.Alarm Alarm)
        {

            AlarmUpdate.UpdateAlarmList(AlarmManagement.GetCurrent());
            //AlarmUpdate.UpdateAlarmHistory(AlarmManagement.GetHistory());
        }


        public void On_Connection_Error(string DIOName, string ErrorMsg)
        {

        }





        private void Signal_MouseClick(object sender, MouseEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "RED_Signal":
                    if (MainControl.Instance.DIO.GetIO("DOUT", "RED").ToUpper().Equals("TRUE"))
                    {
                        MainControl.Instance.DIO.SetIO("RED", "False");
                    }
                    else
                    {
                        MainControl.Instance.DIO.SetIO("RED", "True");
                    }
                    break;
                case "ORANGE_Signal":
                    if (MainControl.Instance.DIO.GetIO("DOUT", "ORANGE").ToUpper().Equals("TRUE"))
                    {
                        MainControl.Instance.DIO.SetIO("ORANGE", "False");
                    }
                    else
                    {
                        MainControl.Instance.DIO.SetIO("ORANGE", "True");
                    }
                    break;
                case "GREEN_Signal":
                    if (MainControl.Instance.DIO.GetIO("DOUT", "GREEN").ToUpper().Equals("TRUE"))
                    {
                        MainControl.Instance.DIO.SetIO("GREEN", "False");
                    }
                    else
                    {
                        MainControl.Instance.DIO.SetIO("GREEN", "True");
                    }
                    break;
                case "BLUE_Signal":
                    if (MainControl.Instance.DIO.GetIO("DOUT", "BLUE").ToUpper().Equals("TRUE"))
                    {
                        MainControl.Instance.DIO.SetIO("BLUE", "False");
                    }
                    else
                    {
                        MainControl.Instance.DIO.SetIO("BLUE", "True");
                    }
                    break;
                case "BUZZER1_Signal":
                    if(MainControl.Instance.DIO.GetIOStatus("DOUT", "BUZZER2").ToUpper().Equals("BLINK") ||
                        MainControl.Instance.DIO.GetIOStatus("DOUT", "BUZZER2").ToUpper().Equals("BLINKSTOP"))
                    {
                        if(MainControl.Instance.DIO.GetIOStatus("DOUT", "BUZZER2").ToUpper().Equals("BLINK"))
                        {
                            MainControl.Instance.DIO.SetBlink("BUZZER2", "False");
                            MainControl.Instance.DIO.SetBlinkStop("BUZZER2", "True");
                            MainControl.Instance.DIO.SetShortBlink("BUZZER1", "True");
                        }
                        else
                        {
                            MainControl.Instance.DIO.SetIO("BUZZER1", "False");
                            MainControl.Instance.DIO.SetBlink("BUZZER2", "True");
                        }
                    }
                    else
                    {
                        if (MainControl.Instance.DIO.GetIOStatus("DOUT", "BUZZER1").ToUpper().Equals("SHORTBLINK"))
                        {
                            MainControl.Instance.DIO.SetIO("BUZZER1", "False");
                        }
                        else
                        {
                            MainControl.Instance.DIO.SetShortBlink("BUZZER1", "True");
                        }
                    }

                    break;
                case "BUZZER2_Signal":
                    if (MainControl.Instance.DIO.GetIOStatus("DOUT", "BUZZER2").ToUpper().Equals("BLINK"))
                    {
                        MainControl.Instance.DIO.SetBlinkStop("BUZZER2", "False");
                        MainControl.Instance.DIO.SetIO("BUZZER2", "False");
                    }
                    else
                    if (MainControl.Instance.DIO.GetIOStatus("DOUT", "BUZZER2").ToUpper().Equals("BLINKSTOP"))
                    {
                        MainControl.Instance.DIO.SetBlinkStop("BUZZER2", "false");
                        MainControl.Instance.DIO.SetIO("BUZZER2", "False");
                        MainControl.Instance.DIO.SetShortBlink("BUZZER1", "True");
                    }
                    else
                    {
                        if (MainControl.Instance.DIO.GetIOStatus("DOUT", "BUZZER1").ToUpper().Equals("SHORTBLINK"))
                        {
                            MainControl.Instance.DIO.SetBlinkStop("BUZZER2", "True");
                        }
                        else
                        {
                            MainControl.Instance.DIO.SetBlink("BUZZER2", "True");
                        }
                    }
                    break;
            }
        }

        private void Conn_gv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 1:
                    switch (e.Value)
                    {
                        case "Connecting":
                            e.CellStyle.BackColor = Color.Yellow;
                            e.CellStyle.ForeColor = Color.Black;
                            break;
                        case "Connected":
                            e.CellStyle.BackColor = Color.Green;
                            e.CellStyle.ForeColor = Color.White;
                            break;
                        default:
                            e.CellStyle.BackColor = Color.Red;
                            e.CellStyle.ForeColor = Color.White;
                            break;

                    }
                    break;

            }
        }

        private void Connection_btn_Click(object sender, EventArgs e)
        {

            //if (Connection_btn.Tag.ToString() == "Offline")
            //{
            //    RouteCtrl.ConnectAll();

            //    ConnectionStatusUpdate.UpdateOnlineStatus("Connecting");
            //}
            //else
            //{
            //    RouteCtrl.DisconnectAll();
            //    ConnectionStatusUpdate.UpdateOnlineStatus("Offline");
            //}

        }

        private void Mode_btn_Click(object sender, EventArgs e)
        {

            if (Mode_btn.Text.Equals("Manual-Mode"))
            {
                HostControl.OnlineMode = true;
                Mode_btn.Text = "Online-Mode";
                Mode_btn.BackColor = Color.Green;
                btnManual.Enabled = false;
                btnManual.BackColor = Color.Gray;
                tbcMian.Enabled = false;
                tbcMian.SelectedIndex = 0;
                if (formManual != null)
                {
                    formManual.Close();
                }
            }
            else
            {
                //check 密碼
                MD5 md5 = MD5.Create();
                string[] use_info = ShowLoginDialog();
                string user_id = use_info[0];
                string password = use_info[1];
                byte[] source = Encoding.Default.GetBytes(password);//將字串轉為Byte[]
                byte[] crypto = md5.ComputeHash(source);//進行MD5加密
                string md5_result = BitConverter.ToString(crypto).Replace("-", String.Empty).ToUpper();//取得 MD5
                string config_password = SystemConfig.Get().AdminPassword;
                if (md5_result.Equals(config_password))
                {
                    HostControl.OnlineMode = false;
                    Mode_btn.Text = "Manual-Mode";
                    Mode_btn.BackColor = Color.Orange;
                    btnManual.Enabled = true;
                    btnManual.BackColor = Color.Orange;
                    tbcMian.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Password incorrect !!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    btnManual.Enabled = false;
                    btnManual.BackColor = Color.Gray;
                }

            }
        }


        public static string[] ShowLoginDialog()
        {
            string[] result = new string[] { "", "" };
            Form prompt = new Form()
            {
                Width = 450,
                Height = 280,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Authority check",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label lblUser = new Label() { Left = 30, Top = 20, Text = "User", Width = 200 };
            TextBox tbUser = new TextBox() { Left = 30, Top = 50, Width = 350, Text = "Administrator" };
            Label lblPassword = new Label() { Left = 30, Top = 90, Text = "Password", Width = 200 };
            TextBox tbPassword = new TextBox() { Left = 30, Top = 120, Width = 350 };
            tbPassword.PasswordChar = '*';
            Button confirmation = new Button() { Text = "Ok", Left = 280, Width = 100, Top = 170, DialogResult = DialogResult.OK, Height = 35 };
            lblUser.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblPassword.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tbUser.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tbPassword.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            confirmation.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(lblUser);
            prompt.Controls.Add(tbUser);
            prompt.Controls.Add(tbPassword);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(lblPassword);
            prompt.AcceptButton = confirmation;
            tbPassword.Focus();
            tbUser.Enabled = false;

            if (prompt.ShowDialog() == DialogResult.OK)
            {
                result[0] = tbUser.Text;
                result[1] = tbPassword.Text;
            }
            return result;
        }

        public void On_InterLock_Report(Node Node, bool InterLock)
        {
            //throw new NotImplementedException();
        }

        //private void Pause_btn_Click(object sender, EventArgs e)
        //{
        //    if (RouteCtrl.GetMode().Equals("Start"))
        //    {

        //        RouteCtrl.Pause();
        //        NodeStatusUpdate.UpdateCurrentState("Run");
        //        Pause_btn.Text = "Continue";

        //    }
        //    else if (RouteCtrl.GetMode().Equals("Pause"))
        //    {
        //        RouteCtrl.Continue();
        //        NodeStatusUpdate.UpdateCurrentState("Idle");
        //        Pause_btn.Text = "Pause";
        //    }
        //}

        private void btnHelp_Click(object sender, EventArgs e)
        {
            FormQuery form = new FormQuery();
            form.Show();
        }


        private void tbcMian_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void menuMaintenace_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Conn_gv_RowEnter(object sender, DataGridViewCellEventArgs e)
        {

        }

      

        public void On_EFEM_Status_changed(string status)
        {

            NodeStatusUpdate.UpdateCurrentState(status);
        }

       

      

       

        public void On_TaskJob_Aborted(TaskFlowManagement.CurrentProcessTask Task)
        {
            ManualPortStatusUpdate.LockUI(false);
        }

        public void On_TaskJob_Finished(TaskFlowManagement.CurrentProcessTask Task)
        {
            
                ManualPortStatusUpdate.LockUI(false);
            
        }

        private void btnManual_Click(object sender, EventArgs e)
        {
            if (formManual == null)
            {
                formManual = new GUI.FormManual();
                formManual.Show();
            }
            else
            {
                formManual.Focus();
            }
        }

        public void On_Node_Connection_Changed(string NodeName, string Status)
        {
            if (NodeName.Equals("EFEM"))
            {
                switch (Status)
                {
                    case "Connected":
                        ConnectionStatusUpdate.UpdateOnlineStatus("Online");
                        MonitoringUpdate.LogUpdate("Connected");
                        break;
                    case "Connecting":
                        ConnectionStatusUpdate.UpdateOnlineStatus("Connecting");
                        MonitoringUpdate.LogUpdate("Connecting");
                        break;
                    case "Disconnected":
                        ConnectionStatusUpdate.UpdateOnlineStatus("Offline");
                        MonitoringUpdate.LogUpdate("Disconnected");
                        break;
                }

            }
            else
            {
                ConnectionStatusUpdate.UpdateControllerStatus(NodeName, Status);
                Node node = NodeManagement.Get(NodeName);

            }


            logger.Debug("On_Node_Connection_Changed");
        }


        public void On_DIO_Data_Chnaged(string Parameter, string Value, string Type)
        {
            switch (Parameter)
            {
                case "BF1_DOOR_OPEN":
                case "BF1_ARM_EXTEND_ENABLE":
                case "BF2_DOOR_OPEN":
                case "BF2_ARM_EXTEND_ENABLE":
                case "ARM_NOT_EXTEND_BF1":
                case "ARM_NOT_EXTEND_BF2":
                    DIOUpdate.UpdateInterLock(Parameter, Value);
                    break;
                default:
                    DIOUpdate.UpdateDIOStatus(Parameter, Value);
                    break;
            }
        }



        public void On_TaskJob_Ack(TaskFlowManagement.CurrentProcessTask Task)
        {

        }


        public void On_Message_Log(string Type, string Message)
        {
            if (Type.Equals("EFEM"))
            {
                MonitoringUpdate.LogUpdate(Message);
            }
        }

        public void On_Status_Changed(string Type, string Message)
        {

        }


    }
}
