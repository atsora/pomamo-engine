// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of ModuleStampingGrid.
  /// </summary>
  public partial class ModuleStampingGrid : UserControl
  {
    #region Members
    bool m_readOnlyState = false;

    RichTextBox m_textDescription = new RichTextBox ();
    TextBox m_textCycleStart = new TextBox ();
    TextBox m_textCycleEnd = new TextBox ();
    TextBox m_textSequence = new TextBox ();
    TextBox m_textDetection = new TextBox ();

    Label m_labelDescription = new Label ();
    Label m_labelCycleStart = new Label ();
    Label m_labelCycleEnd = new Label ();
    Label m_labelSequence = new Label ();
    Label m_labelDetection = new Label ();

    TextBox m_oldCycleStart = new TextBox ();
    TextBox m_oldCycleEnd = new TextBox ();
    TextBox m_oldSequence = new TextBox ();
    TextBox m_oldDetection = new TextBox ();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Readonly state of the controls
    /// </summary>
    [DefaultValue (false)]
    [Description ("Readonly state of the controls.")]
    public bool ReadOnly
    {
      get { return m_readOnlyState; }
      set {
        m_readOnlyState = value;
        CycleStart = m_textCycleStart.Text;
        CycleEnd = m_textCycleEnd.Text;
        Sequence = m_textSequence.Text;
        Detection = m_textDetection.Text;
      }
    }

    /// <summary>
    /// Cycle start
    /// May be null if disabled
    /// </summary>
    public string CycleStart
    {
      get { return m_textCycleStart.Enabled ? m_textCycleStart.Text : null; }
      set {
        if (value == null) {
          m_textCycleStart.Enabled = false;
          m_textCycleStart.Text = "";
        }
        else {
          m_textCycleStart.Enabled = !m_readOnlyState;
          m_textCycleStart.Text = value;
        }
      }
    }

    /// <summary>
    /// Old sequence
    /// </summary>
    public string OldCycleStart
    {
      get { return m_oldCycleStart.Text; }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_oldCycleStart.Hide ();
          m_oldCycleStart.Text = "";
        }
        else {
          m_oldCycleStart.Text = "Old: " + value;
          m_oldCycleStart.Show ();
        }
      }
    }

    /// <summary>
    /// Cycle end
    /// May be null if disabled
    /// </summary>
    public string CycleEnd
    {
      get { return m_textCycleEnd.Enabled ? m_textCycleEnd.Text : null; }
      set {
        if (value == null) {
          m_textCycleEnd.Enabled = false;
          m_textCycleEnd.Text = "";
        }
        else {
          m_textCycleEnd.Enabled = !m_readOnlyState;
          m_textCycleEnd.Text = value;
        }
      }
    }

    /// <summary>
    /// Old cycle end
    /// </summary>
    public string OldCycleEnd
    {
      get { return m_oldCycleEnd.Text; }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_oldCycleEnd.Hide ();
          m_oldCycleEnd.Text = "";
        }
        else {
          m_oldCycleEnd.Text = "Old: " + value;
          m_oldCycleEnd.Show ();
        }
      }
    }

    /// <summary>
    /// Sequence
    /// May be null if disabled
    /// </summary>
    public string Sequence
    {
      get { return m_textSequence.Enabled ? m_textSequence.Text : null; }
      set {
        if (value == null) {
          m_textSequence.Enabled = false;
          m_textSequence.Text = "";
        }
        else {
          m_textSequence.Enabled = !m_readOnlyState;
          m_textSequence.Text = value;
        }
      }
    }

    /// <summary>
    /// Old sequence
    /// </summary>
    public string OldSequence
    {
      get { return m_oldSequence.Text; }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_oldSequence.Hide ();
          m_oldSequence.Text = "";
        }
        else {
          m_oldSequence.Text = "Old: " + value;
          m_oldSequence.Show ();
        }
      }
    }

    /// <summary>
    /// Detection
    /// May be null if disabled
    /// </summary>
    public string Detection
    {
      get { return m_textDetection.Enabled ? m_textDetection.Text : null; }
      set {
        if (value == null) {
          m_textDetection.Enabled = false;
          m_textDetection.Text = "";
        }
        else {
          m_textDetection.Enabled = !m_readOnlyState;
          m_textDetection.Text = value;
        }
      }
    }

    /// <summary>
    /// Old detection
    /// </summary>
    public string OldDetection
    {
      get { return m_oldDetection.Text; }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_oldDetection.Hide ();
          m_oldDetection.Text = "";
        }
        else {
          m_oldDetection.Text = "Old: " + value;
          m_oldDetection.Show ();
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ModuleStampingGrid (string description)
    {
      InitializeComponent ();


      m_labelDescription.Text = "Description";
      m_labelCycleStart.Text = "Cycle start";
      m_labelCycleEnd.Text = "Cycle end";
      m_labelSequence.Text = "Sequence";
      m_labelDetection.Text = "Detection";

      m_labelDescription.Dock = m_labelCycleStart.Dock = m_labelCycleEnd.Dock = m_labelSequence.Dock =
        m_labelDetection.Dock = DockStyle.Fill;

      m_oldCycleStart.Dock = m_oldCycleEnd.Dock = m_oldSequence.Dock =
        m_oldDetection.Dock = DockStyle.Fill;

      m_oldCycleStart.Font = m_oldCycleEnd.Font = m_oldSequence.Font =
        m_oldDetection.Font = new Font (m_oldCycleStart.Font, FontStyle.Italic);

      m_oldCycleStart.ForeColor = m_oldCycleEnd.ForeColor = m_oldSequence.ForeColor =
        m_oldDetection.ForeColor = SystemColors.ControlDarkDark;

      m_oldCycleStart.Dock = m_oldCycleEnd.Dock = m_oldSequence.Dock =
        m_oldDetection.Dock = DockStyle.Fill;

      m_oldCycleStart.Visible = m_oldCycleEnd.Visible = m_oldSequence.Visible =
        m_oldDetection.Visible = false;

      m_labelDescription.Padding = new Padding (0, 3, 3, 0);
      m_labelDescription.TextAlign = ContentAlignment.TopLeft;
      m_labelCycleStart.TextAlign = ContentAlignment.MiddleLeft;
      m_labelCycleEnd.TextAlign = ContentAlignment.MiddleLeft;
      m_labelSequence.TextAlign = ContentAlignment.MiddleLeft;
      m_labelDetection.TextAlign = ContentAlignment.MiddleLeft;

      m_oldCycleStart.BorderStyle = m_oldCycleEnd.BorderStyle =
        m_oldSequence.BorderStyle = m_oldDetection.BorderStyle = BorderStyle.None;
      AbstractParameter.EnableControl (m_oldCycleStart, false);
      AbstractParameter.EnableControl (m_oldCycleEnd, false);
      AbstractParameter.EnableControl (m_oldSequence, false);
      AbstractParameter.EnableControl (m_oldDetection, false);

      m_textDescription.Height = 40;
      m_textDescription.ReadOnly = true;
      m_textDescription.BorderStyle = BorderStyle.None;
      m_textDescription.Dock = m_textCycleStart.Dock = m_textCycleEnd.Dock =
        m_textSequence.Dock = m_textDetection.Dock = DockStyle.Fill;

      int row = 0;
      if (!string.IsNullOrEmpty (description)) {
        verticalScroll.AddControl (m_labelDescription, 0, row);
        verticalScroll.AddControl (m_textDescription, 1, row++);
        m_textDescription.Text = description;
      }
      verticalScroll.AddControl (m_labelCycleStart, 0, row);
      verticalScroll.AddControl (m_textCycleStart, 1, row++);
      verticalScroll.AddControl (m_oldCycleStart, 1, row++);
      verticalScroll.AddControl (m_labelCycleEnd, 0, row);
      verticalScroll.AddControl (m_textCycleEnd, 1, row++);
      verticalScroll.AddControl (m_oldCycleEnd, 1, row++);
      verticalScroll.AddControl (m_labelSequence, 0, row);
      verticalScroll.AddControl (m_textSequence, 1, row++);
      verticalScroll.AddControl (m_oldSequence, 1, row++);
      verticalScroll.AddControl (m_labelDetection, 0, row);
      verticalScroll.AddControl (m_textDetection, 1, row++);
      verticalScroll.AddControl (m_oldDetection, 1, row++);
    }
    #endregion // Constructors
  }
}
