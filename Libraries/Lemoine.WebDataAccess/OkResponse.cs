// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Class that corresponds to OkDTO
  /// </summary>
  internal class OkResponse
  {
    public int Id { get; set; }
    public int Version { get; set; }
    public string Message { get; set; }
  }
}
