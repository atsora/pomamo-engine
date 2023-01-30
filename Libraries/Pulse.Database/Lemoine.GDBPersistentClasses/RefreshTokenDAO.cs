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
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IRefreshTokenDAODAO">IRefreshTokenDAODAO</see>
  /// </summary>
  public class RefreshTokenDAO
    : GenericByUserNHibernateDAO<RefreshToken, IRefreshToken, int>
    , IRefreshTokenDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public RefreshTokenDAO ()
      : base ()
    {
    }

    /// <summary>
    /// <see cref="IRefreshTokenDAO"/>
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public IEnumerable<IRefreshToken> FindByUser (IUser user)
    {
      Debug.Assert (null != user);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<RefreshToken> ()
        .Add (Restrictions.Eq ("User.Id", user.Id))
        .AddOrder (Order.Desc ("Expiration"))
        .List<IRefreshToken> ();
    }

    /// <summary>
    /// <see cref="IRefreshTokenDAO"/>
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IEnumerable<IRefreshToken>> FindByUserAsync (IUser user)
    {
      Debug.Assert (null != user);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<RefreshToken> ()
        .Add (Restrictions.Eq ("User.Id", user.Id))
        .AddOrder (Order.Desc ("Expiration"))
        .ListAsync<IRefreshToken> ();
    }

    /// <summary>
    /// <see cref="IRefreshTokenDAO"/>
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public IEnumerable<IRefreshToken> FindByUserNotRevoked (IUser user)
    {
      Debug.Assert (null != user);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<RefreshToken> ()
        .Add (Restrictions.Eq ("User.Id", user.Id))
        .Add (Restrictions.IsNull ("Revoked"))
        .AddOrder (Order.Desc ("Expiration"))
        .List<IRefreshToken> ();
    }

    /// <summary>
    /// <see cref="IRefreshTokenDAO"/>
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IEnumerable<IRefreshToken>> FindByUserNotRevokedAsync (IUser user)
    {
      Debug.Assert (null != user);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<RefreshToken> ()
        .Add (Restrictions.Eq ("User.Id", user.Id))
        .Add (Restrictions.IsNull ("Revoked"))
        .AddOrder (Order.Desc ("Expiration"))
        .ListAsync<IRefreshToken> ();
    }

    public IEnumerable<IRefreshToken> FindRevokedOrExpired (IUser user, TimeSpan age)
    {
      Debug.Assert (null != user);

      var before = DateTime.UtcNow.Subtract (age);
      var revokedOrExpired = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<RefreshToken> ()
        .Add (Restrictions.Eq ("User.Id", user.Id))
        .Add (Restrictions.Disjunction ()
          .Add (Restrictions.Lt ("Expiration", before))
          .Add (Restrictions.Lt ("Revoked", before)))
        .List<IRefreshToken> ();
      return revokedOrExpired;
    }

    public async System.Threading.Tasks.Task<IEnumerable<IRefreshToken>> FindRevokedOrExpiredAsync (IUser user, TimeSpan age)
    {
      Debug.Assert (null != user);

      var before = DateTime.UtcNow.Subtract (age);
      var revokedOrExpired = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<RefreshToken> ()
        .Add (Restrictions.Eq ("User.Id", user.Id))
        .Add (Restrictions.Disjunction ()
          .Add (Restrictions.Lt ("Expiration", before))
          .Add (Restrictions.Lt ("Revoked", before)))
        .ListAsync<IRefreshToken> ();
      return revokedOrExpired;
    }

    /// <summary>
    /// Find by refresh token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="user">optional</param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IRefreshToken> FindByTokenAsync (string refreshToken, IUser user = null)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<RefreshToken> ()
        .Add (Restrictions.Eq ("Token", refreshToken))
        .Add (IsValid ());
      if (null != user) {
        criteria = criteria
          .Add (Restrictions.Eq ("User.Id", user.Id));
      }
      return await criteria.UniqueResultAsync<IRefreshToken> ();
    }

    ICriterion IsValid ()
    {
      var now = DateTime.UtcNow;
      return Restrictions.Conjunction ()
        .Add (Restrictions.Gt ("Expiration", now))
        .Add (Restrictions.IsNull ("Revoked"));
    }
  }
}
