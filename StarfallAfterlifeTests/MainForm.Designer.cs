
namespace StarfallAfterlife
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            ConsoleView = new System.Windows.Forms.TextBox();
            panel1 = new System.Windows.Forms.Panel();
            groupBox2 = new System.Windows.Forms.GroupBox();
            PathFindingTestBtn = new System.Windows.Forms.Button();
            SystemHexMapTestBtn = new System.Windows.Forms.Button();
            ScanQuestsBtn = new System.Windows.Forms.Button();
            StartGalaxyInstanseBtn = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            SelectGameDirectoryBtn = new System.Windows.Forms.Button();
            GameDirectoryBox = new System.Windows.Forms.TextBox();
            StartBtn = new System.Windows.Forms.Button();
            Tabs = new System.Windows.Forms.TabControl();
            tabPage1 = new System.Windows.Forms.TabPage();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            panel5 = new System.Windows.Forms.Panel();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            panel6 = new System.Windows.Forms.Panel();
            dataGridView2 = new System.Windows.Forms.DataGridView();
            dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            toolStrip2 = new System.Windows.Forms.ToolStrip();
            toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            tabPage3 = new System.Windows.Forms.TabPage();
            LogTab = new System.Windows.Forms.TabPage();
            tabPage2 = new System.Windows.Forms.TabPage();
            panel4 = new System.Windows.Forms.Panel();
            panel2 = new System.Windows.Forms.Panel();
            panel3 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            Tabs.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            toolStrip1.SuspendLayout();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            toolStrip2.SuspendLayout();
            tabPage3.SuspendLayout();
            LogTab.SuspendLayout();
            tabPage2.SuspendLayout();
            panel4.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // ConsoleView
            // 
            ConsoleView.BackColor = System.Drawing.Color.FromArgb(15, 10, 65);
            ConsoleView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            ConsoleView.Dock = System.Windows.Forms.DockStyle.Fill;
            ConsoleView.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            ConsoleView.ForeColor = System.Drawing.Color.White;
            ConsoleView.Location = new System.Drawing.Point(3, 3);
            ConsoleView.Margin = new System.Windows.Forms.Padding(0);
            ConsoleView.Multiline = true;
            ConsoleView.Name = "ConsoleView";
            ConsoleView.ReadOnly = true;
            ConsoleView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            ConsoleView.Size = new System.Drawing.Size(775, 545);
            ConsoleView.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.SystemColors.Window;
            panel1.Controls.Add(groupBox2);
            panel1.Dock = System.Windows.Forms.DockStyle.Left;
            panel1.Location = new System.Drawing.Point(3, 3);
            panel1.Margin = new System.Windows.Forms.Padding(0);
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(3);
            panel1.Size = new System.Drawing.Size(300, 545);
            panel1.TabIndex = 1;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(PathFindingTestBtn);
            groupBox2.Controls.Add(SystemHexMapTestBtn);
            groupBox2.Controls.Add(ScanQuestsBtn);
            groupBox2.Controls.Add(StartGalaxyInstanseBtn);
            groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox2.Location = new System.Drawing.Point(3, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(294, 167);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Tests";
            // 
            // PathFindingTestBtn
            // 
            PathFindingTestBtn.Dock = System.Windows.Forms.DockStyle.Top;
            PathFindingTestBtn.Location = new System.Drawing.Point(3, 115);
            PathFindingTestBtn.Name = "PathFindingTestBtn";
            PathFindingTestBtn.Size = new System.Drawing.Size(288, 32);
            PathFindingTestBtn.TabIndex = 4;
            PathFindingTestBtn.Text = "Test PathFinding";
            PathFindingTestBtn.UseVisualStyleBackColor = true;
            PathFindingTestBtn.Click += PathFindingTestBtn_Click;
            // 
            // SystemHexMapTestBtn
            // 
            SystemHexMapTestBtn.Dock = System.Windows.Forms.DockStyle.Top;
            SystemHexMapTestBtn.Location = new System.Drawing.Point(3, 83);
            SystemHexMapTestBtn.Name = "SystemHexMapTestBtn";
            SystemHexMapTestBtn.Size = new System.Drawing.Size(288, 32);
            SystemHexMapTestBtn.TabIndex = 3;
            SystemHexMapTestBtn.Text = "Test SystemHexMap";
            SystemHexMapTestBtn.UseVisualStyleBackColor = true;
            SystemHexMapTestBtn.Click += SystemHexMapTestBtn_Click;
            // 
            // ScanQuestsBtn
            // 
            ScanQuestsBtn.Dock = System.Windows.Forms.DockStyle.Top;
            ScanQuestsBtn.Location = new System.Drawing.Point(3, 51);
            ScanQuestsBtn.Name = "ScanQuestsBtn";
            ScanQuestsBtn.Size = new System.Drawing.Size(288, 32);
            ScanQuestsBtn.TabIndex = 2;
            ScanQuestsBtn.Text = "Scan Quests";
            ScanQuestsBtn.UseVisualStyleBackColor = true;
            ScanQuestsBtn.Click += ScanQuestsBtnClick;
            // 
            // StartGalaxyInstanseBtn
            // 
            StartGalaxyInstanseBtn.Dock = System.Windows.Forms.DockStyle.Top;
            StartGalaxyInstanseBtn.Location = new System.Drawing.Point(3, 19);
            StartGalaxyInstanseBtn.Name = "StartGalaxyInstanseBtn";
            StartGalaxyInstanseBtn.Size = new System.Drawing.Size(288, 32);
            StartGalaxyInstanseBtn.TabIndex = 1;
            StartGalaxyInstanseBtn.Text = "Start Galaxy Instanse";
            StartGalaxyInstanseBtn.UseVisualStyleBackColor = true;
            StartGalaxyInstanseBtn.Click += StartGalaxyInstanseClick;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.Controls.Add(SelectGameDirectoryBtn);
            groupBox1.Controls.Add(GameDirectoryBox);
            groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            groupBox1.Location = new System.Drawing.Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(294, 68);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Game directory";
            // 
            // SelectGameDirectoryBtn
            // 
            SelectGameDirectoryBtn.Dock = System.Windows.Forms.DockStyle.Top;
            SelectGameDirectoryBtn.Location = new System.Drawing.Point(3, 42);
            SelectGameDirectoryBtn.Name = "SelectGameDirectoryBtn";
            SelectGameDirectoryBtn.Size = new System.Drawing.Size(288, 23);
            SelectGameDirectoryBtn.TabIndex = 0;
            SelectGameDirectoryBtn.Text = "Select";
            SelectGameDirectoryBtn.UseVisualStyleBackColor = true;
            SelectGameDirectoryBtn.Click += SelectGameDirectoryBtnClick;
            // 
            // GameDirectoryBox
            // 
            GameDirectoryBox.Dock = System.Windows.Forms.DockStyle.Top;
            GameDirectoryBox.Location = new System.Drawing.Point(3, 19);
            GameDirectoryBox.Name = "GameDirectoryBox";
            GameDirectoryBox.Size = new System.Drawing.Size(288, 23);
            GameDirectoryBox.TabIndex = 1;
            // 
            // StartBtn
            // 
            StartBtn.Dock = System.Windows.Forms.DockStyle.Right;
            StartBtn.Location = new System.Drawing.Point(608, 6);
            StartBtn.Name = "StartBtn";
            StartBtn.Size = new System.Drawing.Size(175, 33);
            StartBtn.TabIndex = 2;
            StartBtn.Text = "Start";
            StartBtn.UseVisualStyleBackColor = true;
            StartBtn.Click += StartBtnClick;
            // 
            // Tabs
            // 
            Tabs.Controls.Add(tabPage1);
            Tabs.Controls.Add(tabPage3);
            Tabs.Controls.Add(LogTab);
            Tabs.Controls.Add(tabPage2);
            Tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            Tabs.Location = new System.Drawing.Point(0, 0);
            Tabs.Margin = new System.Windows.Forms.Padding(0);
            Tabs.Name = "Tabs";
            Tabs.SelectedIndex = 0;
            Tabs.Size = new System.Drawing.Size(789, 579);
            Tabs.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer1);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Margin = new System.Windows.Forms.Padding(0);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new System.Drawing.Size(781, 551);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Game";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Window;
            splitContainer1.Panel1.Controls.Add(panel5);
            splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(3);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Window;
            splitContainer1.Panel2.Controls.Add(panel6);
            splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(3);
            splitContainer1.Size = new System.Drawing.Size(781, 551);
            splitContainer1.SplitterDistance = 386;
            splitContainer1.TabIndex = 0;
            // 
            // panel5
            // 
            panel5.Controls.Add(dataGridView1);
            panel5.Controls.Add(toolStrip1);
            panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            panel5.Location = new System.Drawing.Point(3, 3);
            panel5.Margin = new System.Windows.Forms.Padding(0);
            panel5.Name = "panel5";
            panel5.Size = new System.Drawing.Size(380, 545);
            panel5.TabIndex = 2;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(60, 80, 100);
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { Column1 });
            dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView1.Location = new System.Drawing.Point(0, 25);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowTemplate.Height = 25;
            dataGridView1.Size = new System.Drawing.Size(380, 520);
            dataGridView1.TabIndex = 0;
            // 
            // Column1
            // 
            Column1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            Column1.HeaderText = "Name";
            Column1.Name = "Column1";
            Column1.ReadOnly = true;
            Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabel1, toolStripButton1, toolStripButton2 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(380, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.BackColor = System.Drawing.Color.Transparent;
            toolStripLabel1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripLabel1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(62, 22);
            toolStripLabel1.Text = "Realms";
            // 
            // toolStripButton1
            // 
            toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButton1.Image = (System.Drawing.Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new System.Drawing.Size(44, 22);
            toolStripButton1.Text = "Delete";
            // 
            // toolStripButton2
            // 
            toolStripButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButton2.Image = (System.Drawing.Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new System.Drawing.Size(33, 22);
            toolStripButton2.Text = "Add";
            // 
            // panel6
            // 
            panel6.Controls.Add(dataGridView2);
            panel6.Controls.Add(toolStrip2);
            panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            panel6.Location = new System.Drawing.Point(3, 3);
            panel6.Margin = new System.Windows.Forms.Padding(0);
            panel6.Name = "panel6";
            panel6.Size = new System.Drawing.Size(385, 545);
            panel6.TabIndex = 3;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.BackgroundColor = System.Drawing.Color.FromArgb(60, 80, 100);
            dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { dataGridViewTextBoxColumn1 });
            dataGridView2.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView2.Location = new System.Drawing.Point(0, 25);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.ReadOnly = true;
            dataGridView2.RowTemplate.Height = 25;
            dataGridView2.Size = new System.Drawing.Size(385, 520);
            dataGridView2.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn1.HeaderText = "Name";
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            dataGridViewTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // toolStrip2
            // 
            toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabel2, toolStripButton3, toolStripButton4 });
            toolStrip2.Location = new System.Drawing.Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip2.Size = new System.Drawing.Size(385, 25);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.BackColor = System.Drawing.Color.Transparent;
            toolStripLabel2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripLabel2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new System.Drawing.Size(65, 22);
            toolStripLabel2.Text = "Profiles";
            // 
            // toolStripButton3
            // 
            toolStripButton3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButton3.Image = (System.Drawing.Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new System.Drawing.Size(44, 22);
            toolStripButton3.Text = "Delete";
            // 
            // toolStripButton4
            // 
            toolStripButton4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButton4.Image = (System.Drawing.Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new System.Drawing.Size(33, 22);
            toolStripButton4.Text = "Add";
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(panel1);
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new System.Windows.Forms.Padding(3);
            tabPage3.Size = new System.Drawing.Size(781, 551);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Tests";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // LogTab
            // 
            LogTab.Controls.Add(ConsoleView);
            LogTab.Location = new System.Drawing.Point(4, 24);
            LogTab.Name = "LogTab";
            LogTab.Padding = new System.Windows.Forms.Padding(3);
            LogTab.Size = new System.Drawing.Size(781, 551);
            LogTab.TabIndex = 1;
            LogTab.Text = "Log";
            LogTab.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(panel4);
            tabPage2.Location = new System.Drawing.Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Size = new System.Drawing.Size(781, 551);
            tabPage2.TabIndex = 3;
            tabPage2.Text = "Settings";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            panel4.Controls.Add(groupBox1);
            panel4.Dock = System.Windows.Forms.DockStyle.Left;
            panel4.Location = new System.Drawing.Point(0, 0);
            panel4.Name = "panel4";
            panel4.Padding = new System.Windows.Forms.Padding(3);
            panel4.Size = new System.Drawing.Size(300, 551);
            panel4.TabIndex = 1;
            // 
            // panel2
            // 
            panel2.BackColor = System.Drawing.SystemColors.Control;
            panel2.Controls.Add(panel3);
            panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel2.Location = new System.Drawing.Point(0, 579);
            panel2.Name = "panel2";
            panel2.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            panel2.Size = new System.Drawing.Size(789, 48);
            panel2.TabIndex = 3;
            // 
            // panel3
            // 
            panel3.BackColor = System.Drawing.SystemColors.Window;
            panel3.Controls.Add(StartBtn);
            panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            panel3.Location = new System.Drawing.Point(0, 3);
            panel3.Name = "panel3";
            panel3.Padding = new System.Windows.Forms.Padding(6);
            panel3.Size = new System.Drawing.Size(789, 45);
            panel3.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(789, 627);
            Controls.Add(Tabs);
            Controls.Add(panel2);
            Name = "MainForm";
            Text = "Form1";
            panel1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            Tabs.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            tabPage3.ResumeLayout(false);
            LogTab.ResumeLayout(false);
            LogTab.PerformLayout();
            tabPage2.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox ConsoleView;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button SelectGameDirectoryBtn;
        private System.Windows.Forms.TextBox GameDirectoryBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button StartBtn;
        private System.Windows.Forms.Button StartGalaxyInstanseBtn;
        private System.Windows.Forms.Button ScanQuestsBtn;
        private System.Windows.Forms.Button SystemHexMapTestBtn;
        private System.Windows.Forms.Button PathFindingTestBtn;
        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage LogTab;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
    }
}

