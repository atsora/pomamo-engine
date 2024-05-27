// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Factory interface to create the main form
  /// </summary>
  public interface IFormFactory
  {
    /// <summary>
    /// Get the main form
    /// </summary>
    /// <returns></returns>
    Form GetMainForm ();
  }

  /// <summary>
  /// Splash screen options
  /// </summary>
  public class SplashScreenOptions
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public SplashScreenOptions () { }

    /// <summary>
    /// Use identification ?
    /// </summary>
    public bool Identification { get; set; }

    /// <summary>
    /// Login is required
    /// </summary>
    public bool LoginRequired { get; set; }

    /// <summary>
    /// Default login
    /// </summary>
    public string DefaultLogin { get; set; }

    /// <summary>
    /// Default password
    /// </summary>
    public string DefaultPassword { get; set; }

    /// <summary>
    /// Is the remember check box active ?
    /// </summary>
    public bool RememberActive { get; set; }

    /// <summary>
    /// Validate the login and password
    /// 
    /// The arguments are:
    /// <item>login</item>
    /// <item>password</item>
    /// <item>the check box to remember the login is checked</item>
    /// 
    /// It returns true if the user information if the login was validated, else null
    /// </summary>
    public Func<string, string, bool, object> Validate { get; set; }
  }

  /// <summary>
  /// Description of SplashScreen.
  /// </summary>
  public partial class SplashScreen : Form
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SplashScreen).FullName);

    #region Members
    readonly IGuiInitializer m_guiInitializer;
    readonly CancellationToken m_cancellationToken;
    SplashScreenOptions m_options;
    readonly Func<object, Form> m_createForm;
    bool m_buttonGoClicked = false;
    int m_workingWorkers = 0;
    readonly bool m_loginRequired = false;
    readonly bool m_rememberActive = false;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    public SplashScreen (IServiceProvider serviceProvider, SplashScreenOptions options)
      : this ((o) => serviceProvider.GetService<IFormFactory> ().GetMainForm (), serviceProvider.GetService<IGuiInitializer> () ?? new BasicGuiInitializer (null), options ?? serviceProvider.GetService<SplashScreenOptions> ())
    { 
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public SplashScreen (Func<object, Form> createForm, IGuiInitializer guiInitializer, SplashScreenOptions options = null)
    {
      m_guiInitializer = guiInitializer;
      m_options = options ?? new SplashScreenOptions ();
      m_createForm = createForm;
      m_loginRequired = m_options.LoginRequired;
      m_rememberActive = m_options.RememberActive;

      // Database connexion and load extensions
      m_workingWorkers++;
      var worker1 = new BackgroundWorker { WorkerSupportsCancellation = true };
      var cts = new CancellationTokenSource ();
      m_cancellationToken = cts.Token;
      m_cancellationToken.Register (worker1.CancelAsync);
      worker1.DoWork += worker1_DoWork;
      worker1.RunWorkerCompleted += worker_RunWorkerCompleted;
      worker1.RunWorkerAsync ();

      // Preparation of interface
      InitializeComponent ();
      if (m_options.Identification) {
        var login = m_options.DefaultLogin;
        textLogin.Text = login;
        textPassword.Text = m_options.DefaultPassword;
        checkRememberMe.Checked = (login != "");
        if (m_loginRequired && string.IsNullOrEmpty (textLogin.Text)) {
          buttonGo.Enabled = false;
        }
      }
      else {
        tableIdentification.Hide ();
        buttonGo.Enabled = false;
        baseLayout.RowStyles[2].Height = 0;
        this.Height = 220;
        m_buttonGoClicked = true;
      }
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public SplashScreen (Func<object, Form> createForm, Action applicationInitialization = null, SplashScreenOptions options = null)
      : this (createForm, new BasicGuiInitializer (applicationInitialization), options)
    {
    }
    #endregion // Constructors

    #region Methods
    void worker1_DoWork (object sender, DoWorkEventArgs e)
    {
      m_guiInitializer.InitializeGui (m_cancellationToken);
    }

    void worker_RunWorkerCompleted (object sender, RunWorkerCompletedEventArgs e)
    {
      if (m_cancellationToken.IsCancellationRequested) {
        labelMiddle.Text = "Execution cancelled, exiting soon";
        Thread.Sleep (2000);
        Lemoine.Core.Environment.ForceExit ();
      }

      m_workingWorkers--;
      if (m_workingWorkers == 0) {
        if (m_buttonGoClicked) {
          ValidateLoginPassword ();
        }
        else {
          labelMiddle.Text = "Waiting for the user identification";
        }
      }
    }

    void ValidateLoginPassword ()
    {
      if (m_options.Validate is null) {
        var mainForm = m_createForm (null);
        mainForm.Show ();
        this.Hide ();
      }
      else {
        var userInformation = m_options.Validate (textLogin.Text, textPassword.Text, checkRememberMe.Checked);
        if (userInformation is null) {
          labelMiddle.Text = "Identification failed, please try again.";
          buttonGo.Enabled = true;
        }
        else {
          var mainForm = m_createForm (userInformation);
          mainForm.Show ();
          this.Hide ();
        }
      }
    }
    #endregion // Methods

    #region Event reactions
    void ButtonGoClick (object sender, EventArgs e)
    {
      if (m_workingWorkers == 0) {
        ValidateLoginPassword ();
      }
      else {
        labelMiddle.Text = "Please wait...";
        buttonGo.Enabled = false;
        m_buttonGoClicked = true;
      }
    }

    void ButtonCancelClick (object sender, EventArgs e)
    {
      Close ();
    }

    bool m_alreadyFocused1 = false;
    void TextLoginEnter (object sender, EventArgs e)
    {
      if (MouseButtons == MouseButtons.None) {
        textLogin.SelectAll ();
        m_alreadyFocused1 = true;
      }
    }

    void TextLoginLeave (object sender, EventArgs e)
    {
      m_alreadyFocused1 = false;
    }

    void TextLoginMouseUp (object sender, MouseEventArgs e)
    {
      if (!m_alreadyFocused1 && textLogin.SelectionLength == 0) {
        m_alreadyFocused1 = true;
        textLogin.SelectAll ();
      }
    }

    void TextLoginKeyUp (object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) {
        ButtonGoClick (null, null);
      }
    }

    bool m_alreadyFocused2 = false;
    void TextPasswordEnter (object sender, EventArgs e)
    {
      if (MouseButtons == MouseButtons.None) {
        textPassword.SelectAll ();
        m_alreadyFocused2 = true;
      }
    }

    void TextPasswordLeave (object sender, EventArgs e)
    {
      m_alreadyFocused2 = false;
    }

    void TextPasswordMouseUp (object sender, MouseEventArgs e)
    {
      if (!m_alreadyFocused2 && textPassword.SelectionLength == 0) {
        m_alreadyFocused2 = true;
        textPassword.SelectAll ();
      }
    }

    void TextPasswordKeyUp (object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return) {
        ButtonGoClick (null, null);
      }
    }
    #endregion // Event reactions

    private void textLogin_TextChanged (object sender, EventArgs e)
    {
      if (m_loginRequired) {
        buttonGo.Enabled = !string.IsNullOrEmpty (textLogin.Text);
      }
    }

    private void SplashScreen_Load (object sender, EventArgs e)
    {
      checkRememberMe.Visible = m_rememberActive;
      try {
        var bitmap = new Bitmap ("logo-text.png");
        logoPictureBox.Image = bitmap;
      }
      catch (Exception ex) { 
        log.Warn ($"SplashScreen_Load: exception for logo, but continue", ex);
      }
    }
  }
}
