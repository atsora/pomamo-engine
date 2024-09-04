// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    List<string> m_foundCncTypes = new List<string> ();
    readonly List<string> m_allCncTypes = new List<string> ();
    bool m_advancedMode = false;
    string m_currentCncType = "";
    bool m_dontReinit = false;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Severities by CNC"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "All CNC are listed with their associated alarm severities.\n\n" +
          "You can then decide the state of a severity. If a checkbox is checked, this means that the severity is damaging for the client. " +
          "If a checkbox is unchecked, the severity will be ignored. If the checkbox is in an undetermined state, this means that we either don't know " +
          "or that the severity is sometimes important, sometimes not.\n\n" +
          "You can also change the color of a severity.";
      }
    }

    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags
    {
      get { return LemSettingsGlobal.PageFlag.DONT_SHOW_SUCCESS_INFORMATION; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1 ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize (ItemContext context)
    {
      if (m_dontReinit) {
        return;
      }

      System.Diagnostics.Debug.WriteLine ("initialize");
      m_advancedMode = context.UserCategory == LemSettingsGlobal.UserCategory.ADMINISTRATOR ||
        context.UserCategory == LemSettingsGlobal.UserCategory.SUPER_ADMIN;
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      if (m_dontReinit) {
        return;
      }

      data.Store (ItemFocus.CURRENT_CNC, "");
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          // Get all alarms severities with their cnc types
          var severities = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.FindAll ();
          m_allCncTypes.Clear ();
          foreach (var severity in severities) {
            if (!m_allCncTypes.Contains (severity.CncInfo)) {
              m_allCncTypes.Add (severity.CncInfo);
            }
          }

          m_allCncTypes.Sort ();
          m_allCncTypes.Remove ("CncTest");

          // Get all cnc types found in the existing alarms
          m_foundCncTypes = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindAllCncTypes () as List<string>;
          m_foundCncTypes.Sort ();
        }
      }

      // Clear the list
      verticalScroll.Clear ();

      // Create a section for each cnc type found in the alarms
      foreach (var cncType in m_foundCncTypes) {
        AddCncTypeSection (cncType, true);
      }

      // Display the other severities that have not been found in the current alarms
      foreach (var cncType in m_allCncTypes) {
        if (!m_foundCncTypes.Contains (cncType)) {
          AddCncTypeSection (cncType, false);
        }
      }
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (ItemFocus.CURRENT_CNC, m_currentCncType);
    }
    #endregion // Page methods

    #region Private methods
    void AddCncTypeSection (string cncType, bool alarmsFound)
    {
      // Create a widget
      var widget = new CncTypeSection (cncType, alarmsFound, m_advancedMode);

      // Connect the events
      widget.FocusChanged += OnFocusChanged;
      widget.ColorChanged += OnColorChanged;
      widget.DetailsClicked += OnDetailsClicked;

      // Add the widget in the layout
      widget.Dock = DockStyle.Fill;
      verticalScroll.AddControl (widget);
    }
    #endregion // Private methods

    #region Event reactions
    void OnFocusChanged (ICncAlarmSeverity severity, bool? focused)
    {
      // Change the focus state of a severity in the database
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.Lock (severity);
          severity.Focus = focused;
          ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakePersistent (severity);
          transaction.Commit ();
        }
      }

      // Notify a successful change
      string txt = String.Format ("Focus state of severity '{0}.{1}' changed to '{2}'",
                                 severity.CncInfo, severity.Name, focused);
      EmitLogAction ("OnFocusChanged", txt, "ok");
      m_dontReinit = true;
      EmitDataChangedEvent (null);
      m_dontReinit = false;
    }

    void OnColorChanged (ICncAlarmSeverity severity, Color? color)
    {
      // Change the color of a severity in the database
      string colorHtml = color.HasValue ?
        "#" + color.Value.R.ToString ("X2") + color.Value.G.ToString ("X2") + color.Value.B.ToString ("X2") :
        null;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.Lock (severity);
          severity.Color = colorHtml;
          ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakePersistent (severity);
          transaction.Commit ();
        }
      }

      // Notify a successful change
      string txt = String.Format ("Color of severity '{0}.{1}' changed to '{2}'",
                                 severity.CncInfo, severity.Name, colorHtml ?? "null");
      EmitLogAction ("OnColorChanged", txt, "ok");
      m_dontReinit = true;
      EmitDataChangedEvent (null);
      m_dontReinit = false;
    }

    void OnDetailsClicked (string cncType)
    {
      m_currentCncType = cncType;
      EmitDisplayPageEvent ("Page2", null, false);
    }
    #endregion // Event reactions
  }
}
