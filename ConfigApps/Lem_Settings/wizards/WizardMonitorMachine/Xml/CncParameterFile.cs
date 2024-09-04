// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncParameterFile.
  /// Choose a file within a subdirectory of cncconfigs and allow the user to import one
  /// Imported files are prefixed with "_" and can be deleted by the user
  /// </summary>
  public class CncParameterFile: AbstractParameter
  {
    #region Members
    string m_directoryPath;
    string m_subDirectoryName;
    string m_filter;
    bool m_fallbackMode;
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (CncParameterUrl).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncParameterFile(XmlNode node) : base(node) {}
    #endregion // Constructors

    #region Methods
    protected override bool Parse(XmlAttributeCollection attributes)
    {
      bool ok = true;
      
      // Attributes
      m_subDirectoryName = (attributes["subdirectory"] != null) ? (attributes["subdirectory"].Value ?? "") : "";
      if (attributes["filter"] != null && attributes["filter"].Value != null) {
        m_filter = attributes["filter"].Value ?? "";
      }
      else {
        m_filter = "";
      }

      // Check that the subdirectory exists
      m_directoryPath = Path.Combine(Lemoine.Info.PulseInfo.PfrDataDir, "cnc_resources");
      var fullDirectory = Path.Combine(m_directoryPath, m_subDirectoryName);
      if (!Directory.Exists(fullDirectory)) {
        log.Warn("Directory \"" + fullDirectory + "\" specified by parameter \"subdirectory\" doesn't exist in cnc_resources. " +
                 "Try to create it.");
        
        // Try to create it
        try {
          Directory.CreateDirectory(fullDirectory);
        } catch (Exception e) {
          log.ErrorFormat("Cannot create directory {0} as specified in parameter 'SubDirectory': {1}", fullDirectory, e);
          ok = false;
        }
      }
      
      // Try to read the content of the directory
      try {
        var filePaths = Directory.GetFiles(fullDirectory, "*.*", SearchOption.TopDirectoryOnly);
      } catch (Exception e) {
        log.ErrorFormat("Cannot read the content of directory {0}: {1}", fullDirectory, e);
        ok = false;
      }
      
      m_fallbackMode = !ok;
      return true;
    }
    
    protected override Control CreateControl(string defaultValue)
    {
      return m_fallbackMode ?
        new FileSelectorFallback(m_directoryPath, m_subDirectoryName, m_filter, defaultValue) as Control :
        new FileSelector(m_directoryPath, m_subDirectoryName, m_filter, defaultValue) as Control;
    }
    
    protected override string GetValue(Control specializedControl)
    {
      return m_fallbackMode ?
        (specializedControl as FileSelectorFallback).SelectedValue :
        (specializedControl as FileSelector).SelectedValue;
    }
    
    protected override void SetValue(Control specializedControl, string value)
    {
      if (m_fallbackMode) {
        (specializedControl as FileSelectorFallback).SelectedValue = value;
      }
      else {
        var c = specializedControl as FileSelector;
        c.CncAcquisitionId = this.CncAcquisitionId;
        c.SelectedValue = value;
      }
    }
    
    protected override string ValidateInput(Control specializedControl)
    {
      string selectedFile = GetValue(specializedControl);
      
      // File not specified?
      if (string.IsNullOrEmpty(selectedFile)) {
        return "file not specified";
      }

      // Test if the file exists
      return File.Exists(Path.Combine(m_directoryPath, selectedFile)) ?
        "" : "the file '" + Path.Combine(m_directoryPath, selectedFile) + "'doesn't exist";
    }
    
    protected override string GetWarning(Control specializedControl)
    {
      return "";
    }
    #endregion // Methods
  }
}
