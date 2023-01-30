// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarms
{
  partial class Page2
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.comboboxInputItems = new Lemoine.BaseControls.ComboboxTextValue();
      this.labelMachines = new System.Windows.Forms.Label();
      this.labelEventType = new System.Windows.Forms.Label();
      this.labelUsers = new System.Windows.Forms.Label();
      this.checkBoxUsersLinked = new System.Windows.Forms.CheckBox();
      this.checkBoxMachinesLinked = new System.Windows.Forms.CheckBox();
      this.treeViewMachines = new Lemoine.DataReferenceControls.DisplayableTreeView();
      this.label1 = new System.Windows.Forms.Label();
      this.comboBoxEventType = new Lemoine.BaseControls.ComboboxTextValue();
      this.comboBoxEventLevel = new Lemoine.BaseControls.ComboboxTextValue();
      this.listUsers = new ConfiguratorAlarms.ColorCheckedListBox();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.comboboxInputItems, 1, 2);
      this.baseLayout.Controls.Add(this.labelMachines, 0, 3);
      this.baseLayout.Controls.Add(this.labelEventType, 0, 0);
      this.baseLayout.Controls.Add(this.labelUsers, 1, 3);
      this.baseLayout.Controls.Add(this.checkBoxUsersLinked, 1, 5);
      this.baseLayout.Controls.Add(this.checkBoxMachinesLinked, 0, 5);
      this.baseLayout.Controls.Add(this.treeViewMachines, 0, 4);
      this.baseLayout.Controls.Add(this.label1, 0, 1);
      this.baseLayout.Controls.Add(this.comboBoxEventType, 1, 0);
      this.baseLayout.Controls.Add(this.comboBoxEventLevel, 1, 1);
      this.baseLayout.Controls.Add(this.listUsers, 1, 4);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 1;
      // 
      // comboboxInputItems
      // 
      this.comboboxInputItems.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboboxInputItems.Location = new System.Drawing.Point(188, 44);
      this.comboboxInputItems.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.comboboxInputItems.Name = "comboboxInputItems";
      this.comboboxInputItems.Size = new System.Drawing.Size(182, 22);
      this.comboboxInputItems.Sorted = true;
      this.comboboxInputItems.TabIndex = 25;
      // 
      // labelMachines
      // 
      this.labelMachines.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
      | System.Windows.Forms.AnchorStyles.Left) 
      | System.Windows.Forms.AnchorStyles.Right)));
      this.labelMachines.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelMachines.Location = new System.Drawing.Point(0, 66);
      this.labelMachines.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelMachines.Name = "labelMachines";
      this.labelMachines.Size = new System.Drawing.Size(182, 22);
      this.labelMachines.TabIndex = 0;
      this.labelMachines.Text = "Machines";
      this.labelMachines.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelEventType
      // 
      this.labelEventType.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
      | System.Windows.Forms.AnchorStyles.Left) 
      | System.Windows.Forms.AnchorStyles.Right)));
      this.labelEventType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelEventType.Location = new System.Drawing.Point(0, 0);
      this.labelEventType.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelEventType.Name = "labelEventType";
      this.labelEventType.Size = new System.Drawing.Size(182, 22);
      this.labelEventType.TabIndex = 1;
      this.labelEventType.Text = "Event type";
      this.labelEventType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelUsers
      // 
      this.labelUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
      | System.Windows.Forms.AnchorStyles.Left) 
      | System.Windows.Forms.AnchorStyles.Right)));
      this.labelUsers.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelUsers.Location = new System.Drawing.Point(188, 66);
      this.labelUsers.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.labelUsers.Name = "labelUsers";
      this.labelUsers.Size = new System.Drawing.Size(182, 22);
      this.labelUsers.TabIndex = 2;
      this.labelUsers.Text = "Users";
      this.labelUsers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkBoxUsersLinked
      // 
      this.checkBoxUsersLinked.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxUsersLinked.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxUsersLinked.Location = new System.Drawing.Point(188, 272);
      this.checkBoxUsersLinked.Margin = new System.Windows.Forms.Padding(3, 2, 0, 0);
      this.checkBoxUsersLinked.Name = "checkBoxUsersLinked";
      this.checkBoxUsersLinked.Size = new System.Drawing.Size(182, 18);
      this.checkBoxUsersLinked.TabIndex = 18;
      this.checkBoxUsersLinked.Text = "hide users with no alarms";
      this.checkBoxUsersLinked.UseVisualStyleBackColor = true;
      this.checkBoxUsersLinked.CheckedChanged += new System.EventHandler(this.CheckBoxUsersLinkedCheckedChanged);
      // 
      // checkBoxMachinesLinked
      // 
      this.checkBoxMachinesLinked.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkBoxMachinesLinked.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.checkBoxMachinesLinked.Location = new System.Drawing.Point(0, 272);
      this.checkBoxMachinesLinked.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.checkBoxMachinesLinked.Name = "checkBoxMachinesLinked";
      this.checkBoxMachinesLinked.Size = new System.Drawing.Size(185, 18);
      this.checkBoxMachinesLinked.TabIndex = 19;
      this.checkBoxMachinesLinked.Text = "hide machines with no alarms";
      this.checkBoxMachinesLinked.UseVisualStyleBackColor = true;
      this.checkBoxMachinesLinked.CheckedChanged += new System.EventHandler(this.CheckBoxMachinesLinkedCheckedChanged);
      // 
      // treeViewMachines
      // 
      this.treeViewMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.treeViewMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeViewMachines.Location = new System.Drawing.Point(0, 88);
      this.treeViewMachines.Margin = new System.Windows.Forms.Padding(0);
      this.treeViewMachines.MultiSelection = true;
      this.treeViewMachines.Name = "treeViewMachines";
      this.treeViewMachines.Size = new System.Drawing.Size(185, 182);
      this.treeViewMachines.TabIndex = 20;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 22);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(182, 22);
      this.label1.TabIndex = 21;
      this.label1.Text = "Level";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // comboBoxEventType
      // 
      this.comboBoxEventType.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboBoxEventType.Location = new System.Drawing.Point(188, 0);
      this.comboBoxEventType.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.comboBoxEventType.Name = "comboBoxEventType";
      this.comboBoxEventType.Size = new System.Drawing.Size(182, 22);
      this.comboBoxEventType.Sorted = true;
      this.comboBoxEventType.TabIndex = 22;
      this.comboBoxEventType.ItemChanged += new System.Action<string, object>(this.ComboBoxEventTypeItemChanged);
      // 
      // comboBoxEventLevel
      // 
      this.comboBoxEventLevel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboBoxEventLevel.Location = new System.Drawing.Point(188, 22);
      this.comboBoxEventLevel.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.comboBoxEventLevel.Name = "comboBoxEventLevel";
      this.comboBoxEventLevel.Size = new System.Drawing.Size(182, 22);
      this.comboBoxEventLevel.Sorted = true;
      this.comboBoxEventLevel.TabIndex = 23;
      // 
      // listUsers
      // 
      this.listUsers.AlarmManager = null;
      this.listUsers.CheckOnClick = true;
      this.listUsers.Datatype = "";
      this.listUsers.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listUsers.FormattingEnabled = true;
      this.listUsers.IntegralHeight = false;
      this.listUsers.Location = new System.Drawing.Point(188, 88);
      this.listUsers.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.listUsers.Name = "listUsers";
      this.listUsers.Size = new System.Drawing.Size(182, 182);
      this.listUsers.TabIndex = 24;
      // 
      // Page2
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.Name = "Page2";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label labelMachines;
    private System.Windows.Forms.Label labelEventType;
    private System.Windows.Forms.Label labelUsers;
    private System.Windows.Forms.CheckBox checkBoxUsersLinked;
    private System.Windows.Forms.CheckBox checkBoxMachinesLinked;
    private Lemoine.DataReferenceControls.DisplayableTreeView treeViewMachines;
    private Lemoine.BaseControls.ComboboxTextValue comboBoxEventType;
    private Lemoine.BaseControls.ComboboxTextValue comboBoxEventLevel;
    private ConfiguratorAlarms.ColorCheckedListBox listUsers;
    private Lemoine.BaseControls.ComboboxTextValue comboboxInputItems;
  }
}
