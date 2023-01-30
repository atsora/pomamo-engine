// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Diagnostics;
using NHibernate.Criterion;
using System.Collections.Generic;
using NHibernate;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IReasonProposalDAO">IReasonProposalDAO</see>
  /// </summary>
  public class ReasonProposalDAO
    : VersionableByMachineNHibernateDAO<ReasonProposal, IReasonProposal, int>
    , IReasonProposalDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonProposalDAO ()
      : base ("Machine")
    {
    }

    /// <summary>
    /// Insert a new row directly in the database
    /// </summary>
    /// <param name="association"></param>
    /// <param name="range"></param>
    /// <returns>new id</returns>
    public int Insert (IReasonMachineAssociation association, UtcDateTimeRange range)
    {
      Debug.Assert (null != association);
      Debug.Assert (null != association.Machine);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        ModelDAOHelper.DAOFactory.Flush ();

        string partitionExistsRequest = string.Format (@"
SELECT EXISTS (
SELECT 1 FROM information_schema.tables
WHERE table_schema='pgfkpart'
  AND table_name='reasonproposal_p{0}'
);
", association.Machine.Id);
        bool partitionExists = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (partitionExistsRequest)
          .AddScalar ("exists", NHibernate.NHibernateUtil.Boolean)
          .UniqueResult<bool> ();
        if (!partitionExists) {
          var reasonProposal = ModelDAOHelper.ModelFactory.CreateReasonProposal (association, range);
          ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakePersistent (reasonProposal);
          return reasonProposal.Id;
        }

        var insertQuery = $@"
INSERT INTO pgfkpart.reasonproposal_p{association.Machine.Id}
  (machineid, modificationid, reasonproposaldatetimerange, reasonid, reasonproposalscore, reasonproposalkind, reasonproposaldetails)
VALUES ({association.Machine.Id}, :ModificationId, :Range, :Reason, :ReasonScore, :Kind, :Details)
RETURNING reasonproposalid;
";
        var id = NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (insertQuery)
          .AddScalar ("reasonproposalid", NHibernate.NHibernateUtil.Int32)
          .SetInt64 ("ModificationId", ((IDataWithId<long>)association).Id)
          .SetParameter ("Range", range, (NHibernate.Type.IType)new Lemoine.NHibernateTypes.UTCDateTimeRangeType ())
          .SetParameter ("Reason", association.Reason, NHibernateUtil.Entity (typeof (Reason)))
          .SetDouble ("ReasonScore", association.ReasonScore)
          .SetInt32 ("Kind", (int)association.Kind)
          .SetString ("Details", association.ReasonDetails)
          .UniqueResult<int> ();
        return id;
      }
    }

    /// <summary>
    /// Get the item that match the specified reason machine association
    /// </summary>
    /// <param name="reasonMachineAssociation"></param>
    /// <returns></returns>
    public IReasonProposal Get (IReasonMachineAssociation reasonMachineAssociation)
    {
      Debug.Assert (null != reasonMachineAssociation);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonProposal> ()
        .Add (Restrictions.Eq ("Machine.Id", reasonMachineAssociation.Machine.Id))
        .Add (Restrictions.Eq ("ModificationId", ((IDataWithId<long>)reasonMachineAssociation).Id))
        .SetCacheable (true)
        .UniqueResult<IReasonProposal> ();
    }

    /// <summary>
    /// <see cref="IReasonProposalDAO"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IList<IReasonProposal> FindAt (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonProposal> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
        // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), dateTime, "@>"))
        .AddOrder (Order.Asc ("DateTimeRange")) // Ok because they are not so many
                                                // Not cacheable because it may cause some problems with a partitioned table:
                                                // there is a risk to make NHibernate use a FindById without the secondary key
        .List<IReasonProposal> ();
    }

    /// <summary>
    /// <see cref="IReasonProposalDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonProposal> FindOverlapsRange (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonProposal> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IReasonProposal> ();
    }


    /// <summary>
    /// <see cref="IReasonProposalDAO"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonProposal> FindManualOverlapsRange (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonProposal> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Kind", ReasonProposalKind.Manual))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IReasonProposal> ();
    }

    /// <summary>
    /// <see cref="IReasonProposalDAO"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IReasonProposal FindManualAt (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonProposal> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
        // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), dateTime, "@>"))
        .Add (Restrictions.Eq ("Kind", ReasonProposalKind.Manual))
        .UniqueResult<IReasonProposal> ();
    }

    AbstractCriterion OverlapRange (UtcDateTimeRange range)
    {
      return new SimpleExpression ("DateTimeRange", range, "&&");
    }
  }
}
