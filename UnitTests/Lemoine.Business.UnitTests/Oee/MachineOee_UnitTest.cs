// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Lemoine.Business.Oee;
using Lemoine.Core.Log;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Business.UnitTests.Oee
{
  /// <summary>
  /// Unit tests for <see cref="MachineOee"/>
  ///
  /// The reason slots are created on 2018-05-02, a day without any reason slot in the unit test database.
  /// Dedicated machine observation states are created for each capacity level.
  /// The request is called directly, without <see cref="ServiceProvider"/>, so that no result is cached
  /// </summary>
  [TestFixture]
  public class MachineOee_UnitTest
  {
    static readonly string USE_SUMMARY_KEY = "Business.Oee.UseSummary";
    static readonly string PRODUCTION_RATE_SUMMARY_ACTIVE_KEY = "Summary.ProductionRate.Active";

    static readonly DateTime DAY = new DateTime (2018, 05, 02);

    readonly ILog log = LogManager.GetLogger (typeof (MachineOee_UnitTest).FullName);

    IMachine m_machine;
    IMachineMode m_machineMode;
    IReason m_reason;
    IShift m_shift1;
    IShift m_shift2;
    IMachineObservationState m_expectedProduction;
    IMachineObservationState m_open;
    IMachineObservationState m_closed;
    UtcDateTimeRange m_dayRange;

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineOee_UnitTest ()
    { }

    /// <summary>
    /// Weighting by the production rate, cut of the slots at the range limits
    /// and selection of the machine observation states by capacity level
    /// </summary>
    [Test]
    public void TestReasonSlots ()
    {
      RunInTransaction (() => {
        CreateReferenceReasonSlots ();

        {
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Oee).Get ();
          CheckResponse (response, 6, 4, 2);
          Assert.That (response.Rate, Is.EqualTo (4.0 / 6.0).Within (1e-9));
        }
        {
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Ooe).Get ();
          CheckResponse (response, 10, 5, 2);
          Assert.That (response.Rate, Is.EqualTo (0.5).Within (1e-9));
        }
        {
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Teep).Get ();
          CheckResponse (response, 22, 5, 2);
          Assert.That (response.Rate, Is.EqualTo (5.0 / 22.0).Within (1e-9));
        }
      });
    }

    /// <summary>
    /// Same as <see cref="TestReasonSlots"/> with the asynchronous request
    /// </summary>
    [Test]
    public async Task TestReasonSlotsAsync ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue (PRODUCTION_RATE_SUMMARY_ACTIVE_KEY, false);
          LoadReferenceData ();
          CreateReferenceReasonSlots ();

          var response = await new MachineOee (m_machine, m_dayRange, OeeType.Ooe).GetAsync ();
          CheckResponse (response, 10, 5, 2);
          Assert.That (response.Rate, Is.EqualTo (0.5).Within (1e-9));
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Only the reason slots of the requested shift are considered
    /// </summary>
    [Test]
    public void TestShift ()
    {
      RunInTransaction (() => {
        CreateReasonSlot (0, 4, m_expectedProduction, 1.0, m_shift1);
        CreateReasonSlot (4, 8, m_expectedProduction, 0.5, m_shift2);
        CreateReasonSlot (8, 12, m_expectedProduction, 0.25, null);

        {
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Oee, m_shift1).Get ();
          CheckResponse (response, 4, 4, 0);
          Assert.That (response.Rate, Is.EqualTo (1.0).Within (1e-9));
        }
        {
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Oee, m_shift2).Get ();
          CheckResponse (response, 4, 2, 0);
          Assert.That (response.Rate, Is.EqualTo (0.5).Within (1e-9));
        }
        { // No shift: all the slots
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Oee).Get ();
          CheckResponse (response, 12, 7, 0);
        }
      });
    }

    /// <summary>
    /// The rate is null when no reference time is available
    /// </summary>
    [Test]
    public void TestNoData ()
    {
      RunInTransaction (() => {
        { // No reason slot at all
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Oee).Get ();
          CheckResponse (response, 0, 0, 0);
          Assert.That (response.Rate, Is.Null);
        }

        // Only a closed period, and a period without any production rate
        CreateReasonSlot (0, 12, m_closed, 0.0);
        CreateReasonSlot (12, 24, m_expectedProduction, null);
        {
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Oee).Get ();
          CheckResponse (response, 0, 0, 12);
          Assert.That (response.Rate, Is.Null);
        }
        {
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Teep).Get ();
          CheckResponse (response, 12, 0, 12);
          Assert.That (response.Rate, Is.EqualTo (0.0));
        }
      });
    }

    /// <summary>
    /// An empty range returns an empty response
    /// </summary>
    [Test]
    public void TestEmptyRange ()
    {
      RunInTransaction (() => {
        var response = new MachineOee (m_machine, new UtcDateTimeRange (), OeeType.Oee).Get ();
        Assert.Multiple (() => {
          Assert.That (response.Duration, Is.EqualTo (TimeSpan.Zero));
          Assert.That (response.ProductionDuration, Is.EqualTo (TimeSpan.Zero));
          Assert.That (response.Rate, Is.Null);
        });
      });
    }

    /// <summary>
    /// The summary table is used only when it is active and the range matches a whole number of days
    /// </summary>
    [Test]
    public void TestSummary ()
    {
      RunInTransaction (() => {
        Lemoine.Info.ConfigSet.ForceValue (PRODUCTION_RATE_SUMMARY_ACTIVE_KEY, true);

        // Reason slots (the reference data)
        CreateReferenceReasonSlots ();

        // Summary, with different values to know which source is used
        CreateSummary (m_expectedProduction, m_shift1, 8, 0.5);
        CreateSummary (m_expectedProduction, m_shift2, 2, 1.0);
        CreateSummary (m_open, m_shift1, 4, 0.0);
        CreateSummary (m_closed, null, 10, 0.1);

        { // Whole day => summary
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Oee).Get ();
          Assert.Multiple (() => {
            Assert.That (response.FromSummary, Is.True);
            Assert.That (response.NoDataDuration, Is.Null);
            Assert.That (response.Duration, Is.EqualTo (TimeSpan.FromHours (10)));
            Assert.That (response.ProductionDuration, Is.EqualTo (TimeSpan.FromHours (6)));
          });
        }
        { // Whole day restricted to a shift => summary
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Ooe, m_shift1).Get ();
          Assert.Multiple (() => {
            Assert.That (response.FromSummary, Is.True);
            Assert.That (response.Duration, Is.EqualTo (TimeSpan.FromHours (12)));
            Assert.That (response.ProductionDuration, Is.EqualTo (TimeSpan.FromHours (4)));
          });
        }
        { // TEEP on the whole day => summary, closed included
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Teep).Get ();
          Assert.Multiple (() => {
            Assert.That (response.FromSummary, Is.True);
            Assert.That (response.Duration, Is.EqualTo (TimeSpan.FromHours (24)));
            Assert.That (response.ProductionDuration, Is.EqualTo (TimeSpan.FromHours (7)));
          });
        }
        { // Not a whole day => reason slots
          var range = new UtcDateTimeRange (T (0), T (12));
          var response = new MachineOee (m_machine, range, OeeType.Oee).Get ();
          Assert.That (response.FromSummary, Is.False);
          CheckResponse (response, 6, 4, 2);
        }

        // Summary turned off by the Business.Oee.UseSummary key => reason slots
        Lemoine.Info.ConfigSet.ForceValue (USE_SUMMARY_KEY, false);
        {
          var response = new MachineOee (m_machine, m_dayRange, OeeType.Oee).Get ();
          Assert.That (response.FromSummary, Is.False);
          CheckResponse (response, 6, 4, 2);
        }
      });
    }

    /// <summary>
    /// The cache key depends on all the parameters of the request
    /// </summary>
    [Test]
    public void TestCacheKey ()
    {
      RunInTransaction (() => {
        var key = new MachineOee (m_machine, m_dayRange, OeeType.Oee).GetCacheKey ();
        Assert.Multiple (() => {
          Assert.That (new MachineOee (m_machine, m_dayRange, OeeType.Oee).GetCacheKey (), Is.EqualTo (key));
          Assert.That (new MachineOee (m_machine, m_dayRange, OeeType.Ooe).GetCacheKey (), Is.Not.EqualTo (key));
          Assert.That (new MachineOee (m_machine, m_dayRange, OeeType.Oee, m_shift1).GetCacheKey (), Is.Not.EqualTo (key));
          Assert.That (new MachineOee (m_machine, new UtcDateTimeRange (T (0), T (12)), OeeType.Oee).GetCacheKey (), Is.Not.EqualTo (key));
        });
      });
    }

    /// <summary>
    /// Run a test in a transaction that is rolled back at the end,
    /// with the reference data loaded and the summary table disabled by default
    /// </summary>
    /// <param name="test"></param>
    void RunInTransaction (Action test)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          Lemoine.Info.ConfigSet.ForceValue (PRODUCTION_RATE_SUMMARY_ACTIVE_KEY, false);
          LoadReferenceData ();
          test ();
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void LoadReferenceData ()
    {
      m_machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
      m_machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (1);
      m_reason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (1);
      m_shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
      m_shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
      Assert.Multiple (() => {
        Assert.That (m_machine, Is.Not.Null);
        Assert.That (m_machineMode, Is.Not.Null);
        Assert.That (m_reason, Is.Not.Null);
        Assert.That (m_shift1, Is.Not.Null);
        Assert.That (m_shift2, Is.Not.Null);
      });

      m_expectedProduction = CreateMachineObservationState ("OeeTestExpectedProduction", CapacityLevel.ExpectedProduction);
      m_open = CreateMachineObservationState ("OeeTestOpen", CapacityLevel.Open);
      m_closed = CreateMachineObservationState ("OeeTestClosed", CapacityLevel.Closed);

      m_dayRange = ServiceProvider
        .Get (new Lemoine.Business.Time.RangeFromDayRange (new DayRange (DAY, DAY)));
      Assert.Multiple (() => {
        Assert.That (m_dayRange.Lower.HasValue, Is.True);
        Assert.That (m_dayRange.Duration, Is.EqualTo (TimeSpan.FromDays (1)));
      });

      var existingSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindOverlapsRange (m_machine, m_dayRange);
      Assert.That (existingSlots, Is.Empty, "Reason slots already exist in the test day");
    }

    /// <summary>
    /// Reason slots used in several tests, on the day range [L, L+24h]:
    /// <item>[L-2h, L+2h]: expected production, rate 1 => 2h in the range, 2h of production</item>
    /// <item>[L+2h, L+6h]: expected production, rate 0.5 => 4h, 2h of production</item>
    /// <item>[L+6h, L+8h]: expected production, no rate => 2h without any data</item>
    /// <item>[L+8h, L+12h]: open, rate 0.25 => 4h, 1h of production</item>
    /// <item>[L+12h, L+24h]: closed, rate 0 => 12h, no production</item>
    ///
    /// OEE: 6h, 4h of production. OOE: 10h, 5h of production. TEEP: 22h, 5h of production
    /// </summary>
    void CreateReferenceReasonSlots ()
    {
      CreateReasonSlot (-2, 2, m_expectedProduction, 1.0);
      CreateReasonSlot (2, 6, m_expectedProduction, 0.5);
      CreateReasonSlot (6, 8, m_expectedProduction, null);
      CreateReasonSlot (8, 12, m_open, 0.25);
      CreateReasonSlot (12, 24, m_closed, 0.0);
    }

    IMachineObservationState CreateMachineObservationState (string name, CapacityLevel capacityLevel)
    {
      var machineObservationState = ModelDAOHelper.ModelFactory
        .CreateMachineObservationState ();
      machineObservationState.Name = name;
      machineObservationState.CapacityLevel = capacityLevel;
      machineObservationState.IsProduction = (CapacityLevel.ExpectedProduction == capacityLevel);
      ModelDAOHelper.DAOFactory.MachineObservationStateDAO.MakePersistent (machineObservationState);
      return machineObservationState;
    }

    void CreateReasonSlot (double startHour, double endHour, IMachineObservationState machineObservationState, double? productionRate, IShift shift = null)
    {
      var reasonSlot = new ReasonSlot (m_machine, new UtcDateTimeRange (T (startHour), T (endHour)));
      reasonSlot.MachineMode = m_machineMode;
      reasonSlot.MachineObservationState = machineObservationState;
      reasonSlot.Shift = shift;
      reasonSlot.SetDefaultReason (m_reason, 10.0, false, true, new UpperBound<DateTime> (null));
      reasonSlot.Consolidate (null, null);
      reasonSlot.ProductionRate = productionRate;
      ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
    }

    void CreateSummary (IMachineObservationState machineObservationState, IShift shift, double hours, double productionRate)
    {
      var summary = ModelDAOHelper.ModelFactory
        .CreateProductionRateSummary (m_machine, DAY, shift, machineObservationState);
      summary.Duration = TimeSpan.FromHours (hours);
      summary.ProductionRate = productionRate;
      ModelDAOHelper.DAOFactory.ProductionRateSummaryDAO.MakePersistent (summary);
    }

    /// <summary>
    /// Date/time relative to the start of the test day
    /// </summary>
    DateTime T (double hours) => m_dayRange.Lower.Value.AddHours (hours);

    void CheckResponse (OeeResponse response, double durationHours, double productionHours, double noDataHours)
    {
      Assert.Multiple (() => {
        Assert.That (response.Duration, Is.EqualTo (TimeSpan.FromHours (durationHours)), "Duration");
        Assert.That (response.ProductionDuration, Is.EqualTo (TimeSpan.FromHours (productionHours)), "ProductionDuration");
        Assert.That (response.NoDataDuration, Is.EqualTo (TimeSpan.FromHours (noDataHours)), "NoDataDuration");
        Assert.That (response.FromSummary, Is.False, "FromSummary");
      });
    }
  }
}
