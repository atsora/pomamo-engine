// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using Lemoine.I18N;
using Lemoine.ModelDAO;
using Lemoine.GDBPersistentClasses;
using Lemoine.JobControls;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Extensions;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Info.ConfigReader.TargetSpecific;

namespace Lem_OperationExplorer
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MainForm).FullName);

    const string SEQUENCE_PROPERTY_LIST_IMPORT_EXPORT_KEY = "SequencePropertyListImportExport";
    const string SEQUENCE_PROPERTY_LIST_IMPORT_EXPORT_DEFAULT = "Name,Description,EstimatedTime=HH:MM:SS"; // or HH or nothing

    const string SEQUENCE_FIELD_LIST_IMPORT_EXPORT_KEY = "SequenceFieldListImportExport";
    const string SEQUENCE_FIELD_LIST_IMPORT_EXPORT_DEFAULT = "SpindleSpeed,FeedrateUS,ToolName,ToolNumber,ToolDiameter";

    const string ONLY_ACTIVE_FIELDS_IMPORT_EXPORT_KEY = "OnlyActiveFieldsImportExport";
    const bool ONLY_ACTIVE_FIELDS_IMPORT_EXPORT_DEFAULT = true;

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();
      
      string sequencePropertiesExportImportStr = Lemoine.Info.ConfigSet.LoadAndGet (SEQUENCE_PROPERTY_LIST_IMPORT_EXPORT_KEY, SEQUENCE_PROPERTY_LIST_IMPORT_EXPORT_DEFAULT);
      if (sequencePropertiesExportImportStr != null) {
        string[] sequencePropertiesExportImport = sequencePropertiesExportImportStr.Split(',');
        for(int i = 0 ; i < sequencePropertiesExportImport.Length ; i++) {
          string fullStr = sequencePropertiesExportImport[i];
          sequencePropertiesExportImport[i] = fullStr.Trim(' ');
        }
        if (sequencePropertiesExportImport.Length > 0) {
          this.operationTreeView.ColumnPropertyIdentifiers = sequencePropertiesExportImport;
        }
      }
      
      string sequenceFieldsExportImportStr = Lemoine.Info.ConfigSet.LoadAndGet(SEQUENCE_FIELD_LIST_IMPORT_EXPORT_KEY, SEQUENCE_FIELD_LIST_IMPORT_EXPORT_DEFAULT);
      if (sequenceFieldsExportImportStr != null) {
        string[] sequenceFieldsExportImport = sequenceFieldsExportImportStr.Split(',');
        for(int i = 0 ; i < sequenceFieldsExportImport.Length ; i++) {
          string fullStr = sequenceFieldsExportImport[i];
          sequenceFieldsExportImport[i] = fullStr.Trim(' ');
        }
        if (sequenceFieldsExportImport.Length > 0) {
          this.operationTreeView.ColumnFieldIdentifiers = sequenceFieldsExportImport;
        }
      }

      this.operationTreeView.RemoveNonActiveFieldsFromImportExport = Lemoine.Info.ConfigSet
        .LoadAndGet (ONLY_ACTIVE_FIELDS_IMPORT_EXPORT_KEY, ONLY_ACTIVE_FIELDS_IMPORT_EXPORT_DEFAULT);
      
      this.KeyPreview = true;
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScrollKeyDown);
    }

    #endregion // Constructors

    void MainFormLoad(object sender, EventArgs e)
    {
      operationTreeView.OrphanedItemsTreeView.AddObserver(informationControl);
      foreach(ITreeViewObserver observer in informationControl.AllObservers) {
        operationTreeView.OrphanedItemsTreeView.AddObserver(observer);
      }

      operationTreeView.AddObserver(informationControl);
      foreach(ITreeViewObserver observer in informationControl.AllObservers) {
        operationTreeView.AddObserver(observer);
      }
      
      operationTreeView.Init();
    }

    /// <summary>
    /// Called on key down event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ScrollKeyDown(object sender, KeyEventArgs e)
    {
      // toggle show id status on "end scroll" keypress
      if (e.KeyCode == Keys.Scroll)
      {
        this.informationControl.ShowId = !this.informationControl.ShowId;
        e.SuppressKeyPress = true;
        e.Handled = true;
        return;
      }
    }

    private void MainForm_FormClosed (object sender, FormClosedEventArgs e)
    {
      Application.Exit ();
    }
  }
}
