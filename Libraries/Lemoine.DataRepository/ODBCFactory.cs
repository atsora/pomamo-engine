// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD || NET48 || NETCOREAPP

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Data.Odbc;
using System.Threading;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// ODBC Factory: builds a DOMDocument from a schema and SQL requests
  /// 
  /// This class builds a DOMDocument from a DOMDocument schema that:
  /// <item>has the same structure as the expected DOMDocument</item>
  /// <item>contains the parameters to connect to the database</item>
  /// <item>contains the SQL Requests for all missing values in the schema</item>
  ///
  /// In the schema, the missing values have the following format:
  /// <item>pulse:([0-9]+):(string|integer)</item>
  /// <item>([0-9]+) represents the order in which they are retrieved in the SQL request (from 1)</item>
  /// <item>(string|integer) is the type of the returned value in the SQL request</item>
  /// 
  /// The SQL Requests are given in attributes that are named "pulse:request".
  /// The value of these attributes is an SQL request:
  /// <item>whose selected fields keep the order given in the missing values (see above)</item>
  /// <item>whose condition can contain the symbols '%i' where 'i' is the order of a parent field</item>
  /// <item>whose condition can contain xpath expressions in {}</item>
  /// 
  /// The connection parameters are given in the root element
  /// in the following attributes:
  /// <item>pulse:dnsname</item>
  /// <item>pulse:user</item>
  /// <item>pulse:password</item>
  /// 
  /// <example>
  /// Example of a schema:
  /// <![CDATA[
  /// <root pulse:dnsname="mydatabase" pulse:user="myself" pulse:password="pass"\>
  ///   <job name="pulse:1:string" hours="pulse:2:integer"
  ///        pulse:request="SELECT name, hours FROM jobtable"\>
  ///     <component name="pulse:1:string"
  ///                pulse:request"SELECT comptable.name
  ///                              FROM comptable, jobtable
  ///                              WHERE jobtable.name='{../@name}'
  ///                                    AND jobtable.id = comptable.jobid"\>
  ///     </component\>
  ///   </job\>
  /// </root\>
  /// ]]>
  /// </example>
  /// 
  /// TODO: sqlCacheMap if needed one day
  /// TODO: filters if needed one day
  /// </summary>
  public class ODBCFactory : IFactory
  {
    static readonly string REQUEST_KEY = "request";
    static readonly string LIMIT_KEY = "limit";

    static readonly string ODBC_COMMAND_TIMEOUT = "odbc.command.timeout";

    static readonly ILog log = LogManager.GetLogger (typeof (ODBCFactory).FullName);

    #region Exceptions
    /// <summary>
    /// Raised when an error was found in the schema
    /// </summary>
    public class SchemaException : RepositoryException
    {
      /// <summary>
      /// <see cref="RepositoryException"/>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public SchemaException (string message)
        : base (message)
      {
      }

      /// <summary>
      /// <see cref="RepositoryException"/>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      /// <param name="innerException"></param>
      public SchemaException (string message, Exception innerException)
        : base (message, innerException)
      {
      }
    }

    /// <summary>
    /// Database or ODBC exception
    /// </summary>
    public class DatabaseException : RepositoryException
    {
      /// <summary>
      /// <see cref="RepositoryException"/>
      /// </summary>
      /// <param name="message"></param>
      /// <param name="innerException"></param>
      public DatabaseException (string message, Exception innerException)
        : base (message, innerException)
      {
      }
    }

    /// <summary>
    /// XPath evaluation exception
    /// </summary>
    public class XPathException : RepositoryException
    {
      /// <summary>
      /// RepositoryException in case of an XPath evaluation problem
      /// 
      /// <see cref="RepositoryException"/>
      /// </summary>
      /// <param name="message"></param>
      /// <param name="innerException"></param>
      public XPathException (string message, Exception innerException)
        : base (message, innerException)
      {
      }
    }
    #endregion

    #region Members
    OdbcConnection m_connection = null;
    readonly IConnectionParameters m_connectionParameters = null;
    readonly XmlDocument m_schema;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Connection parameters (read-only)
    /// </summary>
    public IConnectionParameters ConnectionParameters
    {
      get { return m_connectionParameters; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// 
    /// XML Schema validation
    /// is always done if source is XmlSourceType.URI
    /// 
    /// If the source is XmlSourceType.STRING, there is no
    /// XML schema validation
    /// </summary>
    /// <param name="source">Source type of data</param>
    /// <param name="data">Data: raw string or file URI</param>
    /// <param name="connectionParams">ODBC connection parameters</param>
    public ODBCFactory (XmlSourceType source, string data, IConnectionParameters connectionParams)
    {
      m_schema = new XmlDocument ();
      m_connectionParameters = connectionParams;
      RepositoryValidationHandler validationHandler = new RepositoryValidationHandler ();
      XmlReader xmlReader = null;
      XmlReaderSettings settings = new XmlReaderSettings ();
      switch (source) {
      case XmlSourceType.STRING:
        settings.XmlResolver = new PulseResolver ();
        settings.ValidationType = ValidationType.None;
        xmlReader = XmlReader.Create (new StringReader (data), settings);
        break;
      case XmlSourceType.URI:
        settings.XmlResolver = new PulseResolver ();
        settings.ValidationType = ValidationType.Schema;
        settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        settings.ValidationEventHandler +=
          new ValidationEventHandler (validationHandler.EventHandler);
        xmlReader = XmlReader.Create (data, settings);
        break;
      }

      // Load the XML
      System.Diagnostics.Debug.Assert (null != xmlReader);
      m_schema.Load (xmlReader);

      if (validationHandler.Errors) {
        log.ErrorFormat ("ODBCBuilder data={0}: " +
                         "a validation error occured",
                         data);
        throw new ValidationException (String.Format ("Validation error in {0}",
                                                      data));
      }

      TestSchema ();
      InitConnectParam ();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Test the schema DOMDocument
    /// </summary>
    private void TestSchema ()
    {
      if (null == m_schema.DocumentElement) {
        log.ErrorFormat ("TestSchema: " +
                         "no root element in schema");
        throw new SchemaException ("No root element in schema");
      }

      return;
    }

    /// <summary>
    /// Initialize the connection parameters
    /// </summary>
    private void InitConnectParam ()
    {
      m_connectionParameters.Build (m_schema);
    }

    /// <summary>
    /// Insert an element in doc from a given node
    /// 
    /// Recursive method that inserts in doc a new element
    /// under the docParentElem node
    /// from the information given by schemaElem
    /// </summary>
    /// <param name="doc">Document to build</param>
    /// <param name="schemaElem">Schema element that contains the information to onsert</param>
    /// <param name="docParentElem">Node under which the new element must be inserted</param>
    void InsertElement (XmlDocument doc,
                        XmlElement schemaElem,
                        XmlElement docParentElem)
    {
      System.Diagnostics.Debug.Assert (null != doc);
      System.Diagnostics.Debug.Assert (null != schemaElem);
      System.Diagnostics.Debug.Assert (null != docParentElem);

      string sqlRequest = schemaElem.GetAttribute (REQUEST_KEY,
                                                   PulseResolver.PULSE_ODBC_NAMESPACE);
      if (0 == sqlRequest.Length) {
        XmlElement docElem = doc.ImportNode (schemaElem, false) as XmlElement;
        docParentElem.AppendChild (docElem);

        // recursive call
        foreach (XmlNode child in schemaElem.ChildNodes) {
          if (child is XmlElement) {
            this.InsertElement (doc,
                                (XmlElement)child,
                                docElem);
          }
        }
      }
      else { // There is a pulse:request attribute
        string limitAttribute = schemaElem.GetAttribute (LIMIT_KEY,
                                                         PulseResolver.PULSE_ODBC_NAMESPACE);
        int limit = Int32.MaxValue;
        if (!string.IsNullOrEmpty (limitAttribute)) {
          if (!int.TryParse (limitAttribute, out limit)) {
            log.ErrorFormat ("InsertElement: limit {0} is not an integer", limitAttribute);
            limit = Int32.MaxValue;
          }
        }

        try {
          using (XPathEvaluator evaluator = new XPathEvaluator (schemaElem,
                                                                docParentElem)) {
            sqlRequest = evaluator.Replace (sqlRequest);
          }
          using (KeyValueTableEvaluator evaluator = new KeyValueTableEvaluator ()) {
            sqlRequest = evaluator.Replace (sqlRequest);
          }
          using (CustomFunctionEvaluator evaluator = new CustomFunctionEvaluator ()) {
            sqlRequest = evaluator.Replace (sqlRequest);
          }
        }
        catch (Exception ex) {
          log.ErrorFormat ("InsertElement: XPath evaluation error {0}", ex);
          throw new XPathException ("XPath evaluation error", ex);
        }

        List<XmlElement> eltList = new List<XmlElement> ();

        log.InfoFormat ("InsertElement: " +
                        "about to run {0}",
                        sqlRequest);

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        using (var command = new OdbcCommand (sqlRequest, m_connection)) {
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          try {
            var commandTimeOut = Lemoine.Info.ConfigSet.Get<TimeSpan> (ODBC_COMMAND_TIMEOUT);
            log.InfoFormat ("InsertElement: use command timeout={0}", commandTimeOut);
            command.CommandTimeout = (int)commandTimeOut.TotalSeconds;
          }
          catch (Exception) { }
          OdbcDataReader reader;
          try {
            reader = command.ExecuteReader ();
          }
          catch (OdbcException odbcException) {
            log.ErrorFormat ("InsertElement: " +
                             "ExecuteReader returned an OdbcException with {0} errors",
                             odbcException.Errors.Count);
            foreach (OdbcError error in odbcException.Errors) {
              log.ErrorFormat ("InsertElement: " +
                               "ExecuteReader returned OdbcError \n" +
                               "Message: {0}\n" +
                               "Native: {1}\n" +
                               "Source: {2}\n" +
                               "SQL: {3}",
                               error.Message,
                               error.NativeError.ToString (),
                               error.Source,
                               error.SQLState);
            }
            throw new DatabaseException ("ExecuteReader ODBC exception", odbcException);
          }
          catch (Exception ex) {
            log.ErrorFormat ("InsertElement: " +
                             "ExecuteReader returned exception {0}",
                             ex);
            throw new DatabaseException ("ExecuteReader error", ex);
          }
          int n = 0; // Row number to limit the number of data to process with limit
          while ((n++ < limit) && reader.Read ()) {
            XmlElement docElem = doc.ImportNode (schemaElem, false) as XmlElement;
            docParentElem.AppendChild (docElem);
            eltList.Add (docElem);
            docElem.RemoveAttribute (REQUEST_KEY,
                                     PulseResolver.PULSE_ODBC_NAMESPACE);
            docElem.RemoveAttribute (LIMIT_KEY,
                                     PulseResolver.PULSE_ODBC_NAMESPACE);
            for (int i = 0; i < reader.FieldCount; ++i) {
              string attributeValue;
              if (reader.IsDBNull (i)) {
                attributeValue = "";
              }
              else {
                Type fieldType = reader.GetFieldType (i);
                object fieldValue = reader.GetValue (i);
                if (fieldType.Equals (typeof (System.Double))) {
                  Double d = (Double)fieldValue;
                  attributeValue = d.ToString (CultureInfo.InvariantCulture);
                }
                else if (fieldType.Equals (typeof (System.DateTime))) {
                  DateTime dateTime = (DateTime)fieldValue;
                  attributeValue = dateTime.ToString ("yyyy-MM-dd HH:mm:ss");
                }
                else {
                  attributeValue = fieldValue.ToString ();
                }
              }
              log.DebugFormat ("InsertElement: " +
                               "about to set {0}={1}",
                               reader.GetName (i),
                               attributeValue);
              if (docElem.GetAttribute (reader.GetName (i)).StartsWith ("pulse")) {
                docElem.SetAttribute (reader.GetName (i),
                                      attributeValue);
              }
              else {
                string aaa = docElem.GetNamespaceOfPrefix ("pulse");
                string abb = docElem.GetPrefixOfNamespace (PulseResolver.PULSE_ODBC_NAMESPACE);
                docElem.SetAttribute (reader.GetName (i),
                                      PulseResolver.PULSE_ODBC_NAMESPACE,
                                      attributeValue);
              }
            }
          }
          reader.Close ();
        }

        // recursive call
        // go through all elements that have been added
        foreach (XmlElement docElem in eltList) {
          foreach (XmlNode child in schemaElem.ChildNodes) {
            if (child is XmlElement) {
              this.InsertElement (doc,
                                  (XmlElement)child,
                                  docElem as XmlElement);
            }
          }
        }
      }
    }


    /// <summary>
    /// Evaluate the XPath given:
    /// <item>a string</item>
    /// <item>the parent element</item>
    /// <item>the schema element</item>
    /// </summary>
    private sealed class XPathEvaluator : IDisposable
    {
      static readonly Regex xpathRegex = new Regex (@"\{([^}]*)\}");

      bool m_isDisposed = false;
      readonly XmlElement m_parent = null;
      readonly XmlElement m_child = null;
      readonly IXmlNamespaceResolver m_namespaceResolver = null;
      readonly XPathNavigator m_schemaNavigator = null;
      readonly XPathNavigator m_documentNavigator = null;

      /// <summary>
      /// Constructor in case both:
      /// <item>the schema element</item>
      /// <item>a document parent element</item>
      /// must be taken into account
      /// </summary>
      /// <param name="schemaElement"></param>
      /// <param name="documentParentElement"></param>
      public XPathEvaluator (XmlElement schemaElement,
                             XmlElement documentParentElement)
      {
        this.m_parent = documentParentElement;
        this.m_child = m_parent.OwnerDocument.CreateElement ("child");
        m_parent.AppendChild (m_child);
        m_schemaNavigator = schemaElement.CreateNavigator ();
        m_documentNavigator = m_child.CreateNavigator ();
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager (schemaElement.OwnerDocument.NameTable);
        namespaceManager.AddNamespace (schemaElement.GetPrefixOfNamespace (PulseResolver.PULSE_ODBC_NAMESPACE),
                                       PulseResolver.PULSE_ODBC_NAMESPACE);
        m_namespaceResolver = namespaceManager;
      }

      /// <summary>
      /// Constructor in case a single element must be taken into account
      /// </summary>
      /// <param name="element"></param>
      public XPathEvaluator (XmlElement element)
      {
        m_documentNavigator = element.CreateNavigator ();
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager (element.OwnerDocument.NameTable);
        namespaceManager.AddNamespace (element.GetPrefixOfNamespace (PulseResolver.PULSE_ODBC_NAMESPACE),
                                       PulseResolver.PULSE_ODBC_NAMESPACE);
        m_namespaceResolver = namespaceManager;
      }

      /// <summary>
      /// Dispose method
      /// <see cref="IDisposable.Dispose" />
      /// </summary>
      public void Dispose ()
      {
        Dispose (true);
        GC.SuppressFinalize (this);
      }

      void Dispose (bool disposing)
      {
        if (!m_isDisposed) {
          if ((null != m_parent)
              && (null != m_child)) {
            m_parent.RemoveChild (m_child);
          }
        }
        m_isDisposed = true;
      }

      ~XPathEvaluator ()
      {
        Dispose (false);
      }

      /// <summary>
      /// Replace all the XPath found in the request argument by their values
      /// </summary>
      /// <param name="request"></param>
      /// <returns></returns>
      public string Replace (string request)
      {
        return xpathRegex.Replace (request,
                                   new MatchEvaluator (ReplaceXPath));
      }

      private string ReplaceXPath (Match m)
      {
        System.Diagnostics.Debug.Assert (true == m.Success);
        System.Diagnostics.Debug.Assert (2 == m.Groups.Count);
        System.Diagnostics.Debug.Assert (true == m.Groups[1].Success);
        string xpath = m.Groups[1].Value;

        // 1. First try in child
        if (null != m_documentNavigator) {
          XPathNavigator node = m_documentNavigator.SelectSingleNode (xpath,
                                                                    m_namespaceResolver);
          if (null != node) {
            return node.Value;
          }
        }

        // 2. Second try in schema
        if (null != m_schemaNavigator) {
          XPathNavigator node = m_schemaNavigator.SelectSingleNode (xpath,
                                                                  m_namespaceResolver);
          if (null != node) {
            return node.Value;
          }
        }

        log.WarnFormat ("ReplaceXPath: " +
                        "XPath {0} was not found in both document and schema " +
                        "return original string {1}",
                        xpath, m.Value);
        return m.Value;
      }
    }


    /// <summary>
    /// Evaluate the key/value table expressions in [%...%] by their values
    /// </summary>
    private sealed class KeyValueTableEvaluator : IDisposable
    {
      static readonly Regex KEY_VALUE_TABLE_REGEX = new Regex (@"\[%([^.]+).([a-zA-Z.]+)%\]");

      bool m_isDisposed = false;

      /// <summary>
      /// Constructor
      /// </summary>
      public KeyValueTableEvaluator ()
      {
      }

      /// <summary>
      /// Dispose method
      /// <see cref="IDisposable.Dispose" />
      /// </summary>
      public void Dispose ()
      {
        Dispose (true);
        GC.SuppressFinalize (this);
      }

      void Dispose (bool disposing)
      {
        if (!m_isDisposed) {
        }
        m_isDisposed = true;
      }

      ~KeyValueTableEvaluator ()
      {
        Dispose (false);
      }

      /// <summary>
      /// Replace all the key/value table expressions in [%...%] found in the request argument by their values
      /// </summary>
      /// <param name="request"></param>
      /// <returns></returns>
      public string Replace (string request)
      {
        return KEY_VALUE_TABLE_REGEX.Replace (request,
                                              new MatchEvaluator (ReplaceKey));
      }

      private string ReplaceKey (Match m)
      {
        System.Diagnostics.Debug.Assert (true == m.Success);
        System.Diagnostics.Debug.Assert (3 == m.Groups.Count);
        System.Diagnostics.Debug.Assert (true == m.Groups[1].Success);
        string table = m.Groups[1].Value;
        System.Diagnostics.Debug.Assert (true == m.Groups[2].Success);
        string key = m.Groups[2].Value;

        if (table.Equals ("applicationstate", StringComparison.InvariantCultureIgnoreCase)) {
          Lemoine.Model.IApplicationState applicationState;
          try {
            using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
                .GetApplicationState (key);
            }
          }
          catch (Exception ex) {
            log.ErrorFormat ("ReplaceKey: error while reading the application state for key {0}, {1}", key, ex);
            throw new DatabaseException ("Applicationstate read error", ex);
          }
          if (null == applicationState) {
            log.ErrorFormat ("ReplaceKey: " +
                             "application state {0} not found " +
                             "=> keep it unchanged",
                             key);
            return m.Value;
          }
          else {
            log.DebugFormat ("ReplaceKey: " +
                             "replace application state key {0} by {1}",
                             key, applicationState.Value);
            return applicationState.Value.ToString ();
          }
        }
        else if (table.Equals ("config", StringComparison.InvariantCultureIgnoreCase)) {
          Lemoine.Model.IConfig config;
          try {
            using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              config = ModelDAOHelper.DAOFactory.ConfigDAO
                .GetConfig (key);
            }
          }
          catch (Exception ex) {
            log.ErrorFormat ("ReplaceKey: error while reading config {0}, {1}", key, ex);
            throw new DatabaseException ("Config read error", ex);
          }
          if (null == config) {
            log.ErrorFormat ("ReplaceKey: " +
                             "config {0} not found " +
                             "=> keep it unchanged",
                             key);
            return m.Value;
          }
          else {
            log.DebugFormat ("ReplaceKey: " +
                             "replace config key {0} by {1}",
                             key, config.Value);
            return config.Value.ToString ();
          }
        }
        else {
          log.ErrorFormat ("ReplaceKey: " +
                           "unsupported key/value table {0} " +
                           "=> do not change it",
                           table);
          return m.Value;
        }
      }
    }


    /// <summary>
    /// Evaluate the custom function calls by their values.
    /// 
    /// The custom functions are:
    /// <item>LocalNow(format)</item>
    /// <item>UtcNow(format)</item>
    /// <item>LocalNowOffset(format,offset)</item>
    /// <item>UtcNowOffset(format,offset)</item>
    /// </summary>
    private sealed class CustomFunctionEvaluator : IDisposable
    {
      static readonly Regex CUSTOM_FUNCTION_REGEX = new Regex (@"(LocalNow|UtcNow|LocalNowOffset|UtcNowOffset)\(([^()]*)\)");

      bool m_isDisposed = false;

      /// <summary>
      /// Constructor
      /// </summary>
      public CustomFunctionEvaluator ()
      {
      }

      /// <summary>
      /// Dispose method
      /// <see cref="IDisposable.Dispose" />
      /// </summary>
      public void Dispose ()
      {
        Dispose (true);
        GC.SuppressFinalize (this);
      }

      void Dispose (bool disposing)
      {
        if (!m_isDisposed) {
        }
        m_isDisposed = true;
      }

      ~CustomFunctionEvaluator ()
      {
        Dispose (false);
      }

      /// <summary>
      /// Replace all the custom function expressions found in the request argument by their values
      /// </summary>
      /// <param name="request"></param>
      /// <returns></returns>
      public string Replace (string request)
      {
        return CUSTOM_FUNCTION_REGEX.Replace (request,
                                              new MatchEvaluator (ReplaceKey));
      }

      private string ReplaceKey (Match m)
      {
        System.Diagnostics.Debug.Assert (true == m.Success);
        System.Diagnostics.Debug.Assert (3 == m.Groups.Count);
        System.Diagnostics.Debug.Assert (true == m.Groups[1].Success);
        string function = m.Groups[1].Value;
        System.Diagnostics.Debug.Assert (true == m.Groups[2].Success);
        string arguments = m.Groups[2].Value;

        if (function.Equals ("LocalNow")) {
          log.DebugFormat ("ReplaceKey: " +
                           "process LocalNow custom function");
          return DateTime.Now.ToString (arguments);
        }
        else if (function.Equals ("UtcNow")) {
          log.DebugFormat ("ReplaceKey: " +
                           "process UtcNow custom function");
          return DateTime.UtcNow.ToString (arguments);
        }
        else if (function.Equals ("LocalNowOffset")) {
          log.DebugFormat ("ReplaceKey: " +
                           "process LocalNowOffset custom function");
          string[] args = arguments.Split (new char[] { ',' }, 2);
          return DateTime.Now.Add (TimeSpan.Parse (args[1])).ToString (args[0]);
        }
        else if (function.Equals ("UtcNowOffset")) {
          log.DebugFormat ("ReplaceKey: " +
                           "process UtcNowOffset custom function");
          string[] args = arguments.Split (new char[] { ',' }, 2);
          return DateTime.UtcNow.Add (TimeSpan.Parse (args[1])).ToString (args[0]);
        }
        else {
          log.ErrorFormat ("ReplaceKey: " +
                           "unsupported custom function {0} " +
                           "=> do not change it",
                           function);
          return m.Value;
        }
      }
    }


    /// <summary>
    /// Check if the factory has an action in case the synchronization is ok
    /// </summary>
    /// <returns></returns>
    public bool CheckSynchronizationOkAction ()
    {
      IDictionary<string, string> prefixToNamespace = new Dictionary<string, string> ();
      prefixToNamespace["odbc"] = PulseResolver.PULSE_ODBC_NAMESPACE;
      string configurationValue = GetConfigurationValue ("//@odbc:synchronizationok", prefixToNamespace);
      if (null == configurationValue) {
        configurationValue = GetConfigurationValue ("//@odbc:applicationstatestatus", prefixToNamespace);
        if (null == configurationValue) {
          log.Debug ("CheckSynchronizationOkAction: " +
                     "return false");
          return false;
        }
      }

      log.DebugFormat ("CheckSynchronizationOkAction: " +
                       "value is {0}, return true",
                       configurationValue);
      return true;
    }

    /// <summary>
    /// Get a configuration value in the XML configuration file / schema
    /// </summary>
    /// <param name="xpath"></param>
    /// <returns></returns>
    public string GetConfigurationValue (string xpath)
    {
      return GetConfigurationValue (xpath,
                                    new Dictionary<string, string> ());
    }

    /// <summary>
    /// Get a configuration value in the XML configuration file / schema
    /// </summary>
    /// <param name="xpath"></param>
    /// <param name="prefixToNamespace">Dictionary prefix to namespace</param>
    /// <returns></returns>
    public string GetConfigurationValue (string xpath,
                                         IDictionary<string, string> prefixToNamespace)
    {
      try {
        XmlNamespaceManager namespaceResolver = new XmlNamespaceManager (m_schema.NameTable);
        XPathNavigator navigator = m_schema.DocumentElement.CreateNavigator ();
        foreach (KeyValuePair<string, string> item in prefixToNamespace) {
          namespaceResolver.AddNamespace (item.Key, item.Value);
        }
        XPathNavigator node = navigator.SelectSingleNode (xpath,
                                                          namespaceResolver);
        if (null == node) {
          log.ErrorFormat ("GetConfigurationValue: " +
                           "xpath={0} could not be resolved",
                           xpath);
          return null;
        }
        else {
          log.DebugFormat ("GetConfigurationValue: " +
                           "xpath {0} returned {1}",
                           xpath, node.Value);
          return node.Value;
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("GetConfigurationValue: for xpath {0} exception {1}", xpath, ex);
        throw new XPathException ("XPath evaluation error in GetConfigurationValue", ex);
      }
    }

    /// <summary>
    /// Specialized method to build the DOMDocument
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="optional"></param>
    /// <returns></returns>
    public XmlDocument GetData (CancellationToken cancellationToken, bool optional = false)
    {
      XmlDocument doc = new XmlDocument ();
      m_connection =
        new OdbcConnection (m_connectionParameters.OdbcConnectionString ());

      try {
        try {
          m_connection.Open ();
        }
        catch (Exception ex) {
          log.ErrorFormat ("GetData: connection.Open with parameter={0} failed with exception {1}", m_connectionParameters.OdbcConnectionString (), ex);
          throw new DatabaseException ("Connection.Open error with parameter=" + m_connectionParameters.OdbcConnectionString (), ex);
        }

        XmlElement schemaRoot = m_schema.DocumentElement;
        System.Diagnostics.Debug.Assert (null != schemaRoot); // tester in TestSchema

        XmlElement docRoot = doc.ImportNode (schemaRoot, false) as XmlElement;
        doc.AppendChild (docRoot);
        foreach (XmlNode child in schemaRoot.ChildNodes) {
          if (child is XmlElement) {
            this.InsertElement (doc,
                                (XmlElement)child,
                                doc.DocumentElement);
          }
        }
      }
      finally {
        m_connection.Dispose ();
      }

      // XPath replacement and odbc:if process
      List<XmlElement> elementsToRemove = new List<XmlElement> ();
      foreach (XmlNode node in doc.GetElementsByTagName ("*")) {
        XmlElement element = node as XmlElement;
        List<XmlAttribute> attributesToRemove = new List<XmlAttribute> ();
        try {
          XPathEvaluator evaluator = new XPathEvaluator (element);
          foreach (XmlAttribute attribute in element.Attributes) {
            attribute.Value = evaluator.Replace (attribute.Value);
            if (attribute.NamespaceURI.Equals (PulseResolver.PULSE_ODBC_NAMESPACE)
                && !attribute.LocalName.Equals ("synchronizationok")
                && !attribute.LocalName.Equals ("synchronizationerror")
                && !attribute.LocalName.Equals ("applicationstatestatus")) {
              if (attribute.LocalName.Equals ("if")) {
                if (attribute.Value.Equals ("False",
                                            StringComparison.InvariantCultureIgnoreCase)
                    || attribute.Value.Equals ("0")) {
                  // Remove the whole element
                  elementsToRemove.Add (element);
                  break;
                }
              }
              // Remove the ODBC attribute except odbc:synchronizationok, odbc:synchronizationerror and odbc:applicationstatestatus
              attributesToRemove.Add (attribute);
            }
          }
        }
        catch (Exception ex) {
          log.ErrorFormat ("GetData: replace errors {0}", ex);
          throw new XPathException ("Replacement errors", ex);
        }
        foreach (XmlAttribute attributeToRemove in attributesToRemove) {
          element.RemoveAttributeNode (attributeToRemove);
        }
      }
      foreach (XmlElement elementToRemove in elementsToRemove) {
        XmlNode parent = elementToRemove.ParentNode as XmlNode;
        parent.RemoveChild (elementToRemove);
      }

      return doc;
    }

    /// <summary>
    /// <see cref="IFactory.FlagSynchronizationAsSuccess" />
    /// </summary>
    /// <param name="document"></param>
    public void FlagSynchronizationAsSuccess (XmlDocument document)
    {
      FlagSynchronization (document, true);
    }

    /// <summary>
    /// <see cref="IFactory.FlagSynchronizationAsFailure" />
    /// </summary>
    /// <param name="document"></param>
    public void FlagSynchronizationAsFailure (XmlDocument document)
    {
      FlagSynchronization (document, false);
    }

    /// <summary>
    /// </summary>
    /// <param name="document"></param>
    /// <param name="success">Success or error ?</param>
    void FlagSynchronization (XmlDocument document, bool success)
    {
      // Initialize the XPathNavigator and namespace manager
      XPathNavigator documentNavigator = document.CreateNavigator ();
      XmlNamespaceManager namespaceManager =
        new XmlNamespaceManager (document.NameTable);
      namespaceManager.AddNamespace ("odbc",
                                     PulseResolver.PULSE_ODBC_NAMESPACE);

      // XPath replacement and odbc:if process
      foreach (XmlNode node in document.GetElementsByTagName ("*")) {
        XmlElement element = node as XmlElement;
        XPathEvaluator evaluator = new XPathEvaluator (element);
        List<XmlAttribute> attributesToRemove = new List<XmlAttribute> ();
        foreach (XmlAttribute attribute in element.Attributes) {
          try {
            attribute.Value = evaluator.Replace (attribute.Value);
          }
          catch (Exception ex) {
            log.WarnFormat ("FlagSynchronization: " +
                            "evaluating xpath {0} failed " +
                            "(do not substitute it), " +
                            "error is {1}",
                            attribute.Value,
                            ex);
          }
        }
      }

      // Update applicationstatestatus if set
      {
        XPathNodeIterator nodes;
        try {
          XPathExpression xpath = XPathExpression.Compile ("//@odbc:applicationstatestatus",
                                                           namespaceManager);
          nodes = documentNavigator.Select (xpath);
        }
        catch (Exception ex) {
          log.ErrorFormat ("FlagSynchronization: error while selecting the applicationstatestatus {0}", ex);
          throw new XPathException ("applicationstatestatus node request error", ex);
        }
        foreach (XPathNavigator node in
                 nodes) {
          string[] keyValue = node.Value.Split (new Char[] { '=' }, 2);
          if (2 != keyValue.Length) {
            log.ErrorFormat ("FlagSynchronization: " +
                             "invalid key=value {0} for applicationstatestatus",
                             node.Value);
          }
          else {
            try {
              using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
              using (IDAOTransaction transaction = session.BeginTransaction ()) {
                Lemoine.Model.IApplicationState applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
                  .GetApplicationState (keyValue[0]);
                if (null == applicationState) {
                  log.DebugFormat ("FlagSynchronization: " +
                                   "create application state key={0} because it does not exist yet",
                                   keyValue[0]);
                  applicationState = ModelDAOHelper.ModelFactory.CreateApplicationState (keyValue[0]);
                }
                applicationState.Value = keyValue[1];
                ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (applicationState);
                transaction.Commit ();
              }
            }
            catch (Exception ex) {
              log.ErrorFormat ("FlagSynchronization: error while updating the application state to {0}, {1}",
                keyValue[0], ex);
              throw new DatabaseException ("FlagSynchronization error", ex);
            }
          }
        }
      }

      // Run the synchronizationok requests
      {
        XPathExpression xpath = (success)
          ? XPathExpression.Compile ("//@odbc:synchronizationok",
                                     namespaceManager)
          : XPathExpression.Compile ("//@odbc:synchronizationerror",
                                     namespaceManager);
        foreach (XPathNavigator node in
                 documentNavigator.Select (xpath)) {
          string sqlRequest = node.Value;
          using (m_connection = new OdbcConnection (m_connectionParameters.OdbcConnectionString ())) {
            m_connection.Open ();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using (var command = new OdbcCommand (sqlRequest, m_connection)) {
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
              try {
                command.ExecuteNonQuery ();
              }
              catch (Exception ex) {
                log.ErrorFormat ("FlagSynchronization(success={0}): " +
                                 "failed at executing SQL request {1}, " +
                                 "error is {2}",
                                 success,
                                 sqlRequest,
                                 ex);
                throw new DatabaseException ("ExecuteNonQuery error", ex);
              }
            }
          }
        }
      }
    }

    #endregion // Methods
  }
}

#endif // NETSTANDARD || NET48_OR_GREATER || NETCOREAPP
