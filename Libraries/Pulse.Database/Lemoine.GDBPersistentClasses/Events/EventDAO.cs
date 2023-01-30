// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IEventDAO">IEventDAO</see>
  /// </summary>
  public class EventDAO
    : IEventDAO
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IEvent FindById(int id)
    {
      var eventAll = NHibernateHelper.GetCurrentSession ()
        .Get<EventAll> (id);
      if (null != eventAll) {
        eventAll.Unproxy ();
        return eventAll.Event;
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IEvent> FindAll()
    {
      var events = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventAll> ()
        .List<EventAll> ();
      foreach (var eventAll in events) {
        eventAll.Unproxy ();
      }
      return GetInstances (events);
    }

    /// <summary>
    /// Find the IEvent items which id is strictly greater than the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IEnumerable<IEvent> FindGreaterThan (int id)
    {
      var events = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventAll> ()
        .Add (Restrictions.Gt ("Id", id))
        .AddOrder (Order.Asc ("Id"))
        .List<EventAll> ();
      return GetInstances (events);
    }

    /// <summary>
    /// Find the IEvent items which id is strictly greater than the specified id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="maxResults">maximum number of items to return</param>
    /// <returns></returns>
    public IEnumerable<IEvent> FindGreaterThan (int id, int maxResults)
    {
      var events = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EventAll> ()
        .Add (Restrictions.Gt ("Id", id))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (maxResults)
        .List<EventAll> ();
      return GetInstances (events);
    }
    
    IEnumerable<IEvent> GetInstances (IEnumerable<EventAll> events)
    {
      foreach (var ev in events) {
        ev.Unproxy ();
      }
      return events.Select (ev => ev.Event);
    }
    
    /// <summary>
    /// Implementation of IEventDAO
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, string> GetAvailableTypes ()
    {
      var result = new Dictionary<string, string> ();
      result ["EventLongPeriod"] = "Long period";
      result ["EventCncValue"] = "Cnc value";
      result ["EventToolLife"] = "Tool life";
      result ["EventMessage"] = "Message";
      result ["EventMachineMessage"] = "Machine message";

      var request = new Lemoine.Business.Extension
        .GlobalExtensions<Pulse.Extensions.Database.IEventExtension> ();
      var extensions = Lemoine.Business.ServiceProvider
        .Get (request);
      foreach (var extension in extensions) {
        result.Add (extension.Type, extension.TypeText);
      }
      
      return result;
    }
  }
}
