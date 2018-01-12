using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Collections;

namespace WebServiceTestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WebClient client = new WebClient();
            txtboxResponse.Text = string.Empty;

            try
            {
                txtboxResponse.Text = client.UploadString(txtboxUrl.Text, txtboxBody.Text).Replace("\n", "\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //System.Diagnostics.Debugger.Break();                
            }

        }
    }
}
