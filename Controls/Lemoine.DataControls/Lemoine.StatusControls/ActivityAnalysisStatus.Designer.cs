// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.StatusControls
{
  partial class ActivityAnalysisStatus
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.resetCountButton = new System.Windows.Forms.Button();
      this.periodicRefreshCheckBox = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.hidePanelCheckBox = new System.Windows.Forms.CheckBox();
      this.machinePanel = new System.Windows.Forms.Panel();
      this.monitoredMachineSelection1 = new Lemoine.DataReferenceControls.MonitoredMachineSelection();
      this.averageTextBox = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.lastActivityAnalysisCountBox = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.lastActivityAnalysisMachineTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.lastActivityAnalysisDateTimeTextBox = new System.Windows.Forms.TextBox();
      this.refreshBtn = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.machinePanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.resetCountButton);
      this.groupBox1.Controls.Add(this.periodicRefreshCheckBox);
      this.groupBox1.Controls.Add(this.groupBox2);
      this.groupBox1.Controls.Add(this.refreshBtn);
      this.groupBox1.Location = new System.Drawing.Point(13, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(414, 450);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Activity Status";
      // 
      // resetCountButton
      // 
      this.resetCountButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.resetCountButton.Location = new System.Drawing.Point(65, 413);
      this.resetCountButton.Name = "resetCountButton";
      this.resetCountButton.Size = new System.Drawing.Size(75, 31);
      this.resetCountButton.TabIndex = 12;
      this.resetCountButton.Text = "Reset Count";
      this.resetCountButton.UseVisualStyleBackColor = true;
      this.resetCountButton.Click += new System.EventHandler(this.ResetCountButtonClick);
      // 
      // periodicRefreshCheckBox
      // 
      this.periodicRefreshCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.periodicRefreshCheckBox.Location = new System.Drawing.Point(285, 417);
      this.periodicRefreshCheckBox.Name = "periodicRefreshCheckBox";
      this.periodicRefreshCheckBox.Size = new System.Drawing.Size(104, 24);
      this.periodicRefreshCheckBox.TabIndex = 11;
      this.periodicRefreshCheckBox.Text = "periodic refresh";
      this.periodicRefreshCheckBox.UseVisualStyleBackColor = true;
      this.periodicRefreshCheckBox.CheckedChanged += new System.EventHandler(this.PeriodicRefreshCheckBoxCheckedChanged);
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.hidePanelCheckBox);
      this.groupBox2.Controls.Add(this.machinePanel);
      this.groupBox2.Controls.Add(this.averageTextBox);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.lastActivityAnalysisCountBox);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.lastActivityAnalysisMachineTextBox);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Controls.Add(this.lastActivityAnalysisDateTimeTextBox);
      this.groupBox2.Location = new System.Drawing.Point(19, 19);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(375, 388);
      this.groupBox2.TabIndex = 10;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Last Analysis";
      // 
      // hidePanelCheckBox
      // 
      this.hidePanelCheckBox.Location = new System.Drawing.Point(17, 155);
      this.hidePanelCheckBox.Name = "hidePanelCheckBox";
      this.hidePanelCheckBox.Size = new System.Drawing.Size(144, 24);
      this.hidePanelCheckBox.TabIndex = 17;
      this.hidePanelCheckBox.Text = "hide machine panel";
      this.hidePanelCheckBox.UseVisualStyleBackColor = true;
      this.hidePanelCheckBox.CheckedChanged += new System.EventHandler(this.HidePanelCheckBoxCheckedChanged);
      // 
      // machinePanel
      // 
      this.machinePanel.Controls.Add(this.monitoredMachineSelection1);
      this.machinePanel.Location = new System.Drawing.Point(6, 185);
      this.machinePanel.Name = "machinePanel";
      this.machinePanel.Size = new System.Drawing.Size(363, 186);
      this.machinePanel.TabIndex = 16;
      // 
      // monitoredMachineSelection1
      // 
      this.monitoredMachineSelection1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.monitoredMachineSelection1.Location = new System.Drawing.Point(10, 3);
      this.monitoredMachineSelection1.Name = "monitoredMachineSelection1";
      this.monitoredMachineSelection1.SelectedMonitoredMachine = null;
      this.monitoredMachineSelection1.Size = new System.Drawing.Size(345, 180);
      this.monitoredMachineSelection1.TabIndex = 13;
      // 
      // averageTextBox
      // 
      this.averageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.averageTextBox.Location = new System.Drawing.Point(130, 122);
      this.averageTextBox.Name = "averageTextBox";
      this.averageTextBox.ReadOnly = true;
      this.averageTextBox.Size = new System.Drawing.Size(231, 20);
      this.averageTextBox.TabIndex = 15;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(16, 125);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(105, 23);
      this.label4.TabIndex = 14;
      this.label4.Text = "Average / 10 Sec";
      // 
      // lastActivityAnalysisCountBox
      // 
      this.lastActivityAnalysisCountBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lastActivityAnalysisCountBox.Location = new System.Drawing.Point(130, 88);
      this.lastActivityAnalysisCountBox.Name = "lastActivityAnalysisCountBox";
      this.lastActivityAnalysisCountBox.ReadOnly = true;
      this.lastActivityAnalysisCountBox.Size = new System.Drawing.Size(231, 20);
      this.lastActivityAnalysisCountBox.TabIndex = 12;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(16, 91);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(73, 23);
      this.label3.TabIndex = 11;
      this.label3.Text = "Total Count";
      // 
      // lastActivityAnalysisMachineTextBox
      // 
      this.lastActivityAnalysisMachineTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lastActivityAnalysisMachineTextBox.Location = new System.Drawing.Point(130, 54);
      this.lastActivityAnalysisMachineTextBox.Name = "lastActivityAnalysisMachineTextBox";
      this.lastActivityAnalysisMachineTextBox.ReadOnly = true;
      this.lastActivityAnalysisMachineTextBox.Size = new System.Drawing.Size(231, 20);
      this.lastActivityAnalysisMachineTextBox.TabIndex = 10;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(16, 26);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(73, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "At";
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(16, 57);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(73, 23);
      this.label2.TabIndex = 9;
      this.label2.Text = "On machine";
      // 
      // lastActivityAnalysisDateTimeTextBox
      // 
      this.lastActivityAnalysisDateTimeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lastActivityAnalysisDateTimeTextBox.Location = new System.Drawing.Point(130, 23);
      this.lastActivityAnalysisDateTimeTextBox.Name = "lastActivityAnalysisDateTimeTextBox";
      this.lastActivityAnalysisDateTimeTextBox.ReadOnly = true;
      this.lastActivityAnalysisDateTimeTextBox.Size = new System.Drawing.Size(231, 20);
      this.lastActivityAnalysisDateTimeTextBox.TabIndex = 1;
      // 
      // refreshBtn
      // 
      this.refreshBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.refreshBtn.Location = new System.Drawing.Point(186, 413);
      this.refreshBtn.Name = "refreshBtn";
      this.refreshBtn.Size = new System.Drawing.Size(60, 31);
      this.refreshBtn.TabIndex = 8;
      this.refreshBtn.Text = "Refresh";
      this.refreshBtn.UseVisualStyleBackColor = true;
      this.refreshBtn.Click += new System.EventHandler(this.RefreshBtnClick);
      // 
      // ActivityAnalysisStatus
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox1);
      this.Name = "ActivityAnalysisStatus";
      this.Size = new System.Drawing.Size(457, 479);
      this.Load += new System.EventHandler(this.ActivityAnalysisStatus_Load);
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.machinePanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.CheckBox hidePanelCheckBox;
    private System.Windows.Forms.Panel machinePanel;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox averageTextBox;
    private System.Windows.Forms.Button resetCountButton;
    private Lemoine.DataReferenceControls.MonitoredMachineSelection monitoredMachineSelection1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox lastActivityAnalysisCountBox;
    private System.Windows.Forms.CheckBox periodicRefreshCheckBox;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox lastActivityAnalysisMachineTextBox;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Button refreshBtn;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox lastActivityAnalysisDateTimeTextBox;
    private System.Windows.Forms.GroupBox groupBox1;
    
    
    void ResetCountButtonClick(object sender, System.EventArgs e)
    {
      this.DoResetCount();
    }
    
    
    
    void HidePanelCheckBoxCheckedChanged(object sender, System.EventArgs e)
    {
      this.machinePanel.Visible = !this.machinePanel.Visible;
    }
  }
}
