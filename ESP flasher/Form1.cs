using ESP_Flasher.Logging;
using ESP_Flasher.Models;
using ESP_Flasher.Services;
using ESP_Flasher.UIBinders;
using FRMLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace ESP_Flasher
{
    public partial class Form1 : Form
    {
        // Constants
        private readonly string archiveFileFilter = "Firmware archive|*.kczip";
        private readonly string hexFileFilter = "Binary file|*.bin";
        private readonly string buildArgsFileFilter = "flash_project_args|flash_project_args";

        // Services
        private readonly ArchiveService _archiveService;
        private readonly DeviceService _flashingService;

        // Binders
        private readonly SerialPortBinder _serialPortBinder;
        private readonly ArchiveListViewBinder _archiveBinder;
        private readonly PartitionTableListViewBinder _partitionBinder;
        private readonly RichTextBoxLoggerFactory _richTextBoxLoggerFactory;
        private readonly ProgressBarBinder _progressBarBinder;

        // Variables
        FirmwareArchive openArchive;
        CancellationTokenSource? cancelButtonSource;
        ILogger<Form1> logger;

        public Form1()
        {
            InitializeComponent();

            // Create the RichTextBoxLoggerFactory
            _richTextBoxLoggerFactory = new RichTextBoxLoggerFactory(richTextBox1);

            // Instantiate services with the logger factory
            _archiveService = new ArchiveService(_richTextBoxLoggerFactory);
            _flashingService = new DeviceService(_archiveService, _richTextBoxLoggerFactory);

            // Bind the UI elements to data
            _serialPortBinder = new SerialPortBinder(comboBoxSerialPort, comboBoxBaudRate);
            _archiveBinder = new ArchiveListViewBinder(listViewHexFiles);
            _partitionBinder = new PartitionTableListViewBinder(listViewPartitionTable);
            _progressBarBinder = new ProgressBarBinder(progressBar1);

            // Set variables;
            logger = _richTextBoxLoggerFactory.CreateLogger<Form1>();
            openArchive = new FirmwareArchive();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStrip1.AddMenuItem("File/New", NewArchive);
            toolStrip1.AddMenuItem("File/Open Archive", OpenArchiveDialog);
            //toolStrip1.AddMenuItem("File/Open Build folder", CreateArchiveFromBuild);
            //toolStrip1.AddMenuItem("File/Save as/Archive", SaveArchive);
            //toolStrip1.AddMenuItem("File/Save as/Hex", SaveArchiveHex);
            toolStrip1.AddMenuItem("File/Exit", Close);

        }

        private void NewArchive()
        {
            openArchive = new FirmwareArchive();
            _archiveBinder.Populate(openArchive);
            _partitionBinder.Populate(openArchive);
        }

        private async void OpenArchiveDialog()
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = archiveFileFilter;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            
            openArchive = await _archiveService.LoadFromZip(dialog.FileName) ?? openArchive;
            _archiveBinder.Populate(openArchive);
            _partitionBinder.Populate(openArchive);
        }

        private async void CreateArchiveFromBuild()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = buildArgsFileFilter;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            openArchive = await _archiveService.LoadFromBuildDirectory(dialog.FileName) ?? openArchive;
            _archiveBinder.Populate(openArchive);
            _partitionBinder.Populate(openArchive);
        }

        private async void SaveArchive()
        {
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = archiveFileFilter;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            using Stream stream = dialog.OpenFile();
            await _archiveService.SaveArchive(stream, openArchive);
        }

        private async void SaveArchiveHex()
        {
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = hexFileFilter;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            using Stream stream = dialog.OpenFile();
            await _archiveService.SaveArchiveHex(stream, openArchive);
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            _serialPortBinder?.LoadSerialPorts();
        }

        private async void buttonErase_Click(object sender, EventArgs e)
        {
            try
            {
                UiEnabled(false);
                cancelButtonSource = new CancellationTokenSource();
                await _flashingService.EraseFlashAsync(cancelButtonSource.Token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to erase flash");
            }
            finally
            {
                UiEnabled(true);
            }
        }

        private async void buttonProgram_Click(object sender, EventArgs e)
        {
            if (openArchive == null)
            {
                logger.LogError($"No archive opened");
                return;
            }
                
            try
            {
                UiEnabled(false);
                cancelButtonSource = new CancellationTokenSource();
                _flashingService.UseCompression = checkBoxCompression.Checked;
                _flashingService.SerialPort = _serialPortBinder.SelectedSerialPortName;
                _flashingService.BaudRate = _serialPortBinder.SelectedBaudRate;
                await _flashingService.FlashAsync(openArchive, cancelButtonSource.Token, _progressBarBinder.Bind());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to upload flash");
            }
            finally
            {
                _progressBarBinder.Bind().Report(0);
                UiEnabled(true);
            }
        }

        private void UiEnabled(bool enabled)
        {
            groupBoxArchive.Enabled = enabled;
            groupBoxSerial.Enabled = enabled;   
            groupBoxLog.Enabled = enabled;
            groupBoxActions.Enabled = enabled;

            groupBoxProgress.Enabled = !enabled;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            cancelButtonSource?.Cancel();
        }
    }

}

