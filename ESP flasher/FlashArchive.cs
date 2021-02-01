using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BasPriveLIB;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using BasPriveLIB;
using System.Text.RegularExpressions;

namespace ESP_flasher
{
    public partial class FlashArchive : UserControl
    {
        readonly ThreadedBindingList<int> supportedBaud = new ThreadedBindingList<int> { 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
        readonly int defaultBaud = 9;
        public string ESPToolExe { get; set; }
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

        public void Program()
        {
            string aha = "";

            foreach (BinFile file in files)
                aha += " 0x" + file.Address.ToString("X") + " \"" + file.File + "\"";

            string arguments = string.Format("--port {0} --baud {1} write_flash{2}", comboBox2.Text, comboBox1.Text, aha);

            StartFlasher(arguments);
        }

        public void Erase()
        {
            string arguments = string.Format("--port {0} --baud {1} erase_flash", comboBox2.Text, comboBox1.Text);

            StartFlasher(arguments);
        }






        Stopwatch flashDurationTimer = new Stopwatch();
        Process espTool = new Process();
        void StartFlasher(string arguments)
        {
            this.Enabled = false;
            richTextBox1.Text = arguments + "\r\n\r\n";

            espTool = new Process();
            espTool.StartInfo.FileName = ESPToolExe;
            espTool.OutputDataReceived += EspTool_OutputDataReceived;
            espTool.Exited += EspTool_Exited;
            espTool.ErrorDataReceived += EspTool_ErrorDataReceived;

            espTool.StartInfo.Arguments = arguments;
            espTool.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            espTool.EnableRaisingEvents = true;
            espTool.StartInfo.UseShellExecute = false;
            espTool.StartInfo.RedirectStandardOutput = true;
            espTool.StartInfo.RedirectStandardError = true;
            espTool.StartInfo.UseShellExecute = false;
            espTool.StartInfo.CreateNoWindow = true;
            flashDurationTimer.Reset();
            espTool.Start();
            flashDurationTimer.Start();

            espTool.BeginOutputReadLine();
            espTool.BeginErrorReadLine();
        }


        private void EspTool_Exited(object sender, EventArgs e)
        {
            flashDurationTimer.Stop();
            label3.InvokeIfRequired(x => x.Text = flashDurationTimer.Elapsed.TotalSeconds.ToString("0.##") + 's');
            this.InvokeIfRequired(x => x.Enabled = true);
            progressBar1.InvokeIfRequired(x => x.Value = 0);
            AddLogRow("Programmer exited");
        }

        private void EspTool_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            AddLogRow(e.Data);
        }

        private void EspTool_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            Match m = Regex.Match(e.Data, @"\((\d+) ?%\)");

            if (m.Success)
            {
                int progress;
                if (int.TryParse(m.Groups[1].Value, out progress))
                {
                    if (progress != 100)
                        progressBar1.InvokeIfRequired(x => x.Value = progress);
                }
            }

            AddLogRow(e.Data);
        }


        void AddLogRow(string text)
        {
            richTextBox1.InvokeIfRequired(x => x.Text += text + "\r\n");
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }

}
