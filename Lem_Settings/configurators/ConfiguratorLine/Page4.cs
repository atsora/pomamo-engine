// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of Page4.
  /// </summary>
  public partial class Page4 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Delete a line"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose the elements to delete with the line.\n" +
          "Please be careful, data may be lost."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.DANGEROUS_ACTIONS |
          LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page4()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        label1.Text = "Delete line \"" + data.Get<ILine>(Item.LINE).Display + "\"";
      }
      checkPart.Checked = data.Get<bool>(Item.DELETE_PART);
      checkOperations.Checked = data.Get<bool>(Item.DELETE_OPERATIONS);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.DELETE_PART, checkPart.Checked);
      data.Store(Item.DELETE_OPERATIONS, checkOperations.Checked);
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data) { return null; }
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revisionId">Revision that is going to be applied when the function returns</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      ILine line = data.Get<ILine>(Item.LINE);
      bool deletePart = data.Get<bool>(Item.DELETE_PART);
      bool deleteOperations = data.Get<bool>(Item.DELETE_OPERATIONS);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          
          // Delete line
          ModelDAOHelper.DAOFactory.LineDAO.MakeTransient(line);
          
          foreach (IComponent component in line.Components)
          {
            // Delete component / project / part
            if (deletePart) {
              ModelDAOHelper.DAOFactory.ComponentDAO.MakeTransient(component);
              if (component.Project != null) {
                ModelDAOHelper.DAOFactory.ProjectDAO.MakeTransient(component.Project);
              }

              if (component.Part != null) {
                ModelDAOHelper.DAOFactory.PartDAO.MakeTransient(component.Part);
              }
            }
            
            // Delete operations and intermediate workpieces
            if (deleteOperations) {
              ICollection<IComponentIntermediateWorkPiece> iciwps = component.ComponentIntermediateWorkPieces;
              foreach (IComponentIntermediateWorkPiece iciwp in iciwps) {
                IIntermediateWorkPiece iwp = iciwp.IntermediateWorkPiece;
                ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakeTransient(iwp);
                if (iwp.SimpleOperation != null) {
                  ModelDAOHelper.DAOFactory.SimpleOperationDAO.MakeTransient(iwp.SimpleOperation);
                }

                if (iwp.Operation != null) {
                  ModelDAOHelper.DAOFactory.OperationDAO.MakeTransient(iwp.Operation);
                }
              }
            }
          }
          
          transaction.Commit();
        }
      }
      
      data.Store(Item.LINE, null);
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
