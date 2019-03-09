namespace CPMPayAgentLisenceKeyGenerator
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtSystemInfo = new System.Windows.Forms.TextBox();
            this.btnSystemInfo = new System.Windows.Forms.Button();
            this.btnLisenceKey = new System.Windows.Forms.Button();
            this.xtxLisenceKey = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dtPickerLisence = new System.Windows.Forms.DateTimePicker();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "System Information";
            // 
            // txtSystemInfo
            // 
            this.txtSystemInfo.Location = new System.Drawing.Point(133, 39);
            this.txtSystemInfo.Name = "txtSystemInfo";
            this.txtSystemInfo.Size = new System.Drawing.Size(328, 20);
            this.txtSystemInfo.TabIndex = 1;
            // 
            // btnSystemInfo
            // 
            this.btnSystemInfo.Location = new System.Drawing.Point(497, 37);
            this.btnSystemInfo.Name = "btnSystemInfo";
            this.btnSystemInfo.Size = new System.Drawing.Size(120, 23);
            this.btnSystemInfo.TabIndex = 2;
            this.btnSystemInfo.Text = "Get System Info";
            this.btnSystemInfo.UseVisualStyleBackColor = true;
            this.btnSystemInfo.Click += new System.EventHandler(this.btnSystemInfo_Click);
            // 
            // btnLisenceKey
            // 
            this.btnLisenceKey.Enabled = false;
            this.btnLisenceKey.Location = new System.Drawing.Point(497, 87);
            this.btnLisenceKey.Name = "btnLisenceKey";
            this.btnLisenceKey.Size = new System.Drawing.Size(120, 23);
            this.btnLisenceKey.TabIndex = 5;
            this.btnLisenceKey.Text = "Register Lisence Key";
            this.btnLisenceKey.UseVisualStyleBackColor = true;
            this.btnLisenceKey.Click += new System.EventHandler(this.btnLisenceKey_Click);
            // 
            // xtxLisenceKey
            // 
            this.xtxLisenceKey.Location = new System.Drawing.Point(133, 89);
            this.xtxLisenceKey.Name = "xtxLisenceKey";
            this.xtxLisenceKey.Size = new System.Drawing.Size(328, 20);
            this.xtxLisenceKey.TabIndex = 4;
            this.xtxLisenceKey.TextChanged += new System.EventHandler(this.xtxLisenceKey_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Lisence Key";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Valid till";
            // 
            // dtPickerLisence
            // 
            this.dtPickerLisence.Location = new System.Drawing.Point(133, 132);
            this.dtPickerLisence.Name = "dtPickerLisence";
            this.dtPickerLisence.Size = new System.Drawing.Size(200, 20);
            this.dtPickerLisence.TabIndex = 7;
            this.dtPickerLisence.Value = new System.DateTime(2014, 12, 31, 23, 59, 0, 0);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 167);
            this.Controls.Add(this.dtPickerLisence);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnLisenceKey);
            this.Controls.Add(this.xtxLisenceKey);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSystemInfo);
            this.Controls.Add(this.txtSystemInfo);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "CPM PayAgent Lisence Registrator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSystemInfo;
        private System.Windows.Forms.Button btnSystemInfo;
        private System.Windows.Forms.Button btnLisenceKey;
        private System.Windows.Forms.TextBox xtxLisenceKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtPickerLisence;
    }
}

