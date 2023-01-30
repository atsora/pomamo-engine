// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonMidCycle
{
  /// <summary>
  /// Detect stops between cycles and apply an autoreason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    /// <summary>
    /// Set to true for testing purpose (to remove the getLastEffective condition)
    /// False by default
    /// </summary>
    public bool TestMode { get; set; }

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.StopMidCycles")
    {
      TestMode = false;
    }

    /// <summary>
    /// Additional configuration of the plugin that comes from the database or from the plugin configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      // Everything is ok
      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.StopWithinCycles.Check")) {
          // Find the next operation cycle
          var nextOperationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO.GetFirstBeginAfter (Machine, DateTime);
          if (nextOperationCycle != null && nextOperationCycle.Begin.HasValue) {
            // Process the period
            ProcessStart (nextOperationCycle.Begin.Value);

            // We go on
            this.GoOn (nextOperationCycle.Begin.Value);
          }
        }
      }
    }

    void GoOn (DateTime dateTime)
    {
      if (dateTime <= this.DateTime) {
        dateTime = this.DateTime.AddSeconds (1);
      }
      AddUpdateDateTimeDelayedAction (dateTime);
    }

    void ProcessStart (DateTime start)
    {
      var range = new UtcDateTimeRange (start);
      AddReason (range);
    }

    void AddReason (UtcDateTimeRange range)
    {
      var action = new ApplyReasonAction (this, range, ",CycleEnd");
      AddDelayedAction (action);
    }
  }
}
