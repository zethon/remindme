namespace WebServiceTestApp
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
            this.txtboxBody = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtboxResponse = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.txtboxUrl = new System.Windows.Forms.TextBox();
            this.labelju93 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtboxBody
            // 
            this.txtboxBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtboxBody.Location = new System.Drawing.Point(13, 77);
            this.txtboxBody.Multiline = true;
            this.txtboxBody.Name = "txtboxBody";
            this.txtboxBody.Size = new System.Drawing.Size(531, 104);
            this.txtboxBody.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Body";
            // 
            // txtboxResponse
            // 
            this.txtboxResponse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtboxResponse.Location = new System.Drawing.Point(13, 225);
            this.txtboxResponse.Multiline = true;
            this.txtboxResponse.Name = "txtboxResponse";
            this.txtboxResponse.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtboxResponse.Size = new System.Drawing.Size(531, 316);
            this.txtboxResponse.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(464, 191);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(80, 28);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtboxUrl
            // 
            this.txtboxUrl.Location = new System.Drawing.Point(13, 28);
            this.txtboxUrl.Name = "txtboxUrl";
            this.txtboxUrl.Size = new System.Drawing.Size(531, 20);
            this.txtboxUrl.TabIndex = 4;
            this.txtboxUrl.Text = "http://dev.remindme.cc/service.php?debug=1";
            // 
            // labelju93
            // 
            this.labelju93.AutoSize = true;
            this.labelju93.Location = new System.Drawing.Point(10, 9);
            this.labelju93.Name = "labelju93";
            this.labelju93.Size = new System.Drawing.Size(29, 13);
            this.labelju93.TabIndex = 5;
            this.labelju93.Text = "URL";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(563, 553);
            this.Controls.Add(this.labelju93);
            this.Controls.Add(this.txtboxUrl);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtboxResponse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtboxBody);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtboxBody;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtboxResponse;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtboxUrl;
        private System.Windows.Forms.Label labelju93;
    }
}

