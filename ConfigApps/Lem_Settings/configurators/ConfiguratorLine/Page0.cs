// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of Page0.
  /// </summary>
  public partial class Page0 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Line selection"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose a line and click on a button to make modifications."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(ILine));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page0(ItemContext context)
    {
      InitializeComponent();
      if (context.UserCategory != LemSettingsGlobal.UserCategory.SUPER_ADMIN) {
        buttonDelete.Hide();
      }
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
      // Nothing here
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // List of lines
      listLines.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        IList<ILine> lines = ModelDAOHelper.DAOFactory.LineDAO.FindAll();
        foreach (ILine line in lines) {
          if (line.Components.Any()) {
            string text = line.Display + " (producing part" + (line.Components.Count > 1 ? "s \"" : " \"");
            IList<string> partNames = new List<string>();
            foreach (Lemoine.Model.IComponent component in line.Components) {
              partNames.Add(component.Display);
            }

            text += String.Join("\", \"", partNames.ToArray()) + "\")";
            listLines.AddItem(text, line);
          }
        }
      }
      
      // Select a line
      if (data.Get<ILine>(Item.LINE) == null) {
        listLines.SelectedIndex = 0;
      }
      else {
        listLines.SelectedValue = data.Get<ILine>(Item.LINE);
      }

      // Reset LineData
      data.Get<LineData>(Item.LINE_DATA).LoadLine(null);
      data.Store(Item.OPERATION, null);
      data.Store(Item.MACHINE, null);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      if (listLines.SelectedValue == null) {
        data.Store(Item.LINE, null);
      }
      else {
        data.Store(Item.LINE, (ILine)listLines.SelectedValue);
      }
    }
    #endregion // Page methods
    
    #region Event reactions
    void ButtonChangeMachinesClick(object sender, EventArgs e)
    {
      if (listLines.SelectedValue != null) {
        EmitDisplayPageEvent ("Page1", null);
      }
      else {
        WarnNoLine ();
      }
    }
    
    void ButtonChangePropertiesClick(object sender, EventArgs e)
    {
      if (listLines.SelectedValue != null) {
        EmitDisplayPageEvent ("Page2", null);
      }
      else {
        WarnNoLine ();
      }
    }
    
    void ButtonDeleteClick(object sender, EventArgs e)
    {
      if (listLines.SelectedValue != null) {
        EmitDisplayPageEvent ("Page4", null);
      }
      else {
        WarnNoLine ();
      }
    }
    #endregion // Event reactions
    
    void WarnNoLine()
    {
      MessageBoxCentered.Show("Please select a line first.", "Warning",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
  }
}
