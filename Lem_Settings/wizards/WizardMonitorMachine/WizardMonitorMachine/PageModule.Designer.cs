// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace WizardMonitorMachine
{
  partial class PageModule
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
      this.listModules = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.baseLayout2 = new System.Windows.Forms.TableLayoutPanel();
      this.buttonRead = new System.Windows.Forms.Button();
      this.buttonOpen = new System.Windows.Forms.Button();
      this.comboProtocols = new Lemoine.BaseControls.ComboboxTextValue();
      this.comboControls = new Lemoine.BaseControls.ComboboxTextValue();
      this.comboMachines = new Lemoine.BaseControls.ComboboxTextValue();
      this.textFilter = new Lemoine.BaseControls.TextBoxWatermarked();
      this.comboCustomer = new Lemoine.BaseControls.ComboboxTextValue();
      this.baseLayout.SuspendLayout();
      this.baseLayout2.SuspendLayout();
      this.SuspendLayout();
      // 
      // listModules
      // 
      this.listModules.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listModules, 4);
      this.listModules.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listModules.Location = new System.Drawing.Point(0, 47);
      this.listModules.Margin = new System.Windows.Forms.Padding(0);
      this.listModules.Name = "listModules";
      this.listModules.Size = new System.Drawing.Size(350, 178);
      this.listModules.Sorted = true;
      this.listModules.TabIndex = 0;
      this.listModules.ItemChanged += new System.Action<string, object>(this.ListModulesItemChanged);
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.baseLayout.Controls.Add(this.listModules, 0, 2);
      this.baseLayout.Controls.Add(this.baseLayout2, 0, 3);
      this.baseLayout.Controls.Add(this.comboProtocols, 2, 0);
      this.baseLayout.Controls.Add(this.comboControls, 1, 0);
      this.baseLayout.Controls.Add(this.comboMachines, 0, 0);
      this.baseLayout.Controls.Add(this.textFilter, 0, 1);
      this.baseLayout.Controls.Add(this.comboCustomer, 3, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 1;
      // 
      // baseLayout2
      // 
      this.baseLayout2.ColumnCount = 3;
      this.baseLayout.SetColumnSpan(this.baseLayout2, 4);
      this.baseLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
      this.baseLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
      this.baseLayout2.Controls.Add(this.buttonRead, 1, 0);
      this.baseLayout2.Controls.Add(this.buttonOpen, 2, 0);
      this.baseLayout2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout2.Location = new System.Drawing.Point(0, 225);
      this.baseLayout2.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout2.Name = "baseLayout2";
      this.baseLayout2.RowCount = 1;
      this.baseLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout2.Size = new System.Drawing.Size(350, 25);
      this.baseLayout2.TabIndex = 3;
      // 
      // buttonRead
      // 
      this.buttonRead.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRead.Enabled = false;
      this.buttonRead.Location = new System.Drawing.Point(181, 3);
      this.buttonRead.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
      this.buttonRead.Name = "buttonRead";
      this.buttonRead.Size = new System.Drawing.Size(101, 22);
      this.buttonRead.TabIndex = 1;
      this.buttonRead.Text = "Read description";
      this.buttonRead.UseVisualStyleBackColor = true;
      this.buttonRead.Click += new System.EventHandler(this.ButtonReadClick);
      // 
      // buttonOpen
      // 
      this.buttonOpen.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOpen.Enabled = false;
      this.buttonOpen.Location = new System.Drawing.Point(285, 3);
      this.buttonOpen.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.buttonOpen.Name = "buttonOpen";
      this.buttonOpen.Size = new System.Drawing.Size(65, 22);
      this.buttonOpen.TabIndex = 2;
      this.buttonOpen.Text = "Open file";
      this.buttonOpen.UseVisualStyleBackColor = true;
      this.buttonOpen.Click += new System.EventHandler(this.ButtonOpenClick);
      // 
      // comboProtocols
      // 
      this.comboProtocols.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboProtocols.Location = new System.Drawing.Point(177, 0);
      this.comboProtocols.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.comboProtocols.Name = "comboProtocols";
      this.comboProtocols.Size = new System.Drawing.Size(84, 21);
      this.comboProtocols.Sorted = true;
      this.comboProtocols.TabIndex = 4;
      this.comboProtocols.ItemChanged += new System.Action<string, object>(this.ComboProtocolsItemChanged);
      // 
      // comboControls
      // 
      this.comboControls.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboControls.Location = new System.Drawing.Point(89, 0);
      this.comboControls.Margin = new System.Windows.Forms.Padding(2, 0, 2, 3);
      this.comboControls.Name = "comboControls";
      this.comboControls.Size = new System.Drawing.Size(83, 21);
      this.comboControls.Sorted = true;
      this.comboControls.TabIndex = 5;
      this.comboControls.ItemChanged += new System.Action<string, object>(this.ComboControlsItemChanged);
      // 
      // comboMachines
      // 
      this.comboMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboMachines.Location = new System.Drawing.Point(0, 0);
      this.comboMachines.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.comboMachines.Name = "comboMachines";
      this.comboMachines.Size = new System.Drawing.Size(84, 21);
      this.comboMachines.Sorted = true;
      this.comboMachines.TabIndex = 6;
      this.comboMachines.ItemChanged += new System.Action<string, object>(this.ComboMachinesItemChanged);
      // 
      // textFilter
      // 
      this.baseLayout.SetColumnSpan(this.textFilter, 4);
      this.textFilter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textFilter.Location = new System.Drawing.Point(0, 24);
      this.textFilter.Margin = new System.Windows.Forms.Padding(0);
      this.textFilter.Name = "textFilter";
      this.textFilter.Size = new System.Drawing.Size(350, 20);
      this.textFilter.TabIndex = 7;
      this.textFilter.WaterMark = "Filter...";
      this.textFilter.WaterMarkActiveForeColor = System.Drawing.Color.Gray;
      this.textFilter.WaterMarkFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textFilter.WaterMarkForeColor = System.Drawing.Color.LightGray;
      this.textFilter.TextChanged += new System.EventHandler(this.TextFilterTextChanged);
      // 
      // comboCustomer
      // 
      this.comboCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboCustomer.Location = new System.Drawing.Point(264, 0);
      this.comboCustomer.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.comboCustomer.Name = "comboCustomer";
      this.comboCustomer.Size = new System.Drawing.Size(86, 21);
      this.comboCustomer.TabIndex = 8;
      this.comboCustomer.ItemChanged += new System.Action<string, object>(this.comboCustomer_ItemChanged);
      // 
      // PageModule
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PageModule";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.baseLayout2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Button buttonOpen;
    private System.Windows.Forms.Button buttonRead;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.List.ListTextValue listModules;
    private System.Windows.Forms.TableLayoutPanel baseLayout2;
    private Lemoine.BaseControls.ComboboxTextValue comboProtocols;
    private Lemoine.BaseControls.ComboboxTextValue comboControls;
    private Lemoine.BaseControls.ComboboxTextValue comboMachines;
    private Lemoine.BaseControls.TextBoxWatermarked textFilter;
    private Lemoine.BaseControls.ComboboxTextValue comboCustomer;
  }
}
