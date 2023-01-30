// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of CncSeverityPatternDetails.
  /// </summary>
  public partial class CncSeverityPatternDetails : UserControl
  {
    #region Events
    /// <summary>
    /// Emitted when the button "Edit pattern" is clicked
    /// The first argument is the pattern
    /// </summary>
    public event Action<ICncAlarmSeverityPattern> EditPatternClicked;

    /// <summary>
    /// Emitted when the button "Delete pattern" is clicked
    /// The first argument is the pattern
    /// </summary>
    public event Action<ICncAlarmSeverityPattern> DeletePatternClicked;
    #endregion // Events

    #region Members
    readonly ICncAlarmSeverityPattern m_pattern = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CncSeverityPatternDetails).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="pattern"></param>
    public CncSeverityPatternDetails (ICncAlarmSeverityPattern pattern)
    {
      m_pattern = pattern;
      InitializeComponent ();

      labelPattern.Text = pattern.Description;
    }
    #endregion // Constructors

    #region Event reactions
    void ButtonEditClick (object sender, EventArgs e)
    {
      EditPatternClicked (m_pattern);
    }

    void ButtonRemoveClick (object sender, EventArgs e)
    {
      DeletePatternClicked (m_pattern);
    }
    #endregion // Event reactions
  }
}
