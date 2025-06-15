// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Net;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MimeKit;

namespace ReportWizardCli
{
  /// <summary>
  /// Worker
  /// </summary>
  public class Worker
    : IHostedService
  {
    static readonly string TIMEOUT_KEY = "ReportWizardCli.Download.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromMinutes (5);

    static readonly string DEFAULT_SENDER = "user@domain";
    static readonly int SMTP_SEND_MAX_ATTEMPTS = 3;

    readonly ILog log = LogManager.GetLogger (typeof (Worker).FullName);

    readonly IApplicationInitializer m_applicationInitializer;
    readonly IHostApplicationLifetime m_appLifeTime;
    readonly HttpClient m_httpClient;
    readonly Options m_options;
    readonly IniConfigInitializer m_iniConfigInitializer;

    /// <summary>
    /// Constructor
    /// </summary>
    public Worker (IApplicationInitializer applicationInitializer, IHostApplicationLifetime appLifeTime, HttpClient httpClient, Options options, IniConfigInitializer iniConfigInitializer)
    {
      m_applicationInitializer = applicationInitializer;
      m_appLifeTime = appLifeTime;
      m_httpClient = httpClient;
      m_options = options;
      m_iniConfigInitializer = iniConfigInitializer;

      m_httpClient.Timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      appLifeTime.ApplicationStarted.Register (OnStarted);
      appLifeTime.ApplicationStopping.Register (OnStopping);
      appLifeTime.ApplicationStopped.Register (OnStopped);
    }

    public async Task StartAsync (CancellationToken cancellationToken)
    {
      log.Debug ("StartAsync");

      var path = m_options.FilePath;
      if (!File.Exists (path)) {
        log.Error ($"StartAsync: file {path} does not exist");
        throw new FileNotFoundException (message: "prw file not found", fileName: path);
      }
      cancellationToken.ThrowIfCancellationRequested ();

      var startDateTime = DateTime.UtcNow;

      await m_applicationInitializer.InitializeApplicationAsync (cancellationToken);

      cancellationToken.ThrowIfCancellationRequested ();

      await m_iniConfigInitializer.InitializeApplicationAsync (cancellationToken);

      cancellationToken.ThrowIfCancellationRequested ();

      try {
        var reportTitle = Lemoine.Info.ConfigSet.Get<string> (GetConfigKey ("InputFile", "remote"));
        var inputUrl = Lemoine.Info.ConfigSet.Get<string> (GetConfigKey ("InputURL", "url"));
        var format = Lemoine.Info.ConfigSet.Get<string> (GetConfigKey ("OutputFormat", "format"));
        var outputPath = Lemoine.Info.ConfigSet.Get<string> (GetConfigKey ("OutputFile", "file")).Replace (@"\\", @"\");
        var sendEmail = Lemoine.Info.ConfigSet.Get<bool> (GetConfigKey ("Email", "sendemail"));
        var to = Lemoine.Info.ConfigSet.Get<string> (GetConfigKey ("Email", "emailto"));
        var cc = Lemoine.Info.ConfigSet.Get<string> (GetConfigKey ("Email", "emailcc"));

        var exportUrl = GetExportUrl (inputUrl, format);

        try {
          await m_httpClient.DownloadAsync (exportUrl, outputPath, cancellationToken);
        }
        catch (Exception ex) {
          log.Error ($"StartAsync: download {exportUrl} into {outputPath} failed for report {reportTitle}", ex);
          throw;
        }
        cancellationToken.ThrowIfCancellationRequested ();

        if (sendEmail) {
          var from = Lemoine.Info.ConfigSet.LoadAndGet<string> ("net.mail.from", DEFAULT_SENDER);
          if (from.Equals (DEFAULT_SENDER)) {
            log.Error ($"StartAsync: net.mail.from is unknown, => use {DEFAULT_SENDER} by default");
          }
          var sender = new MailboxAddress ("Atsora Tracking", from);

          MimeMessage message;
          try {
            message = new MimeMessage ();
            message.From.Add (sender);
          }
          catch (Exception ex) {
            log.Error ($"StartAsync: please check the sender {sender} is valid", ex);
            throw;
          }

          if (!string.IsNullOrEmpty (to)) {
            try {
              message.To.AddRange (InternetAddressList.Parse (to));
            }
            catch (Exception ex) {
              log.Error ($"StartAsync: to={to} is not a valid list of addresses", ex);
            }
          }
          if (!string.IsNullOrEmpty (cc)) {
            try {
              message.Cc.AddRange (InternetAddressList.Parse (cc));
            }
            catch (Exception ex) {
              log.Error ($"StartAsync: Cc={cc} is not a valid list of addresses", ex);
            }
          }

          message.Subject = $"Atsora Report {reportTitle}";

          var bodyString = $"""
Please find attached the report your requested.

The requested URL is {inputUrl}

The requested format is {format}
""";
          var body = new TextPart (MimeKit.Text.TextFormat.Plain) {
            Text = bodyString
          };

          cancellationToken.ThrowIfCancellationRequested ();

          var mime = GetMime (format);
          var attachment = new MimePart (mime.Item1, mime.Item2) {
            Content = new MimeContent (File.OpenRead (outputPath), ContentEncoding.Default),
            ContentDisposition = new ContentDisposition (ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = Path.GetFileName (outputPath)
          };

          var multipart = new MimeKit.Multipart("mixed") {
            body,
            attachment
          };

          message.Body = multipart;

          cancellationToken.ThrowIfCancellationRequested ();

          log.Info ($"StartAsync: send e-mail To={message.To} Subject={message.Subject}");

          // - Send it
          // try several times with delay. Max 3 times. Initial delay 2 seconds, double time.
          int attemptsNumber = 1;
          int waitDurationMs = 2000; // initial wait 2 seconds
          bool failAttempt;

          do {
            cancellationToken.ThrowIfCancellationRequested ();
            try {
              failAttempt = false;
              using (var smtpClient = new SmtpClient ()) {
                smtpClient.ConnectWithConfigSet (cancellationToken);
                cancellationToken.ThrowIfCancellationRequested ();
                smtpClient.Send (message, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested ();
                smtpClient.Disconnect (true, cancellationToken);
              }
            }
            catch (Exception ex) {
              cancellationToken.ThrowIfCancellationRequested ();
              failAttempt = true;
              attemptsNumber++;
              if (attemptsNumber <= SMTP_SEND_MAX_ATTEMPTS) {
                log.Warn ($"StartAsync: Send to {to} failed, attempt={attemptsNumber}, retry", ex);
                await Task.Delay (waitDurationMs, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested ();
                waitDurationMs *= 2;
              }
              else {
                log.Error ($"StartAsync: Send to {to} failed, last attempt={attemptsNumber}", ex);
                throw;
              }
            }
          } while (failAttempt);
        }
      }
      catch (Exception ex) {
        log.Error ($"StartAsync: exception", ex);
      }
      finally {
        m_appLifeTime.StopApplication ();
      }

      var duration = DateTime.UtcNow.Subtract (startDateTime);
      log.Info ($"StartAsync: process time is {duration}");
    }

    static string GetConfigKey (string section, string key)
    {
      return $"ReportWizardCli.{section}.{key}";
    }

    static string GetExportUrl (string url, string format)
    {
      var exportUrl = url.Replace ("viewer", "export");
      var formatString = format.ToUpperInvariant () switch {
        "HTML" => "html",
        "PDF" => "pdf",
        "PS" => "postscript",
        "DOC" => "doc",
        "XLS" => "xls",
        _ => format.ToLowerInvariant ()
      };
      exportUrl += "&__format=" + formatString;
      return exportUrl;
    }

    static (string, string) GetMime (string format) => format switch {
      "HTML" => ("text", "html"),
      "PDF" => ("application", "pdf"),
      "PS" => ("application", "postscript"),
      "DOC" => ("application", "msword"),
      "DOCX" => ("application", "vnd.openxmlformats-officedocument.wordprocessingml.document"),
      "CSV" => ("text", "csv"),
      "XLS" => ("application", "vnd.ms-excel"),
      "XLSX" => ("application", "vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
      _ => ("application", format.ToLowerInvariant ())
    };

    public Task StopAsync (CancellationToken cancellationToken)
    {
      log.Debug ("StopAsync");

      return Task.CompletedTask;
    }

    private void OnStarted ()
    {
      log.Debug ("OnStarted");
    }

    private void OnStopping ()
    {
      log.Debug ("OnStopping");
    }

    private void OnStopped ()
    {
      log.Debug ("OnStopped");
    }
  }
}
