// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ILineMachineDAO">ILineMachineDAO</see>
  /// </summary>
  public class LineMachineDAO
    : VersionableNHibernateDAO<LineMachine, ILineMachine, int>
    , ILineMachineDAO
  {
    /// <summary>
    /// Find all LineMachine for a specific machine
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IList<ILineMachine> FindAllByMachine(IMachine machine)
    {
      Debug.Assert (null != machine);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<LineMachine> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .SetCacheable (true)
        .List<ILineMachine> ();
    }
    
    /// <summary>
    /// Find all LineMachine for a specific line
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="line">not null</param>
    /// <returns></returns>
    public IList<ILineMachine> FindAllByLine(ILine line)
    {
      Debug.Assert (null != line);

      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<LineMachine> ()
        .Add (Restrictions.Eq ("Line.Id", line.Id))
        .SetCacheable (true)
        .List<ILineMachine> ();
    }

    /// <summary>
    /// Find all LineMachine for a specific line and a specific machine
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IList<ILineMachine> FindAllByLineMachine (ILine line, IMachine machine)
    {
      Debug.Assert (null != line);
      Debug.Assert (null != machine);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<LineMachine> ()
        .Add (Restrictions.Eq ("Line.Id", line.Id))
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .SetCacheable (true)
        .List<ILineMachine> ();
    }

    /// <summary>
    /// Find all LineMachine for a specific line and a specific machine
    /// with an eager fetch of the operation
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IList<ILineMachine> FindAllByLineMachineWithOperation (ILine line, IMachine machine)
    {
      Debug.Assert (null != line);
      Debug.Assert (null != machine);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<LineMachine> ()
        .Add (Restrictions.Eq ("Line", line))
        .Add (Restrictions.Eq ("Machine", machine))
        .Fetch (SelectMode.Fetch, "Operation")
        .List<ILineMachine> ();
    }

    /// <summary>
    /// Find all LineMachine for a specific machine and operation
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public IList<ILineMachine> FindAllByMachineOperation (IMachine machine, IOperation operation)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy operation (which causes some problems)
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<LineMachine> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id))
        .SetCacheable (true)
        .List<ILineMachine> ();
    }

    /// <summary>
    /// Find all LineMachine for a specific operation
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public IList<ILineMachine> FindAllByOperation (IOperation operation)
    {
      Debug.Assert (null != operation);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<LineMachine> ()
        .Add (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id))
        .SetCacheable (true)
        .List<ILineMachine> ();
    }
    
    /// <summary>
    /// Find all LineMachine for a specific line and operation
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public IList<ILineMachine> FindAllByLineOperation (ILine line, IOperation operation)
    {
      Debug.Assert (null != operation);
      Debug.Assert (null != line);
      
      // In the request below, Id is used to get profit of the query cache
      // without having to unproxy some entities

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<LineMachine> ()
        .Add (Restrictions.Eq ("Line.Id", line.Id))
        .Add (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id))
        .SetCacheable (true)
        .List<ILineMachine> ();
    }
  }
}
