// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Text.RegularExpressions;

using Lemoine.Core.Log;
using Lemoine.Net;
using System.Threading.Tasks;

namespace Lem_TestReports
{
  internal enum TestGridColumns
  {
    SELECT = 0,
    NAME = 1,
    URL = 2,
    VIEW = 3,
    MESSAGE = 4,
    ACTION = 5,
    DIFF = 6,
    COPY = 7,
    STATUS = 8,
    DURATION
  }

  internal enum TestReportStatus
  {
    NONE = 1,
    RUNNING = 2,
    CANCELLED = 3,
    ERROR = 4,
    OK = 5,
    WAITING = 6,
    DOWNLOAD_OK = 7,
    TEXT_NOK = 8,
    APPEARANCE_NOK = 9,
    COPY_OK = 10,
    NO_OUT_FILE = 11,
    NO_REF_FILE = 12
}

  public enum ViewerType
  {
    PULSEREPORTING = 1
  }

  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    static readonly int NAME_COLUMN_INDEX = (int)TestGridColumns.NAME;
    static readonly int URL_COLUMN_INDEX = (int)TestGridColumns.URL;

    static readonly Regex REGEX_PULSEREPORTING = new Regex (@"http://(.)*:(\d)+/pulsereporting(/)?(.)*");

    static readonly string DIFFPDF_KEY = "DIFFPDF";
    static readonly string DIFFPDF_DEFAULT = @"C:\Devel\pulsetests\FunctionalTests\bin\diffpdf\diffpdf.exe";
    static readonly string COMPAREPDF_KEY = "COMPAREPDF";
    static readonly string COMPAREPDF_DEFAULT = @"C:\Devel\pulsetests\FunctionalTests\bin\comparepdf\comparepdf.exe";

    static readonly string VIEWER_URL_KEY = @"ViewerUrl";
    static readonly string OUT_DIRECTORY_KEY = @"Out";
    static readonly string REF_DIRECTORY_KEY = @"Ref";
    static readonly string JDBC_SERVER_KEY = @"JdbcServer";
    static readonly string URL_PARAMETERS_KEY = @"UrlParameters";

    static readonly string WEB_VIEWER_KEY = "WebViewer";
    static readonly string TEST_REPORTS_WEB_VIEWER_KEY = "TestReports.WebViewer";
    static readonly string WEB_VIEWER_DEFAULT = @"C:\Program Files\Mozilla Firefox\firefox.exe";

    static readonly string PDF_VIEWER_KEY = "PdfViewer";
    static readonly string TEST_REPORTS_PDF_VIEWER_KEY = "TestReports.PdfViewer";
    static readonly string PDF_VIEWER_DEFAULT = @"C:\Program Files\SumatraPDF\SumatraPDF.exe";


    #region Members
    readonly HttpClient m_httpClient = new HttpClient ();
    IDictionary<DataGridViewRow, ReportDownloader> m_runningDownloaders =
      new Dictionary<DataGridViewRow, ReportDownloader> ();
    bool m_autoRun;
    // use to retrieve previous value of changeViewer_combobox
    int prevIndex_ChangeViewer;
    IDictionary<DataGridViewRow, IList<Parameter>> m_parameterLists = new Dictionary<DataGridViewRow, IList<Parameter>> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MainForm).FullName);

    #region Getters / Setters
    /// <summary>
    /// Auto-diff ?
    /// </summary>
    public bool AutoDiff
    {
      get { return autoDiffCheckBox.Checked; }
      set { autoDiffCheckBox.Checked = value; }
    }

    /// <summary>
    /// Run the tests at start
    /// </summary>
    public bool AutoRun
    {
      get { return m_autoRun; }
      set { m_autoRun = value; }
    }

    public int PrevIndex_ChangeViewer
    {
      get { return prevIndex_ChangeViewer; }
      set { prevIndex_ChangeViewer = value; }
    }

    //    public int ParameterLists {
    //      get {return m_parameterLists;}
    //      set {m_parameterLists = value;}
    //    }

    internal Options Options { get; set; }

    public string ViewerUrl
    {
      get { return viewerUrlTextBox.Text; }
      set { viewerUrlTextBox.Text = value; }
    }

    public string Out
    {
      get { return outDirectoryTextBox.Text; }
      set { outDirectoryTextBox.Text = value; }
    }

    public string Ref
    {
      get { return refDirectoryTextBox.Text; }
      set { refDirectoryTextBox.Text = value; }
    }

    public string JdbcServer
    {
      get { return jdbcServerTextBox.Text; }
      set { jdbcServerTextBox.Text = value; }
    }

    public string UrlParameters
    {
      get { return urlParametersTextBox.Text; }
      set { urlParametersTextBox.Text = value; }
    }

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm ()
    {
      m_httpClient.Timeout = TimeSpan.FromMinutes (20);

      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      //
      // Add constructor code after the InitializeComponent() call.
      //
      SetStatusDelegate = new SetStatus (SetStatusMethod);
      formatComboBox.SelectedIndex = 0;
      changeViewer_comboBox.SelectedIndex = 0;
      prevIndex_ChangeViewer = 0;
      changeViewer_comboBox.Tag = 0;
      if (0 < dataGridView1.Rows.Count) {
        InitializeRow (dataGridView1.Rows[0]);
      }
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods


    void DataGridView1RowsAdded (object sender, DataGridViewRowsAddedEventArgs e)
    {
      DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
      InitializeRow (row);
    }

    void InitializeRow (DataGridViewRow row)
    {
      DataGridViewButtonCell viewButton = (DataGridViewButtonCell)row.Cells[(int)TestGridColumns.VIEW];
      viewButton.Value = "View";
      RunSetStatus (row, TestReportStatus.NONE);
      DataGridViewCheckBoxCell selectCell = (DataGridViewCheckBoxCell)row.Cells[(int)TestGridColumns.SELECT];
      selectCell.Value = true;
    }

    internal TestReportStatus GetRowStatus (DataGridViewRow row)
    {
      DataGridViewCell statusCell = row.Cells[(int)TestGridColumns.STATUS];
      return (TestReportStatus)statusCell.Value;
    }

    async void DataGridView1CellContentClick (object sender, DataGridViewCellEventArgs e)
    {
      if (dataGridView1.Rows.Count <= e.RowIndex) {
        return;
      }

      DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
      DataGridViewCell statusCell = row.Cells[(int)TestGridColumns.STATUS];
      switch (e.ColumnIndex) {
      case (int)TestGridColumns.VIEW:
        ViewClick (row);
        break;
      case (int)TestGridColumns.ACTION:
        if (statusCell.Value.Equals (TestReportStatus.RUNNING)
            || statusCell.Value.Equals (TestReportStatus.WAITING)) {
          CancelClick (row);
        }
        else {
          await RunClickAsync (row, true);
        }
        break;
      case (int)TestGridColumns.DIFF:
        if (statusCell.Value.Equals (TestReportStatus.TEXT_NOK)
            || statusCell.Value.Equals (TestReportStatus.APPEARANCE_NOK)
            || statusCell.Value.Equals (TestReportStatus.DOWNLOAD_OK)) {
          DiffClick (row);
        }
        break;
      case (int)TestGridColumns.COPY:
        if (statusCell.Value.Equals (TestReportStatus.TEXT_NOK)
            || statusCell.Value.Equals (TestReportStatus.APPEARANCE_NOK)
            || statusCell.Value.Equals (TestReportStatus.DOWNLOAD_OK)
            || statusCell.Value.Equals (TestReportStatus.NO_REF_FILE)) {
          CopyClick (row);
        }
        break;
      default:
        break;
      }
    }

    void ViewClick (DataGridViewRow row)
    {
      string baseUrl = (string)row.Cells[URL_COLUMN_INDEX].Value;
      string viewUrl = null;
      string[] baseUrlParts;
      ViewerType viewerType = getViewerType (baseUrl);
      if (viewerType == ViewerType.PULSEREPORTING) {
        baseUrlParts = baseUrl.Split (new char[] { '?' });
        viewUrl = this.ViewerUrl + "viewer" + "?" + baseUrlParts[1];
      }
      else {
        throw new Exception ("Invalid value for ViewerType");
      }
      // Replace in jdbcurl and add urlParameters at the end
      viewUrl = viewUrl.Replace ("JdbcServer", jdbcServerTextBox.Text) + urlParametersTextBox.Text;

      var webViewer = Lemoine.Info.ConfigSet
        .LoadAndGet (TEST_REPORTS_WEB_VIEWER_KEY, WEB_VIEWER_KEY, WEB_VIEWER_DEFAULT);
      ProcessStartInfo startInfo = new ProcessStartInfo (webViewer);
      startInfo.Arguments = viewUrl;
      Process.Start (startInfo);
    }

    async Task RunClickAsync (DataGridViewRow row, bool asynchronous)
    {
      string name = (string)row.Cells[NAME_COLUMN_INDEX].Value;
      if (string.IsNullOrEmpty (name)) {
        MessageBox.Show ("Please specify a test name first", "No test name error",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      string baseUrl = (string)row.Cells[URL_COLUMN_INDEX].Value;
      if (string.IsNullOrEmpty (baseUrl)) {
        MessageBox.Show ("Please specify a test URL first", "No test URL error",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      Uri uri = GetExportUri (baseUrl);
      // Replace jdbcurl is done in GetExportUri - and DO NOT add urlParameters at the end

      try {
        DirectoryInfo di = Directory.CreateDirectory (outDirectoryTextBox.Text);
      }
      catch (Exception e) {
        log.Error ("An error occurs when creating output directory : " + e.Message);
        throw;
      }

      string filename = Path.Combine (outDirectoryTextBox.Text,
                                      name + GetSuffix ());

      ReportDownloader downloader = new ReportDownloader (m_httpClient, this, row);
      m_runningDownloaders[row] = downloader;
      if (asynchronous) {
        try {
          RunSetStatus (row, TestReportStatus.RUNNING);
          await downloader.DownloadFileAsync (uri, filename);
        }
        catch (Exception ex) {
          log.ErrorFormat ("RunClick: " +
                           "{0}",
                           ex);
          RunSetStatus (row, TestReportStatus.ERROR);
          MessageBox.Show ("Error while downloading the report", "Download error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      else {
        try {
          RunSetStatus (row, TestReportStatus.RUNNING);
          downloader.DownloadFile (uri, filename);
          RunSetStatus (row, TestReportStatus.DOWNLOAD_OK);
        }
        catch (Exception ex) {
          log.ErrorFormat ("RunClick: " +
                           "{0}",
                           ex);
          RunSetStatus (row, TestReportStatus.ERROR);
          MessageBox.Show ("Error while downloading the report", "Download error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    void CancelClick (DataGridViewRow row)
    {
      DataGridViewCell statusCell = row.Cells[(int)TestGridColumns.STATUS];
      if (statusCell.Value.Equals (TestReportStatus.RUNNING)) {
        ReportDownloader downloader = m_runningDownloaders[row];
        downloader.Cancel ();
        RunSetStatus (row, TestReportStatus.CANCELLED);
      }
      else if (statusCell.Value.Equals (TestReportStatus.WAITING)) {
        RunSetStatus (row, TestReportStatus.CANCELLED);
      }
      else {
        Debug.Assert (false);
      }
    }

    internal void DiffClick (DataGridViewRow row)
    {
      string outFileName;
      string refFileName;
      if (!GetFileNames (row, out outFileName, out refFileName)) {
        return;
      }

      if (!File.Exists (outFileName)) {
        //MessageBox.Show ("Out file does not exist", "Out file error",
        //                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        RunSetStatus (row, TestReportStatus.NO_OUT_FILE);
        return;
      }
      if (!File.Exists (refFileName)) {
        //MessageBox.Show ("Ref file does not exist", "Ref file error",
        //                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        RunSetStatus (row, TestReportStatus.NO_REF_FILE);
        // Appeare
        return;
      }

      if (0 == formatComboBox.SelectedIndex) { // PDF => try to use diffpdf
        var diffpdfPath = Lemoine.Info.ConfigSet
          .LoadAndGet<string> (DIFFPDF_KEY, DIFFPDF_DEFAULT);
        if (File.Exists (diffpdfPath)) {
          ProcessStartInfo diffStartInfo = new ProcessStartInfo (diffpdfPath);
          diffStartInfo.Arguments = refFileName + " " + outFileName;
          Process.Start (diffStartInfo);
          return;
        }
      }

      // Else run the two files
      var pdfViewer = Lemoine.Info.ConfigSet
        .LoadAndGet (TEST_REPORTS_PDF_VIEWER_KEY, PDF_VIEWER_KEY, PDF_VIEWER_DEFAULT);
      ProcessStartInfo outStartInfo = new ProcessStartInfo (pdfViewer, outFileName);
      Process.Start (outStartInfo);
      ProcessStartInfo refStartInfo = new ProcessStartInfo (pdfViewer, refFileName);
      Process.Start (refStartInfo);
    }

    void CopyClick (DataGridViewRow row)
    {
      string outFileName;
      string refFileName;
      if (GetFileNames (row, out outFileName, out refFileName)) {
        File.Copy (outFileName, refFileName, true);
        RunSetStatus (row, TestReportStatus.COPY_OK);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="string"></param>
    /// <returns>success</returns>
    bool GetFileNames (DataGridViewRow row, out string outFileName, out string refFileName)
    {
      outFileName = null;
      refFileName = null;

      string name = (string)row.Cells[NAME_COLUMN_INDEX].Value;
      if (string.IsNullOrEmpty (name)) {
        MessageBox.Show ("Please specify a test name first", "No test name error",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      if (string.IsNullOrEmpty (outDirectoryTextBox.Text)) {
        MessageBox.Show ("Please specify a out directory first", "No out directory error",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      if (string.IsNullOrEmpty (refDirectoryTextBox.Text)) {
        MessageBox.Show ("Please specify a ref directory first", "No ref directory error",
                         MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      outFileName = Path.Combine (outDirectoryTextBox.Text,
                                  name + GetSuffix ());
      refFileName = Path.Combine (refDirectoryTextBox.Text,
                                  name + GetSuffix ());
      return true;
    }

    #region Status methods
    delegate void SetStatus (DataGridViewRow row, TestReportStatus newStatus);

    void SetStatusMethod (DataGridViewRow row, TestReportStatus newStatus)
    {
      DataGridViewTextBoxCell messageTextBox = (DataGridViewTextBoxCell)row.Cells[(int)TestGridColumns.MESSAGE];
      DataGridViewTextBoxCell durationTextBox = (DataGridViewTextBoxCell)row.Cells[(int)TestGridColumns.DURATION];
      DataGridViewButtonCell actionButton = (DataGridViewButtonCell)row.Cells[(int)TestGridColumns.ACTION];
      DataGridViewButtonCell diffButton = (DataGridViewButtonCell)row.Cells[(int)TestGridColumns.DIFF];
      DataGridViewButtonCell copyButton = (DataGridViewButtonCell)row.Cells[(int)TestGridColumns.COPY];
      DataGridViewCell statusCell = row.Cells[(int)TestGridColumns.STATUS];
      string beginString = (string)durationTextBox.Value;
      TimeSpan maxDuration = new TimeSpan (0, 3, 0);
      switch (newStatus) {
      case TestReportStatus.NONE:
        messageTextBox.Value = "";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Black;
        actionButton.Value = "Run";
        diffButton.Value = "";
        copyButton.Value = "";
        statusCell.Value = TestReportStatus.NONE;
        durationTextBox.Value = "";
        durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        break;
      case TestReportStatus.RUNNING:
        messageTextBox.Value = "Running";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Bold);
        messageTextBox.Style.ForeColor = Color.Black;
        actionButton.Value = "Cancel";
        diffButton.Value = "";
        copyButton.Value = "";
        statusCell.Value = TestReportStatus.RUNNING;
        durationTextBox.Value = (string)(DateTime.UtcNow.ToString ("s") + "Z");
        durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Italic);
        break;
      case TestReportStatus.CANCELLED:
        messageTextBox.Value = "Cancelled";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Black;
        actionButton.Value = "Run again";
        diffButton.Value = "";
        copyButton.Value = "";
        statusCell.Value = TestReportStatus.CANCELLED;
        durationTextBox.Value = "";
        durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        break;
      case TestReportStatus.ERROR:
        messageTextBox.Value = "In error";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Red;
        actionButton.Value = "Run again";
        diffButton.Value = "";
        copyButton.Value = "";
        statusCell.Value = TestReportStatus.ERROR;
        durationTextBox.Value = "";
        durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        break;
      case TestReportStatus.NO_OUT_FILE:
        messageTextBox.Value = "No out";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Red;
        actionButton.Value = "Run again";
        diffButton.Value = "";
        copyButton.Value = "";
        statusCell.Value = TestReportStatus.NO_OUT_FILE;
        durationTextBox.Value = "";
        durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        break;
      case TestReportStatus.NO_REF_FILE:
        messageTextBox.Value = "No ref";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Red;
        actionButton.Value = "Run again";
        diffButton.Value = "";
        copyButton.Value = "Copy";
        statusCell.Value = TestReportStatus.NO_REF_FILE;
        durationTextBox.Value = "";
        durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        break;
      case TestReportStatus.OK:
        messageTextBox.Value = "Ok";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Green;
        actionButton.Value = "Run again";
        diffButton.Value = "";
        copyButton.Value = "";
        statusCell.Value = TestReportStatus.OK;
        if (beginString.Length > 0) {
          try {
            DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                         "yyyy-MM-dd'T'HH:mm:ss'Z'",
                                                         System.Globalization.CultureInfo.InvariantCulture,
                                                         System.Globalization.DateTimeStyles.None);
            //System.Globalization.DateTimeStyles.AssumeUniversal |
            //System.Globalization.DateTimeStyles.AdjustToUniversal);
            TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
            durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
            durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
          }
          catch {
            try {
              DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                System.Globalization.DateTimeStyles.None);
              //System.Globalization.DateTimeStyles.AssumeUniversal |
              //System.Globalization.DateTimeStyles.AdjustToUniversal);
              TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
              durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
              durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
            }
            catch {
              //durationTextBox.Value = "";
              //durationTextBox.Style.Font = new Font(messageTextBox.InheritedStyle.Font, FontStyle.Regular);
            }
          }
        }
        break;
      case TestReportStatus.WAITING:
        messageTextBox.Value = "Waiting";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Italic);
        messageTextBox.Style.ForeColor = Color.Gray;
        actionButton.Value = "Cancel";
        diffButton.Value = "";
        copyButton.Value = "";
        statusCell.Value = TestReportStatus.WAITING;
        durationTextBox.Value = "";
        durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        break;
      case TestReportStatus.DOWNLOAD_OK:
        messageTextBox.Value = "Download Ok";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Green;
        actionButton.Value = "";
        diffButton.Value = "Diff";
        copyButton.Value = "Copy";
        statusCell.Value = TestReportStatus.DOWNLOAD_OK;
        if (beginString.Length > 0) {
          try {
            DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                         "yyyy-MM-dd'T'HH:mm:ss'Z'",
                                                         System.Globalization.CultureInfo.InvariantCulture,
                                                         System.Globalization.DateTimeStyles.None);
            //System.Globalization.DateTimeStyles.AssumeUniversal |
            //System.Globalization.DateTimeStyles.AdjustToUniversal);
            TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
            durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
            durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                               (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
          }
          catch {
            try {
              DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                System.Globalization.DateTimeStyles.None);
              //System.Globalization.DateTimeStyles.AssumeUniversal |
              //System.Globalization.DateTimeStyles.AdjustToUniversal);
              TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
              durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
              durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
            }
            catch {
              //durationTextBox.Value = "";
              //durationTextBox.Style.Font = new Font(messageTextBox.InheritedStyle.Font, FontStyle.Regular);
            }
          }
        }
        break;
      case TestReportStatus.TEXT_NOK:
        messageTextBox.Value = "Text differs";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Orange;
        actionButton.Value = "Run again";
        diffButton.Value = "Diff";
        copyButton.Value = "Copy";
        statusCell.Value = TestReportStatus.TEXT_NOK;
        if (beginString.Length > 0) {
          try {
            DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                         "yyyy-MM-dd'T'HH:mm:ss'Z'",
                                                         System.Globalization.CultureInfo.InvariantCulture,
                                                         System.Globalization.DateTimeStyles.None);
            //System.Globalization.DateTimeStyles.AssumeUniversal |
            //System.Globalization.DateTimeStyles.AdjustToUniversal);
            TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
            durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
            durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
          }
          catch {
            try {
              DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                System.Globalization.DateTimeStyles.None);
              //System.Globalization.DateTimeStyles.AssumeUniversal |
              //System.Globalization.DateTimeStyles.AdjustToUniversal);
              TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
              durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
              durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
            }
            catch {
              //durationTextBox.Value = "";
              //durationTextBox.Style.Font = new Font(messageTextBox.InheritedStyle.Font, FontStyle.Regular);
            }
          }
        }
        break;
      case TestReportStatus.APPEARANCE_NOK:
        messageTextBox.Value = "Appearance differs";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Orange;
        actionButton.Value = "Run again";
        diffButton.Value = "Diff";
        copyButton.Value = "Copy";
        statusCell.Value = TestReportStatus.APPEARANCE_NOK;
        if (beginString.Length > 0) {
          try {
            DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                         "yyyy-MM-dd'T'HH:mm:ss'Z'",
                                                         System.Globalization.CultureInfo.InvariantCulture,
                                                         System.Globalization.DateTimeStyles.None);
            //System.Globalization.DateTimeStyles.AssumeUniversal |
            //System.Globalization.DateTimeStyles.AdjustToUniversal);
            TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
            durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
            durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
          }
          catch {
            try {
              DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                System.Globalization.DateTimeStyles.None);
              //System.Globalization.DateTimeStyles.AssumeUniversal |
              //System.Globalization.DateTimeStyles.AdjustToUniversal);
              TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
              durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
              durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
            }
            catch {
              //durationTextBox.Value = "";
              //durationTextBox.Style.Font = new Font(messageTextBox.InheritedStyle.Font, FontStyle.Regular);
            }
          }
        }
        break;
      case TestReportStatus.COPY_OK:
        messageTextBox.Value = "Copy Ok";
        messageTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font, FontStyle.Regular);
        messageTextBox.Style.ForeColor = Color.Green;
        actionButton.Value = "Run again";
        diffButton.Value = "";
        copyButton.Value = "";
        statusCell.Value = TestReportStatus.COPY_OK;
        if (beginString.Length > 0) {
          try {
            DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                         "yyyy-MM-dd'T'HH:mm:ss'Z'",
                                                         System.Globalization.CultureInfo.InvariantCulture,
                                                         System.Globalization.DateTimeStyles.None);
            //System.Globalization.DateTimeStyles.AssumeUniversal |
            //System.Globalization.DateTimeStyles.AdjustToUniversal);
            TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
            durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                 (int)duration.Hours,
                                                 (int)duration.Minutes,
                                                 (int)duration.Seconds);
            durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
          }
          catch {
            try {
              DateTime beginDateTime = DateTime.ParseExact (beginString,
                                                "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                System.Globalization.DateTimeStyles.None);
              //System.Globalization.DateTimeStyles.AssumeUniversal |
              //System.Globalization.DateTimeStyles.AdjustToUniversal);
              TimeSpan duration = DateTime.UtcNow.Subtract (beginDateTime);
              durationTextBox.Value = string.Format ("{0}:{1:00}:{2:00}",
                                                   (int)duration.Hours,
                                                   (int)duration.Minutes,
                                                   (int)duration.Seconds);
              durationTextBox.Style.Font = new Font (messageTextBox.InheritedStyle.Font,
                                                 (duration.CompareTo (maxDuration) > 0) ? FontStyle.Bold : FontStyle.Regular);
            }
            catch {
              //durationTextBox.Value = "";
              //durationTextBox.Style.Font = new Font(messageTextBox.InheritedStyle.Font, FontStyle.Regular);
            }
          }
        }
        break;
      }

      if (newStatus != TestReportStatus.RUNNING) {
        m_runningDownloaders.Remove (row);
      }
    }

    SetStatus SetStatusDelegate;

    internal void RunSetStatus (DataGridViewRow row, TestReportStatus newStatus)
    {
      if (InvokeRequired) {
        Invoke (SetStatusDelegate,
                new object[] { row, newStatus });
      }
      else {
        SetStatusMethod (row, newStatus);
      }
    }
    #endregion // Status methods

    Uri GetExportUri (string baseUrl)
    {
      ViewerType viewerType = getViewerType (baseUrl);
      if (viewerType == ViewerType.PULSEREPORTING) {
        string[] baseUrlParts = baseUrl.Split (new char[] { '?' });
        string shortenUrl = baseUrlParts[1];
        return new Uri (new Uri (viewerUrlTextBox.Text),
                        "export?__format=" + GetFormat () + "&"
                        + shortenUrl.Replace ("JdbcServer", jdbcServerTextBox.Text));
      }
      return null;
    }

    string GetFormat ()
    {
      switch (formatComboBox.SelectedIndex) {
      case 0: // PDF
        return "pdf";
      case 1: // postscript
        return "postscript";
      default: // PDF
        return "pdf";
      }
    }

    string GetSuffix ()
    {
      switch (formatComboBox.SelectedIndex) {
      case 0: // PDF
        return ".pdf";
      case 1: // postscript
        return ".ps";
      default:
        return ".pdf";
      }
    }

    void NewButtonClick (object sender, EventArgs e)
    {
      dataGridView1.Rows.Clear ();
      if (string.IsNullOrEmpty (Options.ViewerUrl)) {
        this.ViewerUrl = @"http://lctr:8080/pulsereporting/";
      }
      else {
        this.ViewerUrl = Options.ViewerUrl;
      }
      if (string.IsNullOrEmpty (Options.Out)) {
        this.Out = @"C:\Devel\pulsetests\FunctionalTests\out\BirtReports";
      }
      else {
        this.Out = Options.Out;
      }
      if (string.IsNullOrEmpty (Options.Ref)) {
        this.Ref = @"C:\Devel\pulsetests\FunctionalTests\ref\BirtReports";
      }
      else {
        this.Ref = Options.Ref;
      }
      if (string.IsNullOrEmpty (Options.JdbcServer)) {
        this.JdbcServer = "localhost:5432";
      }
      else {
        this.JdbcServer = Options.JdbcServer;
      }
      if (string.IsNullOrEmpty (Options.UrlParameters)) {
        this.UrlParameters = "";
      }
      else {
        this.UrlParameters = Options.UrlParameters;
      }
    }

    void OpenButtonClick (object sender, EventArgs e)
    {
      if (DialogResult.OK == openFileDialog.ShowDialog ()) {
        OpenFile (openFileDialog.FileName);
      }
    }

    public void OpenFile (string fileName)
    {
      dataGridView1.Rows.Clear ();
      using (TextReader textReader = File.OpenText (fileName)) {
        string line = "";
        while (null != (line = textReader.ReadLine ())) {
          if (line.StartsWith ("#")) { // Comment
            continue;
          }

          string[] lineParts = line.Split (new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
          if (2 <= lineParts.Length) { // key=value
            if (lineParts[0].Equals (VIEWER_URL_KEY, StringComparison.InvariantCultureIgnoreCase)) {
              if (string.IsNullOrEmpty (this.Options.ViewerUrl)) {
                this.ViewerUrl = lineParts[1];
              }
              continue;
            }
            else if (lineParts[0].Equals (OUT_DIRECTORY_KEY, StringComparison.InvariantCultureIgnoreCase)) {
              if (string.IsNullOrEmpty (this.Options.Out)) {
                this.Out = lineParts[1];
              }
              continue;
            }

            else if (lineParts[0].Equals (REF_DIRECTORY_KEY, StringComparison.InvariantCultureIgnoreCase)) {
              if (string.IsNullOrEmpty (this.Options.Ref)) {
                this.Ref = lineParts[1];
              }
              continue;
            }

            else if (lineParts[0].Equals (JDBC_SERVER_KEY, StringComparison.InvariantCultureIgnoreCase)) {
              if (string.IsNullOrEmpty (this.Options.JdbcServer)) {
                this.JdbcServer = lineParts[1];
              }
              continue;
            }
            else if (lineParts[0].Equals (URL_PARAMETERS_KEY, StringComparison.InvariantCultureIgnoreCase)) {
              if (string.IsNullOrEmpty (this.Options.UrlParameters)) {
                urlParametersTextBox.Text = lineParts[1];
              }
              continue;
            }
            else {
              int newRowIndex = dataGridView1.Rows.Add ();
              DataGridViewRow row = dataGridView1.Rows[newRowIndex];
              InitializeRow (row);
              row.Cells[(int)TestGridColumns.NAME].Value = lineParts[0];
              row.Cells[(int)TestGridColumns.URL].Value = lineParts[1];
            }
          }
        }
      }

      //changeViewer_comboBox.SelectedIndex = 0;
      /* m_parameterLists.Clear();
      foreach (DataGridViewRow row in dataGridView1.Rows) {
        DataGridViewTextBoxCell urlCell = (DataGridViewTextBoxCell) row.Cells [(int) TestGridColumns.URL];
        String url = (String)urlCell.Value;
        if(!string.IsNullOrEmpty(url)){
          int sepIndex = url.IndexOf('#');
          if(sepIndex != -1){
            string queryString = url.Substring(sepIndex+1);
            m_parameterLists.Add(row, Parameter.retrieveParameterList(queryString, 
            ViewerType.PULSEREPORTING));
          }
        }
      }
      //changeViewer_comboBox.SelectedIndex = 1;
      */

      //ChangeViewer_comboBoxSelectionChangeCommitted(new object(), new EventArgs());
    }

    void SaveButtonClick (object sender, EventArgs e)
    {
      if (DialogResult.OK == saveFileDialog.ShowDialog ()) {
        string fileName = saveFileDialog.FileName;
        using (TextWriter writer = new StreamWriter (fileName)) {
          writer.WriteLine (VIEWER_URL_KEY + "=" + viewerUrlTextBox.Text);
          writer.WriteLine (OUT_DIRECTORY_KEY + "=" + outDirectoryTextBox.Text);
          writer.WriteLine (REF_DIRECTORY_KEY + "=" + refDirectoryTextBox.Text);
          writer.WriteLine (JDBC_SERVER_KEY + "=" + jdbcServerTextBox.Text);
          writer.WriteLine (URL_PARAMETERS_KEY + "=" + urlParametersTextBox.Text);
          writer.WriteLine ();
          foreach (DataGridViewRow row in dataGridView1.Rows) {
            string name = (string)row.Cells[(int)TestGridColumns.NAME].Value;
            string url = (string)row.Cells[(int)TestGridColumns.URL].Value;
            if (!string.IsNullOrEmpty (name) && !string.IsNullOrEmpty (url)) {
              writer.WriteLine (name + "=" + url);
            }
          }
        }
      }
    }

    void OutDirectoryButtonClick (object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty (outDirectoryTextBox.Text)
          && Directory.Exists (outDirectoryTextBox.Text)) {
        folderBrowserDialog.SelectedPath = outDirectoryTextBox.Text;
      }
      if (DialogResult.OK == folderBrowserDialog.ShowDialog ()) {
        outDirectoryTextBox.Text = folderBrowserDialog.SelectedPath;
      }
    }

    void RefDirectoryButtonClick (object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty (refDirectoryTextBox.Text)
          && Directory.Exists (refDirectoryTextBox.Text)) {
        folderBrowserDialog.SelectedPath = refDirectoryTextBox.Text;
      }
      if (DialogResult.OK == folderBrowserDialog.ShowDialog ()) {
        refDirectoryTextBox.Text = folderBrowserDialog.SelectedPath;
      }
    }

    void NoneButtonClick (object sender, EventArgs e)
    {
      foreach (DataGridViewRow row in dataGridView1.Rows) {
        DataGridViewCheckBoxCell selectCell = (DataGridViewCheckBoxCell)row.Cells[(int)TestGridColumns.SELECT];
        selectCell.Value = false;
      }
    }

    void AllButtonClick (object sender, EventArgs e)
    {
      foreach (DataGridViewRow row in dataGridView1.Rows) {
        DataGridViewCheckBoxCell selectCell = (DataGridViewCheckBoxCell)row.Cells[(int)TestGridColumns.SELECT];
        selectCell.Value = true;
      }
    }

    void RunButtonClick (object sender, EventArgs e)
    {
      Run ();
    }

    public void Run ()
    {
      foreach (DataGridViewRow row in dataGridView1.Rows) {
        DataGridViewCheckBoxCell selectCell = (DataGridViewCheckBoxCell)row.Cells[(int)TestGridColumns.SELECT];
        if ((bool)selectCell.Value) {
          string name = (string)row.Cells[(int)TestGridColumns.NAME].Value;
          string url = (string)row.Cells[(int)TestGridColumns.URL].Value;
          if (!string.IsNullOrEmpty (name) && !string.IsNullOrEmpty (url)) {
            RunSetStatus (row, TestReportStatus.WAITING);
            waitStatusTimer.Start ();
          }
        }
      }
    }

    async void WaitStatusTimerTick (object sender, EventArgs e)
    {
      bool stopTimer = true;
      IList<Task> tasks = new List<Task> ();
      foreach (DataGridViewRow row in dataGridView1.Rows) {
        DataGridViewCell statusCell = row.Cells[(int)TestGridColumns.STATUS];
        if (statusCell.Value.Equals (TestReportStatus.WAITING)) {
          if (m_runningDownloaders.Count < parallelNumericUpDown.Value) {
            tasks.Add (RunClickAsync (row, true));
          }
          else {
            stopTimer = false;
          }
        }
      }
      if (stopTimer) {
        waitStatusTimer.Stop ();
      }
      await Task.WhenAll (tasks);
    }

    void MainFormLoad (object sender, EventArgs e)
    {
      if (AutoRun) {
        Run ();
      }
    }

    public void ComparePdf (DataGridViewRow row)
    {
      // - out and ref files
      string outFileName;
      string refFileName;
      if (!GetFileNames (row, out outFileName, out refFileName)) {
        return;
      }

      if (!File.Exists (outFileName)) {
        //MessageBox.Show ("Out file does not exist", "Out file error",
        //                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        RunSetStatus (row, TestReportStatus.NO_OUT_FILE);
        
        return;
      }
      if (!File.Exists (refFileName)) {
        //MessageBox.Show ("Ref file does not exist", "Ref file error",
        //                 MessageBoxButtons.OK, MessageBoxIcon.Error);
        RunSetStatus (row, TestReportStatus.NO_REF_FILE);
        return;
      }

      // - Run comparepdf to compare the text
      ProcessStartInfo startInfo = new ProcessStartInfo ();
      startInfo.FileName = Lemoine.Info.ConfigSet.LoadAndGet<string> (COMPAREPDF_KEY, COMPAREPDF_DEFAULT);
      // First: text only
      startInfo.Arguments = String.Format (@"-c t ""{0}"" ""{1}""",
                                           outFileName, refFileName);
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardError = false;
      startInfo.RedirectStandardOutput = true;
      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
      startInfo.CreateNoWindow = true;

      string standardOutput;
      using (Process process = Process.Start (startInfo)) {
        using (StreamReader reader = process.StandardOutput) {
          standardOutput = reader.ReadToEnd ();
        }

        process.WaitForExit ();

        if (0 != process.ExitCode) {
          Debug.Assert (standardOutput.StartsWith ("Files have different texts."));
          RunSetStatus (row, TestReportStatus.TEXT_NOK);
          return;
        }
      }

      // - Run comparepdf to compare the appearance
      startInfo.Arguments = String.Format (@"-c a ""{0}"" ""{1}""",
                                           outFileName, refFileName);
      using (Process process = Process.Start (startInfo)) {
        using (StreamReader reader = process.StandardOutput) {
          standardOutput = reader.ReadToEnd ();
        }

        process.WaitForExit ();

        if (0 != process.ExitCode) {
          Debug.Assert (standardOutput.StartsWith ("Files look different."));
          RunSetStatus (row, TestReportStatus.APPEARANCE_NOK);
          return;
        }
        else {
          RunSetStatus (row, TestReportStatus.OK);
          return;
        }
      }
    }

    ViewerType getViewerType (String url)
    {
      if (REGEX_PULSEREPORTING.IsMatch (url)) {
        return ViewerType.PULSEREPORTING;
      }
      else {
        throw new Exception ("URL value has unknown format : " + url);
      }
    }


    void ChangeViewer_comboBoxSelectionChangeCommitted (object sender, EventArgs e)
    {
      int index = changeViewer_comboBox.SelectedIndex;
      ViewerType viewerType;
      switch (index) {
      case 0:
        viewerType = ViewerType.PULSEREPORTING;
        break;
      default:
        throw new Exception ("Invalid value for ComboBox selected index");
      }

      Regex regex;
      if (viewerUrlTextBox.Text.Contains ("pulsereporting")) {
        regex = new Regex (@"pulsereporting");
      }
      else {
        throw new Exception ("Invalid value for viewer URL");
      }

      switch (viewerType) {
      case ViewerType.PULSEREPORTING:
        viewerUrlTextBox.Text = regex.Replace (viewerUrlTextBox.Text, "pulsereporting");
        break;
      default:
        throw new Exception ("Invalid value for ViewerType");
      }

      foreach (DataGridViewRow row in dataGridView1.Rows) {
        IList<Parameter> parameterList = null;
        if (m_parameterLists.TryGetValue (row, out parameterList)) {
          DataGridViewTextBoxCell urlCell = (DataGridViewTextBoxCell)row.Cells[(int)TestGridColumns.URL];
          string newQueryString = Parameter.buildViewerUrl (parameterList, viewerType);
          switch (viewerType) {
          case ViewerType.PULSEREPORTING:
            urlCell.Value = viewerUrlTextBox.Text + "viewer?" + newQueryString;
            break;
          default:
            throw new Exception ("Invalid value for ViewerType");
          }
        }
      }

    }


    void ChangeViewer_comboBoxDropDown (object sender, EventArgs e)
    {
      PrevIndex_ChangeViewer = ((ComboBox)sender).SelectedIndex;
    }

  }
}
