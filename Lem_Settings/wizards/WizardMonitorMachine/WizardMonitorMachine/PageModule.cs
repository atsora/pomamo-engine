// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.BaseControls;
using Lemoine.DataRepository;
using Lemoine.FileRepository;
using Lemoine.Core.Log;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of PageModule.
  /// </summary>
  internal partial class PageModule : GenericWizardPage, IWizardPage
  {
    #region Members
    IDictionary<string, CncDocument> m_xmlData = null;
    bool m_modified = false;
    readonly IDictionary<string, string> m_modules = new Dictionary<string, string>();
    readonly IDictionary<string, string> m_deprecatedFiles = new Dictionary<string, string>();
    bool m_preparation = true;
    #endregion // Members
    
    ILog log = LogManager.GetLogger(typeof(PageModule).FullName);
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Data acquisition"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "Please choose an .xml file in the list, allowing you to " +
          "monitor the machine.\n\n" +
          "If you need information about the file, you can read its description or open it.\n\n" +
          "Filters on the top of the page can help you to find the right configuration file.";
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageModule()
    {
      InitializeComponent();
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
      m_preparation = true;

      // Prepare filters?
      bool fullLoad = !Lemoine.Info.PulseInfo.UseFileRepositoryCorba;
      if (fullLoad) {
        baseLayout.RowStyles[0].Height = 24;
        comboMachines.Visible = comboControls.Visible = comboProtocols.Visible = comboCustomer.Visible = true;
      }
      else {
        baseLayout.RowStyles[0].Height = 0;
        comboMachines.Visible = comboControls.Visible = comboProtocols.Visible = comboCustomer.Visible = false;
      }
      
      // Load all acquisitions
      m_modules.Clear();
      m_deprecatedFiles.Clear();
      listModules.ClearItems();
      comboMachines.ClearItems();
      comboControls.ClearItems();
      comboProtocols.ClearItems();
      comboCustomer.ClearItems ();
      textFilter.Text = "";
      
      ICollection<string> filePaths = null;
      try {
        filePaths = FileRepoClient.ListFilesInDirectory("cncconfigs");
      }
      catch (Exception e) {
        log.ErrorFormat("Couldn't read cncconfigs directory: {0}", e);
      }
      
      if (filePaths != null) {
        foreach (string filePath in filePaths) {
          try {
            ProcessFile(fullLoad, filePath);
          }
          catch (Exception e) {
            log.ErrorFormat("Couldn't parse file {0}: {1}", filePath, e);
          }
        }
      }
      
      comboMachines.InsertItem("MACHINES", "", 0);
      comboControls.InsertItem("CONTROLS", "", 0);
      comboProtocols.InsertItem ("PROTOCOLS", "", 0);
      comboCustomer.InsertItem ("CUSTOMERS", "", 0);
      comboMachines.SelectedIndex = 0;
      comboControls.SelectedIndex = 0;
      comboProtocols.SelectedIndex = 0;
      comboCustomer.SelectedIndex = 0;
      
      m_preparation = false;
    }
    
    void ProcessFile(bool fullLoad, string filePath)
    {
      if (filePath.EndsWith(".xml")) {
        string fileName = filePath.Split('\\').Last();
        if (fullLoad) {
          // Analyze the document
          var factory = new FileRepoFactory("cncconfigs", fileName);
          XmlDocument xmlDoc = factory.GetData(cancellationToken: System.Threading.CancellationToken.None);
          var cncDoc = new CncDocument(fileName, xmlDoc);
          if (cncDoc.Deprecated) {
            // If deprecated, the file cannot be selected
            log.DebugFormat ("Initialize: {0} is deprecated, skip it", fileName);
            m_deprecatedFiles[fileName] = cncDoc.AlternativeFile;
          }
          else {
            // The module is added
            m_modules[fileName] = FormatAttributes(cncDoc);
            
            // Add machines
            foreach (var supportedMachine in cncDoc.SupportedMachines) {
              string name = supportedMachine.m_mainAttribute;
              if (!String.IsNullOrEmpty (name) && !comboMachines.ContainsObject (name)) {
                comboMachines.AddItem(name, name);
            }
            }
            
            // Add controls
            foreach (var supportedControl in cncDoc.SupportedControls) {
              string name = supportedControl.m_mainAttribute;
              if (!String.IsNullOrEmpty (name) && !comboControls.ContainsObject (name)) {
                comboControls.AddItem(name, name);
            }
            }
            
            // Add protocols
            foreach (var supportedProtocol in cncDoc.SupportedProtocols) {
              string name = supportedProtocol.m_mainAttribute;
              if (!String.IsNullOrEmpty (name) && !comboProtocols.ContainsObject (name)) {
                comboProtocols.AddItem(name, name);
            }
            }

            // Add customers
            foreach (var supportedCustomer in cncDoc.SupportedCustomers) {
              string name = supportedCustomer.m_mainAttribute;
              if (!String.IsNullOrEmpty (name) && !comboCustomer.ContainsObject (name)) {
                comboCustomer.AddItem (name, name);
            }
          }
          }
        }
        else {
          m_modules[fileName] = "";
        }
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      PopulateModules();
      m_modified = false;
      string fileToFind = data.Get<string>(Item.CONFIG_FILE);
      if (listModules.Texts.Contains (fileToFind)) {
        // Monitored with a selectable file
        listModules.SelectedText = fileToFind;
      }
      else {
        if (!string.IsNullOrEmpty (fileToFind)) {
          // Check if it was a deprecated file
          if (m_deprecatedFiles.ContainsKey (fileToFind)) {
            // Alternative possible?
            if (string.IsNullOrEmpty (m_deprecatedFiles[fileToFind])) {
              // Deprecated file with no alternative
              EmitSpecifyHeader(LemSettingsGlobal.COLOR_WARNING, "Deprecated file '" + fileToFind + "'");
              listModules.SelectedIndex = -1;
            }
            else {
              // Is the alternative selectable?
              if (listModules.Texts.Contains (m_deprecatedFiles[fileToFind])) {
                // The alternative is selected
                EmitSpecifyHeader(LemSettingsGlobal.COLOR_WARNING, "Deprecated file replaced by '" + m_deprecatedFiles[fileToFind] + "'");
                listModules.SelectedText = m_deprecatedFiles[fileToFind];
              }
              else {
                // The alternative is not available
                EmitSpecifyHeader(LemSettingsGlobal.COLOR_WARNING, "The alternative '" + m_deprecatedFiles[fileToFind] + "' is not available");
                listModules.SelectedIndex = -1;
              }
            }
          }
          else {
            // File not deprecated but not in the list
            EmitSpecifyHeader(LemSettingsGlobal.COLOR_WARNING, "'" + fileToFind + "' is not available");
            listModules.SelectedIndex = -1;
          }
        }
        else {
          // Not monitored yet
          listModules.SelectedIndex = -1;
        }
      }
      m_xmlData = data.Get<Dictionary<string, CncDocument>>(Item.XML_DATA);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.CONFIG_FILE, listModules.SelectedText);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      string fileName = data.Get<string>(Item.CONFIG_FILE);
      if (string.IsNullOrEmpty (fileName)) {
        errors.Add("an .xml file has to be selected");
      }
      else {
        if (!m_xmlData.ContainsKey (fileName)) {
          LoadData();
        }
        
        if (!m_xmlData[fileName].IsValid) {
          errors.Add("the .xml file is not valid");
      }
      }
      
      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      var warnings = new List<string>();
      
      if (data.Get<bool>(Item.LOAD_PARAMETERS) || m_modified) {
        if (data.Get<CncDocument>(Item.OLD_XML_DATA_FOR_MODULE) != null &&
            data.Get<CncDocument>(Item.OLD_XML_DATA_FOR_MODULE).FileName == data.Get<string>(Item.CONFIG_FILE)) {
          CncDocument.LoadResult result = LoadParameters(data);
          if (result == CncDocument.LoadResult.TOO_MANY_MACHINE_MODULES) {
            warnings.Add("One or more machine module(s) will be disconnected. No data will be lost.");
        }
        }
        data.Store(Item.LOAD_PARAMETERS, false);
      }
      
      string fileName = data.Get<string>(Item.CONFIG_FILE);
      if (m_xmlData[fileName].Deprecated) {
        warnings.Add("The selected configuration file is deprecated.");
      }
      
      return warnings;
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      // Is there something to configure?
      CncDocument cncDocument = m_xmlData[data.Get<string>(Item.CONFIG_FILE)];
      bool toBeConfigured = false;
      bool advancedUsageAllowed = (ContextManager.UserCategory == LemSettingsGlobal.UserCategory.SUPER_ADMIN);
      foreach (AbstractParameter parameter in cncDocument.Parameters) {
        if (!parameter.Hidden && (advancedUsageAllowed || !parameter.AdvancedUsage)) {
          Control control = parameter.GetControl(false);
          toBeConfigured |= (control != null);
        }
      }
      
      return toBeConfigured ? "PageConfModule" : "PageStamping";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      summary.Add(".xml file: \"" + data.Get<string>(Item.CONFIG_FILE) + "\"");
      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    void LoadData()
    {
      string fileName = listModules.SelectedText;
      
      var factory = new FileRepoFactory("cncconfigs", fileName);
      XmlDocument xmlDoc = factory.GetData(cancellationToken: System.Threading.CancellationToken.None);
      
      m_xmlData[fileName] = new CncDocument(fileName, xmlDoc);
    }
    
    CncDocument.LoadResult LoadParameters(ItemData data)
    {
      CncDocument.LoadResult result = CncDocument.LoadResult.SUCCESS;
      
      if (data.Get<bool>(Item.LOAD_PARAMETERS)) {
        var cncDoc = data.Get<Dictionary<string, CncDocument>>(Item.XML_DATA)[data.Get<string>(Item.CONFIG_FILE)];
        if (cncDoc == null) {
          return CncDocument.LoadResult.DIFFERENT_XML;
        }
        
        // Check if the machine is already monitored
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            var machine = data.Get<IMachine>(Item.MACHINE);
            ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
            var moma = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByMachine(machine);
            
            if (moma != null) {
              // Initialize the cnc document based on the configuration in the database
              result = cncDoc.Load(moma);
            }
          }
        }
      }
      
      return result;
    }
    
    string FormatAttributes(CncDocument cncDoc)
    {
      string strMachines = "";
      foreach (var supportedMachine in cncDoc.SupportedMachines) {
        if (!String.IsNullOrEmpty(supportedMachine.m_mainAttribute)) {
          if (strMachines != "") {
            strMachines += "|";
          }

          strMachines += supportedMachine.m_mainAttribute;
        }
      }
      
      string strControls = "";
      foreach (var supportedControl in cncDoc.SupportedControls) {
        if (!String.IsNullOrEmpty(supportedControl.m_mainAttribute)) {
          if (strControls != "") {
            strControls += "|";
          }

          strControls += supportedControl.m_mainAttribute;
        }
      }
      
      string strProtocols = "";
      foreach (var supportedProtocol in cncDoc.SupportedProtocols) {
        if (!String.IsNullOrEmpty(supportedProtocol.m_mainAttribute)) {
          if (strProtocols != "") {
            strProtocols += "|";
          }

          strProtocols += supportedProtocol.m_mainAttribute;
        }
      }

      string strCustomers = "";
      foreach (var supportedCustomer in cncDoc.SupportedCustomers) {
        if (!String.IsNullOrEmpty (supportedCustomer.m_mainAttribute)) {
          if (strCustomers != "") {
            strCustomers += "|";
          }

          strCustomers += supportedCustomer.m_mainAttribute;
        }
      }

      return strMachines + "#" + strControls + "#" + strProtocols + "#" + strCustomers;
    }
    
    void PopulateModules()
    {
      listModules.ClearItems();
      buttonOpen.Enabled = buttonRead.Enabled = false;
      
      // Filters
      string machine = comboMachines.SelectedValue as String;
      string control = comboControls.SelectedValue as String;
      string protocol = comboProtocols.SelectedValue as String;
      string customer = comboCustomer.SelectedValue as String;
      string name = textFilter.Text;
      
      foreach (var path in m_modules.Keys) {
        bool ok = true;
        
        // First apply the filter in the filepath
        if (!String.IsNullOrEmpty (name)) {
          ok = path.ToLower().Contains(name.ToLower());
        }
        
        // Then look in the attributes
        if (ok) {
          var attributes = m_modules[path].Split('#');
          if (attributes.Length == 4) {
            if (!String.IsNullOrEmpty (machine)) {
              ok = attributes[0].Contains(machine);
            }

            if (ok && !String.IsNullOrEmpty (control)) {
              ok = attributes[1].Contains(control);
            }

            if (ok && !String.IsNullOrEmpty (protocol)) {
              ok = attributes[2].Contains(protocol);
            }

            if (ok && !String.IsNullOrEmpty (customer)) {
              ok = attributes[3].Contains (customer);
          }
        }
        }
        
        if (ok) {
          listModules.AddItem(path);
      }
    }
    }
    #endregion // Private methods
    
    #region Event reactions
    void ButtonReadClick(object sender, EventArgs e)
    {
      string fileName = listModules.SelectedText;
      if (fileName == "") {
        return;
      }
      
      if (!m_xmlData.ContainsKey (fileName)) {
        LoadData();
      }
      
      if (m_xmlData[fileName].IsValid) {
        var dialog = new DialogDescriptionXmlFile(fileName, m_xmlData[fileName]);
        dialog.Show();
      }
      else {
        MessageBoxCentered.Show("The .xml file is not valid", "Warning", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
      }
    }
    
    void ButtonOpenClick(object sender, EventArgs e)
    {
      string fileName = listModules.SelectedText;
      if (fileName == "") {
        return;
      }
      
      if (!m_xmlData.ContainsKey (fileName)) {
        LoadData();
      }
      
      var dialog = new XmlReaderDialog(fileName, m_xmlData[fileName].FullText);
      
      // Metadata highlighted
      dialog.HighlightTextBetween("<description>", "</description>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<supported-machines>", "</supported-machines>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<supported-controls>", "</supported-controls>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<supported-protocols>", "</supported-protocols>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween ("<supported-customers>", "</supported-customers>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<unit>", "</unit>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<parameters>", "</parameters>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<running-modes>", "</running-modes>", Color.BlanchedAlmond, true);
      dialog.HighlightTextBetween("<machinemodules>", "</machinemodules>", Color.BlanchedAlmond, true);
      
      dialog.Show();
    }
    
    void ListModulesItemChanged(string arg1, object arg2)
    {
      m_modified = true;
      buttonOpen.Enabled = buttonRead.Enabled = (arg1 != null);
    }
    
    void ComboMachinesItemChanged(string arg1, object arg2)
    {
      if (!m_preparation) {
        PopulateModules();
    }
    }
    
    void ComboControlsItemChanged(string arg1, object arg2)
    {
      if (!m_preparation) {
        PopulateModules();
    }
    }
    
    void ComboProtocolsItemChanged(string arg1, object arg2)
    {
      if (!m_preparation) {
        PopulateModules();
    }
    }

    private void comboCustomer_ItemChanged (string arg1, object arg2)
    {
      if (!m_preparation) {
        PopulateModules ();
    }
    }

    void TextFilterTextChanged(object sender, EventArgs e)
    {
      if (!m_preparation) {
        PopulateModules();
    }
    }
    #endregion // Event reactions
  }
}
