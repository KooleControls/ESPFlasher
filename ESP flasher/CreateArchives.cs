using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO.Compression;
using STDLib.Misc;

namespace ESP_flasher
{
    public partial class CreateArchives : UserControl
    {
        public readonly BindingList<PartitionEntry> partitionTable = new BindingList<PartitionEntry>();
        public readonly ThreadedBindingList<BinFile> files = new ThreadedBindingList<BinFile>();
        public readonly ThreadedBindingList<string> extraFiles = new ThreadedBindingList<string>();

        string[] partitionFilenameOrder = new string[] { "partition-table.bin" };
        public string TempFolder { get; set; }
        public string ArchiveFilter { get; set; }
        public string HexFilter { get; set; }
        

        public CreateArchives()
        {
            InitializeComponent();
        }

        private void CreateArchives_Load(object sender, EventArgs e)
        {
            listBox1.DataSource = files;
            listBox2.DataSource = partitionTable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            if (diag.ShowDialog() == DialogResult.OK)
            {
                OpenFlash_project_args(diag.FileName);
            }
        }


        public void OpenFlash_project_args(string file)
        {
            files.Clear();
            using (StreamReader rdr = new StreamReader(file))
            {
                while(!rdr.EndOfStream)
                {
                    string line = rdr.ReadLine();

                    Match m = Regex.Match(line, "(0x[0-9a-fA-F]+) (.+)");
                    if(m.Success)
                    {
                        AddBinFile(Path.Combine(Path.GetDirectoryName(file), m.Groups[2].Value.Replace('/', '\\')) , m.Groups[1].Value);
                    }
                }
            }

            //Select what should be converted to hex.
            foreach (PartitionEntry pi in partitionTable.Where(p => p.Type == 0))
            {
                foreach (BinFile bf in files.Where(f => f.Address == pi.Address))
                {
                    listBox1.SelectedItem = bf;

                    string elfFile = Path.ChangeExtension(bf.File, "elf");
                    if(File.Exists(elfFile))
                    {
                        extraFiles.Add(elfFile);
                    }
                }
            }
        }

        void AddBinFile(string binfile, string addr)
        {
            int intValue = Convert.ToInt32(addr, 16);
            files.Add(new BinFile { Address = intValue, File = binfile });
            foreach (string pfn in partitionFilenameOrder)
            {
                string fname = Path.GetFileName(binfile);
                if (Regex.IsMatch(fname, pfn))
                    OpenPartitionBIN(binfile);
            }
        }

        void OpenPartitionBIN(string binFile)
        {
            partitionTable.Clear();
            using (Stream s = File.Open(binFile, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[0x20];
                s.Read(data, 0, data.Length);
                while (data[0] != 0xFF)
                {
                    if (data[0] == 0xAA)
                    {
                        PartitionEntry pe = new PartitionEntry(data);
                        partitionTable.Add(pe);
                    }
                    s.Read(data, 0, data.Length);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(files.Count() > 0)
                propertyGrid1.SelectedObject = files[listBox1.SelectedIndex];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            diag.Filter = ArchiveFilter;
            if(diag.ShowDialog() == DialogResult.OK)
            {
                CreateZip(diag.FileName);
            }

        }

        public void CreateZip(string zipFile)
        {
            string tempFolder = TempFiles.PrepTempFolder(TempFolder);


            //using (StreamWriter sw = new StreamWriter(Path.Combine(tempFolder, "")))

            List<BinFile> files2 = new List<BinFile>();

            //Copy all files to temp folder
            foreach (BinFile bf in files)
            {
                string destFile = Path.Combine(tempFolder, Path.GetFileName(bf.File));
                files2.Add(new BinFile { Address = bf.Address, File = Path.GetFileName(bf.File) });
                File.Copy(bf.File, destFile);
            }

            foreach(string file in extraFiles)
            {
                string destFile = Path.Combine(Path.Combine(tempFolder, "DEV"), Path.GetFileName(file));
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                File.Copy(file, destFile);
            }

            //Save list of files and addresses.
            using (StreamWriter sw = new StreamWriter(Path.Combine(tempFolder, "Settings.json")))
            {
                sw.Write(JsonConvert.SerializeObject(files2, Formatting.Indented));
            }

            if (File.Exists(zipFile))
                File.Delete(zipFile);
            ZipFile.CreateFromDirectory(tempFolder, zipFile);

            TempFiles.RemoveTempFolder(tempFolder);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog
            {
                Filter = "Binary file|*.bin"
            };
            if (diag.ShowDialog() == DialogResult.OK)
            {
                AddBinFile(diag.FileName, "0x0");
            }
            diag.Dispose();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                files.RemoveAt(listBox1.SelectedIndex);
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            toolTip1.SetToolTip(sender as Control, "Try add all files automatically. \r\nSelect the following file: 'ProjectFolder\\Build\\flash_project_args'");
        }

        private void button5_Click(object sender, EventArgs e)
        {

            /*
            Release rel = new Release();
            rel.Parent = this;
            rel.ShowDialog();
            */
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedItem is BinFile file)
            {
                SaveFileDialog diag = new SaveFileDialog();
                diag.Filter = HexFilter;
                diag.OverwritePrompt = true;
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    CreateHex(diag.FileName);
                }
            }
        }

        public void CreateHex(string dest)
        {
            if (listBox1.SelectedItem is BinFile file)
            {
                using (StreamWriter stream = new StreamWriter(File.Open(dest, FileMode.Create, FileAccess.Write)))
                    stream.Write(I32HEX.ToHex(file.File));
            }
        }

        public bool BinFound()
        {
            return (listBox1.SelectedItem is BinFile);
        }


    }
}
