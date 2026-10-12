// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lemoine.Cnc.Data;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// Several imports of the same data, in order, for example in cncdataperiod and in cncvalue
  ///
  /// The data are merged only if all the imports accept it.
  /// If an import fails, the data are imported again by all the imports:
  /// each import must skip the data it already imported
  /// </summary>
  internal sealed class ImportDataComposite : IImportData
  {
    readonly IList<IImportData> m_importData;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="importData">not empty</param>
    public ImportDataComposite (params IImportData[] importData)
    {
      m_importData = importData;
    }

    /// <summary>
    /// <see cref="IImportData"/>
    /// </summary>
    public DateTime LastVisitDateTime
    {
      get => m_importData[0].LastVisitDateTime;
      set {
        foreach (var importData in m_importData) {
          importData.LastVisitDateTime = value;
        }
      }
    }

    /// <summary>
    /// <see cref="IImportData"/>
    /// </summary>
    public bool IsMergeable (ExchangeData data, ExchangeData otherData) =>
      m_importData.All (i => i.IsMergeable (data, otherData));

    /// <summary>
    /// <see cref="IImportData"/>
    /// </summary>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      foreach (var importData in m_importData) {
        cancellationToken.ThrowIfCancellationRequested ();
        importData.ImportDatas (datas, cancellationToken);
      }
    }
  }
}
