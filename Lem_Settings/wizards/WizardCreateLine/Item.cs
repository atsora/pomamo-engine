// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of Wizard.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string LINE_NAME = "line_name";
    internal const string LINE_CODE = "line_code";
    internal const string CURRENT_PART_INDEX = "current_part_index";
    internal const string PARTS = "parts";
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public override string Title { get { return "Create a production line"; } }
    
    /// <summary>
    /// Description
    /// </summary>
    public override string Description {
      get {
        return "A new production line will be created, " +
          "by identifying the different parts, operations and assigning machines.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "lines", "productions", "machines", "parts" };
      }
    }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Production line"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Line settings"; } }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "wizard"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(ILine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IOperation)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(Lemoine.Model.IComponent)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IProject)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;

    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new Page0());
        pages.Add(new Page1());
        pages.Add(new Page2());
        pages.Add(new Page3());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Wizard methods
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    public ItemData Initialize(ItemData otherData)
    {
      var data = new ItemData();
      
      // Common data
      data.CurrentPageName = "";
      data.InitValue(LINE_NAME, typeof(string), "", true);
      data.InitValue(LINE_CODE, typeof(string), "", true);
      data.InitValue(PARTS, typeof(List<StructPart>), new List<StructPart>(), true);
      data.InitValue(CURRENT_PART_INDEX, typeof(int), -1, false);
      
      return data;
    }
    
    /// <summary>
    /// All settings are done, changes will take effect
    /// This method is already within a try / catch
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public void Finalize(ItemData data, ref IList<string> warnings, ref IRevision revision)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          // Line creation
          ILine line = ModelDAOHelper.ModelFactory.CreateLine();
          line.Name = data.Get<string>(LINE_NAME);
          if (data.Get<string>(LINE_CODE) != "") {
            line.Code = data.Get<string>(LINE_CODE);
          }

          ModelDAOHelper.DAOFactory.LineDAO.MakePersistent(line);
          
          // Create parts with operations and machines
          IList<StructPart> parts = data.Get<List<StructPart>>(PARTS);
          IDictionary<IOperation, int> orderStorage = new Dictionary<IOperation, int>();
          foreach (StructPart part in parts) {
            CreatePart (part, line, orderStorage);
          }

          transaction.Commit();
        }
      }
    }
    
    // Within a transaction
    void CreatePart(StructPart structPart, ILine line, IDictionary<IOperation, int> orderStorage)
    {
      // Create the part, if necessary
      IPart part = structPart.Part;
      if (structPart.Existing) {
        ModelDAOHelper.DAOFactory.PartDAO.Lock(part);
      }
      else {
        part.Name = structPart.PartName;
        part.Code = structPart.PartCode;
        ModelDAOHelper.DAOFactory.PartDAO.MakePersistent(part);
      }
      line.AddComponent(part.Component);

      // Find previous operations, if any
      IOperation previousOperation = null;
      int operationOrder = 0;
      if (structPart.BaseOperation != null) {
        previousOperation = structPart.BaseOperation;
        operationOrder = orderStorage[structPart.BaseOperation] + 1;
      }
      
      // Create operations
      foreach (StructOperation structOperation in structPart.Operations)
      {
        // SimpleOperation
        IOperation operation = CreateOperationWithMachine(line, structOperation);

        // IntermediateWorkPiece within the component
        if (structOperation.Existing) {
          ModelDAOHelper.DAOFactory.OperationDAO.Lock(structOperation.Operation);
        }
        else {
          foreach (IIntermediateWorkPiece iwp in operation.IntermediateWorkPieces) {
            IComponentIntermediateWorkPiece ciwp = part.Component.AddIntermediateWorkPiece(iwp);
            ciwp.Order = operationOrder;
            ciwp.Code = operation.Code;
            ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent(ciwp);
          }
        }
        orderStorage[structOperation.Operation] = operationOrder;
        operationOrder++;

        // Clear next operations (belonging to the part)
        if (operation.IntermediateWorkPieces != null) {
          foreach (IIntermediateWorkPiece iwp in operation.IntermediateWorkPieces) {
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock(iwp);
            ICollection<IOperation> ops = new List<IOperation>(iwp.PossibleNextOperations);
            if (ops != null) {
              foreach (IOperation op in ops) {
                foreach (StructOperation structOperationTmp in structPart.Operations) {
                  if (Object.Equals(structOperationTmp.Operation, op)) {
                    iwp.RemovePossibleNextOperation(op);
                    break;
                  }
                }
              }
            }
          }
        }
        
        // Link previous operation to current operation, if possible
        if (previousOperation != null) {
          foreach (IIntermediateWorkPiece iwp in previousOperation.IntermediateWorkPieces) {
            iwp.AddPossibleNextOperation(operation);
          }
        }
        previousOperation = operation;
      }

      // Specify the final work piece of the component
      if (previousOperation != null && previousOperation.IntermediateWorkPieces.Count == 1) {
        part.Component.FinalWorkPiece = previousOperation.IntermediateWorkPieces.First();
      }

      ModelDAOHelper.DAOFactory.ComponentDAO.MakePersistent(part.Component);
    }
    
    // Within a transaction
    IOperation CreateOperationWithMachine(ILine line, StructOperation structOperation)
    {
      // Create a simple operation if necessary
      IOperation operation = structOperation.Operation;
      if (structOperation.Existing) {
        ModelDAOHelper.DAOFactory.OperationDAO.Lock(operation);
      }
      else {
        ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent(operation);
        IIntermediateWorkPiece iwp = ModelDAOHelper.ModelFactory.CreateIntermediateWorkPiece(operation);
        iwp.Name = structOperation.Name;
        iwp.Code = structOperation.Code;
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent(iwp);
        operation.AddIntermediateWorkPiece(iwp);
      }
      
      // Change operation attributes
      operation.Name = structOperation.Name;
      operation.Code = structOperation.Code;
      foreach (IIntermediateWorkPiece iwp in operation.IntermediateWorkPieces) {
        iwp.OperationQuantity = structOperation.m_maxPartsPerCycle;
      }

      // Link it to machines within the line
      foreach (StructMachine machine in structOperation.m_machines) {
        ILineMachine lineMachine = ModelDAOHelper.ModelFactory.CreateLineMachine(line, machine.m_machine, operation);
        if (machine.m_dedicated) {
          lineMachine.LineMachineStatus = LineMachineStatus.Dedicated;
        }
        else {
          lineMachine.LineMachineStatus = LineMachineStatus.Extra;
        }

        ModelDAOHelper.DAOFactory.LineMachineDAO.MakePersistent(lineMachine);
      }
      
      return operation;
    }
    #endregion // Wizard methods
  }
}
