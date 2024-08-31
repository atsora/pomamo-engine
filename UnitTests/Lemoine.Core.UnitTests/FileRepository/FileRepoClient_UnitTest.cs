// Copyright (C) 2024 Atsora Solutions

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.FileRepository;
using NUnit.Framework;

namespace Lemoine.Core.FileRepository.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class FileRepoClient_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (FileRepoClient_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoClient_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestSynchronize ()
    {
      var src = "C:\\FileRepoTestSrc";
      Directory.CreateDirectory (src);
      var dest = "C:\\FileRepoTestDest";
      var srca = Path.Combine (src, "a");
      var srcaa = Path.Combine (srca, "a");
      var srcab = Path.Combine (srca, "b");
      var srcac = Path.Combine (srca, "c");
      var srcaac = Path.Combine (srcaa, "c");
      var desta = Path.Combine (dest, "a");
      var destb = Path.Combine (dest, "b");
      var destc = Path.Combine (dest, "c");
      var destac = Path.Combine (desta, "c");
      try {
        Directory.CreateDirectory (dest);
        Directory.CreateDirectory (srca);
        Directory.CreateDirectory (srcaa);
        Directory.CreateDirectory (srcab);
        using (File.Create (srcac)) { }
        using (File.Create (srcaac)) { }
        var fileRepoClient = new Lemoine.FileRepository.FileRepoClientDirectory (src);
        var ld = fileRepoClient.ListDirectoriesInDirectory ("a", "");
        Assert.That (ld, Has.Count.EqualTo (2));
        FileRepoClient.Implementation = fileRepoClient;
        FileRepoClient.TrySynchronize ("a", dest);
        Assert.That (File.Exists (destc), Is.True);
        Assert.That (File.Exists (destac), Is.True);
        Assert.That (Directory.Exists (destb), Is.True);
      }
      finally {
        Directory.Delete (src, true);
        Directory.Delete (dest, true);
      }
    }
  }
}
