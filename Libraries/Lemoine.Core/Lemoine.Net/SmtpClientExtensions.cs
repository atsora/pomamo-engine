// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Info;
using MailKit.Security;

namespace Lemoine.Net
{
  /// <summary>
  /// Set of MailSettings key
  /// 
  /// These keys are prefixed by net.mail.smtp.network
  /// </summary>
  public enum MailSettingsConfigKey
  {
    /// <summary>
    /// Smtp server host name
    /// </summary>
    Host,
    /// <summary>
    /// Smtp server port
    /// </summary>
    Port,
    /// <summary>
    /// Enable SSL encryption
    /// </summary>
    EnableSsl,
    /// <summary>
    /// Smtp connection user name
    /// </summary>
    UserName,
    /// <summary>
    /// Smtp connection password
    /// </summary>
    Password,
    /// <summary>
    /// Default mail sender
    /// </summary>
    From,
  }

  /// <summary>
  /// SmtpClientExtensions
  /// </summary>
  public static class SmtpClientExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SmtpClientExtensions).FullName);

    /// <summary>
    /// Get a mail settings config key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetConfigKey (this MailSettingsConfigKey key)
    {
      string configKey = "net.mail.smtp.network.";
      switch (key) {
      case MailSettingsConfigKey.Host:
        configKey += "host";
        break;
      case MailSettingsConfigKey.Port:
        configKey += "port";
        break;
      case MailSettingsConfigKey.EnableSsl:
        configKey += "enableSsl";
        break;
      case MailSettingsConfigKey.UserName:
        configKey += "userName";
        break;
      case MailSettingsConfigKey.Password:
        configKey += "password";
        break;
      case MailSettingsConfigKey.From:
        configKey = "net.mail.from";
        break;
      }
      return configKey;
    }

    /// <summary>
    /// Connect with the ConfigSet keys
    /// </summary>
    /// <param name="smtpClient"></param>
    public static void ConnectWithConfigSet (this MailKit.Net.Smtp.SmtpClient smtpClient, CancellationToken cancellationToken = default)
    {
      string host = "";
      { // Host
        try {
          host = Lemoine.Info.ConfigSet
            .Get<string> (MailSettingsConfigKey.Host.GetConfigKey ());
          if (!string.IsNullOrEmpty (host)) {
            log.Info ($"Connect: consider host {host}");
          }
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Warn ("Connect: no host is set for the SMTP client", ex);
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ("Connect: (with deprecated KeyNotFoundException) no host is set for the SMTP client", ex);
        }
      }
      int port = 0;
      { // Port
        try {
          port = Lemoine.Info.ConfigSet
            .Get<int> (MailSettingsConfigKey.Port.GetConfigKey ());
          log.Info ($"Connect: consider port {port}");
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Warn ("Connect: no port is set for the SMTP client", ex);
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ("Connect: (with deprecated exception) no port is set for the SMTP client", ex);
        }
      }
      bool enableSsl = false;
      { // EnableSsl
        try {
          enableSsl = Lemoine.Info.ConfigSet
            .Get<bool> (MailSettingsConfigKey.EnableSsl.GetConfigKey ());
          log.Info ($"Connect: consider enableSsl {enableSsl}");
        }
        catch (ConfigKeyNotFoundException ex) {
          if (log.IsDebugEnabled) {
            log.Debug ("Connect: no enableSsl setting is set", ex);
          }
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ("Connect: (with deprecated KeyNotFoundException) no enableSsl setting is set", ex);
        }
      }
      string userName = "";
      string password = "";
      { // UserName / Password
        try {
          userName = Lemoine.Info.ConfigSet
            .Get<string> (MailSettingsConfigKey.UserName.GetConfigKey ());
          if (!string.IsNullOrEmpty (userName)) {
            password = Lemoine.Info.ConfigSet
              .LoadAndGet<string> (MailSettingsConfigKey.Password.GetConfigKey (),
                                   ""); // Default password: empty string
          }
        }
        catch (ConfigKeyNotFoundException ex) { // Skip the userName
          if (log.IsDebugEnabled) {
            log.Debug ("Connect: user name is not set, skip it", ex);
          }
        }
        catch (KeyNotFoundException ex) { // Skip the userName
          log.Fatal ("Connect: (with deprecated KeyNotFoundException user name is not set, skip it", ex);
        }
      }

      try {
        var options = enableSsl
          ? SecureSocketOptions.StartTlsWhenAvailable
          : SecureSocketOptions.None;
        smtpClient.Connect (host, port: port, options: options, cancellationToken: cancellationToken);
        if (!string.IsNullOrEmpty (userName)) {
          smtpClient.Authenticate (userName, password);
        }
      }
      catch (Exception ex) {
        log.Error ($"Connect: Connect failed with exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Connect with the ConfigSet keys
    /// </summary>
    /// <param name="smtpClient"></param>
    public static async Task ConnectWithConfigSetAsync (this MailKit.Net.Smtp.SmtpClient smtpClient, CancellationToken cancellationToken = default)
    {
      string host = "";
      { // Host
        try {
          host = Lemoine.Info.ConfigSet
            .Get<string> (MailSettingsConfigKey.Host.GetConfigKey ());
          if (!string.IsNullOrEmpty (host)) {
            log.Info ($"Connect: consider host {host}");
          }
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Warn ("Connect: no host is set for the SMTP client", ex);
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ("Connect: (with deprecated KeyNotFoundException) no host is set for the SMTP client", ex);
        }
      }
      int port = 0;
      { // Port
        try {
          port = Lemoine.Info.ConfigSet
            .Get<int> (MailSettingsConfigKey.Port.GetConfigKey ());
          log.Info ($"Connect: consider port {port}");
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Warn ("Connect: no port is set for the SMTP client", ex);
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ("Connect: (with deprecated exception) no port is set for the SMTP client", ex);
        }
      }
      bool enableSsl = false;
      { // EnableSsl
        try {
          enableSsl = Lemoine.Info.ConfigSet
            .Get<bool> (MailSettingsConfigKey.EnableSsl.GetConfigKey ());
          log.Info ($"Connect: consider enableSsl {enableSsl}");
        }
        catch (ConfigKeyNotFoundException ex) {
          if (log.IsDebugEnabled) {
            log.Debug ("Connect: no enableSsl setting is set", ex);
          }
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ("Connect: (with deprecated KeyNotFoundException) no enableSsl setting is set", ex);
        }
      }
      string userName = "";
      string password = "";
      { // UserName / Password
        try {
          userName = Lemoine.Info.ConfigSet
            .Get<string> (MailSettingsConfigKey.UserName.GetConfigKey ());
          if (!string.IsNullOrEmpty (userName)) {
            password = Lemoine.Info.ConfigSet
              .LoadAndGet<string> (MailSettingsConfigKey.Password.GetConfigKey (),
                                   ""); // Default password: empty string
          }
        }
        catch (ConfigKeyNotFoundException ex) { // Skip the userName
          if (log.IsDebugEnabled) {
            log.Debug ("Connect: user name is not set, skip it", ex);
          }
        }
        catch (KeyNotFoundException ex) { // Skip the userName
          log.Fatal ("Connect: (with deprecated KeyNotFoundException user name is not set, skip it", ex);
        }
      }

      try {
        var options = enableSsl
          ? SecureSocketOptions.StartTlsWhenAvailable
          : SecureSocketOptions.None;
        await smtpClient.ConnectAsync (host, port: port, options: options, cancellationToken: cancellationToken);
        if (!string.IsNullOrEmpty (host)) {
          await smtpClient.AuthenticateAsync (userName, password);
        }
      }
      catch (Exception ex) {
        log.Error ($"Connect: Connect failed with exception", ex);
        throw;
      }
    }

  }
}
#endif // !NET40
