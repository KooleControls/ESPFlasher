namespace ESP_Flasher
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            groupBox2 = new GroupBox();
            listViewPartitionTable = new ListView();
            listViewHexFiles = new ListView();
            groupBox3 = new GroupBox();
            progressBar1 = new ProgressBar();
            checkBoxCompression = new CheckBox();
            buttonErase = new Button();
            buttonProgram = new Button();
            groupBox1 = new GroupBox();
            richTextBox1 = new RichTextBox();
            label1 = new Label();
            label2 = new Label();
            comboBoxBaudRate = new ComboBox();
            comboBoxSerialPort = new ComboBox();
            buttonRefresh = new Button();
            groupBox4 = new GroupBox();
            toolStrip1 = new ToolStrip();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(listViewPartitionTable);
            groupBox2.Controls.Add(listViewHexFiles);
            groupBox2.Location = new Point(12, 28);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(816, 229);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Archive";
            // 
            // listViewPartitionTable
            // 
            listViewPartitionTable.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listViewPartitionTable.Location = new Point(6, 125);
            listViewPartitionTable.Name = "listViewPartitionTable";
            listViewPartitionTable.Size = new Size(804, 97);
            listViewPartitionTable.TabIndex = 5;
            listViewPartitionTable.UseCompatibleStateImageBehavior = false;
            listViewPartitionTable.View = View.Details;
            // 
            // listViewHexFiles
            // 
            listViewHexFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listViewHexFiles.Location = new Point(6, 22);
            listViewHexFiles.Name = "listViewHexFiles";
            listViewHexFiles.Size = new Size(804, 97);
            listViewHexFiles.TabIndex = 4;
            listViewHexFiles.UseCompatibleStateImageBehavior = false;
            listViewHexFiles.View = View.Details;
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Controls.Add(progressBar1);
            groupBox3.Controls.Add(checkBoxCompression);
            groupBox3.Controls.Add(buttonErase);
            groupBox3.Controls.Add(buttonProgram);
            groupBox3.Location = new Point(12, 355);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(816, 84);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            groupBox3.Text = "Actions";
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(6, 52);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(804, 23);
            progressBar1.TabIndex = 12;
            // 
            // checkBoxCompression
            // 
            checkBoxCompression.AutoSize = true;
            checkBoxCompression.Checked = true;
            checkBoxCompression.CheckState = CheckState.Checked;
            checkBoxCompression.Location = new Point(168, 26);
            checkBoxCompression.Name = "checkBoxCompression";
            checkBoxCompression.Size = new Size(116, 19);
            checkBoxCompression.TabIndex = 11;
            checkBoxCompression.Text = "Use compression";
            checkBoxCompression.UseVisualStyleBackColor = true;
            // 
            // buttonErase
            // 
            buttonErase.Location = new Point(87, 23);
            buttonErase.Name = "buttonErase";
            buttonErase.Size = new Size(75, 23);
            buttonErase.TabIndex = 10;
            buttonErase.Text = "Erase";
            buttonErase.UseVisualStyleBackColor = true;
            buttonErase.Click += buttonErase_Click;
            // 
            // buttonProgram
            // 
            buttonProgram.Location = new Point(6, 23);
            buttonProgram.Name = "buttonProgram";
            buttonProgram.Size = new Size(75, 23);
            buttonProgram.TabIndex = 9;
            buttonProgram.Text = "Program";
            buttonProgram.UseVisualStyleBackColor = true;
            buttonProgram.Click += buttonProgram_Click;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(richTextBox1);
            groupBox1.Location = new Point(12, 445);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(816, 171);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Log";
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(6, 22);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(804, 143);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 26);
            label1.Name = "label1";
            label1.Size = new Size(54, 15);
            label1.TabIndex = 4;
            label1.Text = "Baudrate";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 55);
            label2.Name = "label2";
            label2.Size = new Size(60, 15);
            label2.TabIndex = 5;
            label2.Text = "Serial port";
            // 
            // comboBoxBaudRate
            // 
            comboBoxBaudRate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxBaudRate.FormattingEnabled = true;
            comboBoxBaudRate.Location = new Point(72, 23);
            comboBoxBaudRate.Name = "comboBoxBaudRate";
            comboBoxBaudRate.Size = new Size(657, 23);
            comboBoxBaudRate.TabIndex = 6;
            // 
            // comboBoxSerialPort
            // 
            comboBoxSerialPort.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxSerialPort.FormattingEnabled = true;
            comboBoxSerialPort.Location = new Point(72, 52);
            comboBoxSerialPort.Name = "comboBoxSerialPort";
            comboBoxSerialPort.Size = new Size(657, 23);
            comboBoxSerialPort.TabIndex = 7;
            // 
            // buttonRefresh
            // 
            buttonRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRefresh.Location = new Point(735, 52);
            buttonRefresh.Name = "buttonRefresh";
            buttonRefresh.Size = new Size(75, 23);
            buttonRefresh.TabIndex = 8;
            buttonRefresh.Text = "Refresh";
            buttonRefresh.UseVisualStyleBackColor = true;
            buttonRefresh.Click += buttonRefresh_Click;
            // 
            // groupBox4
            // 
            groupBox4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox4.Controls.Add(comboBoxBaudRate);
            groupBox4.Controls.Add(buttonRefresh);
            groupBox4.Controls.Add(label1);
            groupBox4.Controls.Add(label2);
            groupBox4.Controls.Add(comboBoxSerialPort);
            groupBox4.Location = new Point(12, 263);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(816, 86);
            groupBox4.TabIndex = 3;
            groupBox4.TabStop = false;
            groupBox4.Text = "Serial port";
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(840, 25);
            toolStrip1.TabIndex = 5;
            toolStrip1.Text = "toolStrip1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(840, 628);
            Controls.Add(toolStrip1);
            Controls.Add(groupBox4);
            Controls.Add(groupBox1);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            groupBox2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private ComboBox comboBoxSerialPort;
        private Label label2;
        private ComboBox comboBoxBaudRate;
        private Label label1;
        private GroupBox groupBox1;
        private Button buttonRefresh;
        private GroupBox groupBox4;
        private ProgressBar progressBar1;
        private CheckBox checkBoxCompression;
        private Button buttonErase;
        private Button buttonProgram;
        private RichTextBox richTextBox1;
        private ListView listViewPartitionTable;
        private ListView listViewHexFiles;
        private ToolStrip toolStrip1;
    }
}
