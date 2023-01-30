// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateLine
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericWizardPage, IWizardPage
  {
    #region Members
    string m_lineName = "";
    string m_lineCode = "";
    IList<StructPart> m_parts = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Part(s) produced"; } }
    
    /// <summary>
    /// Some help to fill the page
    /// </summary>
    public string Help { get { return "A line is intended to produce one or several part(s). " +
          "Add here the different parts the line will produce, which can be existing parts or new parts.\n\n" +
          "At least one part must be present in the list.\n\n" +
          "Note: a new part can share the same name and code than the associated line, provided the code is available."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes{
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IPart));
        types.Add(typeof(Lemoine.Model.IComponent));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
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
      // Default name and code for a new part
      m_lineName = data.Get<string>(Item.LINE_NAME);
      m_lineCode = data.Get<string>(Item.LINE_CODE);
      
      // Fill with parts
      m_parts = data.Get<List<StructPart>>(Item.PARTS);
      FillList(null);
    }
    
    /// <summary>
    /// Save the parameters
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Automatically stored
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return "Page2";
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<List<StructPart>>(Item.PARTS).Count == 0) {
        errors.Add("the line must produce at least one part");
      }
      else {
        // Name cannot be empty
        IList<StructPart> structParts = data.Get<List<StructPart>>(Item.PARTS);
        foreach (StructPart part in structParts) {
          if (part.PartName == "") {
            errors.Add("a name must be specified for each part");
            break;
          }
        }
        
        // Different names for each part
        IList<string> listTmp = new List<string>();
        foreach (StructPart part in structParts) {
          if (part.PartName != "") {
            if (listTmp.Contains(part.PartName)) {
              errors.Add("parts must have different names");
              break;
            } else {
              listTmp.Add(part.PartName);
            }
          }
        }
        
        // Part codes must be unique
        IDictionary<IPart, string> partCodes = new Dictionary<IPart, string>();
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IList<IPart> parts = ModelDAOHelper.DAOFactory.PartDAO.FindAll();
            foreach (IPart part in parts) {
              if (!String.IsNullOrEmpty(part.Code)) {
                partCodes[part] = part.Code;
              }
            }
          }
        }
        
        foreach (StructPart structPart in structParts) {
          if (!String.IsNullOrEmpty(structPart.PartCode)) {
            bool alreadyTaken = false;
            foreach (IPart part in partCodes.Keys) {
              if ((!structPart.Existing || !Object.Equals(part, structPart.Part)) &&
                  partCodes[part] == structPart.PartCode) {
                alreadyTaken = true;
                break;
              }
            }
            
            // Update
            partCodes[structPart.Part] = structPart.PartCode;
            
            if (alreadyTaken) {
              errors.Add("the code \"" + structPart.PartCode + "\" is already used by another part");
              break;
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
      
      // Warning if an operation of a part is shared by another part
      IList<string> warnings = new List<string>();
      IList<StructPart> parts = data.Get<List<StructPart>>(Item.PARTS);
      foreach (StructPart structPart in parts) {
        if (structPart.Existing) {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
              IPart part = structPart.Part;
              ModelDAOHelper.DAOFactory.PartDAO.Lock(part);
              bool ok = true;
              foreach (IComponentIntermediateWorkPiece ciwp in part.ComponentIntermediateWorkPieces) {
                ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.Lock(ciwp);
                IIntermediateWorkPiece iwp = ciwp.IntermediateWorkPiece;
                ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock(iwp);
                if (iwp != null && iwp.PossibleNextOperations != null) {
                  foreach (IOperation op in iwp.PossibleNextOperations) {
                    bool okTmp = false;
                    
                    // Within the current part?
                    foreach (IComponentIntermediateWorkPiece ciwpTmp in part.ComponentIntermediateWorkPieces) {
                      if (ciwpTmp.IntermediateWorkPiece != null && ciwpTmp.IntermediateWorkPiece.Operation != null &&
                          Object.Equals(ciwpTmp.IntermediateWorkPiece.Operation, op)) {
                        okTmp = true;
                        break;
                      }
                    }
                    
                    ok &= okTmp;
                  }
                }
              }
              if (!ok) {
                warnings.Add(String.Format("Part \"{0}\" comprises an operation shared by another part, " +
                                           "which may be broken after the wizard.", part.Display));
              }
            }
          }
        }
      }
      
      return warnings;
    }
    
    /// <summary>
    /// Get a summary of the user inputs, which will be displayed in a tree
    /// Each string in the list will be a rootnode. If line breaks are present, all lines
    /// after the first one will be shown as child nodes.
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      IList<StructPart> structParts = data.Get<List<StructPart>>(Item.PARTS);
      foreach (StructPart part in structParts) {
        summary.Add(part.ToString());
      }

      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    void FillList(StructPart selectedPart)
    {
      listParts.ClearItems();
      foreach (StructPart part in m_parts) {
        Color color = part.Existing ? Color.Blue : Color.Black;
        listParts.AddItem(part.ToString(), part, part.ToOrder(), color, part.Existing, false);
      }
      
      if (selectedPart == null) {
        listParts.SelectedIndex = 0;
      }
      else {
        listParts.SelectedValue = selectedPart;
      }

      ListPartsItemChanged ("", "");
    }
    #endregion // Private methods
    
    #region Event reactions
    void ListPartsItemChanged(string arg1, object arg2)
    {
      StructPart part = listParts.SelectedValue as StructPart;
      buttonRemove.Enabled = (part != null);
      
      label1.Enabled = label2.Enabled = textName.Enabled = textCode.Enabled =
        (part != null) && !part.Existing;
      if (part != null) {
        textName.Text = part.PartName;
        textCode.Text = part.PartCode;
      }
    }
    
    void ButtonAddClick(object sender, EventArgs e)
    {
      DialogNewPart dialog = new DialogNewPart(m_lineName, m_lineCode);
      dialog.ShowDialog(this);
      if (dialog.DialogResult == DialogResult.OK) {
        StructPart part;
        if (dialog.Existing) {
          bool alreadyPresent = false;
          foreach (StructPart part2 in m_parts) {
            if (part2.Existing && Object.Equals(part2.Part, dialog.Part)) {
              alreadyPresent = true;
              break;
            }
          }
          if (alreadyPresent) {
            MessageBoxCentered.Show("This part is already in the list.", "Warning",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
          } else {
            part = new StructPart(dialog.Part);
          }
        } else {
          part = new StructPart(dialog.PartName, dialog.PartCode);
        }

        m_parts.Add(part);
        FillList(part);
      }
    }
    
    void ButtonRemoveClick(object sender, EventArgs e)
    {
      StructPart part = listParts.SelectedValue as StructPart;
      int index = listParts.SelectedIndex;
      StructPart nextPart = null;
      if (index + 1 < listParts.Count) {
        nextPart = listParts.Values[index + 1] as StructPart;
      }
      else if (index > 0) {
        nextPart = listParts.Values[index - 1] as StructPart;
      }

      if (part != null && index >= 0 && index < m_parts.Count) {
        m_parts.Remove(part);
        FillList(nextPart);
      }
    }
    
    void TextNameLeave(object sender, EventArgs e)
    {
      StructPart part = listParts.SelectedValue as StructPart;
      if (part != null) {
        part.PartName = textName.Text;
        FillList(part);
      }
    }
    
    void TextCodeLeave(object sender, EventArgs e)
    {
      StructPart part = listParts.SelectedValue as StructPart;
      if (part != null) {
        part.PartCode = textCode.Text;
        FillList(part);
      }
    }
    #endregion // Event reactions
  }
}
