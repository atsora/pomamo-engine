// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Model;

namespace Lemoine.Model
{
  /// <summary>
  /// Analysis tables with a template to process
  /// </summary>
  public interface IWithTemplate: IWithRange
  {
    /// <summary>
    /// Process the template
    /// 
    /// applicableRange must overlap the date/time range of the slot
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="applicableRange"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="maxAnalysisDateTime">return false if not completed at maxAnalysisDateTime</param>
    /// <returns>true if completed, else false</returns>
    bool ProcessTemplate (CancellationToken cancellationToken,
      UtcDateTimeRange applicableRange,
                          IModification mainModification,
                          bool partOfDetectionAnalysis,
                          Lemoine.Threading.IChecked checkedThread,
                          DateTime? maxAnalysisDateTime);
  }
}
