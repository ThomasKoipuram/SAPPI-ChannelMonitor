using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.Xml.Linq;

namespace ChannelMonitor
{
    public partial class Form1 : Form
    {
        String SearchQuery = "";
        String url = "";
        
        public Form1()
        {
            InitializeComponent();
            myToolTip();
        }

        private void myToolTip()
        {
            ToolTip myToolTip = new ToolTip();
            myToolTip.SetToolTip(textBox8, "Enter the Service name or use Wildcard * to select all");
            myToolTip.SetToolTip(textBox9, "Enter the Channel name or use Wildcard * to select all");
            myToolTip.SetToolTip(textBox4, "Filter results by Service Name");
            myToolTip.SetToolTip(textBox3, "Filter results by Channel");
            myToolTip.SetToolTip(textBox5, "Filter results by Activation State");
            myToolTip.SetToolTip(textBox6, "Filter results by Channel State");
            myToolTip.SetToolTip(textBox12, "Filter results by Error Log");
            myToolTip.SetToolTip(button3, "Remove all filters");
            myToolTip.SetToolTip(button5, "You can add upto 5 systems");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Application.UseWaitCursor = true;
            String temp1 = "";
            String url1 = "";
            //Save the current system selected in the system dropdown
            temp1 = comboBox1.Text.ToString();
            //MessageBox.Show("Text:"+temp1+"*");

            //Dynamically set the webservice url based on the values given in settings tab.
            if (temp1 != "Empty" && temp1 != "Select System")
            {
                if (SAPPIChannelMonitor.Properties.Settings.Default["sid1Name"].ToString() == temp1)
                {
                    url1 = SAPPIChannelMonitor.Properties.Settings.Default["sid1"].ToString();
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"].ToString() == temp1)
                {
                    url1 = SAPPIChannelMonitor.Properties.Settings.Default["sid2"].ToString();
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"].ToString() == temp1)
                {
                    url1 = SAPPIChannelMonitor.Properties.Settings.Default["sid3"].ToString();
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"].ToString() == temp1)
                {
                    url1 = SAPPIChannelMonitor.Properties.Settings.Default["sid4"].ToString();
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"].ToString() == temp1)
                {
                    url1 = SAPPIChannelMonitor.Properties.Settings.Default["sid5"].ToString();
                }

            //Call the URL and save response data as XML            
            //WebClient Client = new WebClient ();
            //Client.DownloadFile(url1, @"D:\channel.xml");
            
            //Replace unwanted tags in the downloaded XML
            XDocument doc = XDocument.Load("D:\\channel.xml");
            
            //Converting XDocument to String
            String xmlContent = doc.ToString();

            //Removing <!DOCTYPE ChannelStatusResult SYSTEM "/AdapterFramework/channelAdmin/ChannelAdmin.dtd"[]> from XML file
            string xmlHeader = @"<!DOCTYPE ChannelStatusResult SYSTEM ""/AdapterFramework/channelAdmin/ChannelAdmin.dtd""[]>";
            xmlContent = xmlContent.Replace(xmlHeader, "");

            //Removing the <Channels> node
            xmlContent = xmlContent.Replace("<Channels>", "");
            xmlContent = xmlContent.Replace("</Channels>", "");
            //MessageBox.Show(xmlContent);

            //Write it back to XDocument and overwrite file
            
            doc = XDocument.Parse(xmlContent);
            doc.Save("D:\\channel.xml");
            
            //Read the XML into the table view
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(@"D:\channel.xml");
            dataGridView1.DataSource = dataSet.Tables[0];

            //Remove unwanted columns from Dataset
            //dataGridView1.Columns.Remove("ChannelID");
            //dataGridView1.Columns.Remove("Party");

            //Set parameters for Wordwrap in DataGrid (Short log column)
            dataGridView1.Columns[4].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            //Set parameters for Column Width in DataGrid (Activation Mode, Channel Status) Col ID: 2,3
            DataGridViewColumn column2 = dataGridView1.Columns[2];
            column2.Width = 120;
            DataGridViewColumn column3 = dataGridView1.Columns[3];
            column3.Width = 120;

            Application.UseWaitCursor = false;
            button1.Enabled = true;
            }
            else
            {
                MessageBox.Show("Please select and save the system settings in the settings tab.");
                Application.UseWaitCursor = false;
                button1.Enabled = true;
            }
        }

        //Filter logic for all the columns implemented below
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            
            if(textBox4.Text!="")
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", textBox4.Text);
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }
            
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("AND Service LIKE '%{0}%'", textBox4.Text);
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("AND Service LIKE '%{0}%'", textBox4.Text);
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("AND Service LIKE '%{0}%'", textBox4.Text);
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("ShortLog LIKE '%{0}%'", textBox12.Text);
            }
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("AND Service LIKE '%{0}%'", textBox4.Text);
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;
        }

        //Logic for reset button in the Filter section
        private void button3_Click(object sender, EventArgs e)
        {
            textBox4.Text = "";
            textBox3.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox12.Text = "";

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = "";
        }

        //Saving application settings from the values entered in the settings tab
        private void Form1_Load(object sender, EventArgs e)
        {

            //Populating values from system settings into the combo box in the settings form
            String sid1 = "";
            String sid2 = "";
            String sid3 = "";
            String sid4 = "";
            String sid5 = "";
            String sid1Name = "";
            String sid2Name = "";
            String sid3Name = "";
            String sid4Name = "";
            String sid5Name = "";
            comboBox1.Text = "Select System";

            if (SAPPIChannelMonitor.Properties.Settings.Default["sid1"].ToString() == "")
            {
                sid1 = "Empty";
                sid1Name = "Empty";
            }
            else
            {
                sid1 = SAPPIChannelMonitor.Properties.Settings.Default["sid1"].ToString();
                sid1Name = SAPPIChannelMonitor.Properties.Settings.Default["sid1Name"].ToString();
            }
            if (SAPPIChannelMonitor.Properties.Settings.Default["sid2"].ToString() == "")
            {
                sid2 = "Empty";
                sid2Name = "Empty";
            }
            else
            {
                sid2 = SAPPIChannelMonitor.Properties.Settings.Default["sid2"].ToString();
                sid2Name = SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"].ToString();
            }
            if (SAPPIChannelMonitor.Properties.Settings.Default["sid3"].ToString() == "")
            {
                sid3 = "Empty";
                sid3Name = "Empty";
            }
            else
            {
                sid3 = SAPPIChannelMonitor.Properties.Settings.Default["sid3"].ToString();
                sid3Name = SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"].ToString();
            }
            if (SAPPIChannelMonitor.Properties.Settings.Default["sid4"].ToString() == "")
            {
                sid4 = "Empty";
                sid4Name = "Empty";
            }
            else
            {
                sid4 = SAPPIChannelMonitor.Properties.Settings.Default["sid4"].ToString();
                sid4Name = SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"].ToString();
            }
            if (SAPPIChannelMonitor.Properties.Settings.Default["sid5"].ToString() == "")
            {
                sid5 = "Empty";
                sid5Name = "Empty";
            }
            else
            {
                sid5 = SAPPIChannelMonitor.Properties.Settings.Default["sid5"].ToString();
                sid5Name = SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"].ToString();
            }

            comboBox1.DisplayMember = "Text";
            comboBox1.ValueMember = "Value";

            var items = new[]
            {
            new { Text = sid1Name, Value = sid1 },
            new { Text = sid2Name, Value = sid2 },
            new { Text = sid3Name, Value = sid3 },
            new { Text = sid4Name, Value = sid4 },
            new { Text = sid5Name, Value = sid5 }
            };
            comboBox1.DataSource = items;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //Saving the values from the settings form into the application settings file and ADDING NEW systems via form
            if (textBox1.Text != "" && textBox2.Text != "" && textBox13.Text != "" && textBox14.Text != "" && textBox15.Text != "")
            {
                url = "http://" + textBox1.Text + ":" + textBox2.Text + "/AdapterFramework/ChannelAdminServlet?party=*&service=" + textBox8.Text + "&channel=*&action=status&showProcessLog=true&j_user=" + textBox14.Text + "&j_password=" + textBox15.Text;
                if (SAPPIChannelMonitor.Properties.Settings.Default["sid1"].ToString() == "")
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid1"] = url;
                    SAPPIChannelMonitor.Properties.Settings.Default["sid1Name"] = textBox13.Text.ToUpper();
                    MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid2"].ToString() == "")
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid2"] = url;
                    SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"] = textBox13.Text.ToUpper();
                    MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid3"].ToString() == "")
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid3"] = url;
                    SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"] = textBox13.Text.ToUpper();
                    MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid4"].ToString() == "")
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid4"] = url;
                    SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"] = textBox13.Text.ToUpper();
                    MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid5"].ToString() == "")
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid5"] = url;
                    SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"] = textBox13.Text.ToUpper();
                    MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.");
                }
                else
                {
                    MessageBox.Show("You can only add 5 systems. Please delete one to continue.");
                }
            }
            else
            {
                MessageBox.Show("Either of SID, Hostname, Port, Username or Password is missing!");
            }
            this.Form1_Load(this, null);
            comboBox1.Text = "Select System";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //Deleting systems in settings form
            String temp = "";
            temp = comboBox1.Text.ToString();
            //MessageBox.Show("Text:"+temp+"*");
            if(temp != "Empty" && temp != "Select System")
            {
                if (SAPPIChannelMonitor.Properties.Settings.Default["sid1Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid1"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid1Name"] = "";
                    MessageBox.Show(temp + " system deleted");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid2"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"] = "";
                    MessageBox.Show(temp + " system deleted");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid3"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"] = "";
                    MessageBox.Show(temp + " system deleted");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid4"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"] = "";
                    MessageBox.Show(temp + " system deleted");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid5"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"] = "";
                    MessageBox.Show(temp + " system deleted");
                }
            }
            else
            {
                MessageBox.Show("Select a valid system to delete.");
            }
            this.Form1_Load(this, null);
            comboBox1.Text = "Select System";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //System selection notifier
            if(comboBox1.Text.ToString() != "Empty" && comboBox1.Text.ToString() != "Select System")
            { 
            MessageBox.Show(comboBox1.Text.ToString() + " System selection saved. Now you can run the Channel Monitor.");
            }
            else
            {
                MessageBox.Show("Select a valid system.");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
        }
    }
}
