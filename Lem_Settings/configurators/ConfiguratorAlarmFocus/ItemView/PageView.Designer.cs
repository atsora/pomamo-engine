// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarmFocus
{
  partial class PageView
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
    /// <summary>
    /// Disposes resources used by the control.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (components != null) {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }
    
    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PageView));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.comboCnc = new Lemoine.BaseControls.ComboboxTextValue();
      this.comboFocus = new Lemoine.BaseControls.ComboboxTextValue();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.layoutBottom = new System.Windows.Forms.TableLayoutPanel();
      this.buttonPreviousDay = new System.Windows.Forms.Button();
      this.buttonNextDay = new System.Windows.Forms.Button();
      this.labelDate = new System.Windows.Forms.Label();
      this.stacked = new Lemoine.BaseControls.StackedWidget();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.dataGrid = new System.Windows.Forms.DataGridView();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.label3 = new System.Windows.Forms.Label();
      this.ColumnCNC = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Machine = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColumnNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColumnProperties = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColumnMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColumnSeverity = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColumnPeriod = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.LastSeen = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.baseLayout.SuspendLayout();
      this.layoutBottom.SuspendLayout();
      this.stacked.SuspendLayout();
      this.tabPage1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 33F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 47F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.baseLayout.Controls.Add(this.comboCnc, 1, 0);
      this.baseLayout.Controls.Add(this.comboFocus, 3, 0);
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 2, 0);
      this.baseLayout.Controls.Add(this.layoutBottom, 0, 2);
      this.baseLayout.Controls.Add(this.stacked, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // comboCnc
      // 
      this.comboCnc.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboCnc.Location = new System.Drawing.Point(33, 0);
      this.comboCnc.Margin = new System.Windows.Forms.Padding(0);
      this.comboCnc.Name = "comboCnc";
      this.comboCnc.Size = new System.Drawing.Size(145, 23);
      this.comboCnc.Sorted = true;
      this.comboCnc.TabIndex = 0;
      this.comboCnc.ItemChanged += new System.Action<string, object>(this.ComboCncItemChanged);
      // 
      // comboFocus
      // 
      this.comboFocus.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboFocus.Location = new System.Drawing.Point(225, 0);
      this.comboFocus.Margin = new System.Windows.Forms.Padding(0);
      this.comboFocus.Name = "comboFocus";
      this.comboFocus.Size = new System.Drawing.Size(145, 23);
      this.comboFocus.TabIndex = 1;
      this.comboFocus.ItemChanged += new System.Action<string, object>(this.ComboFocusItemChanged);
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 2);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(30, 21);
      this.label1.TabIndex = 2;
      this.label1.Text = "CNC";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(181, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 2);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(41, 21);
      this.label2.TabIndex = 3;
      this.label2.Text = "Focus";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // layoutBottom
      // 
      this.layoutBottom.ColumnCount = 3;
      this.baseLayout.SetColumnSpan(this.layoutBottom, 4);
      this.layoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.layoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.layoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.layoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.layoutBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.layoutBottom.Controls.Add(this.buttonPreviousDay, 0, 0);
      this.layoutBottom.Controls.Add(this.buttonNextDay, 2, 0);
      this.layoutBottom.Controls.Add(this.labelDate, 1, 0);
      this.layoutBottom.Dock = System.Windows.Forms.DockStyle.Fill;
      this.layoutBottom.Location = new System.Drawing.Point(0, 265);
      this.layoutBottom.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.layoutBottom.Name = "layoutBottom";
      this.layoutBottom.RowCount = 1;
      this.layoutBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.layoutBottom.Size = new System.Drawing.Size(370, 25);
      this.layoutBottom.TabIndex = 5;
      // 
      // buttonPreviousDay
      // 
      this.buttonPreviousDay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonPreviousDay.Image = ((System.Drawing.Image)(resources.GetObject("buttonPreviousDay.Image")));
      this.buttonPreviousDay.Location = new System.Drawing.Point(0, 0);
      this.buttonPreviousDay.Margin = new System.Windows.Forms.Padding(0);
      this.buttonPreviousDay.Name = "buttonPreviousDay";
      this.buttonPreviousDay.Size = new System.Drawing.Size(25, 25);
      this.buttonPreviousDay.TabIndex = 0;
      this.buttonPreviousDay.UseVisualStyleBackColor = true;
      this.buttonPreviousDay.Click += new System.EventHandler(this.ButtonPreviousDayClick);
      // 
      // buttonNextDay
      // 
      this.buttonNextDay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonNextDay.Image = ((System.Drawing.Image)(resources.GetObject("buttonNextDay.Image")));
      this.buttonNextDay.Location = new System.Drawing.Point(345, 0);
      this.buttonNextDay.Margin = new System.Windows.Forms.Padding(0);
      this.buttonNextDay.Name = "buttonNextDay";
      this.buttonNextDay.Size = new System.Drawing.Size(25, 25);
      this.buttonNextDay.TabIndex = 1;
      this.buttonNextDay.UseVisualStyleBackColor = true;
      this.buttonNextDay.Click += new System.EventHandler(this.ButtonNextDayClick);
      // 
      // labelDate
      // 
      this.labelDate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDate.Location = new System.Drawing.Point(25, 0);
      this.labelDate.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelDate.Name = "labelDate";
      this.labelDate.Size = new System.Drawing.Size(317, 25);
      this.labelDate.TabIndex = 3;
      this.labelDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // stacked
      // 
      this.baseLayout.SetColumnSpan(this.stacked, 4);
      this.stacked.Controls.Add(this.tabPage1);
      this.stacked.Controls.Add(this.tabPage2);
      this.stacked.Dock = System.Windows.Forms.DockStyle.Fill;
      this.stacked.Location = new System.Drawing.Point(0, 23);
      this.stacked.Margin = new System.Windows.Forms.Padding(0);
      this.stacked.Name = "stacked";
      this.stacked.SelectedIndex = 0;
      this.stacked.Size = new System.Drawing.Size(370, 239);
      this.stacked.TabIndex = 6;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.dataGrid);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(362, 213);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // dataGrid
      // 
      this.dataGrid.AllowUserToAddRows = false;
      this.dataGrid.AllowUserToDeleteRows = false;
      this.dataGrid.AllowUserToOrderColumns = true;
      this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
      this.ColumnCNC,
      this.Machine,
      this.ColumnType,
      this.ColumnNumber,
      this.ColumnProperties,
      this.ColumnMessage,
      this.ColumnSeverity,
      this.ColumnPeriod,
      this.LastSeen});
      this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dataGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
      this.dataGrid.Location = new System.Drawing.Point(0, 0);
      this.dataGrid.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
      this.dataGrid.MultiSelect = false;
      this.dataGrid.Name = "dataGrid";
      this.dataGrid.ReadOnly = true;
      this.dataGrid.RowHeadersVisible = false;
      this.dataGrid.Size = new System.Drawing.Size(362, 213);
      this.dataGrid.TabIndex = 4;
      // 
      // tabPage2
      // 
      this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage2.Controls.Add(this.label3);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(362, 213);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label3.Location = new System.Drawing.Point(3, 3);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(356, 207);
      this.label3.TabIndex = 0;
      this.label3.Text = "No data";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // ColumnCNC
      // 
      this.ColumnCNC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColumnCNC.HeaderText = "CNC";
      this.ColumnCNC.Name = "ColumnCNC";
      this.ColumnCNC.ReadOnly = true;
      this.ColumnCNC.Width = 54;
      // 
      // Machine
      // 
      this.Machine.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.Machine.HeaderText = "Machine";
      this.Machine.Name = "Machine";
      this.Machine.ReadOnly = true;
      this.Machine.Width = 73;
      // 
      // ColumnType
      // 
      this.ColumnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColumnType.DataPropertyName = "Type";
      this.ColumnType.HeaderText = "Type";
      this.ColumnType.Name = "ColumnType";
      this.ColumnType.ReadOnly = true;
      this.ColumnType.Width = 56;
      // 
      // ColumnNumber
      // 
      this.ColumnNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColumnNumber.DataPropertyName = "Number";
      this.ColumnNumber.HeaderText = "Number";
      this.ColumnNumber.Name = "ColumnNumber";
      this.ColumnNumber.ReadOnly = true;
      this.ColumnNumber.Width = 69;
      // 
      // ColumnProperties
      // 
      this.ColumnProperties.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColumnProperties.DataPropertyName = "Properties";
      this.ColumnProperties.HeaderText = "Properties";
      this.ColumnProperties.Name = "ColumnProperties";
      this.ColumnProperties.ReadOnly = true;
      this.ColumnProperties.Width = 79;
      // 
      // ColumnMessage
      // 
      this.ColumnMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColumnMessage.DataPropertyName = "Message";
      this.ColumnMessage.HeaderText = "Message";
      this.ColumnMessage.Name = "ColumnMessage";
      this.ColumnMessage.ReadOnly = true;
      this.ColumnMessage.Width = 75;
      // 
      // ColumnSeverity
      // 
      this.ColumnSeverity.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColumnSeverity.DataPropertyName = "Severity";
      this.ColumnSeverity.HeaderText = "Severity";
      this.ColumnSeverity.Name = "ColumnSeverity";
      this.ColumnSeverity.ReadOnly = true;
      this.ColumnSeverity.Width = 70;
      // 
      // ColumnPeriod
      // 
      this.ColumnPeriod.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.ColumnPeriod.HeaderText = "Period";
      this.ColumnPeriod.Name = "ColumnPeriod";
      this.ColumnPeriod.ReadOnly = true;
      this.ColumnPeriod.Width = 62;
      // 
      // LastSeen
      // 
      this.LastSeen.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
      this.LastSeen.HeaderText = "Last seen";
      this.LastSeen.Name = "LastSeen";
      this.LastSeen.ReadOnly = true;
      this.LastSeen.Width = 78;
      // 
      // PageView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PageView";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.layoutBottom.ResumeLayout(false);
      this.stacked.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
      this.tabPage2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.ComboboxTextValue comboCnc;
    private Lemoine.BaseControls.ComboboxTextValue comboFocus;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.DataGridView dataGrid;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnType;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnNumber;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnProperties;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMessage;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnSeverity;
    private System.Windows.Forms.TableLayoutPanel layoutBottom;
    private System.Windows.Forms.Button buttonPreviousDay;
    private System.Windows.Forms.Button buttonNextDay;
    private System.Windows.Forms.Label labelDate;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnCNC;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPeriod;
    private Lemoine.BaseControls.StackedWidget stacked;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.DataGridViewTextBoxColumn LastSeen;
    private System.Windows.Forms.DataGridViewTextBoxColumn Machine;
  }
}
