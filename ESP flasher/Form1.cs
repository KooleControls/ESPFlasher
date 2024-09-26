using ESP_Flasher.Logging;
using ESP_Flasher.Models;
using ESP_Flasher.Services;
using ESP_Flasher.UIBinders;
using FRMLib;
using Microsoft.Extensions.DependencyInjection;
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
                MessageBox.Show("Failed to load archive.");
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
            UiEnabled(false);
            await _flashingService.EraseFlashAsync();
            UiEnabled(true);
        }

        private async void buttonProgram_Click(object sender, EventArgs e)
        {
            if(openArchive == null)
                return;
            UiEnabled(false);
            _flashingService.UseCompression = checkBoxCompression.Checked;
            _flashingService.SerialPort = _serialPortBinder.SelectedSerialPortName;
            _flashingService.BaudRate = _serialPortBinder.SelectedBaudRate;
            await _flashingService.FlashAsync(openArchive, default, _progressBarBinder.Bind());
            UiEnabled(true);
        }

        private void UiEnabled(bool enabled)
        {
            buttonErase.Enabled = enabled;
            buttonProgram.Enabled = enabled;
            buttonRefresh.Enabled = enabled;
            checkBoxCompression.Enabled = enabled;
        }


    }

}

