// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICustomerDAODAO">ICustomerDAODAO</see>
  /// </summary>
  public class CustomerDAO
    : VersionableNHibernateDAO<Customer, ICustomer, int>
    , ICustomerDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CustomerDAO).FullName);

    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old customer into a new one
    /// 
    /// This returns the merged Customer
    /// </summary>
    /// <param name="oldCustomer"></param>
    /// <param name="newCustomer"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public ICustomer Merge (ICustomer oldCustomer,
                            ICustomer newCustomer,
                            ConflictResolution conflictResolution)
    {
      if (0 == ((Lemoine.Collections.IDataWithId)newCustomer).Id) { // newCustomer is not persistent, inverse the arguments
        Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)oldCustomer).Id);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newCustomer, oldCustomer,
                              localConflictResolution);
      }
      else {
        return InternalMerge (oldCustomer, newCustomer, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old Customer into a new one
    /// 
    /// This returns the merged Customer
    /// </summary>
    /// <param name="oldCustomer"></param>
    /// <param name="newCustomer">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    ICustomer InternalMerge (ICustomer oldCustomer,
                             ICustomer newCustomer,
                             ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)newCustomer).Id);

      LockForMerge (newCustomer);
      if (0 != ((Lemoine.Collections.IDataWithId)oldCustomer).Id) {
        LockForMerge (oldCustomer);
      }

      Debug.Assert (newCustomer is Customer);
      (newCustomer as Customer).Merge (oldCustomer as Customer, conflictResolution);

      if (0 != ((Lemoine.Collections.IDataWithId)oldCustomer).Id) { // Note: only for persistent classes, not transient ones
        // Merge the data in all the impacted tables
        // - Data

        // Modifications
        // There is no need to add some Modification
        // ProjectCustomerUpdate / CustomerIntermediateWorkUpdate /
        // StampUpdate rows,
        // because the data is automatically updated in the analysis tables above

        // Delete the old customer
        MakeTransient (oldCustomer);
      }

      return newCustomer;
    }

    /// <summary>
    /// Lock a customer for merge
    /// </summary>
    internal protected void LockForMerge (ICustomer customer)
    {
      UpgradeLock (customer);
    }
    #endregion // IMergeDAO implementation

    #region Methods
    /// <summary>
    /// Find Customer by Name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ICustomer FindByName (string name)
    {
      Debug.Assert (!string.IsNullOrEmpty (name));

      return NHibernateHelper.GetCurrentSession ().CreateCriteria<Customer> ()
        .Add (Restrictions.Eq ("Name", name))
        .UniqueResult<ICustomer> ();
    }

    /// <summary>
    /// Find Customer by Code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public ICustomer FindByCode (string code)
    {
      Debug.Assert (!string.IsNullOrEmpty (code));

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Customer> ()
        .Add (Restrictions.Eq ("Code", code))
        .UniqueResult<ICustomer> ();
    }

    /// <summary>
    /// Reload an entity (for example after an update operation fails or because it was changed somewhere else)
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ICustomer Reload (ICustomer entity)
    {
      ICustomer result = base.Reload (entity);
      return result;
    }

    /// <summary>
    /// Find customers whose names match a pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public IList<ICustomer> FindByNameStartPattern (string pattern)
    {
      return NHibernateHelper.GetCurrentSession ().CreateCriteria<Customer> ()
        .Add (Restrictions.Like ("Name", pattern, MatchMode.Start, null))
        .List<ICustomer> ();
    }
    #endregion // Methods
  }
}

