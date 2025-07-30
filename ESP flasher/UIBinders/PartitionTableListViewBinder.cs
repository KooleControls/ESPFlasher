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
            Populate(table);
        }

        public void Populate(PartitionTable table)
        {
            foreach (var partitionEntry in table.Partitions)
            {
                // Create a ListViewItem for each entry
                ListViewItem item = new ListViewItem(partitionEntry.Name);
                item.Tag = partitionEntry;

                // Format the address and size as hexadecimal
                item.SubItems.Add($"0x{partitionEntry.Type:X}");
                item.SubItems.Add($"0x{partitionEntry.Subtype:X}");
                item.SubItems.Add($"0x{partitionEntry.Address:X}");
                item.SubItems.Add($"0x{partitionEntry.Size:X}");


                _listView.Items.Add(item);
            }

            // Auto-resize the columns to fit the content
            _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        public IEnumerable<PartitionEntry> GetSelected()
        {
            foreach (ListViewItem item in _listView.SelectedItems)
            {
                if (item.Tag is PartitionEntry entry)
                {
                    yield return entry;
                }
            }
        }
    }


}

