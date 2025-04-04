// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Lemoine.Core.ExceptionManagement;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web.CommonRequestDTO;

namespace Lemoine.Web
{
  /// <summary>
  /// GenericNoCachePostOnlyService
  /// </summary>
  public abstract class GenericPostOnlyNoCacheService<IPostRequestDTO, IPostDTO>
    : IHandler
    , IBodySupport
  {
    readonly ILog log = LogManager.GetLogger (typeof (GenericNoCacheService<IPostRequestDTO>).FullName);

    Stream m_body;

    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericPostOnlyNoCacheService ()
    {
    }

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Post (IPostRequestDTO request)
    {
      var postDto = PostDTO.Deserialize<IPostDTO> (m_body);
      if (null == postDto) {
        log.Error ("Post: post DTO is null");
        return new ErrorDTO ("Invalid Post data", ErrorStatus.WrongRequestParameter);
      }

      return await PostAsync (postDto);
    }

    /// <summary>
    /// To implement
    /// </summary>
    /// <param name="postDto"></param>
    /// <returns></returns>
    public abstract Task<object> PostAsync (IPostDTO postDto);

    #region IBodySupport
    /// <summary>
    /// <see cref="IBodySupport"/>
    /// </summary>
    /// <param name="body"></param>
    public void SetBody (Stream body)
    {
      m_body = body;
    }
    #endregion // IBodySupport
  }
}
