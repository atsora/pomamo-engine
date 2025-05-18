// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of PageStamping.
  /// </summary>
  internal partial class PageStamping : GenericWizardPage, IWizardPage
  {
    #region Members
    IDictionary<string, CncDocument> m_xmlData = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Stamp variables"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "These variables makes it possible to track the beginning / end " +
"of a cycle (made of a succession of the same operation), and the sequence change within an operation.\n\n" +
"For production plants, the cycle end and the sequence recognition are important.\n\n" +
"For molders (having unique productions), stamping cycle start and end are useless.\n\n" +
"The old configuration, if any, is displayed on the bottom of the page.";
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageStamping ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize (ItemContext context) { }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      // New configuration
      m_xmlData = data.Get<Dictionary<string, CncDocument>> (Item.XML_DATA);
      moduleStampingNew.Initialize (m_xmlData[data.Get<string> (Item.CONFIG_FILE)]);

      // Old configuration
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          var machine = data.Get<IMachine> (Item.MACHINE);
          ModelDAOHelper.DAOFactory.MachineDAO.Lock (machine);
          var moma = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByMachine (machine);
          if (moma == null) {
            // No old configuration
            labelNewConfiguration.Text = "New configuration";
            baseLayout.RowStyles[2].Height = 0;
            baseLayout.RowStyles[3].Height = 0;
            labelOldConfiguration.Hide ();
            moduleStampingOld.Hide ();
          }
          else {
            // Try to mix the old configurations with the new ones
            if (moduleStampingNew.AddOld (moma)) {
              // Ok
              labelNewConfiguration.Text = "Change configuration";
              baseLayout.RowStyles[2].Height = 0;
              baseLayout.RowStyles[3].Height = 0;
              labelOldConfiguration.Hide ();
              moduleStampingOld.Hide ();
            }
            else {
              // Old configurations added below the new ones
              labelNewConfiguration.Text = "New configuration";
              baseLayout.RowStyles[2].Height = 18;
              baseLayout.RowStyles[3].Height = 40;
              labelOldConfiguration.Show ();
              moduleStampingOld.Show ();
              moduleStampingOld.Initialize (moma);
            }
          }
        }
      }
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      moduleStampingNew.SaveValues (m_xmlData[data.Get<string> (Item.CONFIG_FILE)]);
    }

    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName (ItemData data)
    {
      return "PageField";
    }

    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary (ItemData data)
    {
      IList<string> summary = new List<string> ();

      var cncDoc = m_xmlData[data.Get<string> (Item.CONFIG_FILE)];
      foreach (var module in cncDoc.Modules) {
        string str = string.IsNullOrEmpty (module.m_identifier) ?
          "Main module" : module.m_identifier;
        str += "\nStart cycle: " + module.m_startCycleVariableValue;
        str += "\nEnd cycle: " + module.m_cycleVariableValue;
        str += "\nSequence: " + module.m_sequenceVariableValue;
        str += "\nMilestone: " + module.m_milestoneVariableValue;

        if (module.m_detectionMethodVariable) {
          str += "\nDetection method: " + module.m_detectionMethodVariableValue;
        }

        summary.Add (str);
      }

      return summary;
    }
    #endregion // Page methods
  }
}
