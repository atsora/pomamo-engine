// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Net.Mail;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.Misc
{
  /// <summary>
  /// SMTP Error DTO
  /// </summary>
  public class SmtpErrorDTO: ErrorDTO
  {
    /// <summary>
    /// SMTP status code
    /// </summary>
    public string SmtpStatusCode { get; set; }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="errorStatus"></param>
    /// <param name="smtpStatusCode"></param>
    SmtpErrorDTO (string message, ErrorStatus errorStatus, SmtpStatusCode smtpStatusCode)
      : base (message, errorStatus)
    {
      this.SmtpStatusCode = smtpStatusCode.ToString ();
    }
    
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ex"></param>
    public SmtpErrorDTO (SmtpFailedRecipientsException ex)
      : this (string.Format ("The e-mail could not be sent to {0}",
                             ex.FailedRecipient),
              ErrorStatus.WrongRequestParameter,
              ex.StatusCode)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ex"></param>
    public SmtpErrorDTO (SmtpException ex)
      : this ("Smtp error " + ex.Message,
              ErrorStatus.MissingConfiguration,
              ex.StatusCode)
    {
      // Probably a wrong configuration, although the reason may be something else
    }

  }
}
