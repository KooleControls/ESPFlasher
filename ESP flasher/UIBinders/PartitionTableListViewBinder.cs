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
            _listView.Columns.Add("Type", 100);
            _listView.Columns.Add("Subtype", 100);
            _listView.Columns.Add("Address", 100);
            _listView.Columns.Add("Size", 100);
            _listView.Columns.Add("Name", 200);
        }

        public void Populate(PartitionTable partitionTable)
        {
            _listView.Items.Clear();

            foreach (var partition in partitionTable.Partitions)
            {
                ListViewItem item = new ListViewItem(partition.Type.ToString());
                item.SubItems.Add(partition.Subtype.ToString());
                item.SubItems.Add(partition.Address.ToString("X"));
                item.SubItems.Add(partition.Size.ToString());
                item.SubItems.Add(partition.Name);

                _listView.Items.Add(item);
            }

            // Auto-resize the columns to fit the content
            _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }




}

