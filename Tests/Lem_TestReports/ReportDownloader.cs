// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Lemoine.Core.Log;
using Lemoine.Net;

namespace Lem_TestReports
{
  /// <summary>
  /// Description of ReportDownloader.
  /// </summary>
  public class ReportDownloader
  {
    #region Members
    readonly HttpClient m_httpClient;
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    MainForm m_mainForm;
    DataGridViewRow m_row;
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (ReportDownloader).FullName);

    #region Getters / Setters
    /// <summary>
    /// DataGridViewRow
    /// </summary>
    DataGridViewRow Row {
      get { return m_row; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReportDownloader (HttpClient httpClient, MainForm mainForm, DataGridViewRow row)
    {
      m_httpClient = httpClient;
      m_mainForm = mainForm;
      m_row = row;
    }
    #endregion // Constructors

    public void DownloadFile (Uri address, string fileName)
    {
      try {
        m_httpClient.Download (address.ToString (), fileName);
      }
      catch (Exception ex) {
        log.Error ($"DownloadFileAsync: exception for {address} to {fileName}", ex);
        MessageBox.Show ("Error while downloading the report", "Download error",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
        m_mainForm.RunSetStatus (m_row, TestReportStatus.ERROR);
        return;
      }
      m_mainForm.RunSetStatus (m_row, TestReportStatus.DOWNLOAD_OK);
      m_mainForm.ComparePdf (m_row);
      if (m_mainForm.AutoDiff && m_mainForm.GetRowStatus (m_row) != TestReportStatus.OK) {
        m_mainForm.DiffClick (m_row);
      }
    }

    public async Task DownloadFileAsync (Uri address, string fileName)
    {
      try {
        await m_httpClient.DownloadAsync (address.ToString (), fileName, m_cancellationTokenSource.Token);
      }
      catch (Exception ex) {
        log.Error ($"DownloadFileAsync: exception for {address} to {fileName}", ex);
        MessageBox.Show ("Error while downloading the report", "Download error",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
        m_mainForm.RunSetStatus (m_row, TestReportStatus.ERROR);
        return;
      }
      m_mainForm.RunSetStatus (m_row, TestReportStatus.DOWNLOAD_OK);
      m_mainForm.ComparePdf (m_row);
      if (m_mainForm.AutoDiff && m_mainForm.GetRowStatus (m_row) != TestReportStatus.OK) {
        m_mainForm.DiffClick (m_row);
      }
    }

    public void Cancel ()
    {
      m_cancellationTokenSource.Cancel ();
    }
  }
}
