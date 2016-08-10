using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace SNBtoTXT
{
    public partial class SNBtoTXTConverter : Form
    {
        public SNBtoTXTConverter()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            itemsList.AllowDrop = true;
            itemsList.DragDrop += itemsList_DragDrop;
            itemsList.DragEnter += itemsList_DragEnter;
        }

        private void itemsList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void itemsList_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (!string.IsNullOrEmpty(Path.GetExtension(file)))
                    {
                        itemsList.Items.Add(file);
                    }
                }

                stLbl.Text = string.Format("{0} item(s) to convert.", itemsList.Items.Count);

                if (itemsList.Items.Count > 0)
                {
                    btnStart.Enabled = true;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occured. \r\nError information: " + exc.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Convert();
        }

        private void Convert()
        {
            try
            {
                string result = fbd.ShowDialog().ToString();
                if (result == "OK")
                {
                    foreach (string item in itemsList.Items)
                    {
                        using (ZipFile zip = ZipFile.Read(item))
                        {
                            ZipEntry ee = zip["snote\\snote.xml"];
                            List<string> xmlContents = new List<string>();
                            using (var ms = new MemoryStream())
                            {
                                ee.Extract(ms);
                                ms.Position = 0;
                                var sr = new StreamReader(ms);
                                var myStr = sr.ReadToEnd();
                                xmlContents.Add(myStr);
                            }

                            for (int i = 0; i < xmlContents.Count; i++)
                            {
                                string xmlResult = "";
                                using (XmlReader reader = XmlReader.Create(new StringReader(xmlContents[i])))
                                {
                                    XmlWriterSettings ws = new XmlWriterSettings();
                                    ws.Indent = true;
                                    reader.ReadToFollowing("sn:t");
                                    xmlResult = reader.ReadElementContentAsString().Replace("\n", "\r\n");
                                    StreamWriter sw = new StreamWriter(new FileStream(fbd.SelectedPath + "\\" + Path.GetFileNameWithoutExtension(item) + ".txt", FileMode.Create), Encoding.UTF8);
                                    sw.Write(xmlResult);
                                    sw.Close();
                                }
                            }
                        }
                    }
                    stLbl.Text = string.Format("{0} item(s) converted successfuly.", itemsList.Items.Count);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occured. \r\nError information: " + exc.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Samsung® Notes .snb to .txt Converter, made by: Mustafa (@MuStAPro).\r\nSamsung® is a trademark for Samsung Electronics Limited.", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void itemsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (itemsList.Items.Count > 1)
                {
                    int tmpIdx = itemsList.SelectedIndex;
                    itemsList.Items.RemoveAt(itemsList.SelectedIndex);
                    if (tmpIdx + 1 > itemsList.Items.Count)
                    {
                        itemsList.SelectedIndex = itemsList.Items.Count - 1;
                    }
                    else
                    {
                        itemsList.SelectedIndex = tmpIdx;
                    }
                }
                else
                {
                    itemsList.Items.RemoveAt(itemsList.SelectedIndex);
                }

                stLbl.Text = "Ready.";

                if (itemsList.Items.Count == 0)
                {
                    btnStart.Enabled = false;
                }
            }
        }
    }
}
