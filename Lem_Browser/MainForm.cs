// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lem_Browser
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MainForm).FullName);

    #region Getters / Setters
    // TODO: Generate here with the Alt-Inser command the wanted getters / setters
    // TODO: Do not forget to add the description of the property with ///
    #endregion // Getters / Setters

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
      // TODO: Add constructor code after the InitializeComponent() call.
      //
      //Skybound.Gecko.Xpcom.Initialize(@"D:\Program Files (x86)\Mozilla XULRunner");
    }

    // TODO: Add here with Alt-Inser command additional constructors
    #endregion // Constructors

    #region Methods
    // TODO: Add here your methods
    public void Method ()
    {
    }
    #endregion // Methods
    
    void Button1Click(object sender, EventArgs e)
    {
      webBrowser1.Url = new Uri(textBox1.Text);
      //geckoWebBrowser1.Navigate(textBox1.Text);
    }
  }
}
