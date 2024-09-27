using ESP_Flasher.Models;
using ESP_Flasher.Services;

namespace ESP_Flasher.UIBinders
{
    public class AppHeaderListViewBinder
    {
        private readonly ListView _listView;
        private readonly ArchiveService _archiveService;

        public AppHeaderListViewBinder(ListView listView, ArchiveService archiveService)
        {
            _listView = listView;
            _archiveService = archiveService;
            SetupColumns();
        }

        private void SetupColumns()
        {
            _listView.Columns.Clear();
            _listView.Columns.Add("Field", 200);
            _listView.Columns.Add("Value", 400);
        }

        public async Task Populate(FirmwareArchive archive)
        {
            _listView.Items.Clear();

            AppHeader? appHeader = await _archiveService.ExtractAppHeader(archive);

            if (appHeader == null)
            {
                ListViewItem errorItem = new ListViewItem("Error");
                errorItem.SubItems.Add("App header not found or invalid.");
                _listView.Items.Add(errorItem);
                return;
            }

            // Populate the ListView with AppHeader details
            AddListViewItem("Project Name", appHeader.ProjectName);
            AddListViewItem("Version", appHeader.Version);
            AddListViewItem("Compile Time", appHeader.CompileTime);
            AddListViewItem("Compile Date", appHeader.CompileDate);
            AddListViewItem("IDF Version", appHeader.IdfVer);

            AddListViewItem("Entry Address", $"0x{appHeader.EntryAddr:X}");
            AddListViewItem("Segment Count", appHeader.SegmentCount.ToString());
            AddListViewItem("SPI Mode", $"0x{appHeader.SpiMode:X}");
            AddListViewItem("SPI Speed", $"0x{appHeader.SpiSpeed:X}");
            AddListViewItem("SPI Size", $"0x{appHeader.SpiSize:X}");
            AddListViewItem("WP Pin", $"0x{appHeader.WpPin:X}");
            AddListViewItem("SPI Pin Drive", BitConverter.ToString(appHeader.SpiPinDrv).Replace("-", " "));
            AddListViewItem("Hash Appended", appHeader.HashAppended.ToString());
            AddListViewItem("Secure Version", appHeader.SecureVersion.ToString());
            AddListViewItem("App ELF SHA256", BitConverter.ToString(appHeader.AppElfSha256).Replace("-", ""));

            // Auto-resize the columns to fit the content
            _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AddListViewItem(string field, string value)
        {
            ListViewItem item = new ListViewItem(field);
            item.SubItems.Add(value);
            _listView.Items.Add(item);
        }
    }




}

