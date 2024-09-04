// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorGoals
{
  partial class Page3
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.comboboxState = new Lemoine.BaseControls.ComboboxTextValue();
      this.checkEnable = new System.Windows.Forms.CheckBox();
      this.label3 = new System.Windows.Forms.Label();
      this.stackedWidget = new Lemoine.BaseControls.StackedWidget();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.treeMachines = new Lemoine.DataReferenceControls.DisplayableTreeView();
      this.numericGoal = new System.Windows.Forms.NumericUpDown();
      this.numericDefault = new System.Windows.Forms.NumericUpDown();
      this.labelUnit = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.checkOverwrite = new System.Windows.Forms.CheckBox();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.label4 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.stackedWidget.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericGoal)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericDefault)).BeginInit();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 91F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
      this.baseLayout.Controls.Add(this.label1, 0, 0);
      this.baseLayout.Controls.Add(this.comboboxState, 1, 0);
      this.baseLayout.Controls.Add(this.checkEnable, 2, 1);
      this.baseLayout.Controls.Add(this.label3, 0, 1);
      this.baseLayout.Controls.Add(this.stackedWidget, 0, 2);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 3;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 122F));
      this.baseLayout.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(0, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(88, 21);
      this.label1.TabIndex = 4;
      this.label1.Text = "Machine state";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // comboboxState
      // 
      this.baseLayout.SetColumnSpan(this.comboboxState, 2);
      this.comboboxState.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboboxState.Location = new System.Drawing.Point(94, 0);
      this.comboboxState.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
      this.comboboxState.Name = "comboboxState";
      this.comboboxState.Size = new System.Drawing.Size(276, 21);
      this.comboboxState.Sorted = true;
      this.comboboxState.TabIndex = 5;
      this.comboboxState.ItemChanged += new System.Action<string, object>(this.ComboboxStateItemChanged);
      // 
      // checkEnable
      // 
      this.checkEnable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkEnable.Location = new System.Drawing.Point(268, 27);
      this.checkEnable.Name = "checkEnable";
      this.checkEnable.Size = new System.Drawing.Size(99, 18);
      this.checkEnable.TabIndex = 7;
      this.checkEnable.UseVisualStyleBackColor = true;
      this.checkEnable.CheckedChanged += new System.EventHandler(this.CheckEnableCheckedChanged);
      // 
      // label3
      // 
      this.baseLayout.SetColumnSpan(this.label3, 2);
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(0, 24);
      this.label3.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(262, 24);
      this.label3.TabIndex = 10;
      this.label3.Text = "Enable goals for this machine state";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // stackedWidget
      // 
      this.baseLayout.SetColumnSpan(this.stackedWidget, 3);
      this.stackedWidget.Controls.Add(this.tabPage1);
      this.stackedWidget.Controls.Add(this.tabPage2);
      this.stackedWidget.Dock = System.Windows.Forms.DockStyle.Fill;
      this.stackedWidget.Location = new System.Drawing.Point(0, 48);
      this.stackedWidget.Margin = new System.Windows.Forms.Padding(0);
      this.stackedWidget.Name = "stackedWidget";
      this.stackedWidget.SelectedIndex = 0;
      this.stackedWidget.Size = new System.Drawing.Size(370, 242);
      this.stackedWidget.TabIndex = 11;
      // 
      // tabPage1
      // 
      this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage1.Controls.Add(this.tableLayoutPanel1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Size = new System.Drawing.Size(362, 216);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 106F));
      this.tableLayoutPanel1.Controls.Add(this.treeMachines, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.numericGoal, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.numericDefault, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.labelUnit, 2, 2);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.checkOverwrite, 0, 2);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(362, 216);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // treeMachines
      // 
      this.treeMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tableLayoutPanel1.SetColumnSpan(this.treeMachines, 3);
      this.treeMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeMachines.Location = new System.Drawing.Point(0, 24);
      this.treeMachines.Margin = new System.Windows.Forms.Padding(0);
      this.treeMachines.Name = "treeMachines";
      this.treeMachines.Size = new System.Drawing.Size(362, 168);
      this.treeMachines.TabIndex = 1;
      this.treeMachines.SelectionChanged += new System.Action(this.TreeSelectionChanged);
      // 
      // numericGoal
      // 
      this.numericGoal.DecimalPlaces = 2;
      this.numericGoal.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericGoal.Enabled = false;
      this.numericGoal.Location = new System.Drawing.Point(82, 195);
      this.numericGoal.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
      this.numericGoal.Maximum = new decimal(new int[] {
      1000000,
      0,
      0,
      0});
      this.numericGoal.Name = "numericGoal";
      this.numericGoal.Size = new System.Drawing.Size(174, 20);
      this.numericGoal.TabIndex = 3;
      this.numericGoal.ValueChanged += new System.EventHandler(this.NumericGoalValueChanged);
      // 
      // numericDefault
      // 
      this.numericDefault.DecimalPlaces = 2;
      this.numericDefault.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericDefault.Location = new System.Drawing.Point(259, 3);
      this.numericDefault.Name = "numericDefault";
      this.numericDefault.Size = new System.Drawing.Size(100, 20);
      this.numericDefault.TabIndex = 8;
      this.numericDefault.ValueChanged += new System.EventHandler(this.NumericDefaultValueChanged);
      // 
      // labelUnit
      // 
      this.labelUnit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelUnit.Location = new System.Drawing.Point(259, 192);
      this.labelUnit.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.labelUnit.Name = "labelUnit";
      this.labelUnit.Size = new System.Drawing.Size(103, 24);
      this.labelUnit.TabIndex = 3;
      this.labelUnit.Text = "Unit:";
      this.labelUnit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label2
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.label2, 2);
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(253, 24);
      this.label2.TabIndex = 9;
      this.label2.Text = "Default value associated";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // checkOverwrite
      // 
      this.checkOverwrite.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkOverwrite.Enabled = false;
      this.checkOverwrite.Location = new System.Drawing.Point(0, 195);
      this.checkOverwrite.Margin = new System.Windows.Forms.Padding(0, 3, 3, 0);
      this.checkOverwrite.Name = "checkOverwrite";
      this.checkOverwrite.Size = new System.Drawing.Size(79, 21);
      this.checkOverwrite.TabIndex = 6;
      this.checkOverwrite.Text = "overwrite";
      this.checkOverwrite.UseVisualStyleBackColor = true;
      this.checkOverwrite.CheckedChanged += new System.EventHandler(this.CheckOverwriteCheckedChanged);
      // 
      // tabPage2
      // 
      this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
      this.tabPage2.Controls.Add(this.label4);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Size = new System.Drawing.Size(362, 216);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label4.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label4.Location = new System.Drawing.Point(0, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(362, 216);
      this.label4.TabIndex = 0;
      this.label4.Text = "Goals not activated for this machine state";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // Page3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page3";
      this.Size = new System.Drawing.Size(370, 290);
      this.baseLayout.ResumeLayout(false);
      this.stackedWidget.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericGoal)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericDefault)).EndInit();
      this.tabPage2.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Label labelUnit;
    private System.Windows.Forms.NumericUpDown numericGoal;
    private Lemoine.DataReferenceControls.DisplayableTreeView treeMachines;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Label label1;
    private Lemoine.BaseControls.ComboboxTextValue comboboxState;
    private System.Windows.Forms.CheckBox checkOverwrite;
    private System.Windows.Forms.NumericUpDown numericDefault;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox checkEnable;
    private System.Windows.Forms.Label label3;
    private Lemoine.BaseControls.StackedWidget stackedWidget;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Label label4;
  }
}
