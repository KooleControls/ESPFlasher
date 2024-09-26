using ESP_Flasher.Models;

namespace ESP_Flasher.UIBinders
{
    public class ArchiveListViewBinder
    {
        private readonly ListView _listView;

        public ArchiveListViewBinder(ListView listView)
        {
            _listView = listView;
            SetupColumns();
        }

        private void SetupColumns()
        {
            _listView.Columns.Clear();
            _listView.Columns.Add("FileName", 200);
            _listView.Columns.Add("Address", 100);
            _listView.Columns.Add("Size", 100);
        }

        public void Populate(List<BinFile> entries)
        {
            _listView.Items.Clear();

            foreach (var entry in entries)
            {
                // Create a ListViewItem for each entry
                ListViewItem item = new ListViewItem(entry.File);

                // Format the address and size as hexadecimal
                item.SubItems.Add($"0x{entry.Address:X}");  // Address as hexadecimal
                item.SubItems.Add($"0x{entry.Size:X}");     // Size as hexadecimal

                _listView.Items.Add(item);
            }

            // Auto-resize the columns to fit the content
            _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }


}

