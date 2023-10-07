// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.BaseControls
{
  partial class SplashScreen
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreen));
      this.logoPictureBox = new System.Windows.Forms.PictureBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.baseLayout = new System.Windows.Forms.TableLayoutPanel();
      this.buttonGo = new System.Windows.Forms.Button();
      this.tableIdentification = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.textLogin = new System.Windows.Forms.TextBox();
      this.checkRememberMe = new System.Windows.Forms.CheckBox();
      this.textPassword = new System.Windows.Forms.MaskedTextBox();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.labelMiddle = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
      this.panel1.SuspendLayout();
      this.baseLayout.SuspendLayout();
      this.tableIdentification.SuspendLayout();
      this.SuspendLayout();
      // 
      // logoPictureBox
      // 
      this.baseLayout.SetColumnSpan(this.logoPictureBox, 3);
      this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.logoPictureBox.Location = new System.Drawing.Point(8, 8);
      this.logoPictureBox.Name = "logoPictureBox";
      this.logoPictureBox.Size = new System.Drawing.Size(272, 60);
      this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.logoPictureBox.TabIndex = 0;
      this.logoPictureBox.TabStop = false;
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.SystemColors.Control;
      this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panel1.Controls.Add(this.baseLayout);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(290, 300);
      this.panel1.TabIndex = 1;
      // 
      // baseLayout
      // 
      this.baseLayout.ColumnCount = 3;
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.baseLayout.Controls.Add(this.buttonGo, 2, 3);
      this.baseLayout.Controls.Add(this.tableIdentification, 0, 2);
      this.baseLayout.Controls.Add(this.buttonCancel, 0, 3);
      this.baseLayout.Controls.Add(this.logoPictureBox, 0, 0);
      this.baseLayout.Controls.Add(this.labelMiddle, 0, 1);
      this.baseLayout.Dock = System.Windows.Forms.DockStyle.Fill;
      this.baseLayout.Location = new System.Drawing.Point(0, 0);
      this.baseLayout.Name = "baseLayout";
      this.baseLayout.Padding = new System.Windows.Forms.Padding(5);
      this.baseLayout.RowCount = 4;
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 66F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 86F));
      this.baseLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
      this.baseLayout.Size = new System.Drawing.Size(288, 298);
      this.baseLayout.TabIndex = 3;
      // 
      // buttonGo
      // 
      this.buttonGo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonGo.Image = ((System.Drawing.Image)(resources.GetObject("buttonGo.Image")));
      this.buttonGo.Location = new System.Drawing.Point(233, 252);
      this.buttonGo.Margin = new System.Windows.Forms.Padding(0);
      this.buttonGo.Name = "buttonGo";
      this.buttonGo.Size = new System.Drawing.Size(50, 41);
      this.buttonGo.TabIndex = 4;
      this.buttonGo.UseVisualStyleBackColor = true;
      this.buttonGo.Click += new System.EventHandler(this.ButtonGoClick);
      // 
      // tableIdentification
      // 
      this.tableIdentification.ColumnCount = 2;
      this.baseLayout.SetColumnSpan(this.tableIdentification, 3);
      this.tableIdentification.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.tableIdentification.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableIdentification.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableIdentification.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableIdentification.Controls.Add(this.label1, 0, 0);
      this.tableIdentification.Controls.Add(this.label2, 0, 1);
      this.tableIdentification.Controls.Add(this.textLogin, 1, 0);
      this.tableIdentification.Controls.Add(this.checkRememberMe, 1, 2);
      this.tableIdentification.Controls.Add(this.textPassword, 1, 1);
      this.tableIdentification.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableIdentification.Location = new System.Drawing.Point(8, 169);
      this.tableIdentification.Name = "tableIdentification";
      this.tableIdentification.RowCount = 4;
      this.tableIdentification.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableIdentification.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableIdentification.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableIdentification.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableIdentification.Size = new System.Drawing.Size(272, 80);
      this.tableIdentification.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(54, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "Login";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(3, 25);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(54, 25);
      this.label2.TabIndex = 1;
      this.label2.Text = "Password";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // textLogin
      // 
      this.textLogin.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textLogin.Location = new System.Drawing.Point(63, 3);
      this.textLogin.Name = "textLogin";
      this.textLogin.Size = new System.Drawing.Size(206, 20);
      this.textLogin.TabIndex = 1;
      this.textLogin.TextChanged += new System.EventHandler(this.textLogin_TextChanged);
      this.textLogin.Enter += new System.EventHandler(this.TextLoginEnter);
      this.textLogin.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextLoginKeyUp);
      this.textLogin.Leave += new System.EventHandler(this.TextLoginLeave);
      this.textLogin.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TextLoginMouseUp);
      // 
      // checkRememberMe
      // 
      this.checkRememberMe.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkRememberMe.Location = new System.Drawing.Point(63, 53);
      this.checkRememberMe.Name = "checkRememberMe";
      this.checkRememberMe.Size = new System.Drawing.Size(206, 19);
      this.checkRememberMe.TabIndex = 3;
      this.checkRememberMe.Text = "Remember me";
      this.checkRememberMe.UseVisualStyleBackColor = true;
      // 
      // textPassword
      // 
      this.textPassword.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textPassword.Location = new System.Drawing.Point(63, 28);
      this.textPassword.Name = "textPassword";
      this.textPassword.PasswordChar = '•';
      this.textPassword.Size = new System.Drawing.Size(206, 20);
      this.textPassword.TabIndex = 2;
      this.textPassword.Enter += new System.EventHandler(this.TextPasswordEnter);
      this.textPassword.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TextPasswordKeyUp);
      this.textPassword.Leave += new System.EventHandler(this.TextPasswordLeave);
      this.textPassword.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TextPasswordMouseUp);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.buttonCancel.Image = ((System.Drawing.Image)(resources.GetObject("buttonCancel.Image")));
      this.buttonCancel.Location = new System.Drawing.Point(5, 252);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(50, 41);
      this.buttonCancel.TabIndex = 5;
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
      // 
      // labelMiddle
      // 
      this.baseLayout.SetColumnSpan(this.labelMiddle, 3);
      this.labelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelMiddle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
      this.labelMiddle.Location = new System.Drawing.Point(8, 71);
      this.labelMiddle.Name = "labelMiddle";
      this.labelMiddle.Size = new System.Drawing.Size(272, 95);
      this.labelMiddle.TabIndex = 99;
      this.labelMiddle.Text = "Initializing...";
      this.labelMiddle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // SplashScreen
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(290, 300);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "SplashScreen";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Lemoine Settings";
      this.Load += new System.EventHandler(this.SplashScreen_Load);
      ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
      this.panel1.ResumeLayout(false);
      this.baseLayout.ResumeLayout(false);
      this.tableIdentification.ResumeLayout(false);
      this.tableIdentification.PerformLayout();
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.TableLayoutPanel baseLayout;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.CheckBox checkRememberMe;
    private System.Windows.Forms.Label labelMiddle;
    private System.Windows.Forms.Button buttonGo;
    private System.Windows.Forms.MaskedTextBox textPassword;
    private System.Windows.Forms.TextBox textLogin;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TableLayoutPanel tableIdentification;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.PictureBox logoPictureBox;
  }
}
