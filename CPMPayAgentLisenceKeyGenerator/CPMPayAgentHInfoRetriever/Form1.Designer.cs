namespace CPMPayAgentHInfoRetriever
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
            this.btnSystemInfo = new System.Windows.Forms.Button();
            this.txtSystemInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSystemInfo
            // 
            this.btnSystemInfo.Location = new System.Drawing.Point(488, 28);
            this.btnSystemInfo.Name = "btnSystemInfo";
            this.btnSystemInfo.Size = new System.Drawing.Size(120, 23);
            this.btnSystemInfo.TabIndex = 5;
            this.btnSystemInfo.Text = "Generate";
            this.btnSystemInfo.UseVisualStyleBackColor = true;
            this.btnSystemInfo.Click += new System.EventHandler(this.btnSystemInfo_Click);
            // 
            // txtSystemInfo
            // 
            this.txtSystemInfo.Location = new System.Drawing.Point(124, 30);
            this.txtSystemInfo.Name = "txtSystemInfo";
            this.txtSystemInfo.Size = new System.Drawing.Size(328, 20);
            this.txtSystemInfo.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "System Key";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(642, 86);
            this.Controls.Add(this.btnSystemInfo);
            this.Controls.Add(this.txtSystemInfo);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Generate Key";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSystemInfo;
        private System.Windows.Forms.TextBox txtSystemInfo;
        private System.Windows.Forms.Label label1;
    }
}

