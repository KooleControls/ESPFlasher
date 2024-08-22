using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ESP_flasher
{
    public partial class Release : Form
    {
        public readonly BindingList<string> elfFiles = new BindingList<string>();
        public CreateArchives Parent { get; set; }

        public Release()
        {
            InitializeComponent();
        }

        private void Release_Load(object sender, EventArgs e)
        {
            listBox1.DataSource = Parent.files;
            //Try to get version number.


            //Try to get ELF files

            foreach(BinFile f in Parent.files)
            {
                string elf = f.File.Replace(".bin", ".elf");
                if(File.Exists(elf))
                {
                    elfFiles.Add(elf);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string versPath = Path.Combine(textBox2.Text, textBox1.Text);

            if (!Directory.Exists(versPath))
            {
                Directory.CreateDirectory(versPath);

                //TODO

            }
            else
            {
                MessageBox.Show($"Release already exists!\r\n{versPath}");
            }
            




            
        }
    }
}
