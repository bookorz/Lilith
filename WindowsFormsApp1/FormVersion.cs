using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class FormVersion : Form
    {
        public FormVersion()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

        }

        private void FormVersion_Load(object sender, EventArgs e)
        {
            //try
            //{
            //    Ver_lb.Text = "Version " + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() + " (2018)";
            //}
            //catch
            //{
            //    Ver_lb.Text = "Version: 2.1.1.5(2018)";
            //}
            Ver_lb.Text = "Version: 2.1.3.0(2019)";
        }
    
    }
}
