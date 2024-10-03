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
            groupBoxArchive = new GroupBox();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            listViewHexFiles = new ListView();
            listViewPartitionTable = new ListView();
            listViewAppHeader = new ListView();
            groupBoxActions = new GroupBox();
            checkBoxCompression = new CheckBox();
            buttonErase = new Button();
            buttonProgram = new Button();
            buttonCancel = new Button();
            progressBar1 = new ProgressBar();
            groupBoxLog = new GroupBox();
            richTextBox1 = new RichTextBox();
            label1 = new Label();
            label2 = new Label();
            comboBoxBaudRate = new ComboBox();
            comboBoxSerialPort = new ComboBox();
            buttonRefresh = new Button();
            groupBoxSerial = new GroupBox();
            toolStrip1 = new ToolStrip();
            groupBoxProgress = new GroupBox();
            groupBoxArchive.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBoxActions.SuspendLayout();
            groupBoxLog.SuspendLayout();
            groupBoxSerial.SuspendLayout();
            groupBoxProgress.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxArchive
            // 
            groupBoxArchive.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxArchive.Controls.Add(splitContainer1);
            groupBoxArchive.Location = new Point(12, 28);
            groupBoxArchive.Name = "groupBoxArchive";
            groupBoxArchive.Size = new Size(816, 229);
            groupBoxArchive.TabIndex = 1;
            groupBoxArchive.TabStop = false;
            groupBoxArchive.Text = "Archive";
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(6, 22);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(listViewAppHeader);
            splitContainer1.Size = new Size(804, 201);
            splitContainer1.SplitterDistance = 536;
            splitContainer1.TabIndex = 6;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(listViewHexFiles);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(listViewPartitionTable);
            splitContainer2.Size = new Size(536, 201);
            splitContainer2.SplitterDistance = 268;
            splitContainer2.TabIndex = 0;
            // 
            // listViewHexFiles
            // 
            listViewHexFiles.Dock = DockStyle.Fill;
            listViewHexFiles.Location = new Point(0, 0);
            listViewHexFiles.Name = "listViewHexFiles";
            listViewHexFiles.Size = new Size(268, 201);
            listViewHexFiles.TabIndex = 4;
            listViewHexFiles.UseCompatibleStateImageBehavior = false;
            listViewHexFiles.View = View.Details;
            // 
            // listViewPartitionTable
            // 
            listViewPartitionTable.Dock = DockStyle.Fill;
            listViewPartitionTable.Location = new Point(0, 0);
            listViewPartitionTable.Name = "listViewPartitionTable";
            listViewPartitionTable.Size = new Size(264, 201);
            listViewPartitionTable.TabIndex = 5;
            listViewPartitionTable.UseCompatibleStateImageBehavior = false;
            listViewPartitionTable.View = View.Details;
            // 
            // listViewAppHeader
            // 
            listViewAppHeader.Dock = DockStyle.Fill;
            listViewAppHeader.Location = new Point(0, 0);
            listViewAppHeader.Name = "listViewAppHeader";
            listViewAppHeader.Size = new Size(264, 201);
            listViewAppHeader.TabIndex = 5;
            listViewAppHeader.UseCompatibleStateImageBehavior = false;
            listViewAppHeader.View = View.Details;
            // 
            // groupBoxActions
            // 
            groupBoxActions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxActions.Controls.Add(checkBoxCompression);
            groupBoxActions.Controls.Add(buttonErase);
            groupBoxActions.Controls.Add(buttonProgram);
            groupBoxActions.Location = new Point(12, 355);
            groupBoxActions.Name = "groupBoxActions";
            groupBoxActions.Size = new Size(816, 54);
            groupBoxActions.TabIndex = 2;
            groupBoxActions.TabStop = false;
            groupBoxActions.Text = "Actions";
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
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonCancel.Location = new Point(735, 22);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 13;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(6, 22);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(723, 23);
            progressBar1.TabIndex = 12;
            // 
            // groupBoxLog
            // 
            groupBoxLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxLog.Controls.Add(richTextBox1);
            groupBoxLog.Location = new Point(12, 477);
            groupBoxLog.Name = "groupBoxLog";
            groupBoxLog.Size = new Size(816, 253);
            groupBoxLog.TabIndex = 3;
            groupBoxLog.TabStop = false;
            groupBoxLog.Text = "Log";
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(6, 22);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(804, 225);
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
            // groupBoxSerial
            // 
            groupBoxSerial.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxSerial.Controls.Add(comboBoxBaudRate);
            groupBoxSerial.Controls.Add(buttonRefresh);
            groupBoxSerial.Controls.Add(label1);
            groupBoxSerial.Controls.Add(label2);
            groupBoxSerial.Controls.Add(comboBoxSerialPort);
            groupBoxSerial.Location = new Point(12, 263);
            groupBoxSerial.Name = "groupBoxSerial";
            groupBoxSerial.Size = new Size(816, 86);
            groupBoxSerial.TabIndex = 3;
            groupBoxSerial.TabStop = false;
            groupBoxSerial.Text = "Serial port";
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(840, 25);
            toolStrip1.TabIndex = 5;
            toolStrip1.Text = "toolStrip1";
            // 
            // groupBoxProgress
            // 
            groupBoxProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxProgress.Controls.Add(buttonCancel);
            groupBoxProgress.Controls.Add(progressBar1);
            groupBoxProgress.Location = new Point(12, 415);
            groupBoxProgress.Name = "groupBoxProgress";
            groupBoxProgress.Size = new Size(816, 56);
            groupBoxProgress.TabIndex = 12;
            groupBoxProgress.TabStop = false;
            groupBoxProgress.Text = "Progress";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(840, 742);
            Controls.Add(groupBoxProgress);
            Controls.Add(toolStrip1);
            Controls.Add(groupBoxSerial);
            Controls.Add(groupBoxLog);
            Controls.Add(groupBoxActions);
            Controls.Add(groupBoxArchive);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            groupBoxArchive.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBoxActions.ResumeLayout(false);
            groupBoxActions.PerformLayout();
            groupBoxLog.ResumeLayout(false);
            groupBoxSerial.ResumeLayout(false);
            groupBoxSerial.PerformLayout();
            groupBoxProgress.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBoxArchive;
        private GroupBox groupBoxActions;
        private ComboBox comboBoxSerialPort;
        private Label label2;
        private ComboBox comboBoxBaudRate;
        private Label label1;
        private GroupBox groupBoxLog;
        private Button buttonRefresh;
        private GroupBox groupBoxSerial;
        private ProgressBar progressBar1;
        private CheckBox checkBoxCompression;
        private Button buttonErase;
        private Button buttonProgram;
        private RichTextBox richTextBox1;
        private ListView listViewPartitionTable;
        private ListView listViewHexFiles;
        private ToolStrip toolStrip1;
        private Button buttonCancel;
        private GroupBox groupBoxProgress;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private ListView listViewAppHeader;
    }
}
