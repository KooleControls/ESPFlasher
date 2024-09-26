using ESP_Flasher.Models;

namespace ESP_Flasher.UIBinders
{
    public class PartitionTableListViewBinder
    {
        private readonly ListView _listView;

        public PartitionTableListViewBinder(ListView listView)
        {
            _listView = listView;
            SetupColumns();
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

        public void Populate(PartitionTable partitionTable)
        {
            _listView.Items.Clear();

            foreach (var partition in partitionTable.Partitions)
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

