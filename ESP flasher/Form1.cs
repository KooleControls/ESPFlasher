using ESP_Flasher.Logging;
using ESP_Flasher.Models;
using ESP_Flasher.Services;
using ESP_Flasher.UIBinders;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Reflection;

namespace ESP_Flasher
{
    public partial class Form1 : Form
    {
        // Constants
        private readonly string archiveFileFilter = "Firmware archive|*.kczip";
        private readonly string hexFileFilter = "Intel hex file|*.hex";
        private readonly string binFileFilter = "Binary file|*.bin";
        private readonly string buildArgsFileFilter = "flash_project_args|flash_project_args";

        // Services
        private readonly ArchiveService _archiveService;
        private readonly DeviceService _deviceService;

        // UI Binders
        private readonly SerialPortBinder _serialPortBinder;
        private readonly ArchiveListViewBinder _archiveBinder;
        private readonly PartitionTableListViewBinder _partitionBinder;
        private readonly RichTextBoxLoggerFactory _richTextBoxLoggerFactory;
        private readonly ProgressBarBinder _progressBarBinder;
        private readonly AppHeaderListViewBinder _appHeaderListViewBinder;

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
            _deviceService = new DeviceService(_richTextBoxLoggerFactory.CreateLogger<DeviceService>());

            // Bind the UI elements to data
            _serialPortBinder = new SerialPortBinder(comboBoxSerialPort, comboBoxBaudRate);
            _archiveBinder = new ArchiveListViewBinder(listViewHexFiles);
            _partitionBinder = new PartitionTableListViewBinder(listViewPartitionTable, _archiveService);
            _progressBarBinder = new ProgressBarBinder(progressBar1);
            _appHeaderListViewBinder = new AppHeaderListViewBinder(listViewAppHeader, _archiveService);

            // Set variables;
            logger = _richTextBoxLoggerFactory.CreateLogger<Form1>();
            openArchive = new FirmwareArchive();

            listViewPartitionTable.ContextMenuStrip = contextMenuStripPartitionTable;

            UpdateTitle();
            _ = DoVersionCheck();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // File Operations
            mainToolStrip.AddMenuItem("File/New").WithAction(NewArchive);
            mainToolStrip.AddMenuItem("File/Open Archive").WithAction(OpenArchiveDialog);
            mainToolStrip.AddMenuItem("File/Save as/Archive").WithAction(SaveArchive).WithToolTip("Creates KCZIP file");
            mainToolStrip.AddMenuItem("File/Save as/Application intel hex").WithAction(SaveApplicationIntelHex).WithToolTip("Creates HEX file to be used in KC220 tool and LM");
            mainToolStrip.AddMenuItem("File/Save as/Archive intel hex").WithAction(SaveArchiveIntelHex).WithToolTip("Creates HEX file including all partitions");
            mainToolStrip.AddMenuItem("File/Exit").WithAction(Close);

            // Development Tools
            mainToolStrip.AddMenuItem("Development/Open Build folder").WithAction(LoadBuildDirectory);
            mainToolStrip.AddMenuItem("Development/Read device").WithAction(ReadDevice);


            // Partition Table
            contextMenuStripPartitionTable.AddMenuItem("Read selected to file").WithAction(ReadSelectedToFiles);
        }

        private async void ReadSelectedToFiles()
        {
            using var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Zip Archive (*.zip)|*.zip";
            saveDialog.FileName = "flash_backup.zip";

            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;

            var selectedPartitions = _partitionBinder.GetSelected().ToList();
            if (selectedPartitions.Count == 0)
                return;

            using var fileStream = new FileStream(saveDialog.FileName, FileMode.Create);
            using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);

            //await _deviceService.InitializeAsync();

            //var progressBar = _progressBarBinder.Bind();

            long totalBytes = selectedPartitions.Sum(p => (long)p.Size);
            long bytesReadSoFar = 0;

            foreach (var partitionEntry in selectedPartitions)
            {
                var entry = zipArchive.CreateEntry(partitionEntry.Name, CompressionLevel.Optimal);
                using var entryStream = entry.Open();

                long partitionBytesRead = 0;

                var progress = new Progress<float>(partitionProgress =>
                {
                    long newBytesRead = (long)(partitionProgress * partitionEntry.Size);
                    long delta = newBytesRead - partitionBytesRead;
                    partitionBytesRead = newBytesRead;

                    bytesReadSoFar += delta;
                    float totalProgress = (float)bytesReadSoFar / totalBytes;
                   // progressBar.Report(totalProgress);
                });


                

                //await _deviceService.ReadFlashAsync(
                //    entryStream,
                //    partitionEntry.Address,
                //    partitionEntry.Size,
                //    cancelButtonSource.Token,
                //    progress
                //);
            }

            //await _deviceService.ResetAndDisposeDevice();
        }




        private async void NewArchive()
        {
            openArchive = new FirmwareArchive();
            _archiveBinder.Populate(openArchive);
            await _partitionBinder.Populate(openArchive);
            await _appHeaderListViewBinder.Populate(openArchive);
        }

        private async void OpenArchiveDialog()
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = archiveFileFilter;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            openArchive = await _archiveService.LoadFromZip(dialog.FileName) ?? openArchive;
            _archiveBinder.Populate(openArchive);
            await _partitionBinder.Populate(openArchive);
            await _appHeaderListViewBinder.Populate(openArchive);
        }

        private async void LoadBuildDirectory()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = buildArgsFileFilter;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            openArchive = await _archiveService.LoadFromBuildDirectory(dialog.FileName) ?? openArchive;
            _archiveBinder.Populate(openArchive);
            await _partitionBinder.Populate(openArchive);
            await _appHeaderListViewBinder.Populate(openArchive);
        }

        private async void SaveArchive()
        {
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = archiveFileFilter;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            await _archiveService.SaveArchive(openArchive, dialog.FileName);
        }

        private async void SaveApplicationIntelHex()
        {
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = hexFileFilter;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            await _archiveService.SaveApplicationIntelHex(openArchive, dialog.FileName);
        }

        private async void SaveArchiveIntelHex()
        {
            using SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = hexFileFilter;

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            await _archiveService.SaveArchiveIntelHex(openArchive, dialog.FileName);
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
                //_richTextBoxLoggerFactory.Clear();
                //cancelButtonSource = new CancellationTokenSource();
                //_deviceService.UseCompression = checkBoxCompression.Checked;
                //_deviceService.SerialPort = _serialPortBinder.SelectedSerialPortName;
                //_deviceService.BaudRate = _serialPortBinder.SelectedBaudRate;
                //await _deviceService.EraseFlashAsync(cancelButtonSource.Token);
                //_deviceService.Dispose();
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
                _richTextBoxLoggerFactory.Clear();
                cancelButtonSource = new CancellationTokenSource();
                _deviceService.UseCompression = checkBoxCompression.Checked;
                _deviceService.SerialPort = _serialPortBinder.SelectedSerialPortName;
                _deviceService.BaudRate = _serialPortBinder.SelectedBaudRate;
                //await _deviceService.FlashAsync(openArchive, cancelButtonSource.Token, _progressBarBinder.Bind());
                _deviceService.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to upload flash");
            }
            finally
            {
                _progressBarBinder.Report(0);
                UiEnabled(true);
            }
        }

        private async void ReadDevice()
        {
            await RunWithDisabledControlsAsync(async token => 
            { 
                _deviceService.UseCompression = checkBoxCompression.Checked;
                _deviceService.SerialPort = _serialPortBinder.SelectedSerialPortName;
                _deviceService.BaudRate = _serialPortBinder.SelectedBaudRate;

                await _deviceService.InitializeAsync(token);
                Stream flashStream = _deviceService.GetReadFlashStream(0x8000, 0xC00);
                PartitionTableExtractor extractor = new PartitionTableExtractor(_richTextBoxLoggerFactory);
                var partitionTable = await extractor.ParsePartitionTableAsync(flashStream, token);
                _partitionBinder.Populate(partitionTable);
            });
        }
        

        private async Task RunWithDisabledControlsAsync(Func<CancellationToken, Task> task)
        {
            UiEnabled(false);
            try
            {
                _richTextBoxLoggerFactory.Clear();
                cancelButtonSource = new CancellationTokenSource();
                await task(cancelButtonSource.Token);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Task cancelled");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the task.");
            }
            finally
            {
                _progressBarBinder.Report(0);
                UiEnabled(true);
                _deviceService.Dispose();
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




        private void UpdateTitle()
        {
            // Get the version of the application
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;

#if DEBUG
            this.Text = $"ESP Flasher '{version}' (DEBUG)";
#elif RELEASE
            this.Text = $"ESP Flasher '{version}'";
#endif

        }


        private async Task DoVersionCheck()
        {
            toolStripStatusLabel_version.Text = "Checking for updates";

            try
            {
                Version? version = Assembly.GetExecutingAssembly().GetName().Version;
                var checker = new GithubUpdateChecker("KooleControls", "ESPFlasher");
                var latestVersion = await checker.GetLatestVersionAsync();
                if (latestVersion > version)
                {
                    toolStripStatusLabel_version.Text = $"Update available: v{latestVersion.ToString()}";
                    toolStripStatusLabel_version.IsLink = true;
                    toolStripStatusLabel_version.Click += (sender, e) =>
                    {
                        var url = $"https://github.com/KooleControls/ESPFlasher/releases";
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    };
                }
                else
                {
                    toolStripStatusLabel_version.Text = "Up to date";
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabel_version.Text = "Error while checking for updates";
            }

        }

    }

}

