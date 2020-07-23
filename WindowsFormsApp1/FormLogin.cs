using Lilith.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class FormLogin : Form
    {
        //ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        ILog log = LogManager.GetLogger("FormLogin");
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {

            Boolean result = false;
            string user_id = tbUserID.Text;
            string user_pwd = tbPassword.Text;
            if (user_id=="SANWA"&& user_pwd=="ADMIN123")
            {

                string msg = "{\"user_id\": " + user_id + "\", \"action\": \"Login\"}";
                log.Info(msg);
                //SanwaUtil.addActionLog("Authority", "Login", user_id);// add record to log_system_action
               
                                                                               //log.Debug(msg);
                this.Close();
            }
            else
            {
                MessageBox.Show("Please check data and login again.", "Login Fail");
            }


        }

        private void FormLogin_Activated(object sender, EventArgs e)
        {
            tbUserID.Focus();
        }

        private void tbUserID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (tbUserID.Text.Trim().Equals(string.Empty))
                {
                    tbUserID.Focus();
                }

                if (tbUserID.Text.Trim().Equals(string.Empty))
                {
                    tbPassword.Focus();
                }

                btnLogin.Focus();
                btnLogin_Click(this, e);
            }
        }
    }
}
