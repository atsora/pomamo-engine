// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.StatusControls
{
  partial class AnalysisStatus
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
      this.analysisStatusGrpBx = new System.Windows.Forms.GroupBox();
      this.periodicRefreshCheckBox = new System.Windows.Forms.CheckBox();
      this.countTimeoutLabel = new System.Windows.Forms.Label();
      this.timeoutLabel = new System.Windows.Forms.Label();
      this.refreshBtn = new System.Windows.Forms.Button();
      this.countErrorLbl = new System.Windows.Forms.Label();
      this.countNewLbl = new System.Windows.Forms.Label();
      this.newLbl = new System.Windows.Forms.Label();
      this.errorLbl = new System.Windows.Forms.Label();
      this.analysisStatusGrpBx.SuspendLayout();
      this.SuspendLayout();
      // 
      // analysisStatusGrpBx
      // 
      this.analysisStatusGrpBx.Controls.Add(this.periodicRefreshCheckBox);
      this.analysisStatusGrpBx.Controls.Add(this.countTimeoutLabel);
      this.analysisStatusGrpBx.Controls.Add(this.timeoutLabel);
      this.analysisStatusGrpBx.Controls.Add(this.refreshBtn);
      this.analysisStatusGrpBx.Controls.Add(this.countErrorLbl);
      this.analysisStatusGrpBx.Controls.Add(this.countNewLbl);
      this.analysisStatusGrpBx.Controls.Add(this.newLbl);
      this.analysisStatusGrpBx.Controls.Add(this.errorLbl);
      this.analysisStatusGrpBx.Dock = System.Windows.Forms.DockStyle.Fill;
      this.analysisStatusGrpBx.Location = new System.Drawing.Point(0, 0);
      this.analysisStatusGrpBx.Name = "analysisStatusGrpBx";
      this.analysisStatusGrpBx.Size = new System.Drawing.Size(248, 178);
      this.analysisStatusGrpBx.TabIndex = 0;
      this.analysisStatusGrpBx.TabStop = false;
      this.analysisStatusGrpBx.Text = "Analysis Status";
      // 
      // periodicRefreshCheckBox
      // 
      this.periodicRefreshCheckBox.Location = new System.Drawing.Point(138, 90);
      this.periodicRefreshCheckBox.Name = "periodicRefreshCheckBox";
      this.periodicRefreshCheckBox.Size = new System.Drawing.Size(104, 24);
      this.periodicRefreshCheckBox.TabIndex = 11;
      this.periodicRefreshCheckBox.Text = "periodic refresh";
      this.periodicRefreshCheckBox.UseVisualStyleBackColor = true;
      this.periodicRefreshCheckBox.CheckedChanged += new System.EventHandler(this.PeriodicRefreshCheckBoxCheckedChanged);
      // 
      // countTimeoutLabel
      // 
      this.countTimeoutLabel.Location = new System.Drawing.Point(93, 60);
      this.countTimeoutLabel.Name = "countTimeoutLabel";
      this.countTimeoutLabel.Size = new System.Drawing.Size(149, 23);
      this.countTimeoutLabel.TabIndex = 10;
      this.countTimeoutLabel.Text = "countTimeout";
      this.countTimeoutLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // timeoutLabel
      // 
      this.timeoutLabel.Location = new System.Drawing.Point(6, 60);
      this.timeoutLabel.Name = "timeoutLabel";
      this.timeoutLabel.Size = new System.Drawing.Size(54, 23);
      this.timeoutLabel.TabIndex = 9;
      this.timeoutLabel.Text = "Timeout:";
      this.timeoutLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // refreshBtn
      // 
      this.refreshBtn.Location = new System.Drawing.Point(6, 86);
      this.refreshBtn.Name = "refreshBtn";
      this.refreshBtn.Size = new System.Drawing.Size(60, 31);
      this.refreshBtn.TabIndex = 7;
      this.refreshBtn.Text = "Refresh";
      this.refreshBtn.UseVisualStyleBackColor = true;
      this.refreshBtn.Click += new System.EventHandler(this.RefreshBtnClick);
      // 
      // countErrorLbl
      // 
      this.countErrorLbl.Location = new System.Drawing.Point(93, 37);
      this.countErrorLbl.Name = "countErrorLbl";
      this.countErrorLbl.Size = new System.Drawing.Size(149, 23);
      this.countErrorLbl.TabIndex = 6;
      this.countErrorLbl.Text = "countError";
      this.countErrorLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // countNewLbl
      // 
      this.countNewLbl.Location = new System.Drawing.Point(93, 16);
      this.countNewLbl.Name = "countNewLbl";
      this.countNewLbl.Size = new System.Drawing.Size(149, 23);
      this.countNewLbl.TabIndex = 4;
      this.countNewLbl.Text = "countNew";
      this.countNewLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // newLbl
      // 
      this.newLbl.Location = new System.Drawing.Point(6, 16);
      this.newLbl.Name = "newLbl";
      this.newLbl.Size = new System.Drawing.Size(54, 23);
      this.newLbl.TabIndex = 3;
      this.newLbl.Text = "New:";
      this.newLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // errorLbl
      // 
      this.errorLbl.Location = new System.Drawing.Point(6, 37);
      this.errorLbl.Name = "errorLbl";
      this.errorLbl.Size = new System.Drawing.Size(54, 23);
      this.errorLbl.TabIndex = 1;
      this.errorLbl.Text = "Error: ";
      this.errorLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // AnalysisStatus
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.analysisStatusGrpBx);
      this.Name = "AnalysisStatus";
      this.Size = new System.Drawing.Size(248, 178);
      this.Load += new System.EventHandler(this.AnalysisStatus_Load);
      this.analysisStatusGrpBx.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.CheckBox periodicRefreshCheckBox;
    private System.Windows.Forms.Label timeoutLabel;
    private System.Windows.Forms.Label countTimeoutLabel;
    private System.Windows.Forms.Label errorLbl;
    private System.Windows.Forms.Label newLbl;
    private System.Windows.Forms.Label countErrorLbl;
    private System.Windows.Forms.Label countNewLbl;
    private System.Windows.Forms.Button refreshBtn;
    private System.Windows.Forms.GroupBox analysisStatusGrpBx;
  }
}
