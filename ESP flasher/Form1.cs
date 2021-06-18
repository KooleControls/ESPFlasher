using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Globalization;
using CmdArgParser;
using ESP_flasher.Properties;

namespace ESP_flasher
{
    public partial class Form1 : Form
    {
        readonly string archiveFilter = "Binary file|*.kczip";
        readonly string hexFilter = "Intel hex file|*.hex";

        readonly string tempFolderDevelopment = Path.Combine(Path.GetTempPath(), "Koole controls", "ESP flasher", "tempCreate");
        readonly string tempFolderFlash = Path.Combine(Path.GetTempPath(), "Koole controls", "ESP flasher", "tempFlash");



        public Form1()
        {
            InitializeComponent();
            flashArchive1.ArchiveFilter = archiveFilter;
            flashArchive1.TempFolder = tempFolderFlash;
            createArchives1.ArchiveFilter = archiveFilter;
            createArchives1.HexFilter = hexFilter;
            createArchives1.TempFolder = tempFolderDevelopment;
        }

        CommandlineOptions opts;

        private void Form1_Load(object sender, EventArgs e)
        {
            Settings.Load(true);

            Version vers = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text += $" V{vers.Major.ToString("D2")}.{vers.Minor.ToString("D2")}.{vers.Build.ToString("D2")}";

            CommandlineParser.ShowHelp<CommandlineOptions>();
            opts = CommandlineParser.Parse<CommandlineOptions>();


            if (opts.Baudrate != null)
                flashArchive1.SetCom_Baud(opts.Baudrate);

            if (opts.Port != null)
                flashArchive1.SetCom_Port(opts.Port);

            if (opts.File != null)
                flashArchive1.OpenFile(opts.File);

            if (opts.Erase)
                flashArchive1.Erase();

            if (opts.Flash)
                flashArchive1.Program();


            if(Settings.AutoReleaseOnStart)
            {
                tabControl1.SelectedIndex = 1;
                if (File.Exists(Settings.PathToFlashProjectArgs))
                {
                    createArchives1.OpenFlash_project_args(Settings.PathToFlashProjectArgs);

                    if (File.Exists(Settings.PathToVersionC))
                    {
                        using (StreamReader reader = new StreamReader(Settings.PathToVersionC))
                        {
                            string s = reader.ReadToEnd();
                            Match m = Regex.Match(s, Settings.VersionRegex);

                            if(m.Success)
                            {
                                Directory.CreateDirectory(Settings.DefaultReleaseFolder);
                                string version = $"{m.Groups[1].Value}_{m.Groups[2].Value}_{m.Groups[3].Value}";
                                string releaseDir = Path.Combine(Settings.DefaultReleaseFolder, version);

                                if(!Directory.Exists(releaseDir))
                                {
                                    if(createArchives1.BinFound())
                                    {
                                        Directory.CreateDirectory(releaseDir);

                                        string hex = Path.Combine(releaseDir, Settings.HexFileName.Replace("{version}", version));
                                        string kczip = Path.Combine(releaseDir, Settings.KCZipFileName.Replace("{version}", version));

                                        createArchives1.CreateHex(hex);
                                        createArchives1.CreateZip(kczip);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

}


