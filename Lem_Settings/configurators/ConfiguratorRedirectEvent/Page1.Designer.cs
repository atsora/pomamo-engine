// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorRedirectEvent
{
  partial class Page1
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
      this.verticalScroll = new Lemoine.BaseControls.VerticalScrollLayout();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.comboEvent = new Lemoine.BaseControls.ComboboxTextValue();
      this.comboAction = new Lemoine.BaseControls.ComboboxTextValue();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // verticalScroll
      // 
      this.verticalScroll.BackColor = System.Drawing.Color.White;
      this.verticalScroll.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.verticalScroll, 4);
      this.verticalScroll.ContainerMargin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Dock = System.Windows.Forms.DockStyle.Fill;
      this.verticalScroll.Location = new System.Drawing.Point(0, 24);
      this.verticalScroll.Margin = new System.Windows.Forms.Padding(0);
      this.verticalScroll.Name = "verticalScroll";
      this.verticalScroll.Size = new System.Drawing.Size(370, 266);
      this.verticalScroll.TabIndex = 0;
      this.verticalScroll.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.verticalScroll.TitleForeColor = System.Drawing.SystemColors.ControlText;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 62F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.Controls.Add(this.verticalScroll, 0, 1);
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.label2, 2, 0);
      this.baseLayout.Controls.Add(this.comboEvent, 1, 0);
      this.baseLayout.Controls.Add(this.comboAction, 3, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(64, 24);
      this.label1.TabIndex = 1;
      this.label1.Text = "Event type";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(186, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(62, 24);
      this.label2.TabIndex = 2;
      this.label2.Text = "Action type";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // comboEvent
      // 
      this.comboEvent.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboEvent.Location = new System.Drawing.Point(64, 0);
      this.comboEvent.Margin = new System.Windows.Forms.Padding(0);
      this.comboEvent.Name = "comboEvent";
      this.comboEvent.Size = new System.Drawing.Size(122, 24);
      this.comboEvent.TabIndex = 3;
      this.comboEvent.ItemChanged += new System.Action<string, object>(this.ComboEventItemChanged);
      // 
      // comboAlert
      // 
      this.comboAction.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboAction.Location = new System.Drawing.Point(248, 0);
      this.comboAction.Margin = new System.Windows.Forms.Padding(0);
      this.comboAction.Name = "comboAlert";
      this.comboAction.Size = new System.Drawing.Size(122, 24);
      this.comboAction.TabIndex = 4;
      this.comboAction.ItemChanged += new System.Action<string, object>(this.ComboActionItemChanged);
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private Lemoine.BaseControls.VerticalScrollLayout verticalScroll;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private Lemoine.BaseControls.ComboboxTextValue comboEvent;
    private Lemoine.BaseControls.ComboboxTextValue comboAction;
  }
}
