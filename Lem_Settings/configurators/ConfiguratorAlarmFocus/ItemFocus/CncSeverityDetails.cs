// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of CncSeverityDetails.
  /// </summary>
  public partial class CncSeverityDetails : UserControl
  {
    #region Events
    /// <summary>
    /// Emitted when the button "Edit description" is clicked
    /// The first argument is the severity
    /// </summary>
    public event Action<ICncAlarmSeverity> EditDescriptionClicked;

    /// <summary>
    /// Emitted when the button "Add pattern" is clicked
    /// The first argument is the severity
    /// </summary>
    public event Action<ICncAlarmSeverity> AddPatternClicked;

    /// <summary>
    /// Emitted when the button "Delete severity" is clicked
    /// The first argument is the severity
    /// </summary>
    public event Action<ICncAlarmSeverity> DeleteSeverityClicked;

    /// <summary>
    /// Emitted when the button "Edit pattern" is clicked
    /// The first argument is the pattern
    /// </summary>
    public event Action<ICncAlarmSeverityPattern> EditPatternClicked;

    /// <summary>
    /// Emitted when the button "Delete pattern" is clicked
    /// The first argument is the pattern
    /// </summary>
    public event Action<ICncAlarmSeverityPattern> DeletePatternClicked;
    #endregion // Events

    #region Members
    readonly ContextMenuStrip m_menu = new ContextMenuStrip ();
    readonly ICncAlarmSeverity m_severity = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CncSeverityDetails).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="severity"></param>
    public CncSeverityDetails (ICncAlarmSeverity severity)
    {
      m_severity = severity;
      InitializeComponent ();
      baseLayout.BackColor = LemSettingsGlobal.COLOR_CATEGORY;

      // Preparation of the menu
      m_menu.ItemClicked += MenuClicked;
      ToolStripItem item1 = m_menu.Items.Add ("Edit description");
      item1.Tag = "description";
      ToolStripItem item2 = m_menu.Items.Add ("Add pattern");
      item2.Tag = "pattern";
      m_menu.Items.Add (new ToolStripSeparator ());
      ToolStripItem item3 = m_menu.Items.Add ("Delete severity");
      item3.Tag = "delete";

      // Name and description
      labelName.Text = severity.Name;
      if (severity.Focus.HasValue) {
        labelName.Text += " (" + (severity.Focus.Value ? "focused" : "ignored") + ")";
      }

      SetToolTip (pictureHelp, m_severity);

      // Get all patterns
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          var patterns = ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO.FindBySeverity (severity, false);
          if (patterns.Count > 0) {
            // Add a cell for each pattern
            int height = 43;
            foreach (var pattern in patterns) {
              var widget = new CncSeverityPatternDetails (pattern);
              widget.Dock = DockStyle.Fill;
              widget.EditPatternClicked += OnEditPatternClicked;
              widget.DeletePatternClicked += OnDeletePatternClicked;
              verticalScroll.AddControl (widget);
              height += 37;
            }
            stackedWidget.SelectedIndex = 0;
            this.Height = height;
          }
          else {
            // Show "no patterns"
            stackedWidget.SelectedIndex = 1;
            this.Height = 100;
          }
        }
      }
    }
    #endregion // Constructors

    #region Methods
    void SetToolTip (Control control, ICncAlarmSeverity severity)
    {
      // Add a tooltip with the description of the severity
      var toolTip = new ToolTip ();
      toolTip.SetToolTip (control, severity.FullDescription);
      toolTip.AutoPopDelay = 32000;
    }
    #endregion // Methods

    #region Event reactions
    void ButtonMenuClick (object sender, EventArgs e)
    {
      m_menu.Show (buttonMenu, new Point (0, buttonMenu.Height));
    }

    void MenuClicked (Object sender, ToolStripItemClickedEventArgs e)
    {
      m_menu.Hide ();
      if (e.ClickedItem.Tag == null) {
        return;
      }

      string actionStr = (string)e.ClickedItem.Tag;
      switch (actionStr) {
      case "description":
        EditDescriptionClicked (m_severity);
        break;
      case "pattern":
        AddPatternClicked (m_severity);
        break;
      case "delete":
        DeleteSeverityClicked (m_severity);
        break;
      }
    }

    void OnEditPatternClicked (ICncAlarmSeverityPattern pattern)
    {
      EditPatternClicked (pattern);
    }

    void OnDeletePatternClicked (ICncAlarmSeverityPattern pattern)
    {
      DeletePatternClicked (pattern);
    }

    void ButtonAddPatternClick (object sender, EventArgs e)
    {
      AddPatternClicked (m_severity);
    }
    #endregion // Event reactions
  }
}
