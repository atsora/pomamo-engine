// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorAlarmFocus
{
  partial class PagePattern
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PagePattern));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.textType = new System.Windows.Forms.TextBox();
      this.textMessage = new System.Windows.Forms.TextBox();
      this.textNumber = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.buttonClearType = new System.Windows.Forms.Button();
      this.buttonClearNumber = new System.Windows.Forms.Button();
      this.buttonClearMessage = new System.Windows.Forms.Button();
      this.buttonAddProperty = new System.Windows.Forms.Button();
      this.buttonRemoveProperty = new System.Windows.Forms.Button();
      this.buttonClearProperties = new System.Windows.Forms.Button();
      this.listProperties = new Lemoine.BaseControls.List.ListTextValue();
      this.buttonEditProperty = new System.Windows.Forms.Button();
      this.label5 = new System.Windows.Forms.Label();
      this.textAcquisitionInfo = new System.Windows.Forms.TextBox();
      this.buttonClearAcquisitionInfo = new System.Windows.Forms.Button();
      this.label6 = new System.Windows.Forms.Label();
      this.labelAcquisitionInfo = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 83F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.Controls.Add(this.label1, 0, 2);
      this.baseLayout.Controls.Add(this.label2, 0, 3);
      this.baseLayout.Controls.Add(this.label3, 0, 5);
      this.baseLayout.Controls.Add(this.textType, 1, 2);
      this.baseLayout.Controls.Add(this.textMessage, 1, 4);
      this.baseLayout.Controls.Add(this.textNumber, 1, 3);
      this.baseLayout.Controls.Add(this.label4, 0, 4);
      this.baseLayout.Controls.Add(this.buttonClearType, 2, 2);
      this.baseLayout.Controls.Add(this.buttonClearNumber, 2, 3);
      this.baseLayout.Controls.Add(this.buttonClearMessage, 2, 4);
      this.baseLayout.Controls.Add(this.buttonAddProperty, 2, 5);
      this.baseLayout.Controls.Add(this.buttonRemoveProperty, 2, 7);
      this.baseLayout.Controls.Add(this.buttonClearProperties, 2, 8);
      this.baseLayout.Controls.Add(this.listProperties, 1, 5);
      this.baseLayout.Controls.Add(this.buttonEditProperty, 2, 6);
      this.baseLayout.Controls.Add(this.label5, 0, 1);
      this.baseLayout.Controls.Add(this.textAcquisitionInfo, 1, 1);
      this.baseLayout.Controls.Add(this.buttonClearAcquisitionInfo, 2, 1);
      this.baseLayout.Controls.Add(this.label6, 0, 0);
      this.baseLayout.Controls.Add(this.labelAcquisitionInfo, 1, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 10;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(0, 54);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(80, 27);
      this.label1.TabIndex = 0;
      this.label1.Text = "Alarm type";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 81);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(80, 27);
      this.label2.TabIndex = 1;
      this.label2.Text = "Alarm number";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(0, 135);
      this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(80, 29);
      this.label3.TabIndex = 2;
      this.label3.Text = "Properties";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textType
      // 
      this.textType.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textType.Location = new System.Drawing.Point(83, 56);
      this.textType.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.textType.Name = "textType";
      this.textType.Size = new System.Drawing.Size(260, 20);
      this.textType.TabIndex = 3;
      // 
      // textMessage
      // 
      this.textMessage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textMessage.Location = new System.Drawing.Point(83, 110);
      this.textMessage.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.textMessage.Name = "textMessage";
      this.textMessage.Size = new System.Drawing.Size(260, 20);
      this.textMessage.TabIndex = 5;
      // 
      // textNumber
      // 
      this.textNumber.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textNumber.Location = new System.Drawing.Point(83, 83);
      this.textNumber.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.textNumber.Name = "textNumber";
      this.textNumber.Size = new System.Drawing.Size(260, 20);
      this.textNumber.TabIndex = 6;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(0, 108);
      this.label4.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(80, 27);
      this.label4.TabIndex = 7;
      this.label4.Text = "Message";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonClearType
      // 
      this.buttonClearType.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonClearType.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearType.Image")));
      this.buttonClearType.Location = new System.Drawing.Point(346, 54);
      this.buttonClearType.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.buttonClearType.Name = "buttonClearType";
      this.buttonClearType.Size = new System.Drawing.Size(24, 24);
      this.buttonClearType.TabIndex = 8;
      this.buttonClearType.UseVisualStyleBackColor = true;
      this.buttonClearType.Click += new System.EventHandler(this.ButtonClearTypeClick);
      // 
      // buttonClearNumber
      // 
      this.buttonClearNumber.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonClearNumber.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearNumber.Image")));
      this.buttonClearNumber.Location = new System.Drawing.Point(346, 81);
      this.buttonClearNumber.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.buttonClearNumber.Name = "buttonClearNumber";
      this.buttonClearNumber.Size = new System.Drawing.Size(24, 24);
      this.buttonClearNumber.TabIndex = 9;
      this.buttonClearNumber.UseVisualStyleBackColor = true;
      this.buttonClearNumber.Click += new System.EventHandler(this.ButtonClearNumberClick);
      // 
      // buttonClearMessage
      // 
      this.buttonClearMessage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonClearMessage.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearMessage.Image")));
      this.buttonClearMessage.Location = new System.Drawing.Point(346, 108);
      this.buttonClearMessage.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.buttonClearMessage.Name = "buttonClearMessage";
      this.buttonClearMessage.Size = new System.Drawing.Size(24, 24);
      this.buttonClearMessage.TabIndex = 10;
      this.buttonClearMessage.UseVisualStyleBackColor = true;
      this.buttonClearMessage.Click += new System.EventHandler(this.ButtonClearMessageClick);
      // 
      // buttonAddProperty
      // 
      this.buttonAddProperty.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAddProperty.Image = ((System.Drawing.Image)(resources.GetObject("buttonAddProperty.Image")));
      this.buttonAddProperty.Location = new System.Drawing.Point(346, 137);
      this.buttonAddProperty.Margin = new System.Windows.Forms.Padding(3, 2, 0, 3);
      this.buttonAddProperty.Name = "buttonAddProperty";
      this.buttonAddProperty.Size = new System.Drawing.Size(24, 24);
      this.buttonAddProperty.TabIndex = 11;
      this.buttonAddProperty.UseVisualStyleBackColor = true;
      this.buttonAddProperty.Click += new System.EventHandler(this.ButtonAddPropertyClick);
      // 
      // buttonRemoveProperty
      // 
      this.buttonRemoveProperty.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRemoveProperty.Enabled = false;
      this.buttonRemoveProperty.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemoveProperty.Image")));
      this.buttonRemoveProperty.Location = new System.Drawing.Point(346, 191);
      this.buttonRemoveProperty.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.buttonRemoveProperty.Name = "buttonRemoveProperty";
      this.buttonRemoveProperty.Size = new System.Drawing.Size(24, 24);
      this.buttonRemoveProperty.TabIndex = 12;
      this.buttonRemoveProperty.UseVisualStyleBackColor = true;
      this.buttonRemoveProperty.Click += new System.EventHandler(this.ButtonRemovePropertyClick);
      // 
      // buttonClearProperties
      // 
      this.buttonClearProperties.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonClearProperties.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearProperties.Image")));
      this.buttonClearProperties.Location = new System.Drawing.Point(346, 218);
      this.buttonClearProperties.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.buttonClearProperties.Name = "buttonClearProperties";
      this.buttonClearProperties.Size = new System.Drawing.Size(24, 24);
      this.buttonClearProperties.TabIndex = 13;
      this.buttonClearProperties.UseVisualStyleBackColor = true;
      this.buttonClearProperties.Click += new System.EventHandler(this.ButtonClearPropertiesClick);
      // 
      // listProperties
      // 
      this.listProperties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.listProperties.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listProperties.Location = new System.Drawing.Point(83, 137);
      this.listProperties.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.listProperties.Name = "listProperties";
      this.baseLayout.SetRowSpan(this.listProperties, 5);
      this.listProperties.Size = new System.Drawing.Size(260, 153);
      this.listProperties.TabIndex = 14;
      this.listProperties.ItemChanged += new System.Action<string, object>(this.ListPropertiesItemChanged);
      // 
      // buttonEditProperty
      // 
      this.buttonEditProperty.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonEditProperty.BackgroundImage")));
      this.buttonEditProperty.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.buttonEditProperty.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonEditProperty.Location = new System.Drawing.Point(346, 164);
      this.buttonEditProperty.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.buttonEditProperty.Name = "buttonEditProperty";
      this.buttonEditProperty.Size = new System.Drawing.Size(24, 24);
      this.buttonEditProperty.TabIndex = 15;
      this.buttonEditProperty.UseVisualStyleBackColor = true;
      this.buttonEditProperty.Click += new System.EventHandler(this.ButtonEditPropertyClick);
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(0, 27);
      this.label5.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(80, 27);
      this.label5.TabIndex = 16;
      this.label5.Text = "Acq. sub-info";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textAcquisitionInfo
      // 
      this.textAcquisitionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textAcquisitionInfo.Location = new System.Drawing.Point(83, 29);
      this.textAcquisitionInfo.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
      this.textAcquisitionInfo.Name = "textAcquisitionInfo";
      this.textAcquisitionInfo.Size = new System.Drawing.Size(260, 20);
      this.textAcquisitionInfo.TabIndex = 17;
      // 
      // buttonClearAcquisitionInfo
      // 
      this.buttonClearAcquisitionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonClearAcquisitionInfo.Image = ((System.Drawing.Image)(resources.GetObject("buttonClearAcquisitionInfo.Image")));
      this.buttonClearAcquisitionInfo.Location = new System.Drawing.Point(346, 27);
      this.buttonClearAcquisitionInfo.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.buttonClearAcquisitionInfo.Name = "buttonClearAcquisitionInfo";
      this.buttonClearAcquisitionInfo.Size = new System.Drawing.Size(24, 24);
      this.buttonClearAcquisitionInfo.TabIndex = 18;
      this.buttonClearAcquisitionInfo.UseVisualStyleBackColor = true;
      this.buttonClearAcquisitionInfo.Click += new System.EventHandler(this.ButtonClearAcquisitionInfoClick);
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label6.Location = new System.Drawing.Point(0, 0);
      this.label6.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(80, 27);
      this.label6.TabIndex = 19;
      this.label6.Text = "Acquisition info";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelAcquisitionInfo
      // 
      this.labelAcquisitionInfo.AutoSize = true;
      this.labelAcquisitionInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelAcquisitionInfo.Location = new System.Drawing.Point(83, 0);
      this.labelAcquisitionInfo.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.labelAcquisitionInfo.Name = "labelAcquisitionInfo";
      this.labelAcquisitionInfo.Size = new System.Drawing.Size(257, 27);
      this.labelAcquisitionInfo.TabIndex = 20;
      this.labelAcquisitionInfo.Text = "...";
      this.labelAcquisitionInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // PagePattern
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "PagePattern";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.baseLayout.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textType;
    private System.Windows.Forms.TextBox textMessage;
    private System.Windows.Forms.TextBox textNumber;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button buttonClearType;
    private System.Windows.Forms.Button buttonClearNumber;
    private System.Windows.Forms.Button buttonClearMessage;
    private System.Windows.Forms.Button buttonAddProperty;
    private System.Windows.Forms.Button buttonRemoveProperty;
    private System.Windows.Forms.Button buttonClearProperties;
    private Lemoine.BaseControls.List.ListTextValue listProperties;
    private System.Windows.Forms.Button buttonEditProperty;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox textAcquisitionInfo;
    private System.Windows.Forms.Button buttonClearAcquisitionInfo;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label labelAcquisitionInfo;
  }
}
