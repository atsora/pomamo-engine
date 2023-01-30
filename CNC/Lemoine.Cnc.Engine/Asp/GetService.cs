// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Cnc.Asp;
using Lemoine.CncEngine;
using Lemoine.Core.Log;

namespace Lemoine.CncEngine.Asp
{
  /// <summary>
  /// Get service
  /// </summary>
  public sealed class GetService
  {
    readonly ILog log = LogManager.GetLogger (typeof (GetService).FullName);

    readonly IAcquisitionSet m_acquisitionSet;
    readonly IAcquisitionFinder m_acquisitionFinder;

    /// <summary>
    /// Constructor
    /// </summary>
    public GetService (IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder)
    {
      m_acquisitionSet = acquisitionSet;
      m_acquisitionFinder = acquisitionFinder;
    }

    /// <summary>
    /// Get request
    /// </summary>
    public SingleResponse Get (CancellationToken cancellationToken, string acquisitionIdentifier, string moduleref, string method, string property, string param)
    {
#if NETSTANDARD || NET48 || NETCOREAPP
      return System.Threading.Tasks.Task.Run (async () => await GetAsync (cancellationToken, acquisitionIdentifier, moduleref, method, property, param)).Result;
    }

    /// <summary>
    /// Get request asynchronously
    /// </summary>
    public async System.Threading.Tasks.Task<SingleResponse> GetAsync (CancellationToken cancellationToken, string acquisitionIdentifier, string moduleref, string method, string property, string param)
    {
#endif // NETSTANDARD || NET48 || NETCOREAPP
      var singleResponse = new SingleResponse (acquisitionIdentifier: acquisitionIdentifier, moduleref: moduleref, instruction: "get", method: method, property: property, param: param);

      var acquisition = m_acquisitionSet
        .GetAcquisitions (cancellationToken: cancellationToken)
        .FirstOrDefault (a => m_acquisitionFinder.IsMatch (a, acquisitionIdentifier));
      if (acquisition is null) {
        log.Error ($"GetAsync: missing acquisition");
        singleResponse.SetMissingAcquisition ();
        return singleResponse;
      }

      CncModuleExecutor moduleExecutor;
      try {
        moduleExecutor = acquisition.GetModule (moduleref);
      }
      catch (KeyNotFoundException ex) {
        log.Error ($"GetAsync: GetModule returned KeyNotFoundException for moduleref {moduleref}", ex);
        singleResponse.SetMissingModule ();
        return singleResponse;
      }
      if (null == moduleExecutor) {
        log.Error ($"GetAsync: the cnc data handler has not been initialized yet");
        singleResponse.SetError ("Initializing");
        return singleResponse;
      }

#if NETSTANDARD || NET48 || NETCOREAPP
      using (var semaphoreHolder = await Lemoine.Threading.SemaphoreSlimHolder.CreateAsync (moduleExecutor.Semaphore, cancellationToken)) {
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (moduleExecutor.Semaphore, cancellationToken)) {
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)
        cancellationToken.ThrowIfCancellationRequested ();

        if (!moduleExecutor.Start ()) {
          singleResponse.SetStartError ();
          return singleResponse;
        }

        try {
          if (!string.IsNullOrEmpty (method)) {
            if (moduleExecutor.TryGet (out object result, method, param)) {
              singleResponse.SetResult (result);
            }
            else {
              singleResponse.SetMissingMethod ();
            }
          }
          else if (!string.IsNullOrEmpty (property)) {
            if (moduleExecutor.TryGetProperty (out object result, property)) {
              singleResponse.SetResult (result);
            }
            else {
              singleResponse.SetMissingProperty ();
            }
          }
          else {
            singleResponse.SetError ("No method or property was set");
          }
          return singleResponse;
        }
        catch (Exception ex) {
          log.Error ($"GetAsync: exception", ex);
          singleResponse.SetError (ex.Message);
          return singleResponse;
        }
        finally {
          moduleExecutor.Finish ();
        }
      }
    }
  }
}
