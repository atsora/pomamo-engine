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

namespace Pulse.Business.Computer
{
  /// <summary>
  /// ValidLctrChecker
  /// </summary>
  public class ValidLctrChecker: IValidServerChecker, IApplicationInitializer
  {
    readonly ILog log = LogManager.GetLogger (typeof (ValidLctrChecker).FullName);

    const string CHECK_LCTR_KEY = "CheckLctr";
    const bool CHECK_LCTR_DEFAULT = true;

    ILctrChecker m_lctrChecker;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ValidLctrChecker (ILctrChecker lctrChecker)
    {
      Debug.Assert (null != lctrChecker);

      m_lctrChecker = lctrChecker;
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void InitializeApplication (CancellationToken cancellationToken = default)
    {
      log.Info ("InitializeApplication: check if the server is an lctr server");

      // Check the computer is the auto-reason server
      // not to run twice the auto-reason service
      // If not the auto-reason server, just do nothing
      if (!IsValidServerForService () && Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (CHECK_LCTR_KEY, CHECK_LCTR_DEFAULT)) {
        log.Error ("InitializeApplication: do nothing, not the right server");
        cancellationToken.WaitHandle.WaitOne ();
        cancellationToken.ThrowIfCancellationRequested ();
      }
    }

    /// <summary>
    /// <see cref="IApplicationInitializer"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InitializeApplicationAsync (CancellationToken cancellationToken = default)
    {
      await Task.Delay (0);
      InitializeApplication (cancellationToken);
    }

    /// <summary>
    /// <see cref="IValidServerChecker"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool IsValidServerForService ()
    {
      return m_lctrChecker.IsLctr ();
    }
    #endregion // Constructors

  }
}
