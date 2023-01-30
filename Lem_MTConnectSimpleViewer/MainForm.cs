// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;

namespace Lem_MTConnectSimpleViewer
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    static readonly string XSL_FILE = "current.xsl";
    static readonly string TABLE_XSL_FILE = "table.xsl";
    static readonly string BASIC_XSL_FILE = "basic.xsl";
    
    #region Members
    string url = "";
    XmlWriterSettings xmlSettings = new XmlWriterSettings ();
    XslCompiledTransform xslt = new XslCompiledTransform ();
    #endregion

    #region Getters / Setters
    #endregion

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      xmlSettings.Indent = true;
      string xslFile = XSL_FILE;
      if (!File.Exists (xslFile)) {
        xslFile = TABLE_XSL_FILE;
      }
      if (!File.Exists (xslFile)) {
        xslFile = BASIC_XSL_FILE;
      }
      try {
        xslt.Load (xslFile);
      }
      catch (Exception) {
        MessageBox.Show ("The stylesheet can't be found",
                         "Stylesheet loading error",
                         MessageBoxButtons.OK,
                         MessageBoxIcon.Error);
        Environment.Exit (1);
      }
      refreshTimer.Interval = (int) (1000 * refreshNumericUpDown.Value);
      try {
        urlTextBox.Text = File.ReadAllText ("Lem_MTConnectSimpleViewer.lastAddress");
        UpdateUrl ();
      }
      catch (Exception) {
      }
    }
    #endregion

    void UpdateButtonClick(object sender, EventArgs e)
    {
      UpdateUrl ();
    }
    
    void UpdateUrl ()
    {
      url = urlTextBox.Text + "/current";
      try {
        File.WriteAllText ("Lem_MTConnectSimpleViewer.lastAddress",
                           urlTextBox.Text);
      }
      catch (Exception) {
      }
      UpdateData ();
    }
    
    void UpdateData ()
    {
      StringBuilder sb = new StringBuilder ();
      using (XmlWriter writer = XmlWriter.Create (sb, xmlSettings))
      {
        try {
          xslt.Transform (url, writer);
          webBrowser.DocumentText = sb.ToString ();
        }
        catch (Exception) {
          // Display an error
          webBrowser.DocumentText = "";
        }
      }
    }
    
    void RefreshNumericUpDownValueChanged(object sender, EventArgs e)
    {
      refreshTimer.Interval = (int) refreshNumericUpDown.Value * 1000;
    }
    
    void RefreshTimerTick(object sender, EventArgs e)
    {
      UpdateData ();
    }
    
    void WebBrowserNavigating(object sender, WebBrowserNavigatingEventArgs e)
    {
      if (!e.Url.ToString ().StartsWith ("about")) {
        e.Cancel = true;
        System.Diagnostics.Process.Start (e.Url.ToString ());
      }
    }
    
    void UrlTextBoxKeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter) {
        UpdateButtonClick (sender, e);
      }
    }
  }
}
