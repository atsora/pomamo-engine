// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots.UserMachine
{
  partial class UserMachinePage3
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.List.ListTextValue listMachines;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button buttonRemove;
    private System.Windows.Forms.Button buttonAdd;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserMachinePage3));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.listMachines = new Lemoine.BaseControls.List.ListTextValue();
      this.label4 = new System.Windows.Forms.Label();
      this.buttonRemove = new System.Windows.Forms.Button();
      this.buttonAdd = new System.Windows.Forms.Button();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.listMachines, 2, 0);
      this.baseLayout.Controls.Add(this.label4, 0, 0);
      this.baseLayout.Controls.Add(this.buttonRemove, 1, 2);
      this.baseLayout.Controls.Add(this.buttonAdd, 1, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.Size = new System.Drawing.Size(334, 260);
      this.baseLayout.TabIndex = 3;
      // 
      // listMachines
      // 
      this.listMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listMachines.Location = new System.Drawing.Point(73, 0);
      this.listMachines.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.listMachines.Name = "listMachines";
      this.baseLayout.SetRowSpan(this.listMachines, 4);
      this.listMachines.Size = new System.Drawing.Size(258, 257);
      this.listMachines.Sorted = true;
      this.listMachines.TabIndex = 7;
      this.listMachines.ItemChanged += new System.Action<string, object>(this.ListMachinesItemChanged);
      // 
      // label4
      // 
      this.baseLayout.SetColumnSpan(this.label4, 2);
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(3, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(64, 27);
      this.label4.TabIndex = 2;
      this.label4.Text = "Machines";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonRemove
      // 
      this.buttonRemove.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRemove.Enabled = false;
      this.buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemove.Image")));
      this.buttonRemove.Location = new System.Drawing.Point(45, 204);
      this.buttonRemove.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
      this.buttonRemove.Name = "buttonRemove";
      this.buttonRemove.Size = new System.Drawing.Size(25, 25);
      this.buttonRemove.TabIndex = 8;
      this.buttonRemove.UseVisualStyleBackColor = true;
      this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
      // 
      // buttonAdd
      // 
      this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAdd.Image = ((System.Drawing.Image)(resources.GetObject("buttonAdd.Image")));
      this.buttonAdd.Location = new System.Drawing.Point(45, 232);
      this.buttonAdd.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
      this.buttonAdd.Name = "buttonAdd";
      this.buttonAdd.Size = new System.Drawing.Size(25, 25);
      this.buttonAdd.TabIndex = 9;
      this.buttonAdd.UseVisualStyleBackColor = true;
      this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
      // 
      // UserMachinePage3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "UserMachinePage3";
      this.Size = new System.Drawing.Size(334, 260);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
