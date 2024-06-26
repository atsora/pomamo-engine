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

      Assert.That (customSet.Contains (inst1), Is.EqualTo (true));

      IdClass inst2 = new IdClass ();
      inst2.Id = 2;
      customSet.Add (inst2);
      Assert.That (customSet.Contains (inst2), Is.EqualTo (true));

      inst1.Id = 1;
      Assert.Multiple (() => {
        Assert.That (customSet.Contains (inst1), Is.EqualTo (true));
        Assert.That (customSet.Contains (inst2), Is.EqualTo (true));
      });

      IdClass inst3 = new IdClass ();
      inst3.Id = 3;
      Assert.That (customSet.Contains (inst3), Is.EqualTo (false));
      customSet.Add (inst3);
      Assert.Multiple (() => {
        Assert.That (customSet.Contains (inst3), Is.EqualTo (true));
        Assert.That (customSet.Contains (inst2), Is.EqualTo (true));
      });
      customSet.Remove (inst2);
      Assert.Multiple (() => {
        Assert.That (customSet.Contains (inst3), Is.EqualTo (true));
        Assert.That (customSet.Contains (inst2), Is.EqualTo (false));
      });

      IdClass inst4 = new IdClass ();
      customSet.Add (inst4);
      Assert.That (customSet.Contains (inst4), Is.EqualTo (true));
      inst4.Id = 4;
      Assert.Multiple (() => {
        Assert.That (customSet.Contains (inst4), Is.EqualTo (true));
        Assert.That (customSet.Contains (inst3), Is.EqualTo (true));
        Assert.That (customSet.Contains (inst2), Is.EqualTo (false));
        Assert.That (customSet.Contains (inst1), Is.EqualTo (true));
      });

      var classicSet = new HashSet<IdClass> ();

      IdClass inst5 = new IdClass ();
      // id5.Id == 0
      classicSet.Add (inst5);
      // !!
      customSet = classicSet;
      Assert.That (customSet.Contains (inst5), Is.EqualTo (true));
      inst5.Id = 5;
      // arg !
      Assert.That (customSet.Contains (inst5), Is.EqualTo (false));

      // while
      ISet<IdClass> otherCustomSet =
        new InitialNullIdSet<IdClass, int> ();
      IdClass inst6 = new IdClass ();
      otherCustomSet.Add (inst6);
      customSet = otherCustomSet;
      Assert.That (customSet.Contains (inst6), Is.EqualTo (true));
      inst6.Id = 6;
      // OK here
      Assert.That (customSet.Contains (inst6), Is.EqualTo (true));

      // Same type of problem
      customSet = new InitialNullIdSet<IdClass, int> ();
      IdClass inst7 = new IdClass ();
      customSet.Add (inst7);
      Assert.That (customSet.Contains (inst7), Is.EqualTo (true));
      classicSet = new HashSet<IdClass> ();
      classicSet.Add (inst7);
      Assert.That (classicSet.Contains (inst7), Is.EqualTo (true));
      // !!
      customSet = classicSet;
      Assert.That (customSet.Contains (inst7), Is.EqualTo (true));
      inst7.Id = 7;
      // arg
      Assert.That (customSet.Contains (inst7), Is.EqualTo (false));
    }

  }
}
