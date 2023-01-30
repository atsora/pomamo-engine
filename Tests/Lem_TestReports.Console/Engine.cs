// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Net;

namespace Lem_TestReports.Console
{
  /// <summary>
  /// Engine
  /// </summary>
  public class Engine
  {
    static readonly Regex REGEX_PULSEREPORTING = new Regex (@"http://(.)*:(\d)+/pulsereporting(/)?(.)*");

    static readonly string COMPAREPDF_KEY = "COMPAREPDF";
    static readonly string COMPAREPDF_DEFAULT = @"C:\Devel\pulsetests\FunctionalTests\bin\comparepdf\comparepdf.exe";

    static readonly string LOG_FILE_KEY = $"LogFile";
    static readonly string VIEWER_URL_KEY = @"ViewerUrl";
    static readonly string OUT_DIRECTORY_KEY = @"Out";
    static readonly string REF_DIRECTORY_KEY = @"Ref";
    static readonly string JDBC_SERVER_KEY = @"JdbcServer";
    static readonly string URL_PARAMETERS_KEY = @"UrlParameters";
    static readonly string NUMBER_PARALLEL_EXECUTIONS_KEY = @"NumberExecutions";

    readonly ILog log = LogManager.GetLogger (typeof (Engine).FullName);

    internal enum TestReportStatus
    {
      ERROR = 4,
      OK = 5,
      TEXT_NOK = 8,
      APPEARANCE_NOK = 9,
    }

    public enum ViewerType
    {
      PULSEREPORTING = 1
    }

    IList<SingleTest> m_tests = new List<SingleTest> ();
    int m_freeExecutions = 0;
    HttpClient m_httpClient = new HttpClient ();

    #region Getters / Setters
    internal Options Options { get; set; }

    public string LogFile { get; set; }

    public string ViewerUrl { get; set; }

    public string Out { get; set; }

    public string Ref { get; set; }

    public string JdbcServer { get; set; }

    public string UrlParameters { get; set; }

    public int NumberParallelExecutions { get; set; } = 4; // Default value
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Engine ()
    {
      m_httpClient.Timeout = TimeSpan.FromMinutes (20);
    }
    #endregion // Constructors

    bool IsFreeToRun ()
    {
      var freeExecutions = m_freeExecutions;
      if (freeExecutions <= 0) {
        return false;
      }
      else {
        return freeExecutions == Interlocked.CompareExchange (ref m_freeExecutions, freeExecutions - 1, freeExecutions);
      }
    }

    public async Task<bool> RunAsync (SingleTest singleTest)
    {
      while (!IsFreeToRun ()) {
        await Task.Delay (50);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"RunAsync: {singleTest.Name} is free to run");
      }

      try {
        var name = singleTest.Name;
        if (string.IsNullOrEmpty (name)) {
          await SetStatusAsync (singleTest, TestReportStatus.ERROR, "No test name");
          return false;
        }

        var baseUrl = singleTest.Url;
        if (string.IsNullOrEmpty (baseUrl)) {
          await SetStatusAsync (singleTest, TestReportStatus.ERROR, "No test URL");
          return false;
        }

        Uri uri = GetExportUri (baseUrl);
        // Replace jdbcurl is done in GetExportUri - and DO NOT add urlParameters at the end

        try {
          DirectoryInfo di = Directory.CreateDirectory (this.Out);
        }
        catch (Exception ex) {
          log.Error ($"Run: An error occurs when creating output directory {this.Out}", ex);
          throw;
        }

        string outFilePath = Path.Combine (this.Out, name + GetSuffix ());

        await m_httpClient.DownloadAsync (uri.ToString (), outFilePath);
        return await ComparePdfAsync (singleTest);
      }
      catch (Exception ex) {
        log.Error ($"RunAsync: exception for test {singleTest.Name}", ex);
        await SetStatusAsync (singleTest, TestReportStatus.ERROR, ex.Message);
        return false;
      }
      finally {
        if (log.IsDebugEnabled) {
          log.Debug ($"RunAsync: {singleTest.Name} completed");
        }
        Interlocked.Increment (ref m_freeExecutions);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="string"></param>
    /// <returns>success</returns>
    bool GetFileNames (SingleTest singleTest, out string outFileName, out string refFileName)
    {
      outFileName = null;
      refFileName = null;

      if (string.IsNullOrEmpty (singleTest.Name)) {
        log.Error ($"GetFileNames: name is empty for URL={singleTest.Url}");
        return false;
      }

      if (string.IsNullOrEmpty (this.Out)) {
        log.Error ($"GetFileNames: no out directory");
        return false;
      }
      if (string.IsNullOrEmpty (this.Ref)) {
        log.Error ($"GetFileNames: no ref directory");
        return false;
      }

      outFileName = Path.Combine (this.Out,
                                  singleTest.Name + GetSuffix ());
      refFileName = Path.Combine (this.Ref,
                                  singleTest.Name + GetSuffix ());
      return true;
    }

    Uri GetExportUri (string baseUrl)
    {
      ViewerType viewerType = getViewerType (baseUrl);
      if (viewerType == ViewerType.PULSEREPORTING) {
        string[] baseUrlParts = baseUrl.Split (new char[] { '?' });
        string shortenUrl = baseUrlParts[1];
        return new Uri (new Uri (this.ViewerUrl),
                        "export?__format=" + GetFormat () + "&"
                        + shortenUrl.Replace ("JdbcServer", this.JdbcServer));
      }
      return null;
    }

    string GetFormat ()
    {
      return "pdf";
    }

    string GetSuffix ()
    {
      return ".pdf";
    }

    public void OpenFile (string fileName)
    {
      using (TextReader textReader = File.OpenText (fileName)) {
        string line = "";
        while (null != (line = textReader.ReadLine ())) {
          if (line.StartsWith ("#")) { // Comment
            continue;
          }

          string[] lineParts = line.Split (new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
          if (2 <= lineParts.Length) { // key=value
            if (lineParts[0].Equals (LOG_FILE_KEY, StringComparison.InvariantCultureIgnoreCase)) {
              if (string.IsNullOrEmpty (this.Options.LogFile)) {
                this.LogFile = lineParts[1];
              }
              continue;
            }
            else if (lineParts[0].Equals (VIEWER_URL_KEY, StringComparison.InvariantCultureIgnoreCase)) {
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
                this.UrlParameters = lineParts[1];
              }
              continue;
            }
            else if (lineParts[0].Equals (NUMBER_PARALLEL_EXECUTIONS_KEY, StringComparison.InvariantCultureIgnoreCase)) {
              if (0 == this.Options.NumberParallelExecutions) {
                if (int.TryParse (lineParts[1], out var numberParallelExecutions)) {
                  this.NumberParallelExecutions = numberParallelExecutions;
                }
                else {
                  log.Error ($"OpenFile: {lineParts[1]} is not a valid integer for the number of parallel executions. {this.NumberParallelExecutions} is considered instead");
                }
              }
              continue;
            }
            else {
              var singleTest = new SingleTest {
                Name = lineParts[0],
                Url = lineParts[1]
              };
              m_tests.Add (singleTest);
            }
          }
        }
      }
    }

    void RemoveFile (string path)
    {
      if (File.Exists (path)) {
        try {
          File.Delete (path);
        }
        catch (Exception ex) {
          log.Error ($"RemoveFile: removing {path} failed", ex);
        }
      }
    }

    public async Task<bool> RunAsync ()
    {
      RemoveFile (this.LogFile);
      m_freeExecutions = this.NumberParallelExecutions;
      var tasks = m_tests.Select (x => RunAsync (x));
      await Task.WhenAll (tasks);
      return tasks.All (x => x.Result);
    }

    internal async Task SetStatusAsync (SingleTest test, TestReportStatus newStatus, string additionalMessage = "")
    {
      Debug.Assert (!string.IsNullOrEmpty (this.LogFile));

      // Log file
      try {
        if (!string.IsNullOrEmpty (this.LogFile)) {
          var line = $"{test.Name}: {newStatus} {additionalMessage}";
          await File.AppendAllLinesAsync (this.LogFile, new string[] { line });
        }
      }
      catch (Exception ex) {
        log.Error ($"SetStatusAsync: writing to the log file {this.LogFile} failed", ex);
      }

      // Console
      System.Console.ResetColor ();
      System.Console.Write ($"{test.Name}: ");
      switch (newStatus) {
      case TestReportStatus.OK:
        System.Console.ForegroundColor = ConsoleColor.Green;
        break;
      case TestReportStatus.APPEARANCE_NOK:
      case TestReportStatus.TEXT_NOK:
      case TestReportStatus.ERROR:
        System.Console.ForegroundColor = ConsoleColor.Red;
        break;
      default:
        break;
      }
      System.Console.Write (newStatus);
      System.Console.ResetColor ();
      if (!string.IsNullOrEmpty (additionalMessage)) {
        System.Console.Write ($" {additionalMessage}");
      }
      System.Console.WriteLine ();
    }

    public async Task<bool> ComparePdfAsync (SingleTest singleTest)
    {
      // - out and ref files
      string outFileName;
      string refFileName;
      if (!GetFileNames (singleTest, out outFileName, out refFileName)) {
        await SetStatusAsync (singleTest, TestReportStatus.ERROR, "File names could not be determined");
        return false;
      }

      if (!File.Exists (outFileName)) {
        await SetStatusAsync (singleTest, TestReportStatus.ERROR, "Out file does not exist");
        return false;
      }
      if (!File.Exists (refFileName)) {
        await SetStatusAsync (singleTest, TestReportStatus.ERROR, "Ref file does not exist");
        return false;
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
          await SetStatusAsync (singleTest, TestReportStatus.TEXT_NOK);
          return false;
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
          await SetStatusAsync (singleTest, TestReportStatus.APPEARANCE_NOK);
          return false;
        }
        else {
          await SetStatusAsync (singleTest, TestReportStatus.OK);
          return true;
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
  }
}
