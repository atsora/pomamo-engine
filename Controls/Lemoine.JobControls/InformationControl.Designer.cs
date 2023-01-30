// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.JobControls
{
  partial class InformationControl
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
      this.groupBox = new System.Windows.Forms.GroupBox();
      this.pathControl = new Lemoine.JobControls.PathControl();
      this.sequenceControl = new Lemoine.JobControls.SequenceControl();
      this.simpleOperationControl = new Lemoine.JobControls.SimpleOperationControl();
      this.partControl = new Lemoine.JobControls.PartControl();
      this.jobControl = new Lemoine.JobControls.JobControl();
      this.operationControl = new Lemoine.JobControls.OperationControl();
      this.intermediateWorkPieceControl = new Lemoine.JobControls.IntermediateWorkPieceControl();
      this.componentControl = new Lemoine.JobControls.ComponentControl();
      this.projectControl = new Lemoine.JobControls.ProjectControl();
      this.workOrderControl = new Lemoine.JobControls.WorkOrderControl();
      this.groupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox
      // 
      this.groupBox.Controls.Add(this.pathControl);
      this.groupBox.Controls.Add(this.sequenceControl);
      this.groupBox.Controls.Add(this.simpleOperationControl);
      this.groupBox.Controls.Add(this.partControl);
      this.groupBox.Controls.Add(this.jobControl);
      this.groupBox.Controls.Add(this.operationControl);
      this.groupBox.Controls.Add(this.intermediateWorkPieceControl);
      this.groupBox.Controls.Add(this.componentControl);
      this.groupBox.Controls.Add(this.projectControl);
      this.groupBox.Controls.Add(this.workOrderControl);
      this.groupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox.Location = new System.Drawing.Point(0, 0);
      this.groupBox.Name = "groupBox";
      this.groupBox.Size = new System.Drawing.Size(860, 625);
      this.groupBox.TabIndex = 9;
      this.groupBox.TabStop = false;
      // 
      // pathControl
      // 
      this.pathControl.Location = new System.Drawing.Point(396, 19);
      this.pathControl.Name = "pathControl";
      this.pathControl.Operation = null;
      this.pathControl.ParentType = null;
      this.pathControl.Path = null;
      this.pathControl.Size = new System.Drawing.Size(434, 120);
      this.pathControl.TabIndex = 19;
      // 
      // sequenceControl
      // 
      this.sequenceControl.AutoSize = true;
      this.sequenceControl.Location = new System.Drawing.Point(405, 132);
      this.sequenceControl.Name = "sequenceControl";
      this.sequenceControl.Operation = null;
      this.sequenceControl.ParentType = null;
      this.sequenceControl.Path = null;
      this.sequenceControl.Sequence = null;
      this.sequenceControl.ShowPath = true;
      this.sequenceControl.Size = new System.Drawing.Size(428, 560);
      this.sequenceControl.TabIndex = 18;
      // 
      // simpleOperationControl
      // 
      this.simpleOperationControl.AutoSize = true;
      this.simpleOperationControl.Location = new System.Drawing.Point(155, 156);
      this.simpleOperationControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.simpleOperationControl.Name = "simpleOperationControl";
      this.simpleOperationControl.Size = new System.Drawing.Size(374, 372);
      this.simpleOperationControl.TabIndex = 16;
      // 
      // partControl
      // 
      this.partControl.AutoSize = true;
      this.partControl.Location = new System.Drawing.Point(113, 46);
      this.partControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.partControl.Name = "partControl";
      this.partControl.Size = new System.Drawing.Size(353, 170);
      this.partControl.TabIndex = 15;
      // 
      // jobControl
      // 
      this.jobControl.AutoSize = true;
      this.jobControl.Location = new System.Drawing.Point(5, 435);
      this.jobControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.jobControl.Name = "jobControl";
      this.jobControl.Size = new System.Drawing.Size(353, 135);
      this.jobControl.TabIndex = 14;
      // 
      // operationControl
      // 
      this.operationControl.Location = new System.Drawing.Point(4, 15);
      this.operationControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.operationControl.Name = "operationControl";
      this.operationControl.Size = new System.Drawing.Size(353, 231);
      this.operationControl.TabIndex = 13;
      // 
      // intermediateWorkPieceControl
      // 
      this.intermediateWorkPieceControl.AutoSize = true;
      this.intermediateWorkPieceControl.Location = new System.Drawing.Point(4, 15);
      this.intermediateWorkPieceControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.intermediateWorkPieceControl.Name = "intermediateWorkPieceControl";
      this.intermediateWorkPieceControl.Size = new System.Drawing.Size(353, 181);
      this.intermediateWorkPieceControl.TabIndex = 12;
      // 
      // componentControl
      // 
      this.componentControl.AutoSize = true;
      this.componentControl.Location = new System.Drawing.Point(4, 15);
      this.componentControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.componentControl.Name = "componentControl";
      this.componentControl.ParentType = null;
      this.componentControl.Size = new System.Drawing.Size(353, 201);
      this.componentControl.TabIndex = 11;
      // 
      // projectControl
      // 
      this.projectControl.Location = new System.Drawing.Point(4, 15);
      this.projectControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.projectControl.Name = "projectControl";
      this.projectControl.Size = new System.Drawing.Size(353, 120);
      this.projectControl.TabIndex = 10;
      // 
      // workOrderControl
      // 
      this.workOrderControl.AutoSize = true;
      this.workOrderControl.Location = new System.Drawing.Point(4, 15);
      this.workOrderControl.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.workOrderControl.Name = "workOrderControl";
      this.workOrderControl.Size = new System.Drawing.Size(353, 135);
      this.workOrderControl.TabIndex = 9;
      // 
      // InformationControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox);
      this.Name = "InformationControl";
      this.Size = new System.Drawing.Size(860, 628);
      this.groupBox.ResumeLayout(false);
      this.groupBox.PerformLayout();
      this.ResumeLayout(false);
    }
    private Lemoine.JobControls.PathControl pathControl;
    private Lemoine.JobControls.SequenceControl sequenceControl;
    private System.Windows.Forms.GroupBox groupBox;
    private Lemoine.JobControls.SimpleOperationControl simpleOperationControl;
    private Lemoine.JobControls.PartControl partControl;
    private Lemoine.JobControls.JobControl jobControl;
    private Lemoine.JobControls.OperationControl operationControl;
    private Lemoine.JobControls.IntermediateWorkPieceControl intermediateWorkPieceControl;
    private Lemoine.JobControls.ComponentControl componentControl;
    private Lemoine.JobControls.ProjectControl projectControl;
    private Lemoine.JobControls.WorkOrderControl workOrderControl;
    
  }
}
