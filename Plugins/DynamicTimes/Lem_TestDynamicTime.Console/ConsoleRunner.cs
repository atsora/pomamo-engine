// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lem_TestDynamicTime.Console
{
  /// <summary>
  /// ConsoleRunner
  /// </summary>
  public class ConsoleRunner: IConsoleRunner<Options>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConsoleRunner).FullName);

    readonly IApplicationInitializer m_applicationInitializer;
    Options m_options;

    /// <summary>
    /// Constructor
    /// </summary>
    public ConsoleRunner (IApplicationInitializer applicationInitializer)
    {
      Debug.Assert (null != applicationInitializer);

      m_applicationInitializer = applicationInitializer;
    }

    public async Task RunConsoleAsync (CancellationToken cancellationToken = default)
    {
      await m_applicationInitializer.InitializeApplicationAsync (cancellationToken);

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      try {
        Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();

        DateTime dateTime = DateTime.Parse (m_options.IsoDateTime).ToUniversalTime ();
        string name = m_options.Name;
        IMachine machine;
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (m_options.MachineId);
        }

        LowerBound<DateTime> hintLower = new LowerBound<DateTime> (null);
        if (!string.IsNullOrEmpty (m_options.LowerHint)) {
          hintLower = DateTime.Parse (m_options.LowerHint).ToUniversalTime ();
        }
        UpperBound<DateTime> hintUpper = new UpperBound<DateTime> (null);
        if (!string.IsNullOrEmpty (m_options.UpperHint)) {
          hintUpper = DateTime.Parse (m_options.UpperHint).ToUniversalTime ();
        }
        var hint = new UtcDateTimeRange (hintLower, hintUpper);

        LowerBound<DateTime> lower = new LowerBound<DateTime> (null);
        if (!string.IsNullOrEmpty (m_options.LowerLimit)) {
          lower = DateTime.Parse (m_options.LowerLimit).ToUniversalTime ();
        }
        UpperBound<DateTime> upper = new UpperBound<DateTime> (null);
        if (!string.IsNullOrEmpty (m_options.UpperLimit)) {
          upper = DateTime.Parse (m_options.UpperLimit).ToUniversalTime ();
        }
        var limit = new UtcDateTimeRange (lower, upper);

        var dynamicTimeResponse = Lemoine.Business.DynamicTimes.DynamicTime
            .GetDynamicTime (name, machine, dateTime, hint, limit);
        if (dynamicTimeResponse.Timeout) {
          System.Console.WriteLine ("Timeout");
        }
        else if (dynamicTimeResponse.NotApplicable) {
          System.Console.WriteLine ("Not applicable");
        }
        else if (dynamicTimeResponse.NoData) {
          System.Console.WriteLine ("No data");
        }
        else if (dynamicTimeResponse.Final.HasValue) {
          System.Console.WriteLine ("Final: " + dynamicTimeResponse.Final.Value);
        }
        else {
          System.Console.WriteLine ("Hint: " + dynamicTimeResponse.Hint);
        }
        System.Console.WriteLine ("Implementation: " + dynamicTimeResponse.ImplementationType.AssemblyQualifiedName);
      }
      catch (Exception ex) {
        log.Error ("Main: exception", ex);
        System.Console.Error.WriteLine ($"Exception {ex} raised");
        Environment.ExitCode = 1;
        return;
      }
    }

    public void SetOptions (Options options)
    {
      m_options = options;
    }
  }
}
