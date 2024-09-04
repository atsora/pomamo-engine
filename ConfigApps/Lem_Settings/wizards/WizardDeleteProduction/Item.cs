// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardDeleteProduction
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string LINE = "line";
    internal const string WOLS = "wols";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Delete a production period"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Delete one or more production period(s).";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "lines", "productions", "shifts", "targets", "quantities", "quantity",
          "periods", "goals" };
      }
    }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Production line"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Production periods"; } }
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "wizard"; } }

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IWorkOrderLine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IWorkOrder)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(ILine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;

    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new Page1());
        pages.Add(new Page2());
        return pages; 
      }
    }
    #endregion // Getters / Setters

    #region Wizard methods
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    public ItemData Initialize(ItemData otherData)
    {
      var data = new ItemData();
      
      // Common data
      data.CurrentPageName = "";
      data.InitValue(LINE, typeof(ILine), null, true);
      data.InitValue(WOLS, typeof(IList<IWorkOrderLine>), new List<IWorkOrderLine>(), true);
      
      return data;
    }
    
    /// <summary>
    /// All settings are done, changes will take effect
    /// This method is already within a try / catch
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public void Finalize(ItemData data, ref IList<string> warnings, ref IRevision revision)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          IModelFactory factory = ModelDAOHelper.ModelFactory;
          IDAOFactory factoryDAO = ModelDAOHelper.DAOFactory;
          
          // Commun revision for all deletions
          revision = factory.CreateRevision();
          factoryDAO.RevisionDAO.MakePersistent(revision);
          
          // Delete wols
          ILine line = data.Get<ILine>(LINE);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          IList<IWorkOrderLine> wols = data.Get<IList<IWorkOrderLine>>(WOLS);
          DeleteWols(revision, line, wols);
          
          transaction.Commit();
        }
      }
    }
    #endregion // Wizard methods
    
    #region Other methods
    void DeleteWols(IRevision revision, ILine line, IList<IWorkOrderLine> wols)
    {
      IModelFactory factory = ModelDAOHelper.ModelFactory;
      IDAOFactory factoryDAO = ModelDAOHelper.DAOFactory;
      
      foreach (IWorkOrderLine wol in wols) {
        if (null != wol) {
          // Test if the version didn't change (otherwise stale exception)
          ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock (wol);
          
          // Delete all intermediate workpiece targets related to the line and workorder
          if (wol.WorkOrder != null) {
            DeleteIwpt (wol.WorkOrder, line);
          }

          // Delete the association
          DeleteWol (revision, line, wol);
        }
      }
    }
    
    void DeleteWol(IRevision revision, ILine line, IWorkOrderLine wol)
    {
      // Remove the workorder from the component(s)
      IList<IComponent> components = line.Components.ToList();
      foreach (IComponent component in components) {
        if (component.Part != null) {
          component.Part.RemoveWorkOrder(wol.WorkOrder);
        }
      }

      // Warning: Do not delete the work order !
      // Because there may be references in operation slot and some other summary tables
      // to the work orders
      // But they can be renamed...
      wol.WorkOrder.Name = wol.WorkOrder.Name + "(removed)";
      ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent (wol.WorkOrder);      
      // Find the previous wol
      IWorkOrderLine previousWol = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindAt(
        line, wol.BeginDateTime.Value.AddTicks(-1));
      
      if (previousWol != null)
      {
        // Postpone the end of the previous workorderline
        IWorkOrderLineAssociation association = ModelDAOHelper.ModelFactory.CreateWorkOrderLineAssociation(
          line, previousWol.BeginDateTime.Value, previousWol.Deadline);
        association.End = new UpperBound<DateTime>(wol.EndDateTime.NullableValue);
        association.WorkOrder = previousWol.WorkOrder;
        association.Quantity = previousWol.Quantity;
        foreach (IWorkOrderLineQuantity iwlq in previousWol.IntermediateWorkPieceQuantities.Values) {
          association.SetIntermediateWorkPieceQuantity(iwlq.IntermediateWorkPiece, iwlq.Quantity);
        }

        association.Revision = revision;
        ModelDAOHelper.DAOFactory.WorkOrderLineAssociationDAO.MakePersistent(association);
      }
      else
      {
        // Delete the period by a null value
        IWorkOrderLineAssociation association = ModelDAOHelper.ModelFactory.CreateWorkOrderLineAssociation(
          line, wol.BeginDateTime.Value, wol.Deadline);
        association.End = new UpperBound<DateTime>(wol.EndDateTime.NullableValue);
        association.WorkOrder = null;
        association.Quantity = 0;
        association.Revision = revision;
        ModelDAOHelper.DAOFactory.WorkOrderLineAssociationDAO.MakePersistent(association);
      }
    }
    
    void DeleteIwpt(IWorkOrder wo, ILine line)
    {
      IList<IIntermediateWorkPieceTarget> iwpts = ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
        .FindByWorkOrderLine(wo, line);
      foreach (IIntermediateWorkPieceTarget iwpt in iwpts) {
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO.MakeTransient(iwpt);
      }
    }
    #endregion // Other methods
  }
}
