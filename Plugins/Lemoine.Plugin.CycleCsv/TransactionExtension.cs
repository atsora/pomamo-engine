// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.CycleCsv
{
  /// <summary>
  /// Description of TransactionExtension.
  /// </summary>
  public class TransactionExtension
    : Lemoine.Extensions.Database.Impl.TransactionNotifier.TransactionExtension
    , Lemoine.Extensions.Database.ITransactionExtension
  {
  }
}
