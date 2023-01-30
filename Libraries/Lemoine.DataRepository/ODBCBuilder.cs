// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;

using Lemoine.Core.Log;
using System.Data.Odbc;
using Lemoine.Info;
using System.Threading;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// ODBC builder interface
  ///
  /// <para>
  /// The aim of this builder is to execute SQL requests
  /// from a schema in XML and a DOMDocument
  /// </para>
  /// 
  /// <para>
  /// The schema contains:
  /// <list type="bullet">
  /// <item>the nodes of the DOMDocument</item>
  /// <item>the parameters to connect to the database</item>
  /// <item>SQL requests to execute in an attribute 'pulse:request'</item>
  /// </list>
  /// </para>
  /// 
  /// <para>
  /// The SQL requests are given in attributes that are names "pulse:request".
  /// They can take for parameters xpath expressions in {}.
  /// </para>
  /// 
  /// <para>
  /// The connection parameters are given in the root element
  /// in the following attributes:
  /// <list type="bullet">
  /// <item>pulse:dnsname</item>
  /// <item>pulse:user</item>
  /// <item>pulse:password</item>
  /// </list>
  /// </para>
  /// 
  /// <example>
  /// Here is an example of a schema:
  /// <![CDATA[
  /// <root pulse:dnsname="mydatabase" pulse:user="myself" pulse:password="pass">
  ///   <job name="any" hours="1.0"
  ///        pulse:request="INSERT INTO table VALUES ('{@name}', '{@hours}')" />
  /// </root>
  /// ]]>
  /// </example>
  /// </summary>
  public class ODBCBuilder : IBuilder
  {
    #region Exceptions
    /// <summary>
    /// Raised when an error was found in the schema
    /// </summary>
    public class SchemaException : RepositoryException
    {
      /// <summary>
      /// Initializes a new instance of the SchemaException class.
      /// <see cref="RepositoryException">RepositoryException constructor</see>
      /// </summary>
      public SchemaException () : base ()
      {
      }

      /// <summary>
      /// Initializes a new instance of the SchemaException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public SchemaException (string message) : base (message)
      {
      }
    }

    /// <summary>
    /// Database or ODBC exception
    /// </summary>
    public class DatabaseException : RepositoryException
    {
      /// <summary>
      /// Initializes a new instance of the DatabaseException class.
      /// <see cref="RepositoryException">RepositoryException constructor</see>
      /// </summary>
      public DatabaseException () : base ()
      {
      }

      /// <summary>
      /// Initializes a new instance of the DatabaseException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public DatabaseException (string message) : base (message)
      {
      }
    }

    /// <summary>
    /// XPath evaluation exception
    /// </summary>
    public class XPathException : RepositoryException
    {
      /// <summary>
      /// Initializes a new instance of the XPathException class.
      /// <see cref="RepositoryException">RepositoryException constructor</see>
      /// </summary>
      public XPathException () : base ()
      {
      }

      /// <summary>
      /// Initializes a new instance of the XPathException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public XPathException (string message) : base (message)
      {
      }
    }
    #endregion

    #region Members
    OdbcConnection m_connection = null;
    readonly ConnectionParameters m_connectionParameters = new ConnectionParameters ();
    readonly XmlDocument m_schema;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (ODBCBuilder).FullName);

    #region Getters / Setters
    /// <summary>
    /// Connection parameters (read-only)
    /// </summary>
    public ConnectionParameters ConnectionParameters
    {
      get { return m_connectionParameters; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="source">Source type of data</param>
    /// <param name="data">Data: raw string or file name</param>
    public ODBCBuilder (XmlSourceType source, string data)
    {
      m_schema = new XmlDocument ();
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
    #endregion

    #region Methods
    /// <summary>
    /// Give the possibility to use an asynchronous commit
    /// </summary>
    public void SetAsynchronousCommit ()
    {
      // Do nothing
      return;
    }

    /// <summary>
    /// Build an XML file from a DOMDocument
    /// 
    /// <see cref="IBuilder.Build">IBuilder.Build</see>
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="cancellationToken"></param>
    public void Build (System.Xml.XmlDocument doc, CancellationToken cancellationToken)
    {
      if (null == doc) {
        log.Error ("Build: Building a null document => throw an error because it is unexpected");
        throw new RepositoryException ("XmlDocument is null in Build method");
      }

      m_connection =
        new OdbcConnection (m_connectionParameters.OdbcConnectionString);
      try {
        m_connection.Open ();

        XmlElement schemaRoot = m_schema.DocumentElement;
        System.Diagnostics.Debug.Assert (null != schemaRoot); // tester in TestSchema

        XmlElement docRoot = doc.DocumentElement;
        if (null == docRoot) {
          log.Info ("Build: doc argument is null => do nothing");
          return;
        }

        XmlElement mergeRoot = (XmlElement)schemaRoot.CloneNode (false);
        this.Merge (ref mergeRoot,
                    schemaRoot,
                    docRoot);

        cancellationToken.ThrowIfCancellationRequested ();

        ProcessElement (mergeRoot);
      }
      finally {
        m_connection.Dispose ();
      }
    }

    /// <summary>
    /// Get in the schema the value of a given node
    ///
    /// <para>
    /// To describe the searched node, use an XPath expression.
    /// </para>
    /// </summary>
    /// <param name="xpath">XPath expression of the searched node</param>
    /// <returns>Retrieved value from this node or null if not found</returns>
    public string GetNodeValue (string xpath)
    {
      System.Diagnostics.Debug.Assert (null != m_schema.DocumentElement); // tested in TestSchema

      XPathNavigator xpathNavigator =
        m_schema.DocumentElement.CreateNavigator ();
      XPathNavigator node =
        xpathNavigator.SelectSingleNode (xpath);
      if (null == node) {
        log.InfoFormat ("GetNodeValue xpath={0}: " +
                        "XPath could not be evaluated in schema root element",
                        xpath);
        return null;
      }
      else {
        log.DebugFormat ("GetNodeValue xpath={0}: " +
                         "return {1}",
                         xpath, node.Value);
        return node.Value;
      }
    }

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
      System.Diagnostics.Debug.Assert (null != m_schema.DocumentElement); // tester in TestSchema ()

      // DSNName
      string dsnName =
        m_schema.DocumentElement.GetAttribute ("dsnname",
                                             PulseResolver.PULSE_ODBC_NAMESPACE);
      if (0 == dsnName.Length) {
        throw new SchemaException ("No connection parameter root/@pulse:dsnname");
      }
      m_connectionParameters.DsnName = dsnName;

      // UserName
      m_connectionParameters.Username =
        m_schema.DocumentElement.GetAttribute ("user",
                                             PulseResolver.PULSE_ODBC_NAMESPACE);
      if (0 == m_connectionParameters.Username.Length) {
        log.InfoFormat ("InitConnectParam: " +
                        "no parameter pulse:user " +
                        "in the root element of the schema");
      }

      // Password
      m_connectionParameters.Password =
        m_schema.DocumentElement.GetAttribute ("password",
                                             PulseResolver.PULSE_ODBC_NAMESPACE);
      if (0 == m_connectionParameters.Password.Length) {
        log.InfoFormat ("InitConnectParam: " +
                        "no parameter pulse:password " +
                        "in the root element of the schema");
      }
    }

    /// <summary>
    /// Process the SQL request within an element
    /// </summary>
    /// <param name="e"></param>
    private void ProcessElement (XmlElement e)
    {
      System.Diagnostics.Debug.Assert (null != e);
      log.DebugFormat ("ProcessElement tag={0} /B",
                       e.Name);

      string request =
        e.GetAttribute ("request",
                        PulseResolver.PULSE_ODBC_NAMESPACE);
      if (request.Length > 0) {
        log.InfoFormat ("PrcessElement tag={0}: " +
                        "SQL request {1} was found in element {0}",
                        e.Name,
                        request);
        request = this.ReplaceKey (request, e);
        request.Replace ("\\", "\\\\");
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        OdbcCommand command = new OdbcCommand (request, m_connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
        command.ExecuteNonQuery ();
      }

      // Recursive call
      foreach (XmlNode child in e.ChildNodes) {
        if (child is XmlElement) {
          this.ProcessElement ((XmlElement)child);
        }
      }
    }

    /// <summary>
    /// Replace in a given SQL request the parameters by their values
    /// 
    /// A parameter is an XPath expression in {} that is evaluated in this method
    /// </summary>
    /// <param name="sqlRequest">SQL request to process</param>
    /// <param name="e">Current element / context (corresponds to .)</param>
    /// <returns>The modified sqlRequest</returns>
    private string ReplaceKey (string sqlRequest,
                               XmlElement e)
    {
      System.Diagnostics.Debug.Assert (null != e);

      log.DebugFormat ("ReplaceKey sqlRequest={0} element={1} /B",
                       sqlRequest, e.Name);

      // Search for any XPath expression in {}
      Regex xpathRegex = new Regex ("\\{([^}]*)\\}");
      string result = sqlRequest;
      foreach (Match match in xpathRegex.Matches (sqlRequest)) {
        string xpath = match.Groups[1].Value;
        XPathNavigator xpathNavigator = e.CreateNavigator ();
        XPathNavigator node =
          xpathNavigator.SelectSingleNode (xpath);
        if (null == node) {
          log.ErrorFormat ("ReplaceKey sqlRequest={0} element={1}: " +
                           "XPath {2} could not be evaluated",
                           sqlRequest, e.Name, match.Groups[1].Value);
          throw new XPathException (String.Format ("Bad XPath={0} in element={1}",
                                                   match.Groups[1].Value,
                                                   e.Name));
        }
        log.DebugFormat ("ReplaceKey sqlRequest={0} element={1}: " +
                         "value={2} found for XPath={3}",
                         sqlRequest, e.Name,
                         node.Value, xpath);
        result = result.Replace (match.Value, node.Value);
      }
      return result;
    }

    /// <summary>
    /// Merge the schema element with the doc element
    ///
    /// <para>
    /// The new element is built first from
    /// all the sub-elements of schemaElem,
    /// except when the element has a pulse:request attribute
    /// whose corresponding element cannot be found in docElem.
    /// </para>
    /// 
    /// <para>
    /// The resulted element is then completed with the sub-elements
    /// and attributes of docElem.
    /// </para>
    /// 
    /// <remarks>
    /// There are the following pre-requisites:
    /// <list type="bullet">
    /// <item>mergeElem, schemaElem and docElem represent all of them an element with the same tag name</item>
    /// <item>mergeElem contains already all the attributes of schemaElem</item>
    /// <item>mergeElem and schemaElem belong both of them to the same DOMDocument</item>
    /// </list>
    /// </remarks>
    /// </summary>
    /// <param name="mergeElem">Resulted element</param>
    /// <param name="schemaElem">Schema element</param>
    /// <param name="docElem">Document element</param>
    private void Merge (ref XmlElement mergeElem,
                        XmlElement schemaElem,
                        XmlElement docElem)
    {
      // Pre-requisite:
      // - mergeElem, schemaElem and docElem represent all of them
      //   an element with the same tag name
      // - mergeElem contains already all the attributes of schemaElem
      // - mergeElem and schemaElem belong both of them to the same DOMDocument
      System.Diagnostics.Debug.Assert (null != mergeElem);
      System.Diagnostics.Debug.Assert (null != schemaElem);
      System.Diagnostics.Debug.Assert (null != docElem);
      System.Diagnostics.Debug.Assert (mergeElem.Name.Equals (schemaElem.Name));
      System.Diagnostics.Debug.Assert (mergeElem.Name.Equals (docElem.Name));

      log.DebugFormat ("Merge tag={0} /B",
                       mergeElem.Name);

      // All the not empty attributes of docElem are copied into mergeElem
      foreach (XmlAttribute attribute in docElem.Attributes) {
        if (0 == attribute.Value.Length) {
          log.InfoFormat ("Merge tag={0}: " +
                          "Empty attribute {1}={2} in docElem",
                          mergeElem.Name,
                          attribute.Name, attribute.Value);
        }
        else { // Not empty attribute
          log.DebugFormat ("Merge tag={0}: " +
                           "Add attribute {1}={2} of docElem into mergeElem",
                           mergeElem.Name,
                           attribute.Name, attribute.Value);
          mergeElem.SetAttribute (attribute.Name, attribute.Value);
        }
      }

      // All the child elements of schemaElem are visited now
      foreach (XmlNode schemaNode in schemaElem.ChildNodes) {
        if (schemaNode is XmlElement) {
          XmlElement schemaElement = schemaNode as XmlElement;
          log.DebugFormat ("Merge tag={0}: " +
                           "Visiting schema element {1}",
                           mergeElem.Name,
                           schemaElement.Name);

          // If at least one child element with the same tag name
          // was found in docElem,
          // for all such child element in docElem,
          // clone the child element of schemaElem, add it into mergeElem,
          // apply recursively this method on these three elements
          bool docElementFound = false;
          foreach (XmlNode docNode in docElem.ChildNodes) {
            XmlElement docElement = docNode as XmlElement;
            if ((docElement != null)
                && (docElement.Name.Equals (schemaElement.Name))) {
              log.DebugFormat ("Merge tag={0}: " +
                               "Element {1} is in both Schema and Doc elements",
                               mergeElem.Name,
                               schemaElement.Name);
              docElementFound = true;
              XmlNode newNode = schemaElement.CloneNode (false);
              mergeElem.AppendChild (newNode);
              XmlElement newElement = newNode as XmlElement;
              this.Merge (ref newElement,
                          schemaElement,
                          docElement);
            }
          }

          // If no child element with the same tag name was found in docElem:
          // and if the child element of schemaElem
          // does not contain a pulse:request attribute,
          // clone recursively this child element and add it into mergeElem
          if ((false == docElementFound)
              && (false == schemaElement.HasAttribute ("request",
                                                       PulseResolver.PULSE_ODBC_NAMESPACE))) {
            log.DebugFormat ("Merge tag={0}: " +
                             "Schema element {1} is copied recursively " +
                             "because it was not found in docElem " +
                             "and it does not contain any pulse:request attribute",
                             mergeElem.Name,
                             schemaElement.Name);
            mergeElem.AppendChild (CloneRecursivelyElement (schemaElement));
          }
        }
      }
    }

    /// <summary>
    /// Clone recursively an element but ommit the elements
    /// with a pulse:request attribute
    /// 
    /// Pre-requisite: element is not null
    /// </summary>
    /// <param name="element">Element to recursively clone</param>
    /// <returns>Cloned element or null if element has a pulse:request attribute</returns>
    private static XmlElement CloneRecursivelyElement (XmlElement element)
    {
      System.Diagnostics.Debug.Assert (null != element);

      if (element.HasAttribute ("request",
                                PulseResolver.PULSE_ODBC_NAMESPACE)) {
        return null;
      }

      XmlElement clonedElement = element.CloneNode (false) as XmlElement;
      // Visit the child elements
      foreach (XmlNode node in element.ChildNodes) {
        if (node is XmlElement) {
          XmlElement newChild = CloneRecursivelyElement (node as XmlElement);
          if (null != newChild) {
            clonedElement.AppendChild (newChild);
          }
        }
      }
      return clonedElement;
    }
    #endregion
  }
}
