// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.ExceptionManagement;
using Lemoine.Core.Log;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of SettingsExceptionTest.
  /// </summary>
  public class SettingsExceptionTest: IExceptionTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SettingsExceptionTest).FullName);

    #region IExceptionTest implementation
    public bool RequiresExit(Exception ex, ILog logger)
    {
      return false;
    }
    public bool RequiresExitExceptFromDatabase (Exception ex, ILog logger)
    {
      return RequiresExit (ex, logger);
    }
    public bool IsStale(Exception ex, ILog logger)
    {
      if (ex is Lemoine.Settings.StaleException) {
        logger.Info ("IsStale: Lemoine.Settings.StaleException", ex);
        return true;
      }
      
      return false;
    }
    
    /// <summary>
    /// Does this exception relate to a right problem? Admin required?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsUnauthorized (Exception ex, ILog logger)
    {
      if (ex is AccessViolationException || ex is UnauthorizedAccessException) {
        return true;
      }

      return false;
    }
    
    public bool IsTemporary(Exception ex, ILog logger)
    {
      return false;
    }

    public bool IsTransactionSerializationFailure(Exception Ex, ILog logger)
    {
      return false;
    }

    public bool IsTransactionAborted (Exception Ex, ILog logger)
    {
      return false;
    }

    public bool IsTemporaryWithDelay(Exception ex, ILog logger)
    {
      return false;
    }


    public bool IsDatabaseConnectionError(Exception ex, ILog logger)
    {
      return false;
    }

    public bool IsTimeoutFailure(Exception ex, ILog logger)
    {
      return false;
    }
    public bool IsInvalid(Exception ex, ILog logger)
    {
      return false;
    }
    public bool IsIntegrityConstraintViolation(Exception ex, ILog logger)
    {
      return false;
    }
    public bool IsDatabaseException(Exception ex, ILog logger, out IDatabaseExceptionDetails details)
    {
      details = null;
      return false;
    }

    public bool IsNotError (Exception ex, ILog logger)
    {
      return false;
    }
    #endregion
  }
}
