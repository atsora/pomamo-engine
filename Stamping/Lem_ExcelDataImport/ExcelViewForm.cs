// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.ComponentModel;
#if DEBUG
using System.Runtime.InteropServices;
#endif

using NHibernate;
using Lemoine.Core.Log;

using Lemoine.GDBPersistentClasses;
using Lemoine.BaseControls;
using Lemoine.JobControls;
using Lemoine.ExcelDataGrid;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.I18N;
using Lemoine.Business.Config;
using Lemoine.Extensions;
using Lemoine.FileRepository;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.Info.ConfigReader.TargetSpecific;
using Lemoine.Core.Hosting;
using System.Diagnostics;

namespace Lem_ExcelDataImport
{
  /// <summary>
  /// Description of ExcelViewForm.
  /// </summary>
  public partial class ExcelViewForm : Form, IDataGridModifierObserver
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ExcelViewForm).FullName);

#if DEBUG
    /// <summary>
    /// console for debug purpose
    /// </summary>
    /// <returns></returns>
    [DllImport ("kernel32.dll")]
    static extern bool AllocConsole ();
    #endif
    
    #region Members
    static readonly string CONFIGURATION_FILES_KEY = "ConfigurationFiles";
    static readonly string DEFAULT_CONFIGURATION_FILE_KEY = "DefaultConfigurationFile";
    static readonly string SEARCH_DIRECTORY = "SearchDirectory";
    static readonly string SAVE_DIRECTORY = "SaveDirectory";
    static readonly string MACRO_DIRECTORY = "MacroDirectory";
    static readonly string SEQUENCE_REGION_NAME = "SEQUENCE";

    bool m_backgroundWorkerDone = true;
    DataGridModifier m_gridModifier;
    bool m_wasModifiedSinceLastSave = false;
    #endregion // Members
    
    #region Getters / Setters
    
    /// <summary>
    /// Grid modifier used for tracking grid formatting
    /// </summary>
    DataGridModifier GridModifier {
      get { return m_gridModifier; }
      set { m_gridModifier = value; }
    }
    
    /// <summary>
    /// BackgroundWorker for importing an Excel file into the Database
    /// </summary>
    public bool BackGroundWorkerDone {
      get { return m_backgroundWorkerDone; }
      private set { m_backgroundWorkerDone = value; }
    }
    
    /// <summary>
    /// Was the grid modified since last save ?
    /// </summary>
    bool WasModifiedSinceLastSave {
      get { return m_wasModifiedSinceLastSave; }
      set { m_wasModifiedSinceLastSave = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ExcelViewForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Methods
    
    #region Contextual Menus
    /// <summary>
    /// Contextual Menu on right button mouse click on cell
    /// </summary>
    private void dataGridView1_MouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right) {
        if ((e.RowIndex >= 0) && (e.ColumnIndex >= 0)) {
          int selectedCellCount = dataGridView1.GetCellCount(DataGridViewElementStates.Selected);
          if (selectedCellCount > 0) {
            
            ToolStripMenuItem selectSequenceItem = new ToolStripMenuItem ("Select Sequence Region");
            selectSequenceItem.Click +=
              delegate(object s, EventArgs args)
            { SelectSequenceToolStripMenuItemClick(s,args); };

            ToolStripMenuItem inputSelectionItem = new ToolStripMenuItem ("Input");
            inputSelectionItem.Click +=
              delegate(object s, EventArgs args)
            { SetCellValueToolStripMenuItemClick(s,args); };

            ToolStripMenuItem pruneRowColItem = new ToolStripMenuItem ("Prune out");
            pruneRowColItem.Click +=
              delegate(object s, EventArgs args)
            { PruneStripMenuItemClick(s,args); };
            
            ContextMenuStrip contextMenu = new ContextMenuStrip ();
            contextMenu.Items.Add(selectSequenceItem);
            contextMenu.Items.Add(inputSelectionItem);
            contextMenu.Items.Add(pruneRowColItem);
            
            if (GridModifier.SomethingToUndo()) {
              ToolStripMenuItem undoItem = new ToolStripMenuItem ("Undo");
              undoItem.Click +=
                delegate(object s, EventArgs args) { GridModifier.Undo(); };
              contextMenu.Items.Add(undoItem);
              ToolStripMenuItem undoAllItem = new ToolStripMenuItem ("UndoAll");
              undoAllItem.Click +=
                delegate(object s, EventArgs args) { GridModifier.UndoAll(); };
              contextMenu.Items.Add(undoItem);
              contextMenu.Items.Add(undoAllItem);
            }
            contextMenu.Show(this.dataGridView1, this.dataGridView1.PointToClient(Cursor.Position));
          }
        }
      }
    }
    
    /// <summary>
    /// contextual menu on right button mouse click on column header
    /// </summary>
    private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        DataGridViewColumn selectedColumn = dataGridView1.Columns[e.ColumnIndex];
        ContextMenuStrip contextMenu = new ContextMenuStrip();
        ToolStripMenuItem insertColumn = new ToolStripMenuItem ("Insert Column");
        insertColumn.Click += delegate(object s, EventArgs args) {
          WasModifiedSinceLastSave = true;
          GridModifier.InsertColumn(e.ColumnIndex); };
        ToolStripMenuItem deleteColumn = new ToolStripMenuItem ("Delete Column");
        deleteColumn.Click += delegate(object s, EventArgs args) {
          WasModifiedSinceLastSave = true;
          GridModifier.DeleteColumn(e.ColumnIndex); };
        ToolStripMenuItem shiftUpColumn = new ToolStripMenuItem ("Shift up");
        shiftUpColumn.Click += delegate(object s, EventArgs args) {
          WasModifiedSinceLastSave = true;
          GridModifier.ShiftUpColumn(e.ColumnIndex); };
        ToolStripMenuItem shiftDownColumn = new ToolStripMenuItem ("Shift down");
        shiftDownColumn.Click += delegate(object s, EventArgs args) {
          WasModifiedSinceLastSave = true;
          GridModifier.ShiftDownColumn(e.ColumnIndex); };
        contextMenu.Items.Add(insertColumn);
        contextMenu.Items.Add(deleteColumn);
        contextMenu.Items.Add(shiftUpColumn);
        contextMenu.Items.Add(shiftDownColumn);
        contextMenu.Show(this.dataGridView1, this.dataGridView1.PointToClient(Cursor.Position));
      }
    }

    /// <summary>
    /// contextual menu on right button mouse click on row header
    /// </summary>
    private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right)
      {
        DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
        ContextMenuStrip contextMenu = new ContextMenuStrip();
        ToolStripMenuItem insertRow = new ToolStripMenuItem ("Insert Row");
        insertRow.Click += delegate(object s, EventArgs args) {
          WasModifiedSinceLastSave = true;
          GridModifier.InsertRow(e.RowIndex);
        };
        contextMenu.Items.Add(insertRow);
        contextMenu.Show(this.dataGridView1, this.dataGridView1.PointToClient(Cursor.Position));
      }
    }

    #endregion // Contextual
    
    #region Grid modifications and selections
    /// <summary>
    /// called on user row deletion in order to register it as a modification event
    /// </summary>
    private void dataGridView1_DeleteRow(object sender, DataGridViewRowCancelEventArgs e)
    {
      WasModifiedSinceLastSave = true;
      GridModifier.DeleteRow(e.Row.Index, false); // no actual row deletion
    }
    
    
    /// <summary>
    /// Choose and load Excel file
    /// </summary>
    void LoadExcelToolStripMenuItemClick(object sender, EventArgs e)
    {
      OpenFileDialog dialog = new OpenFileDialog();
      dialog.InitialDirectory = Lemoine.Info.OptionsFile.GetOption (SEARCH_DIRECTORY);
      dialog.Filter = "Excel files|*.xls";
      dialog.Title = "Select Excel file";
      if (dialog.ShowDialog() == DialogResult.OK) {
        string sourceFileName = dialog.FileName.ToString();
        // OK since sheetNumberTextBox's content format is controlled to be an int
        int sheetNumber = int.Parse(this.sheetNumberTextBox.Text.ToString());
        IList<GridSelectionEvent> regions =
          ExcelFileDataGrid.ImportFileAsGrid(sourceFileName, dataGridView1, sheetNumber);
        if (regions != null) {
          GridModifier = new DataGridModifier();
          GridModifier.GridView = dataGridView1;
          GridModifier.AddInvalidateRegionObserver(this);
          GridModifier.LoadRegions(regions, new string[1]{ SEQUENCE_REGION_NAME });
          this.updateSequenceSelectionTextbox();
          // now possible to save Excel file and to load a macro
          this.saveExcelToolStripMenuItem.Enabled = true;
          this.loadMacroToolStripMenuItem.Enabled = true;
          this.excelFileNameTextBox.Text = sourceFileName;
          WasModifiedSinceLastSave = false;
        }
      }
    }
    
    /// <summary>
    /// Save current grid as Excel File
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SaveExcelToolStripMenuItemClick(object sender, EventArgs e)
    {
      if (GridModifier == null) {
        return;
      }

      FileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.InitialDirectory =
        Lemoine.Info.OptionsFile.GetOption (SAVE_DIRECTORY);
      saveFileDialog.Filter = "Excel files|*.xls";
      saveFileDialog.Title = "Save Grid as Excel File";
      try {
        if (saveFileDialog.ShowDialog() == DialogResult.OK) {
          IList<GridSelectionEvent> listGridSelectionEvent = new List<GridSelectionEvent>();
          foreach(KeyValuePair<string, GridSelectionEvent> nameGridSelectionEvent in GridModifier.Regions) {
            listGridSelectionEvent.Add(nameGridSelectionEvent.Value);
          }

          ExcelFileDataGrid.ExportGridAsFile(saveFileDialog.FileName,
                                             "Sheet1", this.dataGridView1,
                                             listGridSelectionEvent);
          this.excelFileNameTextBox.Text = saveFileDialog.FileName;
          WasModifiedSinceLastSave = false;
        }
      } catch (UnauthorizedAccessException ex) {
        string errorMsg = ex.ToString();
        log.Error(errorMsg);
        MessageBox.Show(errorMsg);
      }
    }
    
    /// <summary>
    /// Load a macro file and execute it on the grid
    /// </summary>
    void LoadMacroToolStripMenuItemClick(object sender, EventArgs e)
    {
      if (GridModifier == null) {
        return;
      }

      FileDialog loadMacroDialog = new OpenFileDialog();
      loadMacroDialog.InitialDirectory =
        Lemoine.Info.OptionsFile.GetOption (MACRO_DIRECTORY);
      loadMacroDialog.Filter = "Macro files|*.xml";
      loadMacroDialog.Title = "Load Custom Macro";
      if (loadMacroDialog.ShowDialog() == DialogResult.OK) {
        try {
          WasModifiedSinceLastSave = true;
          GridModifier.LoadMacro(loadMacroDialog.FileName);
          updateSequenceSelectionTextbox();
        } catch(System.InvalidOperationException) {
          MessageBox.Show("Unable to load macro");
        }
      }
    }
    
    /// <summary>
    /// Save grid modifications as macro file
    /// </summary>
    void SaveMacroToolStripMenuItemClick(object sender, EventArgs e)
    {
      if (GridModifier == null) {
        return;
      }

      FileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.InitialDirectory =
        Lemoine.Info.OptionsFile.GetOption (MACRO_DIRECTORY);
      saveFileDialog.Filter = "Macro files|*.xml";
      saveFileDialog.Title = "Save Macro";
      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        GridModifier.SaveMacro(saveFileDialog.FileName);
      }
    }
    
    /// <summary>
    /// copy selected cell content to another cell
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void CopyCellsToolStripMenuItemClick(object sender, System.EventArgs e)
    {
      if (GridModifier == null) {
        return;
      }

      int selectedCellCount = dataGridView1.GetCellCount(DataGridViewElementStates.Selected);
      
      if (selectedCellCount != 2)  {
        MessageBox.Show("Select exactly two cells for copy (source then destination)");
        return;
      }
      
      // seems to be the right order
      DataGridViewCell selectedCellTarget = dataGridView1.SelectedCells[0];
      DataGridViewCell selectedCellSource = dataGridView1.SelectedCells[1];
      
      if (selectedCellSource != null) {
        WasModifiedSinceLastSave = true;
        GridModifier.CopyToCell(selectedCellSource.RowIndex, selectedCellSource.ColumnIndex,
                                selectedCellTarget.RowIndex, selectedCellTarget.ColumnIndex);
      }
    }
    
    /// <summary>
    /// Set new cell value
    /// </summary>
    void SetCellValueToolStripMenuItemClick(object sender, EventArgs e)
    {
      if (GridModifier == null) {
        return;
      }

      int selectedCellCount = dataGridView1.GetCellCount(DataGridViewElementStates.Selected);
      
      if (selectedCellCount != 1)  {
        MessageBox.Show("Select exactly one cell for input");
        return;
      }
      
      DataGridViewCell selectedCell = dataGridView1.SelectedCells[0];
      if (selectedCell != null) {
        object oldSelectedValue = selectedCell.Value;
        OKCancelDialog dialog = new OKCancelDialog();
        Label dialogLabel = new Label();
        dialogLabel.Location = new System.Drawing.Point(10,10);
        dialogLabel.Size = new System.Drawing.Size(250,20);
        dialogLabel.TextAlign = ContentAlignment.TopLeft;
        dialogLabel.Text = "Input new cell value";
        TextBox dialogTextBox = new TextBox();
        dialogTextBox.Location = new System.Drawing.Point(10,100);
        dialogTextBox.Size = new System.Drawing.Size(250,200);
        dialogTextBox.Visible = true;
        dialogTextBox.Text = (oldSelectedValue is string ? (string) oldSelectedValue : "");
        dialog.SuspendLayout();
        dialog.StartPosition = FormStartPosition.CenterParent;
        dialog.Controls.Add(dialogLabel);
        dialog.Controls.Add(dialogTextBox);
        dialog.ActiveControl = dialogTextBox;
        dialog.ResumeLayout();
        dialog.PerformLayout();
        
        if (dialog.ShowDialog() != DialogResult.OK) {
          return;
        }
        else {
          WasModifiedSinceLastSave = true;
          GridModifier.SetCellValue(selectedCell.RowIndex, selectedCell.ColumnIndex, dialogTextBox.Text);
        }
      }
    }

    void PruneStripMenuItemClick(object sender, EventArgs e)
    {
      if (GridModifier != null) {
        WasModifiedSinceLastSave = true;
        GridModifier.PruneRowCol();
      }
    }

    /// <summary>
    /// check format sheet number textbox (0 by default)
    /// </summary>
    void SheetNumberTextBoxTextChanged(object sender, EventArgs e)
    {
      try {
        int sheetNumber = int.Parse(this.sheetNumberTextBox.Text.ToString());
      } catch (Exception) {
        this.sheetNumberTextBox.Text = "0";
      }
    }

    private void updateSequenceSelectionTextbox()
    {
      GridSelectionEvent sequenceSelectionEvent = this.getSequenceRegion();
      if (sequenceSelectionEvent != null) {
        this.selectSequenceRegionTextbox.Text = sequenceSelectionEvent.ToString();
      }
      else {
        this.selectSequenceRegionTextbox.Text = "";
      }
    }

    /// <summary>
    /// new sequence region selection
    /// </summary>
    void SelectSequenceToolStripMenuItemClick(object sender, EventArgs e)
    {
      if (GridModifier != null) {
        WasModifiedSinceLastSave = true;
        GridModifier.SelectRegion(SEQUENCE_REGION_NAME);
        updateSequenceSelectionTextbox();
      }
    }

    private GridSelectionEvent getSequenceRegion() {
      if (GridModifier == null) {
        return null;
      }

      GridSelectionEvent sequenceSelection;
      GridModifier.Regions.TryGetValue(SEQUENCE_REGION_NAME, out sequenceSelection);
      return sequenceSelection;
    }

    private void selectRegionInGrid(DataGridView dataGridView, int minRow, int maxRow,
                                    int minCol, int maxCol)
    {
      dataGridView.ClearSelection();
      for(int rowIndex = minRow ; rowIndex <= maxRow ; rowIndex++) {
        for (int colIndex = minCol ; colIndex <= maxCol; colIndex++) {
          dataGridView1.Rows[rowIndex].Cells[colIndex].Selected = true;
        }
      }
    }

    /// <summary>
    /// select cells corresponding to selected sequence region (if any)
    /// </summary>
    void ShowSequenceRegionToolStripMenuItemClick(object sender, EventArgs e)
    {
      GridSelectionEvent sequenceSelection = getSequenceRegion();
      if (sequenceSelection != null) {
        selectRegionInGrid(this.dataGridView1,
                           sequenceSelection.MinRow, sequenceSelection.MaxRow,
                           sequenceSelection.MinColumn, sequenceSelection.MaxColumn);
      } else {
        MessageBox.Show("No sequence region selected (or region has been invalidated)");
      }
    }

    #endregion // Grid modifications and selections

    #region Excel DB import

    /// <summary>
    /// Result of DB import
    /// </summary>
    class ImportResult {
      bool m_importOK = false;
      Exception m_raisedEx = null;
      
      public ImportResult() { m_importOK = true; }
      public ImportResult(Exception ex) { m_importOK = false; m_raisedEx = ex; }
      
      public bool IsOK {
        get { return m_importOK; }
      }
      
      public Exception RaisedException {
        get { return m_raisedEx; }
      }
    }

    /// <summary>
    /// Select an Excel file and import it into DB
    /// </summary>
    void ImportIntoDatabaseToolStripMenuItemClick(object sender, EventArgs e)
    {
      if (!this.BackGroundWorkerDone) {
        MessageBox.Show("Waiting for previous import to end");
        return;
      }
      
      if (WasModifiedSinceLastSave) {
        OKCancelMessageDialog okCancelDialog = new OKCancelMessageDialog();
        okCancelDialog.Message = "Grid modifications were not saved to file: proceed anyway ?"; // TODO:internationalization
        if (okCancelDialog.ShowDialog() != DialogResult.OK) {
          return;
        }
      }
      
      string fileNameToImport = this.excelFileNameTextBox.Text;
      if (fileNameToImport == "") {
        OpenFileDialog dialog = new OpenFileDialog();
        dialog.InitialDirectory = Lemoine.Info.OptionsFile.GetOption (SAVE_DIRECTORY);
        dialog.Filter = "Excel files|*.xls";
        dialog.Title = "Select Excel file";
        if (dialog.ShowDialog() == DialogResult.OK) {
          fileNameToImport = dialog.FileName.ToString();
        }
      }
      
      if (fileNameToImport == "") {
        return;
      }
      
      // first select operation (single path mode) or path (multi-path mode)
      // and patch excel file
      string finalFileName = MyAddOperationOrPathRegionInFile(fileNameToImport);
      if (finalFileName != null) {
        // launch import thread and periodically check whether it has ended
        BackgroundWorker bg = new BackgroundWorker();
        bg.DoWork += new DoWorkEventHandler(ImportExcelFile);
        bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ExcelFileCompleted);
        object[] worker_params = new object[] { finalFileName, this.excelImportConfigFileComboBox.SelectedItem };
        this.BackGroundWorkerDone = false;
        Cursor=Cursors.AppStarting;
        bg.RunWorkerAsync(worker_params);
      }
    }
    

    /// <summary>
    /// Ask user to select and operation (single path mode) or path (multi path mode)
    /// then create a temporary Excel file adding an associated region
    /// to the input file ; return name of this file or null on failure
    /// </summary>
    /// <param name="excelFileName"></param>
    /// <returns></returns>
    private string MyAddOperationOrPathRegionInFile(string excelFileName) {
      // ask user for selecting the operation corresponding to the file
      // (or creating it)
      Form pleaseWait = new Form();
      pleaseWait.AutoScaleBaseSize = new System.Drawing.Size(5,13);
      pleaseWait.ClientSize = new System.Drawing.Size(232,0);
      pleaseWait.ControlBox = false;
      pleaseWait.Cursor = System.Windows.Forms.Cursors.AppStarting;
      pleaseWait.MaximizeBox = false;
      pleaseWait.MinimizeBox = false;
      pleaseWait.Name = "PleaseWaitForm";
      pleaseWait.Text = "Loading Tree View - please wait";
      pleaseWait.StartPosition = FormStartPosition.CenterScreen;
      pleaseWait.Show(this);
      
      
      SelectOperationOrPathDialog selectOperationOrPathDialog = new SelectOperationOrPathDialog();
      selectOperationOrPathDialog.StartPosition = FormStartPosition.CenterParent;

      if (Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath))) {
        selectOperationOrPathDialog.OnlyOperationOK = true;
      } else {
        selectOperationOrPathDialog.OnlyPathOK = true;
      }
      
      pleaseWait.Hide();
      
      if (selectOperationOrPathDialog.ShowDialog(this) == DialogResult.OK) {
        
        if (Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath))) {
          IOperation selectedOperation = selectOperationOrPathDialog.Operation;
          if (selectedOperation != null) {
            int sequenceCount;
            int pathCount;
            using(IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
              selectedOperation = ModelDAOHelper.DAOFactory.OperationDAO.FindById(((Lemoine.Collections.IDataWithId)selectedOperation).Id);
              sequenceCount =
                selectedOperation.Sequences.Count;
              pathCount =
                selectedOperation.Paths.Count;
            }
            if (sequenceCount > 0) {
              MessageBox.Show("This operation already has associated sequences. Import aborted");
            } else {
              
              if (pathCount > 1) {
                log.ErrorFormat("Selected operation {0} has more than one path in single path mode",
                                selectedOperation);
                MessageBox.Show(PulseCatalog.GetString("MultiplePathInSinglePath"));
                return null;
              }
              
              if (pathCount == 0) {
                using(IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
                  using (IDAOTransaction transaction = daoSession.BeginTransaction()) {
                  log.InfoFormat("Creating a path for operation {0}", selectedOperation.ToString());
                  IPath path = ModelDAOHelper.ModelFactory.CreatePath();
                  path.Operation = selectedOperation;
                  ModelDAOHelper.DAOFactory.PathDAO.MakePersistent(path);
                  transaction.Commit();
                }
              }
              IEnumerator<IPath> pathEnumerator = selectedOperation.Paths.GetEnumerator();
              bool isNext = pathEnumerator.MoveNext();
              System.Diagnostics.Debug.Assert(isNext == true);
              IPath selectedPath = pathEnumerator.Current;
              return ExcelImporter.AddPathRegionInFile(excelFileName,
                                                       selectedPath, 0);
            }
          } else {
            MessageBox.Show("Please select an operation");
          }
        } else {
          IPath selectedPath = selectOperationOrPathDialog.Path;
          if (selectedPath != null) {
            int sequenceCount;
            using(IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
              sequenceCount =
                ModelDAOHelper.DAOFactory.PathDAO.FindById(((Lemoine.Collections.IDataWithId)selectedPath).Id).Sequences.Count;
            }
            if (sequenceCount > 0) {
              MessageBox.Show("This path already has associated sequences. Import aborted");
            } else {
              return ExcelImporter.AddPathRegionInFile(excelFileName,
                                                       selectedPath, 0);
            }
          } else {
            MessageBox.Show("Please select a path");
          }
        }
      }
      return null;
    }

    /// <summary>
    /// launch import an excel file into DB
    /// Argument of DoWorkEventArgs should start
    /// with imported excel file name
    /// </summary>
    void ImportExcelFile(object sender, DoWorkEventArgs e)
    {
      // called in BackgroundWorker thread
      // fetch Excel file name to import
      object[] parameters = e.Argument as object[];
      if (parameters.Length != 2) {
        e.Result = new ImportResult(new ArgumentException());
        return;
      }
      string excelFileName = (string) parameters[0];
      string configurationFile = (string) parameters[1];
      
      try {
        ExcelImporter importer = new ExcelImporter(configurationFile, excelFileName);
        importer.Build();
        e.Result = new ImportResult();
      } catch (Exception ex) {
        // TODO: find a way not to fail horribly when Excel file is opened elsewhere
        e.Result = new ImportResult(ex);
      }
    }

    /// <summary>
    ///  Import GUI finalization on BackgroundWorker termination
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ExcelFileCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      // called in GUI thread
      this.BackGroundWorkerDone = true;
      Cursor = Cursors.Default;
      ImportResult res = (ImportResult) e.Result;
      if (res.IsOK) {
        MessageBox.Show("Import successful");
      }
      else {
        MessageBox.Show("Problem during import " + res.RaisedException.ToString());
      }
    }

    #endregion // Excel DB import

    #region IDataGridModifierObserver implementation
    /// <summary>
    /// action to take when observed DataGridModifier notifies invalidation of region selections
    /// </summary>
    public void InvalidateRegionSelections() {
      this.selectSequenceRegionTextbox.Text = "";
    }
    #endregion // IDataGridModifierObserver implementation

    #endregion // Methods

    private void ExcelViewForm_Load (object sender, EventArgs e)
    {
      IList<string> configurationFilesList = Lemoine.Info.ConfigSet
        .LoadAndGet (CONFIGURATION_FILES_KEY, "")
        .Split (new char[] { ',' });

      foreach (string configFileName in configurationFilesList) {
        this.excelImportConfigFileComboBox.Items.Add (configFileName);
        if (configFileName
            .Equals (Lemoine.Info.OptionsFile.GetOption (DEFAULT_CONFIGURATION_FILE_KEY))) {
          this.excelImportConfigFileComboBox.SelectedItem = configFileName;
        }
      }
    }

    private void ExcelViewForm_FormClosed (object sender, FormClosedEventArgs e)
    {
      Application.Exit ();
    }
  }
}
