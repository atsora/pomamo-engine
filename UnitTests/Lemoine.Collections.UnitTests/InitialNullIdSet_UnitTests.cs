// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Collections.UnitTests.InitialNullIdSet
{
  [TestFixture]
  public class UnitTests : SetFixture
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UnitTests).FullName);

    /// <summary>
    /// method overriding for SetFixture
    /// </summary>
    /// <returns></returns>
    protected override ISet<IdClass> CreateInstance ()
    {
      return new InitialNullIdSet<IdClass, int> ();
    }

    protected override ISet<IdClass> CreateInstance (System.Collections.Generic.ICollection<IdClass> init)
    {
      ISet<IdClass> setToReturn = new InitialNullIdSet<IdClass, int> ();
      foreach (IdClass element in init)
        setToReturn.Add (element);
      return setToReturn;
    }

    protected override Type ExpectedType
    {
      get { return typeof (InitialNullIdSet<IdClass, int>); }
    }

    /// <summary>
    /// Test "rehash" of null Id inserted elements
    /// </summary>
    [Test]
    public void TestRehashNullFunctionality ()
    {
      ISet<IdClass> customSet =
        new InitialNullIdSet<IdClass, int> ();

      IdClass inst1 = new IdClass ();
      customSet.Add (inst1);

      Assert.AreEqual (true, customSet.Contains (inst1));

      IdClass inst2 = new IdClass ();
      inst2.Id = 2;
      customSet.Add (inst2);
      Assert.AreEqual (true, customSet.Contains (inst2));

      inst1.Id = 1;
      Assert.AreEqual (true, customSet.Contains (inst1));
      Assert.AreEqual (true, customSet.Contains (inst2));

      IdClass inst3 = new IdClass ();
      inst3.Id = 3;
      Assert.AreEqual (false, customSet.Contains (inst3));
      customSet.Add (inst3);
      Assert.AreEqual (true, customSet.Contains (inst3));
      Assert.AreEqual (true, customSet.Contains (inst2));
      customSet.Remove (inst2);
      Assert.AreEqual (true, customSet.Contains (inst3));
      Assert.AreEqual (false, customSet.Contains (inst2));

      IdClass inst4 = new IdClass ();
      customSet.Add (inst4);
      Assert.AreEqual (true, customSet.Contains (inst4));
      inst4.Id = 4;
      Assert.AreEqual (true, customSet.Contains (inst4));
      Assert.AreEqual (true, customSet.Contains (inst3));
      Assert.AreEqual (false, customSet.Contains (inst2));
      Assert.AreEqual (true, customSet.Contains (inst1));

      var classicSet = new HashSet<IdClass> ();

      IdClass inst5 = new IdClass ();
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
        new InitialNullIdSet<IdClass, int> ();
      IdClass inst6 = new IdClass ();
      otherCustomSet.Add (inst6);
      customSet = otherCustomSet;
      Assert.AreEqual (true, customSet.Contains (inst6));
      inst6.Id = 6;
      // OK here
      Assert.AreEqual (true, customSet.Contains (inst6));

      // Same type of problem
      customSet = new InitialNullIdSet<IdClass, int> ();
      IdClass inst7 = new IdClass ();
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
