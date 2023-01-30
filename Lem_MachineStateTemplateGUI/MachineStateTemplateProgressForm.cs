// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;

namespace Lem_MachineStateTemplateGUI
{
  /// <summary>
  /// Description of MachineObservationStateProgressForm.
  /// </summary>
  public partial class MachineStateTemplateProgressForm : Form
  {
    #region Members
    IList<IMachine> m_selectedMachines;
    IMachineStateTemplate m_selectedMachineStateTemplate;
    DateTime m_selectedBeginDate;
    DateTime? m_selectedEndDate;
    IShift m_selectedShift;
    bool m_force = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateProgressForm).FullName);

    #region Getters / Setters
    /// <summary>
    /// MachineSelection (machines, departments and companies)
    /// </summary>
    public IList<IMachine> SelectedMachines {
      get { return m_selectedMachines; }
      set { m_selectedMachines = value; }
    }
    
    /// <summary>
    /// Selected machine observation state
    /// </summary>
    public IMachineStateTemplate SelectedMachineStateTemplate {
      get { return m_selectedMachineStateTemplate; }
      set { m_selectedMachineStateTemplate = value; }
    }
    
    /// <summary>
    /// Selected Begin DateTime
    /// </summary>
    public DateTime SelectedBeginDate {
      get { return m_selectedBeginDate; }
      set { m_selectedBeginDate = value; }
    }
    
    /// <summary>
    /// Selected End DateTime
    /// </summary>
    public DateTime? SelectedEndDate {
      get { return m_selectedEndDate; }
      set { m_selectedEndDate = value; }
    }
    
    /// <summary>
    /// Selected Shift
    /// </summary>
    public IShift SelectedShift {
      get { return m_selectedShift; }
      set { m_selectedShift = value; }
    }
    
    /// <summary>
    /// Force re-building the machine state templates
    /// </summary>
    public bool Force
    {
      get { return m_force; }
      set { m_force = value; }
    }
    
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStateTemplateProgressForm()
    {
      InitializeComponent();
      
      this.Load += new System.EventHandler(this.MachineObservationStateProgressFormLoad);
    }

    #endregion // Constructors

    #region Methods
    void MachineObservationStateProgressFormLoad(object sender, EventArgs e)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      
      using (IDAOSession daoSession = daoFactory.OpenSession())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
        revision.Application = Lemoine.Info.ProgramInfo.Name;
        revision.IPAddress = Lemoine.Info.ComputerInfo.GetIPAddresses ().FirstOrDefault ();
        
        ICollection<IMachineStateTemplateAssociation> machineStateTemplateAssociations = new List<IMachineStateTemplateAssociation>();
        foreach (IMachine machine in SelectedMachines) {
          IMachineStateTemplateAssociation machineStateTemplateAssociation =
            modelFactory.CreateMachineStateTemplateAssociation(machine,SelectedMachineStateTemplate,SelectedBeginDate);
          if (SelectedEndDate.HasValue) {
            machineStateTemplateAssociation.End = new UpperBound<DateTime> (SelectedEndDate);
          }
          if (SelectedShift != null) {
            machineStateTemplateAssociation.Shift = SelectedShift;
          }
          machineStateTemplateAssociation.Force = m_force;
          machineStateTemplateAssociations.Add(machineStateTemplateAssociation);
          machineStateTemplateAssociation.Revision = revision;
          daoFactory.RevisionDAO.MakePersistent (revision);
          daoFactory.MachineStateTemplateAssociationDAO.MakePersistent(machineStateTemplateAssociation);
        }
        
        transaction.Commit();
        
        this.modificationProgressBar1.AddModifications((System.Collections.ICollection) machineStateTemplateAssociations);
        this.modificationProgressBar1.AnalysisProgressLoad();
      }
    }
    
    void ProgressBarDisposed(object sender, EventArgs e)
    {
      this.Dispose();
    }
    
    #endregion // Methods
  }
}
