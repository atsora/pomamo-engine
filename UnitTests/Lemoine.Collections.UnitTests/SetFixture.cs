// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace Lemoine.Collections.UnitTests
{

  /// <summary>
  /// Helper class
  /// </summary>
  public class IdClass : Lemoine.Collections.IDataWithId, IComparable
  {
    string m_name;
    int m_id = 0; // 0 and nothing else
    int m_order;
    // Id should only change from 0 to another value (and then never again)
    public int Id { get { return m_id; } set { m_id = value; } }
    public int Order { get { return m_order; } set { m_order = value; } }
    public string Name { get { return m_name; } set { m_name = value; } }
    public IdClass () { }


    public override string ToString ()
    {
      return String.Format ("[IdClass Id={0} Order={1} Name={2}]",
                           Id, Order, Name);
    }

    public int CompareTo (Object obj)
    {
      if (object.ReferenceEquals (this, obj)) return 0;
      if ((obj == null) || (this.GetType () != obj.GetType ()))
        throw new ArgumentException ("Comparison of a sequence with a null or other type");
      IdClass other = obj as IdClass;
      return this.m_order.CompareTo (other.m_order); // NOT id

    }
    public IdClass (int id, string name)
    {
      m_id = id;
      m_order = id; // but this won't change when id changes
      m_name = name;
    }

    public virtual bool Equals (IdClass other)
    {
      if (object.ReferenceEquals (this, other)) return true;
      if (other == null) return false;
      return (other.Id == this.Id);
    }

    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) return true;
      if (obj == null) return false;
      if (this.GetType () != obj.GetType ()) return false;
      IdClass other = obj as IdClass;
      return (other.Id == this.Id);
    }

    public override int GetHashCode ()
    {
      return m_id.GetHashCode ();
    }
  }

  /// <summary>
  /// Summary description for SetFixture.
  /// </summary>
  public abstract class SetFixture
  {
    private System.Collections.Generic.IList<IdClass> _aInitValues;
    private System.Collections.Generic.IList<IdClass> _bInitValues;
    protected ISet<IdClass> _set;

    public static IdClass one = new IdClass (1, "one");
    public static IdClass two = new IdClass (2, "two");
    public static IdClass three = new IdClass (3, "three");

    [SetUp]
    public virtual void SetUp ()
    {
      _aInitValues = new System.Collections.Generic.List<IdClass> ();
      _aInitValues.Add (new IdClass (0, "zero"));
      _aInitValues.Add (new IdClass (1, "one"));
      _aInitValues.Add (new IdClass (2, "two"));
      _aInitValues.Add (new IdClass (3, "three"));

      _bInitValues = new System.Collections.Generic.List<IdClass> ();
      _bInitValues.Add (new IdClass (2, "two"));
      _bInitValues.Add (new IdClass (3, "three"));
      _bInitValues.Add (new IdClass (4, "four"));

      _set = CreateInstance (new IdClass[] { one, two, three });
    }

    #region System.Collections.ICollection Member Tests

    [Test]
    public void CopyTo ()
    {
      IdClass[] dest = new IdClass[3];
      _set.CopyTo (dest, 0);

      int count = 0;

      foreach (IdClass obj in dest) {
        Assert.IsTrue (_set.Contains (obj), "set should contain the object in the array");
        count++;
      }

      Assert.AreEqual (3, count, "should have 3 items in array");
    }

    [Test]
    public void Count ()
    {
      Assert.AreEqual (3, _set.Count (), "should be 3 items");
      Assert.AreEqual (0, CreateInstance ().Count (), "new set should have nothing in it.");
    }

    #endregion

    #region Iesi.Collections.ISet<string> Constructor Tests

    [Test]
    public void CtorWithDefaults ()
    {
      var init = new System.Collections.Generic.List<IdClass> (3);
      init.Add (new IdClass (1, "one"));
      init.Add (new IdClass (2, "two"));
      init.Add (new IdClass (3, "three"));

      ISet<IdClass> theSet = CreateInstance (init);

      Assert.AreEqual (3, init.Count, "3 items in set");

      int index = 0;
      foreach (IdClass obj in init) {
        Assert.IsTrue (theSet.Contains (obj), "set should contain obj at index = " + index.ToString ());
        index++;
      }
    }

    #endregion

    #region Iesi.Collections.ISet<string> Member Tests

    [Test]
    public void Add ()
    {
      try {
        Assert.IsTrue (_set.Add (new IdClass (4, "four")), "should have added 'four'");
        Assert.AreEqual (4, _set.Count (), "should have added 'four'");

        Assert.IsFalse (_set.Add (two), "'two' was already there");
        Assert.AreEqual (4, _set.Count (), "object already in set");
        if (_set.IsReadOnly)
          Assert.Fail ("Read-only set can be modified");
      }
      catch (NotSupportedException) {
        if (!_set.IsReadOnly)
          throw;
      }
    }

    [Test]
    public void Clear ()
    {
      try {
        ((System.Collections.Generic.ICollection<IdClass>)_set).Clear ();
        Assert.AreEqual (0, _set.Count (), "should have no items in ISet.");

        if (_set.IsReadOnly)
          Assert.Fail ("Read-only set can be modified");
      }
      catch (NotSupportedException) {
        if (!_set.IsReadOnly)
          throw;
      }
    }

    [Test]
    public void Contains ()
    {
      Assert.IsTrue (_set.Contains (one), "does contain one");
      Assert.IsFalse (_set.Contains (new IdClass (4, "four")), "does not contain 'four'");
    }

    [Test]
    public void Remove ()
    {
      try {
        Assert.IsTrue (_set.Remove (one), "should have removed 'one'");
        Assert.IsFalse (_set.Contains (one), "one should have been removed");
        Assert.AreEqual (2, _set.Count (), "should be 2 items after one removed.");

        Assert.IsFalse (_set.Remove (one), "was already removed.");
        if (_set.IsReadOnly)
          Assert.Fail ("Read-only set can be modified");
      }
      catch (NotSupportedException) {
        if (!_set.IsReadOnly)
          throw;
      }
    }

    #endregion

    protected abstract ISet<IdClass> CreateInstance ();

    protected abstract ISet<IdClass> CreateInstance (System.Collections.Generic.ICollection<IdClass> init);

    protected abstract Type ExpectedType { get; }
  }
}
