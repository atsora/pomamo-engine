// Copyright (c) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.NHibernateTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.NHibernateTypes
{
  /// <summary>
  /// JsonAsStringStringMapType
  /// </summary>
  [Serializable]
  public class JsonAsStringStringMapType: JsonAsT<IDictionary<string, string>>
  {
  }
}
