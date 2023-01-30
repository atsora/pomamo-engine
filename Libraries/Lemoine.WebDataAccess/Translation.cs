// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// 
  /// </summary>
  public class Translation: Lemoine.Model.ITranslation
  {
    #region ITranslation implementation
    public string Locale {
      get; set;
    }
    public string TranslationKey {
      get; set;
    }
    public string TranslationValue {
      get; set;
    }
    #endregion
    #region IDataWithVersion implementation
    public int Version {
      get; set;
    }
    #endregion
    #region IDataWithId implementation
    public int Id {
      get; set;
    }
    #endregion
  }
}
