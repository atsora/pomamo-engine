// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericWizardPage, IWizardPage
  {
    #region Members
    int m_currentPartIndex = -1;
    IList<StructOperation> m_operations;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Defining operations"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "A line is made of a sequence of operations, each one representing " +
          "a kind of transformation. All operations are listed in the left part, operations coming from an " +
          "existing part being displayed in blue.\n\n" +
          "Each operation is identified by a name and an optional code. It is possible to add, remove and " +
          "reorder them (except for blue operations: they cannot be removed). The maximum number of parts " +
          "per cycle has further to be defined.\n\n"; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2()
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
    /// Load the page
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Current part (this page is called several times)
      m_currentPartIndex = data.Get<int>(Item.CURRENT_PART_INDEX);
      IList<StructPart> parts = data.Get<List<StructPart>>(Item.PARTS);
      if (m_currentPartIndex < parts.Count) {
        EmitSetTitle(Title + " (part \"" + parts[m_currentPartIndex].PartName + "\")");
        m_operations = parts[m_currentPartIndex].Operations;
      } else {
        m_operations = new List<StructOperation>();
      }

      FillList (0);
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // nothing to do: each change in the page is automatically saved
    }
    
    /// <summary>
    /// Called after SavePage and before going to the previous page
    /// </summary>
    /// <param name="data"></param>
    public override void DoSomethingBeforePrevious(ItemData data)
    {
      //if (m_currentPartIndex == 0)
      data.Store(Item.CURRENT_PART_INDEX, data.Get<int>(Item.CURRENT_PART_INDEX) - 1);
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      // Starting point has been removed => merging operations will be done somewhere else
      //      if (m_currentPartIndex < (data.Get<List<StructPart>>(Item.PARTS)).Count - 1)
      //        return "PageStartingPoint";
      if (m_currentPartIndex < (data.Get<List<StructPart>>(Item.PARTS)).Count - 1) {
        return "Page2";
      }

      return "Page3";
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      IList<StructPart> parts = data.Get<List<StructPart>>(Item.PARTS);
      
      // Current operations
      IList<StructOperation> operations = parts[m_currentPartIndex].Operations;
      
      // Previous operations
      IList<StructOperation> previousOperations =  new List<StructOperation>();
      for (int i = 0; i < m_currentPartIndex; i++) {
        foreach (StructOperation op in parts[i].Operations) {
          previousOperations.Add(op);
        }
      }

      // Following operations
      IList<StructOperation> followingOperations =  new List<StructOperation>();
      for (int i = m_currentPartIndex + 1; i < parts.Count; i++) {
        foreach (StructOperation op in parts[i].Operations) {
          followingOperations.Add(op);
        }
      }

      // At least one operation
      if (operations.Count == 0) {
        errors.Add("at least one operation must be defined");
      }

      // Each operation has a name
      foreach (StructOperation operation in operations) {
        if (operation.Name == "") {
          errors.Add("each operation must have a name");
          break;
        }
      }
      
      // Different names for each operation (ignore empty names => an error already rose)
      string errorName = "";
      foreach (StructOperation operation in operations) {
        if (operation.Name != "" && errorName == "") {
          foreach (StructOperation operation2 in operations) {
            if (!Object.Equals(operation2.Operation, operation.Operation) && operation2.Name == operation.Name) {
              errorName = operation.Name;
              break;
            }
          }
          if (errorName == "") {
            foreach (StructOperation operation2 in previousOperations) {
              if (!Object.Equals(operation2.Operation, operation.Operation) && operation2.Name == operation.Name) {
                errorName = operation.Name;
                break;
              }
            }
          }
        }
      }
      if (errorName != "") {
        errors.Add("the name \"" + errorName + "\" is used for several operations");
      }

      // Different code for each operation (ignore empty codes)
      errorName = "";
      foreach (StructOperation operation in operations) {
        if (operation.Code != "" && errorName == "") {
          foreach (StructOperation operation2 in operations) {
            if (!Object.Equals(operation2.Operation, operation.Operation) && operation2.Code == operation.Code) {
              errorName = operation.Code;
              break;
            }
          }
          if (errorName == "") {
            foreach (StructOperation operation2 in previousOperations) {
              if (!Object.Equals(operation2.Operation, operation.Operation) && operation2.Code == operation.Code) {
                errorName = operation.Code;
                break;
              }
            }
          }
        }
      }
      if (errorName != "") {
        errors.Add("the code \"" + errorName + "\" is used for several operations");
      }

      // Verify that the operation codes are not already used in the database
      {
        IDictionary<IOperation, string> opCodes = new Dictionary<IOperation, string>();
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IList<IOperation> bddOperations = ModelDAOHelper.DAOFactory.OperationDAO.FindAll();
            foreach (IOperation operation in bddOperations) {
              if (!String.IsNullOrEmpty(operation.Code)) {
                opCodes[operation] = operation.Code;
              }
            }
          }
        }
        
        // We ignore the codes of all current operations
        foreach (StructOperation operation in previousOperations) {
          if (operation.Existing) {
            opCodes.Remove(operation.Operation);
          }
        }

        foreach (StructOperation operation in operations) {
          if (operation.Existing) {
            opCodes.Remove(operation.Operation);
          }
        }

        foreach (StructOperation operation in followingOperations) {
          if (operation.Existing) {
            opCodes.Remove(operation.Operation);
          }
        }

        foreach (StructOperation operation in operations) {
          if (!String.IsNullOrEmpty(operation.Code) && opCodes.Values.Contains(operation.Code)) {
            errors.Add("the code \"" + operation.Code + "\" is already used by another operation");
          }
        }
      }
      
      // If the last operation produces several iwps, the final workpiece should be one of them
      if (operations.Count > 0 && operations[operations.Count - 1].Existing) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IOperation operation = operations[operations.Count - 1].Operation;
            ModelDAOHelper.DAOFactory.OperationDAO.Lock(operation);
            if (operation.IntermediateWorkPieces.Count > 1) {
              IPart part = parts[m_currentPartIndex].Part;
              ModelDAOHelper.DAOFactory.PartDAO.Lock(part);
              if (part.FinalWorkPiece != null && !operation.IntermediateWorkPieces.Contains(part.Component.FinalWorkPiece)) {
                errors.Add("the last operation produces several kinds of parts and cannot be used at the end of the process");
              }
            }
          }
        }
      }
      
      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      data.Store(Item.CURRENT_PART_INDEX, data.Get<int>(Item.CURRENT_PART_INDEX) + 1);
      return null;
    }
    
    /// <summary>
    /// Get a summary of the user inputs, which will be displayed in a tree
    /// Each string in the list will be a rootnode. If line breaks are present, all lines
    /// after the first one will be shown as child nodes.
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      m_currentPartIndex = data.Get<int>(Item.CURRENT_PART_INDEX);
      if (m_currentPartIndex >= data.Get<List<StructPart>>(Item.PARTS).Count) {
        m_currentPartIndex = 0;
      }

      IList<string> summary = new List<string>();
      
      StructPart part = data.Get<List<StructPart>>(Item.PARTS)[m_currentPartIndex];
      IList<StructOperation> operations = part.Operations;
      if (operations.Count == 0) {
        summary.Add("no more operations for part \"" + part.PartName + "\"");
      }
      else {
        string text = "Operation" + (operations.Count > 1 ? "s" : "") + " of part \"" +
          part.PartName + "\"";
        foreach (StructOperation operation in operations) {
          text += "\n" + (operation.Existing ? "Existing" : "New") + " operation \"" +
            operation.Name;
          if (operation.Code == "") {
            text += "\"";
          }
          else {
            text += " (" + operation.Code + ")\"";
          }
        }
        summary.Add(text);
      }
      
      data.Store(Item.CURRENT_PART_INDEX, m_currentPartIndex + 1);
      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    void FillList(int selectedIndex)
    {
      listBox.ClearItems();
      foreach (StructOperation operation in m_operations) {
        Color color = operation.Existing ? Color.Blue : Color.Black;
        string text = operation.Name;
        if (operation.Code != "") {
          text += " (" + operation.Code + ")";
        }

        listBox.AddItem(text, operation, "", color, operation.Existing, false);
      }
      
      listBox.SelectedIndex = selectedIndex;
      ListBoxItemChanged("", "");
    }
    #endregion // Private methods
    
    #region Event reactions
    void ListBoxItemChanged(string arg1, object arg2)
    {
      int selectedIndex = listBox.SelectedIndex;
      
      // Enable controls
      bool isIndexSelected = (selectedIndex != -1);
      label6.Enabled = label2.Enabled = label5.Enabled = textOperationCode.Enabled =
        textOperationName.Enabled = numericParts.Enabled = isIndexSelected;
      buttonUp.Enabled = (selectedIndex > 0);
      buttonDown.Enabled = (isIndexSelected && selectedIndex < m_operations.Count - 1);
      buttonRemove.Enabled = isIndexSelected;
      
      // Fill values
      if (selectedIndex == -1) {
        textOperationName.Text = "";
        textOperationCode.Text = "";
        numericParts.Text = "1";
      } else {
        StructOperation currentOperation = listBox.SelectedValue as StructOperation;
        
        textOperationName.Text = currentOperation.Name;
        textOperationCode.Text = currentOperation.Code;
        numericParts.Text = currentOperation.m_maxPartsPerCycle.ToString();
        
        // Button remove disabled for operations linked to an existing part
        buttonRemove.Enabled = (currentOperation.m_part == null);
      }
    }
    
    void ButtonUpClick(object sender, EventArgs e)
    {
      int currentIndex = listBox.SelectedIndex;
      
      // Switch currentIndex and currentIndex - 1
      StructOperation operationTmp = m_operations[currentIndex];
      m_operations[currentIndex] = m_operations[currentIndex - 1];
      m_operations[currentIndex - 1] = operationTmp;
      
      FillList(currentIndex - 1);
    }
    
    void ButtonDownClick(object sender, EventArgs e)
    {
      int currentIndex = listBox.SelectedIndex;
      
      // Switch currentIndex and currentIndex + 1
      StructOperation operationTmp = m_operations[currentIndex];
      m_operations[currentIndex] = m_operations[currentIndex + 1];
      m_operations[currentIndex + 1] = operationTmp;
      
      FillList(currentIndex + 1);
    }
    
    void ButtonAddClick(object sender, EventArgs e)
    {
      m_operations.Add(new StructOperation("Operation " + (m_operations.Count + 1), ""));
      FillList(m_operations.Count - 1);
    }
    
    void ButtonRemoveClick(object sender, EventArgs e)
    {
      int currentIndex = listBox.SelectedIndex;
      m_operations.RemoveAt(currentIndex);
      if (currentIndex >= m_operations.Count) {
        currentIndex--;
      }

      FillList (currentIndex);
    }

    void TextOperationNameLeave(object sender, EventArgs e)
    {
      m_operations[listBox.SelectedIndex].Name = textOperationName.Text;
      FillList(listBox.SelectedIndex);
    }
    
    void TextOperationCodeLeave(object sender, EventArgs e)
    {
      m_operations[listBox.SelectedIndex].Code = textOperationCode.Text;
      FillList(listBox.SelectedIndex);
    }
    
    void NumericPartsLeave(object sender, EventArgs e)
    {
      m_operations[listBox.SelectedIndex].m_maxPartsPerCycle = (int)numericParts.Value;
    }
    #endregion // Event reactions
  }
}
