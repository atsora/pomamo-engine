// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class NullableColorDialog
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
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.buttonClear = new System.Windows.Forms.Button();
      this.buttonOk = new System.Windows.Forms.Button();
      this.labelHex = new System.Windows.Forms.Label();
      this.subLayout = new System.Windows.Forms.TableLayoutPanel();
      this.marker26 = new Lemoine.BaseControls.Marker();
      this.marker25 = new Lemoine.BaseControls.Marker();
      this.marker24 = new Lemoine.BaseControls.Marker();
      this.marker23 = new Lemoine.BaseControls.Marker();
      this.marker22 = new Lemoine.BaseControls.Marker();
      this.marker21 = new Lemoine.BaseControls.Marker();
      this.marker20 = new Lemoine.BaseControls.Marker();
      this.marker19 = new Lemoine.BaseControls.Marker();
      this.marker18 = new Lemoine.BaseControls.Marker();
      this.marker17 = new Lemoine.BaseControls.Marker();
      this.marker16 = new Lemoine.BaseControls.Marker();
      this.marker15 = new Lemoine.BaseControls.Marker();
      this.marker14 = new Lemoine.BaseControls.Marker();
      this.marker13 = new Lemoine.BaseControls.Marker();
      this.marker12 = new Lemoine.BaseControls.Marker();
      this.marker11 = new Lemoine.BaseControls.Marker();
      this.marker10 = new Lemoine.BaseControls.Marker();
      this.marker9 = new Lemoine.BaseControls.Marker();
      this.marker8 = new Lemoine.BaseControls.Marker();
      this.marker7 = new Lemoine.BaseControls.Marker();
      this.marker6 = new Lemoine.BaseControls.Marker();
      this.marker5 = new Lemoine.BaseControls.Marker();
      this.marker4 = new Lemoine.BaseControls.Marker();
      this.marker1 = new Lemoine.BaseControls.Marker();
      this.marker2 = new Lemoine.BaseControls.Marker();
      this.marker3 = new Lemoine.BaseControls.Marker();
      this.panelHue = new System.Windows.Forms.Panel();
      this.panelSL = new System.Windows.Forms.Panel();
      this.border1 = new System.Windows.Forms.Label();
      this.border2 = new System.Windows.Forms.Label();
      this.baseLayout.SuspendLayout();
      this.subLayout.SuspendLayout();
      this.SuspendLayout();
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 4;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.baseLayout.Controls.Add(this.buttonCancel, 1, 5);
      this.baseLayout.Controls.Add(this.buttonClear, 2, 5);
      this.baseLayout.Controls.Add(this.buttonOk, 3, 5);
      this.baseLayout.Controls.Add(this.labelHex, 0, 5);
      this.baseLayout.Controls.Add(this.subLayout, 0, 4);
      this.baseLayout.Controls.Add(this.panelHue, 0, 2);
      this.baseLayout.Controls.Add(this.panelSL, 0, 0);
      this.baseLayout.Controls.Add(this.border1, 0, 1);
      this.baseLayout.Controls.Add(this.border2, 0, 3);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Margin = new System.Windows.Forms.Padding(0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.RowCount = 6;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
      this.baseLayout.Size = new System.Drawing.Size(345, 270);
      this.baseLayout.TabIndex = 0;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCancel.Location = new System.Drawing.Point(165, 241);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(57, 26);
      this.buttonCancel.TabIndex = 0;
      this.buttonCancel.Text = "Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
      // 
      // buttonClear
      // 
      this.buttonClear.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonClear.Location = new System.Drawing.Point(225, 241);
      this.buttonClear.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.buttonClear.Name = "buttonClear";
      this.buttonClear.Size = new System.Drawing.Size(57, 26);
      this.buttonClear.TabIndex = 1;
      this.buttonClear.Text = "Clear";
      this.buttonClear.UseVisualStyleBackColor = true;
      this.buttonClear.Click += new System.EventHandler(this.ButtonClearClick);
      // 
      // buttonOk
      // 
      this.buttonOk.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonOk.Location = new System.Drawing.Point(285, 241);
      this.buttonOk.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
      this.buttonOk.Name = "buttonOk";
      this.buttonOk.Size = new System.Drawing.Size(57, 26);
      this.buttonOk.TabIndex = 2;
      this.buttonOk.Text = "Ok";
      this.buttonOk.UseVisualStyleBackColor = true;
      this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
      // 
      // labelHex
      // 
      this.labelHex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.labelHex.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelHex.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.labelHex.Location = new System.Drawing.Point(3, 242);
      this.labelHex.Margin = new System.Windows.Forms.Padding(3, 1, 3, 4);
      this.labelHex.Name = "labelHex";
      this.labelHex.Size = new System.Drawing.Size(159, 24);
      this.labelHex.TabIndex = 4;
      this.labelHex.Text = "#FF0022";
      this.labelHex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // subLayout
      // 
      this.subLayout.ColumnCount = 25;
      this.baseLayout.SetColumnSpan(this.subLayout, 4);
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.333611F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 8.330276F));
      this.subLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.Controls.Add(this.marker26, 24, 2);
      this.subLayout.Controls.Add(this.marker25, 22, 2);
      this.subLayout.Controls.Add(this.marker24, 20, 2);
      this.subLayout.Controls.Add(this.marker23, 18, 2);
      this.subLayout.Controls.Add(this.marker22, 16, 2);
      this.subLayout.Controls.Add(this.marker21, 14, 2);
      this.subLayout.Controls.Add(this.marker20, 12, 2);
      this.subLayout.Controls.Add(this.marker19, 10, 2);
      this.subLayout.Controls.Add(this.marker18, 8, 2);
      this.subLayout.Controls.Add(this.marker17, 6, 2);
      this.subLayout.Controls.Add(this.marker16, 4, 2);
      this.subLayout.Controls.Add(this.marker15, 2, 2);
      this.subLayout.Controls.Add(this.marker14, 0, 2);
      this.subLayout.Controls.Add(this.marker13, 24, 0);
      this.subLayout.Controls.Add(this.marker12, 22, 0);
      this.subLayout.Controls.Add(this.marker11, 20, 0);
      this.subLayout.Controls.Add(this.marker10, 18, 0);
      this.subLayout.Controls.Add(this.marker9, 16, 0);
      this.subLayout.Controls.Add(this.marker8, 14, 0);
      this.subLayout.Controls.Add(this.marker7, 12, 0);
      this.subLayout.Controls.Add(this.marker6, 10, 0);
      this.subLayout.Controls.Add(this.marker5, 8, 0);
      this.subLayout.Controls.Add(this.marker4, 6, 0);
      this.subLayout.Controls.Add(this.marker1, 0, 0);
      this.subLayout.Controls.Add(this.marker2, 2, 0);
      this.subLayout.Controls.Add(this.marker3, 4, 0);
      this.subLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.subLayout.Location = new System.Drawing.Point(3, 189);
      this.subLayout.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
      this.subLayout.Name = "subLayout";
      this.subLayout.RowCount = 3;
      this.subLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.subLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.subLayout.Size = new System.Drawing.Size(342, 52);
      this.subLayout.TabIndex = 5;
      // 
      // marker26
      // 
      this.marker26.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker26.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker26.Location = new System.Drawing.Point(312, 27);
      this.marker26.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.marker26.Name = "marker26";
      this.marker26.Size = new System.Drawing.Size(27, 25);
      this.marker26.TabIndex = 29;
      this.marker26.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker25
      // 
      this.marker25.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker25.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker25.Location = new System.Drawing.Point(286, 27);
      this.marker25.Margin = new System.Windows.Forms.Padding(0);
      this.marker25.Name = "marker25";
      this.marker25.Size = new System.Drawing.Size(25, 25);
      this.marker25.TabIndex = 28;
      this.marker25.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker24
      // 
      this.marker24.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker24.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker24.Location = new System.Drawing.Point(260, 27);
      this.marker24.Margin = new System.Windows.Forms.Padding(0);
      this.marker24.Name = "marker24";
      this.marker24.Size = new System.Drawing.Size(25, 25);
      this.marker24.TabIndex = 27;
      this.marker24.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker23
      // 
      this.marker23.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker23.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker23.Location = new System.Drawing.Point(234, 27);
      this.marker23.Margin = new System.Windows.Forms.Padding(0);
      this.marker23.Name = "marker23";
      this.marker23.Size = new System.Drawing.Size(25, 25);
      this.marker23.TabIndex = 26;
      this.marker23.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker22
      // 
      this.marker22.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker22.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker22.Location = new System.Drawing.Point(208, 27);
      this.marker22.Margin = new System.Windows.Forms.Padding(0);
      this.marker22.Name = "marker22";
      this.marker22.Size = new System.Drawing.Size(25, 25);
      this.marker22.TabIndex = 25;
      this.marker22.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker21
      // 
      this.marker21.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker21.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker21.Location = new System.Drawing.Point(182, 27);
      this.marker21.Margin = new System.Windows.Forms.Padding(0);
      this.marker21.Name = "marker21";
      this.marker21.Size = new System.Drawing.Size(25, 25);
      this.marker21.TabIndex = 24;
      this.marker21.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker20
      // 
      this.marker20.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker20.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker20.Location = new System.Drawing.Point(156, 27);
      this.marker20.Margin = new System.Windows.Forms.Padding(0);
      this.marker20.Name = "marker20";
      this.marker20.Size = new System.Drawing.Size(25, 25);
      this.marker20.TabIndex = 23;
      this.marker20.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker19
      // 
      this.marker19.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker19.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker19.Location = new System.Drawing.Point(130, 27);
      this.marker19.Margin = new System.Windows.Forms.Padding(0);
      this.marker19.Name = "marker19";
      this.marker19.Size = new System.Drawing.Size(25, 25);
      this.marker19.TabIndex = 22;
      this.marker19.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker18
      // 
      this.marker18.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker18.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker18.Location = new System.Drawing.Point(104, 27);
      this.marker18.Margin = new System.Windows.Forms.Padding(0);
      this.marker18.Name = "marker18";
      this.marker18.Size = new System.Drawing.Size(25, 25);
      this.marker18.TabIndex = 21;
      this.marker18.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker17
      // 
      this.marker17.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker17.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker17.Location = new System.Drawing.Point(78, 27);
      this.marker17.Margin = new System.Windows.Forms.Padding(0);
      this.marker17.Name = "marker17";
      this.marker17.Size = new System.Drawing.Size(25, 25);
      this.marker17.TabIndex = 20;
      this.marker17.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker16
      // 
      this.marker16.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker16.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker16.Location = new System.Drawing.Point(52, 27);
      this.marker16.Margin = new System.Windows.Forms.Padding(0);
      this.marker16.Name = "marker16";
      this.marker16.Size = new System.Drawing.Size(25, 25);
      this.marker16.TabIndex = 19;
      this.marker16.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker15
      // 
      this.marker15.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker15.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker15.Location = new System.Drawing.Point(26, 27);
      this.marker15.Margin = new System.Windows.Forms.Padding(0);
      this.marker15.Name = "marker15";
      this.marker15.Size = new System.Drawing.Size(25, 25);
      this.marker15.TabIndex = 18;
      this.marker15.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker14
      // 
      this.marker14.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker14.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker14.Location = new System.Drawing.Point(0, 27);
      this.marker14.Margin = new System.Windows.Forms.Padding(0);
      this.marker14.Name = "marker14";
      this.marker14.Size = new System.Drawing.Size(25, 25);
      this.marker14.TabIndex = 17;
      this.marker14.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker13
      // 
      this.marker13.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker13.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker13.Location = new System.Drawing.Point(312, 0);
      this.marker13.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
      this.marker13.Name = "marker13";
      this.marker13.Size = new System.Drawing.Size(27, 25);
      this.marker13.TabIndex = 16;
      this.marker13.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker12
      // 
      this.marker12.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker12.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker12.Location = new System.Drawing.Point(286, 0);
      this.marker12.Margin = new System.Windows.Forms.Padding(0);
      this.marker12.Name = "marker12";
      this.marker12.Size = new System.Drawing.Size(25, 25);
      this.marker12.TabIndex = 15;
      this.marker12.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker11
      // 
      this.marker11.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker11.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker11.Location = new System.Drawing.Point(260, 0);
      this.marker11.Margin = new System.Windows.Forms.Padding(0);
      this.marker11.Name = "marker11";
      this.marker11.Size = new System.Drawing.Size(25, 25);
      this.marker11.TabIndex = 14;
      this.marker11.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker10
      // 
      this.marker10.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker10.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker10.Location = new System.Drawing.Point(234, 0);
      this.marker10.Margin = new System.Windows.Forms.Padding(0);
      this.marker10.Name = "marker10";
      this.marker10.Size = new System.Drawing.Size(25, 25);
      this.marker10.TabIndex = 13;
      this.marker10.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker9
      // 
      this.marker9.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker9.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker9.Location = new System.Drawing.Point(208, 0);
      this.marker9.Margin = new System.Windows.Forms.Padding(0);
      this.marker9.Name = "marker9";
      this.marker9.Size = new System.Drawing.Size(25, 25);
      this.marker9.TabIndex = 12;
      this.marker9.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker8
      // 
      this.marker8.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker8.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker8.Location = new System.Drawing.Point(182, 0);
      this.marker8.Margin = new System.Windows.Forms.Padding(0);
      this.marker8.Name = "marker8";
      this.marker8.Size = new System.Drawing.Size(25, 25);
      this.marker8.TabIndex = 11;
      this.marker8.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker7
      // 
      this.marker7.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker7.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker7.Location = new System.Drawing.Point(156, 0);
      this.marker7.Margin = new System.Windows.Forms.Padding(0);
      this.marker7.Name = "marker7";
      this.marker7.Size = new System.Drawing.Size(25, 25);
      this.marker7.TabIndex = 10;
      this.marker7.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker6
      // 
      this.marker6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker6.Location = new System.Drawing.Point(130, 0);
      this.marker6.Margin = new System.Windows.Forms.Padding(0);
      this.marker6.Name = "marker6";
      this.marker6.Size = new System.Drawing.Size(25, 25);
      this.marker6.TabIndex = 9;
      this.marker6.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker5
      // 
      this.marker5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker5.Location = new System.Drawing.Point(104, 0);
      this.marker5.Margin = new System.Windows.Forms.Padding(0);
      this.marker5.Name = "marker5";
      this.marker5.Size = new System.Drawing.Size(25, 25);
      this.marker5.TabIndex = 8;
      this.marker5.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker4
      // 
      this.marker4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker4.Location = new System.Drawing.Point(78, 0);
      this.marker4.Margin = new System.Windows.Forms.Padding(0);
      this.marker4.Name = "marker4";
      this.marker4.Size = new System.Drawing.Size(25, 25);
      this.marker4.TabIndex = 7;
      this.marker4.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker1
      // 
      this.marker1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker1.Location = new System.Drawing.Point(0, 0);
      this.marker1.Margin = new System.Windows.Forms.Padding(0);
      this.marker1.Name = "marker1";
      this.marker1.Size = new System.Drawing.Size(25, 25);
      this.marker1.TabIndex = 0;
      this.marker1.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker2
      // 
      this.marker2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker2.Location = new System.Drawing.Point(26, 0);
      this.marker2.Margin = new System.Windows.Forms.Padding(0);
      this.marker2.Name = "marker2";
      this.marker2.Size = new System.Drawing.Size(25, 25);
      this.marker2.TabIndex = 1;
      this.marker2.Click += new System.EventHandler(this.MarkerClick);
      // 
      // marker3
      // 
      this.marker3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.marker3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.marker3.Location = new System.Drawing.Point(52, 0);
      this.marker3.Margin = new System.Windows.Forms.Padding(0);
      this.marker3.Name = "marker3";
      this.marker3.Size = new System.Drawing.Size(25, 25);
      this.marker3.TabIndex = 6;
      this.marker3.Click += new System.EventHandler(this.MarkerClick);
      // 
      // panelHue
      // 
      this.baseLayout.SetColumnSpan(this.panelHue, 4);
      this.panelHue.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelHue.Location = new System.Drawing.Point(0, 162);
      this.panelHue.Margin = new System.Windows.Forms.Padding(0);
      this.panelHue.Name = "panelHue";
      this.panelHue.Size = new System.Drawing.Size(345, 23);
      this.panelHue.TabIndex = 6;
      this.panelHue.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelHueMouseDown);
      this.panelHue.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelHueMouseMove);
      // 
      // panelSL
      // 
      this.baseLayout.SetColumnSpan(this.panelSL, 4);
      this.panelSL.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panelSL.Location = new System.Drawing.Point(0, 0);
      this.panelSL.Margin = new System.Windows.Forms.Padding(0);
      this.panelSL.Name = "panelSL";
      this.panelSL.Size = new System.Drawing.Size(345, 161);
      this.panelSL.TabIndex = 7;
      this.panelSL.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelSLPaint);
      this.panelSL.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelSVMouseDown);
      this.panelSL.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelSVMouseMove);
      // 
      // border1
      // 
      this.border1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.border1, 4);
      this.border1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.border1.Location = new System.Drawing.Point(0, 161);
      this.border1.Margin = new System.Windows.Forms.Padding(0);
      this.border1.Name = "border1";
      this.border1.Size = new System.Drawing.Size(345, 1);
      this.border1.TabIndex = 8;
      // 
      // border2
      // 
      this.border2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.baseLayout.SetColumnSpan(this.border2, 4);
      this.border2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.border2.Location = new System.Drawing.Point(0, 185);
      this.border2.Margin = new System.Windows.Forms.Padding(0);
      this.border2.Name = "border2";
      this.border2.Size = new System.Drawing.Size(345, 1);
      this.border2.TabIndex = 9;
      // 
      // NullableColorDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(345, 270);
      this.Controls.Add(this.baseLayout);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(361, 309);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(361, 309);
      this.Name = "NullableColorDialog";
      this.ShowInTaskbar = false;
      this.Text = "Color picker";
      this.TopMost = true;
      this.Shown += new System.EventHandler(this.NullableColorDialogShown);
      this.baseLayout.ResumeLayout(false);
      this.subLayout.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonClear;
    private System.Windows.Forms.Button buttonOk;
    private System.Windows.Forms.TableLayoutPanel subLayout;
    private System.Windows.Forms.Label labelHex;
    private Lemoine.BaseControls.Marker marker26;
    private Lemoine.BaseControls.Marker marker25;
    private Lemoine.BaseControls.Marker marker24;
    private Lemoine.BaseControls.Marker marker23;
    private Lemoine.BaseControls.Marker marker22;
    private Lemoine.BaseControls.Marker marker21;
    private Lemoine.BaseControls.Marker marker20;
    private Lemoine.BaseControls.Marker marker19;
    private Lemoine.BaseControls.Marker marker18;
    private Lemoine.BaseControls.Marker marker17;
    private Lemoine.BaseControls.Marker marker16;
    private Lemoine.BaseControls.Marker marker15;
    private Lemoine.BaseControls.Marker marker14;
    private Lemoine.BaseControls.Marker marker13;
    private Lemoine.BaseControls.Marker marker12;
    private Lemoine.BaseControls.Marker marker11;
    private Lemoine.BaseControls.Marker marker10;
    private Lemoine.BaseControls.Marker marker9;
    private Lemoine.BaseControls.Marker marker8;
    private Lemoine.BaseControls.Marker marker7;
    private Lemoine.BaseControls.Marker marker6;
    private Lemoine.BaseControls.Marker marker5;
    private Lemoine.BaseControls.Marker marker4;
    private Lemoine.BaseControls.Marker marker1;
    private Lemoine.BaseControls.Marker marker2;
    private Lemoine.BaseControls.Marker marker3;
    private System.Windows.Forms.Panel panelHue;
    private System.Windows.Forms.Panel panelSL;
    private System.Windows.Forms.Label border1;
    private System.Windows.Forms.Label border2;
  }
}
