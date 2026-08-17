// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Business.Oee;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Business.UnitTests.Oee
{
  /// <summary>
  /// Unit tests for <see cref="OeeTypeExtensions"/>
  /// </summary>
  public class OeeType_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (OeeType_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OeeType_UnitTest ()
    { }

    /// <summary>
    /// Test both the English and the French acronyms are accepted
    /// </summary>
    [Test]
    public void TestTryParse ()
    {
      Assert.Multiple (() => {
        Assert.That (TryParse ("OEE"), Is.EqualTo (OeeType.Oee));
        Assert.That (TryParse ("trs"), Is.EqualTo (OeeType.Oee));
        Assert.That (TryParse (" Ooe "), Is.EqualTo (OeeType.Ooe));
        Assert.That (TryParse ("TRG"), Is.EqualTo (OeeType.Ooe));
        Assert.That (TryParse ("teep"), Is.EqualTo (OeeType.Teep));
        Assert.That (TryParse ("TRE"), Is.EqualTo (OeeType.Teep));
        Assert.That (OeeTypeExtensions.TryParse ("", out var _), Is.False);
        Assert.That (OeeTypeExtensions.TryParse ("unknown", out var _), Is.False);
      });
    }

    OeeType TryParse (string s)
    {
      Assert.That (OeeTypeExtensions.TryParse (s, out var oeeType), Is.True);
      return oeeType;
    }

    /// <summary>
    /// Test which capacity levels are part of each rate
    /// </summary>
    [Test]
    public void TestIsIncluded ()
    {
      var closed = CreateMachineObservationState (CapacityLevel.Closed);
      var open = CreateMachineObservationState (CapacityLevel.Open);
      var expectedProduction = CreateMachineObservationState (CapacityLevel.ExpectedProduction);

      Assert.Multiple (() => {
        Assert.That (OeeType.Oee.IsIncluded (expectedProduction), Is.True);
        Assert.That (OeeType.Oee.IsIncluded (open), Is.False);
        Assert.That (OeeType.Oee.IsIncluded (closed), Is.False);

        Assert.That (OeeType.Ooe.IsIncluded (expectedProduction), Is.True);
        Assert.That (OeeType.Ooe.IsIncluded (open), Is.True);
        Assert.That (OeeType.Ooe.IsIncluded (closed), Is.False);

        Assert.That (OeeType.Teep.IsIncluded (expectedProduction), Is.True);
        Assert.That (OeeType.Teep.IsIncluded (open), Is.True);
        Assert.That (OeeType.Teep.IsIncluded (closed), Is.True);
      });
    }

    /// <summary>
    /// Test the fall-back on IsProduction when no capacity level is configured
    /// </summary>
    [Test]
    public void TestIsProductionFallback ()
    {
      var production = CreateMachineObservationState (null);
      production.IsProduction = true;
      var notProduction = CreateMachineObservationState (null);
      notProduction.IsProduction = false;

      Assert.Multiple (() => {
        Assert.That (production.GetCapacityLevel (), Is.EqualTo (CapacityLevel.ExpectedProduction));
        Assert.That (notProduction.GetCapacityLevel (), Is.EqualTo (CapacityLevel.Closed));
        Assert.That (OeeType.Oee.IsIncluded (production), Is.True);
        Assert.That (OeeType.Oee.IsIncluded (notProduction), Is.False);
        Assert.That (OeeType.Teep.IsIncluded (notProduction), Is.True);
      });

      try {
        Lemoine.Info.ConfigSet.ForceValue ("Business.Oee.UseIsProductionFallback", false);
        Assert.Multiple (() => {
          Assert.That (production.GetCapacityLevel (), Is.Null);
          Assert.That (OeeType.Oee.IsIncluded (production), Is.False);
          Assert.That (OeeType.Ooe.IsIncluded (production), Is.False);
          Assert.That (OeeType.Teep.IsIncluded (production), Is.True);
        });
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    IMachineObservationState CreateMachineObservationState (CapacityLevel? capacityLevel)
    {
      var machineObservationState = ModelDAOHelper.ModelFactory
        .CreateMachineObservationState ();
      machineObservationState.CapacityLevel = capacityLevel;
      return machineObservationState;
    }
  }
}
