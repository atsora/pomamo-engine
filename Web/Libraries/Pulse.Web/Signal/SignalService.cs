// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business.Operation;
using Lemoine.Collections;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;
using Lemoine.WebMiddleware.HttpContext;
using Microsoft.AspNetCore.Http;
using Pulse.Extensions.Web;
using Pulse.Web.CommonResponseDTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pulse.Web.Signal
{
  /// <summary>
  /// Cycle progress service
  /// </summary>
  public class SignalService
    : GenericCachedService<SignalRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SignalService).FullName);

    /// <summary>
    /// Request date/time for the unit tests.
    /// 
    /// Default: DateTime.UtcNow
    /// </summary>
    internal DateTime? RequestDateTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public SignalService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    async Task<IEnumerable<ISignalExtension>> GetExtensionsAsync ()
    {
      return await Lemoine.Business.ServiceProvider
        .GetAsync (new Lemoine.Business.Extension
        .GlobalExtensions<ISignalExtension> ((ext) => ext.Initialize ()));
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async override Task<object> Get (SignalRequestDTO request) => await GetByGroupAsync (request);

    async Task<object> GetByGroupAsync (SignalRequestDTO request)
    {
      var responseDto = new SignalResponseDTO ();

      var extensions = await GetExtensionsAsync ();
      if (!extensions.Any ()) {
        log.Debug ("GetByGroupAsync: no signal extension");
        return responseDto;
      }

      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = await Lemoine.Business.ServiceProvider
        .GetAsync (groupRequest);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetByGroupAsync: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var roleKey = request.RoleKey;
        IRole role = null;
        if (!string.IsNullOrEmpty (roleKey)) {
          role = await ModelDAOHelper.DAOFactory.RoleDAO
            .FindByKeyAsync (roleKey);
          if (role is null) {
            log.Error ($"GetByGroupAsync: unknown role with ID {roleKey}");
            return new ErrorDTO ("No role with the specified key",
                                 ErrorStatus.WrongRequestParameter);
          }
        }
        foreach (var extension in extensions) {
          var signals = extension.GetSignals (group, role);
          foreach (var signal in signals) {
            var bgColor = signal.Color;
            var fgColor = ColorGenerator.GetContrastColor (bgColor);
            responseDto.AddMessage (signal.Message, bgColor, fgColor);
          }
        }
      }

      return responseDto;
    }
  }
}
