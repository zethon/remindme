using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using MsgParser;


namespace MsgParserTestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            responseTxt.Text = string.Empty;
            toUserTxt.Text = string.Empty;
            msgTxt.Text = string.Empty;
            userTimeTxt.Text = string.Empty;
            serverTimeTxt.Text = string.Empty;

            MessageParser parser = new MessageParser(portTxt.Text, pathTxt.Text);
            
            bool bSent = parser.SendParseRequest(messageTxt.Text, timezoneTxt.Text, dslCheckBox.Checked, actionBox.Text);

            if (!bSent)
            {
                MessageBox.Show("Could not send request.");
                return;
            }

            if (!parser.ProcessResponse())
            {
                MessageBox.Show("Could not process response.");
                return;
            }

            responseTxt.Text = parser.RawResponse;
            
            toUserTxt.Text = parser.ToUser;
            msgTxt.Text = parser.MessageText;
            userTimeTxt.Text = parser.UserTimeString;
            serverTimeTxt.Text = parser.ServerTimeString;

            errorTxt.Text = parser.ParserErrorCode;


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}