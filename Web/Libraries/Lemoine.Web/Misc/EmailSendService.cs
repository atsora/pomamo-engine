// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;
using Lemoine.Model;

using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Net;
using MimeKit;
using MailKit.Net.Smtp;

namespace Lemoine.Web.Misc
{
  /// <summary>
  /// EmailSend Service.
  /// </summary>
  public class EmailSendService
    : GenericSaveService<EmailSendRequestDTO>
    , IBodySupport
  {
    static readonly string SENDER_KEY = "net.mail.from";
    static readonly string SENDER_DEFAULT = "user@domain";
    
    static readonly ILog log = LogManager.GetLogger(typeof (EmailSendService).FullName);

    Stream m_body;

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public EmailSendService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync(EmailSendRequestDTO request)
    {
      return SendAndGetResponse (request.From, request.To, request.Subject, request.Body, request.Cc, request.Bcc);
    }

#if NSERVICEKIT
    /// <summary>
    /// Response to POST request for Email/Send service
    /// </summary>
    /// <param name="request"></param>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    public object Post (EmailSendPostRequestDTO request,
                        NServiceKit.ServiceHost.IHttpRequest httpRequest)
    {
      string body;
      using (StreamReader sr = new StreamReader(httpRequest.InputStream))
      {
        body = sr.ReadToEnd();
      }

      return SendAndGetResponse (request.From, request.To, request.Subject, body, request.Cc, request.Bcc);
    }
#else // !NSERVICEKIT

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<object> Post (EmailSendPostRequestDTO request)
    {
      string bodyString;
      m_body.Seek (0, SeekOrigin.Begin);
      using (StreamReader sr = new StreamReader (m_body)) {
        bodyString = sr.ReadToEnd ();
      }

      var result = SendAndGetResponse (request.From, request.To, request.Subject, bodyString, request.Cc, request.Bcc);
      return Task.FromResult (result);
    }
#endif // NSERVICEKIT

    object SendAndGetResponse (string from, string to, string subject, string body, string cc, string bcc)
    {
      try {
        Send (from, to, subject, body, cc, bcc);
      }
      catch (ArgumentNullException ex) {
        log.Error ("SendAndGetResponse: invalid (null) sender or recipient", ex);
        return new ErrorDTO ("Invalid (null) sender or recipient", ErrorStatus.WrongRequestParameter);
      }
      catch (ArgumentOutOfRangeException ex) {
        log.Error ("SendAndGetResponse: missing recipient", ex);
        return new ErrorDTO ("Missing recipient", ErrorStatus.WrongRequestParameter);
      }
      catch (InvalidOperationException ex) {
        log.Error ("SendAndGetResponse: invalid SMTP host or port", ex);
        return new ErrorDTO ("Invalid SMTP host or port", ErrorStatus.MissingConfiguration);
      }
      catch (Exception ex) {
        log.Error ("SendAndGetResponse: unexpected error", ex);
        return new ErrorDTO ("Unexpected SmtpClient error", ErrorStatus.UnexpectedError);
      }
      
      return new EmailSendResponseDTO ();
    }

    /// <summary>
    /// Send an e-mail
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    void Send (string from, string to, string subject, string body, string cc, string bcc)
    {
      var senderName = "Lemoine Pulse";
      var sender = from;
      if (string.IsNullOrEmpty (sender)) {
        sender = Lemoine.Info.ConfigSet.LoadAndGet<string> (SENDER_KEY, SENDER_DEFAULT);
        if (sender.Equals (SENDER_DEFAULT)) {
          log.Warn ($"Send: net.mail.from is unknown => default {SENDER_DEFAULT} is used instead");
        }
        else { // Not default
          senderName = sender;
        }
      }

      var message = new MimeMessage ();
      message.From.Add (new MailboxAddress (senderName, sender));
      message.To.Add (new MailboxAddress (to, to));
      if (!string.IsNullOrEmpty (cc)) {
        message.Cc.Add (new MailboxAddress (cc, cc));
      }
      if (!string.IsNullOrEmpty (bcc)) {
        message.Bcc.Add (new MailboxAddress (bcc, bcc));
      }
      message.Subject = subject;
      message.Body = new TextPart (MimeKit.Text.TextFormat.Plain) {
        Text = body
      };

      
      try {
        using (var smtpClient = new SmtpClient ()) {
          smtpClient.ConnectWithConfigSet ();
          smtpClient.Send (message);
          smtpClient.Disconnect (true);
        }
      }
      catch (SmtpCommandException ex) {
        log.Error ($"Send: Smtp Command exception Status={ex.StatusCode} Error={ex.ErrorCode} {ex.Message}");
        throw;
      }
      catch (SmtpProtocolException ex) {
        log.Error ($"Send: Smtp Protocol exception {ex.Message}");
        throw;
      }
      catch (System.Net.Sockets.SocketException ex) { // For example SocketErrorCode TimedOut
        log.Error ($"Send: Socket exception code={ex.SocketErrorCode} {ex.Message}");
        throw;
      }
      catch (Exception ex) {
        log.Error ($"Send: send failed", ex);
        throw;
      }
    }

    #endregion // Methods

    #region IBodySupport
    /// <summary>
    /// <see cref="IBodySupport"/>
    /// </summary>
    /// <param name="body"></param>
    public void SetBody (Stream body)
    {
      m_body = body;
    }
    #endregion // IBodySupport
  }
}
