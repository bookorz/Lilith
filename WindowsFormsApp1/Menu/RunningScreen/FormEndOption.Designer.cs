namespace Lilith.Menu.RunningScreen
{
    partial class FormEndOption
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.cycle_stop_rb = new System.Windows.Forms.RadioButton();
            this.lot_end_rb = new System.Windows.Forms.RadioButton();
            this.ok_btn = new System.Windows.Forms.Button();
            this.cancel_btn = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lot_end_rb);
            this.panel1.Controls.Add(this.cycle_stop_rb);
            this.panel1.Location = new System.Drawing.Point(39, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(237, 141);
            this.panel1.TabIndex = 0;
            // 
            // cycle_stop_rb
            // 
            this.cycle_stop_rb.AutoSize = true;
            this.cycle_stop_rb.Checked = true;
            this.cycle_stop_rb.Location = new System.Drawing.Point(82, 23);
            this.cycle_stop_rb.Name = "cycle_stop_rb";
            this.cycle_stop_rb.Size = new System.Drawing.Size(74, 16);
            this.cycle_stop_rb.TabIndex = 0;
            this.cycle_stop_rb.TabStop = true;
            this.cycle_stop_rb.Text = "Cycle Stop";
            this.cycle_stop_rb.UseVisualStyleBackColor = true;
            // 
            // lot_end_rb
            // 
            this.lot_end_rb.AutoSize = true;
            this.lot_end_rb.Location = new System.Drawing.Point(82, 78);
            this.lot_end_rb.Name = "lot_end_rb";
            this.lot_end_rb.Size = new System.Drawing.Size(61, 16);
            this.lot_end_rb.TabIndex = 1;
            this.lot_end_rb.Text = "Lot End";
            this.lot_end_rb.UseVisualStyleBackColor = true;
            // 
            // ok_btn
            // 
            this.ok_btn.Location = new System.Drawing.Point(39, 206);
            this.ok_btn.Name = "ok_btn";
            this.ok_btn.Size = new System.Drawing.Size(92, 44);
            this.ok_btn.TabIndex = 1;
            this.ok_btn.Text = "OK";
            this.ok_btn.UseVisualStyleBackColor = true;
            this.ok_btn.Click += new System.EventHandler(this.ok_btn_Click);
            // 
            // cancel_btn
            // 
            this.cancel_btn.Location = new System.Drawing.Point(184, 206);
            this.cancel_btn.Name = "cancel_btn";
            this.cancel_btn.Size = new System.Drawing.Size(92, 44);
            this.cancel_btn.TabIndex = 2;
            this.cancel_btn.Text = "Cancel";
            this.cancel_btn.UseVisualStyleBackColor = true;
            this.cancel_btn.Click += new System.EventHandler(this.cancel_btn_Click);
            // 
            // FormEndOption
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 262);
            this.Controls.Add(this.cancel_btn);
            this.Controls.Add(this.ok_btn);
            this.Controls.Add(this.panel1);
            this.Name = "FormEndOption";
            this.Text = "FormEndOption";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton lot_end_rb;
        private System.Windows.Forms.RadioButton cycle_stop_rb;
        private System.Windows.Forms.Button ok_btn;
        private System.Windows.Forms.Button cancel_btn;
    }
}