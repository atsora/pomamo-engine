// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Info.ConfigReader;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Interface to implement for the OS specific config reader
  /// </summary>
  public interface IOsConfigReader: IGenericConfigReader, IPersistentConfigWriter
  {
  }
}
