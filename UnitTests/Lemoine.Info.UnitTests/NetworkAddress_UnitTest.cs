// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Net;
using log4net;
using NUnit.Framework;

namespace Lemoine.Info.UnitTests
{
  /// <summary>
  /// Unit tests for the class NetworkAddress.
  /// 
  /// TODO: To be moved later in another project (Lemoine.Net.UnitTests)
  /// or get only one project for both Lemoine.Net and Lemoine.Info
  /// </summary>
  [TestFixture]
  public class NetworkAddress_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NetworkAddress_UnitTest).FullName);

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestGetIPAddressesV4OnlyLocalhost ()
    {
      var ipAddresses = Lemoine.Net.NetworkAddress.GetIPAddressesV4Only ("localhost");
      Assert.IsTrue (ipAddresses.Contains (new IPAddress (new byte[] { 127, 0, 0, 1 })));
    }

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestGetIPAddressesV4Only127001 ()
    {
      var ipAddresses = Lemoine.Net.NetworkAddress.GetIPAddressesV4Only ("127.0.0.1");
      Assert.IsTrue (ipAddresses.Contains (new IPAddress (new byte[] { 127, 0, 0, 1 })));
    }

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestGetIPAddressesV4Only3333 ()
    {
      var ipAddresses = Lemoine.Net.NetworkAddress.GetIPAddressesV4Only ("3.3.3.3");
      Assert.IsTrue (ipAddresses.Contains (new IPAddress (new byte[] { 3, 3, 3, 3 })));
    }
  }
}
