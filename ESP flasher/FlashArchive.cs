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
using System.Threading;

namespace ESP_flasher
{
    public partial class FlashArchive : UserControl
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
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
            cmb_Baudrate.DataSource = supportedBaud;
            cmb_Baudrate.SelectedIndex = defaultBaud;

            RefreshComList();
            SetControlsEnabled(true);
        }

        private void btn_RefreshCOM_Click(object sender, EventArgs e)
        {
            RefreshComList();
        }


        void RefreshComList()
        {
            cmb_Comname.Items.Clear();
            cmb_Comname.Items.AddRange(SerialPort.GetPortNames());
            if (cmb_Comname.Items.Count > 0)
                cmb_Comname.SelectedIndex = 0;
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
            cmb_Comname.Text = portname;
        }

        public void SetCom_Baud(string baud)
        {
            cmb_Baudrate.Text = baud;
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

        
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
        }


        public async void Program()
        {
            cancellationTokenSource = new CancellationTokenSource();
            SetControlsEnabled(false);
            FirmwareImage fi = new FirmwareImage();

            foreach (var file in files)
            {
                Segment segment = new Segment();
                segment.Offset = (UInt32)file.Address;
                segment.Data = File.ReadAllBytes(file.File);
                fi.Segments.Add(segment);
            }

            string com = cmb_Comname.Text;
            int baud = int.Parse(cmb_Baudrate.Text);
            Progress<float> progress = new Progress<float>((e) => progressBar1.InvokeIfRequired(() => progressBar1.Value = (int)(e * 100)));
            Result result = await espTool.FlashFirmware(com, baud, fi, false, cancellationTokenSource.Token);
            richTextBox1.AppendText($"Flashing firmware {(result.Success?"oke":"failed")}.");
            SetControlsEnabled(true);
        }

        public async void Erase()
        {
            cancellationTokenSource = new CancellationTokenSource();
            SetControlsEnabled(false);
            string com = cmb_Comname.Text;
            int baud = int.Parse(cmb_Baudrate.Text);
            Result result = await espTool.Erase(com, baud, cancellationTokenSource.Token);
            richTextBox1.AppendText($"Flashing firmware {(result.Success ? "oke" : "failed")}.");
            SetControlsEnabled(true);
        }

        void SetControlsEnabled(bool enabled)
        {
            cmb_Baudrate.Enabled = enabled;
            cmb_Comname.Enabled = enabled;
            btn_Browse.Enabled = enabled;
            btn_Erase.Enabled = enabled;
            btn_Program.Enabled = enabled;
            btn_Refresh.Enabled = enabled;
            btn_Cancel.Enabled = !enabled;
            progressBar1.Value = 0;
        }

    }

}
