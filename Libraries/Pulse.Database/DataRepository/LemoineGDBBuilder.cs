// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD || NET48 || NETCOREAPP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using System.Threading;
using Lemoine.Database.Persistent;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Builder interface of LemoineGDB persistent classes
  ///
  /// The aim of this builder is to update the tables
  /// of LemoineGDB from a DOMDocument
  /// 
  /// The following attributes of namespace urn:pulse.lemoinetechnologies.com:synchro:gdb
  /// drives the update of the persistent classes:
  /// <item>action: it takes one of the following values: create, reference, update, id_update</item>
  /// <item>identifiers: list of possible identifiers separated by a space</item>
  /// <item>relation: it takes the value inverse or none</item>
  /// <item>notfound: it is made of one or several of these following values (separated by a space): fail, error, log, create</item>
  /// <item>logthreshold: minimum log level to log</item>
  /// </summary>
  public class LemoineGDBBuilder : IBuilder
  {
    #region Exceptions
    /// <summary>
    /// Raised when an element name references an invalid type
    /// </summary>
    public class InvalidTypeException : RepositoryException
    {
      /// <summary>
      /// Initializes a new instance of the InvalidTypeException class.
      /// <see cref="RepositoryException">RepositoryException constructor</see>
      /// </summary>
      public InvalidTypeException () : base ()
      {
      }

      /// <summary>
      /// Initializes a new instance of the InvalidTypeException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public InvalidTypeException (string message) : base (message)
      {
      }
    }

    /// <summary>
    /// Raised when an element references a property that does not exist
    /// </summary>
    public class UnknownPropertyException : RepositoryException
    {
      /// <summary>
      /// Initializes a new instance of the UnknownPropertyException class.
      /// <see cref="RepositoryException">RepositoryException constructor</see>
      /// </summary>
      public UnknownPropertyException () : base ()
      {
      }

      /// <summary>
      /// Initializes a new instance of the UnknownPropertyException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public UnknownPropertyException (string message) : base (message)
      {
      }
    }

    /// <summary>
    /// Raised when the given identifiers do not reference a unique record
    /// </summary>
    public class NotUniqueException : RepositoryException
    {
      /// <summary>
      /// Initializes a new instance of the NotUniqueException class.
      /// <see cref="RepositoryException">RepositoryException constructor</see>
      /// </summary>
      public NotUniqueException () : base ()
      {
      }

      /// <summary>
      /// Initializes a new instance of the NotUniqueException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public NotUniqueException (string message) : base (message)
      {
      }

      /// <summary>
      /// Initializes a new instance of the NotUniqueException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      /// <param name="ex"></param>
      public NotUniqueException (string message, Exception ex) : base (message, ex)
      {
      }
    }

    /// <summary>
    /// Raised when a persistent class to update or to reference was not found
    /// </summary>
    public class NotFoundException : RepositoryException
    {
      /// <summary>
      /// Initializes a new instance of the NotFoundException class.
      /// <see cref="RepositoryException">RepositoryException constructor</see>
      /// </summary>
      public NotFoundException () : base ()
      {
      }

      /// <summary>
      /// Initializes a new instance of the NotFoundException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public NotFoundException (string message) : base (message)
      {
      }
    }

    /// <summary>
    /// Raised when there is a serialization error
    /// </summary>
    public class SerializationException : RepositoryException
    {
      /// <summary>
      /// Initializes a new instance of the SerializationException class.
      /// <see cref="RepositoryException">RepositoryException constructor</see>
      /// </summary>
      public SerializationException () : base ()
      {
      }

      /// <summary>
      /// Initializes a new instance of the SerializationException class with a specified error message.
      /// <see cref="RepositoryException">Exception constructor</see>
      /// </summary>
      /// <param name="message">The message that describes the error.</param>
      public SerializationException (string message) : base (message)
      {
      }
    }
    #endregion // Exceptions

    static readonly string ACTION_KEY = "action";
    static readonly string ACTION_NONE = "none";
    static readonly string ACTION_CREATE = "create";
    static readonly string ACTION_ID = "id";
    static readonly string ACTION_REFERENCE = "reference";
    static readonly string ACTION_UPDATE = "update";
    static readonly string ACTION_ID_UPDATE = "id_update";

    static readonly string RELATION_KEY = "relation";
    static readonly string RELATION_INVERSE = "inverse";
    static readonly string RELATION_NONE = "none";

    static readonly string NOTFOUND_KEY = "notfound";
    static readonly string NOTFOUND_FAIL = "fail";
    static readonly string NOTFOUND_ERROR = "error";
    static readonly string NOTFOUND_LOG = "log";
    static readonly string NOTFOUND_CREATE = "create";

    static readonly string IF = "if";
    static readonly string IFNOT = "ifnot";

    #region Members
    LogLevel m_logThreshold = LogLevel.INFO;
    bool m_errorLogged = false;
    readonly IDictionary<Type, XmlSerializer> m_xmlSerializers = new Dictionary<Type, XmlSerializer> ();
    bool m_asynchronousCommit = false;
    bool m_autoTransaction = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (LemoineGDBBuilder).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LemoineGDBBuilder () { }

    #region Methods
    /// <summary>
    /// Give the possibility to use an asynchronous commit
    /// </summary>
    public void SetAsynchronousCommit ()
    {
      m_asynchronousCommit = true;
    }

    IDAOTransaction BeginTransaction (IDAOSession session)
    {
      log.Debug ("BeginTransaction /B");
      IDAOTransaction transaction = session.BeginTransaction ();
      if (m_asynchronousCommit) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off;
      }
      return transaction;
    }

    /// <summary>
    /// Update the Revision/Modification persistent classes
    /// from the XmlDocument in parameter
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="doc"></param>
    public void Build (System.Xml.XmlDocument doc, CancellationToken cancellationToken)
    {
      m_errorLogged = false;

      // - Properties in attributes
      // . logthreshold
      string logThresholdAttribute =
        doc.DocumentElement.GetAttribute ("logthreshold",
                                          PulseResolver.PULSE_GDB_NAMESPACE);
      if (logThresholdAttribute.Length > 0) {
        try {
          m_logThreshold = (LogLevel)Enum.Parse (typeof (LogLevel),
                                                  logThresholdAttribute);
        }
        catch (Exception ex) {
          log.Error ($"Build: Parsing {logThresholdAttribute} for logThreshold failed ", ex);
        }
      }
      // . asynchronouscommit
      string asynchronousCommitAttribute =
        doc.DocumentElement.GetAttribute ("asynchronouscommit",
                                          PulseResolver.PULSE_GDB_NAMESPACE);
      if (asynchronousCommitAttribute.Equals ("true") || asynchronousCommitAttribute.Equals ("1")) {
        SetAsynchronousCommit ();
      }
      // . autotransaction
      string autoTransactionAttribute =
        doc.DocumentElement.GetAttribute ("autotransaction",
                                          PulseResolver.PULSE_GDB_NAMESPACE);
      if (autoTransactionAttribute.Equals ("false") || autoTransactionAttribute.Equals ("0")) {
        log.Info ("Build: de-activate the auto-transaction");
        m_autoTransaction = false;
      }
      cancellationToken.ThrowIfCancellationRequested ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IDAOTransaction transaction = null;
        try {
          if (m_autoTransaction) {
            log.Debug ("Build: begin the main transaction (auto-transaction)");
            transaction = BeginTransaction (session);
          }

          IList<XmlElement> childElementsToRemove = new List<XmlElement> ();
          foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
            cancellationToken.ThrowIfCancellationRequested ();
            if (!(node is XmlElement)) {
              continue;
            }
            XmlElement element = node as XmlElement;
            if (false == DeserializeAndProcessElement (element, null)) {
              log.Debug ($"Build: DeserializeAndProcessElement of element {element.Name} return false => remove the element");
              childElementsToRemove.Add (element);
            }
          }
          foreach (XmlElement childElementToRemove in childElementsToRemove) {
            cancellationToken.ThrowIfCancellationRequested ();
            doc.DocumentElement.RemoveChild (childElementToRemove);
          }
          if (null != transaction) {
            log.Debug ("Build: commit the main transaction (auto-transaction)");
            transaction.Commit ();
          }
        }
        catch (Exception ex) {
          log.Error ($"Build: exception raised", ex);
          if (!m_errorLogged) {
            AddSynchronizationError (ex.ToString (), doc.OuterXml);
          }
          throw;
        }
        finally {
          if (null != transaction) {
            transaction.Dispose ();
          }
        }
      }
    }

    bool OpenTransaction (XmlElement element)
    {
      string transactionAttribute = element.GetAttribute ("transaction",
                                                          PulseResolver.PULSE_GDB_NAMESPACE);
      return transactionAttribute.Equals ("true") || transactionAttribute.Equals ("1");
    }

    /// <summary>
    /// De-serialize an XML element and process it recursively
    /// </summary>
    /// <param name="element">XML element to process</param>
    /// <param name="parent">Parent object</param>
    /// <returns>false if the element must be removed (notfound or any wrong condition)</returns>
    private bool DeserializeAndProcessElement (XmlElement element, object parent)
    {
      log.Debug ($"DeserializeAndProcessElement: element={element.Name} parent={parent} before checking condition");

      // Check the gdb:ifnot attribute
      if (false == CheckCondition (element)) {
        log.Debug ($"DeserializeAndProcessElement: gdb:if or gdb:ifnot condition is not realized, remove the element");
        return false;
      }

      // Process the element
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IDAOTransaction transaction = null;
        try {
          object elementObject = null;
          string action = element.GetAttribute (ACTION_KEY,
                                                PulseResolver.PULSE_GDB_NAMESPACE);
          if (action.Equals (ACTION_CREATE)
              || action.Equals (ACTION_ID)
              || action.Equals (ACTION_REFERENCE)
              || action.Equals (ACTION_UPDATE)
              || action.Equals (ACTION_ID_UPDATE)) {
            Type type = Type.GetType ("Lemoine.GDBPersistentClasses." + element.Name + ", Pulse.Database");
            if (null == type) {
              log.ErrorFormat ("type {0} from element name is unknown " +
                               "=> action {1} can't be processed",
                               element.Name,
                               action);
              AddSynchronizationError ("Type " + element.Name + " is unknown and was not processed",
                                       element.OuterXml);
              throw new InvalidTypeException ("Type " + element.Name + " is unknown");
            }
            else { // null != type
              try { // Deserialize
                XmlSerializer xmlSerializer;
                if (m_xmlSerializers.ContainsKey (type)) {
                  xmlSerializer = m_xmlSerializers[type];
                }
                else {
                  xmlSerializer = new XmlSerializer (type);
                  m_xmlSerializers.Add (type, xmlSerializer);
                }
                TextReader reader = new StringReader (element.OuterXml);
                elementObject =
                  xmlSerializer.Deserialize (reader);
              }
              catch (Exception ex) {
                log.Error ($"Deserialize of element {element.Name} failed", ex);
                AddSynchronizationError ("Deserialization error " + ex,
                                         element.OuterXml);
                throw;
              }

              log.DebugFormat ("DeserializeAndProcesElement: " +
                               "after de-serializing it, element {0} is {1}",
                               element.Name, elementObject);

              if (OpenTransaction (element)) {
                transaction = BeginTransaction (session);
              }

              if (false == ProcessElement (ref elementObject, element, parent)) {
                // notfound
                log.Warn ($"DeserializeAndProcessElement: element {element.Name} was not found in ProcessElement => return false");
                if (null != transaction) {
                  transaction.Commit ();
                }
                return false;
              }
            } // type != null
          } // there is an action

          // Begin the transaction if it has not been started yet
          if ((null == transaction) && OpenTransaction (element)) {
            transaction = BeginTransaction (session);
          }

          // Process the children recursively
          // - with pulse:relation="inverse" or pulse:relation="none"
          // - or all children if there is no action
          IList<XmlElement> childElementsToRemove = new List<XmlElement> ();
          foreach (XmlNode node in element.ChildNodes) {
            if (!(node is XmlElement)) {
              continue;
            }
            XmlElement child = node as XmlElement;
            string relation = child.GetAttribute (RELATION_KEY,
                                                  PulseResolver.PULSE_GDB_NAMESPACE);
            log.DebugFormat ("DeserializeAndProcessElement: " +
                             "visiting in element {0} child {1} with element action={2} and child relation={3}",
                             element.Name, child.Name, action, relation);
            if ((0 == action.Length)
                || action.Equals (ACTION_NONE)
                || relation.Equals (RELATION_INVERSE)
                || relation.Equals (RELATION_NONE)) {
              if (false == DeserializeAndProcessElement (child, elementObject)) {
                log.DebugFormat ("DeserializeAndProcessElement: " +
                                 "recursive call of DeserializeAndProcessElement returned false " +
                                 "=> remove the child {0} from element {1}",
                                 child.Name, element.Name);
                childElementsToRemove.Add (child);
              }
            }
          }
          foreach (XmlElement childElementToRemove in childElementsToRemove) {
            element.RemoveChild (childElementToRemove);
          }
          if (null != transaction) {
            transaction.Commit ();
          }
        }
        finally {
          if (null != transaction) {
            log.Debug ("DeserializeAndProcessElement: " +
                       "dispose the transaction");
            transaction.Dispose ();
          }
        }
      }

      return true;
    }

    bool CheckCondition (XmlElement element)
    {
      return CheckConditionIf (element) && CheckConditionIfNot (element);
    }

    bool CheckConditionIf (XmlElement element)
    {
      log.DebugFormat ("CheckConditionIf /B: " +
                       "element={0}",
                       element.Name);

      XmlAttribute ifAttribute = element.GetAttributeNode (IF,
                                                           PulseResolver.PULSE_GDB_NAMESPACE);
      if (null == ifAttribute) {
        log.DebugFormat ("CheckConditionIf: " +
                         "no attribute if in element {0}",
                         element.Name);
        return true;
      }
      return EvalCondition (element, ifAttribute);
    }

    bool CheckConditionIfNot (XmlElement element)
    {
      log.DebugFormat ("CheckConditionIfNot /B: " +
                       "element={0}",
                       element.Name);

      XmlAttribute ifnotAttribute = element.GetAttributeNode (IFNOT,
                                                              PulseResolver.PULSE_GDB_NAMESPACE);
      if (null == ifnotAttribute) {
        log.DebugFormat ("CheckConditionIfNot: " +
                         "no attribute ifnot in element {0}",
                         element.Name);
        return true;
      }
      return !EvalCondition (element, ifnotAttribute);
    }

    bool EvalCondition (XmlElement element, XmlAttribute attribute)
    {
      try {
        log.DebugFormat ("EvalCondition: " +
                         "raw attribute value is {0} " +
                         "for element {1}",
                         attribute.Value,
                         element.Name);
        string result =
          new XPathEvaluator (element).Replace (attribute.Value);
        log.DebugFormat ("EvalCondition: " +
                         "new attribute is {0} " +
                         "for element {1}",
                         result,
                         element.Name);
        if (result.Contains ("{")) {
          log.Debug ("EvalCondition: attribute was not fully evaluated (one remaining {) => return false");
          return false;
        }
        return !string.IsNullOrEmpty (result);
      }
      catch (Exception ex) {
        log.Debug ($"EvalCondition: Checking the condition failed on element {element.Name} => return false", ex);
        return false;
      }
      finally {
        Debug.Assert (null != attribute);
        element.RemoveAttributeNode (attribute);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="elementObject"></param>
    /// <param name="element"></param>
    /// <param name="parent"></param>
    /// <returns>false if no element was found and the element should be removed</returns>
    bool ProcessElement (ref object elementObject,
                         XmlElement element, object parent)
    {
      System.Diagnostics.Debug.Assert (null != elementObject);
      Type type = elementObject.GetType ();

      // Process the children first with no attribute pulse:relation (inverse or none)
      IList<XmlElement> childElementsToRemove = new List<XmlElement> ();
      foreach (XmlNode node in element.ChildNodes) {
        if (!(node is XmlElement)) {
          continue;
        }
        XmlElement child = node as XmlElement;
        string relation = child.GetAttribute (RELATION_KEY,
                                              PulseResolver.PULSE_GDB_NAMESPACE);
        if (0 == relation.Length) {
          log.DebugFormat ("ProcessElement: " +
                           "process child {0} with no attribute pulse:relation first",
                           child.Name);
          object childObject;
          PropertyInfo childInfo =
            type.GetProperty (child.Name);
          if (null == childInfo) {
            log.WarnFormat ("ProcessElement: " +
                            "child {0} is not a property of {1}, " +
                            "skip it",
                            child.Name, element.Name);
            AddSynchronizationLog (LogLevel.WARN,
                                   child.Name + " is not a property of " + element.Name,
                                   element.OuterXml);
          }
          else {
            childObject = childInfo.GetValue (elementObject, null);
            if (null == childObject) {
              string error = string.Format ("ProcessElement: " +
                                            "property {0} of {1} was not de-serialized",
                                            child.Name, elementObject);
              log.Error (error);
              AddSynchronizationError (error,
                                       element.OuterXml);
              throw new SerializationException (error);
            }
            if (false == ProcessElement (ref childObject, child, element)) {
              log.Warn ($"ProcessElement: ProcessElement did not find child element {child.Name}");
              childElementsToRemove.Add (child);
            }
            log.DebugFormat ("ProcessElement: " +
                             "set child {0} to element {1}",
                             childObject, elementObject);
            childInfo.SetValue (elementObject, childObject, null);
          }
        }
      }
      foreach (XmlElement childElementToRemove in childElementsToRemove) {
        element.RemoveChild (childElementToRemove);
      }

      // Process the element itself
      string action = element.GetAttribute (ACTION_KEY,
                                            PulseResolver.PULSE_GDB_NAMESPACE);
      if (action.Equals (ACTION_CREATE)) {
        log.Debug ($"ProcessELement: create item {elementObject}");
        if (element.GetAttribute (RELATION_KEY,
                                  PulseResolver.PULSE_GDB_NAMESPACE)
            .Equals (RELATION_INVERSE)) {
          PropertyInfo parentInfo =
            type.GetProperty (element.ParentNode.Name);
          if (null == parentInfo) {
            log.ErrorFormat ("ProcessElement: " +
                             "there is no parent {0} in the properties of {1} " +
                             "although pulse:relation='inverse' is set",
                             element.ParentNode.Name, element.Name);
            string error = "No parent named " +
              element.ParentNode.Name +
              " exists in " +
              element.Name +
              " with pulse:relation='inverse'";
            AddSynchronizationError (error,
                                     element.OuterXml);
            throw new UnknownPropertyException (error);
          }
          log.DebugFormat ("ProcessElement: " +
                           "about to set parent {0} property {1} to {2}",
                           parent, parentInfo.Name, elementObject);
          parentInfo.SetValue (elementObject, parent, null);
          log.DebugFormat ("ProcessElement: " +
                           "parent {0} property {1} to {2} successfully set",
                           parent, parentInfo.Name, elementObject);
        }
        NHibernateHelper.GetCurrentSession ().Save (elementObject);
        NHibernateHelper.GetCurrentSession ().Flush ();
        AddNewAttributes (element, elementObject);
        AddNewElements (element, elementObject);
      }
      else if (action.Equals (ACTION_ID)
               || action.Equals (ACTION_REFERENCE)
               || action.Equals (ACTION_UPDATE)
               || action.Equals (ACTION_ID_UPDATE)) { // Get the item in database
        if (!(elementObject is BaseData)) {
          log.WarnFormat ("the item {0} is not a BaseData " +
                          "=> it can't be a reference or being updated",
                          element.Name);
        }
        else { // deserialized is BaseData
          object databaseItem;
          string[] identifiers = element.GetAttribute ("identifiers",
                                                       PulseResolver.PULSE_GDB_NAMESPACE)
            .Split (new char[] { ' ' });
          if (action.Equals (ACTION_ID)
              || action.Equals (ACTION_ID_UPDATE)) {
            try {
              XmlAttribute idAttribute = element.GetAttributeNode ("Id");
              if (null == idAttribute) {
                log.ErrorFormat ("ProcessElement: " +
                                 "no attribute Id in element {0}",
                                 element.Name);
                throw new ArgumentNullException ();
              }
              log.DebugFormat ("ProcessElement: " +
                               "raw attribute value for Id is {0} " +
                               "for element {1}",
                               idAttribute.Value,
                               element.Name);
              idAttribute.Value =
                new XPathEvaluator (element).Replace (idAttribute.Value);
              log.DebugFormat ("ProcessElement: " +
                               "new Id attribute is {0} " +
                               "for element {1}",
                               idAttribute.Value,
                               element.Name);
              try {
                int id = int.Parse (idAttribute.Value);
                databaseItem = NHibernateHelper.GetCurrentSession ().Get (elementObject.GetType (), id);
              }
              catch (System.FormatException ex) {
                log.Error ($"ProcessElement: Parse of attribute id {idAttribute.Value} as int failed", ex);
                throw;
              }
            }
            catch (Exception ex) {
              log.Error ($"ProcessElement: Getting element {element.Name} from its ID failed", ex);
              AddSynchronizationError ("Id action error for " + element.Name + ex,
                                       element.OuterXml);
              throw;
            }
          }
          else { // Not action="id" (reference or update)
            Debug.Assert (action.Equals (ACTION_REFERENCE) || action.Equals (ACTION_UPDATE));
            BaseData elementBaseData = elementObject as BaseData;
            if (null == elementBaseData) {
              log.Error ("ProcessElement: trying to retrieve an element that is not a BaseData");
              AddSynchronizationError ("Not a BaseData " + element.Name,
                                       element.OuterXml);
              throw new InvalidTypeException ("Not a BaseData " + element.Name);
            }
            else {
              try {
                if (0 == identifiers[0].Length) { // Use the default initializers
                  identifiers = elementBaseData.Identifiers; // Default identifiers
                  databaseItem =
                    elementBaseData.FindPersistentClass (NHibernateHelper.GetCurrentSession ());
                }
                else {
                  databaseItem =
                    elementBaseData.FindPersistentClass (NHibernateHelper.GetCurrentSession (), identifiers);
                }
              }
              catch (Exception ex) {
                log.Error ($"ProcessElement: FindPersistentClass raised for element {element.Name}", ex);
                AddSynchronizationError ("Not unique " + element.Name + " " + ex,
                                         element.OuterXml);
                throw new NotUniqueException ("Not unique " + element.Name, ex);
              }
            }
          }
          if (null != databaseItem) { // Found
            log.DebugFormat ("ProcessElement: " +
                             "{0} was found from database for element {1}",
                             databaseItem, element.Name);
            AddNewAttributes (element, databaseItem);
            AddNewElements (element, databaseItem);
            if (action.Equals (ACTION_UPDATE)
                || action.Equals (ACTION_ID_UPDATE)) {
              log.DebugFormat ("ProcessElement: " +
                               "update item {0} with {1}",
                               elementObject,
                               databaseItem);
              if (element.GetAttribute (RELATION_KEY,
                                        PulseResolver.PULSE_GDB_NAMESPACE)
                  .Equals (RELATION_INVERSE)) {
                log.Debug ($"ProcessElement: update with an inverse relation");
                PropertyInfo parentInfo =
                  type.GetProperty (element.ParentNode.Name);
                if (null == parentInfo) {
                  log.ErrorFormat ("ProcessElement: " +
                                   "there is no parent {0} in the properties of {1} " +
                                   "although the pulse:relation='inverse' is set",
                                   element.ParentNode.Name, element.Name);
                  string error = "No parent named " +
                    element.ParentNode.Name +
                    " exists in " +
                    element.Name +
                    " with pulse:relation='inverse'";
                  AddSynchronizationError (error,
                                           element.OuterXml);
                  throw new UnknownPropertyException (error);
                }
                parentInfo.SetValue (elementObject, parent, null);
              }
              foreach (XmlAttribute attribute in element.Attributes) {
                if (attribute.NamespaceURI.Length > 0) {
                  continue;
                }
                bool isIdentifier = false;
                foreach (string identifier in identifiers) {
                  if (attribute.Name.Equals (identifier)) {
                    isIdentifier = true;
                    break;
                  }
                }
                if (isIdentifier || attribute.Name.Equals ("Id")) {
                  continue;
                }
                PropertyInfo info =
                  type.GetProperty (attribute.LocalName);
                if (null == info) {
                  log.WarnFormat ("ProcessElement: " +
                                  "attribute {0} is not a property of {1}, " +
                                  "skip it",
                                  attribute.LocalName, element.Name);
                  AddSynchronizationLog (LogLevel.WARN,
                                         "Attribute " +
                                         attribute.LocalName +
                                         " is not a property of " +
                                         element.Name,
                                         element.OuterXml);
                }
                else if (null == info.GetSetMethod (true)) {
                  log.WarnFormat ("ProcessElement: " +
                                  "attribute {0} has a property {1} without any setter " +
                                  "=> skip it",
                                  attribute.LocalName, element.Name);
                }
                else {
                  object newValue = info.GetValue (elementObject, null);
                  log.DebugFormat ("ProcessElement: " +
                                   "about to set value {0} to property {1} of {2}",
                                   newValue, info.Name, databaseItem);
                  info.SetValue (databaseItem,
                                 newValue,
                                 null);
                  log.DebugFormat ("ProcessElement: " +
                                   "property {0} was successfully set in {1}",
                                   info.Name, databaseItem);
                }
              }
              elementObject = databaseItem;
              NHibernateHelper.GetCurrentSession ().Update (elementObject);
              NHibernateHelper.GetCurrentSession ().Flush ();
            }
            else if (action.Equals (ACTION_ID)
                     || action.Equals (ACTION_REFERENCE)) {
              elementObject = databaseItem;
            }
          }
          else { // Not found: null == databaseItem
            log.WarnFormat ("ProcessElement: " +
                            "{0} was not found in database",
                            element.Name);
            string notfound = element.GetAttribute (NOTFOUND_KEY,
                                                    PulseResolver.PULSE_GDB_NAMESPACE);
            if (notfound.Contains (NOTFOUND_FAIL)
                || notfound.Contains (NOTFOUND_ERROR)) {
              AddSynchronizationError ("Persistent class not found",
                                       element.OuterXml);
              throw new NotFoundException ("Persistent class " + element.Name);
            }
            if (notfound.Contains (NOTFOUND_LOG)) {
              AddSynchronizationLog (LogLevel.WARN,
                                     "Persistent class not found",
                                     element.OuterXml);
            }
            if (notfound.Contains (NOTFOUND_CREATE)) {
              NHibernateHelper.GetCurrentSession ().Save (elementObject);
              NHibernateHelper.GetCurrentSession ().Flush ();
              AddNewAttributes (element, elementObject);
              AddNewElements (element, elementObject);
            }
            return false;
          }
        } // deserialized is BaseData
      } // action switch

      return true;
    }

    /// <summary>
    /// Complete the XML element with the new attributes
    /// that are listed in the attribute gdb:newattributes
    /// from the associated object
    /// </summary>
    /// <param name="element"></param>
    /// <param name="elementObject"></param>
    void AddNewAttributes (XmlElement element, object elementObject)
    {
      // newattributes
      string[] newAttributes = element.GetAttribute ("newattributes",
                                                     PulseResolver.PULSE_GDB_NAMESPACE)
        .Split (new char[] { ' ' });
      foreach (string newAttribute in newAttributes) {
        if (!string.IsNullOrEmpty (newAttribute)) {
          AddNewAttribute (element, elementObject, newAttribute);
        }
      }
    }

    /// <summary>
    /// Draw the given attribute from the associated element object
    /// and complete the XML element with it
    /// </summary>
    /// <param name="element"></param>
    /// <param name="elementObject"></param>
    /// <param name="newAttribute"></param>
    void AddNewAttribute (XmlElement element, object elementObject, string newAttribute)
    {
      PropertyInfo info =
        elementObject.GetType ().GetProperty (newAttribute);
      if (null == info) {
        log.WarnFormat ("AddNewAttribute: " +
                        "new attribute {0} is not a property of {1}",
                        newAttribute, element.Name);
        AddSynchronizationLog (LogLevel.WARN,
                               "New attribute " +
                               newAttribute +
                               " is not a property of " +
                               element.Name,
                               element.OuterXml);
      }
      else {
        string newAttributeValue =
          info.GetValue (elementObject, null).ToString ();
        log.DebugFormat ("AddNewAttribute: " +
                         "new attribute {0} value is {1}",
                         newAttribute, newAttributeValue);
        element.SetAttribute (newAttribute,
                              newAttributeValue);
      }
    }

    /// <summary>
    /// Complete the XML element with the new elements
    /// that are listed in the attribute gdb:newelements
    /// from the associated object
    /// </summary>
    /// <param name="element"></param>
    /// <param name="elementObject"></param>
    void AddNewElements (XmlElement element, object elementObject)
    {
      // newelements
      string[] newElements = element.GetAttribute ("newelements",
                                                   PulseResolver.PULSE_GDB_NAMESPACE)
        .Split (new char[] { ' ' });
      foreach (string newElement in newElements) {
        if (!string.IsNullOrEmpty (newElement)) {
          AddNewElement (element, elementObject, newElement);
        }
      }
    }

    /// <summary>
    /// Draw the given element from the associated element object,
    /// and complete the XML element with it and its Id
    /// </summary>
    /// <param name="element"></param>
    /// <param name="elementObject"></param>
    /// <param name="newElement"></param>
    void AddNewElement (XmlElement element, object elementObject, string newElement)
    {
      PropertyInfo info =
        elementObject.GetType ().GetProperty (newElement);
      if (null == info) {
        log.WarnFormat ("AddNewElement: " +
                        "new element {0} is not a property of {1}",
                        newElement, element.Name);
        AddSynchronizationLog (LogLevel.WARN,
                               "New element " +
                               newElement +
                               " is not a property of " +
                               element.Name,
                               element.OuterXml);
      }
      else {
        object newElementValue =
          info.GetValue (elementObject, null);
        if (null == newElementValue) {
          log.DebugFormat ("AddNewElement: " +
                           "new element is null " +
                           "=> do nothing");
          return;
        }
        log.DebugFormat ("AddNewElement: " +
                         "new element {0} value is {1}",
                         newElement, newElementValue);
        // Note: there are some problems serializing it directly because of some lazy properties
        // => add only the item and its id
        XmlElement childElement = element.OwnerDocument.CreateElement (newElement);
        element.AppendChild (childElement);
        try {
          PropertyInfo idInfo =
            newElementValue.GetType ().GetProperty ("Id");
          string id = idInfo.GetValue (newElementValue, null).ToString ();
          childElement.SetAttribute ("Id", id);
        }
        catch (Exception ex) {
          log.Warn ($"AddNewElement: retrieving the id of {newElementValue} failed", ex);
        }
      }
    }

    /// <summary>
    /// Add an error log record in the synchronizationlog table,
    /// and set an error log has already been recorded so that it would not
    /// be recorded twice
    /// </summary>
    /// <param name="message"></param>
    /// <param name="xmlElement"></param>
    private void AddSynchronizationError (string message, string xmlElement)
    {
      AddSynchronizationLog (LogLevel.ERROR,
                             message,
                             xmlElement);
      m_errorLogged = true;
    }

    /// <summary>
    /// Add a log record in the synchronization table
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="message"></param>
    /// <param name="xmlElement"></param>
    private void AddSynchronizationLog (LogLevel logLevel,
                                        string message,
                                        string xmlElement)
    {
      if (logLevel >= m_logThreshold) {
        using (ISession logSession = NHibernateHelper.OpenSession ()) {
          string truncatedXmlElement = xmlElement;
          if (truncatedXmlElement.Length > 2047) {
            truncatedXmlElement = truncatedXmlElement.Substring (0, 2047);
          }
          SynchronizationLog synchronizationLog =
            new SynchronizationLog (logLevel, message, truncatedXmlElement);
          if (synchronizationLog.Message.Length > 254) {
            synchronizationLog.Message = synchronizationLog.Message.Substring (0, 254);
          }
          synchronizationLog.Module = this.GetType ().ToString ();
          if (synchronizationLog.Module.Length > 254) {
            synchronizationLog.Module = synchronizationLog.Module.Substring (0, 254);
          }
          logSession.Save (synchronizationLog);
          logSession.Flush ();
        }
      }
    }
    #endregion // Methods

    /// <summary>
    /// Evaluate the XPath given:
    /// <item>a string</item>
    /// <item>the current element</item>
    /// </summary>
    private sealed class XPathEvaluator
    {
      static readonly Regex xpathRegex = new Regex (@"\{([^}]*)\}");
      readonly IXmlNamespaceResolver namespaceResolver = null;
      readonly XPathNavigator navigator = null;

      /// <summary>
      /// Constructor in case a single element must be taken into account
      /// </summary>
      /// <param name="element"></param>
      public XPathEvaluator (XmlElement element)
      {
        navigator = element.CreateNavigator ();
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager (element.OwnerDocument.NameTable);
        namespaceManager.AddNamespace (element.GetPrefixOfNamespace (PulseResolver.PULSE_GDB_NAMESPACE),
                                       PulseResolver.PULSE_GDB_NAMESPACE);
        namespaceManager.AddNamespace (element.GetPrefixOfNamespace (PulseResolver.PULSE_ODBCGDBCONFIG_NAMESPACE),
                                       PulseResolver.PULSE_ODBCGDBCONFIG_NAMESPACE);
        namespaceResolver = namespaceManager;
      }

      /// <summary>
      /// Replace all the XPath found in the s argument by their values
      /// </summary>
      /// <param name="s"></param>
      /// <returns></returns>
      public string Replace (string s)
      {
        return xpathRegex.Replace (s,
                                   new MatchEvaluator (ReplaceXPath));
      }

      private string ReplaceXPath (Match m)
      {
        System.Diagnostics.Debug.Assert (true == m.Success);
        System.Diagnostics.Debug.Assert (2 == m.Groups.Count);
        System.Diagnostics.Debug.Assert (true == m.Groups[1].Success);
        string xpath = m.Groups[1].Value;

        // 1. First try in child
        if (null != navigator) {
          XPathNavigator node = navigator.SelectSingleNode (xpath,
                                                            namespaceResolver);
          if (null != node) {
            return node.Value;
          }
        }

        log.WarnFormat ("ReplaceXPath: " +
                        "XPath {0} was not found " +
                        "return original string {1}",
                        xpath, m.Value);
        return m.Value;
      }
    }
  }
}

#endif // NETSTANDARD || NET48_OR_GREATER || NETCOREAPP
