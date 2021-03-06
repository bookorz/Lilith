﻿using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TransferControl.Comm;

namespace Lilith.UI_Update.Monitoring
{
    class NodeStatusUpdate
    {
        static ILog logger = LogManager.GetLogger(typeof(NodeStatusUpdate));
        delegate void UpdateNode(string Device_ID, string State);
        delegate void UpdateState(string State);
        static List<Setting> SignalSetting;
        class Setting
        {
            public string Eqp_Status { get; set; }
            public bool Is_Alarm { get; set; }
            public string Red { get; set; }
            public string Orange { get; set; }
            public string Blue { get; set; }
            public string Green { get; set; }
            public string Buzzer1 { get; set; }
            public string Buzzer2 { get; set; }
        }

        public static void InitialSetting()
        {
            DBUtil dBUtil = new DBUtil();

            string Sql = @"select * from config_signal_tower";
            DataTable dt = dBUtil.GetDataTable(Sql, null);

            string str_json = JsonConvert.SerializeObject(dt, Formatting.Indented);
            SignalSetting = JsonConvert.DeserializeObject<List<Setting>>(str_json);

        }

        public static void UpdateNodeState(string Device_ID, string State)
        {
            try
            {
                Form form = Application.OpenForms["FormMain"];
                TextBox State_tb;
                if (form == null)
                    return;

                State_tb = form.Controls.Find(Device_ID + "_State", true).FirstOrDefault() as TextBox;
                if (State_tb == null)
                    return;

                if (State_tb.InvokeRequired)
                {
                    UpdateNode ph = new UpdateNode(UpdateNodeState);
                    State_tb.BeginInvoke(ph, Device_ID, State);
                }
                else
                {
                    State_tb.Text = State;
                    switch (State)
                    {
                        case "Run":
                            State_tb.BackColor = Color.Lime;
                            //UpdateCurrentState();
                            break;
                        case "Idle":
                            State_tb.BackColor = Color.Orange;
                            //UpdateCurrentState();
                            break;
                        case "Ready To Load":
                            State_tb.BackColor = Color.DarkGray;
                            break;
                        case "Load Complete":
                            State_tb.BackColor = Color.Green;
                            break;
                        case "Ready To UnLoad":
                            State_tb.BackColor = Color.DarkOrange;
                            break;
                        case "UnLoad Complete":
                            State_tb.BackColor = Color.Blue;
                            break;
                        case "Alarm":
                            State_tb.BackColor = Color.Red;
                            //UpdateCurrentState();
                            break;
                    }

                }


            }
            catch
            {
                logger.Error("UpdateControllerStatus: Update fail.");
            }
        }

        public static void UpdateCurrentState(string State)
        {
            try
            {
                Form form = Application.OpenForms["FormMain"];

                if (form == null)
                    return;

                Button state_btn = form.Controls.Find("CurrentState_btn", true).FirstOrDefault() as Button;

                if (state_btn == null)
                    return;

                if (state_btn.InvokeRequired)
                {
                    UpdateState ph = new UpdateState(UpdateCurrentState);
                    state_btn.BeginInvoke(ph, State);
                }
                else
                {
                   
                    state_btn.Text = State;

                }


            }
            catch (Exception e)
            {
                logger.Error("UpdateCurrentState: Update fail.:" + e.StackTrace);
            }
        }

    }
}
