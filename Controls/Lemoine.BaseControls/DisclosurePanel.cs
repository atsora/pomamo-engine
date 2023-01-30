// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of DisclosurePanel.
  /// </summary>
  public partial class DisclosurePanel : UserControl
  {
    #region Members
    bool m_open;
    String m_title;
    Control m_content;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DisclosurePanel).FullName);

    #region Getters / Setters
    /// <summary>
    ///   State of DisclosurePanel
    /// </summary>
    public bool State {
      get {
        return m_open;
      }
      set {
        m_open = value;
        if (m_open) {
          Open();
        }
        else
        {
          Close();
        }
      }
    }
    
    /// <summary>
    ///   Title of DisclosurePanel
    /// </summary>
    public String Title {
      get {
        return m_title;
      }
      set {
        m_title = value;
        TitleLbl.Text = m_title;
      }
    }
    /// <summary>
    ///   Control contained in DisclosurePanel
    /// </summary>
    public Control Content {
      get {
        return m_content;
      }
      set {
        m_content = value;
        contentPanel.Controls.Clear();
        if (value != null) {
          contentPanel.Controls.Add(value);
        }
        else {
          contentPanel.Height = 0;
        }
      }
    }
    /// <summary>
    ///  Height of Content
    /// </summary>
    public int ContentHeight {
      get{
        return contentPanel.Height;
      }
      set {
        contentPanel.Height = value;
      }
    }
    
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DisclosurePanel()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
    }

    #endregion // Constructors

    #region Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="control"></param>
    public void AddComponent(Control control)
    {
      contentPanel.Controls.Add(control);
    }
    
    void DisclosurePanelLoad(object sender, EventArgs e)
    {
      pictureBox.Image = imageList.Images[0];
      State = false;
    }
    
    /// <summary>
    ///   Show content of DisclosurePanel
    /// </summary>
    public void Open()
    {
      this.Height = headerPanel.Height+contentPanel.Height;
    }
    
    /// <summary>
    ///   Hide content of DisclosurePanel
    /// </summary>
    public void Close()
    {
      this.Height = headerPanel.Height;
    }
    
    
    void PictureBoxMouseClick(object sender, MouseEventArgs e)
    {
      if (State)
      {
        pictureBox.Image = imageList.Images[0];
        State = false;
      }
      else
      {
        pictureBox.Image = imageList.Images[1];
        State = true;
      }
    }
    

    void ContentPanelControlAdded(object sender, ControlEventArgs e)
    {
      e.Control.Dock = DockStyle.Top;
    }
    #endregion // Methods
    
    
  }
}
