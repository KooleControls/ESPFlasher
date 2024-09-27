using ESP_Flasher.Models;
using ESP_Flasher.Services;

namespace ESP_Flasher.UIBinders
{
    public class PartitionTableListViewBinder
    {
        private readonly ListView _listView;
        private readonly ArchiveService _archiveService;
        public PartitionTableListViewBinder(ListView listView, ArchiveService archiveService)
        {
            _listView = listView;
            SetupColumns();
            _archiveService = archiveService;
        }

        private void SetupColumns()
        {
            _listView.Columns.Clear();
            _listView.Columns.Add("Name", 200);
            _listView.Columns.Add("Type", 100);
            _listView.Columns.Add("Subtype", 100);
            _listView.Columns.Add("Address", 100);
            _listView.Columns.Add("Size", 100);
        }

        public async Task Populate(FirmwareArchive archive)
        {
            _listView.Items.Clear();

            PartitionTable table = await _archiveService.ExtractPartitionTable(archive) ?? new PartitionTable();


            foreach (var partition in table.Partitions)
            {
                // Create a ListViewItem for each entry
                ListViewItem item = new ListViewItem(partition.Name);

                // Format the address and size as hexadecimal
                item.SubItems.Add($"0x{partition.Type:X}");
                item.SubItems.Add($"0x{partition.Subtype:X}");
                item.SubItems.Add($"0x{partition.Address:X}");
                item.SubItems.Add($"0x{partition.Size:X}");

                _listView.Items.Add(item);
            }

            // Auto-resize the columns to fit the content
            _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }




}

