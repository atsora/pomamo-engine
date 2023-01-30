// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// Additional checker that returns false when the file repository was not available at start and is now available
  /// to force a restart of the service
  /// </summary>
  public interface IFileRepoChecker: Lemoine.Threading.IAdditionalChecker
  {
  }
}
