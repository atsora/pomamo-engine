// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a translation key
  /// </summary>
  public partial class TranslationKeySelection : UserControl
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TranslationKeySelection).FullName);
    
    #region Members
    String m_selectedTranslationKey = null;
    ICatalog m_catalog = new CachedCatalog (new Lemoine.ModelDAO.i18n.ModelDAOCatalog ());
    #endregion

    #region Getters / Setters
    /// <summary>
    /// Is a null TranslationKey a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(true), Description("Is a null translation key valid ?")]
    public bool Nullable {
      get { return nullCheckBox.Visible; }
      set
      {
        if (value) {
          tableLayoutPanel1.RowStyles [1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles [1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }
    
    /// <summary>
    /// Selected TranslationKey
    /// </summary>
    public string SelectedTranslationKey {
      get
      {
        Debug.Assert (dataGridView.SelectedRows.Count <= 1);
        if (nullCheckBox.Checked) {
          return null;
        }
        else {
          if (0 < dataGridView.SelectedRows.Count) {
            return (dataGridView.SelectedRows [0]
                    .DataBoundItem as TranslationDisplay).Key;
          }
          else {
            return null;
          }
        }
      }
      set {
        this.m_selectedTranslationKey = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TranslationKeySelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nullCheckBox.Text = PulseCatalog.GetString ("TranslationKeyNull");
    }
    #endregion // Constructors

    #region Methods
    private void SetSelectedTranslationKey(){
      if(!String.IsNullOrEmpty(this.m_selectedTranslationKey)){
        foreach(DataGridViewRow row in this.dataGridView.Rows){
          TranslationDisplay translationDisplay = row.DataBoundItem as TranslationDisplay;
          if(translationDisplay != null & translationDisplay.Key.Equals(this.m_selectedTranslationKey)){
            row.Selected = true;
          }
        }
      }
    }
    #endregion // Methods
    
    void TranslationKeySelectionLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      IList<string> translationKeys;
      using (IDAOSession session = daoFactory.OpenSession ())
      {
        translationKeys =
          daoFactory.TranslationDAO.GetDistinctTranslationKeys ();
      }
      
      IList<TranslationDisplay> translationDisplays =
        new List<TranslationDisplay> ();
      foreach (string translationKey in translationKeys) {
        translationDisplays.Add (new TranslationDisplay (translationKey, m_catalog));
      }
      
      dataGridView.DataSource =
        translationDisplays;
      
      foreach (DataGridViewColumn column in dataGridView.Columns) {
        if (column.DataPropertyName.Equals ("Key")) {
          column.ReadOnly = true;
          column.HeaderText = PulseCatalog.GetString ("TranslationKey");
          column.DisplayIndex = 0;
          column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }
        else if (column.DataPropertyName.Equals ("Display")) {
          column.ReadOnly = true;
          column.HeaderText = PulseCatalog.GetString ("Display");
          column.DisplayIndex = 1;
          column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }
        else {
          column.Visible = false;
        }
      }
      
      this.SetSelectedTranslationKey();
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        dataGridView.Enabled = false;
      }
      else {
        dataGridView.Enabled = true;
      }
    }
  }
  
  
  
  
  /// <summary>
  /// Translation display
  /// </summary>
  public class TranslationDisplay
  {
    #region Members
    readonly string m_key;
    readonly ICatalog m_catalog;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (TranslationDisplay).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    /// <param name="catalog"></param>
    public TranslationDisplay(string key, ICatalog catalog)
    {
      m_key = key;
      m_catalog = catalog;
    }
    
    /// <summary>
    /// Translation key
    /// </summary>
    public string Key {
      get { return m_key; }
    }
    
    /// <summary>
    /// Display that corresponds to the given key,
    /// taking automatically the right locale
    /// </summary>
    public string Display {
      get { return m_catalog.GetString (m_key, LocaleSettings.CurrentCulture); }
    }
  }
}
