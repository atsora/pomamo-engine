// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarms
{
  partial class Page3
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Disposes resources used by the control.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose (bool disposing)
    {
      if (disposing) {
        if (components != null) {
          components.Dispose ();
        }
      }
      base.Dispose (disposing);
    }

    /// <summary>
    /// This method is required for Windows Forms designer support.
    /// Do not change the method contents inside the source code editor. The Forms designer might
    /// not be able to load this method if it was changed manually.
    /// </summary>
    private void InitializeComponent ()
    {
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel ();
      this.layoutEventType = new System.Windows.Forms.TableLayoutPanel ();
      this.labelEventType = new System.Windows.Forms.Label ();
      this.labelName = new System.Windows.Forms.Label ();
      this.textBoxName = new System.Windows.Forms.TextBox ();
      this.checkBoxActivated = new System.Windows.Forms.CheckBox ();
      this.comboBoxEventType = new Lemoine.BaseControls.ComboboxTextValue ();
      this.stackedWidget = new Lemoine.BaseControls.StackedWidget ();
      this.tabPage1 = new System.Windows.Forms.TabPage ();
      this.layoutTab = new System.Windows.Forms.TableLayoutPanel ();
      this.layout2 = new System.Windows.Forms.TableLayoutPanel ();
      this.label2 = new System.Windows.Forms.Label ();
      this.label3 = new System.Windows.Forms.Label ();
      this.treeViewMachines = new Lemoine.DataReferenceControls.DisplayableTreeView ();
      this.checkAllMachines = new System.Windows.Forms.CheckBox ();
      this.listUsers = new ConfiguratorAlarms.EditableEmailList ();
      this.layout1 = new System.Windows.Forms.TableLayoutPanel ();
      this.comboboxInputItems = new Lemoine.BaseControls.ComboboxTextValue ();
      this.labelLevel = new System.Windows.Forms.Label ();
      this.labelFilter = new System.Windows.Forms.Label ();
      this.checkBoxExpiration = new System.Windows.Forms.CheckBox ();
      this.checkBoxStart = new System.Windows.Forms.CheckBox ();
      this.labelDaysInWeek = new System.Windows.Forms.Label ();
      this.dateTimeEndTime = new Lemoine.BaseControls.TimePicker ();
      this.comboBoxLevel = new Lemoine.BaseControls.ComboboxTextValue ();
      this.textBoxFilter = new System.Windows.Forms.TextBox ();
      this.dayInWeekPicker = new Lemoine.BaseControls.DayInWeekPicker ();
      this.dateTimeStartTime = new Lemoine.BaseControls.TimePicker ();
      this.dateTimeEndDate = new Lemoine.BaseControls.DatePicker ();
      this.dateTimeStartDate = new Lemoine.BaseControls.DatePicker ();
      this.periodInDayStart = new Lemoine.BaseControls.TimePicker ();
      this.periodInDayEnd = new Lemoine.BaseControls.TimePicker ();
      this.checkPeriodInDay = new System.Windows.Forms.CheckBox ();
      this.tabPage2 = new System.Windows.Forms.TabPage ();
      this.label5 = new System.Windows.Forms.Label ();
      this.baseLayout.SuspendLayout ();
      this.layoutEventType.SuspendLayout ();
      this.stackedWidget.SuspendLayout ();
      this.tabPage1.SuspendLayout ();
      this.layoutTab.SuspendLayout ();
      this.layout2.SuspendLayout ();
      this.layout1.SuspendLayout ();
      this.tabPage2.SuspendLayout ();
      this.SuspendLayout ();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add (this.layoutEventType, 0, 0);
      this.baseLayout.Controls.Add (this.stackedWidget, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point (0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding (0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 44F));
      this.baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size (370, 313);
      this.baseLayout.TabIndex = 1;
      // 
      // layoutEventType
      // 
      this.layoutEventType.ColumnCount = 3;
      this.layoutEventType.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Absolute, 75F));
      this.layoutEventType.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 50F));
      this.layoutEventType.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 50F));
      this.layoutEventType.Controls.Add (this.labelEventType, 0, 1);
      this.layoutEventType.Controls.Add (this.labelName, 0, 0);
      this.layoutEventType.Controls.Add (this.textBoxName, 1, 0);
      this.layoutEventType.Controls.Add (this.checkBoxActivated, 2, 0);
      this.layoutEventType.Controls.Add (this.comboBoxEventType, 1, 1);
      this.layoutEventType.Dock = System.Windows.Forms.DockStyle.Fill;
      this.layoutEventType.Location = new System.Drawing.Point (0, 0);
      this.layoutEventType.Margin = new System.Windows.Forms.Padding (0);
      this.layoutEventType.Name = "layoutEventType";
      this.layoutEventType.Padding = new System.Windows.Forms.Padding (3, 3, 3, 0);
      this.layoutEventType.RowCount = 2;
      this.layoutEventType.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.layoutEventType.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 14.28571F));
      this.layoutEventType.Size = new System.Drawing.Size (370, 44);
      this.layoutEventType.TabIndex = 6;
      // 
      // label6  Event Type
      // 
      this.labelEventType.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelEventType.Location = new System.Drawing.Point (3, 23);
      this.labelEventType.Margin = new System.Windows.Forms.Padding (0);
      this.labelEventType.Name = "label6";
      this.labelEventType.Size = new System.Drawing.Size (75, 21);
      this.labelEventType.TabIndex = 8;
      this.labelEventType.Text = "Event type";
      this.labelEventType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label1  Name
      // 
      this.labelName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelName.Location = new System.Drawing.Point (3, 3);
      this.labelName.Margin = new System.Windows.Forms.Padding (0);
      this.labelName.Name = "label1";
      this.labelName.Size = new System.Drawing.Size (75, 20);
      this.labelName.TabIndex = 14;
      this.labelName.Text = "Name";
      this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textBoxName
      // 
      this.textBoxName.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBoxName.Location = new System.Drawing.Point (78, 3);
      this.textBoxName.Margin = new System.Windows.Forms.Padding (0);
      this.textBoxName.Name = "textBoxName";
      this.textBoxName.Size = new System.Drawing.Size (144, 20);
      this.textBoxName.TabIndex = 1;
      // 
      // checkBoxActivated
      // 
      this.checkBoxActivated.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxActivated.Location = new System.Drawing.Point (225, 3);
      this.checkBoxActivated.Margin = new System.Windows.Forms.Padding (3, 0, 0, 0);
      this.checkBoxActivated.Name = "checkBoxActivated";
      this.checkBoxActivated.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.checkBoxActivated.Size = new System.Drawing.Size (142, 20);
      this.checkBoxActivated.TabIndex = 10;
      this.checkBoxActivated.Text = "Activated";
      this.checkBoxActivated.UseVisualStyleBackColor = true;
      // 
      // comboBoxEventType
      // 
      this.layoutEventType.SetColumnSpan (this.comboBoxEventType, 2);
      this.comboBoxEventType.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboBoxEventType.Location = new System.Drawing.Point (78, 23);
      this.comboBoxEventType.Margin = new System.Windows.Forms.Padding (0);
      this.comboBoxEventType.Name = "comboBoxEventType";
      this.comboBoxEventType.Size = new System.Drawing.Size (289, 21);
      this.comboBoxEventType.Sorted = true;
      this.comboBoxEventType.TabIndex = 19;
      this.comboBoxEventType.ItemChanged += new System.Action<string, object> (this.ComboBoxEventTypeItemChanged);
      this.comboBoxEventType.TextChanged += new System.Action<string> (this.ComboBoxEventTypeTextChanged);
      // 
      // stackedWidget
      // 
      this.stackedWidget.Controls.Add (this.tabPage1);
      this.stackedWidget.Controls.Add (this.tabPage2);
      this.stackedWidget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.stackedWidget.Location = new System.Drawing.Point (0, 44);
      this.stackedWidget.Margin = new System.Windows.Forms.Padding (0);
      this.stackedWidget.Name = "stackedWidget";
      this.stackedWidget.SelectedIndex = 0;
      this.stackedWidget.Size = new System.Drawing.Size (370, 269);
      this.stackedWidget.TabIndex = 13;
      // 
      // tabPage1
      // 
      this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage1.Controls.Add (this.layoutTab);
      this.tabPage1.Location = new System.Drawing.Point (4, 22);
      this.tabPage1.Margin = new System.Windows.Forms.Padding (0);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding (3);
      this.tabPage1.Size = new System.Drawing.Size (362, 243);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      // 
      // layoutTab
      // 
      this.layoutTab.ColumnCount = 1;
      this.layoutTab.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 100F));
      this.layoutTab.Controls.Add (this.layout2, 0, 1);
      this.layoutTab.Controls.Add (this.layout1, 0, 0);
      this.layoutTab.Dock = System.Windows.Forms.DockStyle.Fill;
      this.layoutTab.Location = new System.Drawing.Point (3, 3);
      this.layoutTab.Name = "layoutTab";
      this.layoutTab.RowCount = 2;
      this.layoutTab.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 135F));
      this.layoutTab.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 100F));
      this.layoutTab.Size = new System.Drawing.Size (356, 237);
      this.layoutTab.TabIndex = 0;
      // 
      // layout2
      // 
      this.layout2.ColumnCount = 2;
      this.layout2.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 50F));
      this.layout2.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 50F));
      this.layout2.Controls.Add (this.label2, 0, 0);
      this.layout2.Controls.Add (this.label3, 1, 0);
      this.layout2.Controls.Add (this.treeViewMachines, 0, 2);
      this.layout2.Controls.Add (this.checkAllMachines, 0, 1);
      this.layout2.Controls.Add (this.listUsers, 1, 1);
      this.layout2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.layout2.Location = new System.Drawing.Point (0, 135);
      this.layout2.Margin = new System.Windows.Forms.Padding (0);
      this.layout2.Name = "layout2";
      this.layout2.RowCount = 3;
      this.layout2.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 24F));
      this.layout2.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 24F));
      this.layout2.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 100F));
      this.layout2.Size = new System.Drawing.Size (356, 102);
      this.layout2.TabIndex = 12;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point (0, 0);
      this.label2.Margin = new System.Windows.Forms.Padding (0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size (178, 24);
      this.label2.TabIndex = 0;
      this.label2.Text = "Machines";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point (181, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size (172, 24);
      this.label3.TabIndex = 1;
      this.label3.Text = "Users";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // treeViewMachines
      // 
      this.treeViewMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.treeViewMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeViewMachines.Location = new System.Drawing.Point (0, 48);
      this.treeViewMachines.Margin = new System.Windows.Forms.Padding (0);
      this.treeViewMachines.MultiSelection = true;
      this.treeViewMachines.Name = "treeViewMachines";
      this.treeViewMachines.Size = new System.Drawing.Size (178, 54);
      this.treeViewMachines.TabIndex = 10;
      // 
      // checkAllMachines
      // 
      this.checkAllMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkAllMachines.Location = new System.Drawing.Point (3, 24);
      this.checkAllMachines.Margin = new System.Windows.Forms.Padding (3, 0, 0, 0);
      this.checkAllMachines.Name = "checkAllMachines";
      this.checkAllMachines.Size = new System.Drawing.Size (175, 24);
      this.checkAllMachines.TabIndex = 12;
      this.checkAllMachines.Text = "All";
      this.checkAllMachines.UseVisualStyleBackColor = true;
      this.checkAllMachines.CheckedChanged += new System.EventHandler (this.CheckAllMachinesCheckedChanged);
      // 
      // listUsers
      // 
      this.listUsers.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listUsers.Location = new System.Drawing.Point (181, 24);
      this.listUsers.Margin = new System.Windows.Forms.Padding (3, 0, 0, 0);
      this.listUsers.Name = "listUsers";
      this.layout2.SetRowSpan (this.listUsers, 2);
      this.listUsers.Size = new System.Drawing.Size (175, 78);
      this.listUsers.TabIndex = 11;
      // 
      // layout1
      // 
      this.layout1.ColumnCount = 3;
      this.layout1.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Absolute, 94F));
      this.layout1.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 50F));
      this.layout1.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 50F));

      this.layout1.Controls.Add (this.labelLevel, 0, 0);
      this.layout1.Controls.Add (this.comboBoxLevel, 1, 0);
      this.layout1.Controls.Add (this.comboboxInputItems, 1, 1);
      this.layout1.Controls.Add (this.labelFilter, 0, 1);
      this.layout1.Controls.Add (this.textBoxFilter, 1, 1);
      this.layout1.Controls.Add (this.checkBoxExpiration, 0, 5);
      this.layout1.Controls.Add (this.checkBoxStart, 0, 4);
      this.layout1.Controls.Add (this.labelDaysInWeek, 0, 2);
      this.layout1.Controls.Add (this.dateTimeEndTime, 2, 5);

      this.layout1.Controls.Add (this.dayInWeekPicker, 1, 2);
      this.layout1.Controls.Add (this.dateTimeStartTime, 2, 4);
      this.layout1.Controls.Add (this.dateTimeEndDate, 1, 5);
      this.layout1.Controls.Add (this.dateTimeStartDate, 1, 4);
      this.layout1.Controls.Add (this.periodInDayStart, 1, 3);
      this.layout1.Controls.Add (this.periodInDayEnd, 2, 3);
      this.layout1.Controls.Add (this.checkPeriodInDay, 0, 3);
      this.layout1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.layout1.Location = new System.Drawing.Point (0, 0);
      this.layout1.Margin = new System.Windows.Forms.Padding (0);
      this.layout1.Name = "layout1";
      this.layout1.RowCount = 6;
      this.layout1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.layout1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.layout1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.layout1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.layout1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.layout1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 16.66667F));
      this.layout1.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 20F));
      this.layout1.Size = new System.Drawing.Size (356, 135);
      this.layout1.TabIndex = 13;
      // 
      // comboboxInputItems
      // 
      this.layout1.SetColumnSpan (this.comboboxInputItems, 2);
      this.comboboxInputItems.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboboxInputItems.Location = new System.Drawing.Point (94, 22);
      this.comboboxInputItems.Margin = new System.Windows.Forms.Padding (0);
      this.comboboxInputItems.Name = "comboboxInputItems";
      this.comboboxInputItems.Size = new System.Drawing.Size (262, 22);
      this.comboboxInputItems.Sorted = true;
      this.comboboxInputItems.TabIndex = 25;
      //      this.comboboxInputItems.Hide();   // FRC

      // 
      // textBoxFilter   //FRC
      // 
      this.layout1.SetColumnSpan (this.textBoxFilter, 2);
      this.textBoxFilter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBoxFilter.Location = new System.Drawing.Point (0, 22);
      this.textBoxFilter.Margin = new System.Windows.Forms.Padding (0);
      this.textBoxFilter.Name = "textBoxFilter";
      this.textBoxFilter.Size = new System.Drawing.Size (262, 22);
      this.textBoxFilter.Text = "";
      this.textBoxFilter.TabIndex = 25;
      this.textBoxFilter.TextChanged += new System.EventHandler (this.textBoxFilter_TextChanged);
      this.textBoxFilter.Hide ();
      // 
      // label  level
      // 
      this.labelLevel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelLevel.Location = new System.Drawing.Point (0, 0);
      this.labelLevel.Margin = new System.Windows.Forms.Padding (0);
      this.labelLevel.Name = "label4";
      this.labelLevel.Size = new System.Drawing.Size (94, 22);
      this.labelLevel.TabIndex = 0;
      this.labelLevel.Text = "Level";
      this.labelLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      //this.label4.Hide ();
      // 
      // Message filter label // FRC
      //
      //
      this.labelFilter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelFilter.Location = new System.Drawing.Point (0, 0);
      this.labelFilter.Margin = new System.Windows.Forms.Padding (0);
      this.labelFilter.Name = "labelFilter";
      this.labelFilter.Size = new System.Drawing.Size (75, 21);
      this.labelFilter.TabIndex = 0;
      this.labelFilter.Text = "Filter";
      this.labelFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.labelFilter.Hide ();
      // 
      // checkBoxExpiration
      // 
      this.checkBoxExpiration.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxExpiration.Location = new System.Drawing.Point (3, 110);
      this.checkBoxExpiration.Margin = new System.Windows.Forms.Padding (3, 0, 0, 0);
      this.checkBoxExpiration.Name = "checkBoxExpiration";
      this.checkBoxExpiration.Size = new System.Drawing.Size (91, 25);
      this.checkBoxExpiration.TabIndex = 3;
      this.checkBoxExpiration.Text = "Expiration";
      this.checkBoxExpiration.UseVisualStyleBackColor = true;
      this.checkBoxExpiration.CheckedChanged += new System.EventHandler (this.CheckBoxExpirationCheckedChanged);
      // 
      // checkBoxStart
      // 
      this.checkBoxStart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxStart.Location = new System.Drawing.Point (3, 88);
      this.checkBoxStart.Margin = new System.Windows.Forms.Padding (3, 0, 0, 0);
      this.checkBoxStart.Name = "checkBoxStart";
      this.checkBoxStart.Size = new System.Drawing.Size (91, 22);
      this.checkBoxStart.TabIndex = 2;
      this.checkBoxStart.Text = "Start date";
      this.checkBoxStart.UseVisualStyleBackColor = true;
      this.checkBoxStart.CheckedChanged += new System.EventHandler (this.CheckBoxStartCheckedChanged);
      // 
      // label7
      // 
      this.labelDaysInWeek.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelDaysInWeek.Location = new System.Drawing.Point (0, 44);
      this.labelDaysInWeek.Margin = new System.Windows.Forms.Padding (0);
      this.labelDaysInWeek.Name = "label7";
      this.labelDaysInWeek.Size = new System.Drawing.Size (94, 22);
      this.labelDaysInWeek.TabIndex = 13;
      this.labelDaysInWeek.Text = "Days in week";
      this.labelDaysInWeek.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // dateTimeEndTime
      // 
      this.dateTimeEndTime.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateTimeEndTime.Enabled = false;
      this.dateTimeEndTime.Location = new System.Drawing.Point (225, 110);
      this.dateTimeEndTime.Margin = new System.Windows.Forms.Padding (0);
      this.dateTimeEndTime.Name = "dateTimeEndTime";
      this.dateTimeEndTime.Size = new System.Drawing.Size (131, 20);
      this.dateTimeEndTime.TabIndex = 18;
      this.dateTimeEndTime.Time = System.TimeSpan.Parse ("18:00:00");
      this.dateTimeEndTime.Value = new System.DateTime (2015, 8, 20, 18, 0, 0, 0);
      // 
      // comboBoxLevel
      // 
      this.layout1.SetColumnSpan (this.comboBoxLevel, 2);
      this.comboBoxLevel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboBoxLevel.Location = new System.Drawing.Point (94, 0);
      this.comboBoxLevel.Margin = new System.Windows.Forms.Padding (0);
      this.comboBoxLevel.Name = "comboBoxLevel";
      this.comboBoxLevel.Size = new System.Drawing.Size (262, 22);
      this.comboBoxLevel.Sorted = true;
      this.comboBoxLevel.TabIndex = 20;

      // 
      // dayInWeekPicker
      // 
      this.layout1.SetColumnSpan (this.dayInWeekPicker, 2);
      this.dayInWeekPicker.DaysInWeek = 127;
      this.dayInWeekPicker.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dayInWeekPicker.Location = new System.Drawing.Point (94, 44);
      this.dayInWeekPicker.Margin = new System.Windows.Forms.Padding (0);
      this.dayInWeekPicker.MinimumSize = new System.Drawing.Size (155, 20);
      this.dayInWeekPicker.Name = "dayInWeekPicker";
      this.dayInWeekPicker.Size = new System.Drawing.Size (262, 22);
      this.dayInWeekPicker.TabIndex = 12;
      // 
      // dateTimeStartTime
      // 
      this.dateTimeStartTime.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateTimeStartTime.Enabled = false;
      this.dateTimeStartTime.Location = new System.Drawing.Point (225, 88);
      this.dateTimeStartTime.Margin = new System.Windows.Forms.Padding (0);
      this.dateTimeStartTime.Name = "dateTimeStartTime";
      this.dateTimeStartTime.Size = new System.Drawing.Size (131, 20);
      this.dateTimeStartTime.TabIndex = 17;
      this.dateTimeStartTime.Time = System.TimeSpan.Parse ("08:00:00");
      this.dateTimeStartTime.Value = new System.DateTime (2015, 8, 20, 8, 0, 0, 0);
      // 
      // dateTimeEndDate
      // 
      this.dateTimeEndDate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateTimeEndDate.Enabled = false;
      this.dateTimeEndDate.Location = new System.Drawing.Point (94, 110);
      this.dateTimeEndDate.Margin = new System.Windows.Forms.Padding (0, 0, 3, 0);
      this.dateTimeEndDate.Name = "dateTimeEndDate";
      this.dateTimeEndDate.Size = new System.Drawing.Size (128, 20);
      this.dateTimeEndDate.TabIndex = 16;
      // 
      // dateTimeStartDate
      // 
      this.dateTimeStartDate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.dateTimeStartDate.Enabled = false;
      this.dateTimeStartDate.Location = new System.Drawing.Point (94, 88);
      this.dateTimeStartDate.Margin = new System.Windows.Forms.Padding (0, 0, 3, 0);
      this.dateTimeStartDate.Name = "dateTimeStartDate";
      this.dateTimeStartDate.Size = new System.Drawing.Size (128, 20);
      this.dateTimeStartDate.TabIndex = 15;
      // 
      // periodInDayStart
      // 
      this.periodInDayStart.Dock = System.Windows.Forms.DockStyle.Fill;
      this.periodInDayStart.Enabled = false;
      this.periodInDayStart.Location = new System.Drawing.Point (94, 66);
      this.periodInDayStart.Margin = new System.Windows.Forms.Padding (0, 0, 3, 0);
      this.periodInDayStart.Name = "periodInDayStart";
      this.periodInDayStart.Size = new System.Drawing.Size (128, 20);
      this.periodInDayStart.TabIndex = 22;
      this.periodInDayStart.Time = System.TimeSpan.Parse ("08:00:00");
      this.periodInDayStart.Value = new System.DateTime (2017, 6, 13, 8, 0, 0, 0);
      // 
      // periodInDayEnd
      // 
      this.periodInDayEnd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.periodInDayEnd.Enabled = false;
      this.periodInDayEnd.Location = new System.Drawing.Point (225, 66);
      this.periodInDayEnd.Margin = new System.Windows.Forms.Padding (0);
      this.periodInDayEnd.Name = "periodInDayEnd";
      this.periodInDayEnd.Size = new System.Drawing.Size (131, 20);
      this.periodInDayEnd.TabIndex = 23;
      this.periodInDayEnd.Time = System.TimeSpan.Parse ("18:00:00");
      this.periodInDayEnd.Value = new System.DateTime (2017, 6, 13, 18, 0, 0, 0);
      // 
      // checkPeriodInDay
      // 
      this.checkPeriodInDay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkPeriodInDay.Location = new System.Drawing.Point (3, 66);
      this.checkPeriodInDay.Margin = new System.Windows.Forms.Padding (3, 0, 0, 0);
      this.checkPeriodInDay.Name = "checkPeriodInDay";
      this.checkPeriodInDay.Size = new System.Drawing.Size (91, 22);
      this.checkPeriodInDay.TabIndex = 24;
      this.checkPeriodInDay.Text = "Period in day";
      this.checkPeriodInDay.UseVisualStyleBackColor = true;
      this.checkPeriodInDay.CheckedChanged += new System.EventHandler (this.CheckPeriodInDayCheckedChanged);
      // 
      // tabPage2
      // 
      this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage2.Controls.Add (this.label5);
      this.tabPage2.Location = new System.Drawing.Point (4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding (3);
      this.tabPage2.Size = new System.Drawing.Size (362, 243);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Font = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label5.ForeColor = System.Drawing.SystemColors.ControlDark;
      this.label5.Location = new System.Drawing.Point (3, 3);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size (356, 237);
      this.label5.TabIndex = 0;
      this.label5.Text = "No events have been configured for this event type yet,\r\nthis is thus not possibl" +
    "e to connect users.\r\n\r\nPlease run a wizard to configure events first.";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF (6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add (this.baseLayout);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size (370, 313);
      this.baseLayout.ResumeLayout (false);
      this.layoutEventType.ResumeLayout (false);
      this.layoutEventType.PerformLayout ();
      this.stackedWidget.ResumeLayout (false);
      this.tabPage1.ResumeLayout (false);
      this.layoutTab.ResumeLayout (false);
      this.layout2.ResumeLayout (false);
      this.layout1.ResumeLayout (false);
      this.tabPage2.ResumeLayout (false);
      this.ResumeLayout (false);

    }
    private System.Windows.Forms.Label labelName;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.TextBox textBoxName;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TableLayoutPanel layoutEventType;
    private System.Windows.Forms.CheckBox checkBoxExpiration;
    private Lemoine.BaseControls.DatePicker dateTimeEndDate;
    private Lemoine.BaseControls.TimePicker dateTimeEndTime;
    private Lemoine.BaseControls.TimePicker dateTimeStartTime;
    private Lemoine.BaseControls.DatePicker dateTimeStartDate;
    private System.Windows.Forms.CheckBox checkBoxStart;
    private Lemoine.BaseControls.ComboboxTextValue comboBoxLevel;
    private System.Windows.Forms.TextBox textBoxFilter;
    private System.Windows.Forms.Label labelLevel;
    private System.Windows.Forms.Label labelEventType;
    private System.Windows.Forms.Label labelFilter;
    private Lemoine.BaseControls.ComboboxTextValue comboBoxEventType;
    private System.Windows.Forms.CheckBox checkBoxActivated;
    private System.Windows.Forms.Label labelDaysInWeek;
    private Lemoine.BaseControls.DayInWeekPicker dayInWeekPicker;
    private Lemoine.DataReferenceControls.DisplayableTreeView treeViewMachines;
    private ConfiguratorAlarms.EditableEmailList listUsers;
    private System.Windows.Forms.TableLayoutPanel layout2;
    private Lemoine.BaseControls.StackedWidget stackedWidget;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TableLayoutPanel layoutTab;
    private System.Windows.Forms.TableLayoutPanel layout1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Label label5;
    private Lemoine.BaseControls.TimePicker periodInDayStart;
    private Lemoine.BaseControls.TimePicker periodInDayEnd;
    private System.Windows.Forms.CheckBox checkPeriodInDay;
    private System.Windows.Forms.CheckBox checkAllMachines;
    private Lemoine.BaseControls.ComboboxTextValue comboboxInputItems;
  }
}
