// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IEmailConfigDAO">IEmailConfigDAO</see>
  /// </summary>
  public class EmailConfigDAO
    : VersionableNHibernateDAO<EmailConfig, IEmailConfig, int>
    , IEmailConfigDAO
  {
    /// <summary>
    /// FindAll implementation (cacheable)
    /// </summary>
    /// <returns></returns>
    public override IList<IEmailConfig> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EmailConfig> ()
        .SetCacheable (true)
        .List<IEmailConfig> ();
    }

    /// <summary>
    /// FindAll implementation (cacheable)
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IEmailConfig>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EmailConfig> ()
        .SetCacheable (true)
        .ListAsync<IEmailConfig> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IEmailConfigDAO.FindAllForConfig" />
    /// 
    /// Note: this is not registered to be cacheable because of the eager fetch
    /// </summary>
    /// <returns></returns>
    public IList<IEmailConfig> FindAllForConfig()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EmailConfig> ()
        .Fetch (SelectMode.Fetch, "EventLevel")
        .Fetch (SelectMode.Fetch, "Machine")
        .Fetch (SelectMode.Fetch, "Machine.MonitoringType")
        .Fetch (SelectMode.Fetch, "MachineFilter")
        .AddOrder (Order.Asc ("Name"))
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .List<IEmailConfig> ();
    }
    
    /// <summary>
    /// Find all active configurations (cacheable)
    /// </summary>
    /// <returns></returns>
    public virtual IList<IEmailConfig> FindActive ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EmailConfig> ()
        .Add (Restrictions.Eq ("Active", true))
        .SetCacheable (true)
        .List<IEmailConfig> ();
    }
    
    /// <summary>
    /// Find all configurations that match a specified data type and machine
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IList<IEmailConfig> FindByDataTypeMachine (string dataType, IMachine machine)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<EmailConfig> ()
        .Add (Restrictions.Eq ("DataType", dataType))
        .Add (Restrictions.Eq ("Machine", machine))
        .AddOrder (Order.Asc ("Name"))
        .List<IEmailConfig> ();
    }
  }
}
