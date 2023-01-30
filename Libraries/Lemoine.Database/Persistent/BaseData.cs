// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Base data class from which all the base persistent classes may inherit
  /// </summary>
  [Serializable]
  public abstract class BaseData
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BaseData).FullName);

    #region Getters / Setters
    /// <summary>
    /// Default list of possible identifiers
    /// </summary>
    public abstract string[] Identifiers {
      get;
    }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Find the persistent class in the database
    /// that corresponds to this object.
    /// 
    /// To find the right matching persistent classes,
    /// a list of identifiers is given.
    /// </summary>
    /// <param name="identifiers"></param>
    /// <returns></returns>
    public virtual T FindPersistentClass<T> (string[] identifiers) where T: BaseData
    {
      return (T) FindPersistentClass (identifiers);
    }

    /// <summary>
    /// Find the persistent class in the database
    /// that corresponds to this object.
    /// 
    /// To find the right matching persistent classes,
    /// a list of identifiers is given.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="identifiers"></param>
    /// <returns></returns>
    public virtual T FindPersistentClass<T> (ISession session, string[] identifiers) where T: BaseData
    {
      return (T) FindPersistentClass (session, identifiers);
    }

    /// <summary>
    /// Find the persistent class in the database
    /// that corresponds to this object.
    /// 
    /// To find the right matching persistent classes,
    /// a list of identifiers is given.
    /// </summary>
    /// <param name="identifiers"></param>
    /// <returns></returns>
    public virtual BaseData FindPersistentClass (string[] identifiers)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        return FindPersistentClass (session, identifiers);
      }
    }
    
    /// <summary>
    /// Find the persistent class in the database
    /// that corresponds to this object.
    /// 
    /// To find the right matching persistent classes,
    /// a list of identifiers is given.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="identifiers"></param>
    /// <returns></returns>
    public virtual BaseData FindPersistentClass (ISession session,
                                                 string[] identifiers)
    {
      // Note: CreateCriteria does not work with composite-id (bug in NHibernate ?)
      // http://www.codewrecks.com/blog/index.php/2009/04/29/nhibernate-icriteria-and-composite-id-with-key-many-to-one/
      // Because it prevents from using WorkOrderProject for example,
      // use HQL instead
      IDictionary<string, object> restrictions =
        new Dictionary<string, object> ();
      foreach (string identifier in identifiers) {
        if (RefineQuery (ref restrictions, identifier, identifier, this)) {
          break;
        }
      }
      string queryString = "from " + this.GetType ().Name
        + " foo";
      bool first = true;
      foreach (string key in restrictions.Keys) {
        if (first) {
          queryString += " where foo.";
          first = false;
        }
        else {
          queryString += " and foo.";
        }
        queryString += key;
        queryString += "=:";
        queryString += key.Replace('.', '_');
      }
      log.DebugFormat ("FindPersistentClass: " +
                       "HQL query is {0}",
                       queryString);
      IQuery query = session.CreateQuery (queryString);
      foreach (KeyValuePair<string, object> restriction in restrictions) {
        query.SetParameter (restriction.Key.Replace ('.', '_'),
                            restriction.Value);
      }
      return (BaseData) query.UniqueResult ();
    }
    
    /// <summary>
    /// Find the persistent class in the database
    /// that corresponds to this object.
    /// 
    /// A default list of possible identifiers is used for this.
    /// </summary>
    /// <returns></returns>
    public virtual T FindPersistentClass<T> () where T: BaseData
    {
      return this.FindPersistentClass<T> (this.Identifiers);
    }

    /// <summary>
    /// Find the persistent class in the database
    /// that corresponds to this object.
    /// 
    /// A default list of possible identifiers is used for this.
    /// </summary>
    /// <returns></returns>
    public virtual BaseData FindPersistentClass ()
    {
      return this.FindPersistentClass (this.Identifiers);
    }

    /// <summary>
    /// Find the persistent class in the database
    /// that corresponds to this object.
    /// 
    /// A default list of possible identifiers is used for this.
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public virtual T FindPersistentClass<T> (ISession session)
      where T: BaseData
    {
      return this.FindPersistentClass<T> (session, this.Identifiers);
    }

    /// <summary>
    /// Find the persistent class in the database
    /// that corresponds to this object.
    /// 
    /// A default list of possible identifiers is used for this.
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public virtual BaseData FindPersistentClass (ISession session)
    {
      return this.FindPersistentClass (session, this.Identifiers);
    }

    /// <summary>
    /// Refine recursively the query given an initial identifier
    /// and an initial data item
    /// </summary>
    /// <param name="restrictions"></param>
    /// <param name="absoluteIdentifier">To use in query</param>
    /// <param name="relativeIdentifier">Relative identifier to currentObject</param>
    /// <param name="currentObject"></param>
    /// <returns>true if absoluteIdentifier is a valid ID</returns>
    private bool RefineQuery (ref IDictionary<string, object> restrictions,
                              string absoluteIdentifier,
                              string relativeIdentifier,
                              object currentObject)
    {
      int dotPosition = relativeIdentifier.IndexOf ('.');
      if (-1 == dotPosition) {
        PropertyInfo identifierInfo =
          currentObject.GetType ().GetProperty (relativeIdentifier);
        if (null == identifierInfo) {
          log.WarnFormat ("RefineQuery: " +
                          "{0} does not have an identifier {1}",
                          currentObject.GetType (),
                          relativeIdentifier);
          return false;
        }
        Type identifierType =
          identifierInfo.PropertyType;
        object identifierValue =
          identifierInfo.GetValue (currentObject, null);
        if (identifierValue is BaseData) {
          BaseData identifierBaseDataValue = (BaseData) identifierValue;
          foreach (string subIdentifier in identifierBaseDataValue.Identifiers) {
            if (RefineQuery (ref restrictions,
                             absoluteIdentifier + "." + subIdentifier,
                             subIdentifier,
                             identifierBaseDataValue)) {
              // This was a valid identifier, there is no need
              // to continue to refine the query
              break;
            }
          }
        }
        else if ( ((identifierValue is int) && (0 != (int) identifierValue))
                 || ((identifierValue is string) && (0 < ((string) identifierValue).Length))
                 || ((identifierValue is bool) && ((bool) identifierValue))) {
          log.DebugFormat ("GetValue: " +
                           "use identifier {0}={1}",
                           absoluteIdentifier, identifierValue);
          restrictions.Add (absoluteIdentifier, identifierValue);
          if (IsIdentifierAnId (absoluteIdentifier)) {
            // This is a an Id:
            // There is no need to check other identifiers
            return true;
          }
        }
      }
      else {
        string id1 = relativeIdentifier.Substring (0, dotPosition);
        string id2 = relativeIdentifier.Substring (dotPosition + 1);
        object id1Value =
          currentObject.GetType ().GetProperty (id1).GetValue (currentObject, null);
        if (null != id1Value) {
          RefineQuery (ref restrictions,
                       absoluteIdentifier,
                       id2,
                       id1Value);
        }
      }
      
      return false;
    }
    
    bool IsIdentifierAnId (string identifier)
    {
      return identifier.EndsWith ("Id");
    }
    
    #region Collections utilities
    /// <summary>
    /// Modifier delegate to modify an item in a collection
    /// </summary>
    protected delegate void Modifier<T> (T item);
    
    /// <summary>
    /// Modify a set of items in a collection
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="modifier"></param>
    protected static void ModifyItems<T> (IEnumerable<T> collection,
                                          Modifier<T> modifier)
    {
      IList<T> items = new List<T> ();
      foreach (T item in collection) {
        items.Add (item);
      }
      foreach (T item in items) {
        modifier (item);
      }
    }
    
    /// <summary>
    /// Remove all the values from a dictionary
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="v"></param>
    protected static void RemoveDictionaryValue<TKey, TValue> (IDictionary<TKey, TValue> dictionary,
                                                               TValue v)
    {
      IList<TKey> keys = new List<TKey> ();
      foreach (KeyValuePair<TKey, TValue> item in dictionary) {
        if (item.Value.Equals (v)) {
          keys.Add (item.Key);
        }
      }
      foreach (TKey item in keys) {
        dictionary.Remove (item);
      }
    }
    
    /// <summary>
    /// Get the first element of a collection,
    /// or raise an exception if the collection is empty
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException">the collection is empty</exception>
    protected static T GetFirst<T> (IEnumerable<T> collection)
    {
      using (IEnumerator<T> iterator = collection.GetEnumerator ())
      {
        if (iterator.MoveNext ()) {
          return iterator.Current;
        }
        else {
          throw new IndexOutOfRangeException ();
        }
      }
    }
    
    /// <summary>
    /// Add an item to a proxy collection:
    /// <item>Try to initialize it first</item>
    /// <item>Do nothing if it could not be initialized</item>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="item"></param>
    protected static void AddToProxyCollection<T> (ICollection<T> collection, T item)
    {
      try {
        NHibernateUtil.Initialize (collection);
      }
      catch (Exception) {
      }
      
      if ( (null != collection)
          && NHibernateUtil.IsInitialized (collection)) {
        collection.Add (item);
      }
    }
    
    /// <summary>
    /// Remove an item from a proxy collection
    /// <item>Try to initialize it first</item>
    /// <item>Do nothing if it could not be initialized</item>
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="item"></param>
    protected static void RemoveFromProxyCollection<T> (ICollection<T> collection, T item)
    {
      try {
        NHibernateUtil.Initialize (collection);
      }
      catch (Exception) {
      }
      
      if ( (null != collection)
          && NHibernateUtil.IsInitialized (collection)) {
        collection.Remove (item);
      }
    }

    /// <summary>
    /// Add an item to a proxy dictionary:
    /// <item>Try to initialize it first</item>
    /// <item>Do nothing if it could not be initialized</item>
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="v"></param>
    protected static void AddToProxyDictionary<TKey, TValue> (IDictionary<TKey, TValue> dictionary,
                                                              TKey key, TValue v)
    {
      try {
        NHibernateUtil.Initialize (dictionary);
      }
      catch (Exception) {
      }
      
      if ( (null != dictionary)
          && NHibernateUtil.IsInitialized (dictionary)) {
        dictionary [key] = v;
      }
    }
    
    /// <summary>
    /// Remove an item from a proxy dictionary
    /// <item>Try to initialize it first</item>
    /// <item>Do nothing if it could not be initialized</item>
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="v"></param>
    protected static void RemoveFromProxyDictionaryValue<TKey, TValue> (IDictionary<TKey, TValue> dictionary,
                                                                        TValue v)
    {
      try {
        NHibernateUtil.Initialize (dictionary);
      }
      catch (Exception) {
      }
      
      if ( (null != dictionary)
          && NHibernateUtil.IsInitialized (dictionary)) {
        RemoveDictionaryValue<TKey, TValue> (dictionary, v);
      }
    }
    #endregion // Collection utilities
    
    #endregion // Methods
  }
}
