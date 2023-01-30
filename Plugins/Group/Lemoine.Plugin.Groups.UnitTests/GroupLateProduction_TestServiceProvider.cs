// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Business;
using Lemoine.Business.Operation;
using Lemoine.GDBPersistentClasses;

namespace Lemoine.Plugin.Groups.UnitTests
{
  class GroupLateProduction_TestServiceProvider : IService
  {
    public async System.Threading.Tasks.Task<T> GetAsync<T> (IRequest<T> request)
    {
      return await System.Threading.Tasks.Task.FromResult (Get (request));
    }

    /// <summary>
    /// Get the result of a request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public T Get<T> (IRequest<T> request)
    {
      if (request is PartProductionCurrentShiftOperation) {
        var typedRequest = request as PartProductionCurrentShiftOperation;

        // Current machine
        var machine = typedRequest.Machine;

        // Corresponding values for the machine
        PartProductionCurrentShiftOperationResponse typedResult = new PartProductionCurrentShiftOperationResponse ();
        switch (machine.Name[machine.Name.Length - 1]) {
        case '1':
          typedResult.NbPiecesCurrentShift = 2;
          typedResult.GoalCurrentShift = 10;
          break;
        case '2':
          typedResult.NbPiecesCurrentShift = 12;
          typedResult.GoalCurrentShift = 10;
          break;
        case '3':
          typedResult.NbPiecesCurrentShift = 0;
          typedResult.GoalCurrentShift = 0;
          break;
        default:
          typedResult.NbPiecesCurrentShift = 0;
          typedResult.GoalCurrentShift = 0;
          break;
        }

        return (T)Convert.ChangeType (typedResult, typeof (T));
      }

      return request.Get ();
    }

    /// <summary>
    /// Get the data in cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns>null if no cache data</returns>
    public CacheValue<T> GetCacheData<T> (IRequest<T> request)
    {
      return null;
    }
  }
}
