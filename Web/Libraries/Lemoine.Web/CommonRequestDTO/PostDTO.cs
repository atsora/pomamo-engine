// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.IO;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Web.CommonRequestDTO
{
  /// <summary>
  /// Description of PostDTO.
  /// </summary>
  public abstract class PostDTO
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PostDTO).FullName);

    #region Constructors
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Deserialize
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static T Deserialize<T> (Stream stream)
    {
      if (stream.CanSeek) {
        stream.Seek (0, SeekOrigin.Begin);
      }

      return System.Text.Json.JsonSerializer.Deserialize<T> (stream);
    }

    /// <summary>
    /// Deserialize
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<T> DeserializeAsync<T> (Stream stream)
    {
      if (stream.CanSeek) {
        stream.Seek (0, SeekOrigin.Begin);
      }

      return await System.Text.Json.JsonSerializer.DeserializeAsync<T> (stream);
    }
    #endregion // Methods
  }
}
