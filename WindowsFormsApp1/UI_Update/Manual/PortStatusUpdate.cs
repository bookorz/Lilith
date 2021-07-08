using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransferControl.Management;
using TransferControl.Parser;

namespace Lilith.UI_Update.Manual
{
    class ManualPortStatusUpdate
    {
        static ILog logger = LogManager.GetLogger(typeof(ManualPortStatusUpdate));
        delegate void UpdateData(string NodeNAme, string Data);
        delegate void UpdateLock(bool Enable);
        delegate void UpdateId(string Data);



        public static void LockUI(bool Enable)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                TabControl Tab;
                if (form == null)
                    return;

                Tab = form.Controls.Find("tbcManual", true).FirstOrDefault() as TabControl;
                if (Tab == null)
                    return;

                if (Tab.InvokeRequired)
                {
                    UpdateLock ph = new UpdateLock(LockUI);
                    Tab.BeginInvoke(ph, Enable);
                }
                else
                {
                    Tab.Enabled = !Enable;
                    if (!Enable)
                    {
                        form.Cursor = Cursors.Default;
                    }
                    else
                    {
                        form.Cursor = Cursors.WaitCursor;
                    }
                }


            }
            catch
            {
                logger.Error("LockUI: Update fail.");
            }
        }

        public static void UpdateLog(string NodeName, string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                ComboBox portName;
                if (form == null)
                    return;

                portName = form.Controls.Find("Cb_LoadPortSelect", true).FirstOrDefault() as ComboBox;
                if (portName == null)
                    return;

                if (portName.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateLog);
                    portName.BeginInvoke(ph, NodeName, Data);
                }
                else
                {
                    if (portName.Text.Equals(NodeName))
                    {
                        RichTextBox LOG = form.Controls.Find("RTxt_Message_A", true).FirstOrDefault() as RichTextBox;
                        if (LOG == null)
                            return;
                        LOG.AppendText(Data + "\n");
                        LOG.ScrollToCaret();
                    }

                }


            }
            catch
            {
                logger.Error("UpdateControllerStatus: Update fail.");
            }
        }

        public static void UpdateSmifLog(string NodeName, string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                ComboBox portName;
                if (form == null)
                    return;

                portName = form.Controls.Find("Cb_SMIFSelect", true).FirstOrDefault() as ComboBox;
                if (portName == null)
                    return;

                if (portName.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateLog);
                    portName.BeginInvoke(ph, NodeName, Data);
                }
                else
                {
                    if (portName.Text.Equals(NodeName))
                    {
                        RichTextBox LOG = form.Controls.Find("Smif_log_rt", true).FirstOrDefault() as RichTextBox;
                        if (LOG == null)
                            return;
                        LOG.AppendText(Data + "\n");
                        LOG.ScrollToCaret();
                    }

                }


            }
            catch
            {
                logger.Error("UpdateControllerStatus: Update fail.");
            }
        }

        public static void UpdateVersion(string NodeName, string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                ComboBox portName;
                if (form == null)
                    return;

                portName = form.Controls.Find("Cb_LoadPortSelect", true).FirstOrDefault() as ComboBox;
                if (portName == null)
                    return;

                if (portName.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateVersion);
                    portName.BeginInvoke(ph, NodeName, Data);
                }
                else
                {
                    if (portName.Text.Equals(NodeName))
                    {
                        Label VER = form.Controls.Find("LblVersion_A", true).FirstOrDefault() as Label;
                        if (VER == null)
                            return;
                        VER.Text = Data;
                    }

                }


            }
            catch
            {
                logger.Error("UpdateControllerStatus: Update fail.");
            }
        }

        public static void UpdateID(string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                TextBox id;
                if (form == null)
                    return;

                id = form.Controls.Find("SmartTagRead_tb", true).FirstOrDefault() as TextBox;
                if (id == null)
                    return;

                if (id.InvokeRequired)
                {
                    UpdateId ph = new UpdateId(UpdateID);
                    id.BeginInvoke(ph, Data);
                }
                else
                {
                    id.Text = Data;

                }


            }
            catch
            {
                logger.Error("UpdateControllerStatus: Update fail.");
            }
        }

        public static void UpdateParameter(string Name,string Value)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                TextBox id;
                if (form == null)
                    return;

                id = form.Controls.Find(Name, true).FirstOrDefault() as TextBox;
                if (id == null)
                    return;

                if (id.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateParameter);
                    id.BeginInvoke(ph, Name, Value);
                }
                else
                {
                    id.Text = Value;

                }


            }
            catch
            {
                logger.Error("UpdateControllerStatus: Update fail.");
            }
        }

        public static void UpdateE84Status(string NodeName, string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                ComboBox portName;
                if (form == null)
                    return;

                portName = form.Controls.Find("cmbE84Select", true).FirstOrDefault() as ComboBox;
                if (portName == null)
                    return;

                if (portName.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateE84Status);
                    portName.BeginInvoke(ph, NodeName, Data);
                }
                else
                {
                    Node node = NodeManagement.Get(NodeName);
                    Label STS = null;
                    if (portName.Text.Equals(NodeName))
                    {
                        if(node.E84Mode == E84_Mode.AUTO)
                        {
                            STS = form.Controls.Find("lbE84AutoMode", true).FirstOrDefault() as Label;
                            STS.BackColor = Color.Lime;

                            STS = form.Controls.Find("lbE84ManualMode", true).FirstOrDefault() as Label;
                            STS.BackColor = Color.White;
                        }

                        if (node.E84Mode == E84_Mode.MANUAL)
                        {
                            STS = form.Controls.Find("lbE84AutoMode", true).FirstOrDefault() as Label;
                            STS.BackColor = Color.White;

                            STS = form.Controls.Find("lbE84ManualMode", true).FirstOrDefault() as Label;
                            STS.BackColor = Color.Lime;
                        }

                        STS = form.Controls.Find("lbE84_L_REQ", true).FirstOrDefault() as Label;
                        if(STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["L_REQ"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_U_REQ", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["U_REQ"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_READY", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["READY"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_HO_AVBL", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["HO_AVBL"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_ES", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["ES"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_VALID", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["VALID"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_CS_0", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["CS_0"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_CS_1", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["CS_1"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_TR_REQ", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["TR_REQ"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_BUSY", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["BUSY"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_COMPT", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["COMPT"] ? Color.Lime : Color.White;

                        STS = form.Controls.Find("lbE84_CONT", true).FirstOrDefault() as Label;
                        if (STS.BackColor != null)
                            STS.BackColor = node.E84IOStatus["CONT"] ? Color.Lime : Color.White;
                    }
                }
            }
            catch
            {
                logger.Error("UpdateE84Status: Update fail.");
            }
        }

        public static void UpdateLoadPortStatus(string NodeName, string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                ComboBox portName;
                if (form == null)
                    return;

                portName = form.Controls.Find("cmbLoadportSelect", true).FirstOrDefault() as ComboBox;
                if (portName == null)
                    return;

                if (portName.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateLoadPortStatus);
                    portName.BeginInvoke(ph, NodeName, Data);
                }
                else
                {
                    Node node = NodeManagement.Get(NodeName);
                    Label STS = null;
                    if (portName.Text.Equals(NodeName))
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            string Idx = (i + 1).ToString("00");
                            switch (Idx)
                            {
                                case "01":
                                    STS = form.Controls.Find("tbLPEquipmentStatus", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    { 
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                    STS.Text = "Normal";
                                                break;
                                            case 'A':
                                                    STS.Text = "Recoverable error";
                                                break;
                                            case 'E':
                                                    STS.Text = "Fatal error";
                                                break;
                                        }
                                    }
                                    break;
                                case "02":
                                    STS = form.Controls.Find("tbLPMode", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Online";
                                                break;
                                            case '1':
                                                STS.Text = "Teaching";
                                                break;
                                        }
                                    }
                                    break;
                                case "03":
                                    STS = form.Controls.Find("tbLPInitalPosition", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Unexecuted";
                                                break;
                                            case '1':
                                                STS.Text = "Executed";
                                                break;
                                        }
                                    }
                                    break;

                                case "04":
                                    STS = form.Controls.Find("tbLPOperationStatus", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Stopped";
                                                break;
                                            case '1':
                                                STS.Text = "Operating";
                                                break;
                                        }
                                    }

                                    break;
                                case "06":
                                    STS = form.Controls.Find("tbLPErrorCode", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                        STS.Text = node.StatusRawData[4].ToString() + node.StatusRawData[5].ToString();

                                    break;
                                case "07":
                                    STS = form.Controls.Find("tbLPCassettepresence", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "None";
                                                break;
                                            case '1':
                                                STS.Text = "Normal position";
                                                break;
                                            case '2':
                                                STS.Text = "Error load";
                                                break;
                                        }
                                    }
                                    break;
                                case "08":
                                    STS = form.Controls.Find("tbFoupClampStatus", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Open";
                                                break;
                                            case '1':
                                                STS.Text = "Close";
                                                break;
                                            case '?':
                                                STS.Text = "Not defined";
                                                break;
                                        }
                                    }

                                    break;
                                case "09":
                                    STS = form.Controls.Find("tbLatchKeyStatus", true).FirstOrDefault() as Label;
                                    if(STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Open";
                                                break;
                                            case '1':
                                                STS.Text = "Close";
                                                break;
                                            case '?':
                                                STS.Text = "Not defined";
                                                break;
                                        }
                                    }

                                    break;
                                case "10":
                                    STS = form.Controls.Find("tbLPVacuum", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "OFF";
                                                break;
                                            case '1':
                                                STS.Text = "ON";
                                                break;
                                        }
                                    }

                                    break;
                                case "11":
                                    STS = form.Controls.Find("tbLPDoorPosition", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Open position";
                                                break;
                                            case '1':
                                                STS.Text = "Close position";
                                                break;
                                            case '?':
                                                STS.Text = "Not defined";
                                                break;
                                        }
                                    }

                                    break;
                                case "12":
                                    STS = form.Controls.Find("tbWaferProtusionSensor", true).FirstOrDefault() as Label;
                                    if(STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Blocked";
                                                break;
                                            case '1':
                                                STS.Text = "Unblocked";
                                                break;
                                        }
                                    }
                                    break;
                                case "13":
                                    STS = form.Controls.Find("tbLPZAxisPosition", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Up position";
                                                break;
                                            case '1':
                                                STS.Text = "Down position";
                                                break;
                                            case '2':
                                                STS.Text = "Start position";
                                                break;
                                            case '3':
                                                STS.Text = "End position";
                                                break;
                                            case '?':
                                                STS.Text = "Not defined";
                                                break;
                                        }
                                    }
                                    break;
                                case "14":
                                    STS = form.Controls.Find("tbLPYAxisPosition", true).FirstOrDefault() as Label;
                                    if(STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Undock position";
                                                break;
                                            case '1':
                                                STS.Text = "Dock position";
                                                break;
                                            case '?':
                                                STS.Text = "Not defined";
                                                break;
                                        }
                                    }
                                    break;
                                case "15":
                                    STS = form.Controls.Find("tbLPMapperArmPosition", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Open";
                                                break;
                                            case '1':
                                                STS.Text = "Close";
                                                break;
                                            case '?':
                                                STS.Text = "Not defined";
                                                break;
                                        }
                                    }
                                    break;
                                case "16":
                                    STS = form.Controls.Find("tbLPMapperZAixs", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Retract position";
                                                break;
                                            case '1':
                                                STS.Text = "Mapping position";
                                                break;
                                            case '?':
                                                STS.Text = "Not defined";
                                                break;
                                        }
                                    }
                                    break;
                                case "17":
                                    STS = form.Controls.Find("tbLPMapperStopper", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "ON";
                                                break;
                                            case '1':
                                                STS.Text = "OFF";
                                                break;
                                            case '?':
                                                STS.Text = "Not defined";
                                                break;
                                        }
                                    }
                                    break;
                                case "18":
                                    STS = form.Controls.Find("tbLPMappingStatus", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Unexecuted";
                                                break;
                                            case '1':
                                                STS.Text = "Normal end";
                                                break;
                                            case '2':
                                                STS.Text = "Abnormal end";
                                                break;
                                        }
                                    }
                                    break;
                                case "19":
                                    STS = form.Controls.Find("tbLPInterlockKey", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "Enable";
                                                break;
                                            case '1':
                                            case '2':
                                            case '3':
                                                STS.Text = "Disable";
                                                break;
                                        }
                                    }
                                    break;

                                case "20":
                                    STS = form.Controls.Find("tbLPInfoPad", true).FirstOrDefault() as Label;
                                    if (STS != null)
                                    {
                                        switch (node.StatusRawData[i])
                                        {
                                            case '0':
                                                STS.Text = "No input";
                                                break;
                                            case '1':
                                                STS.Text = "A-pin ON";
                                                break;
                                            case '2':
                                                STS.Text = "B-pin ON";
                                                break;
                                            case '3':
                                                STS.Text = "A-pin/B-pin ON";
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }


            }
            catch
            {
                logger.Error("UpdateStatus: Update fail.");
            }
        }

        public static void UpdateSmifStatus(string NodeName, string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                ComboBox portName;
                if (form == null)
                    return;

                portName = form.Controls.Find("Cb_SMIFSelect", true).FirstOrDefault() as ComboBox;
                if (portName == null)
                    return;

                if (portName.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateSmifStatus);
                    portName.BeginInvoke(ph, NodeName, Data);
                }
                else
                {
                    if (portName.Text.Equals(NodeName))
                    {
                        Node port = NodeManagement.Get(NodeName);
                        MessageParser parser = new MessageParser(port.Vendor);
                        port.Status = parser.ParseMessage(Transaction.Command.LoadPortType.ReadStatus, Data);

                        foreach (KeyValuePair<string, string> item in port.Status)
                        {
                            Label StsLb = form.Controls.Find(item.Key+"_lb", true).FirstOrDefault() as Label;
                            if (StsLb != null)
                            {
                                StsLb.Text = item.Value;
                                if (item.Key.Equals("SLOTPOS"))
                                {
                                    ComboBox Slot_cb = form.Controls.Find("Move_To_Slot_cb", true).FirstOrDefault() as ComboBox;
                                    
                                    if (Slot_cb != null)
                                    {
                                        if (item.Value.Equals("255"))
                                        {
                                            Slot_cb.SelectedIndex = 0;
                                        }
                                        else
                                        {
                                            Slot_cb.SelectedIndex = Convert.ToInt32(item.Value);
                                        }
                                    }
                                    //Lab_I_Slot_11
                                    for(int i = 1; i <= 25; i++)
                                    {
                                        StsLb = form.Controls.Find("Lab_I_Slot_" + i.ToString("00"), true).FirstOrDefault() as Label;
                                        if(i== Convert.ToInt32(item.Value))
                                        {
                                            StsLb.BackColor = Color.Yellow;
                                        }
                                        else
                                        {
                                            StsLb.BackColor = Color.Silver;
                                        }

                                    }
                                }
                            }
                            
                        }
                    }
                }

            }
            catch
            {
                logger.Error("UpdateSmifStatus: Update fail.");
            }
        }

        public static void UpdateLED(string NodeName, string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                ComboBox portName;
                if (form == null)
                    return;

                portName = form.Controls.Find("Cb_LoadPortSelect", true).FirstOrDefault() as ComboBox;
                if (portName == null)
                    return;

                if (portName.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateLED);
                    portName.BeginInvoke(ph, NodeName, Data);
                }
                else
                {
                    if (portName.Text.Equals(NodeName))
                    {
                        Label LED = form.Controls.Find("LblLED_A", true).FirstOrDefault() as Label;
                        if (LED == null)
                            return;
                        LED.Text = Data;
                    }

                }


            }
            catch
            {
                logger.Error("UpdateControllerStatus: Update fail.");
            }
        }

        public static void UpdateMapping(string NodeName, string Data)
        {
            try
            {
                Form form = Application.OpenForms["FormManual"];
                ComboBox portName = null;
                if (form == null)
                    return;
                Node node = NodeManagement.Get(NodeName);

                switch(node.Vendor)
                {
                    case "SANWA_MC":
                        portName = form.Controls.Find("Cb_SMIFSelect", true).FirstOrDefault() as ComboBox;
                        break;

                    case "TDK":
                        portName = form.Controls.Find("cmbLoadportSelect", true).FirstOrDefault() as ComboBox;
                        break;
                }

                if (portName == null)
                    return;

                if (portName.InvokeRequired)
                {
                    UpdateData ph = new UpdateData(UpdateMapping);
                    portName.BeginInvoke(ph, NodeName, Data);
                }
                else
                {
                    if (portName.Text.Equals(NodeName))
                    {
                        node = NodeManagement.Get(NodeName);

                        switch (node.Vendor)
                        {
                            case "SANWA_MC":
                            case "TDK":
                                SanwaMappingResult(node, form, Data);
                                break;
                        }
                    }

                }

            }
            catch
            {
                logger.Error("UpdateControllerStatus: Update fail.");
            }
        }
        private static void SanwaMappingResult(Node node, Form form, string Data)
        {
            for (int i = Data.Length - 1; i >= 0; i--)
            {
                string Slot = (i + 1).ToString("00");
                Label slotLb = null;

                if(node.Vendor.Equals("SANWA_MC"))
                {
                    slotLb = form.Controls.Find("Lab_S_Slot_" + Slot, true).FirstOrDefault() as Label;
                }
                else if(node.Vendor.Equals("TDK"))
                {
                    slotLb = form.Controls.Find("lbMappingResult_" + Slot, true).FirstOrDefault() as Label;
                }

                if (slotLb == null) return;


                switch (Data[i])
                {
                    case '0':
                        slotLb.Text = "No wafer";
                        slotLb.BackColor = Color.Silver;
                        break;
                    case '1':
                        slotLb.Text = "Wafer";
                        slotLb.BackColor = Color.Lime;
                        break;
                    case '2':
                        slotLb.Text = "Crossed";
                        slotLb.BackColor = Color.Red;
                        break;
                    case '?':
                        slotLb.Text = "Undefined";
                        slotLb.BackColor = Color.Red;
                        break;
                    case 'W':
                        slotLb.Text = "Overlapping";
                        slotLb.BackColor = Color.Red;
                        break;
                    case 'E':
                        slotLb.Text = "Error";
                        slotLb.BackColor = Color.Red;
                        break;

                }
            }
        }
    }
}
