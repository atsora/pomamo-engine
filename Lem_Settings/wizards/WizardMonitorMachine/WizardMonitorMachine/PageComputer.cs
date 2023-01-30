// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of PageComputer.
  /// </summary>
  internal partial class PageComputer : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Select computer (LPOST)"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "Data sent by the machine will be received by a server. " +
"Select here which computer you want to use as a server.";
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
        types.Add (typeof (IComputer));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageComputer ()
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
      // Load the different sources of data
      listComputers.ClearItems ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          IList<IComputer> computers = ModelDAOHelper.DAOFactory.ComputerDAO.GetLposts ();
          foreach (IComputer computer in computers) {
            listComputers.AddItem (computer.Display, computer);
          }
        }
      }
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      listComputers.SelectedValue = data.Get<IComputer> (Item.COMPUTER);
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (Item.COMPUTER, listComputers.SelectedValue);
    }

    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext (ItemData data)
    {
      IList<string> errors = new List<string> ();

      IComputer computer = data.Get<IComputer> (Item.COMPUTER);
      if (computer == null) {
        errors.Add ("a computer must be selected");
      }

      return errors;
    }

    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName (ItemData data)
    {
      return null;
    }

    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary (ItemData data)
    {
      IList<string> summary = new List<string> ();

      IComputer computer = data.Get<IComputer> (Item.COMPUTER);
      if (computer != null) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
            ModelDAOHelper.DAOFactory.ComputerDAO.Lock (computer);
            summary.Add ("Machine receiving the data: \"" + computer.Display + "\"");
          }
        }
      }
      else {
        summary.Add ("No machines receiving the data.");
      }

      return summary;
    }
    #endregion // Page methods
  }
}
