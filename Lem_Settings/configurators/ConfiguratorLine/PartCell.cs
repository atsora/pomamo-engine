// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of PartCell.
  /// </summary>
  public partial class PartCell : UserControl
  {
    #region Events
    /// <summary>
    /// Triggered when the name changed
    /// First argument is the index, second argument is the name
    /// </summary>
    public event Action<int, string> NameChanged;
    
    /// <summary>
    /// Triggered when the code changed
    /// First argument is the index, second argument is the code
    /// </summary>
    public event Action<int, string> CodeChanged;
    #endregion // Events

    static readonly ILog log = LogManager.GetLogger(typeof (PartCell).FullName);
    
    #region Members
    int m_index = -1;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="index"></param>
    /// <param name="name"></param>
    /// <param name="code"></param>
    public PartCell(int index, string name, string code)
    {
      InitializeComponent();
      m_index = index;
      textName.Text = name;
      textCode.Text = code;
      labelPartNumber.Text = "- Part #" + m_index;
    }
    #endregion // Constructors

    #region Event reactions
    void TextNameTextChanged(object sender, EventArgs e)
    {
      if (NameChanged != null) {
        NameChanged (m_index, textName.Text);
      }
    }
    
    void TextCodeTextChanged(object sender, EventArgs e)
    {
      if (CodeChanged != null) {
        CodeChanged (m_index, textCode.Text);
      }
    }
    #endregion // Event reactions
  }
}
