// Copyright (c) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.NHibernateTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// JsonAsStringObjectMapType
  /// </summary>
  [Serializable]
  public class JsonAsStringJsonElementMapType : JsonAsT<IDictionary<string, JsonElement>>
  {
  }
}
