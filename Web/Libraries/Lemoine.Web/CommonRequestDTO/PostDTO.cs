// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.IO;
using Lemoine.Core.Log;
using Newtonsoft.Json;

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
      stream.Seek (0, SeekOrigin.Begin);
      string json;
      using (StreamReader sr = new StreamReader (stream)) {
        json = sr.ReadToEnd ();
      }
      T deserializedResult = JsonConvert.DeserializeObject<T> (json);
      return deserializedResult;
    }
    #endregion // Methods
  }
}
