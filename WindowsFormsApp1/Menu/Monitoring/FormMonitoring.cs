using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TransferControl.Management;

namespace Lilith.Menu.Monitoring
{
    public partial class FormMonitoring : Lilith.Menu.FormFrame
    {
        public FormMonitoring()
        {
            InitializeComponent();
        }


        private void LoadPort_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 1:
                    List<Job> JobList = (sender as DataGridView).DataSource as List<Job>;

                    switch (JobList[e.RowIndex].NeedProcess)
                    {
                        case true:
                            e.CellStyle.BackColor = Color.Green;
                            e.CellStyle.ForeColor = Color.White;
                            break;

                    }

                    switch (e.Value)
                    {
                        case "No wafer":
                            e.CellStyle.BackColor = Color.Gray;
                            e.CellStyle.ForeColor = Color.White;
                            break;
                        case "Crossed":
                        case "Undefined":
                        case "Double":
                            e.CellStyle.BackColor = Color.Red;
                            e.CellStyle.ForeColor = Color.White;
                            break;


                    }
                    break;

            }
        }

        private void label142_Click(object sender, EventArgs e)
        {

        }

        private void label140_Click(object sender, EventArgs e)
        {

        }

        private void label138_Click(object sender, EventArgs e)
        {

        }

        private void label136_Click(object sender, EventArgs e)
        {

        }

        private void label134_Click(object sender, EventArgs e)
        {

        }

        private void label132_Click(object sender, EventArgs e)
        {

        }

        private void label130_Click(object sender, EventArgs e)
        {

        }

        private void label128_Click(object sender, EventArgs e)
        {

        }

        private void label126_Click(object sender, EventArgs e)
        {

        }

        private void label124_Click(object sender, EventArgs e)
        {

        }

        private void label122_Click(object sender, EventArgs e)
        {

        }

        private void label120_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label118_Click(object sender, EventArgs e)
        {

        }

        private void Slot_Click(object sender, EventArgs e)
        {

            string[] ary = (sender as Label).Name.Split('_');
            if (ary.Length == 3)
            {
                string Port = ary[0];
                string Slot = ary[2];
                Node p = NodeManagement.Get(Port);
                if (p != null)
                {
                    Job j;
                    if (p.JobList.TryGetValue(Slot, out j))
                    {
                        if (j.OCRImgPath == "")
                        {
                            MessageBox.Show("未找到OCR紀錄");
                        }
                        else
                        {
                            OCRResult form2 = new OCRResult(j);
                            form2.ShowDialog();
                            //// open image in default viewer
                            //System.Diagnostics.Process.Start(j.OCRImgPath);
                        }
                    }
                    else
                    {
                        MessageBox.Show("未找到Wafer");
                    }
                }

            }
        }

        private void OCR01_Pic_DoubleClick(object sender, EventArgs e)
        {
            OCRResult form2 = new OCRResult((sender as PictureBox).Tag as Job);
            form2.ShowDialog();
        }

        private void OCR02_Pic_DoubleClick(object sender, EventArgs e)
        {
            OCRResult form2 = new OCRResult((sender as PictureBox).Tag as Job);
            form2.ShowDialog();
        }

        //private void RunSwitch_Click(object sender, EventArgs e)
        //{
        //    FormMain.AutoReverse = AutoReverse_ck.Checked;
        //    if (RunSwitch.Text.Equals("Start"))
        //    {
        //        if (NodeManagement.IsNeedInitial())
        //        {
        //            MessageBox.Show("請先執行初始化功能.");
        //        }
        //        else
        //        {
        //            RunSwitch.Enabled = false;
        //            foreach (Node each in NodeManagement.GetList())
        //            {
        //                each.InitialComplete = false;
        //            }
        //            //RunSwitch.Text = "Stop";
        //            //RunSwitch.BackColor = Color.OrangeRed;
        //            FormMain.RouteCtrl.Start("Running");
        //        }
        //    }
        //    else
        //    {
        //        RunSwitch.Text = "Start";
        //        RunSwitch.Enabled = false;
        //        RunSwitch.BackColor = Color.Lime;
        //        FormMain.RouteCtrl.Stop();
        //    }
        //}

        //private void AutoReverse_ck_CheckedChanged(object sender, EventArgs e)
        //{
        //    FormMain.AutoReverse = AutoReverse_ck.Checked;
        //    if (!AutoReverse_ck.Checked && FormMain.RouteCtrl.GetMode().Equals("Start"))
        //    {
        //        AutoReverse_ck.Enabled = false;
        //    }
        //}

        //private void Initial_btn_Click(object sender, EventArgs e)
        //{
        //    if (FormMain.RouteCtrl.GetMode().Equals("Start"))
        //    {
        //        MessageBox.Show("目前為Start模式，無法進行Initial.");
        //    }
        //    else
        //    {
        //        string strMsg = "This equipment performs the initialization and origin search OK?\r\n" + "This equipment will be initalized, each axis will return to home position.\r\n" + "Check the condition of the wafer.";
        //        if (MessageBox.Show(strMsg, "Initialize", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.OK)
        //        {
        //            Initial_btn.Enabled = false;
        //            foreach (Node each in NodeManagement.GetList())
        //            {
        //                each.CheckStatus = false;
        //                switch (each.Type)
        //                {
        //                    case "ALIGNER":

        //                        each.ErrorMsg = "";
        //                        each.ExcuteScript("AlignerStateGet", "GetStatsBeforeInit");
        //                        break;
        //                    case "ROBOT":
        //                        each.ErrorMsg = "";
        //                        each.ExcuteScript("RobotStateGet", "GetStatsBeforeInit");
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //}

        private void NotchDirect_cb_TextChanged(object sender, EventArgs e)
        {
            switch (NotchDirect_cb.Text)
            {
                case "朝前":
                    FormMain.RouteCtrl.NotchDirect = 90;
                    break;
                case "朝左":
                    FormMain.RouteCtrl.NotchDirect = 180;
                    break;
                case "朝右":
                    FormMain.RouteCtrl.NotchDirect = 0;
                    break;
                case "朝後":
                    FormMain.RouteCtrl.NotchDirect = 270;
                    break;
            }
        }

        private void ResetInterface_Click(object sender, EventArgs e)
        {
            FormMain.ctrl.Reset();
        }

        private void FormMonitoring_Load(object sender, EventArgs e)
        {
           
            //MAPDT_ck.Checked = FormMain.ctrl.Events.MAPDT;
            //PORT_ck.Checked = FormMain.ctrl.Events.PORT;
            //PRS_ck.Checked = FormMain.ctrl.Events.PRS;
            //SYSTEM_ck.Checked = FormMain.ctrl.Events.SYSTEM;
            //TRANSREQ_ck.Checked = FormMain.ctrl.Events.TRANSREQ;
            //FFU_ck.Checked = FormMain.ctrl.Events.FFU;

        }

        private void Events_CheckedChanged(object sender, EventArgs e)
        {
            switch ((sender as CheckBox).Name)
            {
                case "MAPDT_ck":
                    FormMain.ctrl.Events.MAPDT = MAPDT_ck.Checked;
                    break;
                case "TRANSREQ_ck":
                    FormMain.ctrl.Events.TRANSREQ = TRANSREQ_ck.Checked;
                    break;
                case "SYSTEM_ck":
                    FormMain.ctrl.Events.SYSTEM = SYSTEM_ck.Checked;
                    break;
                case "PORT_ck":
                    FormMain.ctrl.Events.PORT = PORT_ck.Checked;
                    break;
                case "PRS_ck":
                    FormMain.ctrl.Events.PRS = PRS_ck.Checked;
                    break;
                case "FFU_ck":
                    FormMain.ctrl.Events.FFU = FFU_ck.Checked;
                    break;
            }
            FormMain.ctrl.Events.Save();
        }
    }
}
