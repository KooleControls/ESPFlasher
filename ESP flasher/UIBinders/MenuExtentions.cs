namespace ESP_Flasher.UIBinders
{
    public static class MenuExtensions
    {
        public static ToolStripMenuItem AddMenuItem(this ToolStrip menu, string menuPath)
        {
            string[] split = menuPath.Split('/');
            ToolStripMenuItem item = null;

            if (menu.Items[split[0]] is ToolStripMenuItem tsi)
                item = tsi;
            else
            {
                item = new ToolStripMenuItem(split[0]);
                item.Name = split[0];
                menu.Items.Add(item);
            }

            for (int i = 1; i < split.Length; i++)
            {
                string name = split[i];

                if (item.DropDownItems[name] is ToolStripMenuItem tsii)
                    item = tsii;
                else
                {
                    ToolStripMenuItem newItem = new ToolStripMenuItem(name);
                    newItem.Name = name;
                    item.DropDownItems.Add(newItem);
                    item = newItem;
                }
            }

            return item;
        }

        public static ToolStripMenuItem AddMenuItem(this ToolStripMenuItem menuItem, string menuPath)
        {
            string[] split = menuPath.Split('/');
            ToolStripMenuItem item = menuItem;

            for (int i = 1; i < split.Length; i++)
            {
                string name = split[i];

                if (item.DropDownItems[name] is ToolStripMenuItem tsii)
                    item = tsii;
                else
                {
                    ToolStripMenuItem newItem = new ToolStripMenuItem(name);
                    newItem.Name = name;
                    item.DropDownItems.Add(newItem);
                    item = newItem;
                }
            }

            return item;
        }

        public static ToolStripMenuItem WithAction(this ToolStripMenuItem menuItem, Action action)
        {
            if (action != null)
                menuItem.Click += (a, b) => action.Invoke();

            return menuItem;
        }

        public static ToolStripMenuItem WithToolTip(this ToolStripMenuItem menuItem, string toolTip)
        {
            menuItem.ToolTipText = toolTip;
            return menuItem;
        }
    }
}
