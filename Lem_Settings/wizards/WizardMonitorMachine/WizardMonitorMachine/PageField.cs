// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of PageField.
  /// </summary>
  internal partial class PageField : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Performance field"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "Select in the list the most suitable parameter to represent " +
"the real time performance of the machine. This performance will then be displayed in " +
"the performance panel.";
      }
    }

    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes
    {
      get {
        IList<Type> types = new List<Type> ();
        types.Add (typeof (IField));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageField ()
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
      // Load the different fields
      listFields.ClearItems ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          IList<IField> fields = ModelDAOHelper.DAOFactory.FieldDAO.FindAll ();
          foreach (IField field in fields) {
            if (field.Active && field.CncDataAggregationType.HasValue) {
              string txt = field.Display;
              if (field.Unit != null && field.Unit.Display != "") {
                txt += " (" + field.Unit.Display + ")";
              }

              listFields.AddItem (txt, field);
            }
          }
        }
      }
      listFields.InsertItem ("none", null, 0, SystemColors.ControlText, true, false);
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      listFields.SelectedValue = data.Get<IField> (Item.FIELD);
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (Item.FIELD, listFields.SelectedValue);
    }

    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings (ItemData data)
    {
      IList<string> warnings = new List<string> ();

      IDictionary<string, CncDocument> xmlData = data.Get<Dictionary<string, CncDocument>> (Item.XML_DATA);
      CncDocument cncDoc = xmlData[data.Get<string> (Item.CONFIG_FILE)];
      if (data.Get<IField> (Item.FIELD) != null) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
            IField field = data.Get<IField> (Item.FIELD);
            ModelDAOHelper.DAOFactory.FieldDAO.Lock (field);
            if (field.Unit != null) {
              if (field.Unit.Id == 1 && cncDoc.IsImperial () ||
                  field.Unit.Id == 2 && cncDoc.IsMetric ()) {
                warnings.Add ("The unit of the field chosen (" + field.Unit.Display +
                             ") might be not compatible with the unit of the .xml file (" +
                             cncDoc.Unit + ").");
              }
            }
          }
        }
      }

      return warnings;
    }

    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName (ItemData data)
    {
      return "PageOperationBar";
    }

    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary (ItemData data)
    {
      IList<string> summary = new List<string> ();

      if (data.Get<IField> (Item.FIELD) == null) {
        summary.Add ("performance field: -");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
            IField field = data.Get<IField> (Item.FIELD);
            ModelDAOHelper.DAOFactory.FieldDAO.Lock (field);
            summary.Add ("performance field: \"" + field.Display + "\"");
          }
        }
      }

      return summary;
    }
    #endregion // Page methods
  }
}
