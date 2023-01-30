// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Get a default sequence time from the operation duration
  /// in case it is not set up directly in the sequence level
  /// </summary>
  internal sealed class DefaultSequenceTime
    : IRequest<DefaultSequenceTimeResponse>
  {
    static readonly string STANDARD_SEQUENCE_TIME_MULTI_PATH_KEY = "DefaultSequenceTime.MultiPath";
    static readonly bool STANDARD_SEQUENCE_TIME_MULTI_PATH_DEFAULT = false;

    static readonly ILog log = LogManager.GetLogger (typeof (DefaultSequenceTime).FullName);

    /// <summary>
    /// Reference to the operation
    /// 
    /// not null
    /// </summary>
    public IOperation Operation { get; internal set; }

    /// <summary>
    /// Reference to the operation path
    /// 
    /// not null
    /// </summary>
    public IPath Path { get; internal set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="path">not null</param>
    public DefaultSequenceTime (IOperation operation, IPath path)
    {
      Debug.Assert (null != operation);
      Debug.Assert (null != path);

      this.Operation = operation;
      this.Path = path;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>DefaultSequenceTimeResponse</returns>
    public DefaultSequenceTimeResponse Get ()
    {
      var response = new DefaultSequenceTimeResponse ();

      var operation = this.Operation;
      var path = this.Path;

      var operationInitialized = ModelDAOHelper.DAOFactory.IsInitialized (operation);
      var pathInitialized = ModelDAOHelper.DAOFactory.IsInitialized (path);

      if (!operationInitialized || !pathInitialized) {
        // => get a new instance here of the object
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Business.Operation.DefaultSequenceTime.Initialization")) {
            if (!operationInitialized) {
              operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (((IDataWithId)operation).Id);
              if (IsNoStandardSequenceTimeMultiPath ()) {
                ModelDAOHelper.DAOFactory.Initialize (operation.Paths);
                foreach (var p in operation.Paths) {
                  ModelDAOHelper.DAOFactory.Initialize (p.Sequences);
                }
              }
            }
            if (operation.MachiningDuration.HasValue && !pathInitialized) {
              path = ModelDAOHelper.DAOFactory.PathDAO.FindById (((IDataWithId)path).Id);
            }
          }
        }
      }
      else if (IsNoStandardSequenceTimeMultiPath ()) {
        var pathsInitialized = ModelDAOHelper.DAOFactory.IsInitialized (operation.Paths);
        if (pathsInitialized) {
          foreach (var p in operation.Paths) {
            if (!ModelDAOHelper.DAOFactory.IsInitialized (p.Sequences)) {
              pathsInitialized = false;
            }
          }
        }
        if (!pathsInitialized) {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ("Business.Operation.DefaultSequenceTime.InitializationPaths")) {
              operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (((IDataWithId)operation).Id);
              ModelDAOHelper.DAOFactory.Initialize (operation.Paths);
              foreach (var p in operation.Paths) {
                ModelDAOHelper.DAOFactory.Initialize (p.Sequences);
              }
            }
          }
        }
      }
      if (IsNoStandardSequenceTimeMultiPath ()) {
        if (1 < operation.Paths.Where (p => p.Sequences.Any ()).Count ()) {
          log.InfoFormat ("Get: no standard sequence time for multi-path: return null");
          var noStandardResponse = new DefaultSequenceTimeResponse ();
          return noStandardResponse;
        }
      }

      if (operation.MachiningDuration.HasValue) {
        // Because this.Path may be lazy, get the sequences with a new SQL request
        IEnumerable<ISequence> sequences;
        if (ModelDAOHelper.DAOFactory.IsInitialized (path.Sequences)) {
          sequences = path.Sequences;
        }
        else {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ("Business.Operation.DefaultSequenceTime.Sequences")) {
              sequences = ModelDAOHelper.DAOFactory.SequenceDAO.FindAllWithPath (path);
            }
          }
        }

        // - Give the priority first of the machining sequences
        // Get the number of sequences for this operation with no estimated sequence duration
        int machiningSequenceWithNoDurationNumber = sequences.Count (s => !s.EstimatedTime.HasValue
                            && (SequenceKind.Machining == s.Kind));
        if (0 < machiningSequenceWithNoDurationNumber) {
          response.MachiningSequenceDuration = GetDefaultSequenceDuration (operation, path, machiningSequenceWithNoDurationNumber);
        }
        else {
          // - Else to the non-machining sequences
          int nonMachiningSequenceWithNoDurationNumber = sequences.Count (s => !s.EstimatedTime.HasValue
                              && (SequenceKind.NonMachining == s.Kind));
          if (0 < nonMachiningSequenceWithNoDurationNumber) {
            response.NonMachiningSequenceDuration = GetDefaultSequenceDuration (operation, path, nonMachiningSequenceWithNoDurationNumber);
          }
          else {
            // - Else to the stops
            int stopSequenceWithNoDurationNumber = sequences.Count (s => !s.EstimatedTime.HasValue
                                && (SequenceKind.Stop == s.Kind));
            if (0 < stopSequenceWithNoDurationNumber) {
              response.StopSequenceDuration = GetDefaultSequenceDuration (operation, path, stopSequenceWithNoDurationNumber);
            }
          }
        }
      }

      return response;
    }


    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<DefaultSequenceTimeResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    bool IsNoStandardSequenceTimeMultiPath ()
    {
      var singlePath = Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.SinglePath));
      return !singlePath
        && !Lemoine.Info.ConfigSet.LoadAndGet<bool> (STANDARD_SEQUENCE_TIME_MULTI_PATH_KEY,
                                                     STANDARD_SEQUENCE_TIME_MULTI_PATH_DEFAULT);
    }

    TimeSpan? GetDefaultSequenceDuration (IOperation operation, IPath path, int numberOfSequences)
    {
      if (0 == numberOfSequences) {
        log.FatalFormat ("GetDefaultSequenceDuration: number of sequences is 0");
        Debug.Assert (0 < numberOfSequences);
        return null;
      }

      IEnumerable<ISequence> sequences;
      if (ModelDAOHelper.DAOFactory.IsInitialized (path.Sequences)) {
        sequences = path.Sequences;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Business.Operation.DefaultSequenceTime.GetDefaultSequenceDuration.Sequences")) {
            sequences = ModelDAOHelper.DAOFactory.SequenceDAO.FindAllWithPath (path);
          }
        }
      }

      // Get the total time of the sequence defined time
      TimeSpan sequenceTotalTime =
        TimeSpan.FromTicks (sequences
                            .Where (s => s.EstimatedTime.HasValue)
                            .Sum (s => s.EstimatedTime.Value.Ticks));
      // Remaining time for the operation
      TimeSpan remainingOperationTime = operation.MachiningDuration.Value
        .Subtract (sequenceTotalTime);
      if (0 == remainingOperationTime.Ticks) {
        log.Warn ($"GetDefaultSequenceDuration: remaining operation time is {remainingOperationTime} while number of sequences without duration is {numberOfSequences} => null will be returned");
      }
      if (TimeSpan.FromTicks (0) < remainingOperationTime) {
        return TimeSpan.FromTicks (remainingOperationTime.Ticks / numberOfSequences);
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Operation.DefaultSequenceTime." + ((IDataWithId<int>)Operation).Id
        + "." + ((IDataWithId<int>)Path).Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<DefaultSequenceTimeResponse> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (DefaultSequenceTimeResponse data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation

  }

  /// <summary>
  /// Response structure to the DefaultSequenceTime request
  /// </summary>
  public class DefaultSequenceTimeResponse
  {
    /// <summary>
    /// Default sequence time for a machining sequence
    /// </summary>
    public TimeSpan? MachiningSequenceDuration { get; set; }

    /// <summary>
    /// Default sequence time for a non-machining sequence
    /// </summary>
    public TimeSpan? NonMachiningSequenceDuration { get; set; }

    /// <summary>
    /// Default sequence time for a stop sequence
    /// </summary>
    public TimeSpan? StopSequenceDuration { get; set; }
  }
}
