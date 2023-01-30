// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEvent.
  /// </summary>
  public interface IEventDAO
  {
    /// <summary>
    /// Find an item T by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IEvent FindById (int id);

    /// <summary>
    /// Find all the entities
    /// </summary>
    /// <returns></returns>
    IEnumerable<IEvent> FindAll ();    
    
    /// <summary>
    /// Find the IEvent items which id is strictly greater than the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IEnumerable<IEvent> FindGreaterThan (int id);

    /// <summary>
    /// Find the IEvent items which id is strictly greater than the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maxResults">maximum number of items to return</param>
    /// <returns></returns>
    IEnumerable<IEvent> FindGreaterThan (int id, int maxResults);
    
    /// <summary>
    /// Get all the available types (key, text)
    /// </summary>
    /// <returns></returns>
    IDictionary<string, string> GetAvailableTypes ();
  }
}
