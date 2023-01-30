// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.Implementation;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.CustomAction
{
  /// <summary>
  /// Custom action with no config
  /// </summary>
  public interface ICustomActionNoConfig: ICustomAction<EmptyConfiguration>
  {
  }
}
