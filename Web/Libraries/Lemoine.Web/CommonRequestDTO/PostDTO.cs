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

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    #endregion // Constructors

    #region Methods
#if NSERVICEKIT
    /// <summary>
    /// Deserialize
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    public static T Deserialize<T> (NServiceKit.ServiceHost.IHttpRequest httpRequest)
    {
      string json;
      using (StreamReader sr = new StreamReader(httpRequest.InputStream))
      {
        json = sr.ReadToEnd();
      }
      T deserializedResult = JsonConvert.DeserializeObject<T> (json);
      return deserializedResult;
    }
#endif // NSERVICEKIT

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
