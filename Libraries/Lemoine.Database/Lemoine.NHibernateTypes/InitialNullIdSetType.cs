// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NHibernate;
using System.Collections.Generic;
using Lemoine.Collections;
using NHibernate.Collection.Generic;
using Lemoine.ModelDAO;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// NHibernate Type for InitialNullIdSet
  /// see NHibernate documentation for NHibernate.Type.GenericSetType.cs
  /// </summary>
  public class InitialNullIdSetType<T, ID> :
    NHibernate.UserTypes.IUserCollectionType
  {
    #region Members
    #endregion // Members

    readonly ILog log = LogManager.GetLogger(typeof (InitialNullIdSetType<T, ID>).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors

    #endregion // Constructors

    #region Methods

    /// <summary>
    /// instantiate an InitialNullIdSet with an anticipated size
    /// </summary>
    /// <param name="anticipatedSize"></param>
    /// <returns></returns>
    public object Instantiate(int anticipatedSize)
    {
      return new InitialNullIdSet<T, ID>();
    }

    
    /// <summary>
    /// instantiate a new IPersistentCollection for the set
    /// </summary>
    /// <param name="session"></param>
    /// <param name="persister"></param>
    /// <returns></returns>
    public NHibernate.Collection.IPersistentCollection
      Instantiate(NHibernate.Engine.ISessionImplementor session,
                  NHibernate.Persister.Collection.ICollectionPersister persister)
    {
      return (new PersistentGenericSet<T>(session));
    }
    
    /// <summary>
    /// wraps a Iset of T in a PersistentGenericSet
    /// </summary>
    /// <param name="session"></param>
    /// <param name="collection"></param>
    /// <returns></returns>
    public NHibernate.Collection.IPersistentCollection
      Wrap(NHibernate.Engine.ISessionImplementor session,
           object collection)
    {
      return (new PersistentGenericSet<T>(session, (ISet<T>) collection));
    }
    
    /// <summary>
    /// set membership 
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public bool Contains(object collection, object entity)
    {
      return ((ISet<T>)collection).Contains((T) entity);
    }
    
    /// <summary>
    /// optional not implemented operation
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public object IndexOf(object collection, object entity)
    {
      return -1;
    }
    
    /// <summary>
    /// return an IEnumerable over the elements of the collection 
    /// (which may or may not be a wrapper)
    /// </summary>
    /// <param name="collection"></param>
    /// <returns></returns>
    public System.Collections.IEnumerable GetElements(object collection)
    {
      return (System.Collections.IEnumerable) collection;
    }
    
    /// <summary>
    /// replace the elements of a collection with the elements of another collection
    /// </summary>
    /// <param name="original"></param>
    /// <param name="target"></param>
    /// <param name="persister"></param>
    /// <param name="owner"></param>
    /// <param name="copyCache"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    public object ReplaceElements(object original, object target,
                                  NHibernate.Persister.Collection.ICollectionPersister persister,
                                  object owner, System.Collections.IDictionary copyCache,
                                  NHibernate.Engine.ISessionImplementor session)
    {
      ISet<T> result = (ISet<T>) target;
      result.Clear();
      foreach(T item in ((System.Collections.IEnumerable) original))
      {
        result.Add(item);
      }
      return result;
    }
    #endregion // Methods
  }
}
