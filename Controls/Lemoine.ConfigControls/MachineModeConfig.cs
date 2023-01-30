// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Iesi.Collections.Generic;
using Lemoine.BaseControls;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Configuration panel for the machine modes
  /// </summary>
  public partial class MachineModeConfig
    : UserControl
    , IConfigControlObservable<IMachineMode>
  {
    #region Members
    SortableBindingList<IMachineMode> m_machineModes = new SortableBindingList<IMachineMode>();
    
    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IMachineMode> m_deleteList =
      new List<IMachineMode> ();

    ISet<IConfigControlObserver<IMachineMode> > m_observers =
      new HashSet<IConfigControlObserver<IMachineMode> > ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineModeConfig()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("MachineMode");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      translationKeyColumn.HeaderText = PulseCatalog.GetString ("TranslationKey");
      parentColumn.HeaderText = PulseCatalog.GetString ("Parent");
      runningColumn.HeaderText = PulseCatalog.GetString ("MachineModeRunning");
      autoColumn.HeaderText = PulseCatalog.GetString ("MachineModeAuto");
      manualColumn.HeaderText = PulseCatalog.GetString ("MachineModeManual");
      autoSequenceColumn.HeaderText = PulseCatalog.GetString ("MachineModeAutoSequence");
      colorColumn.HeaderText = PulseCatalog.GetString ("Color");
      machineModeCategoryColumn.HeaderText = PulseCatalog.GetString ("MachineModeCategory");

      m_machineModes.SortColumns = false;
      
      {
        MachineModeDialog dialog =
          new MachineModeDialog ();
        dialog.Nullable = true;
        dialog.DisplayedProperty = "SelectionText";
        DataGridViewCell cell = new DataGridViewSelectionableCell<IMachineMode> (dialog);
        parentColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors
    
    void MachineModeConfigLoad(object sender, EventArgs e)
    {
      MachineModeConfigLoad ();
    }
    
    void MachineModeConfigLoad()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("MachineModeConfigLoad: " +
                         "no DAO factory is defined");
        return;
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        IList<IMachineMode> machineModes =
          daoFactory.MachineModeDAO.FindAll ();

        m_machineModes.Clear ();
        foreach(IMachineMode machineMode in machineModes) {
          m_machineModes.Add(machineMode);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows
        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_machineModes;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler(BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }
    
    void MachineModeConfigValidated(object sender, EventArgs e)
    {
      CommitChanges ();
    }
    
    void CommitChanges ()
    {
      if ( (0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }
      
      IList<IMachineMode> updateList = new List<IMachineMode> ();
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        foreach (DataGridViewRow row in m_updateSet) {
          IMachineMode machineMode = row.DataBoundItem as IMachineMode;
          if (null == machineMode) {
            continue; // The row may have been deleted since
          }
          daoFactory.MachineModeDAO.MakePersistent (machineMode);
          updateList.Add (machineMode);
        }

        foreach (IMachineMode machineMode in m_deleteList) {
          daoFactory.MachineModeDAO.MakeTransient (machineMode);
        }
        
        transaction.Commit ();
      }
            
      Lemoine.WebClient.Request.NotifyConfigUpdate ();
      
      NotifyUpdate (updateList);
      NotifyDelete (m_deleteList);
      
      m_updateSet.Clear ();
      m_deleteList.Clear ();
    }

    void DataGridViewUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      IMachineMode machineMode =
        e.Row.DataBoundItem
        as IMachineMode;
      if (null != machineMode) {
        if (machineMode.Id < 100) {
          e.Cancel = true;
        }
        else { // Id >= 100
          m_updateSet.Remove (e.Row);
          m_deleteList.Add (machineMode);
        }
      }
    }
    
    void DataGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows [e.RowIndex];
        IMachineMode machineMode =
          row.DataBoundItem
          as IMachineMode;
        if (null != machineMode) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateMachineModeFromName ("", false);
    }
    
    void DataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if(this.dataGridView.Columns[e.ColumnIndex].Name == "colorColumn") {
        if(e.Value != null){
          e.CellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml(e.Value.ToString());
          e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
          e.CellStyle.SelectionForeColor = Color.Black;
        }
      }
    }
    
    void DataGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        if(this.dataGridView.Columns[e.ColumnIndex].Name == "colorColumn"
           || this.dataGridView.Columns[e.ColumnIndex].Name == "reportColorColumn") {
          ColorDialog colorDialog = new ColorDialog();
          DataGridViewCell selectedCell = this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
          string cellValue = (string)selectedCell.Value;
          if(!String.IsNullOrEmpty(cellValue)){
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(cellValue);
          }
          DialogResult dialogResult = colorDialog.ShowDialog();
          Color selectedColor = Color.White;
          switch(dialogResult){
            case DialogResult.OK:
              {
                selectedColor = colorDialog.Color;
                break;
              }
            case DialogResult.Cancel:
              {
                if(!String.IsNullOrEmpty(cellValue)){
                  selectedColor = System.Drawing.ColorTranslator.FromHtml(cellValue);
                }
                break;
              }
            default:
              {
                selectedColor = Color.White;
                break;
              }
          }
          selectedCell.Style.BackColor = selectedColor;
          selectedCell.Value = "#" + selectedColor.R.ToString("X2") + selectedColor.G.ToString("X2") + selectedColor.B.ToString("X2");
          this.dataGridView.RefreshEdit();
        }
      }
    }

    #region IConfigControlObservable implementation
    /// <summary>
    /// Add an observer to this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void AddObserver (IConfigControlObserver<IMachineMode> observer)
    {
      m_observers.Add (observer);
    }
    
    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IMachineMode> observer)
    {
      m_observers.Remove (observer);
    }
    
    /// <summary>
    /// Notify the observers after an update
    /// </summary>
    void NotifyUpdate (IList<IMachineMode> updatedEntities)
    {
      foreach (IConfigControlObserver<IMachineMode> observer in m_observers)
      {
        observer.UpdateAfterUpdate (updatedEntities);
      }
    }

    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IMachineMode> deletedEntities)
    {
      foreach (IConfigControlObserver<IMachineMode> observer in m_observers)
      {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}
