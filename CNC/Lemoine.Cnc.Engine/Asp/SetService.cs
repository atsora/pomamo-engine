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
  /// Set service
  /// </summary>
  public sealed class SetService
  {
    readonly ILog log = LogManager.GetLogger (typeof (SetService).FullName);

    readonly IAcquisitionSet m_acquisitionSet;
    readonly IAcquisitionFinder m_acquisitionFinder;

    /// <summary>
    /// Constructor
    /// </summary>
    public SetService (IAcquisitionSet acquisitionSet, IAcquisitionFinder acquisitionFinder)
    {
      m_acquisitionSet = acquisitionSet;
      m_acquisitionFinder = acquisitionFinder;
    }

    /// <summary>
    /// Set request
    /// </summary>
    public SingleResponse Set (CancellationToken cancellationToken, string acquisitionIdentifier, string moduleref, string method, string property, string param, string v, string stringvalue, string longvalue, string intvalue, string doublevalue, string boolvalue)
    {
#if NETSTANDARD || NET48 || NETCOREAPP
      return System.Threading.Tasks.Task.Run (async () => await SetAsync (cancellationToken, acquisitionIdentifier, moduleref, method, property, param, v, stringvalue, longvalue, intvalue, doublevalue, boolvalue)).Result;
    }

    /// <summary>
    /// Set request
    /// </summary>
    public async System.Threading.Tasks.Task<SingleResponse> SetAsync (CancellationToken cancellationToken, string acquisitionIdentifier, string moduleref, string method, string property, string param, string v, string stringvalue, string longvalue, string intvalue, string doublevalue, string boolvalue)
    {
#endif // !(NETSTANDARD || NET48 || NETCOREAPP)
      var singleResponse = new SingleResponse (acquisitionIdentifier: acquisitionIdentifier, moduleref: moduleref, instruction: "set", method: method, property: property, param: param);

      object datavalue;
      if (!string.IsNullOrEmpty (v)) {
        datavalue = v;
      }
      else if (!string.IsNullOrEmpty (stringvalue)) {
        datavalue = stringvalue;
      }
      else if (!string.IsNullOrEmpty (longvalue)) {
        datavalue = long.Parse (longvalue);
      }
      else if (!string.IsNullOrEmpty (intvalue)) {
        datavalue = int.Parse (intvalue);
      }
      else if (!string.IsNullOrEmpty (doublevalue)) {
        datavalue = double.Parse (doublevalue);
      }
      else if (!string.IsNullOrEmpty (boolvalue)) {
        datavalue = bool.Parse (boolvalue);
      }
      else {
        singleResponse.SetError ("No value is set");
        return singleResponse;
      }

      var acquisition = m_acquisitionSet
        .GetAcquisitions (cancellationToken: cancellationToken)
        .FirstOrDefault (a => m_acquisitionFinder.IsMatch (a, acquisitionIdentifier));
      if (acquisition is null) {
        singleResponse.SetMissingAcquisition ();
        return singleResponse;
      }

      CncModuleExecutor moduleExecutor;
      try {
        moduleExecutor = acquisition.GetModule (moduleref);
      }
      catch (KeyNotFoundException ex) {
        log.Error ($"InvokeAsync: GetModule returned KeyNotFoundException for moduleref {moduleref}", ex);
        singleResponse.SetMissingModule ();
        return singleResponse;
      }
      if (null == moduleExecutor) {
        log.Error ($"InvokeAsync: the cnc data handler has not been initialized yet");
        singleResponse.SetError ("Initializing");
        return singleResponse;
      }

#if NETSTANDARD || NET48 || NETCOREAPP
      using (var semaphoreHolder = await Lemoine.Threading.SemaphoreSlimHolder.CreateAsync (moduleExecutor.Semaphore, cancellationToken)) {
#else // !(NETSTANDARD || NET48 || NETCOREAPP)
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (moduleExecutor.Semaphore, cancellationToken)) {
#endif  // !(NETSTANDARD || NET48 || NETCOREAPP)
        cancellationToken.ThrowIfCancellationRequested ();

        if (!moduleExecutor.Start ()) {
          singleResponse.SetStartError ();
          return singleResponse;
        }

        try {
          if (!string.IsNullOrEmpty (method)) {
            if (moduleExecutor.TrySet (method, datavalue, () => param)) {
              singleResponse.SetSuccess ();
            }
            else {
              singleResponse.SetMissingMethod ();
            }
          }
          else if (!string.IsNullOrEmpty (property)) {
            if (moduleExecutor.TrySetProperty (property, datavalue)) {
              singleResponse.SetSuccess ();
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
          log.Error ($"SetAsync: exception in TrySet", ex);
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
