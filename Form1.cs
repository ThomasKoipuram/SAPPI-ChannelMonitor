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
        //Setting some variables globally
        String SearchQuery = "";
        String url = "";
        int chCount = 0;
        
        public Form1()
        {
            InitializeComponent();
            myToolTip();
        }

        //Setting Tool tip values
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
            myToolTip.SetToolTip(button3, "Remove all applied filters");
            myToolTip.SetToolTip(button5, "You can save upto 5 systems");
            myToolTip.SetToolTip(button1, "To prevent unnecessary load in the system, avoid running with both parameters as *.");
            myToolTip.SetToolTip(button6, "Caution: The system entry will be deleted instantly!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label21.Visible = true;
            Cursor.Current = Cursors.WaitCursor;
            String temp1 = "";
            String url1 = "";
            //Save the current system selected in the system dropdown
            temp1 = comboBox1.Text.ToString();
            //MessageBox.Show("Selected System:"+temp1+"*");

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
                //MessageBox.Show(url1);
                //Call the URL and save response data as XML. Handle any exceptions while doing the webservice call
                try
                {
                    //******************Read workspace location from system settings******************//
                    String url1Temp = "";
                    String xDocTemp = "";
                    url1Temp = SAPPIChannelMonitor.Properties.Settings.Default["workspace"].ToString();
                    xDocTemp = url1Temp.Replace(@":\", @":\\");

                    WebClient Client = new WebClient();
                    //Client.DownloadFile(url1, @"D:\channel.xml");
                    //Client.DownloadFile(url1, url1Temp);
                    //MessageBox.Show("Download complete...");

                    //******************Formating file location to include file name******************//

                    url1Temp = url1Temp + "channel.xml";
                    xDocTemp = xDocTemp + "channel.xml";
                    //MessageBox.Show(xDocTemp);
                    //MessageBox.Show(url1Temp);

                    //******************Replace unwanted tags in the downloaded XML******************//

                    //MessageBox.Show("Trying to load file from downloaded location *" + xDocTemp+ "*");
                    string xmlContent = System.IO.File.ReadAllText(url1Temp);
                    //MessageBox.Show("Load complete..." + xmlContent);

                    //******************Removing funny characters from content******************//

                    xmlContent = xmlContent.Replace(@"<ShortLog>", @"<ShortLog><![CDATA[");
                    xmlContent = xmlContent.Replace(@"</ShortLog>", @"]]></ShortLog>");
                    //xmlContent = xmlContent.Replace("AS2MessageId <24719174.410397895.2016-06-17.122807.34@tradecard>, 

                    //**************************************************************************//

                    //Removing <!DOCTYPE ChannelStatusResult SYSTEM "/AdapterFramework/channelAdmin/ChannelAdmin.dtd"[]> from XML file
                    string xmlHeader = @"<!DOCTYPE ChannelStatusResult SYSTEM ""/AdapterFramework/channelAdmin/ChannelAdmin.dtd""[]>";
                    xmlContent = xmlContent.Replace(xmlHeader, @"");

                    //******************Removing the <Channels> node********************//

                    xmlContent = xmlContent.Replace(@"<Channels>", @"");
                    xmlContent = xmlContent.Replace(@"</Channels>", @"");
                    //MessageBox.Show("Going to save file" + xmlContent + "*");

                    //******************Write it back to XDocument and overwrite file********************//  

                    var streamWriter = System.IO.File.CreateText(xDocTemp);
                    streamWriter.Write(xmlContent);

                    //******************Finish writing to the file********************//

                    streamWriter.Close();
                    //MessageBox.Show("Savedfile" + xmlContent + "*");

                    //******************Read the XML into the table view********************//

                    DataSet dataSet = new DataSet();
                    //dataSet.ReadXml(@"D:\channel.xml");
                    dataSet.ReadXml(url1Temp);
                    dataGridView1.DataSource = dataSet.Tables[0];

                    //******************Remove unwanted columns from Dataset********************//

                    dataGridView1.Columns.Remove("ChannelID");
                    dataGridView1.Columns.Remove("Party");

                    //******************Set parameters for Wordwrap in DataGrid (Short log column)********************//

                    dataGridView1.Columns[4].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                    //Set parameters for Column Width in DataGrid (Activation Mode, Channel Status) Col ID: 2,3

                    DataGridViewColumn column2 = dataGridView1.Columns[2];
                    column2.Width = 120;
                    DataGridViewColumn column3 = dataGridView1.Columns[3];
                    column3.Width = 120;

                    //******************Count of rows in datagrid to be displayed in Label 7********************//
                    int chCount = dataGridView1.Rows.Count;
                    if (chCount == 0)
                    {
                        label7.Text = " 0 Channels found";
                    }
                    else if (chCount == 2)
                    {
                        label7.Text = (chCount - 1).ToString() + " Channel found";
                    }
                    else
                    {
                        label7.Text = (chCount - 1).ToString() + " Channels found";
                    }

                    Cursor.Current = Cursors.Default;
                    button1.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Check the system parameters for System " + temp1 + "." + ex + ".", "Error calling Webservice");
                    label21.Visible = false;
                    Cursor.Current = Cursors.Default;
                    button1.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Please select and save the system settings in the settings tab.", "Error");
                label21.Visible = false;
                Cursor.Current = Cursors.Default;
                button1.Enabled = true;
            }
        }

        //Filter logic for all the columns implemented below
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", textBox4.Text);
            }
            else
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", "*");
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", "*");
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", "*");
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", "*");
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", "*");
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;

            //******************Count of rows in datagrid to be displayed in Label 7********************//
            chCount = dataGridView1.Rows.Count;
            if (chCount == 0)
            {
                label7.Text = " 0 Channels found";
            }
            else if (chCount == 2)
            {
                label7.Text = (chCount - 1).ToString() + " Channel found";
            }
            else
            {
                label7.Text = (chCount - 1).ToString() + " Channels found";
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", textBox4.Text);
            }
            else
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", "*");
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", "*");
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", "*");
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", "*");
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", "*");
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;

            //******************Count of rows in datagrid to be displayed in Label 7********************//

            chCount = dataGridView1.Rows.Count;
            if (chCount == 0)
            {
                label7.Text = " 0 Channels found";
            }
            else if (chCount == 2)
            {
                label7.Text = (chCount - 1).ToString() + " Channel found";
            }
            else
            {
                label7.Text = (chCount - 1).ToString() + " Channels found";
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", textBox4.Text);
            }
            else
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", "*");
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", "*");
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", "*");
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", "*");
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", "*");
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;

            //******************Count of rows in datagrid to be displayed in Label 7********************//
            chCount = dataGridView1.Rows.Count;
            if (chCount == 0)
            {
                label7.Text = " 0 Channels found";
            }
            else if (chCount == 2)
            {
                label7.Text = (chCount - 1).ToString() + " Channel found";
            }
            else
            {
                label7.Text = (chCount - 1).ToString() + " Channels found";
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", textBox4.Text);
            }
            else
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", "*");
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", "*");
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", "*");
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", "*");
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", "*");
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;

            //******************Count of rows in datagrid to be displayed in Label 7********************//

            chCount = dataGridView1.Rows.Count;
            if (chCount == 0)
            {
                label7.Text = " 0 Channels found";
            }
            else if (chCount == 2)
            {
                label7.Text = (chCount - 1).ToString() + " Channel found";
            }
            else
            {
                label7.Text = (chCount - 1).ToString() + " Channels found";
            }
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != "")
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", textBox4.Text);
            }
            else
            {
                SearchQuery = string.Format("Service LIKE '%{0}%'", "*");
            }
            if (textBox3.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", textBox3.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelName LIKE '%{0}%'", "*");
            }
            if (textBox5.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", textBox5.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ActivationState LIKE '%{0}%'", "*");
            }
            if (textBox6.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", textBox6.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ChannelState LIKE '%{0}%'", "*");
            }
            if (textBox12.Text != "")
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", textBox12.Text);
            }
            else
            {
                SearchQuery = SearchQuery + string.Format("AND ShortLog LIKE '%{0}%'", "*");
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = SearchQuery;

            //******************Count of rows in datagrid to be displayed in Label 7********************//

            chCount = dataGridView1.Rows.Count;
            if (chCount == 0)
            {
                label7.Text = " 0 Channels found";
            }
            else if (chCount == 2)
            {
                label7.Text = (chCount - 1).ToString() + " Channel found";
            }
            else
            {
                label7.Text = (chCount - 1).ToString() + " Channels found";
            }
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

                //Checking if SID already configured
                int flag = 0;
                String compare = "";
                compare = textBox13.Text.ToUpper();

                if ( compare == SAPPIChannelMonitor.Properties.Settings.Default["sid1Name"].ToString())
                {
                    flag++;
                }
                if (compare == SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"].ToString())
                {
                    flag++;
                }
                if (compare == SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"].ToString())
                {
                    flag++;
                }
                if (compare == SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"].ToString())
                {
                    flag++;
                }
                if (compare == SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"].ToString())
                {
                    flag++;
                }

                //If SID is unique, saving it to the application settings
                if (flag <= 0)
                {
                    url = "http://" + textBox1.Text + ":" + textBox2.Text + "/AdapterFramework/ChannelAdminServlet?party=" + textBox9.Text + "&service=" + textBox8.Text + "&channel=*&action=status&showProcessLog=true&j_user=" + textBox14.Text + "&j_password=" + textBox15.Text;
                    if (SAPPIChannelMonitor.Properties.Settings.Default["sid1"].ToString() == "")
                    {
                        SAPPIChannelMonitor.Properties.Settings.Default["sid1"] = url;
                        SAPPIChannelMonitor.Properties.Settings.Default["sid1Name"] = textBox13.Text.ToUpper();
                        SAPPIChannelMonitor.Properties.Settings.Default.Save();
                        MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.", "Success!");
                    }
                    else if (SAPPIChannelMonitor.Properties.Settings.Default["sid2"].ToString() == "")
                    {
                        SAPPIChannelMonitor.Properties.Settings.Default["sid2"] = url;
                        SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"] = textBox13.Text.ToUpper();
                        SAPPIChannelMonitor.Properties.Settings.Default.Save();
                        MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.", "Success!");
                    }
                    else if (SAPPIChannelMonitor.Properties.Settings.Default["sid3"].ToString() == "")
                    {
                        SAPPIChannelMonitor.Properties.Settings.Default["sid3"] = url;
                        SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"] = textBox13.Text.ToUpper();
                        SAPPIChannelMonitor.Properties.Settings.Default.Save();
                        MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.", "Success!");
                    }
                    else if (SAPPIChannelMonitor.Properties.Settings.Default["sid4"].ToString() == "")
                    {
                        SAPPIChannelMonitor.Properties.Settings.Default["sid4"] = url;
                        SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"] = textBox13.Text.ToUpper();
                        SAPPIChannelMonitor.Properties.Settings.Default.Save();
                        MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.", "Success!");
                    }
                    else if (SAPPIChannelMonitor.Properties.Settings.Default["sid5"].ToString() == "")
                    {
                        SAPPIChannelMonitor.Properties.Settings.Default["sid5"] = url;
                        SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"] = textBox13.Text.ToUpper();
                        SAPPIChannelMonitor.Properties.Settings.Default.Save();
                        MessageBox.Show(textBox13.Text.ToUpper() + " system added successfully. Now select it from the dropdown and save your selection.", "Success!");
                    }
                    else
                    {
                        MessageBox.Show("You can only add 5 systems. Please delete one to continue.","Error");
                    }
                }
                else
                {
                    MessageBox.Show("SID name already configured. Please choose a different SID name","Error");
                }
            }  
            else
            {
                MessageBox.Show("Either of SID, Hostname, Port, Username or Password is missing!","Error");
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
                    SAPPIChannelMonitor.Properties.Settings.Default.Save();
                    MessageBox.Show(temp + " system deleted","Success!");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid2"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid2Name"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default.Save();
                    MessageBox.Show(temp + " system deleted", "Success!");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid3"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid3Name"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default.Save();
                    MessageBox.Show(temp + " system deleted", "Success!");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid4"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid4Name"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default.Save();
                    MessageBox.Show(temp + " system deleted", "Success!");
                }
                else if (SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"].ToString() == temp)
                {
                    SAPPIChannelMonitor.Properties.Settings.Default["sid5"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default["sid5Name"] = "";
                    SAPPIChannelMonitor.Properties.Settings.Default.Save();
                    MessageBox.Show(temp + " system deleted", "Success!");
                }
            }
            else
            {
                MessageBox.Show("Select a valid system to delete.","Error");
            }
            this.Form1_Load(this, null);
            comboBox1.Text = "Select System";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //System selection notifier
            if(comboBox1.Text.ToString() != "Empty" && comboBox1.Text.ToString() != "Select System")
            { 
            MessageBox.Show(comboBox1.Text.ToString() + " System selection saved. Now you can run the Channel Monitor.","Success!");
            }
            else
            {
                MessageBox.Show("Select a valid system to save.","Error");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            //Load Help form
            Form2 f2 = new Form2();
            f2.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Create a new instance of FolderBrowserDialog.

            FolderBrowserDialog folderBrowserDlg = new FolderBrowserDialog();

            // A new folder button will display in FolderBrowserDialog.

            folderBrowserDlg.ShowNewFolderButton = true;

            //Show FolderBrowserDialog

            DialogResult dlgResult = folderBrowserDlg.ShowDialog();


            if (dlgResult.Equals(DialogResult.OK))

            {

                //Show selected folder path in textbox7.

                textBox7.Text = folderBrowserDlg.SelectedPath;

                //Browsing start from root folder.

                Environment.SpecialFolder rootFolder = folderBrowserDlg.RootFolder;

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SAPPIChannelMonitor.Properties.Settings.Default["workspace"] = textBox7.Text;
            SAPPIChannelMonitor.Properties.Settings.Default.Save();
            MessageBox.Show("Workspace location saved successfully.", "Success!");
        }
    }
}
