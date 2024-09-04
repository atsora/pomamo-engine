// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class PageSeverityDescription : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    string m_currentCncType = "";
    ICncAlarmSeverity m_currentSeverity = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "..."; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Change here the properties of a severity (name, description, ...)."; } }

    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags
    {
      get { return LemSettingsGlobal.PageFlag.WITH_VALIDATION; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageSeverityDescription ()
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
      // Fill the different stop status
      comboStopStatus.ClearItems ();
      foreach (CncAlarmStopStatus stopStatus in Enum.GetValues (typeof (CncAlarmStopStatus))) {
        comboStopStatus.AddItem (stopStatus.ToString (), stopStatus);
      }
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      m_currentCncType = data.Get<string> (ItemFocus.CURRENT_CNC);
      m_currentSeverity = data.Get<ICncAlarmSeverity> (ItemFocus.CURRENT_SEVERITY);

      if (m_currentSeverity == null) {
        EmitSetTitle ("Create a new severity for the CNC '" + m_currentCncType + "'");
        textName.Text = "new severity";
        textDescription.Text = "explain what is this severity";
        comboStopStatus.SelectedValue = CncAlarmStopStatus.Unknown;
      }
      else {
        EmitSetTitle ("Edit a severity of the CNC '" + m_currentCncType + "'");
        textName.Text = m_currentSeverity.Name;
        textDescription.Text = m_currentSeverity.Description;
        comboStopStatus.SelectedValue = m_currentSeverity.StopStatus;
      }
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (ItemFocus.SEVERITY_NAME, textName.Text);
      data.Store (ItemFocus.SEVERITY_DESCRIPTION, textDescription.Text);
      data.Store (ItemFocus.SEVERITY_STOP_STATUS, comboStopStatus.SelectedValue);
    }

    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation (ItemData data)
    {
      var errors = new List<string> ();

      var name = data.Get<string> (ItemFocus.SEVERITY_NAME);
      if (string.IsNullOrEmpty (name)) {
        errors.Add ("the name cannot be empty");
      }
      else {
        // Check that no other severities among the same cnc type exist
        var currentSeverity = data.Get<ICncAlarmSeverity> (ItemFocus.CURRENT_SEVERITY);
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
            var severities = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.FindByCnc (data.Get<string> (ItemFocus.CURRENT_CNC), true);
            foreach (var severity in severities) {
              if (currentSeverity == null || currentSeverity.Id != severity.Id) {
                if (severity.Status != EditStatus.DEFAULT_VALUE_DELETED && string.Equals (severity.Name, name, StringComparison.CurrentCultureIgnoreCase)) {
                  errors.Add ("the name is already used by another severity");
                  break;
                }
              }
            }
          }
        }
      }

      return errors;
    }

    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revisionId">Revision that is going to be applied when the function returns (not used for views)</param>
    public override void Validate (ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      var name = data.Get<string> (ItemFocus.SEVERITY_NAME);
      var description = data.Get<string> (ItemFocus.SEVERITY_DESCRIPTION);
      var stopStatus = data.Get<CncAlarmStopStatus> (ItemFocus.SEVERITY_STOP_STATUS);
      var currentCncType = data.Get<string> (ItemFocus.CURRENT_CNC);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          var currentSeverity = data.Get<ICncAlarmSeverity> (ItemFocus.CURRENT_SEVERITY);
          if (currentSeverity == null) {
            // NEW SEVERITY TO CREATE
            var previousSeverity = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.FindByCncName (currentCncType, name);
            if (previousSeverity != null) {
              // We modify an existing default severity that has been previously removed
              currentSeverity = previousSeverity;
              Page2.DeletePatterns (currentSeverity);
              currentSeverity.Status = EditStatus.DEFAULT_VALUE_EDITED;
            }
            else {
              // We create a new severity with a name that doesn't exist
              currentSeverity = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity (currentCncType, name);
              currentSeverity.Status = EditStatus.MANUAL_INPUT;
            }
          }
          else {
            // WE MODIFY AN EXISTING SEVERITY
            ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.Lock (currentSeverity);
            if (!string.Equals (currentSeverity.Name, name)) {
              // The name changed
              var previousSeverity = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.FindByCncName (currentCncType, name);
              if (previousSeverity != null) {
                // The name changed to another name that already exists
                if (previousSeverity.Id == currentSeverity.Id) {
                  // Only lowercase / uppercase changed
                  currentSeverity.Name = "something that won't be chosen KJFHUIU?";
                  ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakePersistent (currentSeverity);
                  ModelDAOHelper.DAOFactory.Flush ();
                  currentSeverity.Name = name;
                }
                else {
                  Page2.DeletePatterns (previousSeverity);
                  Page2.CopyPatterns (currentSeverity, previousSeverity);
                  Page2.DeleteSeverity (currentSeverity);
                  if (previousSeverity.Status != EditStatus.MANUAL_INPUT) {
                    previousSeverity.Status = EditStatus.DEFAULT_VALUE_EDITED;
                  }

                  currentSeverity = previousSeverity;
                }
              }
              else {
                // The name changed to a name that doesn't exist
                if (currentSeverity.Status != EditStatus.MANUAL_INPUT) {
                  var newSeverity = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity (currentCncType, name);
                  ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakePersistent (newSeverity);
                  newSeverity.Status = EditStatus.MANUAL_INPUT;
                  Page2.CopyPatterns (currentSeverity, newSeverity);
                  Page2.DeleteSeverity (currentSeverity);
                  currentSeverity = newSeverity;
                }
                else {
                  // A manual input just changed its name
                  currentSeverity.Name = name;
                }
              }
            }
            else {
              // The name didn't change
              if (currentSeverity.Status != EditStatus.MANUAL_INPUT && (
                !string.Equals (currentSeverity.Description, description) || currentSeverity.StopStatus != stopStatus
               )) {
                currentSeverity.Status = EditStatus.DEFAULT_VALUE_EDITED;
              }
            }
          }

          // Description and StopStatus are updated
          currentSeverity.Description = description;
          currentSeverity.StopStatus = stopStatus;

          ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO.MakePersistent (currentSeverity);
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// If the validation step is enabled, method called after the validation and after the possible progress
    /// bar linked to a revision (the user or the timeout could have canceled the progress bar but in that
    /// case a warning is displayed).
    /// Don't forget to emit "DataChangedEvent" if data changed
    /// </summary>
    /// <param name="data">data that can be processed before the page changes</param>
    public override void ProcessAfterValidation (ItemData data)
    {
      EmitDataChangedEvent (null);
    }
    #endregion // Page methods
  }
}
