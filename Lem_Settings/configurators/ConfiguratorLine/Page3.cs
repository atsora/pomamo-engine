// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
  public partial class Page3 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Edit an operation"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "An operation is being edited. What can be modified is:\n" +
          "- the operation name,\n" +
          "- the operation code,\n" +
          "- the  maximum number of parts per cycle (what is the maximum number " +
          "of parts that can produce a machine processing this operation?)."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page3()
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
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IOperation operation = data.Get<IOperation>(Item.OPERATION);
          ModelDAOHelper.DAOFactory.OperationDAO.Lock(operation);
          
          // Text and name
          textName.Text = operation.Name;
          textCode.Text = operation.Code;
          
          // Max parts per cycle (may be disabled if several or no values are found)
          IList<IIntermediateWorkPiece> iwps =
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindByOperation(operation);
          if (iwps.Count > 0) {
            int val = iwps[0].OperationQuantity;
            for (int i = 1; i < iwps.Count; i++) {
              if (val != iwps[i].OperationQuantity) {
                val = -1;
              }
            }
            if (val < 0) {
              numericPartsPerCycle.Value = 1;
              numericPartsPerCycle.Enabled = false;
            } else if (val == 0) {
              numericPartsPerCycle.Value = 1;
              numericPartsPerCycle.Enabled = true;
            } else {
              numericPartsPerCycle.Value = val;
              numericPartsPerCycle.Enabled = true;
            }
          } else {
            numericPartsPerCycle.Value = 1;
            numericPartsPerCycle.Enabled = false;
          }
        }
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.OPERATION_NAME, textName.Text);
      data.Store(Item.OPERATION_CODE, textCode.Text);
      if (numericPartsPerCycle.Enabled) {
        data.Store(Item.OPERATION_MAX_PART, (int)numericPartsPerCycle.Value);
      }
      else {
        data.Store(Item.OPERATION_MAX_PART, -1);
      }
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      // The name must be filled
      if (data.Get<string>(Item.OPERATION_NAME) == "") {
        errors.Add("the name must be specified");
      }

      // Code already taken
      bool ok = true;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IOperation currentOperation = data.Get<IOperation>(Item.OPERATION);
          ModelDAOHelper.DAOFactory.OperationDAO.Lock(currentOperation);
          string currentCode = data.Get<string>(Item.OPERATION_CODE);
          IList<IOperation> operations = ModelDAOHelper.DAOFactory.OperationDAO.FindAll();
          foreach (IOperation operation in operations) {
            if (operation.Code == currentCode && operation != currentOperation) {
              ok = false;
              break;
            }
          }
        }
      }
      if (!ok) {
        errors.Add("the code is already taken by another operation");
      }

      return errors;
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
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          IOperation operation = data.Get<IOperation>(Item.OPERATION);
          ModelDAOHelper.DAOFactory.OperationDAO.Lock(operation);
          operation.Name = data.Get<string>(Item.OPERATION_NAME);
          operation.Code = data.Get<string>(Item.OPERATION_CODE);
          
          // Quantity for all iwps produced by the operation
          int quantity = data.Get<int>(Item.OPERATION_MAX_PART);
          
          IList<IIntermediateWorkPiece> iwps = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindByOperation(operation);
          if (quantity > 0) {
            foreach (IIntermediateWorkPiece iwp in iwps) {
              iwp.OperationQuantity = quantity;
            }
          }
          
          // Name and code of the iwp
          if (iwps.Count == 1) {
            iwps[0].Name = operation.Name;
            iwps[0].Code = operation.Code;
          }
          
          transaction.Commit();
        }
      }
      
      IList<OperationData> ops = data.Get<LineData>(Item.LINE_DATA).GetOperations();
      foreach (OperationData op in ops) {
        if (op.Operation == data.Get<IOperation>(Item.OPERATION)) {
          op.UpdateDisplay();
          break;
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
    public override void ProcessAfterValidation(ItemData data)
    {
      EmitDataChangedEvent(null);
    }
    #endregion // Page methods
  }
}
