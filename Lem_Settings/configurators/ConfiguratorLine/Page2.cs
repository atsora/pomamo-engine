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

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  public partial class Page2 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    PartData m_partData = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Change line and part properties"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "The name and code of a line and its produced part may be changed here."; } }
    
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
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IPart));
        types.Add(typeof(IComponent));
        return types;
      }
    }
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
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      ILine line = data.Get<ILine>(Item.LINE);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          textLineName.Text = line.Name;
          textLineCode.Text = line.Code;
        }
      }
      
      m_partData = data.Get<PartData>(Item.PARTS);
      m_partData.Load(line);
      FillParts();
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.LINE_NAME, textLineName.Text);
      data.Store(Item.LINE_CODE, textLineCode.Text);
      
      // Parts automatically stored
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
      
      // Code already taken by another line
      string lineCode = data.Get<string>(Item.LINE_CODE);
      if (lineCode != "") {
        ILine currentLine = data.Get<ILine>(Item.LINE);
        bool ok = true;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            ModelDAOHelper.DAOFactory.LineDAO.Lock(currentLine);
            IList<ILine> lines = ModelDAOHelper.DAOFactory.LineDAO.FindAll();
            foreach (ILine line in lines) {
              if (line != currentLine && line.Code == lineCode) {
                ok = false;
              }
            }
          }
        }
        if (!ok) {
          errors.Add("the code \"" + lineCode + "\" is already taken by another line");
        }
      }
      
      // Line name cannot be empty
      if (data.Get<string>(Item.LINE_NAME) == "") {
        errors.Add("the line name cannot be empty");
      }

      // Errors coming from the name / code of the parts
      data.Get<PartData>(Item.PARTS).GetErrors(ref errors);
      
      return errors;
    }
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      data.Get<PartData>(Item.PARTS).Save();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          ILine line = data.Get<ILine>(Item.LINE);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          line.Name = data.Get<string>(Item.LINE_NAME);
          line.Code = data.Get<string>(Item.LINE_CODE);
          transaction.Commit();
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
    
    #region Event reactions
    void NameChanged(int index, string name)
    {
      m_partData.SetName(index, name);
    }
    
    void CodeChanged(int index, string code)
    {
      m_partData.SetCode(index, code);
    }
    #endregion // Event reactions
    
    #region Private methods
    void FillParts()
    {
      // Clear the verticalScroll layout
      verticalScroll.Clear();
      
      // Add an item for each part
      for (int i = 0; i < m_partData.Count; i++) {
        PartCell cell = new PartCell(i, m_partData.GetName(i), m_partData.GetCode(i));
        cell.Dock = DockStyle.Fill;
        verticalScroll.AddControl(cell);
        cell.CodeChanged += CodeChanged;
        cell.NameChanged += NameChanged;
      }
    }
    #endregion // Private methods
  }
}
