namespace MsgParserTestApp
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
            this.messageTxt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.timezoneTxt = new System.Windows.Forms.TextBox();
            this.responseTxt = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.portTxt = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pathTxt = new System.Windows.Forms.TextBox();
            this.dslCheckBox = new System.Windows.Forms.CheckBox();
            this.actionBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.toUserTxt = new System.Windows.Forms.TextBox();
            this.msgTxt = new System.Windows.Forms.TextBox();
            this.serverTimeTxt = new System.Windows.Forms.TextBox();
            this.userTimeTxt = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.errorTxt = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // messageTxt
            // 
            this.messageTxt.Location = new System.Drawing.Point(12, 27);
            this.messageTxt.Name = "messageTxt";
            this.messageTxt.Size = new System.Drawing.Size(433, 20);
            this.messageTxt.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Message";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Timezone";
            // 
            // timezoneTxt
            // 
            this.timezoneTxt.Location = new System.Drawing.Point(82, 54);
            this.timezoneTxt.Name = "timezoneTxt";
            this.timezoneTxt.Size = new System.Drawing.Size(63, 20);
            this.timezoneTxt.TabIndex = 2;
            this.timezoneTxt.Text = "-0500";
            // 
            // responseTxt
            // 
            this.responseTxt.Location = new System.Drawing.Point(12, 161);
            this.responseTxt.Multiline = true;
            this.responseTxt.Name = "responseTxt";
            this.responseTxt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.responseTxt.Size = new System.Drawing.Size(433, 95);
            this.responseTxt.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 145);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Response";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(362, 90);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(83, 40);
            this.button1.TabIndex = 6;
            this.button1.Text = "Send";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Port";
            // 
            // portTxt
            // 
            this.portTxt.Location = new System.Drawing.Point(23, 110);
            this.portTxt.Name = "portTxt";
            this.portTxt.Size = new System.Drawing.Size(63, 20);
            this.portTxt.TabIndex = 7;
            this.portTxt.Text = "666";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(102, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Path";
            // 
            // pathTxt
            // 
            this.pathTxt.Location = new System.Drawing.Point(105, 110);
            this.pathTxt.Name = "pathTxt";
            this.pathTxt.Size = new System.Drawing.Size(63, 20);
            this.pathTxt.TabIndex = 9;
            this.pathTxt.Text = "666";
            // 
            // dslCheckBox
            // 
            this.dslCheckBox.AutoSize = true;
            this.dslCheckBox.Checked = true;
            this.dslCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.dslCheckBox.Location = new System.Drawing.Point(162, 57);
            this.dslCheckBox.Name = "dslCheckBox";
            this.dslCheckBox.Size = new System.Drawing.Size(53, 17);
            this.dslCheckBox.TabIndex = 11;
            this.dslCheckBox.Text = "DSL?";
            this.dslCheckBox.UseVisualStyleBackColor = true;
            // 
            // actionBox
            // 
            this.actionBox.FormattingEnabled = true;
            this.actionBox.Items.AddRange(new object[] {
            "creation_parse",
            "repeat_parse"});
            this.actionBox.Location = new System.Drawing.Point(325, 53);
            this.actionBox.Name = "actionBox";
            this.actionBox.Size = new System.Drawing.Size(122, 21);
            this.actionBox.TabIndex = 12;
            this.actionBox.Text = "creation_parse";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(266, 58);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Action";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(50, 271);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "To User:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(21, 297);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Message Text:";
            // 
            // toUserTxt
            // 
            this.toUserTxt.Location = new System.Drawing.Point(109, 271);
            this.toUserTxt.Name = "toUserTxt";
            this.toUserTxt.Size = new System.Drawing.Size(336, 20);
            this.toUserTxt.TabIndex = 16;
            // 
            // msgTxt
            // 
            this.msgTxt.Location = new System.Drawing.Point(110, 297);
            this.msgTxt.Name = "msgTxt";
            this.msgTxt.Size = new System.Drawing.Size(336, 20);
            this.msgTxt.TabIndex = 17;
            // 
            // serverTimeTxt
            // 
            this.serverTimeTxt.Location = new System.Drawing.Point(111, 349);
            this.serverTimeTxt.Name = "serverTimeTxt";
            this.serverTimeTxt.Size = new System.Drawing.Size(336, 20);
            this.serverTimeTxt.TabIndex = 21;
            // 
            // userTimeTxt
            // 
            this.userTimeTxt.Location = new System.Drawing.Point(110, 323);
            this.userTimeTxt.Name = "userTimeTxt";
            this.userTimeTxt.Size = new System.Drawing.Size(336, 20);
            this.userTimeTxt.TabIndex = 20;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 349);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Server Time String";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(11, 323);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 13);
            this.label10.TabIndex = 18;
            this.label10.Text = "User Time String:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Black;
            this.pictureBox1.Location = new System.Drawing.Point(-9, 136);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(502, 1);
            this.pictureBox1.TabIndex = 22;
            this.pictureBox1.TabStop = false;
            // 
            // errorTxt
            // 
            this.errorTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.errorTxt.Location = new System.Drawing.Point(111, 375);
            this.errorTxt.Name = "errorTxt";
            this.errorTxt.Size = new System.Drawing.Size(336, 20);
            this.errorTxt.TabIndex = 24;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(9, 375);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(89, 13);
            this.label11.TabIndex = 23;
            this.label11.Text = "ERROR CODE";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 414);
            this.Controls.Add(this.errorTxt);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.serverTimeTxt);
            this.Controls.Add(this.userTimeTxt);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.msgTxt);
            this.Controls.Add(this.toUserTxt);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.actionBox);
            this.Controls.Add(this.dslCheckBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pathTxt);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.portTxt);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.responseTxt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.timezoneTxt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.messageTxt);
            this.Name = "Form1";
            this.Text = "Message Parser Test App";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox messageTxt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox timezoneTxt;
        private System.Windows.Forms.TextBox responseTxt;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox portTxt;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox pathTxt;
        private System.Windows.Forms.CheckBox dslCheckBox;
        private System.Windows.Forms.ComboBox actionBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox toUserTxt;
        private System.Windows.Forms.TextBox msgTxt;
        private System.Windows.Forms.TextBox serverTimeTxt;
        private System.Windows.Forms.TextBox userTimeTxt;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox errorTxt;
        private System.Windows.Forms.Label label11;
    }
}

