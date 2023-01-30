// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lem_GDBMigrator
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
      this.connectionGroupBox = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.passwordTextBox = new System.Windows.Forms.TextBox();
      this.serverLabel = new System.Windows.Forms.Label();
      this.userTextBox = new System.Windows.Forms.TextBox();
      this.portLabel = new System.Windows.Forms.Label();
      this.databaseTextBox = new System.Windows.Forms.TextBox();
      this.databaseLabel = new System.Windows.Forms.Label();
      this.portTextBox = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.serverTextBox = new System.Windows.Forms.TextBox();
      this.userLabel = new System.Windows.Forms.Label();
      this.buttonRefresh = new System.Windows.Forms.Button();
      this.connectionStringTextBox = new System.Windows.Forms.RichTextBox();
      this.migrationsGroupBox = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.upgradeButton = new System.Windows.Forms.Button();
      this.versionNumericUpDown = new System.Windows.Forms.NumericUpDown();
      this.migrateButton = new System.Windows.Forms.Button();
      this.migrationsTextBox = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.connectionGroupBox.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.migrationsGroupBox.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.versionNumericUpDown)).BeginInit();
      this.baseLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // connectionGroupBox
      // 
      this.connectionGroupBox.Controls.Add(this.tableLayoutPanel2);
      this.connectionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.connectionGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.connectionGroupBox.Location = new System.Drawing.Point(3, 3);
      this.connectionGroupBox.Name = "connectionGroupBox";
      this.connectionGroupBox.Size = new System.Drawing.Size(398, 204);
      this.connectionGroupBox.TabIndex = 0;
      this.connectionGroupBox.TabStop = false;
      this.connectionGroupBox.Text = "Connection parameters";
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 3;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 63F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 118F));
      this.tableLayoutPanel2.Controls.Add(this.passwordTextBox, 1, 4);
      this.tableLayoutPanel2.Controls.Add(this.serverLabel, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.userTextBox, 1, 3);
      this.tableLayoutPanel2.Controls.Add(this.portLabel, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.databaseTextBox, 1, 2);
      this.tableLayoutPanel2.Controls.Add(this.databaseLabel, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.portTextBox, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.label1, 0, 4);
      this.tableLayoutPanel2.Controls.Add(this.serverTextBox, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.userLabel, 0, 3);
      this.tableLayoutPanel2.Controls.Add(this.buttonRefresh, 2, 6);
      this.tableLayoutPanel2.Controls.Add(this.connectionStringTextBox, 0, 5);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 7;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(392, 185);
      this.tableLayoutPanel2.TabIndex = 2;
      // 
      // passwordTextBox
      // 
      this.tableLayoutPanel2.SetColumnSpan(this.passwordTextBox, 2);
      this.passwordTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.passwordTextBox.Location = new System.Drawing.Point(66, 95);
      this.passwordTextBox.Name = "passwordTextBox";
      this.passwordTextBox.Size = new System.Drawing.Size(323, 20);
      this.passwordTextBox.TabIndex = 9;
      // 
      // serverLabel
      // 
      this.serverLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.serverLabel.Location = new System.Drawing.Point(3, 0);
      this.serverLabel.Name = "serverLabel";
      this.serverLabel.Size = new System.Drawing.Size(57, 23);
      this.serverLabel.TabIndex = 0;
      this.serverLabel.Text = "Server";
      this.serverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // userTextBox
      // 
      this.tableLayoutPanel2.SetColumnSpan(this.userTextBox, 2);
      this.userTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.userTextBox.Location = new System.Drawing.Point(66, 72);
      this.userTextBox.Name = "userTextBox";
      this.userTextBox.Size = new System.Drawing.Size(323, 20);
      this.userTextBox.TabIndex = 7;
      // 
      // portLabel
      // 
      this.portLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.portLabel.Location = new System.Drawing.Point(3, 23);
      this.portLabel.Name = "portLabel";
      this.portLabel.Size = new System.Drawing.Size(57, 23);
      this.portLabel.TabIndex = 2;
      this.portLabel.Text = "Port";
      this.portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // databaseTextBox
      // 
      this.tableLayoutPanel2.SetColumnSpan(this.databaseTextBox, 2);
      this.databaseTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.databaseTextBox.Location = new System.Drawing.Point(66, 49);
      this.databaseTextBox.Name = "databaseTextBox";
      this.databaseTextBox.Size = new System.Drawing.Size(323, 20);
      this.databaseTextBox.TabIndex = 5;
      // 
      // databaseLabel
      // 
      this.databaseLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.databaseLabel.Location = new System.Drawing.Point(3, 46);
      this.databaseLabel.Name = "databaseLabel";
      this.databaseLabel.Size = new System.Drawing.Size(57, 23);
      this.databaseLabel.TabIndex = 4;
      this.databaseLabel.Text = "Database";
      this.databaseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // portTextBox
      // 
      this.tableLayoutPanel2.SetColumnSpan(this.portTextBox, 2);
      this.portTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.portTextBox.Location = new System.Drawing.Point(66, 26);
      this.portTextBox.Name = "portTextBox";
      this.portTextBox.Size = new System.Drawing.Size(323, 20);
      this.portTextBox.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 92);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(57, 23);
      this.label1.TabIndex = 8;
      this.label1.Text = "Password";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // serverTextBox
      // 
      this.tableLayoutPanel2.SetColumnSpan(this.serverTextBox, 2);
      this.serverTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.serverTextBox.Location = new System.Drawing.Point(66, 3);
      this.serverTextBox.Name = "serverTextBox";
      this.serverTextBox.Size = new System.Drawing.Size(323, 20);
      this.serverTextBox.TabIndex = 1;
      // 
      // userLabel
      // 
      this.userLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.userLabel.Location = new System.Drawing.Point(3, 69);
      this.userLabel.Name = "userLabel";
      this.userLabel.Size = new System.Drawing.Size(57, 23);
      this.userLabel.TabIndex = 6;
      this.userLabel.Text = "User";
      this.userLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // buttonRefresh
      // 
      this.buttonRefresh.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonRefresh.Location = new System.Drawing.Point(277, 162);
      this.buttonRefresh.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.buttonRefresh.Name = "buttonRefresh";
      this.buttonRefresh.Size = new System.Drawing.Size(112, 23);
      this.buttonRefresh.TabIndex = 12;
      this.buttonRefresh.Text = "Refresh migration list";
      this.buttonRefresh.UseVisualStyleBackColor = true;
      this.buttonRefresh.Click += new System.EventHandler(this.ButtonRefreshClick);
      // 
      // connectionStringTextBox
      // 
      this.connectionStringTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.tableLayoutPanel2.SetColumnSpan(this.connectionStringTextBox, 3);
      this.connectionStringTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.connectionStringTextBox.Location = new System.Drawing.Point(3, 118);
      this.connectionStringTextBox.Name = "connectionStringTextBox";
      this.connectionStringTextBox.ReadOnly = true;
      this.connectionStringTextBox.Size = new System.Drawing.Size(386, 41);
      this.connectionStringTextBox.TabIndex = 13;
      this.connectionStringTextBox.Text = "";
      // 
      // migrationsGroupBox
      // 
      this.migrationsGroupBox.Controls.Add(this.tableLayoutPanel1);
      this.migrationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.migrationsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.migrationsGroupBox.Location = new System.Drawing.Point(3, 213);
      this.migrationsGroupBox.Name = "migrationsGroupBox";
      this.migrationsGroupBox.Size = new System.Drawing.Size(398, 296);
      this.migrationsGroupBox.TabIndex = 1;
      this.migrationsGroupBox.TabStop = false;
      this.migrationsGroupBox.Text = "Migrations";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 43F));
      this.tableLayoutPanel1.Controls.Add(this.upgradeButton, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.versionNumericUpDown, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.migrateButton, 2, 2);
      this.tableLayoutPanel1.Controls.Add(this.migrationsTextBox, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(392, 277);
      this.tableLayoutPanel1.TabIndex = 2;
      // 
      // upgradeButton
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.upgradeButton, 3);
      this.upgradeButton.Dock = System.Windows.Forms.DockStyle.Fill;
      this.upgradeButton.Location = new System.Drawing.Point(3, 231);
      this.upgradeButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
      this.upgradeButton.Name = "upgradeButton";
      this.upgradeButton.Size = new System.Drawing.Size(386, 23);
      this.upgradeButton.TabIndex = 1;
      this.upgradeButton.Text = "Upgrade to last migration";
      this.upgradeButton.UseVisualStyleBackColor = true;
      this.upgradeButton.Click += new System.EventHandler(this.UpgradeButtonClick);
      // 
      // versionNumericUpDown
      // 
      this.versionNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.versionNumericUpDown.Location = new System.Drawing.Point(68, 257);
      this.versionNumericUpDown.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
      this.versionNumericUpDown.Name = "versionNumericUpDown";
      this.versionNumericUpDown.Size = new System.Drawing.Size(278, 20);
      this.versionNumericUpDown.TabIndex = 2;
      this.versionNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      // 
      // migrateButton
      // 
      this.migrateButton.Dock = System.Windows.Forms.DockStyle.Fill;
      this.migrateButton.Location = new System.Drawing.Point(352, 257);
      this.migrateButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
      this.migrateButton.Name = "migrateButton";
      this.migrateButton.Size = new System.Drawing.Size(37, 20);
      this.migrateButton.TabIndex = 3;
      this.migrateButton.Text = "Go";
      this.migrateButton.UseVisualStyleBackColor = true;
      this.migrateButton.Click += new System.EventHandler(this.MigrateButtonClick);
      // 
      // migrationsTextBox
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.migrationsTextBox, 3);
      this.migrationsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.migrationsTextBox.Location = new System.Drawing.Point(3, 3);
      this.migrationsTextBox.Multiline = true;
      this.migrationsTextBox.Name = "migrationsTextBox";
      this.migrationsTextBox.ReadOnly = true;
      this.migrationsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.migrationsTextBox.Size = new System.Drawing.Size(386, 225);
      this.migrationsTextBox.TabIndex = 0;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 254);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(59, 23);
      this.label2.TabIndex = 4;
      this.label2.Text = "Migrate to";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 1;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Controls.Add(this.migrationsGroupBox, 0, 1);
      this.baseLayout.Controls.Add(this.connectionGroupBox, 0, 0);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 2;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.Size = new System.Drawing.Size(404, 512);
      this.baseLayout.TabIndex = 2;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(404, 512);
      this.Controls.Add(this.baseLayout);
      this.MinimumSize = new System.Drawing.Size(420, 550);
      this.Name = "MainForm";
      this.Text = "GDB Migrator";
      this.connectionGroupBox.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel2.PerformLayout();
      this.migrationsGroupBox.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.versionNumericUpDown)).EndInit();
      this.baseLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.RichTextBox connectionStringTextBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.NumericUpDown versionNumericUpDown;
    private System.Windows.Forms.Button migrateButton;
    private System.Windows.Forms.Button buttonRefresh;
    private System.Windows.Forms.Button upgradeButton;
    private System.Windows.Forms.TextBox migrationsTextBox;
    private System.Windows.Forms.GroupBox migrationsGroupBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox passwordTextBox;
    private System.Windows.Forms.Label userLabel;
    private System.Windows.Forms.TextBox userTextBox;
    private System.Windows.Forms.Label portLabel;
    private System.Windows.Forms.TextBox portTextBox;
    private System.Windows.Forms.Label databaseLabel;
    private System.Windows.Forms.TextBox databaseTextBox;
    private System.Windows.Forms.Label serverLabel;
    private System.Windows.Forms.TextBox serverTextBox;
    private System.Windows.Forms.GroupBox connectionGroupBox;
  }
}
