// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_CncGUI
{
  partial class MainForm
  {
    /// <summary>
    /// Designer variable used to keep track of non-visual components.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    
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
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
      this.dataGridView = new System.Windows.Forms.DataGridView();
      this.Key = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.fileLabel = new System.Windows.Forms.Label();
      this.openButton = new System.Windows.Forms.Button();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.selectButton = new System.Windows.Forms.Button();
      this.configParamButton = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // openFileDialog
      // 
      this.openFileDialog.DefaultExt = "xml";
      this.openFileDialog.Filter = "XML files|*.xml|All files|*.*";
      // 
      // dataGridView
      // 
      this.dataGridView.AllowUserToAddRows = false;
      this.dataGridView.AllowUserToDeleteRows = false;
      this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
      this.dataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
      this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Key,
            this.Value});
      this.dataGridView.Location = new System.Drawing.Point(14, 78);
      this.dataGridView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.dataGridView.MultiSelect = false;
      this.dataGridView.Name = "dataGridView";
      this.dataGridView.ReadOnly = true;
      this.dataGridView.RowHeadersVisible = false;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.dataGridView.RowsDefaultCellStyle = dataGridViewCellStyle1;
      this.dataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
      this.dataGridView.ShowCellErrors = false;
      this.dataGridView.ShowCellToolTips = false;
      this.dataGridView.ShowEditingIcon = false;
      this.dataGridView.ShowRowErrors = false;
      this.dataGridView.Size = new System.Drawing.Size(408, 396);
      this.dataGridView.TabIndex = 0;
      // 
      // Key
      // 
      this.Key.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
      this.Key.FillWeight = 81.21828F;
      this.Key.HeaderText = "Key";
      this.Key.Name = "Key";
      this.Key.ReadOnly = true;
      this.Key.Width = 120;
      // 
      // Value
      // 
      this.Value.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.Value.FillWeight = 118.7817F;
      this.Value.HeaderText = "Value";
      this.Value.Name = "Value";
      this.Value.ReadOnly = true;
      // 
      // fileLabel
      // 
      this.fileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.fileLabel.Location = new System.Drawing.Point(15, 44);
      this.fileLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.fileLabel.Name = "fileLabel";
      this.fileLabel.Size = new System.Drawing.Size(407, 27);
      this.fileLabel.TabIndex = 1;
      this.fileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // openButton
      // 
      this.openButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.openButton.Location = new System.Drawing.Point(327, 14);
      this.openButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.openButton.Name = "openButton";
      this.openButton.Size = new System.Drawing.Size(96, 27);
      this.openButton.TabIndex = 2;
      this.openButton.Text = "Open config...";
      this.openButton.UseVisualStyleBackColor = true;
      this.openButton.Click += new System.EventHandler(this.OpenButtonClick);
      // 
      // timer
      // 
      this.timer.Enabled = true;
      this.timer.Interval = 1000;
      this.timer.Tick += new System.EventHandler(this.TimerTick);
      // 
      // selectButton
      // 
      this.selectButton.Location = new System.Drawing.Point(14, 14);
      this.selectButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.selectButton.Name = "selectButton";
      this.selectButton.Size = new System.Drawing.Size(153, 27);
      this.selectButton.TabIndex = 3;
      this.selectButton.Text = "Select Cnc Acquisition...";
      this.selectButton.UseVisualStyleBackColor = true;
      this.selectButton.Click += new System.EventHandler(this.SelectButtonClick);
      // 
      // configParamButton
      // 
      this.configParamButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.configParamButton.Location = new System.Drawing.Point(174, 14);
      this.configParamButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.configParamButton.Name = "configParamButton";
      this.configParamButton.Size = new System.Drawing.Size(146, 27);
      this.configParamButton.TabIndex = 4;
      this.configParamButton.Text = "Select Config+Param";
      this.configParamButton.UseVisualStyleBackColor = true;
      this.configParamButton.Click += new System.EventHandler(this.ConfigParamButtonClick);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(436, 488);
      this.Controls.Add(this.configParamButton);
      this.Controls.Add(this.selectButton);
      this.Controls.Add(this.openButton);
      this.Controls.Add(this.fileLabel);
      this.Controls.Add(this.dataGridView);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "MainForm";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
      this.Text = "Lem_CncGUI";
      this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormFormClosed);
      ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.Button configParamButton;
    private System.Windows.Forms.Button selectButton;
    private System.Windows.Forms.DataGridView dataGridView;
    private System.Windows.Forms.Timer timer;
    private System.Windows.Forms.Label fileLabel;
    private System.Windows.Forms.Button openButton;
    private System.Windows.Forms.OpenFileDialog openFileDialog;
    private System.Windows.Forms.DataGridViewTextBoxColumn Key;
    private System.Windows.Forms.DataGridViewTextBoxColumn Value;
  }
}
