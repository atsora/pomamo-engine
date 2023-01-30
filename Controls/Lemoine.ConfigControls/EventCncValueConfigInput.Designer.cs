// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.ConfigControls
{
  partial class EventCncValueConfigInput
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
      this.components = new System.ComponentModel.Container();
      this.nameTextBox = new System.Windows.Forms.TextBox();
      this.messageTextBox = new System.Windows.Forms.TextBox();
      this.conditionTextBox = new System.Windows.Forms.TextBox();
      this.nameLabel = new System.Windows.Forms.Label();
      this.messageLabel = new System.Windows.Forms.Label();
      this.conditionLabel = new System.Windows.Forms.Label();
      this.conditionErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.conditionErrorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // nameTextBox
      // 
      this.nameTextBox.Location = new System.Drawing.Point(108, 3);
      this.nameTextBox.Name = "nameTextBox";
      this.nameTextBox.Size = new System.Drawing.Size(166, 20);
      this.nameTextBox.TabIndex = 0;
      // 
      // messageTextBox
      // 
      this.messageTextBox.Location = new System.Drawing.Point(108, 29);
      this.messageTextBox.Name = "messageTextBox";
      this.messageTextBox.Size = new System.Drawing.Size(166, 20);
      this.messageTextBox.TabIndex = 1;
      // 
      // conditionTextBox
      // 
      this.conditionTextBox.Location = new System.Drawing.Point(108, 55);
      this.conditionTextBox.Multiline = true;
      this.conditionTextBox.Name = "conditionTextBox";
      this.conditionTextBox.Size = new System.Drawing.Size(166, 59);
      this.conditionTextBox.TabIndex = 2;
      this.conditionTextBox.Validated += new System.EventHandler(this.ConditionTextBoxValidated);
      // 
      // nameLabel
      // 
      this.nameLabel.Location = new System.Drawing.Point(3, 6);
      this.nameLabel.Name = "nameLabel";
      this.nameLabel.Size = new System.Drawing.Size(100, 23);
      this.nameLabel.TabIndex = 3;
      this.nameLabel.Text = "label1";
      // 
      // messageLabel
      // 
      this.messageLabel.Location = new System.Drawing.Point(3, 32);
      this.messageLabel.Name = "messageLabel";
      this.messageLabel.Size = new System.Drawing.Size(100, 23);
      this.messageLabel.TabIndex = 4;
      this.messageLabel.Text = "label2";
      // 
      // conditionLabel
      // 
      this.conditionLabel.Location = new System.Drawing.Point(3, 58);
      this.conditionLabel.Name = "conditionLabel";
      this.conditionLabel.Size = new System.Drawing.Size(100, 23);
      this.conditionLabel.TabIndex = 5;
      this.conditionLabel.Text = "label3";
      // 
      // conditionErrorProvider
      // 
      this.conditionErrorProvider.BlinkRate = 1000;
      this.conditionErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.AlwaysBlink;
      this.conditionErrorProvider.ContainerControl = this;
      // 
      // EventCncValueConfigInput
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.conditionLabel);
      this.Controls.Add(this.messageLabel);
      this.Controls.Add(this.nameLabel);
      this.Controls.Add(this.conditionTextBox);
      this.Controls.Add(this.messageTextBox);
      this.Controls.Add(this.nameTextBox);
      this.Name = "EventCncValueConfigInput";
      this.Size = new System.Drawing.Size(307, 117);
      this.Load += new System.EventHandler(this.EventCncValueConfigInputLoad);
      ((System.ComponentModel.ISupportInitialize)(this.conditionErrorProvider)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
    private System.Windows.Forms.ErrorProvider conditionErrorProvider;
    private System.Windows.Forms.Label conditionLabel;
    private System.Windows.Forms.Label messageLabel;
    private System.Windows.Forms.Label nameLabel;
    private System.Windows.Forms.TextBox conditionTextBox;
    private System.Windows.Forms.TextBox messageTextBox;
    private System.Windows.Forms.TextBox nameTextBox;
  }
}
