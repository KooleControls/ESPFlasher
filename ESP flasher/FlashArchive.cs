using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using STDLib.Misc;
using FRMLib;
using ESPTool.Firmware;
using ESPTool;

namespace ESP_flasher
{
    public partial class FlashArchive : UserControl
    {
        ESPTool.ESPTool espTool = new ESPTool.ESPTool();
        readonly ThreadedBindingList<int> supportedBaud = new ThreadedBindingList<int> { 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
        readonly int defaultBaud = 9;
        public string ArchiveFilter { get; set; }
        public string TempFolder { get; set; }

        private readonly ThreadedBindingList<BinFile> files = new ThreadedBindingList<BinFile>();

        public FlashArchive()
        {
            InitializeComponent();
        }

        ~FlashArchive()
        {
            TempFiles.RemoveTempFolder(TempFolder);
        }

        private void FlashArchive_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = supportedBaud;
            comboBox1.SelectedIndex = defaultBaud;

            RefreshComList();
        }

        private void btn_RefreshCOM_Click(object sender, EventArgs e)
        {
            RefreshComList();
        }


        void RefreshComList()
        {
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(SerialPort.GetPortNames());
            if (comboBox2.Items.Count > 0)
                comboBox2.SelectedIndex = 0;
        }

        private void btn_Browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog
            {
                Filter = ArchiveFilter
            };
            if (diag.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = diag.FileName;
                OpenFile(diag.FileName);
            }
            diag.Dispose();
        }

        public void SetCom_Port(string portname)
        {
            comboBox2.Text = portname;
        }

        public void SetCom_Baud(string baud)
        {
            comboBox1.Text = baud;
        }

        public void OpenFile(string filename)
        {
            textBox1.Text = filename;

            string tempfolder = TempFiles.PrepTempFolder(TempFolder);
            List<BinFile> temp;
            try
            {
                ZipFile.ExtractToDirectory(filename, tempfolder);
                using (StreamReader reader = new StreamReader(Path.Combine(tempfolder, "Settings.json")))
                    temp = JsonConvert.DeserializeObject<List<BinFile>>(reader.ReadToEnd());

                files.Clear();
                foreach (BinFile b in temp.OrderBy(c => c.Address))
                {
                    b.File = Path.Combine(tempfolder, b.File);
                    files.Add(b);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void btn_Program_Click(object sender, EventArgs e)
        {
            Program();
        }

        private void btn_Erase_Click(object sender, EventArgs e)
        {
            Erase();
        }

        public async void Program()
        {
            FirmwareImage fi = new FirmwareImage();

            foreach(var file in files)
            {
                Segment segment = new Segment();
                segment.Offset = (UInt32)file.Address;
                segment.Data = File.ReadAllBytes(file.File);
                fi.Segments.Add(segment);
            }

            string com = comboBox2.Text;
            int baud = int.Parse(comboBox1.Text);

            await espTool.FlashFirmware(com, baud, Progress, fi);
        }


        void Progress(ProgressReport report)
        {
            progressBar1.InvokeIfRequired(()=>progressBar1.Value = (int)(report.Progress * 100));
            if(report.Message!= null)
            {
                richTextBox1.InvokeIfRequired(() => richTextBox1.Text += report.Message + "\r\n");
            }
        }


        public void Erase()
        {
            throw new NotImplementedException();
        }


        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }

}
