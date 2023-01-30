// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_Settings
{
  partial class TextDialog
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Button buttonCopy;
    private System.Windows.Forms.Button buttonClose;
    private System.Windows.Forms.RichTextBox richTextBox;
    private System.Windows.Forms.Label labelSuccess;
    
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextDialog));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonCopy = new System.Windows.Forms.Button();
      this.buttonClose = new System.Windows.Forms.Button();
      this.richTextBox = new System.Windows.Forms.RichTextBox();
      this.labelSuccess = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85F));
      this.baseLayout.Controls.Add(this.buttonCopy, 0, 1);
      this.baseLayout.Controls.Add(this.buttonClose, 2, 1);
      this.baseLayout.Controls.Add(this.richTextBox, 0, 0);
      this.baseLayout.Controls.Add(this.labelSuccess, 1, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
      this.baseLayout.Size = new System.Drawing.Size(404, 296);
      this.baseLayout.TabIndex = 0;
      // 
      // buttonCopy
      // 
      this.buttonCopy.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCopy.Location = new System.Drawing.Point(3, 271);
      this.buttonCopy.Name = "buttonCopy";
      this.buttonCopy.Size = new System.Drawing.Size(79, 22);
      this.buttonCopy.TabIndex = 0;
      this.buttonCopy.Text = "Copy";
      this.buttonCopy.UseVisualStyleBackColor = true;
      this.buttonCopy.Click += new System.EventHandler(this.ButtonCopyClick);
      // 
      // buttonClose
      // 
      this.buttonClose.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonClose.Location = new System.Drawing.Point(322, 271);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new System.Drawing.Size(79, 22);
      this.buttonClose.TabIndex = 1;
      this.buttonClose.Text = "Close";
      this.buttonClose.UseVisualStyleBackColor = true;
      this.buttonClose.Click += new System.EventHandler(this.ButtonCloseClick);
      // 
      // richTextBox
      // 
      this.richTextBox.BackColor = System.Drawing.SystemColors.Window;
      this.baseLayout.SetColumnSpan(this.richTextBox, 3);
      this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextBox.Location = new System.Drawing.Point(3, 3);
      this.richTextBox.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.richTextBox.Name = "richTextBox";
      this.richTextBox.ReadOnly = true;
      this.richTextBox.Size = new System.Drawing.Size(398, 265);
      this.richTextBox.TabIndex = 2;
      this.richTextBox.Text = "";
      // 
      // labelSuccess
      // 
      this.labelSuccess.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelSuccess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelSuccess.ForeColor = System.Drawing.Color.ForestGreen;
      this.labelSuccess.Location = new System.Drawing.Point(88, 268);
      this.labelSuccess.Name = "labelSuccess";
      this.labelSuccess.Size = new System.Drawing.Size(228, 28);
      this.labelSuccess.TabIndex = 3;
      this.labelSuccess.Text = "Successfully copied!";
      this.labelSuccess.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.labelSuccess.Visible = false;
      // 
      // TextDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(404, 296);
      this.Controls.Add(this.baseLayout);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimumSize = new System.Drawing.Size(420, 330);
      this.Name = "TextDialog";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Wiki text";
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
  }
}
