// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Net;
using Lemoine.Extensions.Alert;
using MailKit.Net.Smtp;
using MimeKit;

namespace Lemoine.Alert
{
  /// <summary>
  /// Generic abstract class to send E-Mails
  /// </summary>
  [Serializable]
  public abstract class GenericEMailAction : IAction
  {
    static readonly string DEFAULT_SENDER = "user@domain";
    static readonly int SMTP_SEND_MAX_ATTEMPTS = 3;

    #region Members
    [NonSerialized]
    string m_to = "";
    [NonSerialized]
    string m_cc = "";
    [NonSerialized]
    string m_bcc = "";
    [NonSerialized]
    Regex m_configRegex = new Regex (@"{config\.([^}]+)}");
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (GenericEMailAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Default list of recipients
    /// </summary>
    [XmlAttribute ("To")]
    public string To
    {
      get { return m_to; }
      set { m_to = value; }
    }

    /// <summary>
    /// Default CC:
    /// </summary>
    [XmlAttribute ("CC")]
    public string CC
    {
      get { return m_cc; }
      set { m_cc = value; }
    }

    /// <summary>
    /// Default Bcc:
    /// </summary>
    [XmlAttribute ("Bcc")]
    public string Bcc
    {
      get { return m_bcc; }
      set { m_bcc = value; }
    }

    /// <summary>
    /// Applicable week days
    /// </summary>
    [XmlIgnore]
    public Lemoine.Model.WeekDay WeekDays
    {
      get; set;
    }

    /// <summary>
    /// Applicable week days for XML serialization
    /// </summary>
    [XmlAttribute ("WeekDays")]
    public int XmlSerializationWeekDays
    {
      get { return (int)this.WeekDays; }
      set { this.WeekDays = (WeekDay)value; }
    }

    /// <summary>
    /// Applicable time period
    /// </summary>
    [XmlElement ("TimePeriod")]
    public Lemoine.Model.TimePeriodOfDay TimePeriod
    {
      get; set;
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericEMailAction ()
    {
      this.WeekDays = Model.WeekDay.AllDays;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Implementation of <see cref="IAction.Execute" />
    /// </summary>
    /// <param name="data"></param>
    public void Execute (XmlElement data)
    {
      var dateTime = GetDateTime (data);
      if (!CheckActivePeriod (dateTime)) {
        log.Info ($"Execute: action is not active because the data date/time {dateTime} does not match the config");
        return;
      }

      MailboxAddress sender;
      {
        string f = Lemoine.Info.ConfigSet.LoadAndGet<string> ("net.mail.from", DEFAULT_SENDER);
        if (f.Equals (DEFAULT_SENDER)) {
          log.Error ($"Execute: net.mail.from is unknown, => use {DEFAULT_SENDER} by default");
        }
        sender = new MailboxAddress (
#if ATSORA
          "Atsora Tracking",
#else
          "Pomamo",
#endif
          f);
      }

      // - Create the message
      MimeMessage message = null;
      try {
        message = new MimeMessage ();
        message.From.Add (sender);
      }
      catch (Exception ex) {
        log.Error ($"Execute: please check the sender {sender} is valid", ex);
        throw;
      }

      string to = GetTo (data);
      string cc = GetCc (data);
      string bcc = GetBcc (data);
      if (string.IsNullOrEmpty (to) && string.IsNullOrEmpty (cc) && string.IsNullOrEmpty (bcc)) {
        log.Debug ("Execute: message with no recipient, do nothing");
        return;
      }

      if (!string.IsNullOrEmpty (to)) {
        try {
          message.To.AddRange (InternetAddressList.Parse (to));
        }
        catch (Exception ex) {
          log.Error ($"Execute: to={to} is not a valid list of addresses", ex);
        }
      }
      if (!string.IsNullOrEmpty (cc)) {
        try {
          message.Cc.AddRange (InternetAddressList.Parse (cc));
        }
        catch (Exception ex) {
          log.Error ($"Execute: Cc={cc} is not a valid list of addresses", ex);
        }
      }
      if (!string.IsNullOrEmpty (bcc)) {
        try {
          message.Bcc.AddRange (InternetAddressList.Parse (bcc));
        }
        catch (Exception ex) {
          log.Error ($"Execute: Bcc={bcc} is not a valid list of addresses", ex);
        }
      }
      if (log.IsErrorEnabled && (0 == message.To.Count) && (0 == message.Cc.Count) == (0 == message.Bcc.Count)) {
        log.Error ($"Execute: no recipient was set (or they were all invalid)");
      }

      message.Subject = GetSubject (data);
      string body = GetBody (data);

      // Replace {config.*} by the corresponding configuration value
      body = m_configRegex.Replace (body, new MatchEvaluator (ConfigReplacement));

      // TODO: add a mechanism to keep the messages that could not be sent
      if (body.Contains ("<html>")) {
        try {
          try { // Add logo
            var logoFileName = Path.Combine (Lemoine.Info.AssemblyInfo.AbsoluteDirectory, "Logo.png");
            if (!File.Exists (logoFileName)) {
              log.Warn ($"Execute: logo file {logoFileName} does not exist");
              throw new Exception ("Logo file does not exist");
            }
            else { // File.Exists
              if (log.IsDebugEnabled) {
                log.Debug ($"Execute: logo file name is {logoFileName}");
              }
              var bodyBuilder = new BodyBuilder ();
              var image = bodyBuilder.LinkedResources.Add (logoFileName, new ContentType ("image", "png"));
              image.ContentId = "logo";
              bodyBuilder.HtmlBody = body;
              message.Body = bodyBuilder.ToMessageBody ();
            }
          }
          catch (Exception ex) {
            log.Error ("Execute: exception in adding the logo process", ex);
            message.Body = new TextPart (MimeKit.Text.TextFormat.Html) {
              Text = body
            };
          }
        }
        catch (Exception ex) {
          log.Error ("Execute: exception in creating the HTML view", ex);
          throw;
        }
      }
      else {
        message.Body = new TextPart (MimeKit.Text.TextFormat.Plain) {
          Text = body
        };
      }

      log.Info ($"Execute: send e-mail To={message.To} Subject={message.Subject} Body={message.Body}");

      // - Send it
      // try several times with delay. Max 3 times. Initial delay 2 seconds, double time.
      int attemptsNumber = 1;
      int waitDurationMs = 2000; // initial wait 2 seconds
      bool failAttempt;

      do {
        try {
          failAttempt = false;
          using (var smtpClient = new SmtpClient ()) {
            smtpClient.ConnectWithConfigSet ();
            smtpClient.Send (message);
            smtpClient.Disconnect (true);
          }
        }
        catch (Exception ex) {
          failAttempt = true;
          attemptsNumber++;
          if (attemptsNumber <= SMTP_SEND_MAX_ATTEMPTS) {
            log.Warn ($"Execute: Send to {to} failed, attempt={attemptsNumber}, retry", ex);
            Thread.Sleep (waitDurationMs);
            waitDurationMs *= 2;
          }
          else {
            log.Error ($"Execute: Send to {to} failed, last attempt={attemptsNumber}", ex);
            throw;
          }
        }
      } while (failAttempt);
    }

    static string ConfigReplacement (Match m)
    {
      if (m.Groups[1].Success) {
        string configKey = m.Groups[1].Value;
        object configValue;
        try {
          configValue = Lemoine.Info.ConfigSet.Get<string> (configKey);
          return configValue.ToString ();
        }
        catch (System.Collections.Generic.KeyNotFoundException) {
          string result = $"{m.Value}:notfound({configKey})";
          log.Error ($"ConfigReplacement: configuration key {configKey} was not found");
          return result;
        }
        catch (Exception ex) {
          string result = $"{m.Value}:exception({ex.Message})";
          log.Error ($"ConfigReplacement: exception => return {result}", ex);
          return result;
        }
      }
      else {
        string result = m.Value + ":nomatch";
        log.Error ($"ConfigReplacement: first group does not match => return {result}");
        return m.Value;
      }
    }

    /// <summary>
    /// Build the subject of the Email from the Xml data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public abstract string GetSubject (XmlElement data);

    /// <summary>
    /// Build the body of the Email from the Xml data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public abstract string GetBody (XmlElement data);

    /// <summary>
    /// Build the recipient To: of the Email from the Xml data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public virtual string GetTo (XmlElement data)
    {
      return m_to;
    }

    /// <summary>
    /// Build the recipient Cc: of the Email from the Xml data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public virtual string GetCc (XmlElement data)
    {
      return m_cc;
    }

    /// <summary>
    /// Build the recipient Bcc: of the Email from the Xml data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public virtual string GetBcc (XmlElement data)
    {
      return m_bcc;
    }

    /// <summary>
    /// Date/time that is associated to data
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public virtual DateTime GetDateTime (XmlElement data)
    {
      return DateTime.UtcNow;
    }

    /// <summary>
    /// Check if a configuration is active at the specified UTC date/time
    /// </summary>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    bool CheckActivePeriod (DateTime dateTime)
    {
      Debug.Assert (DateTimeKind.Utc == dateTime.Kind);
      Debug.Assert (0 < dateTime.Ticks);

      // - WeekDay
      DateTime day = dateTime.ToLocalTime ().Date;
      if (!this.WeekDays.HasFlagDayOfWeek (day.DayOfWeek)) {
        log.DebugFormat ("CheckActivePeriod: " +
                         "for dateTime={0}, dayOfWeek={1} does not match config",
                         dateTime, day.DayOfWeek);
        return false;
      }

      // - TimePeriod
      if (false == this.TimePeriod.IsFullDay ()) {
        DateTime timePeriodBegin = day.Add (this.TimePeriod.Begin).ToUniversalTime ();
        if (dateTime < timePeriodBegin) {
          log.DebugFormat ("CheckActivityPeriod: " +
                           "for dateTime={0} localDateTime={1} time is before {2} " +
                           "=> config does not match",
                           dateTime, dateTime.ToLocalTime (), this.TimePeriod.Begin);
          return false;
        }
        DateTime timePeriodEnd = day.Add (this.TimePeriod.EndOffset).ToUniversalTime ();
        if (timePeriodEnd < dateTime) {
          log.DebugFormat ("CheckActivityPeriod: " +
                           "for dateTime={0} localDateTime={1} time is after {2} " +
                           "=> config does not match",
                           dateTime, dateTime.ToLocalTime (), this.TimePeriod.End);
          return false;
        }
      }

      return true;
    }
#endregion // Methods
  }
}
