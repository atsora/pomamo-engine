// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections;

using Lemoine.Core.Log;
using System.Collections.Generic;

namespace Lemoine.Collections
{
  /// <summary>
  /// Hashed Set based collection for objects having an Id
  /// Objects first inserted will a null (0) Id
  /// can have their Id modified externally
  /// while still remaining present in the collection
  /// </summary>
  public class InitialNullIdSet<T, ID>
    : HashSet<T>
    , ISet<T>
  /* where T : Lemoine.Collections.IDataWithId<ID> */
  /* this constraint would be nice to have, but interferes with the fact
   * that some of our to be persisted classes do not extend IDataWithID
   * despite having an Id property */
  {
    #region Members
    System.Collections.Generic.IList<T> ZeroIdList = new System.Collections.Generic.List<T> ();
    // ISet<T> NonZeroIDSet = new HashSet<T>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (InitialNullIdSet<T, ID>).FullName);

    #region Getters / Setters
    /// <summary>
    /// return number of elements in set
    /// </summary>
    public new int Count { get { return ZeroIdList.Count + base.Count; } }

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Simple constructor
    /// </summary>
    public InitialNullIdSet ()
    {
    }
    #endregion // Constructors

    #region Methods

    private void RehashZeroID ()
    {
      for (int i = this.ZeroIdList.Count - 1; i >= 0; i--) {
        T dataWithId = this.ZeroIdList[i];
        if (!object.Equals (default (ID), ((Lemoine.Collections.IDataWithId<ID>)dataWithId).Id)) {
          this.ZeroIdList.RemoveAt (i);
          base.Add (dataWithId);
        }
      }
    }

    /// <summary>
    /// remove element from set
    /// </summary>
    /// <param name="dataWithId"></param>
    /// <returns></returns>
    public new bool Remove (T dataWithId)
    {
      Debug.Assert (dataWithId is Lemoine.Collections.IDataWithId<ID>);

      if (!(dataWithId is Lemoine.Collections.IDataWithId<ID>)) {
        log.Fatal ("adding object which is not a Lemoine.Collections.IDataWithId");
        return false;
      }

      if (object.Equals (default (ID), ((Lemoine.Collections.IDataWithId<ID>)dataWithId).Id)) {
        return ZeroIdList.Remove (dataWithId);
      }
      else {
        RehashZeroID ();
        return base.Remove (dataWithId);
      }
    }

    /// <summary>
    /// add element in set
    /// </summary>
    /// <param name="dataWithId"></param>
    /// <returns></returns>
    public new bool Add (T dataWithId)
    {
      Debug.Assert (dataWithId is Lemoine.Collections.IDataWithId<ID>);

      if (!(dataWithId is Lemoine.Collections.IDataWithId<ID>)) {
        log.Fatal ("adding object which is not a Lemoine.Collections.IDataWithId");
        return false;
      }

      if (object.Equals (default (ID), ((Lemoine.Collections.IDataWithId<ID>)dataWithId).Id)) {
        if (!ZeroIdList.Contains (dataWithId)) {
          this.ZeroIdList.Add (dataWithId);
          return true;
        }
        else {
          return false;
        }
      }
      else {
        RehashZeroID ();
        return base.Add (dataWithId);
      }
    }

    /// <summary>
    /// check set membership
    /// </summary>
    /// <param name="dataWithId"></param>
    /// <returns></returns>
    public new bool Contains (T dataWithId)
    {
      Debug.Assert (dataWithId is Lemoine.Collections.IDataWithId<ID>);

      if (!(dataWithId is Lemoine.Collections.IDataWithId<ID>)) {
        log.Fatal ("adding object which is not a Lemoine.Collections.IDataWithId");
        return false;
      }

      if (!object.Equals (default (ID), ((Lemoine.Collections.IDataWithId<ID>)dataWithId).Id)) {
        RehashZeroID ();
        return base.Contains (dataWithId);
      }
      else {
        return this.ZeroIdList.Contains (dataWithId);
      }
    }

    /// <summary>
    /// reset to empty set
    /// </summary>
    public new void Clear ()
    {
      this.ZeroIdList.Clear ();
      base.Clear ();
    }

    /// <summary>
    /// return IEnumerator on set
    /// </summary>
    /// <returns></returns>
    public new System.Collections.Generic.IEnumerator<T> GetEnumerator ()
    {
      return (System.Collections.Generic.IEnumerator<T>)
        (new ElementEnum (this, base.GetEnumerator ()));
    }

    private class ElementEnum : System.Collections.Generic.IEnumerator<T>
    {
      System.Collections.Generic.IEnumerator<T> listEnumerator;
      System.Collections.Generic.IEnumerator<T> setEnumerator;
      bool stillOnList;

      void IDisposable.Dispose () { }

      public ElementEnum (InitialNullIdSet<T, ID> custom, System.Collections.Generic.IEnumerator<T> hashEnumerator)
      {
        listEnumerator = custom.ZeroIdList.GetEnumerator ();
        setEnumerator = hashEnumerator;
        stillOnList = true;
      }

      public bool MoveNext ()
      {
        if (stillOnList) {
          if (listEnumerator.MoveNext ()) {
            return true;
          }
          else {
            stillOnList = false;
          }
        }
        return setEnumerator.MoveNext ();
      }

      public void Reset ()
      {
        listEnumerator.Reset ();
        setEnumerator.Reset ();
        stillOnList = true;
      }

      public T Current
      {
        get
        {
          if (stillOnList) {
            return listEnumerator.Current;
          }
          else {
            return setEnumerator.Current;
          }
        }
      }

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }
    }
  }
  #endregion // Methods
}
