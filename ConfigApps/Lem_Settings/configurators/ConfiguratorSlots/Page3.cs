// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
  public partial class Page3 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    IPartPage3 m_part = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Configuration of a slot"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return m_part.Description; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes { get { return m_part.EditableTypes; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page3(IPartPage3 part)
    {
      InitializeComponent();
      m_part = part;
      groupBoxElements.Text = m_part.ElementName;
      groupBoxParameters.Controls.Add(m_part as Control);
      (m_part as Control).Dock = DockStyle.Fill;
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context)
    {
      m_part.Initialize();
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Start and end dates
      DateTime start = data.Get<DateTime>(AbstractItem.PERIOD_START);
      labelFrom.Text = start.ToShortDateString() + " " + start.ToString("T");
      if (data.Get<bool>(AbstractItem.PERIOD_HAS_END)) {
        DateTime end = data.Get<DateTime>(AbstractItem.PERIOD_END);
        labelTo.Text = end.ToShortDateString() + " " + end.ToString("T");
      } else {
        labelTo.Text = "(no end)";
      }

      // Elements
      IList<string> names = m_part.GetElementName(data.Get<List<object>>(AbstractItem.SELECTED_ITEMS));
      if (names != null && names.Count > 0) {
        textMachines.Text = string.Join(", ", names.ToArray());
        baseLayout.RowStyles[0].Height = 60;
        groupBoxElements.Show();
      } else {
        baseLayout.RowStyles[0].Height = 0;
        groupBoxElements.Hide();
      }

      m_part.LoadPageFromData(data);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      m_part.SavePageInData(data);
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      return m_part.GetErrorsBeforeValidation(data);
    }
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revisionId">Revision that is going to be applied when the function returns</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        var revision = ModelDAOHelper.DAOFactory.RevisionDAO.FindById (revisionId);
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          // Revision creation
          revision = ModelDAOHelper.ModelFactory.CreateRevision();
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent(revision);
          
          IList<object> items = data.Get<List<object>>(AbstractItem.SELECTED_ITEMS);
          foreach (object item in items) {
            // Get the modification
            IModification modification = m_part.PrepareModification(data, item);
            if (modification != null) {
              modification.Revision = revision;
            }
          }
          
          transaction.Commit();
        }
      }
      
      // The timelines need to be updated
      data.Store(AbstractItem.TIMELINE_UPDATE, true);
    }
    
    /// <summary>
    /// If the validation step is enabled, method called after the validation and after the possible progress
    /// bar linked to a revision (the user or the timeout could have canceled the progress bar but in that
    /// case a warning is displayed).
    /// Don't forget to emit "DataChangedEvent" if data changed
    /// </summary>
    /// <param name="data">data that can be processed before the page changes</param>
    public override void ProcessAfterValidation(ItemData data)
    {
      EmitDataChangedEvent(null);
    }
    #endregion // Page methods
  }
}
