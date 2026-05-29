using ESP_Flasher.Logging;
using ESP_Flasher.Models;
using ESP_Flasher.Services;
using ESP_Flasher.UIBinders;
using Microsoft.Extensions.Logging;
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
        private readonly DeviceService _flashingService;

        // Binders
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
            _flashingService = new DeviceService(_richTextBoxLoggerFactory);

            // Bind the UI elements to data
            _serialPortBinder = new SerialPortBinder(comboBoxSerialPort, comboBoxBaudRate);
            _archiveBinder = new ArchiveListViewBinder(listViewHexFiles);
            _partitionBinder = new PartitionTableListViewBinder(listViewPartitionTable, _archiveService);
            _progressBarBinder = new ProgressBarBinder(progressBar1);
            _appHeaderListViewBinder = new AppHeaderListViewBinder(listViewAppHeader, _archiveService);

            // Set variables;
            logger = _richTextBoxLoggerFactory.CreateLogger<Form1>();
            openArchive = new FirmwareArchive();

            UpdateTitle();
            _ = DoVersionCheck();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // File Operations
            toolStrip1.AddMenuItem("File/New").WithAction(NewArchive);
            toolStrip1.AddMenuItem("File/Open").WithAction(Open);
            toolStrip1.AddMenuItem("File/Open Archive").WithAction(OpenArchiveDialog);
            toolStrip1.AddMenuItem("File/Save as/Archive").WithAction(SaveArchive).WithToolTip("Creates KCZIP file");
            toolStrip1.AddMenuItem("File/Save as/Application intel hex").WithAction(SaveApplicationIntelHex).WithToolTip("Creates HEX file to be used in KC220 tool and LM");
            toolStrip1.AddMenuItem("File/Save as/Archive intel hex").WithAction(SaveArchiveIntelHex).WithToolTip("Creates HEX file including all parititions");
            toolStrip1.AddMenuItem("File/Exit").WithAction(Close);

            // Development Tools
            toolStrip1.AddMenuItem("Development/Open Build folder").WithAction(LoadBuildDirectory);
            toolStrip1.AddMenuItem("Development/Read partition table from device").WithAction(ReadPartitionTableFromDevice);

            // Right-click a partition row to read it from the device or inspect downloaded bytes.
            var partitionMenu = new ContextMenuStrip();
            partitionMenu.Items.Add("Download to memory", null, (s, e) => DownloadSelectedPartition());
            partitionMenu.Items.Add("Peek (hex viewer)", null, (s, e) => PeekSelectedPartition());
            listViewPartitionTable.ContextMenuStrip = partitionMenu;
            listViewPartitionTable.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Right) return;
                var hit = listViewPartitionTable.HitTest(e.Location);
                listViewPartitionTable.SelectedItems.Clear();
                if (hit.Item != null) hit.Item.Selected = true;
            };
        }

        private async void NewArchive()
        {
            openArchive = new FirmwareArchive();
            _archiveBinder.Populate(openArchive);
            await _partitionBinder.Populate(openArchive);
            await _appHeaderListViewBinder.Populate(openArchive);
        }

        private async void Open()
        {
            // Support .bin (both the appl only and the full factory flash)
            // Support the .hex file
            // Dont support the .kczip file, as that is a custom format 



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

        private async void ReadPartitionTableFromDevice()
        {
            // Default ESP32 partition table location and size.
            const uint partitionTableOffset = 0x8000;
            const uint partitionTableSize = 0x1000;

            try
            {
                UiEnabled(false);
                _richTextBoxLoggerFactory.Clear();
                cancelButtonSource = new CancellationTokenSource();
                _flashingService.SerialPort = _serialPortBinder.SelectedSerialPortName;
                _flashingService.BaudRate = _serialPortBinder.SelectedBaudRate;

                byte[] data = await _flashingService.ReadFlashAsync(partitionTableOffset, partitionTableSize, cancelButtonSource.Token, _progressBarBinder.Bind());

                // Feed the bytes through the existing partition-table extractor + binder by wrapping
                // them in a one-off archive that looks like the on-disk format.
                var archive = new FirmwareArchive();
                archive.BinFiles.Add(new BinFile
                {
                    File = "partition-table.bin",
                    Address = (int)partitionTableOffset,
                    Contents = data,
                });
                await _partitionBinder.Populate(archive);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read partition table from device");
            }
            finally
            {
                _progressBarBinder.Bind().Report(0);
                UiEnabled(true);
            }
        }

        private async void DownloadSelectedPartition()
        {
            if (listViewPartitionTable.SelectedItems.Count == 0 ||
                listViewPartitionTable.SelectedItems[0].Tag is not PartitionEntry entry)
                return;

            try
            {
                UiEnabled(false);
                _richTextBoxLoggerFactory.Clear();
                cancelButtonSource = new CancellationTokenSource();
                _flashingService.SerialPort = _serialPortBinder.SelectedSerialPortName;
                _flashingService.BaudRate = _serialPortBinder.SelectedBaudRate;

                byte[] data = await _flashingService.ReadFlashAsync(entry.Address, entry.Size, cancelButtonSource.Token, _progressBarBinder.Bind());
                entry.DownloadedContents = data;
                _partitionBinder.MarkAsDownloaded(entry);
                logger.LogInformation("Downloaded partition '{Name}' ({Size} bytes) into memory", entry.Name, data.Length);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to download partition '{Name}'", entry.Name);
            }
            finally
            {
                _progressBarBinder.Bind().Report(0);
                UiEnabled(true);
            }
        }

        private void PeekSelectedPartition()
        {
            if (listViewPartitionTable.SelectedItems.Count == 0 ||
                listViewPartitionTable.SelectedItems[0].Tag is not PartitionEntry entry)
                return;

            if (entry.DownloadedContents == null)
            {
                logger.LogWarning("Partition '{Name}' has not been downloaded yet", entry.Name);
                return;
            }

            var viewer = new HexViewerForm($"{entry.Name} @ 0x{entry.Address:X}", entry.Address, entry.DownloadedContents);
            viewer.Show(this);
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
                _richTextBoxLoggerFactory.Clear();
                cancelButtonSource = new CancellationTokenSource();
                _flashingService.UseCompression = checkBoxCompression.Checked;
                _flashingService.SerialPort = _serialPortBinder.SelectedSerialPortName;
                _flashingService.BaudRate = _serialPortBinder.SelectedBaudRate;
                await _flashingService.EraseFlashAsync(cancelButtonSource.Token);
                _flashingService.DisposeDevice();
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
                _flashingService.UseCompression = checkBoxCompression.Checked;
                _flashingService.SerialPort = _serialPortBinder.SelectedSerialPortName;
                _flashingService.BaudRate = _serialPortBinder.SelectedBaudRate;
                await _flashingService.FlashAsync(openArchive, cancelButtonSource.Token, _progressBarBinder.Bind());
                _flashingService.DisposeDevice();
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
                    toolStripStatusLabel_version.Click += (sender, e) => {
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
            }catch (Exception)
            {
                toolStripStatusLabel_version.Text = "Error while checking for updates";
            }
            
        }

    }

}

