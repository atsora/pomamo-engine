// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Settings
{
  /// <summary>
  /// Interface shared by all item pages
  /// </summary>
  public interface IItemPage: IDisposable
  {
    /// <summary>
    /// Change the title of the page
    /// In most cases you can take the initial title of the page
    /// and append precisions between brackets
    /// A null value will restore the original title
    /// </summary>
    event Action<string> SetTitle;
    
    /// <summary>
    /// Specify a header with a text and a color:
    /// * LemSettingsGlobal.COLOR_OK
    /// * LemSettingsGlobal.COLOR_WARNING
    /// * LemSettingsGlobal.COLOR_ERROR
    /// An empty text remove the header.
    /// </summary>
    event Action<System.Drawing.Color, string> SpecifyHeader;
    
    /// <summary>
    /// Title
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    string Help { get; }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    LemSettingsGlobal.PageFlag Flags { get; }
    
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    void Initialize(ItemContext context);
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    void LoadPageFromData(ItemData data);
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    void SavePageInData(ItemData data);
    
    /// <summary>
    /// Method called every second
    /// </summary>
    void OnTimeOut();
  }
}
