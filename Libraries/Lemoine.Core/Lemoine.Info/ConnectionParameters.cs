// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Info;
using Lemoine.Core.Log;
using System.Text.RegularExpressions;

namespace Lemoine.Info
{
  /// <summary>
  /// Description of ConnectionParameters.
  /// </summary>
  public class ConnectionParameters
  {
    static readonly Regex DB_CONNECTION_REGEX = new Regex (@"^((?'username'\w+)(:(?'password'\w+))?@)?(?'host'[^/@:]+)(:(?'port'\d+))?(/(?'database'[-_A-Za-z0-9]+))?$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
    static readonly string HOST_GROUP = "host";
    static readonly string PORT_GROUP = "port";
    static readonly string DATABASE_GROUP = "database";
    static readonly string USERNAME_GROUP = "username";
    static readonly string PASSWORD_GROUP = "password";

    static readonly string PORT_DEFAULT = "5432";
    static readonly string USERNAME_DEFAULT = Constants.DEFAULT_DATABASE_USER;
    static readonly string PASSWORD_DEFAULT = Constants.DEFAULT_DATABASE_PASSWORD;
    static readonly string DATABASE_DEFAULT = Constants.DEFAULT_DATABASE_NAME;

    static readonly string DEFAULT_DSN_NAME_KEY = "DefaultDSNName";
    static readonly string DEFAULT_DSN_NAME_DEFAULT = Constants.DEFAULT_DSN_NAME;

    static readonly string DEV_KEY = "dev";
    static readonly bool DEV_DEFAULT = false;

    static readonly string DEV_SUFFIX = "-dev";

    #region Members
    string m_dsnName = Constants.DEFAULT_DSN_NAME;
    string m_server = "localhost";
    string m_port = PORT_DEFAULT;
    string m_database = DATABASE_DEFAULT;
    string m_username = USERNAME_DEFAULT;
    string m_password = PASSWORD_DEFAULT;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (ConnectionParameters).FullName);

    #region Getters / Setters
    /// <summary>
    /// DSN Name
    /// </summary>
    public string DsnName
    {
      get { return m_dsnName; }
      set { m_dsnName = value; }
    }

    /// <summary>
    /// Server name
    /// </summary>
    public string Server
    {
      get { return m_server; }
      set {
        m_server = value;
        Updated (this, new System.EventArgs ());
      }
    }

    /// <summary>
    /// Port
    /// </summary>
    public string Port
    {
      get { return m_port; }
      set {
        m_port = value;
        Updated (this, new System.EventArgs ());
      }
    }

    /// <summary>
    /// Database name
    /// </summary>
    public string Database
    {
      get { return m_database; }
      set {
        m_database = value;
        Updated (this, new System.EventArgs ());
      }
    }

    /// <summary>
    /// User name
    /// </summary>
    public string Username
    {
      get { return m_username; }
      set {
        m_username = value;
        Updated (this, new System.EventArgs ());
      }
    }

    /// <summary>
    /// Password
    /// </summary>
    public string Password
    {
      get { return m_password; }
      set {
        m_password = value;
        Updated (this, new System.EventArgs ());
      }
    }

    /// <summary>
    /// Connection string
    /// </summary>
    public virtual string ConnectionString
    {
      get {
        var connectionString = "Server=" + Server + ";" +
      "Port=" + Port + ";" +
      "Database=" + Database + ";" +
      "User ID=" + Username + ";" +
      "Password=" + Password + ";SSL Mode=Prefer;Trust Server Certificate=true;";
        if (log.IsDebugEnabled) {
          log.Debug ($"ConnectionString.Get: return {connectionString}");
        }
        return connectionString;
      }
    }

    /// <summary>
    /// ODBC connection string
    /// </summary>
    public string OdbcConnectionString
    {
      get {
        return "DSN=" + DsnName + ";" +
      "UID=" + Username + ";" +
      "Pwd=" + Password + ";";
      }
    }
    #endregion

    #region Events
    /// <summary>
    /// A property (except DsnName) has been updated
    /// </summary>
    public event EventHandler Updated;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ConnectionParameters ()
    {
      this.Updated += new EventHandler (OnConnectionParametersUpdated);
    }

    /// <summary>
    /// Constructor from a DSN Name
    /// </summary>
    /// <param name="dsnName"></param>
    /// <exception cref="ArgumentException">The given DSN name does not exist in registry</exception>
    public ConnectionParameters (string dsnName)
    {
      this.DsnName = dsnName;
      // LoadParameters can NOT be called here

      this.Updated += new EventHandler (OnConnectionParametersUpdated);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the connection string with a specified application name
    /// </summary>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public string GetConnectionString (string applicationName)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetConnectionString: applicationName={applicationName} /B");
      }
      var connectionString = this.ConnectionString;
      if (!string.IsNullOrEmpty (applicationName)) {
        connectionString = connectionString.TrimEnd (new char[] { ';' });
        connectionString += ";ApplicationName=" + applicationName + ";";
      }
      return connectionString;
    }

    /// <summary>
    /// Default delegate method for the Updated event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnConnectionParametersUpdated (object sender,
                                                          System.EventArgs e)
    {
      // Do nothing
    }

    /// <summary>
    /// Load the GDB parameters
    /// </summary>
    /// <returns>Success</returns>
    public bool LoadGDBParameters ()
    {
      m_dsnName = ConfigSet.LoadAndGet<string> (DEFAULT_DSN_NAME_KEY, DEFAULT_DSN_NAME_DEFAULT);

      return LoadParameters ();
    }

    /// <summary>
    /// Load from the registry a given DSN parameters
    /// </summary>
    /// <param name="dsnName"></param>
    /// <returns></returns>
    public bool LoadParameters (string dsnName)
    {
      this.DsnName = dsnName;
      return LoadParameters ();
    }

    /// <summary>
    /// Load the parameters in registry for the DSN Name in properties
    /// </summary>
    /// <returns>Success</returns>
    public bool LoadParameters ()
    {
      bool dev = Lemoine.Info.ConfigSet.LoadAndGet<bool> (DEV_KEY,
                                                          DEV_DEFAULT);
      if (dev) {
        if (LoadParametersWithSuffix (DEV_SUFFIX)) {
          log.Info ($"LoadParameters: consider suffix {DEV_SUFFIX}");
          return true;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"LoadParameters: no dsn name with suffix {DEV_SUFFIX}");
          }
        }
      }

      return LoadParametersWithSuffix ("");
    }

    bool LoadParametersWithSuffix (string suffix)
    {
      string dsnNameWithSuffix = this.m_dsnName;
      if (!string.IsNullOrEmpty (suffix)) {
        dsnNameWithSuffix += suffix;
      }

      var configKeyPrefix = "DbConnection";
      if (!string.IsNullOrEmpty (dsnNameWithSuffix)) {
        var configKey = $"{configKeyPrefix}.{dsnNameWithSuffix}";
        if (LoadParametersFromConfigKey (configKey)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"LoadParametersWithSuffix: success with the full config key {configKey}");
          }
          return true;
        }
      }

      if (string.IsNullOrEmpty (suffix)
        && (string.IsNullOrEmpty (dsnNameWithSuffix)
          || dsnNameWithSuffix.Equals (DEFAULT_DSN_NAME_DEFAULT, StringComparison.InvariantCultureIgnoreCase))) {
        if (LoadParametersFromConfigKey (configKeyPrefix)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"LoadParametersWithSuffix: success with default dsn name config key {configKeyPrefix}");
          }
          return true;
        }
      }

      if (log.IsWarnEnabled) {
        log.Warn ($"LoadParametersWithSuffix: no valid config key for DSN={m_dsnName} and suffix={suffix}");
      }
      return false;
    }

    bool LoadParametersFromConfigKey (string configKey)
    {
      // Get the properties of the connection in configset
      try {
        var configValue = Lemoine.Info.ConfigSet.Get<string> (configKey);
        var match = DB_CONNECTION_REGEX.Match (configValue);
        if (!match.Success) {
          log.Error ($"LoadParametersWithSuffix: configuration value {configValue} for key {configKey} does not match the regex");
          return false;
        }

        var hostGroup = match.Groups[HOST_GROUP];
        if (hostGroup.Success) {
          m_server = hostGroup.Value;
        }
        else {
          log.Fatal ($"LoadParametersWithSuffix: no host in configuration value {configValue} for key {configKey}, which is unexpected");
          return false;
        }

        var portGroup = match.Groups[PORT_GROUP];
        if (portGroup.Success) {
          m_port = portGroup.Value;
        }
        else {
          log.Debug ("LoadParametersWithSuffix: default port");
          m_port = PORT_DEFAULT;
        }

        var usernameGroup = match.Groups[USERNAME_GROUP];
        if (usernameGroup.Success) {
          m_username = usernameGroup.Value;
        }
        else {
          log.Debug ("LoadParametersWithSuffix: default username");
          m_username = USERNAME_DEFAULT;
        }

        var passwordGroup = match.Groups[PASSWORD_GROUP];
        if (passwordGroup.Success) {
          m_password = passwordGroup.Value;
        }
        else {
          log.Debug ("LoadParametersWithSuffix: default password");
          m_password = PASSWORD_DEFAULT;
        }

        var databaseGroup = match.Groups[DATABASE_GROUP];
        if (databaseGroup.Success) {
          m_database = databaseGroup.Value;
        }
        else {
          log.Debug ("LoadParametersWithSuffix: default database");
          m_database = DATABASE_DEFAULT;
        }

        if (null != Updated) {
          Updated (this, new System.EventArgs ());
        }
        log.InfoFormat ("LoadParameters: " +
                        "The connection parameters taken from configset are " +
                        "server={0} port={1} database={2} user={3} password={4}",
                        m_server, m_port, m_database, m_username, m_password);
        return true;
      }
      catch (Exception ex) {
        if (log.IsWarnEnabled) {
          log.Warn ($"LoadParameters: Could not get the configuration key {configKey}", ex);
        }
        return false;
      }
    }
    #endregion
  }
}
