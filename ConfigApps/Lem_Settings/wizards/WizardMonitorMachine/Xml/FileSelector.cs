// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of FileSelector.
  /// </summary>
  public partial class FileSelector : UserControl
  {
    #region Members
    readonly string m_directoryPath = "";
    readonly string m_subDirectoryName = "";
    readonly string m_filter = "";
    readonly bool m_isNotLctr = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (FileSelector).FullName);

    #region Getters / Setters
    /// <summary>
    /// Selected value (name of the file without the path)
    /// </summary>
    public string SelectedValue {
      get {
        return (combobox.SelectedValue ?? "").ToString();
      }
      set {
        combobox.SelectedValue = combobox.ContainsObject(value) ? value : "";
      }
    }
    
    /// <summary>
    /// Cnc acquisition id
    /// </summary>
    public int CncAcquisitionId { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="directoryPath">root directory</param>
    /// <param name="subDirectoryName">sub directory (can be null or empty)</param>
    /// <param name="filter">extensions allowed when browsing files</param>
    /// <param name="defaultValue">default file name (can be null or empty)</param>
    public FileSelector(string directoryPath, string subDirectoryName, string filter, string defaultValue)
    {
      m_directoryPath = directoryPath;
      m_subDirectoryName = subDirectoryName;
      m_filter = filter;
      InitializeComponent();
      
      // LPOST?
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var localComputer = ModelDAOHelper.DAOFactory.ComputerDAO.GetLocal();
          if (localComputer != null) {
            m_isNotLctr = !localComputer.IsLctr;
          }
          else {
            log.WarnFormat("FileSelector: the local computer could not be found in the computer table");
          }
        }
      }
      if (m_isNotLctr) {
        buttonDelete.Hide();
        layout.ColumnStyles[2].Width = 0;
      }
      
      // Tooltip
      SetToolTip(buttonUpload, "Upload the selected file on the desktop");
      SetToolTip(buttonDelete, "Delete the selected file");
      
      // Load the combobox
      ReloadCombobox(defaultValue);
    }
    #endregion // Constructors
    
    #region Methods
    void ReloadCombobox(string selectedValue)
    {
      // Clear all items
      combobox.ClearItems();
      
      // Scan the subdirectory content
      var filePaths = Directory.GetFiles(Path.Combine(m_directoryPath, m_subDirectoryName),
                                         "*.*", SearchOption.TopDirectoryOnly);
      
      // Extensions allowed
      var extensions = new List<string>();
      if (!string.IsNullOrEmpty(m_filter)) {
        var exts = m_filter.Split('|');
        foreach (var ext in exts) {
          extensions.Add("." + ext.ToLower());
        }
      }
      
      foreach (var filePath in filePaths) {
        if (extensions.Count == 0 || extensions.Contains(Path.GetExtension(filePath).ToLower())) {
          var fileName = Path.GetFileName(filePath);
          combobox.AddItem(fileName, Path.Combine(m_subDirectoryName, fileName));
        }
      }
      combobox.InsertItem("Select a file...", "", 0);
      if (!m_isNotLctr) {
        combobox.InsertItem("Browse...", null, combobox.Count);
      }

      // Try to select a value
      SelectedValue = selectedValue;
    }
    
    void SetToolTip(Control control, string text)
    {
      var toolTip = new ToolTip();
      toolTip.SetToolTip(control, text);
      toolTip.AutoPopDelay = 32000;
    }
    #endregion // Methods

    #region Event reactions
    void ButtonDeleteMouseClick(object sender, MouseEventArgs e)
    {
      // File to delete
      string fileName = SelectedValue;
      if (string.IsNullOrEmpty(fileName)) {
        return;
      }

      // Check that this file is not used in another configuration
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var acquisitions = ModelDAOHelper.DAOFactory.CncAcquisitionDAO.FindAll();
          foreach (var acquisition in acquisitions) {
            if (acquisition.Id != CncAcquisitionId &&
                !String.IsNullOrEmpty(acquisition.ConfigParameters) &&
                acquisition.ConfigParameters.Contains(fileName))
            {
              IMachineModule mamo = null;
              foreach (var mm in acquisition.MachineModules) {
                mamo = mm;
                break;
              }
              string acquisitionName = (mamo != null) ? "machine '" + mamo.Name + "'" : "another machine";
              MessageBoxCentered.Show("The file '" + fileName + "' is used in the acquisition for " +
                                      acquisitionName + " => don't delete it.",
                                      "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              return;
            }
          }
        }
      }
      
      // Confirmation to delete this file
      if (MessageBoxCentered.Show(
        "Are you sure you want to delete the file '" + Path.GetFileName(fileName) + "'?",
        "Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
      {
        var filePath = Path.Combine(m_directoryPath, fileName);
        try {
          File.Delete(filePath);
          ReloadCombobox("");
        } catch (Exception ex) {
          MessageBoxCentered.Show("Couldn't delete the file '" + fileName + "'. You can try to run LemSettings with admin rights.",
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          log.ErrorFormat("LemSettings.WizardMonitorMachine - couldn't delete the file '{0}': {1}",
                          filePath, ex);
        }
      }
    }
    
    void ButtonUploadMouseClick(object sender, MouseEventArgs e)
    {
      // File to copy
      string fileName = Path.GetFileName(SelectedValue);
      if (string.IsNullOrEmpty(fileName)) {
        return;
      }

      var filePath = Path.Combine(Path.Combine(m_directoryPath, m_subDirectoryName), fileName);
      
      // Find an available file name on the desktop
      var desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
      
      if (File.Exists(Path.Combine(desktopFolder, fileName))) {
        int suffix = 0;
        do {
          suffix++;
          fileName = Path.GetFileNameWithoutExtension(filePath) + "-" + suffix + Path.GetExtension(filePath);
        } while (File.Exists(Path.Combine(desktopFolder, fileName)));
      }
      
      // Copy the file to the desktop
      try {
        File.Copy(filePath, Path.Combine(desktopFolder, fileName));
        MessageBoxCentered.Show("The file '" + fileName + "' has been created on your desktop.",
                                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
      } catch (Exception ex) {
        MessageBoxCentered.Show("Couldn't create the file '" + fileName + "' on your desktop.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        log.ErrorFormat("LemSettings.WizardMonitorMachine - couldn't create the file '{0}' on the desktop: {1}",
                        fileName, ex);
      }
    }
    
    void ComboboxItemChanged(string text, object value)
    {
      buttonUpload.Enabled = buttonDelete.Enabled =
        (combobox.SelectedValue != null && (string)combobox.SelectedValue != "");
      if (value != null) {
        return;
      }

      // Browse a file
      var dialog = new OpenFileDialog();
      if (!string.IsNullOrEmpty(m_filter)) {
        var str = "";
        foreach (var elt in m_filter.Split('|')) {
          if (str != "") {
            str += ";";
          }

          str += "*." + elt;
        }
        dialog.Filter = "Files|" + str;
      }
      var result = dialog.ShowDialog();
      if (result == DialogResult.OK)
      {
        // File already uploaded?
        var fileName = Path.GetFileName(dialog.FileName);
        var filePath = Path.Combine(Path.Combine(m_directoryPath, m_subDirectoryName), fileName);
        if (File.Exists(filePath))
        {
          // Ask for replacement
          if (MessageBoxCentered.Show(
            "The file '" + fileName + "' already exists. Do you want to replace it?",
            "Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
          {
            // Try to delete the existing file
            try {
              File.Delete(filePath);
            } catch (Exception e) {
              MessageBoxCentered.Show("Couldn't replace the file '" + fileName + "'. You can try to run LemSettings with admin rights.",
                                      "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              log.ErrorFormat("LemSettings.WizardMonitorMachine - couldn't replace the file '{0}' : {1}",
                              filePath, e);
              ReloadCombobox(Path.Combine(m_subDirectoryName, fileName));
              return;
            }
          } else {
            ReloadCombobox(Path.Combine(m_subDirectoryName, fileName));
            return;
          }
        }
        
        // Import it
        try {
          File.Copy(dialog.FileName, filePath);
        } catch (Exception e) {
          MessageBoxCentered.Show("Couldn't copy the file '" + fileName + "'. You can try to run LemSettings with admin rights.",
                                  "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          log.ErrorFormat("LemSettings.WizardMonitorMachine - couldn't copy the file '{0}' : {1}",
                          filePath, e);
        }
        
        // Reload the combobox and select it
        ReloadCombobox(Path.Combine(m_subDirectoryName, fileName));
      } else {
        // Select the first element
        SelectedValue = "";
      }
    }
    #endregion // Event reactions
  }
}
