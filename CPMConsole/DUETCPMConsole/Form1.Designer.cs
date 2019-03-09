namespace DUETCPMConsole
{
    partial class CPMPAMainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CPMPAMainForm));
            this.txtLogs = new System.Windows.Forms.RichTextBox();
            this.btnStopDuet = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.notifyIconDUET = new System.Windows.Forms.NotifyIcon(this.components);
            this.SuspendLayout();
            // 
            // txtLogs
            // 
            this.txtLogs.Location = new System.Drawing.Point(12, 95);
            this.txtLogs.Name = "txtLogs";
            this.txtLogs.Size = new System.Drawing.Size(729, 240);
            this.txtLogs.TabIndex = 0;
            this.txtLogs.Text = "";
            // 
            // btnStopDuet
            // 
            this.btnStopDuet.BackColor = System.Drawing.Color.Salmon;
            this.btnStopDuet.Location = new System.Drawing.Point(650, 32);
            this.btnStopDuet.Name = "btnStopDuet";
            this.btnStopDuet.Size = new System.Drawing.Size(75, 23);
            this.btnStopDuet.TabIndex = 1;
            this.btnStopDuet.Text = "Start";
            this.btnStopDuet.UseVisualStyleBackColor = false;
            this.btnStopDuet.Click += new System.EventHandler(this.btnStopDuet_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Logs:";
            // 
            // notifyIconDUET
            // 
            this.notifyIconDUET.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconDUET.Icon")));
            this.notifyIconDUET.Text = "CPM-DUET service";
            this.notifyIconDUET.Visible = true;
            this.notifyIconDUET.DoubleClick += new System.EventHandler(this.notifyIconDUET_DoubleClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 347);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStopDuet);
            this.Controls.Add(this.txtLogs);
            this.Name = "Form1";
            this.Text = "CPM Pay-Agent Console";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtLogs;
        private System.Windows.Forms.Button btnStopDuet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NotifyIcon notifyIconDUET;
    }
}

