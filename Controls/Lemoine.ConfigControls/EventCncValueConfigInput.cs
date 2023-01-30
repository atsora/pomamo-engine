// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of EventCncValueConfigInput.
  /// </summary>
  public partial class EventCncValueConfigInput : UserControl
  {
    #region Members
    //See L:123
//    Evaluator m_evaluator = new Evaluator (new CompilerContext (new CompilerSettings (), new ConsoleReportPrinter ()));
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventCncValueConfigInput).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return Name 
    /// </summary>
    public string SelectedName {
      get {
        return nameTextBox.Text;
      }
      set {
        nameTextBox.Text = value;
      }
    }
    
    /// <summary>
    /// Return Message
    /// </summary>
    public string SelectedMessage {
      get {
        return messageTextBox.Text;
      }
      set {
        messageTextBox.Text = value;
      }
    }
    
    /// <summary>
    /// Return Condition
    /// </summary>
    public string SelectedCondition {
      get {
        return conditionTextBox.Text;
      }
      set {
        conditionTextBox.Text = value;
      }
    }
    
    /// <summary>
    /// OkButton from Parent Control to controle data validation
    /// </summary>
    public Button OkButton { get; set;}
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EventCncValueConfigInput()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      nameLabel.Text = PulseCatalog.GetString("Name");
      messageLabel.Text = PulseCatalog.GetString("Message");
      conditionLabel.Text = PulseCatalog.GetString("Condition");
      
      conditionErrorProvider.SetIconAlignment(this.conditionTextBox, ErrorIconAlignment.MiddleRight);
      conditionErrorProvider.SetIconPadding(this.conditionTextBox, 2);
    }

    #endregion // Constructors

    #region Methods
    void EventCncValueConfigInputLoad(object sender, EventArgs e)
    {
      //See L:123
//      m_evaluator.Run ("using System;");
//      m_evaluator.ReferenceAssembly (System.Reflection.Assembly.GetAssembly (typeof(Func<object, bool>)));
    } 
    
    void ConditionTextBoxValidated(object sender, EventArgs e)
    {
      if(IsConditionValid(this.conditionTextBox.Text)){
        conditionErrorProvider.SetError(this.conditionTextBox,"");
        OkButton.Enabled = true;
      }
      else{
        conditionErrorProvider.SetError(this.conditionTextBox,PulseCatalog.GetString("EventCncValueRequiredValidLambda"));
        OkButton.Enabled = false;
      }
    }
    
    /// <summary>
    /// Test if entered Condition is a valid Lambda.
    /// </summary>
    /// <param name="conditionLambda"></param>
    /// <returns></returns>
    private bool IsConditionValid(string conditionLambda){
      if(String.IsNullOrEmpty(conditionLambda)){
        return false;
      }
      
      //TODO : find why it isn't running like it might be
      // Can look into pulse\DotNet\CNC\OutputModules\Lemoine.Cnc.EventCncValue\EventCncValue.cs L:189
      // and pulse\DotNet\CNC\UnitTests\Lemoine.Cnc.EventCncValue.UnitTests to see example of Mono CasS 
      // example.
      // Mono Doc: http://docs.go-mono.com/?link=N%3aMono.CSharp
      // Mono Impl. example : https://github.com/mono/mono/blob/master/mcs/tools/csharp/repl.cs      
//      var s = string.Format (@"new Func<object, bool>({0});", conditionLambda);
//      Func<object, bool> ConditionFunction = null;
//
//      try {
//        ConditionFunction = (Func<object, bool>)m_evaluator.Evaluate(s);
//      }
//      catch (Exception ex) {
//        log.WarnFormat("@[EventCncValueConfigInput][IsConditionValid]: Can't validate lambda : "
//                       +s+" for raison : "+ex.ToString());
//        return false;
//      }
      
      return true;
    }
    #endregion // Methods
  }
}
