// Copyright (c) 2023 Atsora Solutions

using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Lemoine.Core.UnitTests.Security
{
  /// <summary>
  /// 
  /// </summary>
  public class Identity_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (Identity_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Identity_UnitTest ()
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestRunImpersonatedAsExplorerUser ()
    {
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        var userName = Lemoine.Core.Security.Identity.RunImpersonatedAsExplorerUser (GetCurrentUser);
        Assert.That (userName, Is.EqualTo ("NICOLAS-LAPTOP\\nrela"));
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestRunImpersonatedAsFileOwner ()
    {
      var path1 = "C:\\Windows\\regedit.exe";
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        Assert.Throws<System.Security.SecurityException> (() => Lemoine.Core.Security.Identity.RunImpersonatedAsFileOwner (path1, GetCurrentUser)); // Since "NT SERVICE\TrustedInstaller" is not a domain user
      }
    }

    string GetCurrentUser ()
    {
      if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        return WindowsIdentity.GetCurrent ().Name;
      }
      else {
        throw new PlatformNotSupportedException ();
      }
    }
  }
}
