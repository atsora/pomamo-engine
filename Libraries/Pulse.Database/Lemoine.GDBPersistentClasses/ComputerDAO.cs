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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of ComputerDAO.
  /// </summary>
  public class ComputerDAO
    : VersionableNHibernateDAO<Computer, IComputer, int>
    , IComputerDAO
  {

    static readonly ILog log = LogManager.GetLogger (typeof (ComputerDAO).FullName);

    /// <summary>
    /// Get the Lctr computer
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IComputer GetLctr ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsLctr", true))
        .SetCacheable (true)
        .UniqueResult<Computer> ();
    }

    /// <summary>
    /// Get the Lctr computer asynchronously
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IComputer> GetLctrAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsLctr", true))
        .SetCacheable (true)
        .UniqueResultAsync<Computer> ();
    }

    /// <summary>
    /// Try to get the computer that matches the local computer in database
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns>null if not found or if it could not be determined</returns>
    public IComputer GetLocal ()
    {
      var computerNames = Lemoine.Info.ComputerInfo.GetNames ();
      IComputer result;
      try {
        result =
          NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<Computer> ()
          .Add (Restrictions.Disjunction ()
             .Add (Restrictions.Eq ("Name", System.Environment.MachineName))
             .Add (Restrictions.InG ("Address", computerNames))
             )
          .SetCacheable (true)
          .UniqueResult<Computer> ();
      }
      catch (NHibernate.HibernateException ex) {
        log.Error ("GetLocal: several computers match the local computer in database => return null", ex);
        return null;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetLocal: got Computer {result}");
      }
      return result;
    }

    /// <summary>
    /// Try to get the computer that matches the local computer in database.
    /// 
    /// If the local computer does not exist in the database, create it
    /// </summary>
    /// <returns></returns>
    public IComputer GetOrCreateLocal ()
    {
      IComputer computer = GetLocal ();
      if (null == computer) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetOrCreateLocal: add the local computer {System.Environment.MachineName} in database");
        }
        computer = ModelDAOHelper.ModelFactory.CreateComputer (System.Environment.MachineName,
                                                              System.Net.Dns.GetHostName ());

        NHibernateHelper.GetCurrentSession ().Save (computer);
      }
      Debug.Assert (null != computer);
      return computer;
    }

    /// <summary>
    ///  Get the Lposts computers
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    public IList<IComputer> GetLposts ()
    {
      IList<IComputer> lposts =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsLpst", true))
        .SetCacheable (true)
        .List<IComputer> ();

      return lposts;
    }

    /// <summary>
    ///  Get the Lposts computers asynchronously
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    public async System.Threading.Tasks.Task<IList<IComputer>> GetLpostsAsync ()
    {
      IList<IComputer> lposts =
        await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsLpst", true))
        .SetCacheable (true)
        .ListAsync<IComputer> ();

      return lposts;
    }

    /// <summary>
    ///  Get the cnc computers
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    public IList<IComputer> GetCnc ()
    {
      IList<IComputer> cncMachines =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsCnc", true))
        .SetCacheable (true)
        .List<IComputer> ();

      return cncMachines;
    }

    /// <summary>
    ///  Get the cnc computers asynchronously
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    public async System.Threading.Tasks.Task<IList<IComputer>> GetCncAsync ()
    {
      IList<IComputer> cncMachines =
        await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsCnc", true))
        .SetCacheable (true)
        .ListAsync<IComputer> ();

      return cncMachines;
    }

    /// <summary>
    ///  Get the web computers
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    public IList<IComputer> GetWeb ()
    {
      IList<IComputer> cncMachines =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsWeb", true))
        .SetCacheable (true)
        .List<IComputer> ();

      return cncMachines;
    }

    /// <summary>
    ///  Get the web computers
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    public async System.Threading.Tasks.Task<IList<IComputer>> GetWebAsync ()
    {
      IList<IComputer> cncMachines =
        await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsWeb", true))
        .SetCacheable (true)
        .ListAsync<IComputer> ();

      return cncMachines;
    }

    /// <summary>
    /// Get the auto-reason server
    /// </summary>
    /// <returns></returns>
    public IComputer GetAutoReason ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsAutoReason", true))
        .SetCacheable (true)
        .UniqueResult<Computer> ();
    }

    /// <summary>
    /// Get the auto-reason server
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IComputer> GetAutoReasonAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsAutoReason", true))
        .SetCacheable (true)
        .UniqueResultAsync<Computer> ();
    }

    /// <summary>
    /// Get the alert server
    /// </summary>
    /// <returns></returns>
    public IComputer GetAlert ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsAlert", true))
        .SetCacheable (true)
        .UniqueResult<Computer> ();
    }

    /// <summary>
    /// Get the alert server
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IComputer> GetAlertAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsAlert", true))
        .SetCacheable (true)
        .UniqueResultAsync<Computer> ();
    }

    /// <summary>
    /// Get the synchronization server
    /// </summary>
    /// <returns></returns>
    public IComputer GetSynchronization ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsSynchronization", true))
        .SetCacheable (true)
        .UniqueResult<Computer> ();
    }

    /// <summary>
    /// Get the synchronization server
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IComputer> GetSynchronizationAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Computer> ()
        .Add (Restrictions.Eq ("IsSynchronization", true))
        .SetCacheable (true)
        .UniqueResultAsync<Computer> ();
    }

  }
}
