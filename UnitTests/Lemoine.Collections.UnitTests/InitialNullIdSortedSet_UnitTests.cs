// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using System.Collections.Generic;
using NUnit.Framework;

namespace Lemoine.Collections.UnitTests.InitialNullIdSortedSet
{
  /// <summary>
  /// Unit tests for the class InitialNullIdSortedSet
  /// Includes test for sorted set structure
  /// as well as a test for Ids changing from 0 to a non-0 value
  /// </summary>  
  [TestFixture]
  public class UnitTests : SortedSetFixture
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UnitTests).FullName);

    /// <summary>
    /// method overriding for SortedSetFixture
    /// </summary>
    /// <returns></returns>
    protected override ISet<IdClass> CreateInstance ()
    {
      return new InitialNullIdSortedSet<IdClass, int> ();
    }

    protected override ISet<IdClass> CreateInstance (System.Collections.Generic.ICollection<IdClass> init)
    {
      var setToReturn = new InitialNullIdSortedSet<IdClass, int> ();
      foreach (IdClass element in init)
        setToReturn.Add (element);
      return setToReturn;
    }

    protected override Type ExpectedType
    {
      get { return typeof (InitialNullIdSortedSet<IdClass, int>); }
    }

    /// <summary>
    /// Test "rehash" of null Id inserted elements
    /// </summary>
    [Test]
    public void TestRehashNullFunctionality ()
    {
      ISet<IdClass> customSet =
        new InitialNullIdSortedSet<IdClass, int> ();

      IdClass inst1 = new IdClass ();
      inst1.Order = 1;
      customSet.Add (inst1);

      Assert.AreEqual (true, customSet.Contains (inst1));

      IdClass inst2 = new IdClass ();
      inst2.Id = 2;
      inst2.Order = 2;
      customSet.Add (inst2);
      Assert.AreEqual (true, customSet.Contains (inst2));

      inst1.Id = 1;
      Assert.AreEqual (true, customSet.Contains (inst1));
      Assert.AreEqual (true, customSet.Contains (inst2));

      IdClass inst3 = new IdClass ();
      inst3.Id = 3;
      inst3.Order = 3;
      Assert.AreEqual (false, customSet.Contains (inst3));
      customSet.Add (inst3);
      Assert.AreEqual (true, customSet.Contains (inst3));
      Assert.AreEqual (true, customSet.Contains (inst2));
      customSet.Remove (inst2);
      Assert.AreEqual (true, customSet.Contains (inst3));
      Assert.AreEqual (false, customSet.Contains (inst2));

      IdClass inst4 = new IdClass ();
      inst4.Order = 4;
      customSet.Add (inst4);
      Assert.AreEqual (true, customSet.Contains (inst4));
      inst4.Id = 4;
      Assert.AreEqual (true, customSet.Contains (inst4));
      Assert.AreEqual (true, customSet.Contains (inst3));
      Assert.AreEqual (false, customSet.Contains (inst2));
      Assert.AreEqual (true, customSet.Contains (inst1));

      ISet<IdClass> classicSet =
        new HashSet<IdClass> ();

      IdClass inst5 = new IdClass ();
      inst5.Order = 5;
      // id5.Id == 0
      classicSet.Add (inst5);
      // !!
      customSet = classicSet;
      Assert.AreEqual (true, customSet.Contains (inst5));
      inst5.Id = 5;
      // arg !
      Assert.AreEqual (false, customSet.Contains (inst5));

      // while
      ISet<IdClass> otherCustomSet =
        new InitialNullIdSortedSet<IdClass, int> ();
      IdClass inst6 = new IdClass ();
      inst6.Order = 6;
      otherCustomSet.Add (inst6);
      customSet = otherCustomSet;
      Assert.AreEqual (true, customSet.Contains (inst6));
      inst6.Id = 6;
      // OK here
      Assert.AreEqual (true, customSet.Contains (inst6));

      // Same type of problem
      customSet = new InitialNullIdSortedSet<IdClass, int> ();
      IdClass inst7 = new IdClass ();
      inst7.Order = 7;
      customSet.Add (inst7);
      Assert.AreEqual (true, customSet.Contains (inst7));
      classicSet = new HashSet<IdClass> ();
      classicSet.Add (inst7);
      Assert.AreEqual (true, classicSet.Contains (inst7));
      // !!
      customSet = classicSet;
      Assert.AreEqual (true, customSet.Contains (inst7));
      inst7.Id = 7;
      // arg
      Assert.AreEqual (false, customSet.Contains (inst7));
    }

  }
}
