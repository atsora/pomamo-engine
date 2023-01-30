// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots.UserMachine
{
  partial class DialogMachineStateTemplate
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private Lemoine.BaseControls.List.ListTextValue listMachineStateTemplates;
    private Lemoine.DataReferenceControls.DisplayableTreeView treeMachines;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.Button buttonCancel;
    
    /// <summary>
    /// Disposes resources used by the form.
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DialogMachineStateTemplate));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.listMachineStateTemplates = new Lemoine.BaseControls.List.ListTextValue();
      this.treeMachines = new Lemoine.DataReferenceControls.DisplayableTreeView();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.buttonOk = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 1, 0);
      this.baseLayout.Controls.Add(this.listMachineStateTemplates, 1, 1);
      this.baseLayout.Controls.Add(this.treeMachines, 0, 1);
      this.baseLayout.Controls.Add(this.tableLayoutPanel1, 0, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.Size = new System.Drawing.Size(378, 262);
      this.baseLayout.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(189, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "Machine";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(189, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(189, 25);
      this.label2.TabIndex = 1;
      this.label2.Text = "Machine state template";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // listMachineStateTemplates
      // 
      this.listMachineStateTemplates.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listMachineStateTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listMachineStateTemplates.Location = new System.Drawing.Point(192, 25);
      this.listMachineStateTemplates.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.listMachineStateTemplates.Name = "listMachineStateTemplates";
      this.listMachineStateTemplates.Size = new System.Drawing.Size(183, 209);
      this.listMachineStateTemplates.Sorted = true;
      this.listMachineStateTemplates.TabIndex = 2;
      // 
      // treeMachines
      // 
      this.treeMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.treeMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeMachines.Location = new System.Drawing.Point(3, 25);
      this.treeMachines.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.treeMachines.MultiSelection = false;
      this.treeMachines.Name = "treeMachines";
      this.treeMachines.Size = new System.Drawing.Size(183, 209);
      this.treeMachines.TabIndex = 3;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.baseLayout.SetColumnSpan(this.tableLayoutPanel1, 2);
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
      this.tableLayoutPanel1.Controls.Add(this.buttonOk, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 1, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 237);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(378, 22);
      this.tableLayoutPanel1.TabIndex = 4;
      // 
      // buttonOk
      // 
      this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOk.Location = new System.Drawing.Point(303, 0);
      this.buttonOk.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(72, 22);
      this.buttonOk.TabIndex = 0;
      this.buttonOk.Text = "Ok";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCancel.Location = new System.Drawing.Point(228, 0);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(72, 22);
      this.buttonCancel.TabIndex = 1;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
      // 
      // DialogMachineStateTemplate
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(378, 262);
      this.Controls.Add(this.baseLayout);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DialogMachineStateTemplate";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Machine - Machine state template";
      this.baseLayout.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
