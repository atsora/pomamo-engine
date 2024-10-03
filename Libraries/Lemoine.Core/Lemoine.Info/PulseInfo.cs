// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023-2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;
using static System.Environment;

namespace Lemoine.Info
{
  /// <summary>
  /// Get information from the registry
  /// (global settings set during the installation of the system)
  /// </summary>
  public sealed class PulseInfo
  {
    static readonly string WEB_SERVICE_URL_KEY = "WebServiceUrl";
    static readonly string WEB_SERVICE_URL_DEFAULT = "";

    static readonly string MAIN_WEB_SERVICE_URL_KEY = "MainWebServiceUrl"; // For secondary web servers
    static readonly string MAIN_WEB_SERVICE_URL_DEFAULT = "";

    static readonly string FILE_REPOSITORY_CLIENT_KEY = "FileRepoClient"; // web / corba / directory
    static readonly string FILE_REPOSITORY_CLIENT_WEB = "web";
    static readonly string FILE_REPOSITORY_CLIENT_CORBA = "corba";
    static readonly string FILE_REPOSITORY_CLIENT_DIRECTORY = "directory";
    static readonly string FILE_REPOSITORY_CLIENT_MULTI = "multi";
    static readonly string FILE_REPOSITORY_CLIENT_DEFAULT = FILE_REPOSITORY_CLIENT_MULTI;

    static readonly string SHARED_DIRECTORY_PATH_KEY = "SharedDirectoryPath";
    static readonly string SHARED_DIRECTORY_PATH_DEFAULT = "";

    static readonly string MAIN_SERVER_INSTALL_DIR_KEY = "MainServerInstallDir";
    static readonly string COMPANY_CONFIG_DIRECTORY_KEY = "CompanyConfigDirectory";
    static readonly string COMMON_CONFIG_DIRECTORY_KEY = "CommonConfigDirectory";

    static readonly string REPORTS_INSTALL_DIR_KEY = "ReportsInstallDir";
    static readonly string TOMCAT_INSTALL_DIR_KEY = "TomcatInstallDir";
    static readonly string REPORTWEBAPP_INSTALL_DIR_KEY = "ReportWebAppInstallDir";

#if ATSORA
    static readonly string COMPANY_NAME = "Atsora";
#if CONNECTOR
    static readonly string LINUX_PACKAGE_NAME = "aconnector";
    static readonly string WIN_PRODUCT_FOLDER_NAME = "aconnector";
    static readonly string PRODUCT_NAME = "Connector";
#else // !CONNECTOR
    static readonly string LINUX_PACKAGE_NAME = "atracking";
    static readonly string WIN_PRODUCT_FOLDER_NAME = "atracking";
    static readonly string PRODUCT_NAME = "Tracking";
#endif // CONNECTOR
#elif LEMOINE
    static readonly string LINUX_PACKAGE_NAME = "lpulse";
    static readonly string WIN_PRODUCT_FOLDER_NAME = "PULSE";
    static readonly string COMPANY_NAME = "Lemoine";
    static readonly string PRODUCT_NAME = "Pulse"
#else
    static readonly string LINUX_PACKAGE_NAME = "pomamo";
    static readonly string WIN_PRODUCT_FOLDER_NAME = "pomamo";
    static readonly string COMPANY_NAME = "Pomamo";
    static readonly string PRODUCT_NAME = "Pomamo";
#endif

    static readonly string LINUX_CONF_DIRECTORY = $"/etc/{LINUX_PACKAGE_NAME}";

    static readonly string PFR_DATA_DIRECTORY_KEY = "PfrDataDirectory";
    static readonly string SQL_REQUESTS_DIRECTORY_KEY = "SqlRequestsDirectory";
    static readonly string LOCAL_CONFIGURATION_DIRECTORY_KEY = "LocalConfigurationDirectory";

    static readonly string DATABASE_NAME_KEY = "DatabaseName";
    static readonly string DATABASE_NAME_DEFAULT = Constants.DEFAULT_DATABASE_NAME;

    string m_defaultLocalConfigurationDirectory = "";
    bool m_valid = false;

    static readonly ILog log = LogManager.GetLogger (typeof (PulseInfo).FullName);

    /// <summary>
    /// Installation directory of the Server
    /// 
    /// This corresponds to config key "MainServerInstallDir"
    /// 
    /// Return null if it was not defined
    /// </summary>
    public static string MainServerInstallationDirectory
    {
      get {
        try {
          var result = Lemoine.Info.ConfigSet.Get<string> (MAIN_SERVER_INSTALL_DIR_KEY);
          if (log.IsErrorEnabled && string.IsNullOrEmpty (result)) {
            log.Error ("MainServerInstallationDirectory: not defined");
          }
          result = result.Replace (" (x86)", ""); // To replace "Program Files (x86)" by "Program Files" in case the registry key is badly set
          return result;
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Error ($"MainServerInstallationDirectory: config key {MAIN_SERVER_INSTALL_DIR_KEY} was not defined", ex);
          return null;
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"MainServerInstallationDirectory: (with deprecated KeyNotFoundException) config key {MAIN_SERVER_INSTALL_DIR_KEY} was not defined", ex);
          return null;
        }
      }
    }

    /// <summary>
    /// Installation directory of the reports
    /// 
    /// This corresponds to config key "ReportsInstallDir"
    /// 
    /// Return null if it was not defined
    /// </summary>
    public static string ReportsInstallationDirectory
    {
      get {
        try {
          var result = Lemoine.Info.ConfigSet.Get<string> (REPORTS_INSTALL_DIR_KEY);
          if (log.IsErrorEnabled && string.IsNullOrEmpty (result)) {
            log.Error ("ReportsInstallationDirectory: not defined");
          }
          result = result.Replace (" (x86)", ""); // To replace "Program Files (x86)" by "Program Files" in case the registry key is badly set
          return result;
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Error ($"ReportsInstallationDirectory: config key {REPORTS_INSTALL_DIR_KEY} was not defined", ex);
          return null;
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"ReportsInstallationDirectory: (with deprecated KeyNotFoundException) config key {REPORTS_INSTALL_DIR_KEY} was not defined", ex);
          return null;
        }
      }
    }

    /// <summary>
    /// Installation directory of Tomcat
    /// 
    /// This corresponds to config key "TomcatInstallDir"
    /// 
    /// Return null if it was not defined
    /// </summary>
    public static string TomcatInstallationDirectory
    {
      get {
        try {
          var result = Lemoine.Info.ConfigSet.Get<string> (TOMCAT_INSTALL_DIR_KEY);
          if (log.IsErrorEnabled && string.IsNullOrEmpty (result)) {
            log.Error ("TomcatInstallationDirectory: not defined");
          }
          result = result.Replace (" (x86)", ""); // To replace "Program Files (x86)" by "Program Files" in case the registry key is badly set
          return result;
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Error ($"TomcatInstallationDirectory: config key {TOMCAT_INSTALL_DIR_KEY} was not defined", ex);
          return null;
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"TomcatInstallationDirectory: (with deprecated KeyNotFoundException) config key {TOMCAT_INSTALL_DIR_KEY} was not defined", ex);
          return null;
        }
      }
    }

    /// <summary>
    /// Installation directory of ReportWebApp
    /// 
    /// This corresponds to config key "ReportWebAppInstallDir"
    /// 
    /// Return null if it was not defined
    /// </summary>
    public static string ReportWebAppInstallationDirectory
    {
      get {
        try {
          var result = Lemoine.Info.ConfigSet.Get<string> (REPORTWEBAPP_INSTALL_DIR_KEY);
          if (log.IsErrorEnabled && string.IsNullOrEmpty (result)) {
            log.Error ("ReportWebAppInstallationDirectory: not defined");
          }
          result = result.Replace (" (x86)", ""); // To replace "Program Files (x86)" by "Program Files" in case the registry key is badly set
          return result;
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Error ($"ReportWebAppInstallationDirectory: config key {REPORTWEBAPP_INSTALL_DIR_KEY} was not defined", ex);
          return null;
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"ReportWebAppInstallationDirectory: (with deprecated KeyNotFoundException) config key {REPORTWEBAPP_INSTALL_DIR_KEY} was not defined", ex);
          return null;
        }
      }
    }

    /// <summary>
    /// Directory where the common data to all Atsora product is stored
    /// 
    /// If the directory does not exist, create it
    /// </summary>
    public static string CompanyConfigurationDirectory
    {
      get {
        try {
          var companyConfigDirectory = Lemoine.Info.ConfigSet.Get<string> (COMPANY_CONFIG_DIRECTORY_KEY);
          if (!string.IsNullOrEmpty (companyConfigDirectory)) {
            if (!Directory.Exists (companyConfigDirectory)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"CompanyConfigurationDirectory: create {companyConfigDirectory}");
              }
              Directory.CreateDirectory (companyConfigDirectory);
            }
            return companyConfigDirectory;
          }
          else {
            log.Error ($"CompanyConfigurationDirectory: config key {COMPANY_CONFIG_DIRECTORY_KEY} returned an empty value {companyConfigDirectory}");
          }
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Error ($"CompanyConfigurationDirectory: config key {COMPANY_CONFIG_DIRECTORY_KEY} was not defined", ex);
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"CompanyConfigurationDirectory: (with deprecated KeyNotFoundException) config key {COMPANY_CONFIG_DIRECTORY_KEY} was not defined", ex);
        }
#if !NETSTANDARD
        if (true) {
#else // NETSTANDARD
        if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
#endif // NETSTANDARD
          var path = Path.Combine (GetFolderPath (SpecialFolder.CommonApplicationData), COMPANY_NAME);
          log.Warn ($"CompanyConfigurationDirectory: consider default windows folder {path}");
          if (!Directory.Exists (path)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CompanyConfigurationDirectory: create {path} (standard Windows path)");
            }
            Directory.CreateDirectory (path);
          }
          return path;
        }
#if NETSTANDARD
        else {
          var path = LINUX_CONF_DIRECTORY;
          log.Warn ($"CompanyConfigurationDirectory: consider default linux folder {path}");
          if (!Directory.Exists (path)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CompanyConfigurationDirectory: create {path} (standard Linux path)");
            }
            Directory.CreateDirectory (path);
          }
          return path;
        }
#endif // NETSTANDARD
      }
    }

    /// <summary>
    /// Directory where the common (to all the users) configurations are stored
    /// 
    /// If the directory does not exist, create it
    /// </summary>
    public static string CommonConfigurationDirectory
    {
      get {
        try {
          var commonConfigDirectory = Lemoine.Info.ConfigSet.Get<string> (COMMON_CONFIG_DIRECTORY_KEY);
          if (!string.IsNullOrEmpty (commonConfigDirectory)) {
            if (!Directory.Exists (commonConfigDirectory)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"CommonConfigurationDirectory: create {commonConfigDirectory}");
              }
              Directory.CreateDirectory (commonConfigDirectory);
            }
            return commonConfigDirectory;
          }
          else {
            log.Error ($"CommonConfigurationDirectory: config key {COMMON_CONFIG_DIRECTORY_KEY} returned an empty value {commonConfigDirectory}");
          }
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Error ($"CommonConfigurationDirectory: config key {COMMON_CONFIG_DIRECTORY_KEY} was not defined", ex);
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"CommonConfigurationDirectory: (with deprecated KeyNotFoundException) config key {COMMON_CONFIG_DIRECTORY_KEY} was not defined", ex);
        }
#if !NETSTANDARD
        if (true) {
#else // NETSTANDARD
        if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
#endif // NETSTANDARD
          var path = Path.Combine (GetFolderPath (SpecialFolder.CommonApplicationData), COMPANY_NAME, PRODUCT_NAME);
          log.Warn ($"CommonConfigurationDirectory: consider default windows folder {path}");
          if (!Directory.Exists (path)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CommonConfigurationDirectory: create {path} (standard Windows path)");
            }
            Directory.CreateDirectory (path);
          }
          return path;
        }
#if NETSTANDARD
        else {
          var path = LINUX_CONF_DIRECTORY;
          log.Warn ($"CommonConfigurationDirectory: consider default linux folder {path}");
          if (!Directory.Exists (path)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CommonConfigurationDirectory: create {path} (standard Linux path)");
            }
            Directory.CreateDirectory (path);
          }
          return path;
        }
#endif // NETSTANDARD
      }
    }

    /// <summary>
    /// Directory where the local configurations are stored
    /// The configurations are specific to a machine / user
    /// Read and write is allowed
    /// Note: there is no "\" at the end of the path
    /// 
    /// Create it if it does not exist
    /// </summary>
    public static string LocalConfigurationDirectory
    {
      get {
        string defaultLocalConfigurationDirectory = Instance.m_defaultLocalConfigurationDirectory;
        string localConfigurationDirectory = ConfigSet
          .LoadAndGet<string> (LOCAL_CONFIGURATION_DIRECTORY_KEY,
                               defaultLocalConfigurationDirectory);
        if (log.IsDebugEnabled) {
          log.DebugFormat ($"LocalConfigurationDirectory: {localConfigurationDirectory}, default={defaultLocalConfigurationDirectory}");
        }
        if (!Directory.Exists (localConfigurationDirectory)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"LocalConfigurationDirectory: create {localConfigurationDirectory}");
          }
          Directory.CreateDirectory (localConfigurationDirectory);
        }
        return localConfigurationDirectory;
      }
    }

    /// <summary>
    /// Name of the database
    /// </summary>
    public static string DatabaseName
    {
      get {
        return Lemoine.Info.ConfigSet
          .LoadAndGet<string> (DATABASE_NAME_KEY, DATABASE_NAME_DEFAULT);
      }
    }

    /// <summary>
    /// Is it valid ?
    /// 
    /// In case it is not, the installation directory and registry version
    /// may be empty
    /// </summary>
    public static bool Valid
    {
      get { return Instance.m_valid; }
    }

    /// <summary>
    /// Use of the Web protocol in the FileRepository client
    /// </summary>
    public static bool UseFileRepositoryWeb
    {
      get {
        return ConfigSet.LoadAndGet<string> (FILE_REPOSITORY_CLIENT_KEY, FILE_REPOSITORY_CLIENT_DEFAULT)
          .Equals (FILE_REPOSITORY_CLIENT_WEB, StringComparison.InvariantCultureIgnoreCase);
      }
    }

    /// <summary>
    /// Use of the Corba protocol in the FileRepository client
    /// </summary>
    public static bool UseFileRepositoryCorba
    {
      get {
        return ConfigSet.LoadAndGet<string> (FILE_REPOSITORY_CLIENT_KEY, FILE_REPOSITORY_CLIENT_DEFAULT)
          .Equals (FILE_REPOSITORY_CLIENT_CORBA, StringComparison.InvariantCultureIgnoreCase);
      }
    }

    /// <summary>
    /// Use of a shared directory in the FileRepository client
    /// </summary>
    public static bool UseFileRepositorySharedDirectory
    {
      get {
        return ConfigSet.LoadAndGet<string> (FILE_REPOSITORY_CLIENT_KEY, FILE_REPOSITORY_CLIENT_DEFAULT)
          .Equals (FILE_REPOSITORY_CLIENT_DIRECTORY, StringComparison.InvariantCultureIgnoreCase);
      }
    }

    /// <summary>
    /// Use multiple FileRepository clients
    /// </summary>
    public static bool UseFileRepositoryMulti
    {
      get {
        return ConfigSet.LoadAndGet<string> (FILE_REPOSITORY_CLIENT_KEY, FILE_REPOSITORY_CLIENT_DEFAULT)
          .Equals (FILE_REPOSITORY_CLIENT_MULTI, StringComparison.InvariantCultureIgnoreCase);
      }
    }

    /// <summary>
    /// Use of a shared directory in the FileRepository client (deprecated)
    /// </summary>
    public static bool UseSharedDirectory
    {
      get {
        return UseFileRepositorySharedDirectory;
      }
    }

    /// <summary>
    /// Shared directory (may not be used => check UseSharedDirectory)
    /// </summary>
    public static string SharedDirectoryPath
    {
      get { return ConfigSet.LoadAndGet<string> (SHARED_DIRECTORY_PATH_KEY, SHARED_DIRECTORY_PATH_DEFAULT); }
    }

    /// <summary>
    /// Path of the pfrdata repository (on the LCTR computer) if it exists, else null
    /// </summary>
    public static string PfrDataDir
    {
      get {
        string path;
        try {
          path = Lemoine.Info.ConfigSet
            .Get<string> (PFR_DATA_DIRECTORY_KEY);
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Debug ($"PfrDataDir: config key {PFR_DATA_DIRECTORY_KEY} was not defined => return null", ex);
          return null;
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"PfrDataDir: (with deprecated KeyNotFoundException) config key {PFR_DATA_DIRECTORY_KEY} was not defined => return null", ex);
          return null;
        }

        if (Directory.Exists (path)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"PfrDataDir: return {path}");
          }
          return path;
        }
        else {
          if (log.IsWarnEnabled) {
            log.Warn ($"PfrDataDir: {path} does not exist");
          }
          return null;
        }
      }
    }

    /// <summary>
    /// Path of the sql requests directory (on the LCTR computer) if it exists, else null
    /// </summary>
    public static string SqlRequestsDir
    {
      get {
        string path;
        try {
          path = Lemoine.Info.ConfigSet
            .Get<string> (SQL_REQUESTS_DIRECTORY_KEY);
        }
        catch (ConfigKeyNotFoundException ex) {
          log.Debug ($"SqlRequestsDir: config key {SQL_REQUESTS_DIRECTORY_KEY} was not defined => return null", ex);
          return null;
        }
        catch (KeyNotFoundException ex) {
          log.Fatal ($"SqlRequestsDir: (with deprecated KeyNotFoundException) config key {SQL_REQUESTS_DIRECTORY_KEY} was not defined => return null", ex);
          return null;
        }

        if (Directory.Exists (path)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"SqlRequestsDir: return {path}");
          }
          return path;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"SqlRequestsDir: {path} does not exist");
          }
          return null;
        }
      }
    }

    /// <summary>
    /// Product folder name
    /// </summary>
    /// <value></value>
    public static string ProductFolderName =>
#if NETSTANDARD
      RuntimeInformation.IsOSPlatform (OSPlatform.Windows) ? WIN_PRODUCT_FOLDER_NAME : LINUX_PACKAGE_NAME;
#else // !NETSTANDARD
      WIN_PRODUCT_FOLDER_NAME;
#endif

    /// <summary>
    /// Linux conf directory
    /// </summary>
    public static string LinuxConfDirectory => $"/etc/{LINUX_PACKAGE_NAME}";

    /// <summary>
    /// Linux package name
    /// </summary>
    public static string LinuxPackageName => LINUX_PACKAGE_NAME;

    /// <summary>
    /// URL of the web service
    /// </summary>
    public static string WebServiceUrl
    {
      get { return ConfigSet.LoadAndGet<string> (WEB_SERVICE_URL_KEY, WEB_SERVICE_URL_DEFAULT); }
    }

    /// <summary>
    /// URL of the main web service in case the server is a secondary web server
    /// </summary>
    public static string MainWebServiceUrl
    {
      get { return ConfigSet.LoadAndGet<string> (MAIN_WEB_SERVICE_URL_KEY, MAIN_WEB_SERVICE_URL_DEFAULT); }
    }

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private PulseInfo ()
    {
      try {
        Load ();
      }
      catch (Exception ex) {
        log.Error ("PulseInfo: could not get the info in registry", ex);
      }
    }
    #endregion

    /// <summary>
    /// Reload the Pulse info data
    /// </summary>
    public static void Reload ()
    {
      try {
        Instance.Load ();
      }
      catch (Exception ex) {
        log.Error ("Reload: Load failed", ex);
      }
    }

    void Load ()
    {
      try {
        // User directory (writable and readable)
        var localApplicationData = Environment
          .GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty (localApplicationData)) {
          log.Error ($"Load: LocalApplicationData {localApplicationData} is not defined");
          var home = Environment.GetEnvironmentVariable ("HOME");
          if (string.IsNullOrEmpty (home)) {
            log.Error ($"Load: HOME {home} is not defined");
          }
          else {
            localApplicationData = Path.Combine (home, ".local", "share");
            log.Info ($"Load: fallback localApplicationData to {localApplicationData} from home {home}");
          }
        }
        m_defaultLocalConfigurationDirectory = Path
          .Combine (localApplicationData, ProductFolderName);
        try {
          if (!System.IO.Directory.Exists (m_defaultLocalConfigurationDirectory)) {
            System.IO.Directory.CreateDirectory (m_defaultLocalConfigurationDirectory);
          }
        }
        catch (Exception ex1) {
          log.Error ($"Load: creating {m_defaultLocalConfigurationDirectory} failed", ex1);
        }

        this.m_valid = true;
        if (log.IsInfoEnabled) {
          log.Info ($"Load: default local configuration directory={m_defaultLocalConfigurationDirectory}");
        }
      }
      catch (Exception ex) {
        this.m_valid = false;
        log.Error ("Load: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Create the temp directories if they don't exist
    /// 
    /// This is to fix an issue in case a temp directories does not exist.
    /// </summary>
    public static void CreateTempDirectories ()
    {
      var temp = System.Environment.GetEnvironmentVariable ("TEMP");
      CreateIfNotExists (temp);
      var tmp = System.Environment.GetEnvironmentVariable ("TMP");
      CreateIfNotExists (tmp);
    }

    static void CreateIfNotExists (string directory)
    {
      if (!string.IsNullOrEmpty (directory) && !System.IO.Directory.Exists (directory)) {
        System.IO.Directory.CreateDirectory (directory);
      }
    }

    #region Instance
    static PulseInfo Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly PulseInfo instance = new PulseInfo ();
    }
    #endregion
  }
}
