// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of PagePattern.
  /// </summary>
  internal partial class PagePattern : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    CncAlarmSeverityPatternRules m_patternRules = new CncAlarmSeverityPatternRules ();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "..."; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "Change here the properties of a pattern, used to recognize a severity on an alarm.\n\n" +
          "What is in 'Acq. sub-info', 'Type', 'Number' and 'Message' are regular expressions that must match an alarm. " +
          "In 'Properties' are fields that must absolutely match (no regular expression here).";
      }
    }

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
    public PagePattern ()
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
      var currentPattern = data.Get<ICncAlarmSeverityPattern> (ItemFocus.CURRENT_PATTERN);
      if (currentPattern == null) {
        m_patternRules = new CncAlarmSeverityPatternRules ();
        var currentSeverity = data.Get<ICncAlarmSeverity> (ItemFocus.CURRENT_SEVERITY);
        EmitSetTitle ("Create a new severity for the severity '" + currentSeverity.Name + "'");
        ReloadPattern ();
      }
      else {
        m_patternRules = currentPattern.Rules;
        EmitSetTitle ("Edit a pattern for the severity '" + currentPattern.Severity.Name + "'");
      }

      labelAcquisitionInfo.Text = data.Get<string> (ItemFocus.CURRENT_CNC);
      ReloadPattern ();
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      // Regular expressions
      data.Store (ItemFocus.PATTERN_ACQUISITION_INFO, textAcquisitionInfo.Text);
      data.Store (ItemFocus.PATTERN_TYPE, textType.Text);
      data.Store (ItemFocus.PATTERN_NUMBER, textNumber.Text);
      data.Store (ItemFocus.PATTERN_MESSAGE, textMessage.Text);

      // Property list
      IDictionary<string, string> properties = new Dictionary<string, string> ();
      foreach (var value in listProperties.Values) {
        var pair = (KeyValuePair<string, string>)value;
        properties[pair.Key] = pair.Value;
      }
      data.Store (ItemFocus.PATTERN_PROPERTIES, properties);
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

      var acquisitionInfo = data.Get<string> (ItemFocus.PATTERN_ACQUISITION_INFO);
      var type = data.Get<string> (ItemFocus.PATTERN_TYPE);
      var number = data.Get<string> (ItemFocus.PATTERN_NUMBER);
      var message = data.Get<string> (ItemFocus.PATTERN_MESSAGE);
      var properties = data.Get<IDictionary<string, string>> (ItemFocus.PATTERN_PROPERTIES);

      // The pattern cannot be empty
      if (string.IsNullOrEmpty (type) && string.IsNullOrEmpty (number) &&
          string.IsNullOrEmpty (message) && string.IsNullOrEmpty (acquisitionInfo) && properties.Count == 0) {
        errors.Add ("the pattern cannot be empty");
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
      // Build the pattern
      var rules = new CncAlarmSeverityPatternRules ();
      rules.CncSubInfo = data.Get<string> (ItemFocus.PATTERN_ACQUISITION_INFO);
      rules.Type = data.Get<string> (ItemFocus.PATTERN_TYPE);
      rules.Number = data.Get<string> (ItemFocus.PATTERN_NUMBER);
      rules.Message = data.Get<string> (ItemFocus.PATTERN_MESSAGE);
      var properties = data.Get<IDictionary<string, string>> (ItemFocus.PATTERN_PROPERTIES);
      if (properties != null) {
        rules.Properties = properties;
      }
      else {
        rules.Properties.Clear ();
      }

      var pattern = data.Get<ICncAlarmSeverityPattern> (ItemFocus.CURRENT_PATTERN);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          if (pattern == null) {
            // Create a pattern
            pattern = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern (
              data.Get<string> (ItemFocus.CURRENT_CNC), rules,
              data.Get<ICncAlarmSeverity> (ItemFocus.CURRENT_SEVERITY));
            pattern.Status = EditStatus.MANUAL_INPUT;
          }
          else {
            // Update an existing pattern
            ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.Lock (pattern);
            if (pattern.Status != EditStatus.MANUAL_INPUT) {
              pattern.Status = EditStatus.DEFAULT_VALUE_EDITED;
            }

            pattern.Rules = rules;
          }

          ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.MakePersistent (pattern);
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

    #region Private methods
    void ReloadPattern ()
    {
      buttonRemoveProperty.Enabled = buttonEditProperty.Enabled = false;
      listProperties.ClearItems ();

      // Regular expression for CncSubInfo
      textAcquisitionInfo.Text = m_patternRules.CncSubInfo;

      // Regular expression for Type
      textType.Text = m_patternRules.Type;

      // Regular expression for Number
      textNumber.Text = m_patternRules.Number;

      // Regular expression for Message
      textMessage.Text = m_patternRules.Message;

      // Properties that must match
      var properties = m_patternRules.Properties;
      if (properties != null) {
        foreach (var key in properties.Keys) {
          listProperties.AddItem (key + " is '" + properties[key] + "'", new KeyValuePair<string, string> (key, properties[key]));
        }
      }
    }
    #endregion // Private methods

    #region Event reactions
    void ButtonClearAcquisitionInfoClick (object sender, EventArgs e)
    {
      textAcquisitionInfo.Clear ();
    }

    void ButtonClearTypeClick (object sender, EventArgs e)
    {
      textType.Clear ();
    }

    void ButtonClearNumberClick (object sender, EventArgs e)
    {
      textNumber.Clear ();
    }

    void ButtonClearMessageClick (object sender, EventArgs e)
    {
      textMessage.Clear ();
    }

    void ButtonClearPropertiesClick (object sender, EventArgs e)
    {
      listProperties.ClearItems ();
    }

    void ButtonAddPropertyClick (object sender, EventArgs e)
    {
      var dialog = new PropertyDialog ();
      if (dialog.ShowDialog (this) == DialogResult.OK) {
        if (string.IsNullOrEmpty (dialog.Property)) {
          MessageBoxCentered.Show ("The property cannot be empty", "Warning");
          return;
        }
        if (string.IsNullOrEmpty (dialog.Value)) {
          MessageBoxCentered.Show ("The value cannot be empty", "Warning");
          return;
        }
        listProperties.AddItem (dialog.Property + " is '" + dialog.Value + "'",
                               new KeyValuePair<string, string> (dialog.Property, dialog.Value));
        listProperties.SelectedIndex = listProperties.Count - 1;
      }
    }

    void ButtonRemovePropertyClick (object sender, EventArgs e)
    {
      if (listProperties.SelectedIndex != -1) {
        listProperties.RemoveIndex (listProperties.SelectedIndex);
      }
    }

    void ListPropertiesItemChanged (string arg1, object arg2)
    {
      buttonRemoveProperty.Enabled = buttonEditProperty.Enabled = (listProperties.SelectedIndex != -1);
    }

    void ButtonEditPropertyClick (object sender, EventArgs e)
    {
      if (listProperties.SelectedValue == null) {
        return;
      }

      var pair = (KeyValuePair<string, string>)listProperties.SelectedValue;

      // Prepare the dialog
      var dialog = new PropertyDialog ();
      dialog.Property = pair.Key;
      dialog.Value = pair.Value.ToString ();

      if (dialog.ShowDialog (this) == DialogResult.OK) {
        if (string.IsNullOrEmpty (dialog.Property)) {
          MessageBoxCentered.Show ("The property cannot be empty", "Warning");
          return;
        }
        if (string.IsNullOrEmpty (dialog.Value)) {
          MessageBoxCentered.Show ("The value cannot be empty", "Warning");
          return;
        }

        // Update the list
        listProperties.UpdateItemText (pair, dialog.Property + " is '" + dialog.Value + "'");
        listProperties.UpdateItemValue (pair, new KeyValuePair<string, string> (dialog.Property, dialog.Value));
        listProperties.Refresh ();
      }
    }
    #endregion // Event reactions
  }
}
