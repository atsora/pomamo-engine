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
    private void InitializeComponent ()
    {
      listModules = new Lemoine.BaseControls.List.ListTextValue ();
      baseLayout = new System.Windows.Forms.TableLayoutPanel ();
      baseLayout2 = new System.Windows.Forms.TableLayoutPanel ();
      buttonRead = new System.Windows.Forms.Button ();
      buttonOpen = new System.Windows.Forms.Button ();
      comboProtocols = new Lemoine.BaseControls.ComboboxTextValue ();
      comboControls = new Lemoine.BaseControls.ComboboxTextValue ();
      comboMachines = new Lemoine.BaseControls.ComboboxTextValue ();
      textFilter = new Lemoine.BaseControls.TextBoxWatermarked ();
      comboCustomer = new Lemoine.BaseControls.ComboboxTextValue ();
      baseLayout.SuspendLayout ();
      baseLayout2.SuspendLayout ();
      SuspendLayout ();
      // 
      // listModules
      // 
      listModules.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      baseLayout.SetColumnSpan (listModules, 4);
      listModules.Dock = System.Windows.Forms.DockStyle.Fill;
      listModules.Location = new System.Drawing.Point (0, 55);
      listModules.Margin = new System.Windows.Forms.Padding (0);
      listModules.Name = "listModules";
      listModules.Size = new System.Drawing.Size (408, 204);
      listModules.Sorted = true;
      listModules.TabIndex = 0;
      listModules.ItemChanged += ListModulesItemChanged;
      // 
      // baseLayout
      // 
      baseLayout.ColumnCount = 4;
      baseLayout.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 25F));
      baseLayout.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 25F));
      baseLayout.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 25F));
      baseLayout.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 25F));
      baseLayout.Controls.Add (listModules, 0, 2);
      baseLayout.Controls.Add (baseLayout2, 0, 3);
      baseLayout.Controls.Add (comboProtocols, 2, 0);
      baseLayout.Controls.Add (comboControls, 1, 0);
      baseLayout.Controls.Add (comboMachines, 0, 0);
      baseLayout.Controls.Add (textFilter, 0, 1);
      baseLayout.Controls.Add (comboCustomer, 3, 0);
      baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      baseLayout.Location = new System.Drawing.Point (0, 0);
      baseLayout.Margin = new System.Windows.Forms.Padding (0);
      baseLayout.Name = "baseLayout";
      baseLayout.RowCount = 4;
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 28F));
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 27F));
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 100F));
      baseLayout.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 29F));
      baseLayout.Size = new System.Drawing.Size (408, 288);
      baseLayout.TabIndex = 1;
      // 
      // baseLayout2
      // 
      baseLayout2.ColumnCount = 3;
      baseLayout.SetColumnSpan (baseLayout2, 4);
      baseLayout2.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Percent, 100F));
      baseLayout2.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Absolute, 121F));
      baseLayout2.ColumnStyles.Add (new System.Windows.Forms.ColumnStyle (System.Windows.Forms.SizeType.Absolute, 76F));
      baseLayout2.Controls.Add (buttonRead, 1, 0);
      baseLayout2.Controls.Add (buttonOpen, 2, 0);
      baseLayout2.Dock = System.Windows.Forms.DockStyle.Fill;
      baseLayout2.Location = new System.Drawing.Point (0, 259);
      baseLayout2.Margin = new System.Windows.Forms.Padding (0);
      baseLayout2.Name = "baseLayout2";
      baseLayout2.RowCount = 1;
      baseLayout2.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Percent, 100F));
      baseLayout2.RowStyles.Add (new System.Windows.Forms.RowStyle (System.Windows.Forms.SizeType.Absolute, 29F));
      baseLayout2.Size = new System.Drawing.Size (408, 29);
      baseLayout2.TabIndex = 3;
      // 
      // buttonRead
      // 
      buttonRead.Dock = System.Windows.Forms.DockStyle.Fill;
      buttonRead.Enabled = false;
      buttonRead.Location = new System.Drawing.Point (211, 3);
      buttonRead.Margin = new System.Windows.Forms.Padding (0, 3, 4, 0);
      buttonRead.Name = "buttonRead";
      buttonRead.Size = new System.Drawing.Size (117, 26);
      buttonRead.TabIndex = 1;
      buttonRead.Text = "Read description";
      buttonRead.UseVisualStyleBackColor = true;
      buttonRead.Click += ButtonReadClick;
      // 
      // buttonOpen
      // 
      buttonOpen.Dock = System.Windows.Forms.DockStyle.Fill;
      buttonOpen.Enabled = false;
      buttonOpen.Location = new System.Drawing.Point (332, 3);
      buttonOpen.Margin = new System.Windows.Forms.Padding (0, 3, 0, 0);
      buttonOpen.Name = "buttonOpen";
      buttonOpen.Size = new System.Drawing.Size (76, 26);
      buttonOpen.TabIndex = 2;
      buttonOpen.Text = "Open file";
      buttonOpen.UseVisualStyleBackColor = true;
      buttonOpen.Click += ButtonOpenClick;
      // 
      // comboProtocols
      // 
      comboProtocols.Dock = System.Windows.Forms.DockStyle.Fill;
      comboProtocols.Location = new System.Drawing.Point (208, 0);
      comboProtocols.Margin = new System.Windows.Forms.Padding (4, 0, 0, 3);
      comboProtocols.Name = "comboProtocols";
      comboProtocols.Size = new System.Drawing.Size (98, 25);
      comboProtocols.Sorted = true;
      comboProtocols.TabIndex = 4;
      comboProtocols.ItemChanged += ComboProtocolsItemChanged;
      // 
      // comboControls
      // 
      comboControls.Dock = System.Windows.Forms.DockStyle.Fill;
      comboControls.Location = new System.Drawing.Point (104, 0);
      comboControls.Margin = new System.Windows.Forms.Padding (2, 0, 2, 3);
      comboControls.Name = "comboControls";
      comboControls.Size = new System.Drawing.Size (98, 25);
      comboControls.Sorted = true;
      comboControls.TabIndex = 5;
      comboControls.ItemChanged += ComboControlsItemChanged;
      // 
      // comboMachines
      // 
      comboMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      comboMachines.Location = new System.Drawing.Point (0, 0);
      comboMachines.Margin = new System.Windows.Forms.Padding (0, 0, 4, 3);
      comboMachines.Name = "comboMachines";
      comboMachines.Size = new System.Drawing.Size (98, 25);
      comboMachines.Sorted = true;
      comboMachines.TabIndex = 6;
      comboMachines.ItemChanged += ComboMachinesItemChanged;
      // 
      // textFilter
      // 
      baseLayout.SetColumnSpan (textFilter, 4);
      textFilter.Dock = System.Windows.Forms.DockStyle.Fill;
      textFilter.Location = new System.Drawing.Point (0, 28);
      textFilter.Margin = new System.Windows.Forms.Padding (0);
      textFilter.Name = "textFilter";
      textFilter.Size = new System.Drawing.Size (408, 23);
      textFilter.TabIndex = 7;
      textFilter.WaterMark = "Filter...";
      textFilter.WaterMarkActiveForeColor = System.Drawing.Color.Gray;
      textFilter.WaterMarkFont = new System.Drawing.Font ("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      textFilter.WaterMarkForeColor = System.Drawing.Color.LightGray;
      textFilter.TextChanged += TextFilterTextChanged;
      // 
      // comboCustomer
      // 
      comboCustomer.Dock = System.Windows.Forms.DockStyle.Fill;
      comboCustomer.Location = new System.Drawing.Point (310, 0);
      comboCustomer.Margin = new System.Windows.Forms.Padding (4, 0, 0, 3);
      comboCustomer.Name = "comboCustomer";
      comboCustomer.Size = new System.Drawing.Size (98, 25);
      comboCustomer.TabIndex = 8;
      comboCustomer.ItemChanged += comboCustomer_ItemChanged;
      // 
      // PageModule
      // 
      AutoScaleDimensions = new System.Drawing.SizeF (7F, 15F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      Controls.Add (baseLayout);
      Margin = new System.Windows.Forms.Padding (4, 3, 4, 3);
      Name = "PageModule";
      Size = new System.Drawing.Size (408, 288);
      baseLayout.ResumeLayout (false);
      baseLayout.PerformLayout ();
      baseLayout2.ResumeLayout (false);
      ResumeLayout (false);

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
