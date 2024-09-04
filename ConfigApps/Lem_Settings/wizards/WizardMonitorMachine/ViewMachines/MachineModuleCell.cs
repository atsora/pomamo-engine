// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Lemoine.BaseControls;
using Lemoine.DataRepository;
using Lemoine.Model;
using Lemoine.Settings;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of MachineModuleCell.
  /// </summary>
  public partial class MachineModuleCell : UserControl
  {
    #region Members
    readonly ContextMenuStrip m_menu = new ContextMenuStrip ();
    CncDocument m_cncDoc = null;
    readonly IList<string> m_errors = new List<string> ();
    string m_fullXmlPath = "";
    string m_logErrorName = "";
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Errors linked to the machine module
    /// If no errors: empty
    /// </summary>
    public string Errors { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Create and initialize a cell with the information from a machine module
    /// Within a transaction
    /// </summary>
    /// <param name="machineModule">machineModule, cannot be null</param>
    /// <param name="advancedMode">true to display more options</param>
    public MachineModuleCell (IMachineModule machineModule, bool advancedMode)
    {
      // Preparation of the interface
      InitializeComponent ();

      // Information about the machine module
      var acquisition = machineModule.CncAcquisition;
      if (acquisition != null) {
        labelAcquisitionName.Text = string.Format ("{0} (id {1})", acquisition.Name, acquisition.Id);
        labelAcquisitionFile.Text = acquisition.ConfigFile;

        // Load the CncDocument
        string fileName = labelAcquisitionFile.Text;
        if (!String.IsNullOrEmpty (fileName)) {
          XmlDocument xmlDoc = null;
          try {
            var factory = new FileRepoFactory ("cncconfigs", fileName);
            xmlDoc = factory.GetData (cancellationToken: System.Threading.CancellationToken.None);
          }
          catch {
            labelAcquisitionFile.ForeColor = LemSettingsGlobal.COLOR_ERROR;
            labelAcquisitionFile.Font = new Font (labelAcquisitionFile.Font, FontStyle.Italic);
            m_errors.Add ("cannot load the xml document (missing file?)");
          }

          if (xmlDoc != null) {
            m_cncDoc = new CncDocument (fileName, xmlDoc);
            if (!m_cncDoc.IsValid) {
              m_errors.Add ("invalid xml file");
            }
            else {
              buttonMenu.Enabled = true;
            }
          }
        }

        // Computer
        CheckComputer (acquisition.Computer);

        // Use of a process
        CheckProcess (acquisition.UseProcess, acquisition.StaThread, advancedMode);

        // Unit
        CheckUnit (machineModule.MonitoredMachine);

        // Parameters
        CheckParameters (acquisition.ConfigParameters, machineModule.MonitoredMachine);

        // Deprecated state
        CheckDeprecated ();

        // Full xml path
        m_fullXmlPath = GetXmlFullPath ("CncAcquisition-" + acquisition.Id + ".xml");

        // Log name
        if (acquisition.UseProcess) {
          m_logErrorName = "Lem_CncConsole-" + acquisition.Id + ".in.error";
        }
        else {
          m_logErrorName = "Lem_CncService.in.error";
        }
      }

      // Preparation of the menu
      m_menu.ItemClicked += MenuClicked;
      ToolStripItem item = m_menu.Items.Add ("Read description");
      item.Tag = "description";

      item = m_menu.Items.Add ("Open xml file");
      item.Tag = "open";

      if (advancedMode) {
        // Open full xml
        if (!String.IsNullOrEmpty (m_fullXmlPath)) {
          item = m_menu.Items.Add ("Open full xml");
          item.Tag = "open_full";
        }

        // Show logs
        item = m_menu.Items.Add ("Show logs");
        item.Tag = "show_logs";
      }

      DisplayErrors ();
    }
    #endregion // Constructors

    #region Event reactions
    void ButtonOpenClick (object sender, EventArgs e)
    {
      m_menu.Show (buttonMenu, new Point (0, buttonMenu.Height));
    }

    void MenuClicked (Object sender, ToolStripItemClickedEventArgs e)
    {
      m_menu.Hide ();
      if (e.ClickedItem.Tag == null) {
        return;
      }

      string actionStr = (string)e.ClickedItem.Tag;
      switch (actionStr) {
        case "description":
          ReadDescription ();
          break;
        case "open":
          Open ();
          break;
        case "open_full":
          OpenFull ();
          break;
        case "show_logs":
          ShowLogs ();
          break;
      }
    }
    #endregion // Event reactions

    #region Methods
    void DisplayErrors ()
    {
      if (m_errors.Count != 0) {
        Errors = string.Format ("Error{0} found: \n- {1}.",
                               m_errors.Count > 1 ? "s" : "",
                               string.Join (",\n- ", m_errors.ToArray ()));
      }
    }

    void CheckComputer (IComputer computer)
    {
      // Check that the computer is filled
      if (computer == null) {
        labelAcquisitionComputer.ForeColor = LemSettingsGlobal.COLOR_ERROR;
        labelAcquisitionComputer.Font = new Font (labelAcquisitionComputer.Font, FontStyle.Italic);
        m_errors.Add ("missing computer");
      }
      else {
        labelAcquisitionComputer.Text = computer.Name;

        // Check that the computer is reachable
        string errorMsg = Util.Ping (computer.Address);
        if (!string.IsNullOrEmpty (errorMsg)) {
          labelAcquisitionComputer.ForeColor = LemSettingsGlobal.COLOR_ERROR;
          labelAcquisitionComputer.Font = new Font (labelAcquisitionComputer.Font, FontStyle.Italic);
          m_errors.Add (string.Format (
            "distant computer with address '{0}' cannot be reached: {1}",
            computer.Address, errorMsg));
        }

        // Check that the computer is marked as an LPOST
        if (!computer.IsLpst) {
          m_errors.Add ("computer '" + computer.Name + "'is not marked as an LPOST in the database");
        }
      }
    }

    void CheckProcess (bool useProcess, bool staThread, bool advancedMode)
    {
      if (advancedMode) {
        labelProcess.Text = useProcess ? "(process)" : (staThread ? "(STA thread)" : "(MTA thread)");
      }
      else {
        labelProcess.Hide ();
      }

      // Consistency with the xml file
      if (m_cncDoc != null && m_cncDoc.IsValid && m_cncDoc.RunningModes.Count > 0) {
        if (useProcess && !m_cncDoc.RunningModes.Contains (CncDocument.RunningMode.USE_PROCESS)) {
          m_errors.Add ("the acquisition shouldn't use a process according to the xml file");
          labelProcess.ForeColor = LemSettingsGlobal.COLOR_ERROR;
          labelProcess.Font = new Font (labelProcess.Font, FontStyle.Italic);
        }
        else if (!useProcess && !staThread && !m_cncDoc.RunningModes.Contains (CncDocument.RunningMode.USE_THREAD)) {
          m_errors.Add ("the acquisition should not use a simple thread according to the xml file");
          labelProcess.ForeColor = LemSettingsGlobal.COLOR_ERROR;
          labelProcess.Font = new Font (labelProcess.Font, FontStyle.Italic);
        }
        else if (!useProcess && staThread && !m_cncDoc.RunningModes.Contains (CncDocument.RunningMode.USE_STA_THREAD)) {
          m_errors.Add ("the acquisition shouldn't use a STA thread according to the xml file");
          labelProcess.ForeColor = LemSettingsGlobal.COLOR_ERROR;
          labelProcess.Font = new Font (labelProcess.Font, FontStyle.Italic);
        }
      }
    }

    void CheckUnit (IMonitoredMachine machine)
    {
      if (machine == null) {
        return;
      }

      if (machine.PerformanceField != null && machine.PerformanceField.Unit != null &&
          m_cncDoc != null && m_cncDoc.IsValid) {
        var unit = machine.PerformanceField.Unit;

        // Consistency with the xml file
        if (m_cncDoc.IsImperial () && unit.Id == 1) {
          m_errors.Add ("the acquisition should use 'FeedrateUS' according to the xml file");
        }
        else if (m_cncDoc.IsMetric () && unit.Id == 2) {
          m_errors.Add ("the acquisition should use 'Feedrate' according to the xml file");
        }
      }
    }

    void CheckParameters (string parameters, IMonitoredMachine machine)
    {
      bool isRed = false;
      var paramList = new List<string> ();
      paramList.Add (parameters);

      // Check the consistency of the parameters
      if (m_cncDoc != null && m_cncDoc.IsValid) {
        CncDocument.LoadResult ret = m_cncDoc.Load (machine);
        if ((ret & CncDocument.LoadResult.TOO_MANY_PARAMETERS) == CncDocument.LoadResult.TOO_MANY_PARAMETERS ||
            (ret & CncDocument.LoadResult.WRONG_PARAMETERS) == CncDocument.LoadResult.WRONG_PARAMETERS) {
          if (string.IsNullOrEmpty (m_cncDoc.ErrorFound)) {
            m_errors.Add ("wrong parameters for the xml file");
          }
          else {
            m_errors.Add ("wrong parameters for the xml file (" + m_cncDoc.ErrorFound + ")");
          }

          isRed = true;
        }
        else {
          var errors = m_cncDoc.GetErrors () ?? new List<string> ();
          var warnings = m_cncDoc.GetWarnings () ?? new List<string> ();
          if (warnings.Count + errors.Count > 0) {
            foreach (var warning in warnings) {
              m_errors.Add (warning);
            }

            foreach (var error in errors) {
              m_errors.Add (error);
            }

            isRed = true;
          }

          paramList.Clear ();
          foreach (var parameter in m_cncDoc.Parameters) {
            string value = "";
            try {
              value = parameter.GetValue ();
              if (!parameter.Optional && !string.IsNullOrEmpty (parameter.DefaultValue) &&
                  (string.IsNullOrEmpty (value) || parameter.DefaultIsUsed)) {
                value = parameter.DefaultValue + " (default value)";
              }
            }
            catch (Exception ex) {
              value = "error " + ex.Message;
            }
            paramList.Add ($"{parameter.Description}: {value}");
          }
        }
      }

      if (isRed) {
        listParameters.ForeColor = LemSettingsGlobal.COLOR_ERROR;
        listParameters.Font = new Font (listParameters.Font, FontStyle.Italic);
      }
      foreach (var param in paramList) {
        listParameters.AddItem (param);
      }
    }

    void CheckDeprecated ()
    {
      if (m_cncDoc != null && m_cncDoc.Deprecated) {
        if (string.IsNullOrEmpty (m_cncDoc.AlternativeFile)) {
          m_errors.Add ("deprecated file with no alternative");
        }
        else {
          m_errors.Add ("this deprecated file has not been replaced yet by '" + m_cncDoc.AlternativeFile + "'");
        }
      }
    }

    void ReadDescription ()
    {
      if (m_cncDoc == null) {
        MessageBoxCentered.Show ("No .xml file", "Warning", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
      }
      else {
        var dialog = new DialogDescriptionXmlFile (labelAcquisitionFile.Text, m_cncDoc);
        dialog.Show ();
      }
    }

    void Open ()
    {
      if (m_cncDoc == null) {
        MessageBoxCentered.Show ("No .xml file", "Warning", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
      }
      else {
        var dialog = new XmlReaderDialog (labelAcquisitionFile.Text, m_cncDoc.FullText);

        // Metadata highlighted
        dialog.HighlightTextBetween ("<description>", "</description>", Color.BlanchedAlmond, true);
        dialog.HighlightTextBetween ("<supported-machines>", "</supported-machines>", Color.BlanchedAlmond, true);
        dialog.HighlightTextBetween ("<supported-controls>", "</supported-controls>", Color.BlanchedAlmond, true);
        dialog.HighlightTextBetween ("<supported-protocols>", "</supported-protocols>", Color.BlanchedAlmond, true);
        dialog.HighlightTextBetween ("<unit>", "</unit>", Color.BlanchedAlmond, true);
        dialog.HighlightTextBetween ("<parameters>", "</parameters>", Color.BlanchedAlmond, true);
        dialog.HighlightTextBetween ("<running-modes>", "</running-modes>", Color.BlanchedAlmond, true);
        dialog.HighlightTextBetween ("<machinemodules>", "</machinemodules>", Color.BlanchedAlmond, true);

        dialog.Show ();
      }
    }

    void OpenFull ()
    {
      string content = "";
      try {
        content = File.ReadAllText (m_fullXmlPath);
      }
      catch (Exception e) {
        MessageBoxCentered.Show (e.Message, "Warning", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
        return;
      }

      var dialog = new XmlReaderDialog (m_fullXmlPath, content);

      // Metadata highlighted
      dialog.HighlightTextBetween ("<description>", "</description>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<supported-machines>", "</supported-machines>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<supported-controls>", "</supported-controls>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<supported-protocols>", "</supported-protocols>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<unit>", "</unit>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<parameters>", "</parameters>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<running-modes>", "</running-modes>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<machinemodules>", "</machinemodules>", Color.BlanchedAlmond, true);

      dialog.Show ();
    }

    void ShowLogs ()
    {
      // TODO: Logs directory may change
      string logDirectory = Lemoine.Info.ConfigSet.LoadAndGet ("LogDirectory", "Logs");
      string filePath = Path.Combine (logDirectory, m_logErrorName);
      if (!File.Exists (filePath)) {
        MessageBoxCentered.Show ("No logs found.", "Warning", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
        return;
      }

      // Combine the arguments together. It doesn't matter if there is a space after ','
      string argument = "/select, \"" + filePath + "\"";

      System.Diagnostics.Process.Start ("explorer.exe", argument);
    }

    string GetXmlFullPath (string xmlName)
    {
      var possiblePath = new[] {
        "C:\\Windows\\system32\\config\\systemprofile\\Application Data\\PULSE",
        "C:\\Windows\\system32\\config\\systemprofile\\Local Settings\\Application Data\\PULSE",
        "C:\\Windows\\system32\\config\\systemprofile\\My Documents\\PULSE",
        "C:\\Windows\\system32\\config\\systemprofile\\AppData\\Local\\PULSE",
        "C:\\Windows\\system32\\config\\systemprofile\\Documents\\PULSE",
        "C:\\Windows\\SysWOW64\\config\\systemprofile\\AppData\\Local\\PULSE",
        "C:\\Windows\\SysWOW64\\config\\systemprofile\\Documents\\PULSE",
        Lemoine.Info.PulseInfo.LocalConfigurationDirectory
      };

      // Try to find a valid path
      foreach (var path in possiblePath) {
        var fullPath = Path.Combine (path, xmlName);
        if (File.Exists (fullPath)) {
          return fullPath;
        }
      }

      return "";
    }
    #endregion // Methods
  }
}
