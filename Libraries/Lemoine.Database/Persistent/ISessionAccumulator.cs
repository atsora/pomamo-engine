// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Interface for a session accumulator
  /// </summary>
  public interface ISessionAccumulator
  {
    /// <summary>
    /// Clear the accumulators and messages that are associated to the specified session
    /// </summary>
    /// <param name="session">not null</param>
    /// <returns></returns>
    void Clear (ISession session);

    /// <summary>
    /// Check if there is no accumulator
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    bool IsEmpty (ISession session);

    /// <summary>
    /// Store the content of all the accumulators into the database
    /// </summary>
    /// <param name="session"></param>
    /// <param name="transactionName"></param>
    void Store (ISession session, string transactionName);

    /// <summary>
    /// Send the messages
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    void SendMessages (ISession session);

    /// <summary>
    /// Send the messages asynchronously
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    Task SendMessagesAsync (ISession session);
  }
}
