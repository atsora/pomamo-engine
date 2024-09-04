// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    string m_currentCncType = "";
    ICncAlarmSeverity m_severity = null;
    ICncAlarmSeverityPattern m_pattern = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Details of alarm severities"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "Here are listed all severities used for a CNC type. " +
          "For each of them you can edit the rules used to assign the severity to an alarm.\n\n" +
          "It's also possible to add or remove severities.\n\n" +
          "If a default configuration is available for the current CNC, you can restore it.";
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2 ()
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
    public void Initialize (ItemContext context) { }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      m_currentCncType = data.Get<string> (ItemFocus.CURRENT_CNC);
      EmitSetTitle (Title + " (" + m_currentCncType + ")");
      EmitProtectAgainstQuit (false);

      // Possibility to restore a configuration?
      buttonDefault.Enabled = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.AreDefaultValuesAvailable (m_currentCncType);

      // Populate the interface
      ReloadSeverities ();
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (ItemFocus.CURRENT_SEVERITY, m_severity);
      data.Store (ItemFocus.CURRENT_PATTERN, m_pattern);
    }
    #endregion // Page methods

    #region Private methods
    void ReloadSeverities ()
    {
      m_severity = null;
      m_pattern = null;

      // Clear the list
      verticalScroll.Clear ();

      // Find all severities for the selected cnc type
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          var severities = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.FindByCnc (m_currentCncType, false);
          if (severities.Count > 0) {
            // Add a cell for each severity
            foreach (var severity in severities) {
              var widget = new CncSeverityDetails (severity);
              widget.Dock = DockStyle.Fill;
              widget.EditPatternClicked += OnEditPatternClicked;
              widget.DeletePatternClicked += OnDeletePatternClicked;
              widget.AddPatternClicked += OnAddPatternClicked;
              widget.EditDescriptionClicked += OnEditDescriptionClicked;
              widget.DeleteSeverityClicked += OnDeleteSeverityClicked;
              verticalScroll.AddControl (widget);
            }
            stackedWidget.SelectedIndex = 0;
          }
          else {
            stackedWidget.SelectedIndex = 1;
          }
        }
      }
    }

    static internal void DeleteSeverity (ICncAlarmSeverity severity)
    {
      if (severity.Status == EditStatus.MANUAL_INPUT) {
        ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakeTransient (severity);
      }
      else {
        // All severity patterns are also deleted
        var patterns = ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.FindBySeverity (severity, false);
        foreach (var pattern in patterns) {
          DeletePattern (pattern);
        }

        severity.Status = EditStatus.DEFAULT_VALUE_DELETED;
        ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakePersistent (severity);
      }
    }

    static internal void DeletePattern (ICncAlarmSeverityPattern pattern)
    {
      if (pattern.Status == EditStatus.MANUAL_INPUT) {
        ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.MakeTransient (pattern);
      }
      else {
        pattern.Status = EditStatus.DEFAULT_VALUE_DELETED;
        ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.MakePersistent (pattern);
      }
    }

    static internal void DeletePatterns (ICncAlarmSeverity severity)
    {
      // Clear patterns in severity
      var patternsToDelete = ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.FindBySeverity (severity, false);
      foreach (var patternToDelete in patternsToDelete) {
        DeletePattern (patternToDelete);
      }
    }

    static internal void CopyPatterns (ICncAlarmSeverity fromSeverity, ICncAlarmSeverity toSeverity)
    {
      // Copy patterns from fromSeverity to toSeverity
      var patternsToCopy = ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.FindBySeverity (fromSeverity, false);
      foreach (var patternToCopy in patternsToCopy) {
        var newPattern = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern (
          patternToCopy.CncInfo, patternToCopy.Rules, toSeverity);
        newPattern.Status = EditStatus.MANUAL_INPUT;
        ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.MakePersistent (newPattern);
      }
    }
    #endregion // Private methods

    #region Event reactions
    void ButtonAddSeverityClick (object sender, EventArgs e)
    {
      m_severity = null;
      m_pattern = null;

      // Add a severity
      EmitDisplayPageEvent ("PageSeverityDescription", null, false);
    }

    void OnEditDescriptionClicked (ICncAlarmSeverity severity)
    {
      m_severity = severity;
      m_pattern = null;

      // Edit the description of a severity
      EmitDisplayPageEvent ("PageSeverityDescription", null, false);
    }

    void OnDeleteSeverityClicked (ICncAlarmSeverity severity)
    {
      // Delete a severity
      if (MessageBoxCentered.Show ("Are you sure you want to delete this severity?", "Warning",
                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
                                  MessageBoxDefaultButton.Button2) == DialogResult.OK) {
        string name = severity.Name;

        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ("Delete severity")) {
            ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.Lock (severity);
            DeleteSeverity (severity);
            transaction.Commit ();
          }
        }

        // Notify a successful change
        string txt = String.Format ("Severity '{0}.{1}' has been deleted", m_currentCncType, name);
        EmitLogAction ("OnDeleteSeverityClicked", txt, "ok");
        EmitDataChangedEvent (null);

        // Reload the page
        ReloadSeverities ();
      }
    }

    void OnAddPatternClicked (ICncAlarmSeverity severity)
    {
      m_severity = severity;
      m_pattern = null;

      // Add a new pattern to a severity
      EmitDisplayPageEvent ("PagePattern", null, false);
    }

    void OnEditPatternClicked (ICncAlarmSeverityPattern pattern)
    {
      m_severity = null;
      m_pattern = pattern;

      // Edit a pattern
      EmitDisplayPageEvent ("PagePattern", null, false);
    }

    void OnDeletePatternClicked (ICncAlarmSeverityPattern pattern)
    {
      // Delete a pattern
      if (MessageBoxCentered.Show ("Are you sure you want to delete this pattern?", "Warning",
                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
                                  MessageBoxDefaultButton.Button2) == DialogResult.OK) {
        var description = pattern.Description;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ("Delete pattern")) {
            ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.Lock (pattern);
            DeletePattern (pattern);
            transaction.Commit ();
          }
        }

        // Notify a successful change
        string txt = String.Format ("Pattern '{0}' for CNC '{1}' has been deleted", description, m_currentCncType);
        EmitLogAction ("OnDeletePatternClicked", txt, "ok");
        EmitDataChangedEvent (null);

        // Reload the page
        ReloadSeverities ();
      }
    }

    void ButtonDefaultClick (object sender, EventArgs e)
    {
      if (MessageBoxCentered.Show ("Are you sure you want to restore the default values for this CNC?", "Warning",
                                  MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
                                  MessageBoxDefaultButton.Button2) == DialogResult.OK) {
        m_severity = null;
        m_pattern = null;

        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.RestoreDefaultValues (m_currentCncType);
            ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.RestoreDefaultValues (m_currentCncType);
            transaction.Commit ();
          }
        }

        // Restore all parameters for the current cnc
        EmitDataChangedEvent (null);

        // Reload the page
        ReloadSeverities ();
      }
    }
    #endregion // Event reactions
  }
}
