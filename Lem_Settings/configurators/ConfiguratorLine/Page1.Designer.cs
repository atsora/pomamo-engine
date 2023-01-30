// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace ConfiguratorLine
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Page1));
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.buttonEditOperation = new System.Windows.Forms.Button();
      this.buttonAddMachine = new System.Windows.Forms.Button();
      this.buttonRemoveMachine = new System.Windows.Forms.Button();
      this.tableParameters = new System.Windows.Forms.TableLayoutPanel();
      this.label4 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.numericExpected = new System.Windows.Forms.NumericUpDown();
      this.numericBest = new System.Windows.Forms.NumericUpDown();
      this.unitBest = new System.Windows.Forms.ComboBox();
      this.unitExpected = new System.Windows.Forms.ComboBox();
      this.checkDedicated = new System.Windows.Forms.CheckBox();
      this.listOperations = new Lemoine.BaseControls.List.ListTextValue();
      this.listMachines = new Lemoine.BaseControls.List.ListTextValue();
      this.baseLayout.SuspendLayout();
      this.tableParameters.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericExpected)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericBest)).BeginInit();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 7;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F));
      this.baseLayout.Controls.Add(this.label3, 3, 0);
      this.baseLayout.Controls.Add(this.label2, 0, 0);
      this.baseLayout.Controls.Add(this.buttonEditOperation, 2, 0);
      this.baseLayout.Controls.Add(this.buttonAddMachine, 4, 0);
      this.baseLayout.Controls.Add(this.buttonRemoveMachine, 5, 0);
      this.baseLayout.Controls.Add(this.tableParameters, 6, 1);
      this.baseLayout.Controls.Add(this.listOperations, 0, 1);
      this.baseLayout.Controls.Add(this.listMachines, 3, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.baseLayout.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.TabIndex = 0;
      // 
      // label3
      // 
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(119, 0);
      this.label3.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(68, 24);
      this.label3.TabIndex = 4;
      this.label3.Text = "Machines";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(0, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(71, 24);
      this.label2.TabIndex = 1;
      this.label2.Text = "Operations";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonEditOperation
      // 
      this.buttonEditOperation.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonEditOperation.Image = ((System.Drawing.Image)(resources.GetObject("buttonEditOperation.Image")));
      this.buttonEditOperation.Location = new System.Drawing.Point(93, 0);
      this.buttonEditOperation.Margin = new System.Windows.Forms.Padding(0);
      this.buttonEditOperation.Name = "buttonEditOperation";
      this.buttonEditOperation.Size = new System.Drawing.Size(23, 24);
      this.buttonEditOperation.TabIndex = 7;
      this.buttonEditOperation.UseVisualStyleBackColor = true;
      this.buttonEditOperation.Click += new System.EventHandler(this.ButtonEditOperationClick);
      // 
      // buttonAddMachine
      // 
      this.buttonAddMachine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonAddMachine.Image = ((System.Drawing.Image)(resources.GetObject("buttonAddMachine.Image")));
      this.buttonAddMachine.Location = new System.Drawing.Point(187, 0);
      this.buttonAddMachine.Margin = new System.Windows.Forms.Padding(0);
      this.buttonAddMachine.Name = "buttonAddMachine";
      this.buttonAddMachine.Size = new System.Drawing.Size(24, 24);
      this.buttonAddMachine.TabIndex = 8;
      this.buttonAddMachine.UseVisualStyleBackColor = true;
      this.buttonAddMachine.Click += new System.EventHandler(this.ButtonAddMachineClick);
      // 
      // buttonRemoveMachine
      // 
      this.buttonRemoveMachine.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRemoveMachine.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemoveMachine.Image")));
      this.buttonRemoveMachine.Location = new System.Drawing.Point(211, 0);
      this.buttonRemoveMachine.Margin = new System.Windows.Forms.Padding(0);
      this.buttonRemoveMachine.Name = "buttonRemoveMachine";
      this.buttonRemoveMachine.Size = new System.Drawing.Size(24, 24);
      this.buttonRemoveMachine.TabIndex = 9;
      this.buttonRemoveMachine.UseVisualStyleBackColor = true;
      this.buttonRemoveMachine.Click += new System.EventHandler(this.ButtonRemoveMachineClick);
      // 
      // tableParameters
      // 
      this.tableParameters.ColumnCount = 1;
      this.tableParameters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableParameters.Controls.Add(this.label4, 0, 5);
      this.tableParameters.Controls.Add(this.label5, 0, 2);
      this.tableParameters.Controls.Add(this.numericExpected, 0, 6);
      this.tableParameters.Controls.Add(this.numericBest, 0, 3);
      this.tableParameters.Controls.Add(this.unitBest, 0, 4);
      this.tableParameters.Controls.Add(this.unitExpected, 0, 7);
      this.tableParameters.Controls.Add(this.checkDedicated, 0, 9);
      this.tableParameters.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableParameters.Location = new System.Drawing.Point(235, 24);
      this.tableParameters.Margin = new System.Windows.Forms.Padding(0);
      this.tableParameters.Name = "tableParameters";
      this.tableParameters.RowCount = 10;
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
      this.tableParameters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableParameters.Size = new System.Drawing.Size(115, 226);
      this.tableParameters.TabIndex = 14;
      // 
      // label4
      // 
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Enabled = false;
      this.label4.Location = new System.Drawing.Point(0, 119);
      this.label4.Margin = new System.Windows.Forms.Padding(0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(115, 25);
      this.label4.TabIndex = 6;
      this.label4.Text = "Expected performance";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Enabled = false;
      this.label5.Location = new System.Drawing.Point(0, 47);
      this.label5.Margin = new System.Windows.Forms.Padding(0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(115, 25);
      this.label5.TabIndex = 7;
      this.label5.Text = "Best performance";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // numericExpected
      // 
      this.numericExpected.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericExpected.Enabled = false;
      this.numericExpected.Location = new System.Drawing.Point(3, 144);
      this.numericExpected.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.numericExpected.Maximum = new decimal(new int[] {
      100000,
      0,
      0,
      0});
      this.numericExpected.Minimum = new decimal(new int[] {
      1,
      0,
      0,
      0});
      this.numericExpected.Name = "numericExpected";
      this.numericExpected.Size = new System.Drawing.Size(112, 20);
      this.numericExpected.TabIndex = 10;
      this.numericExpected.Value = new decimal(new int[] {
      1,
      0,
      0,
      0});
      // 
      // numericBest
      // 
      this.numericBest.Dock = System.Windows.Forms.DockStyle.Fill;
      this.numericBest.Enabled = false;
      this.numericBest.Location = new System.Drawing.Point(3, 72);
      this.numericBest.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.numericBest.Maximum = new decimal(new int[] {
      100000,
      0,
      0,
      0});
      this.numericBest.Minimum = new decimal(new int[] {
      1,
      0,
      0,
      0});
      this.numericBest.Name = "numericBest";
      this.numericBest.Size = new System.Drawing.Size(112, 20);
      this.numericBest.TabIndex = 9;
      this.numericBest.Value = new decimal(new int[] {
      1,
      0,
      0,
      0});
      // 
      // unitBest
      // 
      this.unitBest.Dock = System.Windows.Forms.DockStyle.Fill;
      this.unitBest.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.unitBest.Enabled = false;
      this.unitBest.FormattingEnabled = true;
      this.unitBest.Items.AddRange(new object[] {
      "cycles per hour",
      "cycles per minute",
      "seconds per cycle",
      "minutes per cycle"});
      this.unitBest.Location = new System.Drawing.Point(3, 94);
      this.unitBest.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.unitBest.Name = "unitBest";
      this.unitBest.Size = new System.Drawing.Size(112, 21);
      this.unitBest.TabIndex = 3;
      // 
      // unitExpected
      // 
      this.unitExpected.Dock = System.Windows.Forms.DockStyle.Fill;
      this.unitExpected.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.unitExpected.Enabled = false;
      this.unitExpected.FormattingEnabled = true;
      this.unitExpected.Items.AddRange(new object[] {
      "cycles per hour",
      "cycles per minute",
      "seconds per cycle",
      "minutes per cycle"});
      this.unitExpected.Location = new System.Drawing.Point(3, 166);
      this.unitExpected.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.unitExpected.Name = "unitExpected";
      this.unitExpected.Size = new System.Drawing.Size(112, 21);
      this.unitExpected.TabIndex = 2;
      // 
      // checkDedicated
      // 
      this.checkDedicated.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.checkDedicated.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkDedicated.Location = new System.Drawing.Point(0, 201);
      this.checkDedicated.Margin = new System.Windows.Forms.Padding(0);
      this.checkDedicated.Name = "checkDedicated";
      this.checkDedicated.Size = new System.Drawing.Size(115, 25);
      this.checkDedicated.TabIndex = 8;
      this.checkDedicated.Text = "Dedicated";
      this.checkDedicated.UseVisualStyleBackColor = true;
      this.checkDedicated.CheckedChanged += new System.EventHandler(this.CheckDedicatedCheckedChanged);
      // 
      // listOperations
      // 
      this.listOperations.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listOperations, 3);
      this.listOperations.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listOperations.Location = new System.Drawing.Point(0, 24);
      this.listOperations.Margin = new System.Windows.Forms.Padding(0);
      this.listOperations.Name = "listOperations";
      this.listOperations.SelectedIndex = -1;
      this.listOperations.SelectedIndexes = null;
      this.listOperations.SelectedText = "";
      this.listOperations.SelectedTexts = null;
      this.listOperations.SelectedValue = null;
      this.listOperations.SelectedValues = null;
      this.listOperations.Size = new System.Drawing.Size(116, 226);
      this.listOperations.Sorted = true;
      this.listOperations.TabIndex = 15;
      this.listOperations.ItemChanged += new System.Action<string, object>(this.ListOperationsItemChanged);
      this.listOperations.ItemDoubleClicked += new System.Action<string, object>(this.ListOperationsItemDoubleClicked);
      // 
      // listMachines
      // 
      this.listMachines.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.listMachines, 3);
      this.listMachines.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listMachines.Location = new System.Drawing.Point(119, 24);
      this.listMachines.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.listMachines.Name = "listMachines";
      this.listMachines.SelectedIndex = -1;
      this.listMachines.SelectedIndexes = null;
      this.listMachines.SelectedText = "";
      this.listMachines.SelectedTexts = null;
      this.listMachines.SelectedValue = null;
      this.listMachines.SelectedValues = null;
      this.listMachines.Size = new System.Drawing.Size(116, 226);
      this.listMachines.Sorted = true;
      this.listMachines.TabIndex = 16;
      this.listMachines.ItemChanged += new System.Action<string, object>(this.ListMachinesItemChanged);
      // 
      // Page1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.baseLayout);
      this.Name = "Page1";
      this.Size = new System.Drawing.Size(350, 250);
      this.baseLayout.ResumeLayout(false);
      this.tableParameters.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericExpected)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericBest)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.ComboBox unitExpected;
    private System.Windows.Forms.ComboBox unitBest;
    private System.Windows.Forms.NumericUpDown numericBest;
    private System.Windows.Forms.NumericUpDown numericExpected;
    private System.Windows.Forms.CheckBox checkDedicated;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TableLayoutPanel tableParameters;
    private Lemoine.BaseControls.List.ListTextValue listMachines;
    private Lemoine.BaseControls.List.ListTextValue listOperations;
    private System.Windows.Forms.Button buttonRemoveMachine;
    private System.Windows.Forms.Button buttonAddMachine;
    private System.Windows.Forms.Button buttonEditOperation;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TableLayoutPanel baseLayout;
  }
}
