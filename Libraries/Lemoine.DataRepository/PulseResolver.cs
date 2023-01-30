// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;

using Lemoine.Info;
using Lemoine.Core.Log;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Resolve the XML entities like the schema locations
  /// </summary>
  public class PulseResolver : XmlUrlResolver
  {
    /// <summary>
    /// Old ODBC namespace
    /// </summary>
    public static readonly string PULSE_OLD_ODBC_NAMESPACE = "http://odbc.pulse.com";

    /// <summary>
    /// ODBC namespace used to synchronize some external database with ODBC
    /// </summary>
    public static readonly string PULSE_ODBC_NAMESPACE = "urn:pulse.lemoinetechnologies.com:synchro:odbc";

    /// <summary>
    /// GDB namespace used to synchronize the global database
    /// </summary>
    public static readonly string PULSE_GDB_NAMESPACE = "urn:pulse.lemoinetechnologies.com:synchro:gdb";

    /// <summary>
    /// odbcgdbconfig namespace used to synchronize the global database from the an external database with ODBC
    /// </summary>
    public static readonly string PULSE_ODBCGDBCONFIG_NAMESPACE = "urn:pulse.lemoinetechnologies.com:synchro:odbcgdbconfig";

    const string XML_SCHEMAS_DIRECTORY_KEY = "XmlSchemasDirectory";

    static readonly ILog log = LogManager.GetLogger (typeof (PulseResolver).FullName);

    #region Getters / Setters
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public PulseResolver ()
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Override GetEntity to resolve:
    /// <item>PULSE_OLD_ODBC_NAMESPACE http://odbc.pulse.com</item>
    /// <item>PULSE_ODBC_NAMESPACE urn:pulse.lemoinetechnologies.com:synchro:odbc</item>
    /// <item>PULSE_GDB_NAMESPACE urn:pulse.lemoinetechnologies.com:synchro:gdb</item>
    /// <item>PULSE_ODBCGDBCONFIG_NAMESPACE urn:pulse.lemoinetechnologies.com:synchro:odbcgdbconfig</item>
    /// 
    /// TODO: unit tests
    /// 
    /// <see cref="XmlUrlResolver.GetEntity">XmlUrlResolver.GetEntity method</see>
    /// </summary>
    /// <param name="absoluteUri"></param>
    /// <param name="role"></param>
    /// <param name="ofObjectToReturn"></param>
    /// <returns></returns>
    public override Object GetEntity (Uri absoluteUri,
                                      string role,
                                      Type ofObjectToReturn)
    {
      log.DebugFormat ("GetEntity aboluteUri={0}",
                       absoluteUri.ToString ());

      if (absoluteUri.AbsoluteUri.Equals (PULSE_OLD_ODBC_NAMESPACE)) {
        log.DebugFormat ("GetEntity absoluteUri={0}: " +
                         "substitute http://odbc.pulse.com " +
                         "with odbcpulse.xsd",
                         absoluteUri);
        Uri odbcPulseUri = ResolveUri (new Uri (""), "odbcpulse.xsd");
        return base.GetEntity (odbcPulseUri,
                               role,
                               ofObjectToReturn);
      }

      if (absoluteUri.AbsoluteUri.Equals (PULSE_ODBC_NAMESPACE)) {
        log.DebugFormat ("GetEntity absoluteUri={0}: " +
                         "substitute urn:pulse.lemoinetechnologies.com:synchro:odbc" +
                         "with odbcsynchro.xsd",
                         absoluteUri);
        Uri odbcPulseUri = ResolveUri (new Uri (""), "odbcsynchro.xsd");
        return base.GetEntity (odbcPulseUri,
                               role,
                               ofObjectToReturn);
      }

      if (absoluteUri.AbsoluteUri.Equals (PULSE_GDB_NAMESPACE)) {
        log.DebugFormat ("GetEntity absoluteUri={0}: " +
                         "substitute urn:pulse.lemoinetechnologies.com:synchro:gdb " +
                         "with gdbsynchro.xsd",
                         absoluteUri);
        Uri gdbPulseUri = ResolveUri (new Uri (""), "gdbsynchro.xsd");
        return base.GetEntity (gdbPulseUri,
                               role,
                               ofObjectToReturn);
      }

      if (absoluteUri.AbsoluteUri.Equals (PULSE_ODBCGDBCONFIG_NAMESPACE)) {
        log.DebugFormat ("GetEntity absoluteUri={0}: " +
                         "substitute urn:pulse.lemoinetechnologies.com:synchro:odbcgdbconfig " +
                         "with synchroodbcgdbconfig.xsd",
                         absoluteUri);
        Uri gdbPulseUri = ResolveUri (new Uri (""), "synchroodbcgdbconfig.xsd");
        return base.GetEntity (gdbPulseUri,
                               role,
                               ofObjectToReturn);
      }

      return base.GetEntity (absoluteUri,
                             role,
                             ofObjectToReturn);
    }

    /// <summary>
    /// Resolves the absolute URI from the base and relative URIs,
    /// considering a list of possible base path that are specific to POmamo:
    /// <list type="bullet">
    /// <item>the default base path baseUri</item>
    /// <item>the current directory</item>
    /// <item>XmlSchemasDirectory</item>
    /// <item>"installation path"/share/XMLSchemas (if defined)</item>
    /// <item>"installation path"/XMLSchemas (if defined)</item>
    /// <item>"installation path" (if defined)</item>
    /// <item>"Application path"</item>
    /// <item>"Assembly path"</item>
    /// </list>
    /// 
    /// TODO: unit tests
    /// 
    /// <see cref="XmlUrlResolver.ResolveUri">XmlUrlResolver.ResolverUri method</see>
    /// </summary>
    /// <param name="baseUri"></param>
    /// <param name="relativeUri"></param>
    /// <returns></returns>
    public override Uri ResolveUri (Uri baseUri,
                                    string relativeUri)
    {
      log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1} /B",
                       baseUri, relativeUri);

      // Try the different possible base paths
      Uri result = null;
      // 1. The default base path baseUri
      result = base.ResolveUri (baseUri, relativeUri);
      if (result != null) {
        if (result.IsFile && !File.Exists (result.LocalPath)) {
          log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                           "resulted URI is {2} with base default path " +
                           "but the file does not exist, " +
                           "try another baseUri",
                           baseUri, relativeUri, result);
        }
        else {
          log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                          "consider base path is the default one, " +
                          "result is {2}",
                          baseUri, relativeUri, result);
          return result;
        }
      }
      // 2. The current directory
      Uri testBaseUri = new Uri (Environment.CurrentDirectory);
      result = base.ResolveUri (testBaseUri,
                                relativeUri);
      if (result != null) {
        if (result.IsFile && !File.Exists (result.LocalPath)) {
          log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                           "resulted URI is {2} with current directory " +
                           "but the file does not exist, " +
                           "try another baseUri",
                           baseUri, relativeUri, result);
        }
        else {
          log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                          "consider base path is the current directory {2}, " +
                          "result is {3}",
                          baseUri, relativeUri, testBaseUri, result);
          return result;
        }
      }
      // 3. XML_SCHEMAS_DIRECTORY
      try {
        testBaseUri = new Uri (Lemoine.Info.ConfigSet.Get<string> (XML_SCHEMAS_DIRECTORY_KEY));
        result = base.ResolveUri (testBaseUri,
                                  relativeUri);
        if (result != null) {
          if (result.IsFile && !File.Exists (result.LocalPath)) {
            log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                             "resulted URI is {2} with current directory " +
                             "but the file does not exist, " +
                             "try another baseUri",
                             baseUri, relativeUri, result);
          }
          else {
            log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                            "consider base path is the current directory {2}, " +
                            "result is {3}",
                            baseUri, relativeUri, testBaseUri, result);
            return result;
          }
        }
      }
      catch (ConfigKeyNotFoundException ex) {
        log.Warn ($"ResolveUri: config key {XML_SCHEMAS_DIRECTORY_KEY} is not defined", ex);
      }
      catch (System.Collections.Generic.KeyNotFoundException ex) {
        log.Fatal ($"ResolveUri: (with deprecated KeyNotFoundException) config key {XML_SCHEMAS_DIRECTORY_KEY} is not defined", ex);
      }
      // Deprecated: please use XmlSchemasDirectory instead from now to set where the XML schemas are stored
      // PulseInfo.InstallationDir will be removed one day
      string installationDir = PulseInfo.InstallationDir;
      if (!string.IsNullOrEmpty (installationDir) && Path.IsPathRooted (installationDir)) {
        // 4a. "installation path"/share/XMLSchemas
        testBaseUri = new Uri (installationDir +
                               "\\share\\XMLSchemas\\");
        result = base.ResolveUri (testBaseUri,
                                  relativeUri);
        if (result != null) {
          if (result.IsFile && !File.Exists (result.LocalPath)) {
            log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                             "resulted URI is {2} with installation/share/XMLSchemas " +
                             "but the file does not exist, " +
                             "try another baseUri",
                             baseUri, relativeUri, result);
          }
          else {
            log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                            "consider base path is " +
                            "installation/share/XMLSchemas {2}, " +
                            "result is {3}",
                            baseUri, relativeUri, testBaseUri, result);
            return result;
          }
        }
        // 4b. "installation path"/XMLSchemas
        testBaseUri = new Uri (installationDir +
                               "\\XMLSchemas\\");
        result = base.ResolveUri (testBaseUri,
                                  relativeUri);
        if (result != null) {
          if (result.IsFile && !File.Exists (result.LocalPath)) {
            log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                             "resulted URI is {2} with installation/XMLSchemas " +
                             "but the file does not exist, " +
                             "try another baseUri",
                             baseUri, relativeUri, result);
          }
          else {
            log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                            "consider base path is " +
                            "installation/XMLSchemas {2}, " +
                            "result is {3}",
                            baseUri, relativeUri, testBaseUri, result);
            return result;
          }
        }
        // 4c. "installation path"
        testBaseUri = new Uri (installationDir);
        result = base.ResolveUri (testBaseUri,
                                  relativeUri);
        if (result != null) {
          if (result.IsFile && !File.Exists (result.LocalPath)) {
            log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                             "resulted URI is {2} with installation " +
                             "but the file does not exist, " +
                             "try another baseUri",
                             baseUri, relativeUri, result);
          }
          else {
            log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                            "consider base path is " +
                            "installation {2}, " +
                            "result is {3}",
                            baseUri, relativeUri, testBaseUri, result);
            return result;
          }
        }
      }
      // 5. "Application path"
      string programPath = ProgramInfo.AbsoluteDirectory;
      if (null == programPath) {
        log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                        "could not get the program path",
                        baseUri, relativeUri);
      }
      else {
        testBaseUri = new Uri (programPath);
        result = base.ResolveUri (testBaseUri,
                                  relativeUri);
        if (result != null) {
          if (result.IsFile && !File.Exists (result.LocalPath)) {
            log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                             "resulted URI is {2} with application path " +
                             "but the file does not exist, " +
                             "try another baseUri",
                             baseUri, relativeUri, result);
          }
          else {
            log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                            "consider base path is " +
                            "the application path {2}, " +
                            "result is {3}",
                            baseUri, relativeUri, testBaseUri, result);
            return result;
          }
        }
      }
      // 6. "Assembly path"
      testBaseUri = new Uri (Lemoine.Info.AssemblyInfo.AbsoluteDirectory);
      result = base.ResolveUri (testBaseUri,
                                relativeUri);
      if (result != null) {
        if (result.IsFile && !File.Exists (result.LocalPath)) {
          log.DebugFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                           "resulted URI is {2} with assembly path " +
                           "but the file does not exist, " +
                           "try another baseUri",
                           baseUri, relativeUri, result);
        }
        else {
          log.InfoFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                          "consider base path is " +
                          "the assembly path {2}, " +
                          "result is {3}",
                          baseUri, relativeUri, testBaseUri, result);
          return result;
        }
      }
      // 8. Fallback to the default method
      result = base.ResolveUri (baseUri, relativeUri);
      log.WarnFormat ("ResolveUri baseUri={0} relativeUri={1}: " +
                      "no valid URI was found, use the default one {2}",
                      baseUri, relativeUri,
                      result);
      return result;
    }
    #endregion
  }
}
