// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of SeverityWidget.
  /// </summary>
  public partial class SeverityWidget : UserControl
  {
    #region Events
    /// <summary>
    /// Event emitted when the focus state of the severity changed
    /// The first argument is the severity
    /// The second argument is focus state associated to it
    /// </summary>
    public event Action<ICncAlarmSeverity, bool?> FocusChanged;

    /// <summary>
    /// Event emitted when the color of the severity changed
    /// The first argument is the severity
    /// The second argument is color associated to it
    /// </summary>
    public event Action<ICncAlarmSeverity, Color?> ColorChanged;
    #endregion // Events

    #region Members
    ICncAlarmSeverity m_severity = null;
    bool m_initialize = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SeverityWidget).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="severity">severity to display</param>
    public SeverityWidget (ICncAlarmSeverity severity)
    {
      InitializeComponent ();
      m_severity = severity;
      labelName.Text = severity.Name;

      // Description
      SetToolTip (pictureHelp, m_severity);

      // Focus
      if (m_severity.Focus != null && m_severity.Focus.HasValue) {
        checkFocus.CheckState = m_severity.Focus.Value ?
          CheckState.Checked : CheckState.Unchecked;
      }
      else {
        checkFocus.CheckState = CheckState.Indeterminate;
      }

      UpdateTextStyle ();

      // Color
      colorPicker.SelectedColor = string.IsNullOrEmpty (m_severity.Color) ? (Color?)null :
        ColorTranslator.FromHtml (m_severity.Color);
      if (colorPicker.SelectedColor == Color.Empty) {
        colorPicker.SelectedColor = null;
      }

      m_initialize = false;
    }
    #endregion // Constructors

    #region Methods
    void SetToolTip (Control control, ICncAlarmSeverity severity)
    {
      // Add a tooltip with the description of the severity
      var toolTip = new ToolTip ();
      toolTip.SetToolTip (control, severity.FullDescription);
      toolTip.AutoPopDelay = 32000;
    }

    void UpdateTextStyle ()
    {
      switch (checkFocus.CheckState) {
      case CheckState.Checked:
        labelName.Font = new Font (labelName.Font, FontStyle.Bold);
        labelName.ForeColor = LemSettingsGlobal.COLOR_ERROR;
        break;
      case CheckState.Unchecked:
        labelName.Font = new Font (labelName.Font, FontStyle.Regular);
        labelName.ForeColor = SystemColors.ControlDarkDark;
        break;
      case CheckState.Indeterminate:
        labelName.Font = new Font (labelName.Font, FontStyle.Regular);
        labelName.ForeColor = SystemColors.ControlText;
        break;
      }
    }
    #endregion // Methods

    #region Event reactions
    void ColorPickerColorChanged (Color? color)
    {
      if (m_initialize) {
        return;
      }

      if (ColorChanged != null) {
        ColorChanged (m_severity, color);
      }
    }

    void CheckFocusCheckStateChanged (object sender, EventArgs e)
    {
      if (m_initialize) {
        return;
      }

      if (FocusChanged != null) {
        UpdateTextStyle ();
        switch (checkFocus.CheckState) {
        case CheckState.Checked:
          FocusChanged (m_severity, true);
          break;
        case CheckState.Unchecked:
          FocusChanged (m_severity, false);
          break;
        case CheckState.Indeterminate:
          FocusChanged (m_severity, null);
          break;
        }
      }
    }
    #endregion // Event reactions
  }
}
