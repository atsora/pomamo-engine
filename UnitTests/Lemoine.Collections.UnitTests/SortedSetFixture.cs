// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Lemoine.Collections.UnitTests
{
  /// <summary>
  /// Summary description for SortedSetFixture.
  /// </summary>
  [TestFixture]
  public abstract class SortedSetFixture : SetFixture
  {
    [Test]
    public void OrderedEnumeration()
    {
      System.Collections.ArrayList expectedOrder = new System.Collections.ArrayList(3);
      expectedOrder.Add(one);
      expectedOrder.Add(three);
      expectedOrder.Add(two);
      expectedOrder.Sort();

      int index = 0;
      foreach (object obj in _set)
      {
        Assert.That (expectedOrder[index], Is.EqualTo (obj), index.ToString() + " did not have same value");
        index++;
      }
    }

    [Test]
    public void OrderedEnumerationWithZeroId()
    {
      IdClass zero = new IdClass(0, "zero");
      System.Collections.ArrayList expectedOrder = new System.Collections.ArrayList (3);
      expectedOrder.Add(one);
      expectedOrder.Add(three);
      expectedOrder.Add(two);
      expectedOrder.Add(zero);
      expectedOrder.Sort();

      ISet<IdClass> set_plus_0 = new InitialNullIdSortedSet<IdClass, int>();
      set_plus_0.Add(zero);
    }
  }
}