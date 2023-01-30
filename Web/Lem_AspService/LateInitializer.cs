// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Hosting.AsyncInitialization;
using Lemoine.Core.Log;
using Lemoine.FileRepository;
using Lemoine.I18N;
using Lemoine.Info.ConfigReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lem_AspService
{
  public class LateInitializer: IAsyncInitializer
  {
    readonly ILog log = LogManager.GetLogger<LateInitializer> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public LateInitializer ()
    {
    }

    public Task InitializeAsync ()
    {
      /* // TODO: check if this is really required
      log.Info ("InitializeAsync: activate DaySlotCache");
      Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();
      log.Info ("InitializeAsync: activate OperationCompletionCache");
      Lemoine.GDBPersistentClasses.OperationCompletionCache.Activate ();
      */
      
      if (!Lemoine.FileRepository.FileRepoClient.Implementation.Test ()) {
        log.Error ("InitializeAsync: active file repository implementation is not valid right now but it may be ok later");
      }
      return Task.CompletedTask;
    }
  }
}
