// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using Lemoine.Cnc.DataRepository;
using Lemoine.DataReferenceControls;
using Lemoine.DataRepository;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.FileRepository;
using Lemoine.Extensions.Interfaces;
using Lemoine.Core.Plugin;

namespace Lem_CncGUI
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    #region Members
    readonly IExtensionsLoader m_extensionsLoader;
    readonly IAssemblyLoader m_assemblyLoader;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    Lemoine.CncEngine.CncDataHandler m_dataHandler = null;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (MainForm).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm(IExtensionsLoader extensionsLoader, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory)
    {
      m_extensionsLoader = extensionsLoader;
      m_assemblyLoader = assemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;

      InitializeComponent ();
    }
    #endregion

    void OpenButtonClick(object sender, EventArgs e)
    {
      DialogResult result = openFileDialog.ShowDialog ();
      if (result == DialogResult.OK) {
        fileLabel.Text = openFileDialog.FileName;
        XMLFactory factory = new XMLFactory (XmlSourceType.URI, openFileDialog.FileName);
        Repository repository = new Repository (factory);
        m_dataHandler = new Lemoine.CncEngine.CncDataHandler (repository, m_assemblyLoader, m_fileRepoClientFactory, CancellationToken.None);
        timer.Interval = (int)m_dataHandler.Every.TotalMilliseconds;
      }
    }
    
    void SelectButtonClick(object sender, EventArgs e)
    {
      CncAcquisitionDialog dialog = new CncAcquisitionDialog ();
      dialog.Nullable = false;
      dialog.DisplayedProperty = "SelectionText";
      if (DialogResult.OK == dialog.ShowDialog ()) {
        ICncAcquisition cncAcquisition = dialog.SelectedValue;
        fileLabel.Text = cncAcquisition.SelectionText + " (" + cncAcquisition.ConfigFile + ")";
        Lemoine.CncEngine.Acquisition acquisition = new Lemoine.CncEngine.Acquisition (m_extensionsLoader, cncAcquisition, m_assemblyLoader, m_fileRepoClientFactory);
        acquisition.InitDataHandler (CancellationToken.None);
        m_dataHandler = acquisition.CncDataHandler;
        timer.Interval = (int) m_dataHandler.Every.TotalMilliseconds;
      }
    }

    void TimerTick(object sender, EventArgs e)
    {
      if (null != m_dataHandler) {
        m_dataHandler.ProcessTasks (cancellationToken: CancellationToken.None);
        dataGridView.Rows.Clear ();
        foreach (KeyValuePair<string, object> data in m_dataHandler.Data) {
          dataGridView.Rows.Add (new object [] { data.Key, m_dataHandler.GetStringFromKeyValueItem (data) });
        }
      }
    }
    
    void MainFormFormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
    {
      if (null != m_dataHandler) {
        m_dataHandler.Dispose ();
      }
      Application.Exit ();
    }
    
    void ConfigParamButtonClick(object sender, EventArgs e)
    {
      ConfigParamDialog dialog = new ConfigParamDialog ();
      if (DialogResult.OK == dialog.ShowDialog ()) {
        ICncAcquisition cncAcquisition = dialog.CncAcquisition;
        fileLabel.Text = cncAcquisition.SelectionText + " (" + cncAcquisition.ConfigFile + ")";
        CncFileRepoFactory factory = new CncFileRepoFactory (m_extensionsLoader, cncAcquisition, checkedThread: null);
        string localConfigurationFile = $"CncConfigParam-{cncAcquisition.Name}.xml";
        XMLFactory copyFactory = new XMLFactory (XmlSourceType.URI, localConfigurationFile);
        XMLBuilder copyBuilder = new XMLBuilder (localConfigurationFile);
        Repository repository = new Repository (factory, copyBuilder, copyFactory);
        var acquisition = new Lemoine.CncEngine.Acquisition (cncAcquisition.Id, repository, m_assemblyLoader, m_fileRepoClientFactory);
        acquisition.InitDataHandler (CancellationToken.None);
        m_dataHandler = acquisition.CncDataHandler;
        timer.Interval = (int) m_dataHandler.Every.TotalMilliseconds;
      }
    }
  }
}
