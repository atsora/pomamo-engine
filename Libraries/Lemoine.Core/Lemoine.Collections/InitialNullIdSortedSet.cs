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
  /// Sorted Set based collection for objects having an Id
  /// (Sorted means iteration appears in order)
  /// Objects first inserted will a null (0) Id
  /// can have their Id modified externally
  /// while still remaining present in the collection
  /// </summary>
  public class InitialNullIdSortedSet<T, ID>
    : SortedSet<T>
    , ISet<T>
    where T : IComparable /* , Lemoine.Collections.IDataWithId<ID> */
  {
    #region Members
    System.Collections.Generic.IDictionary<T, object> ZeroIdList = new System.Collections.Generic.SortedList<T, object> ();
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (InitialNullIdSortedSet<T, ID>).FullName);

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
    public InitialNullIdSortedSet ()
    {
    }
    #endregion // Constructors

    #region Methods

    private void RehashZeroID ()
    {
      var blackList = new System.Collections.Generic.List<T> ();

      foreach (T dataWithId in ZeroIdList.Keys) {
        if (!object.Equals (default (ID), ((Lemoine.Collections.IDataWithId<ID>)dataWithId).Id)) {
          blackList.Add (dataWithId);
        }
      }

      foreach (T dataWithId in blackList) {
        ZeroIdList.Remove (dataWithId);
        base.Add (dataWithId);
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
        if (!ZeroIdList.ContainsKey (dataWithId)) {
          this.ZeroIdList.Add (dataWithId, null);
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
    public override bool Contains (T dataWithId)
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
        return this.ZeroIdList.ContainsKey (dataWithId);
      }
    }

    /// <summary>
    /// reset to empty set
    /// </summary>
    public override void Clear ()
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
      System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<T, object>> listEnumerator;
      System.Collections.Generic.IEnumerator<T> setEnumerator;

      enum CurrentPosition
      {
        InList,
        InSet,
        InNeither
      }

      CurrentPosition currentPosition;

      enum EnumerationStatus
      {
        Before,
        In,
        After
      }

      EnumerationStatus enumStatusList;
      EnumerationStatus enumStatusSet;

      void IDisposable.Dispose () { }

      public ElementEnum (InitialNullIdSortedSet<T, ID> custom, System.Collections.Generic.IEnumerator<T> hashEnumerator)
      {
        listEnumerator = custom.ZeroIdList.GetEnumerator ();
        setEnumerator = hashEnumerator;

        currentPosition = CurrentPosition.InNeither;
        enumStatusList = EnumerationStatus.Before;
        enumStatusSet = EnumerationStatus.Before;
      }

      public bool MoveNext ()
      {
        bool isElementInList =
          (enumStatusList == EnumerationStatus.After ? false :
           (enumStatusList == EnumerationStatus.Before ? listEnumerator.MoveNext () :
            (currentPosition == CurrentPosition.InList ? listEnumerator.MoveNext () : true)));

        bool isElementInSet =
          (enumStatusSet == EnumerationStatus.After ? false :
           (enumStatusSet == EnumerationStatus.Before ? setEnumerator.MoveNext () :
            (currentPosition == CurrentPosition.InSet ? setEnumerator.MoveNext () : true)));

        if (isElementInList && isElementInSet) {
          enumStatusList = EnumerationStatus.In;
          enumStatusSet = EnumerationStatus.In;

          if (listEnumerator.Current.Key.CompareTo (setEnumerator.Current) <= 0) {
            currentPosition = CurrentPosition.InList;
          }
          else {
            currentPosition = CurrentPosition.InSet;
          }
          return true;
        }
        else if (isElementInList) {
          // but not isElementInSet
          enumStatusList = EnumerationStatus.In;
          enumStatusSet = EnumerationStatus.After;
          currentPosition = CurrentPosition.InList;
          return true;
        }
        else if (isElementInSet) {
          // but not isElementInList
          enumStatusList = EnumerationStatus.After;
          enumStatusSet = EnumerationStatus.In;
          currentPosition = CurrentPosition.InSet;
          return true;
        }

        // end of both
        enumStatusList = EnumerationStatus.After;
        enumStatusSet = EnumerationStatus.After;
        currentPosition = CurrentPosition.InNeither;
        return false;
      }

      public void Reset ()
      {
        listEnumerator.Reset ();
        setEnumerator.Reset ();
        currentPosition = CurrentPosition.InNeither;
        enumStatusList = EnumerationStatus.Before;
        enumStatusSet = EnumerationStatus.Before;
      }

      public T Current
      {
        get
        {
          if (currentPosition == CurrentPosition.InList) {
            return listEnumerator.Current.Key;
          }

          if (currentPosition == CurrentPosition.InSet) {
            return setEnumerator.Current;
          }

          throw new InvalidOperationException ("Invalid access to Current element in enumerator");
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
