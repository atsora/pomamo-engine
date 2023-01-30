// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of CncTypeSection.
  /// </summary>
  public partial class CncTypeSection : UserControl
  {
    #region Members
    readonly string m_cncType = "";
    #endregion // Members

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

    /// <summary>
    /// Event emitted when the user wants to see details
    /// The first argument is the CNC type
    /// </summary>
    public event Action<string> DetailsClicked;
    #endregion // Events

    static readonly ILog log = LogManager.GetLogger (typeof (CncTypeSection).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="cncType">Name of the cnc type</param>
    /// <param name="alarmsFound">If currently used among the alarms</param>
    /// <param name="advancedMode">True for accessing more information</param>
    public CncTypeSection (string cncType, bool alarmsFound, bool advancedMode)
    {
      InitializeComponent ();
      m_cncType = cncType;
      baseLayout.BackColor = LemSettingsGlobal.COLOR_CATEGORY;
      labelTitle.ForeColor = Color.White;
      if (!advancedMode) {
        buttonDetails.Hide ();
      }

      IList<ICncAlarmSeverity> severities = null;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          severities = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.FindByCnc (m_cncType, false);
        }
      }

      labelTitle.Text = m_cncType + (alarmsFound ? "" : " (currently not used)");
      if (severities.Count == 0) {
        stackedWidget.SelectedIndex = 1;
      }
      else {
        stackedWidget.SelectedIndex = 0;
        int height = 22;
        foreach (var severity in severities) {
          // A widget per severity
          var widget = new SeverityWidget (severity);

          // Connect the events
          widget.FocusChanged += OnFocusChanged;
          widget.ColorChanged += OnColorChanged;

          // Fill the layout
          widget.Dock = DockStyle.Fill;
          verticalScroll.AddControl (widget);
          height += 31;
        }
        this.Height = height;
      }
    }
    #endregion // Constructors

    #region Event reactions
    void OnFocusChanged (ICncAlarmSeverity severity, bool? focused)
    {
      FocusChanged (severity, focused);
    }

    void OnColorChanged (ICncAlarmSeverity severity, Color? color)
    {
      ColorChanged (severity, color);
    }

    void ButtonDetailsClick (object sender, EventArgs e)
    {
      DetailsClicked (m_cncType);
    }
    #endregion // Event reactions
  }
}
