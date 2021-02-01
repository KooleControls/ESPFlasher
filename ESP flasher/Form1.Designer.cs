namespace ESP_flasher
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.createArchives1 = new ESP_flasher.CreateArchives();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.flashArchive1 = new ESP_flasher.FlashArchive();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.createArchives1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(510, 460);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Ontwikkeling";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // createArchives1
            // 
            this.createArchives1.ArchiveFilter = null;
            this.createArchives1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.createArchives1.HexFilter = null;
            this.createArchives1.Location = new System.Drawing.Point(3, 3);
            this.createArchives1.Name = "createArchives1";
            this.createArchives1.Size = new System.Drawing.Size(504, 454);
            this.createArchives1.TabIndex = 0;
            this.createArchives1.TempFolder = null;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.flashArchive1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(510, 460);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Productie";
            // 
            // flashArchive1
            // 
            this.flashArchive1.ArchiveFilter = null;
            this.flashArchive1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flashArchive1.ESPToolExe = null;
            this.flashArchive1.Location = new System.Drawing.Point(3, 3);
            this.flashArchive1.Name = "flashArchive1";
            this.flashArchive1.Size = new System.Drawing.Size(504, 454);
            this.flashArchive1.TabIndex = 0;
            this.flashArchive1.TempFolder = null;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(518, 486);
            this.tabControl1.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(518, 486);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "ESP32 flash tool";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabPage2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabControl tabControl1;
        private CreateArchives createArchives1;
        private FlashArchive flashArchive1;
    }
}

