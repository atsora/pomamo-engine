// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Lemoine.Business;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.Machine
{
  /// <summary>
  /// Extensions to <see cref="ClaimsPrincipal"/>
  /// </summary>
  public static class ClaimsPrincipalExtensions
  {
    /// <summary>
    /// Extract the user id string from ClaimsPrincipal
    /// 
    /// It will be empty if it is not set
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string GetUserIdString (this System.Security.Claims.ClaimsPrincipal user)
    {
      return user?.FindFirst (ClaimTypes.PrimarySid)?.Value ?? "";
    }
  }

  /// <summary>
  /// Response of the business service <see cref="AuthorizedMachines"/>
  /// </summary>
  public sealed class AuthorizedMachinesResponse
  {
    /// <summary>
    /// All the machines are authorized
    /// </summary>
    public bool All { get; private set; } = true;

    /// <summary>
    /// Is the returned data user specific ?
    /// </summary>
    public bool UserSpecific { get; set; } = true;

    /// <summary>
    /// Check the <see cref="All"/> property first since this property is only set when All is false
    /// 
    /// null if All is true
    /// </summary>
    public IEnumerable<IMachine> Machines { get; private set; } = null;

    /// <summary>
    /// Constructor when no filter applies
    /// </summary>
    internal AuthorizedMachinesResponse ()
    {
    }

    /// <summary>
    /// Constructor when the list of machines is retricted for the given login
    /// </summary>
    /// <param name="machines"></param>
    internal AuthorizedMachinesResponse (IEnumerable<IMachine> machines)
    {
      this.Machines = machines;
      this.All = false;
    }

    /// <summary>
    /// Is the machine in parameter authorized ?
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public bool IsAuthorized (IMachine machine)
    {
      if (this.All) {
        return true;
      }
      else {
        Debug.Assert (null != this.Machines);
        return this.Machines
          .Any (m => m.Id == machine.Id);
      }
    }
  }

  /// <summary>
  /// Request class to get the authorized machines for the authenticated user
  /// 
  /// TODO: extensions to extract a user for any kind of ClaimsPrincipal
  /// </summary>
  public sealed class AuthorizedMachines
    : IRequest<AuthorizedMachinesResponse>
  {
    static readonly string AUTHENTICATION_REQUIRED_KEY = "Authentication.Required";
    static readonly bool AUTHENTICATION_REQUIRED_DEFAULT = false;

    static readonly ILog log = LogManager.GetLogger (typeof (AuthorizedMachines).FullName);

    readonly System.Security.Claims.ClaimsPrincipal m_user;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="user"></param>
    public AuthorizedMachines (System.Security.Claims.ClaimsPrincipal user)
    {
      m_user = user;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public AuthorizedMachinesResponse Get ()
    {
      var authenticationRequired = Lemoine.Info.ConfigSet
        .LoadAndGet (AUTHENTICATION_REQUIRED_KEY, AUTHENTICATION_REQUIRED_DEFAULT);
      if (!authenticationRequired) {
        if (log.IsDebugEnabled) {
          log.Debug ("Get: authentication is not required => all machines are ok");
        }
        var result = new AuthorizedMachinesResponse ();
        result.UserSpecific = false;
        return result;
      }

      var userIdString = GetUserIdString ();
      if (string.IsNullOrEmpty (userIdString)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: no PrimarySid claim in user {m_user} => return no machine");
        }
        return new AuthorizedMachinesResponse (new IMachine[] { });
      }

      if (!int.TryParse (userIdString, out var userId)) {
        log.Error ($"Get: PrimarySid {userIdString} is not an integer => return no machine");
        return new AuthorizedMachinesResponse (new IMachine[] { });
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var user = ModelDAO.ModelDAOHelper.DAOFactory.UserDAO.FindById (userId);

        if (user is null) {
          // The user is not found
          log.Error ($"Get: user with id {userId} does not exist => return an empty list of machines");
          return new AuthorizedMachinesResponse (new IMachine[] { });
        }
        else { // The user is found, get the company for filtering the machines
          var company = user.Company;
          if (company is null) { // => no filter
            return new AuthorizedMachinesResponse ();
          }
          else {
            var machines = ModelDAO.ModelDAOHelper.DAOFactory.MachineDAO.FindAllInCompany (company.Id);
            return new AuthorizedMachinesResponse (machines);
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public System.Threading.Tasks.Task<AuthorizedMachinesResponse> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      var result = Get ();
      return Task.FromResult (result);
    }

    string GetUserIdString ()
    {
      return m_user?.GetUserIdString () ?? "";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Machine.AuthorizedMachines." + GetUserIdString ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (AuthorizedMachinesResponse data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<AuthorizedMachinesResponse> data)
    {
      if (string.IsNullOrEmpty (GetUserIdString ())) {
        return false;
      }

      if (null == data.Value) {
        return true;
      }
      else {
        return true;
      }
    }
    #endregion // IRequest implementation
  }

  /// <summary>
  /// Request class to get the authorized machines for a specific login
  /// </summary>
  public sealed class AuthorizedMachinesFromLogin
    : IRequest<AuthorizedMachinesResponse>
  {
    static readonly string AUTHENTICATION_REQUIRED_KEY = "Authentication.Required";
    static readonly bool AUTHENTICATION_REQUIRED_DEFAULT = false;

    static readonly ILog log = LogManager.GetLogger (typeof (AuthorizedMachinesFromLogin).FullName);

    readonly string m_login;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AuthorizedMachinesFromLogin (string login)
    {
      m_login = login;
    }
    #endregion // Constructors

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public AuthorizedMachinesResponse Get ()
    {
      var authenticationRequired = Lemoine.Info.ConfigSet
        .LoadAndGet (AUTHENTICATION_REQUIRED_KEY, AUTHENTICATION_REQUIRED_DEFAULT);
      if (!authenticationRequired) {
        if (log.IsDebugEnabled) {
          log.Debug ("Get: authentication is not required => all machines are ok");
        }
        var result = new AuthorizedMachinesResponse ();
        result.UserSpecific = false;
        return result;
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var user = ModelDAO.ModelDAOHelper.DAOFactory.UserDAO.FindByLogin (m_login);

        if (user is null) {
          // The user is not found
          log.Error ($"Get: user with login {m_login} does not exist => return an empty list of machines");
          return new AuthorizedMachinesResponse (new IMachine[] { });
        }
        else { // The user is found, get the company for filtering the machines
          var company = user.Company;
          if (company is null) { // => no filter
            return new AuthorizedMachinesResponse ();
          }
          else {
            var machines = ModelDAO.ModelDAOHelper.DAOFactory.MachineDAO.FindAllInCompany (company.Id);
            return new AuthorizedMachinesResponse (machines);
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<AuthorizedMachinesResponse> GetAsync ()
    {
      var authenticationRequired = Lemoine.Info.ConfigSet
        .LoadAndGet (AUTHENTICATION_REQUIRED_KEY, AUTHENTICATION_REQUIRED_DEFAULT);
      if (!authenticationRequired) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetAsync: authentication is not required => all machines are ok");
        }
        var result = new AuthorizedMachinesResponse ();
        result.UserSpecific = false;
        return result;
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var user = await ModelDAO.ModelDAOHelper.DAOFactory.UserDAO.FindByLoginAsync (m_login);

        if (user is null) {
          // The user is not found
          log.Error ($"GetAsync: user with login {m_login} does not exist => return an empty list of machines");
          return new AuthorizedMachinesResponse (new IMachine[] { });
        }
        else { // The user is found, get the company for filtering the machines
          var company = user.Company;
          if (company is null) { // => no filter
            return new AuthorizedMachinesResponse ();
          }
          else {
            var machines = ModelDAO.ModelDAOHelper.DAOFactory.MachineDAO.FindAllInCompany (company.Id);
            return new AuthorizedMachinesResponse (machines);
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Machine.AuthorizedMachinesFromLogin." + m_login;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (AuthorizedMachinesResponse data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<AuthorizedMachinesResponse> data)
    {
      if (string.IsNullOrEmpty (m_login)) {
        return false;
      }

      if (null == data.Value) {
        return true;
      }
      else {
        return true;
      }
    }
    #endregion // IRequest implementation
  }

}
