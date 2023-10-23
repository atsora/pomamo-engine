// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using Lemoine.Cnc.Data;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// Description of IImportData.
  /// </summary>
  public interface IImportData
  {
    /// <summary>
    /// Last datetime when the method "ImportDatas" has been visited
    /// (automatically set by ImportCncValueFromQueue)
    /// </summary>
    DateTime LastVisitDateTime { get; set; }
    
    /// <summary>
    /// Return true if otherData can be merged with data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="otherData"></param>
    /// <returns></returns>
    bool IsMergeable(ExchangeData data, ExchangeData otherData);
    
    /// <summary>
    /// Import data that has been previously merged
    /// </summary>
    /// <param name="data"></param>
    void ImportDatas(IList<ExchangeData> data, CancellationToken cancellationToken = default);
  }
}
