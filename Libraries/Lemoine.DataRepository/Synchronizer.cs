// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Given a Repository, synchronize automatically a <see cref="IBuilder" />
  /// from one or two <see cref="IFactory" />
  /// </summary>
  public sealed class Synchronizer : ThreadClass
  {
    #region Members
    Repository m_repository;
    TimeSpan m_every;
    TimeSpan m_noDataEvery;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Synchronizer).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated Repository
    /// </summary>
    public Repository Repository
    {
      get { return m_repository; }
    }

    /// <summary>
    /// The data is synchronized every the given duration
    /// in case some data was found previously
    /// </summary>
    public TimeSpan Every
    {
      get { return m_every; }
      set { m_every = value; }
    }

    /// <summary>
    /// The data is synchronized every the given duration
    /// in case no data was found previously
    /// </summary>
    public TimeSpan NoDataEvery
    {
      get { return m_noDataEvery; }
      set { m_noDataEvery = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="repository"></param>
    public Synchronizer (Repository repository)
    {
      this.m_repository = repository;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="ThreadClass"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected override void Run (CancellationToken cancellationToken)
    {
      while (!cancellationToken.IsCancellationRequested) {
        DateTime dateTime = DateTime.UtcNow;

        try {
          m_repository.UpdateAndSynchronize (cancellationToken);
        }
        catch (Exception ex) {
          log.Error ($"Run: UpdateAndSynchronize failed with error", ex);
        }

        TimeSpan every = this.Every;
        try {
          if (m_repository.IsEmpty (cancellationToken)) {
            every = this.NoDataEvery;
          }
        }
        catch (Exception ex) {
          log.Error ($"Run: IsEmpty failed with error ", ex);
        }

        TimeSpan duration = DateTime.UtcNow - dateTime;
        if (duration < every) {
          log.Debug ($"Run: about to sleep {every - duration}");
          this.Sleep (every - duration, cancellationToken);
        }
      }
      if (cancellationToken.IsCancellationRequested) {
        log.Error ($"Run: cancellation was requested");
      }
    }

    /// <summary>
    /// <see cref="ThreadClass"/>
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }
    #endregion // Methods
  }
}
