// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get the sequence standard time
  /// </summary>
  public sealed class SequenceStandardTime
    : IRequest<TimeSpan?>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SequenceStandardTime).FullName);

    /// <summary>
    /// Sequence in the request
    /// </summary>
    ISequence Sequence { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sequence"></param>
    public SequenceStandardTime (ISequence sequence)
    {
      Debug.Assert (null != sequence);

      this.Sequence = sequence;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>TimeSpan?</returns>
    public TimeSpan? Get ()
    {
      ISequence sequence = this.Sequence;

      if (!ModelDAOHelper.DAOFactory.IsInitialized (sequence)) {
        // sequence is lazy => get a new instance here of the object
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Business.SequenceStandardTime")) {
            sequence = ModelDAOHelper.DAOFactory.SequenceDAO.FindById (((IDataWithId)sequence).Id);
          }
        }
      }

      if (sequence.EstimatedTime.HasValue) {
        return sequence.EstimatedTime.Value;
      }
      else {
        var defaultSequenceTime = ServiceProvider
          .Get (new DefaultSequenceTime (sequence.Operation,
                                         sequence.Path));
        switch (sequence.Kind) {
        case SequenceKind.Machining: {
          log.Debug ($"Get: use a default machining sequence time {defaultSequenceTime.MachiningSequenceDuration}");
          return defaultSequenceTime.MachiningSequenceDuration;
        }
        case SequenceKind.NonMachining: {
          log.Debug ($"Get: use a default non machining sequence time {defaultSequenceTime.NonMachiningSequenceDuration}");
          return defaultSequenceTime.NonMachiningSequenceDuration;
        }
        case SequenceKind.Stop: {
          log.Debug ($"Get: use a default stop sequence time {defaultSequenceTime.StopSequenceDuration}");
          return defaultSequenceTime.StopSequenceDuration;
        }
        default: {
          log.Debug ($"Get: no default sequence time for kind {sequence.Kind}");
          return null;
        }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<TimeSpan?> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Operation.SequenceStandardTime." + ((IDataWithId<int>)Sequence).Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<TimeSpan?> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (TimeSpan? data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
