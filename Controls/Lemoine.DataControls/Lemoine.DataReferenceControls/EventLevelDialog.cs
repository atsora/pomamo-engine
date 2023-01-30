// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of EventLevelDialog.
  /// </summary>
  public partial class EventLevelDialog : OKCancelDialog, IValueDialog<IEventLevel>
  {
    #region Members

    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventLevelDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null EventLevel a valid value ?
    /// </summary>
    public bool Nullable {
      get { return eventLevelSelection.Nullable; }
      set { eventLevelSelection.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  eventLevelSelection.DisplayedProperty; }
      set { eventLevelSelection.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected EventLevel or null if no EventLevel is selected
    /// </summary>
    public IEventLevel SelectedValue {
      get
      {
        return this.eventLevelSelection.SelectedEventLevel;
      }
      set {
        this.eventLevelSelection.SelectedEventLevel = value;
      }
    }
    
    /// <summary>
    /// Selected EventLevels or null if no EventLevel is selected
    /// </summary>
    public IList<IEventLevel> SelectedValues {
      get {
        return this.eventLevelSelection.SelectedEventLevels;
      }
      set {
        this.eventLevelSelection.SelectedEventLevels = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// EventLevel Dialog
    /// </summary>
    public EventLevelDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString ("EventLevelDialogTitle");
    }


    #endregion // Constructors

    #region Methods
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    #endregion // Methods
  }
}
