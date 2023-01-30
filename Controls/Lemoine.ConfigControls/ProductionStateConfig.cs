// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.DataReferenceControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of ProductionStateConfig.
  /// </summary>
  public partial class ProductionStateConfig : UserControl, IConfigControlObservable<IProductionState>
  {
    #region Members
    SortableBindingList<IProductionState> m_productionStates
      = new SortableBindingList<IProductionState> ();

    ISet<DataGridViewRow> m_updateSet =
      new HashSet<DataGridViewRow> ();
    IList<IProductionState> m_deleteList =
      new List<IProductionState> ();

    ISet<IConfigControlObserver<IProductionState>> m_observers =
      new HashSet<IConfigControlObserver<IProductionState>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ProductionStateConfig).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ProductionStateConfig ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      dataGridView.TopLeftHeaderCell.Value = PulseCatalog.GetString ("ProductionState");

      idColumn.HeaderText = PulseCatalog.GetString ("Id");
      nameColumn.HeaderText = PulseCatalog.GetString ("Name");
      translationKeyColumn.HeaderText = PulseCatalog.GetString ("TranslationKey");
      descriptionColumn.HeaderText = PulseCatalog.GetString ("Description");
      descriptionTranslationKeyColumn.HeaderText = PulseCatalog.GetString ("DescriptionTranslationKey");
      colorColumn.HeaderText = PulseCatalog.GetString ("Color");
      displayPriorityColumn.HeaderText = PulseCatalog.GetString ("DisplayPriority");

      m_productionStates.SortColumns = false;

      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string> (dialog);
        translationKeyColumn.CellTemplate = cell;
      }
      {
        TranslationKeyDialog dialog =
          new TranslationKeyDialog ();
        DataGridViewCell cell = new DataGridViewSelectionableCell<string> (dialog);
        descriptionTranslationKeyColumn.CellTemplate = cell;
      }
    }
    #endregion // Constructors

    void ProductionStateConfigLoad (object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("ProductionStateConfigLoad: " +
                        "no DAO factory is defined");
        return;
      }

      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        IList<IProductionState> productionStates = daoFactory.ProductionStateDAO.FindAll ();

        m_productionStates.Clear ();
        foreach (IProductionState productionState in productionStates) {
          m_productionStates.Add (productionState);
        }

        // Note: the use of a bindingSource is necessary to
        //       add some new rows

        BindingSource bindingSource = new BindingSource ();
        bindingSource.DataSource = m_productionStates;
        bindingSource.AllowNew = true;
        bindingSource.AddingNew += new AddingNewEventHandler (BindingSourceAddingNew);
        dataGridView.DataSource = bindingSource;
      }
    }

    void ProductionStateConfigValidated (object sender, EventArgs e)
    {
      CommitChanges ();
    }

    void CommitChanges ()
    {
      if ((0 == m_updateSet.Count) && (0 == m_deleteList.Count)) {
        return;
      }

      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession session = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        foreach (DataGridViewRow row in m_updateSet) {
          IProductionState productionState = row.DataBoundItem as IProductionState;
          if (null == productionState) {
            continue; // The row may have been deleted since
          }
          daoFactory.ProductionStateDAO.MakePersistent (productionState);
        }

        foreach (IProductionState productionState in m_deleteList) {
          daoFactory.ProductionStateDAO.MakeTransient (productionState);
        }

        transaction.Commit ();

        NotifyDelete (m_deleteList);

        m_updateSet.Clear ();
        m_deleteList.Clear ();
      }

      Lemoine.WebClient.Request.NotifyConfigUpdate ();
    }

    void DataGridViewUserDeletingRow (object sender, DataGridViewRowCancelEventArgs e)
    {

      IProductionState productionState =
        e.Row.DataBoundItem
        as IProductionState;
      if (null != productionState) {
        m_updateSet.Remove (e.Row);
        m_deleteList.Add (productionState);
      }
    }

    void DataGridViewUserAddedRow (object sender, DataGridViewRowEventArgs e)
    {
      IProductionState productionState =
        e.Row.DataBoundItem
        as IProductionState;
      if (null != productionState) {
        m_updateSet.Add (e.Row);
      }
    }

    void DataGridViewCellValueChanged (object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        DataGridViewRow row = dataGridView.Rows[e.RowIndex];
        IProductionState productionState =
          row.DataBoundItem
          as IProductionState;
        if (null != productionState) {
          m_updateSet.Add (row);
        }
      }
    }

    void BindingSourceAddingNew (object sender, AddingNewEventArgs e)
    {
      e.NewObject = ModelDAOHelper.ModelFactory.CreateProductionState ("#ffffff");
    }

    void DataGridViewCellFormatting (object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (this.dataGridView.Columns[e.ColumnIndex].Name == "colorColumn") {
        if (e.Value != null) {
          e.CellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml (e.Value.ToString ());
          e.CellStyle.SelectionBackColor = e.CellStyle.BackColor;
          e.CellStyle.SelectionForeColor = Color.Black;
        }
      }
    }

    void DataGridViewCellDoubleClick (object sender, DataGridViewCellEventArgs e)
    {
      if (0 <= e.RowIndex) {
        if (this.dataGridView.Columns[e.ColumnIndex].Name == "colorColumn") {
          ColorDialog colorDialog = new ColorDialog ();
          DataGridViewCell selectedCell = this.dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
          string cellValue = (string)selectedCell.Value;
          if (!String.IsNullOrEmpty (cellValue)) {
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml (cellValue);
          }
          DialogResult dialogResult = colorDialog.ShowDialog ();
          Color selectedColor = Color.White;
          switch (dialogResult) {
          case DialogResult.OK: {
            selectedColor = colorDialog.Color;
            break;
          }
          case DialogResult.Cancel: {
            if (!String.IsNullOrEmpty (cellValue)) {
              selectedColor = System.Drawing.ColorTranslator.FromHtml (cellValue);
            }
            break;
          }
          default: {
            selectedColor = Color.White;
            break;
          }
          }
          selectedCell.Style.BackColor = selectedColor;
          selectedCell.Value = "#" + selectedColor.R.ToString ("X2") + selectedColor.G.ToString ("X2") + selectedColor.B.ToString ("X2");
          this.dataGridView.RefreshEdit ();
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
    public void AddObserver (IConfigControlObserver<IProductionState> observer)
    {
      m_observers.Add (observer);
    }

    /// <summary>
    /// Remove an observer from this control
    /// 
    /// This is the implementation of IConfigControlObservable
    /// </summary>
    /// <param name="observer"></param>
    public void RemoveObserver (IConfigControlObserver<IProductionState> observer)
    {
      m_observers.Remove (observer);
    }

    /// <summary>
    /// Notify the observers after a delete
    /// </summary>
    void NotifyDelete (IList<IProductionState> deletedEntities)
    {
      foreach (IConfigControlObserver<IProductionState> observer in m_observers) {
        observer.UpdateAfterDelete (deletedEntities);
      }
    }
    #endregion // IConfigControlObservable implementation
  }
}
