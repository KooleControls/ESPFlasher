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
            _listView.Columns.Add("Address", 100);
            _listView.Columns.Add("FileName", 200);
        }

        public void Populate(List<BinFile> entries)
        {
            _listView.Items.Clear();

            foreach (var entry in entries)
            {
                ListViewItem item = new ListViewItem(entry.Address.ToString("X"));  // Display address as hex
                item.SubItems.Add(entry.File);

                _listView.Items.Add(item);
            }

            // Auto-resize the columns to fit the content
            _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }


}

