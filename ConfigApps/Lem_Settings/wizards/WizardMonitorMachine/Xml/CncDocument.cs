// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
// 
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Xml;
using Lemoine.Collections;
using System.Linq;
using Lemoine.GDBMigration;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of CncDocument.
  /// </summary>
  public class CncDocument
  {
    /// <summary>
    /// Result when loading a machine, compared to a xml document
    /// </summary>
    [Flags]
    public enum LoadResult
    {
      /// <summary>
      /// The configuration match the xml document
      /// </summary>
      SUCCESS = 0x00,

      /// <summary>
      /// The machine is not monitored yet, nothing to load
      /// </summary>
      NOT_MONITORED = 0x01,

      /// <summary>
      /// The xml document specifies more machine modules than those in the database
      /// </summary>
      TOO_FEW_MACHINE_MODULES = 0x02,

      /// <summary>
      /// The xml document specified less machine modules than those in the database
      /// </summary>
      TOO_MANY_MACHINE_MODULES = 0x04,

      /// <summary>
      /// The xml document specified machine modules with different prefix
      /// </summary>
      DIFFERENT_MACHINE_MODULES = 0x40,

      /// <summary>
      /// The xml document title specified in the database is not the one chosen
      /// </summary>
      DIFFERENT_XML = 0x08,

      /// <summary>
      /// Too many parameters in the database compared to those specified in the xml document
      /// </summary>
      TOO_MANY_PARAMETERS = 0x10,

      /// <summary>
      /// Wrong parameters in the database compared to what is expected by the xml document
      /// </summary>
      WRONG_PARAMETERS = 0x20
    }

    public class Module
    {
      public string m_identifier = ""; // Used for the prefix (identifier + "-") and the name of the machine module (name + "-" + identifier)
      public string m_autoSequenceActivity = ""; // Empty, Machine or MachineModule
      public string m_description = ""; // Textual description for the user

      public bool m_sequenceVariable = false;
      public string m_sequenceVariableValue = "";
      public bool m_cycleVariable = false;
      public string m_cycleVariableValue = "";
      public bool m_startCycleVariable = false;
      public string m_startCycleVariableValue = "";
      public bool m_detectionMethodVariable = false;
      public string m_detectionMethodVariableValue = "";

      public int m_sequenceDetectionMethod = -1;
      public int m_cycleDetectionMethod = -1;
      public int m_startCycleDetectionMethod = -1;
    }

    public class SupportedAttributes
    {
      public string m_mainAttribute = "";
      public IDictionary<string, string> m_additionalAttributes =
        new Dictionary<string, string> ();
    }

    public enum RunningMode
    {
      USE_THREAD,
      USE_STA_THREAD,
      USE_PROCESS
    }

    static readonly ILog log = LogManager.GetLogger (typeof (CncDocument).FullName);

    /// <summary>
    /// Filename
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Unit
    /// </summary>
    public string Unit { get; private set; }

    /// <summary>
    /// Supported machines
    /// </summary>
    public IList<SupportedAttributes> SupportedMachines { get; private set; }

    /// <summary>
    /// Supported controls
    /// </summary>
    public IList<SupportedAttributes> SupportedControls { get; private set; }

    /// <summary>
    /// Supported protocols
    /// </summary>
    public IList<SupportedAttributes> SupportedProtocols { get; private set; }

    /// <summary>
    /// Supported customers
    /// </summary>
    public IList<SupportedAttributes> SupportedCustomers { get; private set; }

    /// <summary>
    /// Parameters, configuration is stored here
    /// </summary>
    public IList<AbstractParameter> Parameters { get; private set; }

    /// <summary>
    /// Running modes
    /// </summary>
    public IList<RunningMode> RunningModes { get; private set; }

    /// <summary>
    /// Modules, configuration is stored here
    /// </summary>
    public IList<Module> Modules { get; private set; }

    /// <summary>
    /// Return true if the document is valid
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Full text of the document
    /// </summary>
    public string FullText { get; private set; }

    /// <summary>
    /// Duration in seconds between two acquisitions
    /// </summary>
    public double AcquisitionEvery { get; private set; }

    /// <summary>
    /// Not responding timeout in seconds
    /// </summary>
    public double NotRespondingTimeout { get; private set; }

    /// <summary>
    /// Time to wait in seconds before a restart
    /// </summary>
    public double SleepBeforeRestart { get; private set; }

    /// <summary>
    /// Is the configuration deprecated?
    /// </summary>
    public bool Deprecated { get; private set; }

    /// <summary>
    /// If the configuration is deprecated, this is the alternative file (can be empty or null)
    /// </summary>
    public string AlternativeFile { get; private set; }

    /// <summary>
    /// If an error occurred, this field might explain
    /// </summary>
    public string ErrorFound { get; private set; }

    /// <summary>
    /// Old parameter (single parameter)
    /// </summary>
    public bool OldSingleParameterConfiguration { get; set; }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncDocument (string fileName, XmlDocument xmlDoc)
    {
      // Default parameters
      AcquisitionEvery = -1;
      NotRespondingTimeout = -1;
      SleepBeforeRestart = -1;
      Deprecated = false;
      AlternativeFile = "";
      ErrorFound = "";

      IsValid = true;
      SupportedMachines = new List<SupportedAttributes> ();
      SupportedControls = new List<SupportedAttributes> ();
      SupportedProtocols = new List<SupportedAttributes> ();
      SupportedCustomers = new List<SupportedAttributes> ();
      Parameters = new List<AbstractParameter> ();
      RunningModes = new List<RunningMode> ();
      Modules = new List<Module> ();
      FileName = fileName;
      FullText = xmlDoc.OuterXml;
      try {
        Parse (xmlDoc);
      }
      catch (Exception ex) {
        log.Error ($"Couldn't parse file {fileName}", ex);
        IsValid = false;
      }

      if (IsValid) {
        CheckValidity ();
      }
    }

    /// <summary>
    /// Dispose all controls previously created
    /// </summary>
    public void DisposeAllControls ()
    {
      foreach (AbstractParameter parameter in Parameters) {
        parameter.DisposeAllControls ();
      }
    }

    /// <summary>
    /// Get the input errors
    /// </summary>
    /// <returns></returns>
    public IList<string> GetErrors ()
    {
      IList<string> errors = new List<string> ();

      foreach (AbstractParameter parameter in Parameters) {
        string error = parameter.GetError ();
        if (error != "") {
          errors.Add (error);
        }
      }

      return errors;
    }

    /// <summary>
    /// Get the input warnings
    /// </summary>
    /// <returns></returns>
    public IList<string> GetWarnings ()
    {
      IList<string> warnings = new List<string> ();

      foreach (AbstractParameter parameter in Parameters) {
        string warning = parameter.GetWarning ();
        if (warning != "") {
          warnings.Add (warning);
        }
      }

      return warnings;
    }

    /// <summary>
    /// Get the summary of all inputs
    /// </summary>
    /// <param name="advancedAllowed"></param>
    /// <returns></returns>
    public IList<string> GetSummary (bool advancedAllowed)
    {
      IList<string> summary = new List<string> ();

      foreach (AbstractParameter parameter in Parameters) {
        if (parameter.GetControl (false) != null && !parameter.Hidden &&
            (advancedAllowed || !parameter.AdvancedUsage)) {
          string text = parameter.Description + ": ";
          if (parameter.GetValue () == "") {
            text += "-";
          }
          else {
            text += "\"" + parameter.GetValue () + "\"";
          }

          summary.Add (text);
        }
      }

      return summary;
    }

    /// <summary>
    /// Get a formated string suitable for the cncAcquisition table
    /// </summary>
    /// <returns></returns>
    public string GetConfigParameters ()
    {
      if (OldSingleParameterConfiguration) {
        // Only one parameter
        if (Parameters.Count != 1) {
          throw new Exception ("Wrong parameter number for an old configuration");
        }

        return Parameters[0].GetValue ();
      }
      else {
        // Get the parameters and find a suitable separator
        return EnumerableString.ToListString (Parameters.Where (x => x.IsParamX ()).Select (x => x.GetValue ()));
      }
    }

    /// <summary>
    /// Based on the running mode list, return true if a process should be used
    /// </summary>
    /// <returns></returns>
    public bool UseProcess ()
    {
      if (RunningModes.Count == 0) {
        return false; // Shouldn't happen
      }

      return (RunningModes[0] == RunningMode.USE_PROCESS);
    }

    /// <summary>
    /// Based on the running mode list, return true if a sta thread should be used
    /// </summary>
    /// <returns></returns>
    public bool UseStaThread ()
    {
      if (RunningModes.Count == 0) {
        return false; // Shouldn't happen
      }

      return (RunningModes[0] == RunningMode.USE_STA_THREAD);
    }

    /// <summary>
    /// Return true if metric units are used
    /// False might say "maybe"
    /// </summary>
    /// <returns></returns>
    public bool IsMetric ()
    {
      return (Unit == "metric");
    }

    /// <summary>
    /// Return true if imperial units are used
    /// False might say "maybe"
    /// </summary>
    /// <returns></returns>
    public bool IsImperial ()
    {
      return (Unit == "inches");
    }

    // Method already in try / catch
    void Parse (XmlDocument xmlDoc)
    {
      // Root node and namespace
      XmlNode root = xmlDoc.DocumentElement;
      var nsmgr = new XmlNamespaceManager (xmlDoc.NameTable);
      nsmgr.AddNamespace ("cnc", "http://config.cnc.pulse.lemoinetechnologies.com");

      // Description
      XmlNode node = root.SelectSingleNode ("cnc:description", nsmgr);
      Description = node != null ? node.InnerText : null;

      // Deprecated
      node = root.SelectSingleNode ("cnc:deprecated", nsmgr);
      Deprecated = (node != null);

      // Deprecated with an alternative?
      if (Deprecated) {
        foreach (XmlAttribute attribute in node.Attributes) {
          if (string.Equals (attribute.Name, "file", StringComparison.CurrentCultureIgnoreCase)) {
            AlternativeFile = attribute.Value;
            break;
          }
        }
      }

      // Unit
      node = root.SelectSingleNode ("cnc:unit", nsmgr);
      Unit = node?.InnerText;

      // Supported machines
      XmlNodeList nodes = root.SelectNodes ("cnc:supported-machines/cnc:supported-machine", nsmgr);
      if (nodes != null) {
        foreach (XmlNode nodeTmp in nodes) {
          var supported = new SupportedAttributes ();
          supported.m_mainAttribute = nodeTmp.InnerText;
          foreach (XmlAttribute attribute in nodeTmp.Attributes) {
            supported.m_additionalAttributes[attribute.Name] = attribute.Value;
          }

          SupportedMachines.Add (supported);
        }
      }

      // Supported controls
      nodes = root.SelectNodes ("cnc:supported-controls/cnc:supported-control", nsmgr);
      if (nodes != null) {
        foreach (XmlNode nodeTmp in nodes) {
          var supported = new SupportedAttributes ();
          supported.m_mainAttribute = nodeTmp.InnerText;
          foreach (XmlAttribute attribute in nodeTmp.Attributes) {
            supported.m_additionalAttributes[attribute.Name] = attribute.Value;
          }

          SupportedControls.Add (supported);
        }
      }

      // Supported protocols
      nodes = root.SelectNodes ("cnc:supported-protocols/cnc:supported-protocol", nsmgr);
      if (nodes != null) {
        foreach (XmlNode nodeTmp in nodes) {
          var supported = new SupportedAttributes ();
          supported.m_mainAttribute = nodeTmp.InnerText;
          foreach (XmlAttribute attribute in nodeTmp.Attributes) {
            supported.m_additionalAttributes[attribute.Name] = attribute.Value;
          }

          SupportedProtocols.Add (supported);
        }
      }

      // Supported customers
      nodes = root.SelectNodes ("cnc:customer", nsmgr);
      if (nodes != null) {
        foreach (XmlNode nodeTmp in nodes) {
          var supported = new SupportedAttributes ();
          supported.m_mainAttribute = nodeTmp.InnerText;
          foreach (XmlAttribute attribute in nodeTmp.Attributes) {
            supported.m_additionalAttributes[attribute.Name] = attribute.Value;
          }

          SupportedCustomers.Add (supported);
        }
      }

      // Parameters
      nodes = root.SelectNodes ("cnc:parameters/cnc:parameter", nsmgr);
      OldSingleParameterConfiguration = false;
      if (nodes != null) {
        foreach (XmlNode nodeTmp in nodes) {
          AbstractParameter cncParameter = CncParameterFactory.GetCncParameter (nodeTmp);
          if (cncParameter != null) {
            Parameters.Add (cncParameter);
            OldSingleParameterConfiguration |= cncParameter.Name.Equals ("Parameter");
          }
          else {
            // We stop here: one parameter is wrong
            throw new Exception ("parameter unknown: " + nodeTmp.InnerText);
          }
        }
      }

      if (OldSingleParameterConfiguration && Parameters.Count > 1) {
        throw new Exception ("use of \"Parameter\", but several parameters have been parsed");
      }

      // Running modes
      nodes = root.SelectNodes ("cnc:running-modes/cnc:running-mode", nsmgr);
      if (nodes != null) {
        foreach (XmlNode nodeTmp in nodes) {
          switch (nodeTmp.InnerText) {
            case "thread":
              RunningModes.Add (RunningMode.USE_THREAD);
              break;
            case "sta_thread":
              RunningModes.Add (RunningMode.USE_STA_THREAD);
              break;
            case "process":
              RunningModes.Add (RunningMode.USE_PROCESS);
              break;
            default:
              // One parameter is wrong, we log it
              log.ErrorFormat ("unknown running-mode: {0}", nodeTmp.InnerText);
              break;
          }
        }
      }

      // Machine modules
      nodes = root.SelectNodes ("cnc:machinemodules/cnc:machinemodule", nsmgr);
      bool mainMachineModuleOk = false;
      if (nodes != null) {
        foreach (XmlNode nodeTmp in nodes) {
          if (nodeTmp.Attributes["identifier"].Value != null) {
            var module = new Module ();
            module.m_identifier = nodeTmp.Attributes["identifier"].Value;
            if (string.IsNullOrEmpty (module.m_identifier)) {
              if (mainMachineModuleOk) {
                throw new Exception ("too many main machine modules (empty identifier)");
              }

              mainMachineModuleOk = true;
            }
            module.m_description = nodeTmp.InnerText;
            if (nodeTmp.Attributes["autosequenceactivity"] != null) {
              module.m_autoSequenceActivity = nodeTmp.Attributes["autosequenceactivity"].Value;
            }

            // This method is already within a "try / catch"
            if (nodeTmp.Attributes["sequencevariable"] != null) {
              module.m_sequenceVariable = bool.Parse (nodeTmp.Attributes["sequencevariable"].Value);
            }

            if (nodeTmp.Attributes["cyclevariable"] != null) {
              module.m_cycleVariable = bool.Parse (nodeTmp.Attributes["cyclevariable"].Value);
            }

            if (nodeTmp.Attributes["startcyclevariable"] != null) {
              module.m_startCycleVariable = bool.Parse (nodeTmp.Attributes["startcyclevariable"].Value);
            }

            if (nodeTmp.Attributes["detectionmethodvariable"] != null) {
              module.m_detectionMethodVariable = bool.Parse (nodeTmp.Attributes["detectionmethodvariable"].Value);
            }

            if (nodeTmp.Attributes["sequencedetectionmethod"] != null) {
              module.m_sequenceDetectionMethod =
                int.Parse (nodeTmp.Attributes["sequencedetectionmethod"].Value);
            }

            if (nodeTmp.Attributes["cycledetectionmethod"] != null) {
              module.m_cycleDetectionMethod =
                int.Parse (nodeTmp.Attributes["cycledetectionmethod"].Value);
            }

            if (nodeTmp.Attributes["startcycledetectionmethod"] != null) {
              module.m_startCycleDetectionMethod =
                int.Parse (nodeTmp.Attributes["startcycledetectionmethod"].Value);
            }

            Modules.Add (module);
          }
        }
      }
      if (!mainMachineModuleOk) {
        throw new Exception ("no main machine module (with an empty identifier)");
      }

      // Cnc acquisition configuration
      node = root.SelectSingleNode ("cnc:cncacquisition/cnc:acquisitionevery", nsmgr);
      if (node != null) {
        AcquisitionEvery = double.Parse (node.InnerText);
      }

      node = root.SelectSingleNode ("cnc:cncacquisition/cnc:notrespondingtimeout", nsmgr);
      if (node != null) {
        NotRespondingTimeout = double.Parse (node.InnerText);
      }

      node = root.SelectSingleNode ("cnc:cncacquisition/cnc:sleepbeforestart", nsmgr);
      if (node != null) {
        SleepBeforeRestart = double.Parse (node.InnerText);
      }
    }

    void CheckValidity ()
    {
      // It's possible that a file contains no parameters
      IsValid &=
        !String.IsNullOrEmpty (Description) &&
        RunningModes.Count > 0 &&
        Modules.Count > 0;
      if (!IsValid) {
        log.Error ($"CheckValidity: not valid, description={this.Description}, {RunningModes.Count} running modes, {Modules.Count} modules");
      }
    }

    /// <summary>
    /// Load an existing configuration
    /// This method is within a readonly transaction
    /// </summary>
    /// <param name="moma"></param>
    /// <returns>See LoadResult enum</returns>
    public LoadResult Load (IMonitoredMachine moma)
    {
      LoadResult result = LoadResult.SUCCESS;

      // Number of machine modules in the config file
      int machineModuleNumber = Modules.Count;

      // Number of actual machine modules
      int actualNumber = 0;
      foreach (var mamo in moma.MachineModules) {
        if (mamo.CncAcquisition != null) {
          actualNumber++;
        }
      }

      if (actualNumber == 0) {
        return LoadResult.NOT_MONITORED; // Not monitored yet: nothing to load
      }

      if (actualNumber < machineModuleNumber) {
        result |= LoadResult.TOO_FEW_MACHINE_MODULES; // The current configuration has less modules than described in the .xml
      }

      if (actualNumber > machineModuleNumber) {
        result |= LoadResult.TOO_MANY_MACHINE_MODULES; // The current configuration has more modules than described in the .xml
      }

      // Load parameters linked to the cnc acquisition
      var cncAcquisition = moma.MainCncAcquisition;
      if (cncAcquisition == null) {
        foreach (var mamo in moma.MachineModules) {
          if (mamo.CncAcquisition != null) {
            cncAcquisition = mamo.CncAcquisition;
            break;
          }
        }
      }

      if (cncAcquisition != null) {
        if (cncAcquisition.ConfigFile != FileName) {
          return LoadResult.DIFFERENT_XML; // Xml files are different
        }

        // Load configuration related to the acquisition
        AcquisitionEvery = cncAcquisition.Every.TotalSeconds;
        NotRespondingTimeout = cncAcquisition.NotRespondingTimeout.TotalSeconds;
        SleepBeforeRestart = cncAcquisition.SleepBeforeRestart.TotalSeconds;

        var parameters = cncAcquisition.ConfigKeyParams ?? new Dictionary<string, string> ();
        if (!string.IsNullOrEmpty (cncAcquisition.ConfigParameters)) {
          if (!this.Parameters.Any () || (1 == this.Parameters.Count () && this.Parameters.First ().Name.Equals ("Parameter"))) {
            parameters["Parameter"] = cncAcquisition.ConfigParameters;
          }
          else {
            var parray = EnumerableString.ParseListString (cncAcquisition.ConfigParameters);
            for (int i = 0; i < parray.Length; ++i) {
              parameters[$"Param{i + 1}"] = parray[i];
            }
          }
        }

        if (Parameters.Select (x => x.Name).Any (x => !parameters.Keys.Contains (x))) {
          log.Info ($"Load: missing parameters");
        }
        if (parameters.Keys.Any (x => !Parameters.Select (y => y.Name).Contains (x))) {
          log.Info ($"Load: more parameters in database than parameters in configuration");
          foreach (var parameter in parameters.Where (x => !x.Key.StartsWith ("Param") && !this.Parameters.Select (y => y.Name).Contains (x.Key))) {
            var newParameter = new CncParameterAdditional (parameter.Key);
            this.Parameters.Add (newParameter);
          }
          if (parameters.Keys.Any (x => !Parameters.Select (y => y.Name).Contains (x))) {
            return LoadMachineModules (moma) ? LoadResult.TOO_MANY_PARAMETERS :
            LoadResult.TOO_MANY_PARAMETERS | LoadResult.DIFFERENT_MACHINE_MODULES;
          }
          else {
            return LoadMachineModules (moma) ? LoadResult.SUCCESS : LoadResult.DIFFERENT_MACHINE_MODULES;
          }
        }

        try {
          foreach (var cncDocParameter in Parameters) {
            cncDocParameter.SetCncAcquisitionId (cncAcquisition.Id);
            if (parameters.TryGetValue (cncDocParameter.Name, out var parameterValue)) {
              log.Debug ($"Load: parameter, set {cncDocParameter.Name}={parameterValue}");
              cncDocParameter.SetValue (parameterValue);
            }
            else {
              log.Debug ($"Load: missing parameter {cncDocParameter.Name}");
              cncDocParameter.SetValue (null);
            }
          }
        }
        catch (Exception ex) {
          // Wrong parameters, still load modules
          log.Error ($"Load: exception for a parameter", ex);
          ErrorFound = ex.Message;
          return LoadMachineModules (moma) ? LoadResult.WRONG_PARAMETERS :
            LoadResult.WRONG_PARAMETERS | LoadResult.DIFFERENT_MACHINE_MODULES;
        }
      }
      else {
        // The parameters are not linked to an acquisition id yet
        for (int i = 0; i < Parameters.Count; i++) {
          Parameters[i].SetCncAcquisitionId (-1);
        }
      }

      // Load data related to the modules
      if (!LoadMachineModules (moma)) {
        result |= LoadResult.DIFFERENT_MACHINE_MODULES;
      }

      return result;
    }

    bool LoadMachineModules (IMonitoredMachine moma)
    {
      // Load data related to the modules
      foreach (var mamo in moma.MachineModules) {
        if (mamo.CncAcquisition != null) {
          string currentIdentifier = mamo.ConfigPrefix.TrimEnd ('-');

          // Find a corresponding prefix in the xml
          bool found = false;
          foreach (var module in Modules) {
            if (module.m_identifier == currentIdentifier) {
              // Variables
              module.m_sequenceVariableValue = mamo.SequenceVariable;
              module.m_cycleVariableValue = mamo.CycleVariable;
              module.m_startCycleVariableValue = mamo.StartCycleVariable;
              module.m_detectionMethodVariableValue = mamo.DetectionMethodVariable;

              // Detection methods
              module.m_sequenceDetectionMethod = (int)mamo.SequenceDetectionMethod;
              module.m_cycleDetectionMethod = (int)mamo.CycleDetectionMethod;
              module.m_startCycleDetectionMethod = (int)mamo.StartCycleDetectionMethod;

              // Other
              module.m_autoSequenceActivity = ((int)mamo.AutoSequenceActivity == 1) ?
                "Machine" : "MachineModule";

              found = true;
              break;
            }
          }

          if (!found) {
            return false; // Prefix doesn't match
          }
        }
      }

      return true;
    }
  }
}
