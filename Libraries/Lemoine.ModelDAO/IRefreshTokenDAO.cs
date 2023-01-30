// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IIRefreshTokenDAO.
  /// </summary>
  public interface IRefreshTokenDAO : IGenericByUserUpdateDAO<IRefreshToken, int>
  {
    /// <summary>
    /// Find by user
    /// 
    /// Return the refresh token with the latest expiration date/time first
    /// </summary>
    /// <param name="user">not null</param>
    /// <returns></returns>
    IEnumerable<IRefreshToken> FindByUser (IUser user);

    /// <summary>
    /// Find by user asynchronously
    /// 
    /// Return the refresh token with the latest expiration date/time first
    /// </summary>
    /// <param name="user">not null</param>
    /// <returns></returns>
    Task<IEnumerable<IRefreshToken>> FindByUserAsync (IUser user);

    /// <summary>
    /// Find by user the refresh token that have not been revoked
    /// 
    /// Return the refresh token with the latest expiration date/time first
    /// </summary>
    /// <param name="user">not null</param>
    /// <returns></returns>
    IEnumerable<IRefreshToken> FindByUserNotRevoked (IUser user);

    /// <summary>
    /// Find by user asynchronously the refresh token that have not been revoked
    /// 
    /// Return the refresh token with the latest expiration date/time first
    /// </summary>
    /// <param name="user">not null</param>
    /// <returns></returns>
    Task<IEnumerable<IRefreshToken>> FindByUserNotRevokedAsync (IUser user);

    /// <summary>
    /// Find the refresh token that have been expired or revoked before a date in the past
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="age"></param>
    /// <returns></returns>
    IEnumerable<IRefreshToken> FindRevokedOrExpired (IUser user, TimeSpan age);

    /// <summary>
    /// Find the refresh token that have been expired or revoked before a date in the past
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="age"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IEnumerable<IRefreshToken>> FindRevokedOrExpiredAsync (IUser user, TimeSpan age);

    /// <summary>
    /// Return the unique token if valid
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="user">optional</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IRefreshToken> FindByTokenAsync (string refreshToken, IUser user = null);
  }

  /// <summary>
  /// Extensions to <see cref="IRefreshTokenDAO"/>
  /// </summary>
  public static class RefreshTokenDAOExtensions
  {
    static readonly string REFRESH_TOKEN_EXPIRES_IN_KEY = "RefreshToken.ExpiresIn";
    static readonly TimeSpan REFRESH_TOKEN_EXPIRES_IN_DEFAULT = TimeSpan.FromDays (1);

    static readonly string REFRESH_TOKEN_CLEAN_AGE_KEY = "RefreshToken.CleanAge";
    static readonly TimeSpan REFRESH_TOKEN_CLEAN_AGE_DEFAULT = TimeSpan.FromDays (2);

    /// <summary>
    /// Clean the refresh tokens
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="age"></param>
    /// <returns></returns>
    public static async System.Threading.Tasks.Task CleanAsync (this IRefreshTokenDAO refreshTokenDao, IUser user, TimeSpan age)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("RefreshToken.CleanAsync")) {
          var revokedOrExpiredTokens = await refreshTokenDao
            .FindRevokedOrExpiredAsync (user, age);
          foreach (var revokedOrExpiredToken in revokedOrExpiredTokens) {
            await refreshTokenDao
              .MakeTransientAsync (revokedOrExpiredToken);
          }
        }
      }
    }

    /// <summary>
    /// Clean the refresh tokens
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="age"></param>
    /// <returns></returns>
    public static void Clean (this IRefreshTokenDAO refreshTokenDao, IUser user)
    {
      var age = Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_TOKEN_CLEAN_AGE_KEY, REFRESH_TOKEN_CLEAN_AGE_DEFAULT);
      refreshTokenDao.Clean (user, age);
    }

    /// <summary>
    /// Clean the refresh tokens
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="age"></param>
    /// <returns></returns>
    public static void Clean (this IRefreshTokenDAO refreshTokenDao, IUser user, TimeSpan age)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("RefreshToken.CleanAsync")) {
          var revokedOrExpiredTokens = refreshTokenDao
            .FindRevokedOrExpired (user, age);
          foreach (var revokedOrExpiredToken in revokedOrExpiredTokens) {
            refreshTokenDao
              .MakeTransient (revokedOrExpiredToken);
          }
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// Clean the refresh tokens
    /// </summary>
    /// <param name="user">not null</param>
    /// <returns></returns>
    public static async Task CleanAsync (this IRefreshTokenDAO refreshTokenDao, IUser user)
    {
      var age = Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_TOKEN_CLEAN_AGE_KEY, REFRESH_TOKEN_CLEAN_AGE_DEFAULT);
      await refreshTokenDao.CleanAsync (user, age);
    }

    /// <summary>
    /// Get or create a refresh token for a specific user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static IRefreshToken GetRefreshToken (this IRefreshTokenDAO refreshTokenDao, IUser user)
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var refreshTokens = refreshTokenDao
          .FindByUserNotRevoked (user);
        var refreshToken = refreshTokens.FirstOrDefault (x => x.IsValid ());
        if (refreshToken is null) { // Create a new one
          var expiresIn = Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_TOKEN_EXPIRES_IN_KEY, REFRESH_TOKEN_EXPIRES_IN_DEFAULT);
          refreshToken = ModelDAO.ModelDAOHelper.ModelFactory.CreateRefreshToken (user, expiresIn);
          refreshTokenDao.MakePersistent (refreshToken);
        }
        return refreshToken;
      }
    }

    /// <summary>
    /// Get or create a refresh token for a specific user asynchronously
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="clean"></param>
    /// <returns></returns>
    public static async Task<IRefreshToken> GetRefreshTokenAsync (this IRefreshTokenDAO refreshTokenDao, IUser user, bool clean = false)
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        IEnumerable<IRefreshToken> refreshTokens;
        if (clean) {
          refreshTokens = await refreshTokenDao
            .FindByUserAsync (user);
        }
        else {
          refreshTokens = await refreshTokenDao
            .FindByUserNotRevokedAsync (user);
        }
        if (1 == refreshTokens.Count ()) {
          var uniqueRefreshToken = refreshTokens.Single ();
          if (uniqueRefreshToken.IsValid ()) {
            return uniqueRefreshToken;
          }
        }
        var refreshToken = refreshTokens.FirstOrDefault (x => x.IsValid ());
        if (refreshToken is null) { // Create a new one
          var expiresIn = Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_TOKEN_EXPIRES_IN_KEY, REFRESH_TOKEN_EXPIRES_IN_DEFAULT);
          using (var transaction = session.BeginTransaction ("RefreshToken.Create")) {
            refreshToken = ModelDAO.ModelDAOHelper.ModelFactory.CreateRefreshToken (user, expiresIn);
            await refreshTokenDao.MakePersistentAsync (refreshToken);
            transaction.Commit ();
          }
        }
        if (clean && refreshTokens.Any ()) {
          var notValidTokens = refreshTokens.Where (x => !x.IsValid ()).ToList ();
          var age = Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_TOKEN_CLEAN_AGE_KEY, REFRESH_TOKEN_CLEAN_AGE_DEFAULT);
          var before = DateTime.UtcNow.Subtract (age);
          foreach (var t in notValidTokens) {
            if (t.Revoked.HasValue && (t.Revoked.Value < before)) {
              using (var transaction = session.BeginTransaction ("RefreshToken.CleanRevoked")) {
                await ModelDAOHelper.DAOFactory.RefreshTokenDAO.MakeTransientAsync (t);
                transaction.Commit ();
              }
            }
            else if (t.Expiration < before) {
              using (var transaction = session.BeginTransaction ("RefreshToken.CleanExpired")) {
                await ModelDAOHelper.DAOFactory.RefreshTokenDAO.MakeTransientAsync (t);
                transaction.Commit ();
              }
            }
          }
        }
        return refreshToken;
      }
    }
  }
}
