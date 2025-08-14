// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Linq;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Method to check the relations between some persitent classes
  /// </summary>
  public static class OperationRelations
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationRelations).FullName);

    /// <summary>
    /// Try to guess a work order from a component
    /// 
    /// It returns a not null work order when a unique work order can be determined from the component
    /// because a unique work order is associated to the component
    /// 
    /// component.Project.WorkOrders must not be lazy here
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public static IWorkOrder TryToGuessWorkOrderFromComponent (IComponent component)
    {
      if (null == component) {
        log.Warn ("TryToGuessWorkOrderFromComponent: " +
                  "null component ! " +
                  "=> return null");
        return null;
      }

      if (null == component.Project) {
        log.DebugFormat ("TryToGuessWorkOrderFromComponent: " +
                         "null project for component {0} " +
                         "=> return null",
                         component);
        return null;
      }

      Debug.Assert (null != component);
      Debug.Assert (null != component.Project);
      ICollection<IWorkOrder> workOrders =
        component.Project.WorkOrders;
      if (1 == workOrders.Count) {
        IWorkOrder workOrder = workOrders.First ();
        log.DebugFormat ("TryToGuessWorkOrderFromComponent: " +
                         "get WorkOrder {0} from Component {1}" +
                         "because only 1 work order is associated to the component",
                         workOrder, component);
        return workOrder;
      }

      log.DebugFormat ("TryToGuessWorkOrderFromComponent: " +
                       "no work order could be determined from component {0}",
                       component);
      return null;
    }

    /// <summary>
    /// Guess a work order from a component
    /// 
    /// This method asserts a unique work order can be determined from the component
    /// because of the data structure
    /// 
    /// Return null in case no unique work order could be determined from the component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public static IWorkOrder GuessUniqueWorkOrderFromComponent (IComponent component)
    {
      if (null == component) {
        log.Warn ("GuessUniqueWorkOrderFromComponent: " +
                  "null component ! " +
                  "=> return null");
        return null;
      }

      if (null == component.Project) {
        log.DebugFormat ("GuessUniqueWorkOrderFromComponent: " +
                         "null project for component {0} " +
                         "=> return null",
                         component);
        return null;
      }

      Debug.Assert (null != component);
      Debug.Assert (null != component.Project);
      ICollection<IWorkOrder> workOrders =
        component.Project.WorkOrders;
      if (workOrders.Count > 0) {
        System.Diagnostics.Debug.Assert (1 == workOrders.Count);
        if (workOrders.Count > 1) {
          log.WarnFormat ("GuessUniqueWorkOrderFromComponent: " +
                          "there are more than one workOrder " +
                          "that is associated to component {0} " +
                          "although the data structure does not allow this",
                          component);
        }
        IWorkOrder workOrder = workOrders.FirstOrDefault ();
        log.DebugFormat ("GuessUniqueWorkOrderFromComponent: " +
                         "get WorkOrder {0} from Component {1}",
                         workOrder, component);
        return workOrder;
      }

      log.DebugFormat ("GuessUniqueWorkOrderFromComponent: " +
                       "no work order could be determined from component {0}",
                       component);
      return null;
    }

    /// <summary>
    /// Try to guess a work order from an operation
    /// 
    /// It returns a not null work order when a unique work order can be determined from the operation
    /// because a unique work order is associated to the operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static IWorkOrder TryToGuessWorkOrderFromOperation (IOperation operation)
    {
      if (null == operation) {
        log.Warn ("TryToGuessWorkOrderFromOperation: " +
                  "null operation ! " +
                  "=> return null");
        return null;
      }

      // 1. Try to get the intermediate work piece
      Debug.Assert (null != operation);
      ICollection<IIntermediateWorkPiece> intermediateWorkPieces =
        operation.IntermediateWorkPieces;
      if (1 == intermediateWorkPieces.Count) {
        // 2. Try to get the component
        IIntermediateWorkPiece intermediateWorkPiece =
          intermediateWorkPieces.First ();
        ICollection<IComponentIntermediateWorkPiece> componentIntermediateWorkPieces =
          intermediateWorkPiece.ComponentIntermediateWorkPieces;
        if (1 == componentIntermediateWorkPieces.Count) {
          IComponent component = componentIntermediateWorkPieces.First ().Component;
          return TryToGuessWorkOrderFromComponent (component);
        }
      }

      log.DebugFormat ("TryToGuessWorkOrderFromOperation: " +
                       "no work order could be determined from operation {0}",
                       operation);
      return null;
    }

    /// <summary>
    /// Guess a work order from an operation
    /// 
    /// This method asserts a unique work order can be determined from the operation
    /// because of the data structure
    /// 
    /// Return null in case no work order could be determined from the operation
    /// 
    /// operation.IntermediateWorkPieces must be initialized
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static IWorkOrder GuessUniqueWorkOrderFromOperation (IOperation operation)
    {
      if (null == operation) {
        log.Warn ("GuessUniqueWorkOrderFromOperation: " +
                  "null operation ! " +
                  "=> return null");
        return null;
      }

      // 1. Try to get the intermediate work piece
      Debug.Assert (null != operation);
      ICollection<IIntermediateWorkPiece> intermediateWorkPieces =
        operation.IntermediateWorkPieces;
      if (intermediateWorkPieces.Count > 0) {
        Debug.Assert (1 == intermediateWorkPieces.Count);
        if (intermediateWorkPieces.Count > 1) {
          log.WarnFormat ("GuessUniqueWorkOrderFromOperation: " +
                          "there are more than one intermediate work piece " +
                          "that is associated to operation {0} " +
                          "although the data structure does not allow this",
                          operation);
        }
        // 2. Try to get the component
        IIntermediateWorkPiece intermediateWorkPiece = intermediateWorkPieces.FirstOrDefault ();
        ICollection<IComponentIntermediateWorkPiece> componentIntermediateWorkPieces =
          intermediateWorkPiece.ComponentIntermediateWorkPieces;
        if (componentIntermediateWorkPieces.Count > 0) {
          Debug.Assert (1 == componentIntermediateWorkPieces.Count);
          if (componentIntermediateWorkPieces.Count > 1) {
            log.WarnFormat ("GuessUniqueWorkOrderFromOperation: " +
                            "there are more than one component " +
                            "that is associated to operation {0} " +
                            "although the data structure does not allow this",
                            operation);
          }
          IComponent component = componentIntermediateWorkPieces.First ().Component;
          IProject project = component.Project;
          if (null != project) {
            // 3. Try to get a work order
            ICollection<IWorkOrder> workOrders =
              project.WorkOrders;
            if (workOrders.Count > 0) {
              Debug.Assert (1 == workOrders.Count);
              if (workOrders.Count > 1) {
                log.WarnFormat ("GuessUniqueWorkOrderFromOperation: " +
                                "there are more than one workOrder " +
                                "that is associated to operation {0} " +
                                "although the data structure does not allow this",
                                operation);
              }
              IWorkOrder workOrder = workOrders.FirstOrDefault ();
              log.DebugFormat ("GuessUniqueWorkOrderFromOperation: " +
                               "get WorkOrder {0} from operation {1}",
                               workOrder, operation);
              return workOrder;
            }
          }
        }
      }

      log.DebugFormat ("GuessUniqueWorkOrderFromOperation: " +
                       "no work order could be determined from operation {0}",
                       operation);
      return null;
    }

    /// <summary>
    /// Try to guess a component from an operation
    /// 
    /// It returns a not null component when a unique component can be determined from the operation
    /// because a unique component is associated to the operation
    /// 
    /// operation.IntermediateWorkPieces must be initialized
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static IComponent TryToGuessComponentFromOperation (IOperation operation)
    {
      if (null == operation) {
        log.Warn ("TryToGuessComponentFromOperation: " +
                  "null operation ! " +
                  "=> return null");
        return null;
      }

      ICollection<IIntermediateWorkPiece> intermediateWorkPieces =
        operation.IntermediateWorkPieces;
      if (1 == intermediateWorkPieces.Count) {
        IIntermediateWorkPiece intermediateWorkPiece = intermediateWorkPieces.First ();
        ICollection<IComponentIntermediateWorkPiece> componentIntermediateWorkPieces =
          intermediateWorkPiece.ComponentIntermediateWorkPieces;
        if (1 == componentIntermediateWorkPieces.Count) {
          IComponent component = componentIntermediateWorkPieces.First ().Component;
          log.DebugFormat ("TryToGuessComponentFromOperation: " +
                           "get Component {0} from operation {1} " +
                           "because only 1 component is associated to the operation",
                           component, operation);
          return component;
        }
      }

      log.DebugFormat ("TryToGuessComponentFromOperation: " +
                       "no component could be determined from operation {0}",
                       operation);
      return null;
    }

    /// <summary>
    /// Guess a component from an operation in case the component is always unique
    /// for a specified operation
    /// 
    /// This method asserts a unique component can be determined from the operation
    /// because of the data structure
    /// 
    /// Return null in case no component could be determined from the operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static IComponent GuessUniqueComponentFromOperation (IOperation operation)
    {
      if (operation is null) {
        log.Warn ("GuessUniqueComponentFromOperation: null operation! => return null");
        return null;
      }

      // 1. Try to get the intermediate work piece
      ICollection<IIntermediateWorkPiece> intermediateWorkPieces =
        operation.IntermediateWorkPieces;
      if (intermediateWorkPieces.Any ()) {
        if (log.IsDebugEnabled && (intermediateWorkPieces.Count > 1)) {
          log.Debug ($"GuessUniqueComponentFromOperation: there are more than one intermediate work piece that is associated to operation {operation} although the data structure does not allow this");
        }
        // 2. Try to get the component
        var componentIntermediateWorkPieces = intermediateWorkPieces.SelectMany (x => x.ComponentIntermediateWorkPieces);
        var components = componentIntermediateWorkPieces.Select (x => x.Component).Distinct ();
        if (components.Any ()) {
          if (components.Count () > 1) {
            log.Warn ($"GuessUniqueComponentFromOperation: there are more than one component that is associated to operation {operation} although the data structure does not allow this");
          }
          var component = components.First ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GuessUniqueComponentFromOperation: get Component {component} from operation {operation}");
          }
          return component;
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"GuessUniqueComponentFromOperation: o component could be determined from operation {operation}");
      }
      return null;
    }

    /// <summary>
    /// Check a component and an operation are compatible with each other
    /// 
    /// In case component is null, true is returned.
    /// 
    /// In case operation is null, the returned value depends on
    /// data structure option ComponentFromOperationOnly
    /// 
    /// Warning ! This must be run in a NHibernate session
    /// </summary>
    /// <param name="component"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static bool IsComponentCompatibleWithOperation (IComponent component,
                                                           IOperation operation)
    {
      if (null == component) {
        log.Info ("IsComponentCompatibleWithOperation: " +
                  "null component => return true");
        return true;
      }

      if (null == operation) {
        Debug.Assert (null != component);
        var componentFromOperationOnly = Lemoine.Info.ConfigSet
          .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly));
        if (componentFromOperationOnly) {
          log.Info ("IsComponentCompatibleWithOperation: " +
                    "null operation and the component is determined by the operation " +
                    "=> return false");
          return false;
        }
        else {
          log.Info ("IsComponentCompatibleWithOperation: " +
                    "null operation and the component is not determined by the operation " +
                    "=> return true");
          return true;
        }
      }

      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in component.ComponentIntermediateWorkPieces) {
        if (operation.IntermediateWorkPieces.Contains (componentIntermediateWorkPiece.IntermediateWorkPiece)) {
          log.DebugFormat ("IsComponentCompatibleWithOperation: " +
                           "common intermediate work piece {0} found " +
                           "for component {1} and operation {2} " +
                           "=> return true",
                           componentIntermediateWorkPiece.IntermediateWorkPiece,
                           component, operation);
          return true;
        }
      }

      log.DebugFormat ("IsComponentCompatibleWithOperation: " +
                       "no common intermediate work piece found " +
                       "for component {0} and operation {1} " +
                       "=> return false",
                       component, operation);
      return false;
    }

    /// <summary>
    /// Get possible components from an operation
    /// 
    /// It returns a not null component when a unique component can be determined from the operation
    /// because a unique component is associated to the operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static IList<IComponent> GetPossibleComponentsFromOperation (IOperation operation)
    {
      IList<IComponent> result = new List<IComponent> ();

      if (null == operation) {
        log.Warn ("GetPossibleComponentsFromOperation: " +
                  "null operation ! " +
                  "=> return an empty list");
        return result;
      }

      foreach (IIntermediateWorkPiece intermediateWorkPiece in operation.IntermediateWorkPieces) {
        ICollection<IComponentIntermediateWorkPiece> componentIntermediateWorkPieces =
          intermediateWorkPiece.ComponentIntermediateWorkPieces;
        foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in intermediateWorkPiece.ComponentIntermediateWorkPieces) {
          log.DebugFormat ("GetPossibleComponentsFromOperation: " +
                           "get possible Component {0} from operation {1}",
                           componentIntermediateWorkPiece.Component, operation);
          result.Add (componentIntermediateWorkPiece.Component);
        }
      }

      log.DebugFormat ("GetPossibleComponentsFromOperation: " +
                       "{0} possible components were found for operation {1}",
                       result.Count, operation);
      return result;
    }

    /// <summary>
    /// returns true iff workOrder has no associated component
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public static bool WorkOrderHasNoComponent (IWorkOrder workOrder)
    {
      foreach (IProject project in workOrder.Projects) {
        if (project.Components.Count > 0) {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Check a work order and a component are compatible with each other
    /// 
    /// In case workOrder is null, true is returned.
    /// 
    /// In case component is null, the returned value
    /// depends of data structure option WorkOrderFromComponentOnly.
    /// 
    /// Warning ! This must be run in a NHibernate session
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public static bool IsWorkOrderCompatibleWithComponent (IWorkOrder workOrder,
                                                           IComponent component)
    {
      if (null == workOrder) {
        log.Info ("IsWorkOrderCompatibleWithComponent: " +
                  "null workOrder => return true");
        return true;
      }

      if (null == component) {
        Debug.Assert (null != workOrder);
        var workOrderFromComponentOnly = Lemoine.Info.ConfigSet
          .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly));
        if (workOrderFromComponentOnly) {
          log.Info ("IsWorkOrderCompatibleWithComponent: " +
                    "null component and the work order is determined by the component " +
                    "=> return false");
          return false;
        }
        else {
          log.Info ("IsWorkOrderCompatibleWithComponent: " +
                    "null component and the work order is not determined by the component " +
                    "=> return true");
          return true;
        }
      }

      // TODO: check why workOrder.Projects.Contains(component.Project) always fails
      /*
      ICollection<IProject> workOrderProjects = workOrder.Projects;
      
      var projectCol = new HashSet();
      projectCol.Add(component.Project);
      bool res0 = projectCol.Contains(component.Project);
      
      ICollection<IProject> projectCol2 = new HashSet<IProject>();
      projectCol2.Add(component.Project);
      bool res00 = projectCol2.Contains(component.Project);
      
      ICollection<IProject> projectCol3 =  new HashSet<IProject>();
      foreach(IProject project in workOrder.Projects) {
        projectCol3.Add(project);
      }
      
      bool res000 = projectCol3.Contains(component.Project);
      
      bool res1 = workOrderProjects.Contains(component.Project);
      
       */

      // nb: if (workOrder.Projects.Contains(component.Project)) { ... } does not work
      foreach (IProject project in workOrder.Projects) {
        if (project.Equals (component.Project)) {
          log.DebugFormat ("IsWorkOrderCompatibleWithComponent: " +
                           "common project {0} found " +
                           "for workOrder {1} and component {2} " +
                           "=> return true",
                           component.Project,
                           workOrder, component);
          return true;
        }
      }

      log.DebugFormat ("IsWorkOrderCompatibleWithComponent: " +
                       "no common project found " +
                       "for workOrder {0} and component {1} " +
                       "=> return false",
                       workOrder, component);
      return false;
    }

    /// <summary>
    /// Check a work order and an operation are compatible with each other
    /// 
    /// In case workOrder is null, true is returned.
    /// 
    /// In case operation is null, the returned value
    /// depends on data structure options ComponentFromOperationOnly
    /// and WorkOrderFromComponentOnly
    /// 
    /// Warning ! This must be run in a NHibernate session
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static bool IsWorkOrderCompatibleWithOperation (IWorkOrder workOrder,
                                                           IOperation operation)
    {
      if (null == workOrder) {
        log.Info ("IsWorkOrderCompatibleWithOperation: " +
                  "null workOrder => return true");
        return true;
      }

      if (null == operation) {
        Debug.Assert (null != workOrder);
        var componentFromOperationOnly = Lemoine.Info.ConfigSet
          .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ComponentFromOperationOnly));
        if (componentFromOperationOnly) {
          var workOrderFromComponentOnly = Lemoine.Info.ConfigSet
            .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderFromComponentOnly));
          if (workOrderFromComponentOnly) {
            log.Info ("IsWorkOrderCompatibleWithOperation: " +
                      "null operation and the work order is determined by the operation " +
                      "=> return false");
            return false;
          }
        }
        log.Info ("IsWorkOrderCompatibleWithOperation: " +
                  "null operation " +
                  "and work order does not come necessary from the operation " +
                  "=> return true");
        return true;
      }

      foreach (IProject project in workOrder.Projects) {
        foreach (IComponent component in project.Components) {
          foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in component.ComponentIntermediateWorkPieces) {
            if (operation.IntermediateWorkPieces.Contains (componentIntermediateWorkPiece.IntermediateWorkPiece)) {
              log.DebugFormat ("IsComponentCompatibleWithOperation: " +
                               "common intermediate work piece {0} component {1} found " +
                               "for workOrder {2} and operation {3} " +
                               "=> return true",
                               componentIntermediateWorkPiece.IntermediateWorkPiece, component,
                               workOrder, operation);
              return true;
            }
          }
        }
      }

      log.DebugFormat ("IsWorkOrderCompatibleWithOperation: " +
                       "no common project/component found " +
                       "for workOrder {0} and operation {1} " +
                       "=> return false",
                       workOrder, operation);
      return false;
    }

    /// <summary>
    /// Check a line and a component are compatible with each other
    /// 
    /// In case line is null or component is null, true is returned.
    /// 
    /// Warning ! This must be run in a NHibernate session
    /// </summary>
    /// <param name="line"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public static bool IsLineCompatibleWithComponent (ILine line,
                                                      IComponent component)
    {
      if (null == line) {
        log.Info ("IsLineCompatibleWithComponent: " +
                  "null line => return true");
        return true;
      }

      if (null == component) {
        log.Info ("IsLineCompatibleWithComponent: " +
                  "null component => return true");
        return true;
      }

      foreach (IComponent theComponent in line.Components) {
        if (object.Equals (component, theComponent)) {
          log.Info ("IsLineCompatibleWithComponent: " +
                    "component and line are compatible => return true");
          return true;
        }
      }

      log.InfoFormat ("IsLineCompatibleWithComponent: " +
                      "component and line are not compatible " +
                      "=> return false");
      return false;
    }

    /// <summary>
    /// Check a line and an operation are compatible with each other
    /// 
    /// In case line or operation is null, true is returned.
    /// 
    /// Warning ! This must be run in a NHibernate session
    /// </summary>
    /// <param name="line"></param>
    /// <param name="machine">not null</param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static bool IsLineCompatibleWithMachineOperation (ILine line,
                                                             IMachine machine,
                                                             IOperation operation)
    {
      Debug.Assert (null != machine);

      if (null == line) {
        log.Info ("IsLineCompatibleWithMachineOperation: " +
                  "null line => return true");
        return true;
      }

      if (null == operation) {
        log.Info ("IsLineCompatibleWithMachineOperation: " +
                  "null operation => return true");
        return true;
      }

      IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO
        .FindAllByLineMachine (line, machine);
      foreach (ILineMachine lineMachine in lineMachines) {
        if (object.Equals (operation, lineMachine.Operation)) {
          log.Info ("IsLineCompatibleWithMachineOperation: " +
                    "operation and line are compatible => return true");
          return true;
        }
      }

      log.Info ("IsLineCompatibleWithMachineComponent: operation and line are not compatible => return false");
      return false;
    }

    /// <summary>
    /// Check a manufacturing order and an operation are compatible with each other
    /// 
    /// In case manufacturing order or operation is null, true is returned.
    /// 
    /// Warning ! This must be run in a NHibernate session
    /// </summary>
    /// <param name="manufacturingOrder"></param>
    /// <param name="machine">not null</param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static bool IsManufacturingOrderCompatibleWithMachineOperation (IManufacturingOrder manufacturingOrder,
                                                               IMachine machine,
                                                               IOperation operation)
    {
      Debug.Assert (null != machine);

      if (null == manufacturingOrder) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineOperation: null manufacturing order => return true");
        return true;
      }

      if (null == operation) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineOperation: null operation => return true");
        return true;
      }

      if ((null != manufacturingOrder.Machine) && !object.Equals (machine, manufacturingOrder.Machine)) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineOperation: machine and manufacturing order are not compatible => return false");
        return false;
      }

      if (null == manufacturingOrder.Operation) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineOperation: no operation is associated to the manufacturing order => return true");
        return true;
      }

      Debug.Assert (null != manufacturingOrder.Operation);
      Debug.Assert (null != operation);
      if (!object.Equals (manufacturingOrder.Operation, operation)) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineOperation: operation and manufacturing order are not compatible => return false");
        return false;
      }

      log.Info ("IsManufacturingOrderCompatibleWithMachineOperation: operation and manufacturing order are compatible => return true");
      return true;
    }

    /// <summary>
    /// Check a manufacturing order and a component are compatible with each other
    /// 
    /// In case manufacturing order or component is null, true is returned.
    /// 
    /// Warning ! This must be run in a NHibernate session
    /// </summary>
    /// <param name="manufacturingOrder"></param>
    /// <param name="machine">not null</param>
    /// <param name="component"></param>
    /// <returns></returns>
    public static bool IsManufacturingOrderCompatibleWithMachineComponent (IManufacturingOrder manufacturingOrder,
                                                               IMachine machine,
                                                               IComponent component)
    {
      Debug.Assert (null != machine);

      if (null == manufacturingOrder) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineComponent: null manufacturing order => return true");
        return true;
      }

      if (null == component) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineComponent: null component => return true");
        return true;
      }

      if ((null != manufacturingOrder.Machine) && !object.Equals (machine, manufacturingOrder.Machine)) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineComponent: machine and manufacturing order are not compatible => return false");
        return false;
      }

      if ((null != manufacturingOrder.Component) && !object.Equals (manufacturingOrder.Component, component)) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineComponent: manufacturing order references another component => return false");
        return false;
      }

      Debug.Assert (null != component);
      if ((null != manufacturingOrder.Operation) && IsComponentCompatibleWithOperation (component, manufacturingOrder.Operation)) {
        log.InfoFormat ("IsManufacturingOrderCompatibleWithMachineComponent: " +
                        "component and manufacturing order.operation are not compatible => return false");
        return false;
      }

      log.Info ("IsManufacturingOrderCompatibleWithMachineComponent: component and manufacturing order are compatible => return true");
      return true;
    }

    /// <summary>
    /// Check a manufacturing order and a work order are compatible with each other
    /// 
    /// In case manufacturing order or workOrder is null, true is returned.
    /// 
    /// Warning ! This must be run in a NHibernate session
    /// </summary>
    /// <param name="manufacturingOrder"></param>
    /// <param name="machine">not null</param>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public static bool IsManufacturingOrderCompatibleWithMachineWorkOrder (IManufacturingOrder manufacturingOrder,
                                                               IMachine machine,
                                                               IWorkOrder workOrder)
    {
      Debug.Assert (null != machine);

      if (null == manufacturingOrder) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineWorkOrder: " +
                  "null manufacturing order => return true");
        return true;
      }

      if (null == workOrder) {
        log.Info ("IsManufacturingOrderCompatibleWithMachineWorkOrder: " +
                  "null workOrder => return true");
        return true;
      }

      if ((null != manufacturingOrder.Machine) && (machine.Id != manufacturingOrder.Machine.Id)) {
        log.InfoFormat ("IsManufacturingOrderCompatibleWithMachineWorkOrder: " +
                        "machine and manufacturing order are not compatible => return false");
        return false;
      }

      if ((null != manufacturingOrder.WorkOrder) && (null != workOrder) && (((IDataWithId)manufacturingOrder.WorkOrder).Id != ((IDataWithId)workOrder).Id)) {
        log.InfoFormat ("IsManufacturingOrderCompatibleWithMachineWorkOrder: " +
                        "manufacturing order references another work order => return false");
        return false;
      }

      Debug.Assert (null != workOrder);
      if ((null != manufacturingOrder.Operation) && IsWorkOrderCompatibleWithOperation (workOrder, manufacturingOrder.Operation)) {
        log.InfoFormat ("IsManufacturingOrderCompatibleWithMachineWorkOrder: " +
                        "work order and manufacturing order are not compatible => return false");
        return false;
      }

      log.InfoFormat ("IsManufacturingOrderCompatibleWithMachineWorkOrder: " +
                      "work order and manufacturing order are compatible " +
                      "=> return true");
      return true;
    }

  }
}
