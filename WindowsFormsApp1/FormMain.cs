using SANWA.Utility;
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
using Lilith.UI_Update.OCR;
using Lilith.UI_Update.WaferMapping;
using System.Threading;
using Lilith.UI_Update.Authority;
using DIOControl;
using Lilith.UI_Update.Layout;
using Lilith.UI_Update.Alarm;
using GUI;
using Lilith.UI_Update.Running;
using System.Linq;
using System.Collections.Concurrent;
using Lilith.Util;
using Lilith.UI_Update.Communications;
using EFEMInterface.MessageInterface;
using EFEMInterface;
using static EFEMInterface.MessageInterface.RorzeInterface;
using System.Security.Cryptography;
using System.Text;
using SANWA.Utility.Config;

namespace Lilith
{
    public partial class FormMain : Form, IUserInterfaceReport, IEFEMControl
    {
        public static RouteControl RouteCtrl;
        public static RorzeInterface HostControl;
        public static AlarmMapping AlmMapping;
        private static readonly ILog logger = LogManager.GetLogger(typeof(FormMain));

        public static bool AutoReverse = true;

        FormAlarm alarmFrom = new FormAlarm();
        private Menu.Monitoring.FormMonitoring formMonitoring = new Menu.Monitoring.FormMonitoring();
        private Menu.Communications.FormCommunications formCommunications = new Menu.Communications.FormCommunications();
        private Menu.WaferMapping.FormWaferMapping formWafer = new Menu.WaferMapping.FormWaferMapping();
        private Menu.Status.FormStatus formStatus = new Menu.Status.FormStatus();
        private Menu.OCR.FormOCR formOCR = new Menu.OCR.FormOCR();
        private Menu.SystemSetting.FormSystemSetting formSystem = new Menu.SystemSetting.FormSystemSetting();
        private Menu.RunningScreen.FormRunningScreen formTestMode = new Menu.RunningScreen.FormRunningScreen();
        private Menu.Wafer.FormWafer WaferForm = new Menu.Wafer.FormWafer();
        private GUI.FormManual formManual = new GUI.FormManual();

        public FormMain()
        {
            InitializeComponent();
            XmlConfigurator.Configure();
            Initialize();

            HostControl = new RorzeInterface(this);
            RouteCtrl = new RouteControl(this, HostControl);
            AlmMapping = new AlarmMapping();

            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Location = new System.Drawing.Point(-200, 0);

            SanwaUtil.addPartition();
            SanwaUtil.dropPartition();
            ThreadPool.QueueUserWorkItem(new WaitCallback(DBUtil.consumeSqlCmd));

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

        private void Initialize()
        {


        }

        private void Form1_Load(object sender, EventArgs e)
        {


            Int32 oldWidth = this.Width;
            Int32 oldHeight = this.Height;

            this.WindowState = FormWindowState.Normal;
            this.Width = 1;
            this.Height = 1;

            Control[] ctrlForm = new Control[] { formMonitoring, formCommunications, formWafer, formStatus, formOCR, formTestMode, WaferForm, formSystem };

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

            RouteCtrl.ConnectAll();
            AuthorityUpdate.UpdateFuncGroupEnable("INIT");//init 權限
            //RouteCtrl.ConnectAll();

            this.Width = oldWidth;
            this.Height = oldHeight;
            this.WindowState = FormWindowState.Maximized;
            HostControl.Events = new ReportEvent();

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
                    AuthorityUpdate.UpdateLogoutInfo();
                    //disable authroity function
                    AuthorityUpdate.UpdateFuncGroupEnable("INIT");
                    ((TabControl)formSystem.Controls["tbcSystemSetting"]).SelectTab(0);
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
                            each.ExcuteScript("AlignerStateGet", "GetStatsBeforeInit", out Message);
                            break;
                        case "ROBOT":
                            each.ErrorMsg = "";
                            each.ExcuteScript("RobotStateGet", "GetStatsBeforeInit", out Message);
                            break;
                    }
                }
            }
        }

        private void ProceedInitial()
        {

            foreach (Node each in NodeManagement.GetList())
            {
                each.InitialComplete = false;
                each.CheckStatus = false;
                string Message = "";
                switch (each.Type.ToUpper())
                {
                    case "ROBOT":
                        each.ExcuteScript("RobotInit", "Initialize", out Message);
                        break;
                        //先做ROBOT
                        //case "ALIGNER":
                        //    each.ExcuteScript("AlignerInit", "Initialize");
                        //    break;
                        //case "LOADPORT":
                        //    each.ExcuteScript("LoadPortInit", "Initialize");
                        //    break;
                }
            }
        }
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            string strMsg = "Move to Home position. OK?";
            if (MessageBox.Show(strMsg, "Org.Back", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                string Message = "";
                Transaction txn = new Transaction();
                txn.Method = Transaction.Command.RobotType.RobotHome;
                NodeManagement.Get("Robot01").SendCommand(txn, out Message);
                txn = new Transaction();
                txn.Method = Transaction.Command.RobotType.RobotHome;
                NodeManagement.Get("Robot02").SendCommand(txn, out Message);
            }
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
            FormTerminal formTerminal = new FormTerminal();
            formTerminal.ShowDialog();
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

        public void On_Command_Excuted(Node Node, Transaction Txn, ReturnMessage Msg)
        {
            logger.Debug("On_Command_Excuted");
            string Message = "";

            Transaction SendTxn = new Transaction();

            if (Txn.Method == Transaction.Command.LoadPortType.Reset)
            {
                AlarmUpdate.UpdateAlarmList(AlarmManagement.GetAll());
            }

            if (Txn.Method.Equals(Transaction.Command.LoadPortType.GetMapping))
            {
                Node Port = null;
                switch (Node.Type)
                {
                    case "LOADPORT":
                        Port = Node;
                        break;
                    case "ROBOT":
                        Port = NodeManagement.Get(Node.CurrentPosition);
                        break;
                }

                if (Port != null)
                {
                    //Asign Wafer

                    foreach (Job j in Port.JobList.Values.ToList())
                    {
                        if (j.MapFlag)
                        {
                            j.AlignerFlag = true;
                            j.OCRFlag = true;
                            j.NeedProcess = true;
                            j.RecipeID = Port.WaferSize;
                            j.AssignPort(Port.Name, j.Slot);
                            j.DefaultOCR = "OCR01";
                        }
                    }
                    Port.Mode = "LU";
                    if (!Port.WaferSize.Equals("200MM"))
                    {
                        Port.LoadTime = DateTime.Now;
                    }
                    Port.Available = true;
                }
            }

            switch (Node.Type)
            {
                case "LOADPORT":
                    switch (Txn.Method)
                    {
                        case Transaction.Command.LoadPortType.GetMapping:

                        case Transaction.Command.LoadPortType.Unload:
                        case Transaction.Command.LoadPortType.MappingUnload:
                        case Transaction.Command.LoadPortType.DoorUp:
                        case Transaction.Command.LoadPortType.InitialPos:
                        case Transaction.Command.LoadPortType.ForceInitialPos:
                            WaferAssignUpdate.RefreshMapping(Node.Name);
                            MonitoringUpdate.UpdateNodesJob(Node.Name);
                            break;
                    }
                    break;
                case "ROBOT":
                    switch (Txn.Method)
                    {
                        case Transaction.Command.RobotType.GetMapping:
                            WaferAssignUpdate.RefreshMapping(Node.CurrentPosition);
                            MonitoringUpdate.UpdateNodesJob(Node.CurrentPosition);
                            break;
                    }
                    break;
            }

            switch (Txn.FormName)
            {
                case "GetStatsBeforeInit":
                    switch (Txn.Method)
                    {
                        //case Transaction.Command.AlignerType.GetStatus:

                        //    break;
                        //case Transaction.Command.RobotType.GetCombineStatus:

                        //    break;
                        //case Transaction.Command.AlignerType.GetSpeed:

                        //    break;
                        case Transaction.Command.AlignerType.GetRIO:
                            if (Msg.Value == null || Msg.Value.IndexOf(",") < 0)
                            {
                                break;
                            }
                            string[] result = Msg.Value.Split(',');
                            switch (result[0])
                            {
                                case "004":

                                    if (result[1].Equals("1"))
                                    {
                                        Node.ErrorMsg += "Present_R 在席存在 ";
                                    }

                                    break;
                                case "005":
                                    if (result[1].Equals("1"))
                                    {
                                        Node.ErrorMsg += "Present_L 在席存在 ";
                                    }
                                    break;
                            }
                            break;
                            //case Transaction.Command.AlignerType.GetError:

                            //    break;
                            //case Transaction.Command.AlignerType.GetMode:

                            //    break;
                            //case Transaction.Command.AlignerType.GetSV:

                            //    break;
                    }
                    break;
                case "FormStatus":
                    Util.StateUtil.UpdateSTS(Node.Name, Msg.Value);
                    break;
                case "PauseProcedure":

                    break;
                case "FormManual":
                    switch (Node.Type)
                    {
                        case "SMARTTAG":
                            if (!Txn.Method.Equals(Transaction.Command.SmartTagType.GetLCDData))
                            {
                                //ManualPortStatusUpdate.LockUI(false);
                            }
                            break;
                        case "LOADPORT":
                            if (!Txn.CommandType.Equals("MOV") && !Txn.CommandType.Equals("HCS"))
                            {
                                //ManualPortStatusUpdate.LockUI(false);
                            }
                            else
                            {
                                if (Txn.Method.Equals(Transaction.Command.LoadPortType.Reset))
                                {
                                   // ManualPortStatusUpdate.LockUI(false);
                                }
                            }
                            ManualPortStatusUpdate.UpdateLog(Node.Name, Msg.Command + " Excuted");
                            switch (Txn.Method)
                            {
                                case Transaction.Command.LoadPortType.ReadVersion:
                                    ManualPortStatusUpdate.UpdateVersion(Node.Name, Msg.Value);
                                    break;
                                case Transaction.Command.LoadPortType.GetLED:
                                    ManualPortStatusUpdate.UpdateLED(Node.Name, Msg.Value);
                                    break;
                                case Transaction.Command.LoadPortType.ReadStatus:
                                    ManualPortStatusUpdate.UpdateSmifStatus(Node.Name, Msg.Value);
                                    break;
                                case Transaction.Command.LoadPortType.GetCount:

                                    break;
                                case Transaction.Command.LoadPortType.GetMapping:
                                    ManualPortStatusUpdate.UpdateMapping(Node.Name, Msg.Value);
                                    break;
                            }
                            break;
                        case "OCR":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.OCRType.GetOnline:
                                    //OCRUpdate.UpdateOCRStatus(Node.Name, Msg.Value);
                                    break;
                            }
                            break;
                        case "ROBOT":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.RobotType.RobotSpeed:
                                case Transaction.Command.RobotType.RobotMode:
                                case Transaction.Command.RobotType.Reset:
                                case Transaction.Command.RobotType.RobotServo:
                                    
                                    ManualRobotStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 手動功能畫面 
                                    break;
                                case Transaction.Command.RobotType.GetSpeed:
                                case Transaction.Command.RobotType.GetRIO:
                                case Transaction.Command.RobotType.GetError:
                                case Transaction.Command.RobotType.GetMode:
                                case Transaction.Command.RobotType.GetStatus:
                                case Transaction.Command.RobotType.GetSV:
                                    ManualRobotStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 手動功能畫面
                                    break;
                                case Transaction.Command.RobotType.GetCombineStatus:
                                    ManualRobotStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Command);//update 手動功能畫面
                                    break;
                            }
                            break;
                        case "ALIGNER":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.AlignerType.AlignerSpeed:
                                case Transaction.Command.AlignerType.AlignerMode:
                                case Transaction.Command.AlignerType.Reset:
                                case Transaction.Command.AlignerType.AlignerServo:
                                    Thread.Sleep(500);
                                    //向Aligner 詢問狀態
                                    Node aligner = NodeManagement.Get(Node.Name);
                                    String script_name = aligner.Brand.ToUpper().Equals("SANWA") ? "AlignerStateGet" : "AlignerStateGet(Kawasaki)";
                                    aligner.ExcuteScript(script_name, "FormManual", out Message);
                                    ManualAlignerStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 
                                    break;
                                case Transaction.Command.AlignerType.GetMode:
                                case Transaction.Command.AlignerType.GetSV:
                                case Transaction.Command.AlignerType.GetStatus:
                                case Transaction.Command.AlignerType.GetSpeed:
                                case Transaction.Command.AlignerType.GetRIO:
                                case Transaction.Command.AlignerType.GetError:
                                    ManualAlignerStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 手動功能畫面
                                    break;
                                case Transaction.Command.RobotType.GetCombineStatus:
                                    ManualAlignerStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Command);//update 手動功能畫面
                                    break;
                            }
                            break;

                    }
                    break;
               
                default:

                    break;
            }
        }

        public void On_Command_Error(Node Node, Transaction Txn, ReturnMessage Msg)
        {
            switch (Txn.FormName)
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
            AlarmInfo CurrentAlarm = new AlarmInfo();
            CurrentAlarm.NodeName = Node.Name;
            CurrentAlarm.AlarmCode = Msg.Value;
            CurrentAlarm.NeedReset = true;
            try
            {

                AlarmMessage Detail = AlmMapping.Get(Node.Name, CurrentAlarm.AlarmCode);

                CurrentAlarm.SystemAlarmCode = Detail.CodeID;
                CurrentAlarm.Desc = Detail.Code_Cause;
                CurrentAlarm.EngDesc = Detail.Code_Cause_English;
                CurrentAlarm.Type = Detail.Code_Type;
                CurrentAlarm.IsStop = Detail.IsStop;
                if (CurrentAlarm.IsStop)
                {
                    RouteCtrl.Stop();
                }
            }
            catch (Exception e)
            {
                CurrentAlarm.Desc = "未定義";
                logger.Error(Node.Controller + "-" + Node.AdrNo + "(GetAlarmMessage)" + e.Message + "\n" + e.StackTrace);
            }
            CurrentAlarm.TimeStamp = DateTime.Now;

            AlarmManagement.Add(CurrentAlarm);

            AlarmUpdate.UpdateAlarmList(AlarmManagement.GetAll());
            AlarmUpdate.UpdateAlarmHistory(AlarmManagement.GetHistory());
    
        }

        public void On_Command_Finished(Node Node, Transaction Txn, ReturnMessage Msg)
        {
            logger.Debug("On_Command_Finished");
            //Transaction txn = new Transaction();
            switch (Txn.FormName)
            {
                case "ChangeAlignWaferSize":
                    switch (Node.Type)
                    {
                        case "ROBOT":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.RobotType.GetWait:
                                    Node.WaitForFinish = false;
                                    break;
                            }
                            break;
                    }
                    break;
                case "FormManual":

                    switch (Node.Type)
                    {
                        case "SMARTTAG":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.SmartTagType.GetLCDData:
                                    ManualPortStatusUpdate.UpdateID(Msg.Value);
                                    break;
                            }
                            //ManualPortStatusUpdate.LockUI(false);
                            break;
                        case "LOADPORT":

                            ManualPortStatusUpdate.UpdateLog(Node.Name, Msg.Command + " Finished");
                            //ManualPortStatusUpdate.LockUI(false);

                            break;

                        case "ROBOT":
                            ManualRobotStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 手動功能畫面
                            break;
                        case "ALIGNER":
                            ManualAlignerStatusUpdate.UpdateGUI(Txn, Node.Name, Msg.Value);//update 手動功能畫面
                            break;
                    }
                    break;
                default:
                    switch (Node.Type)
                    {
                        case "ROBOT":
                        //switch (Txn.Method)
                        //{
                        //    case Transaction.Command.RobotType.Mapping: //when 200mm port mapped by robot's fork, then port's busy switch to false.
                        //        NodeManagement.Get(Txn.Position).Busy = false;
                        //        break;
                        //}
                        //break;
                        case "LOADPORT":
                            switch (Txn.Method)
                            {

                            }
                            break;
                        case "OCR":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.OCRType.Read:
                                    OCRUpdate.UpdateOCRRead(Node.Name, Msg.Value, Txn.TargetJobs[0]);

                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        public void On_Command_TimeOut(Node Node, Transaction Txn)
        {
            logger.Debug("On_Command_TimeOut");
            AlarmInfo CurrentAlarm = new AlarmInfo();
            CurrentAlarm.NodeName = Node.Name;
            CurrentAlarm.AlarmCode = "00200002";
            CurrentAlarm.NeedReset = false;
            try
            {

                AlarmMessage Detail = AlmMapping.Get(Node.Name, CurrentAlarm.AlarmCode);

                CurrentAlarm.SystemAlarmCode = Detail.CodeID;
                CurrentAlarm.Desc = Detail.Code_Cause;
                CurrentAlarm.EngDesc = Detail.Code_Cause_English;
                CurrentAlarm.Type = Detail.Code_Type;
                CurrentAlarm.IsStop = Detail.IsStop;
                if (CurrentAlarm.IsStop)
                {
                    RouteCtrl.Stop();
                }
            }
            catch (Exception e)
            {
                CurrentAlarm.Desc = "未定義";
                logger.Error(Node.Controller + "-" + Node.AdrNo + "(GetAlarmMessage)" + e.Message + "\n" + e.StackTrace);
            }
            CurrentAlarm.TimeStamp = DateTime.Now;
            AlarmManagement.Add(CurrentAlarm);
            AlarmUpdate.UpdateAlarmList(AlarmManagement.GetAll());
            AlarmUpdate.UpdateAlarmHistory(AlarmManagement.GetHistory());
        }

        public void On_Event_Trigger(Node Node, ReturnMessage Msg)
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
                case "ROBOT02":
                    ManualRobotStatusUpdate.UpdateRobotStatus(Node.Name, Status);//update 手動功能畫面
                    break;
                case "ALIGNER01":
                case "ALIGNER02":
                    ManualAlignerStatusUpdate.UpdateAlignerStatus(Node.Name, Status);//update 手動功能畫面
                    break;
            }
        }

        public void On_Eqp_State_Changed(string OldStatus, string NewStatus)
        {
            NodeStatusUpdate.UpdateCurrentState(NewStatus);
            StateRecord.EqpStateUpdate("Sorter", OldStatus, NewStatus);
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

        public void On_Port_Begin(string PortName, string FormName)
        {
            logger.Debug("On_Port_Begin");
            string Message = "";
            Node port = NodeManagement.Get(PortName);
            //for 200mm
            Dictionary<string, string> Params = new Dictionary<string, string>();


            if (port.WaferSize.Equals("200MM"))//8" operation before fetch the first wafer.
            {
                Params.Add("12_Inch_OCR", "False");
                Params.Add("8_Inch_OCR", "True");
                RouteControl.Instance.DIO.SetIO(Params);

                Dictionary<string, string> vars = new Dictionary<string, string>();
                RobotPoint robotPoint = PointManagement.GetMapPoint(port.Name, port.WaferSize);
                Node Robot = NodeManagement.Get(robotPoint.NodeName);
                if (!port.IsMapping)
                {//除了第一次用手動Mapping，之後的自動循環都要做Mapping
                    port.WaitForFinish = true;
                    vars.Add("@loadport", PortName);
                    Robot.ExcuteScript("RobotMapping", "MANSW", vars, out Message, "200MM");


                }
                Node Aligner = NodeManagement.Get(Robot.DefaultAligner);
                if (Aligner != null)//切換Aligner位置
                {
                    Aligner.WaitForFinish = true;
                    vars = new Dictionary<string, string>();
                    vars.Add("@size", "100000");
                    Aligner.ExcuteScript("ChangeAlignWaferSize", "ChangeAlignWaferSize", vars, out Message, "200MM");
                    logger.Debug("Wait for ChangeAlignSize.");
                    SpinWait.SpinUntil(() => !Aligner.WaitForFinish, 99999999);

                    if (!port.IsMapping)
                    {
                        logger.Debug("Wait for RobotMapping.");
                        SpinWait.SpinUntil(() => !port.WaitForFinish, 99999999);//等待Robot Mapping動作結束
                    }
                }
                else
                {
                    logger.Error("ChangeAlignSize error: Aligner not found.");
                    RouteCtrl.Stop();
                    return;
                }


            }
            else if (port.WaferSize.Equals("300MM"))//12" operation before fetch the first wafer.
            {

                Params.Add("8_Inch_OCR", "False");
                Params.Add("12_Inch_OCR", "True");

                RouteControl.Instance.DIO.SetIO(Params);

                RobotPoint robotPoint = PointManagement.GetMapPoint(port.Name, port.WaferSize);

                Node Robot = NodeManagement.Get(robotPoint.NodeName);
                Node Aligner = NodeManagement.Get(Robot.DefaultAligner);
                if (Aligner != null)
                {
                    Robot.WaitForFinish = true;
                    Transaction GetWaitCmd = new Transaction();
                    GetWaitCmd.Method = Transaction.Command.RobotType.GetWait;
                    GetWaitCmd.Slot = "1";
                    GetWaitCmd.Arm = "2";
                    GetWaitCmd.Position = port.Name;
                    GetWaitCmd.FormName = "ChangeAlignWaferSize";
                    Robot.SendCommand(GetWaitCmd, out Message);

                    Aligner.WaitForFinish = true;
                    Dictionary<string, string> vars = new Dictionary<string, string>();
                    vars.Add("@size", "150000");
                    Aligner.ExcuteScript("ChangeAlignWaferSize", "ChangeAlignWaferSize", vars, out Message, "300MM");
                    logger.Debug("Wait for ChangeAlignSize.");
                    SpinWait.SpinUntil(() => !Aligner.WaitForFinish, 99999999);


                    logger.Debug("Wait for Robot GetWait.");
                    SpinWait.SpinUntil(() => !Robot.WaitForFinish, 99999999);//等待Robot GetWait動作結束
                }
                else
                {
                    logger.Error("ChangeAlignSize error: Aligner not found.");
                    RouteCtrl.Stop();
                    return;
                }

            }

            WaferAssignUpdate.RefreshMapping(PortName);
            WaferAssignUpdate.RefreshMapping(NodeManagement.Get(PortName).DestPort);
            try
            {
                switch (FormName)
                {
                    case "Running":
                        MonitoringUpdate.UpdateUseState(PortName, true);
                        WaferAssignUpdate.UpdateUseState(PortName, true);
                        break;

                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        public void On_Port_Finished(string PortName, string FormName)
        {
            logger.Debug("On_Port_Finished");
            try
            {
                string Message = "";
                WaferAssignUpdate.RefreshMapping(PortName);
                WaferAssignUpdate.RefreshMapping(NodeManagement.Get(PortName).DestPort);
                Node Port = NodeManagement.Get(PortName);
                Node DestPort = NodeManagement.Get(Port.DestPort);
                switch (FormName)
                {
                    case "Running":
                        //RunningUpdate.UpdateUseState(PortName, false);
                        MonitoringUpdate.UpdateUseState(PortName, false);
                        WaferAssignUpdate.UpdateUseState(PortName, false);
                        if (FormMain.AutoReverse)
                        {
                            if (!Port.ByPass)
                            {
                                if (Port.WaferSize.Equals("200MM"))
                                {
                                    Port.IsMapping = false;//讓每次循環開始時，都做一次Mapping
                                    Port.LoadTime = DateTime.Now;
                                    Port.Available = true;
                                }
                                else
                                {
                                    //foreach (Node eachPort in NodeManagement.GetLoadPortList())
                                    //{//當300MM做完才把200MM啟用，這樣不會都一直取200MM的Port
                                    //    if (eachPort.WaferSize.Equals("200MM"))
                                    //    {
                                    //        eachPort.Available = true;
                                    //    }
                                    //}
                                    Port.ExcuteScript("LoadPortUnloadAndLoad", "Running_Port_Finished", out Message);

                                }
                            }
                            else
                            {

                                //RunningUpdate.ReverseRunning(Port.Name);
                            }
                        }
                        else
                        {
                            RouteCtrl.Stop();
                            foreach (Node port in NodeManagement.GetLoadPortList())
                            {
                                if (Port.WaferSize.Equals("300MM"))
                                {
                                    Port.ExcuteScript("LoadPortUnload", "Port_Finished", out Message);
                                }
                            }
                            MessageBox.Show("自動展示模式停止");
                        }
                        //RunningUpdate.ReverseRunning(PortName);

                        break;
                    default:
                        Port.ExcuteScript("LoadPortUnload", "Port_Finished", out Message);
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        public void On_Task_Finished(string FormName, string LapsedTime, int LapsedWfCount, int LapsedLotCount)
        {
            logger.Debug("On_Task_Finished");
            //NodeStatusUpdate.UpdateCurrentState("Idle");
            try
            {
                RunningUpdate.UpdateRunningInfo("LapsedTime", LapsedTime);
                RunningUpdate.UpdateRunningInfo("TransCount", "+1");
                RunningUpdate.UpdateRunningInfo("LapsedWfCount", LapsedWfCount.ToString());
                RunningUpdate.UpdateRunningInfo("LapsedLotCount", LapsedLotCount.ToString());
                RunningUpdate.UpdateRunningInfo("WPH", (LapsedWfCount / Convert.ToDouble(LapsedTime) * 3600).ToString());
                MonitoringUpdate.UpdateWPH(Math.Round((LapsedWfCount / Convert.ToDouble(LapsedTime) * 3600), 1).ToString());
                foreach (Node port in NodeManagement.GetLoadPortList())
                {
                    WaferAssignUpdate.ResetAssignCM(port.Name, true);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }

        }

        public void On_Mode_Changed(string Mode)
        {
            logger.Debug("On_Mode_Changed");

            ConnectionStatusUpdate.UpdateModeStatus(Mode);
            RunningUpdate.UpdateModeStatus(Mode);
            MonitoringUpdate.UpdateStatus(Mode);
            foreach (Node port in NodeManagement.GetLoadPortList())
            {
                WaferAssignUpdate.RefreshMapping(port.Name);
                if (Mode.Equals("Stop"))
                {
                    WaferAssignUpdate.ResetAssignCM(port.Name, true);
                }
            }

        }

        public void On_Job_Location_Changed(Job Job)
        {
            logger.Debug("On_Job_Location_Changed");
            MonitoringUpdate.UpdateJobMove(Job.Job_Id);


        }

        public void On_Script_Finished(Node Node, string ScriptName, string FormName)
        {
            logger.Debug("On_Script_Finished: " + Node.Name + " Script:" + ScriptName + " Finished, Form name:" + FormName);
            string Message = "";
            Transaction txn;
            switch (FormName)
            {
                case "GetStatsBeforeInit":
                    Node.CheckStatus = true;


                    //檢查在席是否全部回報完成
                    var find = from nd in NodeManagement.GetList()
                               where (nd.Type.Equals("ALIGNER") || nd.Type.Equals("ROBOT")) && !nd.CheckStatus
                               select nd;
                    if (find.Count() == 0)
                    {//在席回報完成

                        find = from nd in NodeManagement.GetList()
                               where (nd.Type.Equals("ALIGNER") || nd.Type.Equals("ROBOT")) && !nd.ErrorMsg.Equals("")
                               select nd;
                        string tmp = "";
                        foreach (Node each in find)
                        {
                            tmp += " " + each.Name + ":" + each.ErrorMsg + "\n";
                        }
                        if (tmp != "")
                        {
                            MessageBox.Show("偵測到在席，請先確認再執行Initial.\n\n" + tmp);
                            MonitoringUpdate.UpdateInitialButton(true);
                        }
                        else
                        {//都沒有在席，自動執行Initial
                            ProceedInitial();
                        }
                    }

                    break;

                case "MANSW":
                    switch (ScriptName)
                    {
                        case "LoadPortMapping":
                            Node.WaitForFinish = false;
                            //檢查是否還在Mapping
                            var findPort = from node in NodeManagement.GetLoadPortList()
                                           where node.WaitForFinish
                                           select node;
                            if (findPort.Count() == 0)
                            {//沒有的話就啟用Start按鈕
                                //檢查Mapping資料是否有異常
                                findPort = from node in NodeManagement.GetLoadPortList()
                                           from j in node.JobList.Values.ToList()
                                           where j.Job_Id.Equals("Crossed") || j.Job_Id.Equals("Undefined") || j.Job_Id.Equals("Double")
                                           select node;
                                if (findPort.Count() == 0)
                                {
                                    MonitoringUpdate.UpdateStartButton(true);
                                }
                                else
                                {
                                    MessageBox.Show("Mapping異常，請檢查Wafer狀態");
                                }
                            }
                            break;
                        case "RobotMapping":
                            txn = new Transaction();
                            txn.Method = Transaction.Command.LoadPortType.SetOpAccess;
                            txn.Value = "0";
                            txn.FormName = "RobotMapping";
                            Node port = NodeManagement.Get(Node.CurrentPosition);
                            if (port != null)
                            {
                                port.SendCommand(txn, out Message);

                                port.WaitForFinish = false;
                            }
                            //檢查是否還在Mapping
                            findPort = from node in NodeManagement.GetLoadPortList()
                                       where node.WaitForFinish
                                       select node;
                            if (findPort.Count() == 0)
                            {//沒有的話就啟用Start按鈕
                                //檢查Mapping資料是否有異常
                                findPort = from node in NodeManagement.GetLoadPortList()
                                           from j in node.JobList.Values.ToList()
                                           where j.Job_Id.Equals("Crossed") || j.Job_Id.Equals("Undefined") || j.Job_Id.Equals("Double")
                                           select node;
                                if (findPort.Count() == 0)
                                {
                                    MonitoringUpdate.UpdateStartButton(true);
                                }
                                else
                                {
                                    MessageBox.Show("Mapping異常，請檢查Wafer狀態");
                                }
                            }
                            break;
                    }
                    break;
                case "ChangeAlignWaferSize":
                    Node.WaitForFinish = false;

                    break;
                case "Initialize":

                    Node.InitialObject();
                    Node.InitialComplete = true;
                    switch (Node.Type)
                    {
                        case "ROBOT":
                        case "ALIGNER":
                            Node.State = "Idle";
                            break;
                        case "LOADPORT":
                            Node.State = "Ready To Load";
                            break;
                    }

                    if (Node.Type.Equals("ROBOT"))//當最後一台Robot 完成Initial時，其他才開始做Initial
                    {
                        if (NodeManagement.IsRobotInitial())
                        {
                            foreach (Node each in NodeManagement.GetList())
                            {

                                switch (each.Type.ToUpper())
                                {
                                    case "ALIGNER":
                                        each.ExcuteScript("AlignerInit", "Initialize", out Message);
                                        break;
                                    case "LOADPORT":
                                        if (each.WaferSize.Equals("200MM"))
                                        {
                                            each.ExcuteScript("LoadPortInit200MM", "Initialize", out Message);
                                        }
                                        else
                                        {
                                            each.ExcuteScript("LoadPortInit", "Initialize", out Message);
                                        }
                                        break;
                                }
                            }
                        }
                    }

                    if (!NodeManagement.IsNeedInitial())
                    {
                        NodeStatusUpdate.UpdateCurrentState("Idle");
                        ConnectionStatusUpdate.UpdateInitial(true.ToString());
                        MonitoringUpdate.UpdateInitialButton(true);
                        foreach (Node EachPort in NodeManagement.GetLoadPortList())
                        {
                            if (!EachPort.ByPass)
                            {

                                txn = new Transaction();
                                txn.Method = Transaction.Command.LoadPortType.ReadStatus;
                                txn.FormName = "InitialFinish";
                                EachPort.SendCommand(txn, out Message);

                            }
                            EachPort.JobList.Clear();
                            MonitoringUpdate.UpdateNodesJob(EachPort.Name);
                            WaferAssignUpdate.RefreshMapping(EachPort.Name);
                        }
                    }
                    break;
                case "FormManual-Script":

                    switch (Node.Type)
                    {
                        case "ROBOT":
                            //ManualRobotStatusUpdate.UpdateGUI(new Transaction(), Node.Name, "");//update 手動功能畫面
                            break;
                    }
                    break;

                case "Running_Port_Finished":
                    if (ScriptName.Equals("LoadPortUnloadAndLoad"))
                    {
                        //Node Port;
                        //Node DestPort;

                        //Node.PortUnloadAndLoadFinished = true;
                        //if (!Node.DestPort.Equals(""))
                        //{
                        //    Port = Node;
                        //    DestPort = NodeManagement.Get(Node.DestPort);
                        //    // SpinWait.SpinUntil(() => (Port.IsMapping && Port.JobList.Count!=0 && DestPort.IsMapping && DestPort.JobList.Count != 0) || RouteCtrl.GetMode().Equals("Stop") , 99999999);

                        //    SpinWait.SpinUntil(() => (Port.PortUnloadAndLoadFinished && DestPort.PortUnloadAndLoadFinished) || RouteCtrl.GetMode().Equals("Stop"), 99999999);

                        //    if (!RouteCtrl.GetMode().Equals("Stop") && Port.PortUnloadAndLoadFinished && DestPort.PortUnloadAndLoadFinished)
                        //    {
                        //        Port.PortUnloadAndLoadFinished = false;
                        //        DestPort.PortUnloadAndLoadFinished = false;
                        //        RunningUpdate.ReverseRunning(Port.Name);
                        //    }
                        //}
                        //else
                        //{
                        //    var findPort = from port in NodeManagement.GetLoadPortList()
                        //                   where port.DestPort.Equals(Node.Name)
                        //                   select port;

                        //    if (findPort.Count() != 0)
                        //    {
                        //        Port = findPort.First();
                        //        DestPort = Node;
                        //        //SpinWait.SpinUntil(() => (Port.IsMapping && Port.JobList.Count != 0 && DestPort.IsMapping && DestPort.JobList.Count != 0) || RouteCtrl.GetMode().Equals("Stop"), 99999999);

                        //        //SpinWait.SpinUntil(() => (Port.IsMapping && DestPort.IsMapping) || RouteCtrl.GetMode().Equals("Stop"), 99999999);
                        //        if (!RouteCtrl.GetMode().Equals("Stop") && Port.PortUnloadAndLoadFinished && DestPort.PortUnloadAndLoadFinished)
                        //        {

                        //            RunningUpdate.ReverseRunning(Port.Name);

                        //        }

                        //    }
                        //}

                    }

                    break;
            }
        }

        //private void vSBRobotStatus_Scroll(object sender, ScrollEventArgs e)
        //{
        //    //pbRobotState.Top = -vSBRobotStatus.Value;
        //}

        //private void vSBAlignerStatus_Scroll(object sender, ScrollEventArgs e)
        //{
        //    //pbAlignerState.Top = -vSBAlignerStatus.Value;
        //}

        //private void vSBPortStatus_Scroll(object sender, ScrollEventArgs e)
        //{
        //    //pbPortState.Top = -vSBPortStatus.Value;
        //}






        public void On_Connection_Status_Report(string DIOName, string Status)
        {
            ConnectionStatusUpdate.UpdateControllerStatus(DIOName, Status);
        }


        public void On_Data_Chnaged(string Parameter, string Value)
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

        public void On_Alarm_Happen(string DIOName, string ErrorCode)
        {
            
            AlarmInfo CurrentAlarm = new AlarmInfo();
            CurrentAlarm.NodeName = DIOName;
            CurrentAlarm.AlarmCode = ErrorCode;
            CurrentAlarm.NeedReset = false;
            try
            {

                AlarmMessage Detail = AlmMapping.Get("DIO", CurrentAlarm.AlarmCode);

                CurrentAlarm.SystemAlarmCode = Detail.CodeID;
                CurrentAlarm.Desc = Detail.Code_Cause;
                CurrentAlarm.EngDesc = Detail.Code_Cause_English;
                CurrentAlarm.Type = Detail.Code_Type;
                CurrentAlarm.IsStop = Detail.IsStop;

                
                if (CurrentAlarm.IsStop)
                {
                    RouteCtrl.Stop();
                }
            }
            catch (Exception e)
            {
                CurrentAlarm.Desc = "未定義";
                logger.Error(DIOName + "(GetAlarmMessage)" + e.Message + "\n" + e.StackTrace);
            }
            CurrentAlarm.TimeStamp = DateTime.Now;
            AlarmManagement.Add(CurrentAlarm);
            AlarmUpdate.UpdateAlarmList(AlarmManagement.GetAll());
            AlarmUpdate.UpdateAlarmHistory(AlarmManagement.GetHistory());
        }

        public void On_Connection_Error(string DIOName, string ErrorMsg)
        {
            //斷線 發ALARM
            logger.Debug("On_Error_Occurred");
            AlarmInfo CurrentAlarm = new AlarmInfo();
            CurrentAlarm.NodeName = DIOName;
            CurrentAlarm.AlarmCode = "00200001";
            CurrentAlarm.NeedReset = false;
            try
            {

                AlarmMessage Detail = AlmMapping.Get("DIO", CurrentAlarm.AlarmCode);

                CurrentAlarm.SystemAlarmCode = Detail.CodeID;
                CurrentAlarm.Desc = Detail.Code_Cause;
                CurrentAlarm.EngDesc = Detail.Code_Cause_English;
                CurrentAlarm.Type = Detail.Code_Type;
                CurrentAlarm.IsStop = Detail.IsStop;
                if (CurrentAlarm.IsStop)
                {
                    RouteCtrl.Stop();
                }
            }
            catch (Exception e)
            {
                CurrentAlarm.Desc = "未定義";
                logger.Error(DIOName + "(GetAlarmMessage)" + e.Message + "\n" + e.StackTrace);
            }
            CurrentAlarm.TimeStamp = DateTime.Now;
            AlarmManagement.Add(CurrentAlarm);
            AlarmUpdate.UpdateAlarmList(AlarmManagement.GetAll());
            AlarmUpdate.UpdateAlarmHistory(AlarmManagement.GetHistory());
        }





        private void Signal_MouseClick(object sender, MouseEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "RED_Signal":
                    if (RouteControl.Instance.DIO.GetIO("OUT", "RED").ToUpper().Equals("TRUE"))
                    {
                        RouteControl.Instance.DIO.SetIO("RED", "False");
                    }
                    else
                    {
                        RouteControl.Instance.DIO.SetIO("RED", "True");
                    }
                    break;
                case "ORANGE_Signal":
                    if (RouteControl.Instance.DIO.GetIO("OUT", "ORANGE").ToUpper().Equals("TRUE"))
                    {
                        RouteControl.Instance.DIO.SetIO("ORANGE", "False");
                    }
                    else
                    {
                        RouteControl.Instance.DIO.SetIO("ORANGE", "True");
                    }
                    break;
                case "GREEN_Signal":
                    if (RouteControl.Instance.DIO.GetIO("OUT", "GREEN").ToUpper().Equals("TRUE"))
                    {
                        RouteControl.Instance.DIO.SetIO("GREEN", "False");
                    }
                    else
                    {
                        RouteControl.Instance.DIO.SetIO("GREEN", "True");
                    }
                    break;
                case "BLUE_Signal":
                    if (RouteControl.Instance.DIO.GetIO("OUT", "BLUE").ToUpper().Equals("TRUE"))
                    {
                        RouteControl.Instance.DIO.SetIO("BLUE", "False");
                    }
                    else
                    {
                        RouteControl.Instance.DIO.SetIO("BLUE", "True");
                    }
                    break;
                case "BUZZER1_Signal":
                    if (RouteControl.Instance.DIO.GetIO("OUT", "BUZZER1").ToUpper().Equals("TRUE"))
                    {
                        RouteControl.Instance.DIO.SetIO("BUZZER1", "False");
                    }
                    else
                    {
                        RouteControl.Instance.DIO.SetIO("BUZZER1", "True");
                    }
                    break;
                case "BUZZER2_Signal":
                    if (RouteControl.Instance.DIO.GetIO("OUT", "BUZZER2").ToUpper().Equals("TRUE"))
                    {
                        RouteControl.Instance.DIO.SetIO("BUZZER2", "False");
                    }
                    else
                    {
                        RouteControl.Instance.DIO.SetIO("BUZZER2", "True");
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
                formManual.Close();
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
            TextBox tbUser = new TextBox() { Left = 30, Top = 50, Width = 350 , Text = "Administrator"};
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
            if (tbcMian.SelectedTab.Text.Equals("Status"))
            {
                formStatus.Focus();
            }
        }

        private void menuMaintenace_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ToolStripMenuItem item in menuMaintenace.Items)
            {
                string user_group = lbl_login_group.Text;
                string fun_form = "FormMain";
                string fun_ref = item.Name;
                Boolean enable = AuthorityUpdate.getFuncEnable(user_group, fun_form, fun_ref);
                item.Enabled = enable;
            }
        }

        private void Conn_gv_RowEnter(object sender, DataGridViewCellEventArgs e)
        {

        }

        public void On_CommandMessage(string msg)
        {
            MonitoringUpdate.LogUpdate(msg);
        }

        public void On_Connection_Connected()
        {
            //MonitoringUpdate.ConnectUpdate("Connected");
            ConnectionStatusUpdate.UpdateOnlineStatus("Online");
            MonitoringUpdate.LogUpdate("Connected");
        }

        public void On_Connection_Connecting()
        {
            //MonitoringUpdate.ConnectUpdate("Connecting");
            ConnectionStatusUpdate.UpdateOnlineStatus("Connecting");
            MonitoringUpdate.LogUpdate("Connecting");
        }

        public void On_Connection_Disconnected()
        {
            //MonitoringUpdate.ConnectUpdate("Disconnected");
            ConnectionStatusUpdate.UpdateOnlineStatus("Offline");
            MonitoringUpdate.LogUpdate("Disconnected");
        }

        public void On_TaskJob_Aborted(string TaskID, string NodeName, string ReportType, string Message)
        {
            if (TaskID.Equals("FormManual"))
            {
                ManualPortStatusUpdate.LockUI(false);
            }
            AlarmInfo CurrentAlarm = new AlarmInfo();
            CurrentAlarm.NodeName = "SYSTEM";
            CurrentAlarm.AlarmCode = Message;
            CurrentAlarm.NeedReset = false;
            try
            {

                AlarmMessage Detail = AlmMapping.Get("SYSTEM", CurrentAlarm.AlarmCode);
                if (!Detail.Code_Group.Equals("UNDEFINITION"))
                {
                    CurrentAlarm.SystemAlarmCode = Detail.CodeID;
                    CurrentAlarm.Desc = Detail.Code_Cause;
                    CurrentAlarm.EngDesc = Detail.Code_Cause_English;
                    CurrentAlarm.Type = Detail.Code_Type;
                    CurrentAlarm.IsStop = Detail.IsStop;
                    if (CurrentAlarm.IsStop)
                    {
                        RouteCtrl.Stop();
                    }
                    CurrentAlarm.TimeStamp = DateTime.Now;

                    AlarmManagement.Add(CurrentAlarm);

                    AlarmUpdate.UpdateAlarmList(AlarmManagement.GetAll());
                    AlarmUpdate.UpdateAlarmHistory(AlarmManagement.GetHistory());
                }
            }
            catch (Exception e)
            {
                CurrentAlarm.Desc = "未定義";
                logger.Error("(GetAlarmMessage)" + e.Message + "\n" + e.StackTrace);
            }
           

        }

        public void On_TaskJob_Finished(string TaskID)
        {
            if (TaskID.Equals("FormManual"))
            {
                ManualPortStatusUpdate.LockUI(false);
            }
        }

        private void btnManual_Click(object sender, EventArgs e)
        {
            formManual.Close();
            formManual = new GUI.FormManual();
            formManual.Show();
        }
    }
}
