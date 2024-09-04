// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorSlots.ShiftTemplate
{
  partial class ShiftTemplatePage3
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShiftTemplatePage3));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label3 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.checkForceAssociation = new System.Windows.Forms.CheckBox();
      this.listShiftTemplate = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 2;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.label3, 0, 0);
      this.baseLayout.Controls.Add(this.label5, 0, 2);
      this.baseLayout.Controls.Add(this.checkForceAssociation, 1, 2);
      this.baseLayout.Controls.Add(this.listShiftTemplate, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.Size = new System.Drawing.Size(274, 150);
      this.baseLayout.TabIndex = 2;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(99, 27);
      this.label3.TabIndex = 1;
      this.label3.Text = "Shift template";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(3, 123);
      this.label5.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(99, 24);
      this.label5.TabIndex = 6;
      this.label5.Text = "Force association";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkForceAssociation
      // 
      this.checkForceAssociation.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkForceAssociation.Location = new System.Drawing.Point(108, 126);
      this.checkForceAssociation.Name = "checkForceAssociation";
      this.checkForceAssociation.Size = new System.Drawing.Size(163, 21);
      this.checkForceAssociation.TabIndex = 5;
      this.checkForceAssociation.UseVisualStyleBackColor = true;
      // 
      // listShiftTemplate
      // 
      this.listShiftTemplate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listShiftTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listShiftTemplate.Location = new System.Drawing.Point(108, 0);
      this.listShiftTemplate.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.listShiftTemplate.MultipleSelection = false;
      this.listShiftTemplate.Name = "listShiftTemplate";
      this.baseLayout.SetRowSpan(this.listShiftTemplate, 2);
      this.listShiftTemplate.SelectedIndex = -1;
      this.listShiftTemplate.SelectedIndexes = ((System.Collections.Generic.IList<int>)(resources.GetObject("listShiftTemplate.SelectedIndexes")));
      this.listShiftTemplate.SelectedText = "";
      this.listShiftTemplate.SelectedTexts = ((System.Collections.Generic.IList<string>)(resources.GetObject("listShiftTemplate.SelectedTexts")));
      this.listShiftTemplate.SelectedValue = null;
      this.listShiftTemplate.SelectedValues = ((System.Collections.Generic.IList<object>)(resources.GetObject("listShiftTemplate.SelectedValues")));
      this.listShiftTemplate.Size = new System.Drawing.Size(163, 123);
      this.listShiftTemplate.Sorted = true;
      this.listShiftTemplate.TabIndex = 7;
      // 
      // ShiftTemplatePage3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "ShiftTemplatePage3";
      this.Size = new System.Drawing.Size(274, 150);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);
    }
    private System.Windows.Forms.CheckBox checkForceAssociation;
    private System.Windows.Forms.Label label5;
    private Lemoine.BaseControls.List.ListTextValue listShiftTemplate;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
