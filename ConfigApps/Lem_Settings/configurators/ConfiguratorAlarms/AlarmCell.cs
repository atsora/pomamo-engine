// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of AlarmCell.
  /// </summary>
  public partial class AlarmCell : UserControl
  {
    #region Members
    readonly Alarm m_alarm;
    #endregion // Members

    #region Events
    /// <summary>
    /// Event triggered when the button "Edit" is clicked
    /// The argument is the alarm that will be edited
    /// </summary>
    public event Action<Alarm> EditTriggered;
    
    /// <summary>
    /// Event triggered when the button "Delete" is clicked
    /// The argument is the alarm that will be deleted
    /// </summary>
    public event Action<Alarm> DeleteTriggered;
    #endregion
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="alarm"></param>
    /// <param name="withUserInformation"></param>
    public AlarmCell(Alarm alarm, bool withUserInformation)
    {
      m_alarm = alarm;
      InitializeComponent();
      
      // Fill information
      labelLine1.Text = alarm.GetLine(1);
      labelLine2.Text = alarm.GetLine(2);
      labelLine3.Text = alarm.GetLine(3);
      labelLine4.Text = alarm.GetLine(4);
      labelLine5.Text = alarm.GetLine(5);
      
      if (alarm.IsExpired) {
        labelLine1.ForeColor = labelLine2.ForeColor = labelLine3.ForeColor =
          labelLine4.ForeColor = labelLine5.ForeColor = SystemColors.GrayText;
      }
      
      if (!alarm.AlarmActivated) {
        labelLine1.ForeColor = labelLine2.ForeColor = labelLine3.ForeColor =
          labelLine4.ForeColor = labelLine5.ForeColor = SystemColors.ControlDarkDark;
      }

      if (!withUserInformation) {
        baseLayout.RowStyles[2].Height = 0;
        this.MinimumSize = new Size(200, 81);
        this.Size = new Size(200, 81);
        labelLine3.Hide();
      }
    }
    #endregion // Constructors

    #region Event reactions
    void ButtonRemoveClick(object sender, EventArgs e)
    {
      if (DeleteTriggered != null) {
        DeleteTriggered (m_alarm);
      }
    }
    
    void ButtonEditClick(object sender, EventArgs e)
    {
      if (EditTriggered != null) {
        EditTriggered (m_alarm);
      }
    }
    #endregion // Event reactions
  }
}
