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
  /// Description of ServiceDAO.
  /// </summary>
  public class ServiceDAO
    : VersionableNHibernateDAO<Service, IService, int>
    , IServiceDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ServiceDAO).FullName);

    /// <summary>
    /// Try to get a local service from its name
    /// 
    /// Return null if it does not exist in database
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="name"></param>
    /// <returns>null if the service does not exist in database</returns>
    public IService GetLocalServiceByName (string name)
    {
      IComputer localComputer = ModelDAOHelper.DAOFactory.ComputerDAO.GetLocal ();

      if (null == localComputer) {
        log.Debug ("GetLocalServiceByName: localComputer is not in database => return null");
        return null;
      }
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Service> ()
        .Add (Restrictions.Eq ("Computer", localComputer))
        .Add (Restrictions.Eq ("Name", name))
        .SetCacheable (true)
        .UniqueResult<Service> ();
    }

    /// <summary>
    /// Try to get a local service from its program name
    /// 
    /// Return null if it does not exist in database
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="program"></param>
    /// <returns>null if the service does not exist in database</returns>
    public IService GetLocalServiceByProgram (string program)
    {
      IComputer localComputer = ModelDAOHelper.DAOFactory.ComputerDAO.GetLocal ();
      if (null == localComputer) {
        log.Debug ("GetLocalServiceByName: localComputer is not in database => return null");
        return null;
      }
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Service> ()
        .Add (Restrictions.Eq ("Computer", localComputer))
        .Add (Restrictions.Eq ("Program", program))
        .SetCacheable (true)
        .UniqueResult<Service> ();
    }

    /// <summary>
    /// Try to get a local service from its name and program name
    /// 
    /// If the service does not exist in database, create it
    /// 
    /// name and program can't be null
    /// </summary>
    /// <param name="name"></param>
    /// <param name="program"></param>
    /// <param name="lemoine">Is the service a Lemoine service ?</param>
    /// <returns></returns>
    public IService GetOrCreateLocalService (string name,
                                             string program,
                                             bool lemoine)
    {
      if ((null == name) || (null == program)) {
        log.Error ("GetOrCreateLocalService: name or program are null");
        throw new ArgumentException ();
      }

      IComputer localComputer = ModelDAOHelper.DAOFactory.ComputerDAO.GetOrCreateLocal ();

      Debug.Assert (null != localComputer);
      IService service =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Service> ()
        .Add (Restrictions.Eq ("Lemoine", lemoine))
        .Add (Restrictions.Eq ("Name", name))
        .Add (Restrictions.Eq ("Program", program))
        .UniqueResult<Service> ();

      if (null == service) { // create it
        log.Debug ($"GetOrCreateLocalService: service name={name} unknown, create it");

        service = new Service ();
        service.Computer = localComputer;
        service.Name = name;
        service.Program = program;
        service.Lemoine = lemoine;
        NHibernateHelper.GetCurrentSession ().Save (service);
      }

      return service;
    }
  }
}
