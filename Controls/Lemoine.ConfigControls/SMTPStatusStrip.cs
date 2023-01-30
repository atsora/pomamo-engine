// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using MailKit;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of SMTPStatusStrip.
  /// </summary>
  public partial class SMTPStatusStrip : UserControl, IConfigControlObserver<IConfig>
  {
    #region Members
    int m_port = 25;
    string m_host = null;
    string m_user = null;
    string m_userPassword = null;
    bool m_enableSsl = false;

    Exception m_lastError = null;

    string m_fromAdress = null;
    string m_toAdress = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (SMTPStatusStrip).FullName);

    #region Getters / Setters

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public SMTPStatusStrip ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      smtpStatusLabel.Text = PulseCatalog.GetString ("SMTPConnexion");
      smtpStatusLed.Text = PulseCatalog.GetString ("NotTested");
      sendEmailToolStripMenuItem.Text = PulseCatalog.GetString ("SendEmail");
      moreInformationToolStripMenuItem.Text = PulseCatalog.GetString ("MoreInformations");
    }
    #endregion // Constructors

    void SMTPStatusStripLoad (object sender, EventArgs e)
    {
      // If STMPClient can one day test connection without sending mail.
      //      LoadData();
      //      TestSMTPConnecting();
    }

    #region Methods
    /// <summary>
    /// Set working Data
    /// </summary>
    void LoadData ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      if (null == daoFactory) {
        log.ErrorFormat ("SMTPStatusStrip: " +
                         "no DAO factory is defined");
        return;
      }

      using (IDAOSession daoSession = daoFactory.OpenSession ()) {
        IList<IConfig> configs =
          daoFactory.ConfigDAO.FindLike ("net.mail.smtp.%");
        foreach (IConfig config in configs) {
          switch (config.Key) {
          case "net.mail.smtp.network.host": {
            this.m_host = (string)config.Value;
            break;
          }
          case "net.mail.smtp.network.port": {
            this.m_port = (int)config.Value;
            break;
          }
          case "net.mail.smtp.network.userName": {
            this.m_user = (string)config.Value;
            break;
          }
          case "net.mail.smtp.network.password": {
            this.m_userPassword = (string)config.Value;
            break;
          }
          case "net.mail.smtp.network.enableSsl": {
            this.m_enableSsl = (bool)config.Value;
            break;
          }
          default: {
            log.WarnFormat ("Unknown SMTP option in SMTPStatusStrip");
            break;
          }
          }
        }
      }
    }

    /// <summary>
    /// Test if we can send a message
    /// </summary>
    /// <returns></returns>
    void TestSMTPSending ()
    {
      m_lastError = null;

      if (m_toAdress == null && m_fromAdress == null) {
        return;
      }

      var message = new MimeMessage ();
      message.From.Add (new MailboxAddress (m_fromAdress, m_fromAdress));
      message.To.Add (new MailboxAddress (m_toAdress, m_toAdress));

      message.Body = new TextPart (MimeKit.Text.TextFormat.Plain) {
        Text = @"
"
      };
      message.Subject = "Lem_Configuration Test Mail";

      try {
        smtpStatusLed.Text = PulseCatalog.GetString ("Sending");
        smtpStatusLed.BackColor = Color.Transparent; //Default Color

        using (var smtpClient = new SmtpClient ()) {
          var options = m_enableSsl
            ? SecureSocketOptions.StartTlsWhenAvailable
            : SecureSocketOptions.None;
          smtpClient.Connect (m_host, port: m_port, options: options);
          if (!string.IsNullOrEmpty (m_user)) {
            smtpClient.Authenticate (m_user, m_userPassword);
          }
          smtpClient.MessageSent += OnMessageSent;
          smtpClient.Send (message);
          smtpClient.Disconnect (true);
        }
        smtpStatusLed.Text = PulseCatalog.GetString ("Success");
      }
      catch (SmtpCommandException ex) {
        m_lastError = ex;
        smtpStatusLed.ToolTipText =$"Smtp command exception Status={ex.StatusCode} Error={ex.ErrorCode} {ex.Message}";
        smtpStatusLed.Text = PulseCatalog.GetString ("Failed");
        smtpStatusLed.BackColor = Color.Red;
      }
      catch (SmtpProtocolException ex) {
        m_lastError = ex;
        smtpStatusLed.ToolTipText = $"Smtp protocol exception {ex.Message}";
        smtpStatusLed.Text = PulseCatalog.GetString ("Failed");
        smtpStatusLed.BackColor = Color.Red;
      }
      catch (System.Net.Sockets.SocketException ex) { // For example SocketErrorCode TimedOut
        m_lastError = ex;
        smtpStatusLed.ToolTipText = $"Socket exception code={ex.SocketErrorCode} {ex.Message}";
        smtpStatusLed.Text = PulseCatalog.GetString ("Failed");
        smtpStatusLed.BackColor = Color.Red;
      }
      catch (Exception ex) {
        m_lastError = ex;
        smtpStatusLed.ToolTipText = ex.Message;
        smtpStatusLed.Text = PulseCatalog.GetString ("Failed");
        smtpStatusLed.BackColor = Color.Red;
      }
    }

    private void OnMessageSent (object sender, MessageSentEventArgs e)
    {
      smtpStatusLed.Text = PulseCatalog.GetString ("Sent");
      smtpStatusLed.BackColor = Color.Green;
    }
    #endregion // Methods

    #region IConfigControlObserver Impl.
    /// <summary>
    /// Called after a Config was deleted 
    /// </summary>
    /// <param name="deletedEntities"></param>
    public void UpdateAfterDelete (System.Collections.Generic.ICollection<IConfig> deletedEntities)
    {
      //Do nothing
    }

    /// <summary>
    /// Called after a Config was updated
    /// </summary>
    /// <param name="updatedEntities"></param>
    public void UpdateAfterUpdate (System.Collections.Generic.ICollection<IConfig> updatedEntities)
    {
      //If SMTPclient can test connection without sending mail.
      //      LoadData();
      //      TestSMTPConnecting();
    }
    #endregion //IConfigControlObserver Impl.

    /// <summary>
    /// Used to send a test mail
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void SendEmailToolStripMenuItemClick (object sender, EventArgs e)
    {

      MailAdressDialog dialog = new MailAdressDialog ();
      if (dialog.ShowDialog () == DialogResult.OK) {
        if (dialog.ToAdress != null && dialog.FromAdress != null) {
          m_toAdress = dialog.ToAdress;
          m_fromAdress = dialog.FromAdress;
          LoadData ();
          TestSMTPSending ();
        }
      }
    }

    /// <summary>
    /// Display all exception message from last Error 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void MoreInformationToolStripMenuItemClick (object sender, EventArgs e)
    {
      if (m_lastError != null) {
        StringBuilder strBuilder = new StringBuilder ();

        Exception ex = (Exception)m_lastError;
        int level = 1;
        while (ex != null) {
          if (!string.IsNullOrEmpty (ex.Message)) {
            strBuilder.AppendFormat (PulseCatalog.GetString ("SMTPErrorLevelDescription"), level, ex.Message);
            strBuilder.AppendLine ();
            level++;
          }
          ex = ex.InnerException;
        }

        strBuilder.AppendLine ();
        strBuilder.AppendFormat (PulseCatalog.GetString ("SMTPError"), m_lastError.Message);
        strBuilder.AppendLine ();
        MessageBox.Show (strBuilder.ToString (), PulseCatalog.GetString ("SMTPErrorDialog"), MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
    }

  }
}
