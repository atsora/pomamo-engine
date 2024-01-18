// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Lemoine.Core.Log;
using Lemoine.Core.Cache;

namespace Lemoine.Core.Cache.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class LruCache_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (LruCache_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LruCache_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestRemoveByRegex ()
    {
      var cache = new LruCacheClient (10);
      cache.Set ("a1", 1);
      cache.Set ("a2", 2);
      using (var batchUpdater = new CacheClientBatchUpdater (cache)) {
        cache.RemoveByRegex ("c.*");
        using (var batchUpdater2 = new CacheClientBatchUpdater (cache)) {
          cache.RemoveByRegex ("a.*");
          cache.RemoveByRegex ("b.*");
          Assert.That (cache.Get<int?> ("a1"), Is.EqualTo (1));
        }
        Assert.That (cache.Get<int?> ("a1"), Is.EqualTo (1));
      }
      Assert.That (cache.Get<int?> ("a1"), Is.EqualTo (null));

      cache.Set ("a1", 1);
      cache.Set ("a2", 2);
      using (var batchUpdater = new CacheClientBatchUpdater (cache)) {
        cache.RemoveByRegex ("c.*");
        cache.RemoveByRegex ("b.*");
      }
      Assert.That (cache.Get<int?> ("a1"), Is.EqualTo (1));

      cache.RemoveByRegex ("a.*");
      Assert.That (cache.Get<int?> ("a1"), Is.EqualTo (null));
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestRemoveByRegexWithCleanExtension ()
    {
      var cache = new CacheClientWithCleanExtension (new LruCacheClient (10));
      cache.Set ("a1", 1, TimeSpan.FromMilliseconds (1));
      cache.Set ("a2", 2, TimeSpan.FromMilliseconds (1));
      System.Threading.Thread.Sleep (1);
      using (var batchUpdater = new CacheClientBatchUpdater (cache)) {
        cache.RemoveByRegex ("c.*");
        using (var batchUpdater2 = new CacheClientBatchUpdater (cache)) {
          cache.RemoveByRegex ("a.*");
          cache.RemoveByRegex ("b.*");
        }
      }
      Assert.That (cache.Get<int?> ("a1"), Is.EqualTo (null));

      cache.Set ("a1", 1, TimeSpan.FromMilliseconds (1));
      cache.Set ("a2", 2, TimeSpan.FromMilliseconds (1));
      System.Threading.Thread.Sleep (1);
      using (var batchUpdater = new CacheClientBatchUpdater (cache)) {
        cache.RemoveByRegex ("c.*");
        cache.RemoveByRegex ("b.*");
      }
      Assert.That (cache.Get<int?> ("a1"), Is.EqualTo (null));

      cache.RemoveByRegex ("a.*");
      Assert.That (cache.Get<int?> ("a1"), Is.EqualTo (null));
    }
  }
}
