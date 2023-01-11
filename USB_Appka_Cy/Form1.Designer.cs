namespace USB_Appka_Cy
{
    partial class Form1
    {
        /// <summary>
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód generovaný Návrhářem Windows Form

        /// <summary>
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_Info = new System.Windows.Forms.Button();
            this.lbl_Status = new System.Windows.Forms.Label();
            this.log = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fX3SPIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bOSInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btn_recive = new System.Windows.Forms.Button();
            this.led_SS = new System.Windows.Forms.Panel();
            this.lbl_led = new System.Windows.Forms.Label();
            this.DeviceTreeView = new System.Windows.Forms.TreeView();
            this.btn_close = new System.Windows.Forms.Button();
            this.lbl_Throughout = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_fails = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Info
            // 
            this.btn_Info.Location = new System.Drawing.Point(304, 104);
            this.btn_Info.Name = "btn_Info";
            this.btn_Info.Size = new System.Drawing.Size(75, 23);
            this.btn_Info.TabIndex = 0;
            this.btn_Info.Text = "Device Info";
            this.btn_Info.UseVisualStyleBackColor = true;
            this.btn_Info.Click += new System.EventHandler(this.btn_Info_Click);
            // 
            // lbl_Status
            // 
            this.lbl_Status.AutoSize = true;
            this.lbl_Status.Location = new System.Drawing.Point(398, 24);
            this.lbl_Status.Name = "lbl_Status";
            this.lbl_Status.Size = new System.Drawing.Size(53, 13);
            this.lbl_Status.TabIndex = 1;
            this.lbl_Status.Text = "lbl_Status";
            // 
            // log
            // 
            this.log.Location = new System.Drawing.Point(401, 50);
            this.log.Multiline = true;
            this.log.Name = "log";
            this.log.Size = new System.Drawing.Size(348, 388);
            this.log.TabIndex = 2;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.deviceInfoToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fX3SPIToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(95, 20);
            this.toolStripMenuItem1.Text = "Download_FW";
            // 
            // fX3SPIToolStripMenuItem
            // 
            this.fX3SPIToolStripMenuItem.Name = "fX3SPIToolStripMenuItem";
            this.fX3SPIToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.fX3SPIToolStripMenuItem.Text = "FX_3 SPI";
            this.fX3SPIToolStripMenuItem.Click += new System.EventHandler(this.fX3SPIToolStripMenuItem_Click);
            // 
            // deviceInfoToolStripMenuItem
            // 
            this.deviceInfoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bOSInfoToolStripMenuItem,
            this.treeViewToolStripMenuItem});
            this.deviceInfoToolStripMenuItem.Name = "deviceInfoToolStripMenuItem";
            this.deviceInfoToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.deviceInfoToolStripMenuItem.Text = "Device Info";
            // 
            // bOSInfoToolStripMenuItem
            // 
            this.bOSInfoToolStripMenuItem.Name = "bOSInfoToolStripMenuItem";
            this.bOSInfoToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.bOSInfoToolStripMenuItem.Text = "BOS Info";
            this.bOSInfoToolStripMenuItem.Click += new System.EventHandler(this.bOSInfoToolStripMenuItem_Click);
            // 
            // treeViewToolStripMenuItem
            // 
            this.treeViewToolStripMenuItem.Name = "treeViewToolStripMenuItem";
            this.treeViewToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.treeViewToolStripMenuItem.Text = "Tree View";
            this.treeViewToolStripMenuItem.Click += new System.EventHandler(this.treeViewToolStripMenuItem_Click);
            // 
            // btn_recive
            // 
            this.btn_recive.Location = new System.Drawing.Point(65, 103);
            this.btn_recive.Name = "btn_recive";
            this.btn_recive.Size = new System.Drawing.Size(75, 23);
            this.btn_recive.TabIndex = 4;
            this.btn_recive.Text = "SendToFile";
            this.btn_recive.UseVisualStyleBackColor = true;
            this.btn_recive.Click += new System.EventHandler(this.btn_recive_Click);
            // 
            // led_SS
            // 
            this.led_SS.BackColor = System.Drawing.Color.PaleGreen;
            this.led_SS.Location = new System.Drawing.Point(12, 39);
            this.led_SS.Name = "led_SS";
            this.led_SS.Size = new System.Drawing.Size(19, 13);
            this.led_SS.TabIndex = 5;
            // 
            // lbl_led
            // 
            this.lbl_led.AutoSize = true;
            this.lbl_led.Location = new System.Drawing.Point(37, 39);
            this.lbl_led.Name = "lbl_led";
            this.lbl_led.Size = new System.Drawing.Size(139, 13);
            this.lbl_led.TabIndex = 6;
            this.lbl_led.Text = "USB 3.0 Compatible Device";
            // 
            // DeviceTreeView
            // 
            this.DeviceTreeView.Location = new System.Drawing.Point(54, 145);
            this.DeviceTreeView.Name = "DeviceTreeView";
            this.DeviceTreeView.Size = new System.Drawing.Size(296, 273);
            this.DeviceTreeView.TabIndex = 7;
            this.DeviceTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DeviceTreeView_AfterSelect);
            // 
            // btn_close
            // 
            this.btn_close.Enabled = false;
            this.btn_close.Location = new System.Drawing.Point(146, 104);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(75, 23);
            this.btn_close.TabIndex = 8;
            this.btn_close.Text = "Close";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // lbl_Throughout
            // 
            this.lbl_Throughout.AutoSize = true;
            this.lbl_Throughout.Location = new System.Drawing.Point(271, 53);
            this.lbl_Throughout.Name = "lbl_Throughout";
            this.lbl_Throughout.Size = new System.Drawing.Size(13, 13);
            this.lbl_Throughout.TabIndex = 9;
            this.lbl_Throughout.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Disconnect count:";
            // 
            // lbl_fails
            // 
            this.lbl_fails.AutoSize = true;
            this.lbl_fails.Location = new System.Drawing.Point(109, 76);
            this.lbl_fails.Name = "lbl_fails";
            this.lbl_fails.Size = new System.Drawing.Size(22, 13);
            this.lbl_fails.TabIndex = 11;
            this.lbl_fails.Text = "xxx";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lbl_fails);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_Throughout);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.DeviceTreeView);
            this.Controls.Add(this.lbl_led);
            this.Controls.Add(this.led_SS);
            this.Controls.Add(this.btn_recive);
            this.Controls.Add(this.log);
            this.Controls.Add(this.lbl_Status);
            this.Controls.Add(this.btn_Info);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Info;
        private System.Windows.Forms.Label lbl_Status;
        private System.Windows.Forms.TextBox log;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fX3SPIToolStripMenuItem;
        private System.Windows.Forms.Button btn_recive;
        private System.Windows.Forms.ToolStripMenuItem deviceInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bOSInfoToolStripMenuItem;
        private System.Windows.Forms.Panel led_SS;
        private System.Windows.Forms.Label lbl_led;
        private System.Windows.Forms.ToolStripMenuItem treeViewToolStripMenuItem;
        private System.Windows.Forms.TreeView DeviceTreeView;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.Label lbl_Throughout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_fails;
    }
}

