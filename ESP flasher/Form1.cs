using ESP_Flasher.Logging;
using ESP_Flasher.Models;
using ESP_Flasher.Services;
using ESP_Flasher.UIBinders;
using FRMLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Ports;
using System.Text;

namespace ESP_Flasher
{
    public partial class Form1 : Form
    {
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
        FirmwareArchive? openArchive;
        CancellationTokenSource? cancelButtonSource;
        ILogger<Form1> logger;

        public Form1()
        {
            InitializeComponent();

            // Create the RichTextBoxLoggerFactory
            _richTextBoxLoggerFactory = new RichTextBoxLoggerFactory(richTextBox1);

            // Instantiate services with the logger factory
            _archiveService = new ArchiveService();
            _flashingService = new DeviceService(_archiveService, _richTextBoxLoggerFactory);

            // Bind the UI elements to data
            _serialPortBinder = new SerialPortBinder(comboBoxSerialPort, comboBoxBaudRate);
            _archiveBinder = new ArchiveListViewBinder(listViewHexFiles);
            _partitionBinder = new PartitionTableListViewBinder(listViewPartitionTable);
            _progressBarBinder = new ProgressBarBinder(progressBar1);

            // Set variables;
            logger = _richTextBoxLoggerFactory.CreateLogger<Form1>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStrip1.AddMenuItem("File/Open", OpenArchiveDialog);
        }

        // Opens the archive file dialog and populates the data using binders
        private void OpenArchiveDialog()
        {
            openArchive = _archiveService.LoadArchive();

            if (openArchive == null)
            {
                logger.LogError("Failed to load archive.");
                return;
            }

            // Use binders to populate UI elements
            _archiveBinder.Populate(openArchive.Entries);

            if (openArchive.PartitionTable != null)
            {
                _partitionBinder.Populate(openArchive.PartitionTable);
            }
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
                logger.LogError($"Failed to erase flash {ex.Message}");
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
                logger.LogError($"Failed to erase flash {ex.Message}");
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

