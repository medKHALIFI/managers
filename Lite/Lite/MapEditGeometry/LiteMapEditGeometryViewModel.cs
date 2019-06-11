using SpatialEye.Framework.Client;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using SpatialEye.Framework.Redlining;
using SpatialEye.Framework.Geometry;
using System.Collections.ObjectModel;
using Lite.Resources.Localization;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Drawing;

namespace Lite
{
  /// <summary>
  /// The edit geometry view model handles editing of feature geometry, comparable to
  /// the trail viewmodel.
  /// </summary>
  public class LiteMapEditGeometryViewModel : ViewModelBase
  {
    #region Fields
    /// <summary>
    /// The mapView to use
    /// </summary>
    private MapViewModel _mapView;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the MapTrail ViewModel and attaches it to the specified messenger
    /// </summary>
    /// <param name="messenger"></param>
    public LiteMapEditGeometryViewModel(Messenger messenger = null)
      : base(messenger)
    {
      // Attach to the messenger; reacting to view model changes
      AttachToMessenger();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      // Set up a handler for active mapView changes
      this.Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, change => MapView = change.NewValue);

      // Register handlers for starting/stopping editing of geometry
      this.Messenger.Register<LiteStartEditGeometryRequestMessage>(this, HandleStartEditGeometryRequest);
      this.Messenger.Register<LiteStopEditGeometryRequestMessage>(this, HandleStopEditGeometryRequest);
    }
    #endregion

    #region Callbacks and Handlers
    /// <summary>
    /// A property of the map view has changed
    /// </summary>
    void MapViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    { }

    /// <summary>
    /// Callback when the edit geometry layer changes
    /// </summary>
    private void Drawing_ContentsChanged(GeometryModel<RedliningElement> model, GeometryModel<RedliningElement>.ContentsChangedEventArgs args)
    { }

    /// <summary>
    /// Callback when the edit geometry selection changes
    /// </summary>
    void DrawingSelection_ContentsChanged(GeometryModel<RedliningElement> selection, GeometryModel<RedliningElement>.ContentsChangedEventArgs args)
    {
      if (!SettingUp && IsEditing && this.EditFeature != null)
      {
        if (selection.Count == 1)
        {
          var element = selection[0];
          var field = element.Tag as FeatureGeometryFieldDescriptor;

          if (field != null)
          {
            // Get the geometry
            var geometry = element.Geometry;

            // Set the geometry
            EditFeature[field] = geometry;
          }
        }
      }
    }

    /// <summary>
    /// Callback for stopping editing the current geometry
    /// </summary>
    void EditGeometryLayer_DoneInteracting(DrawingMapLayerViewModel<RedliningElement> sender, GeometryModel<RedliningElement> selection)
    {
      if (selection.Count == 0)
      {
        Messenger.Send(new LiteStopEditFeatureRequestMessage(this));
      }
    }

    /// <summary>
    /// Handler for request to start editing geometry
    /// </summary>
    private void HandleStartEditGeometryRequest(LiteStartEditGeometryRequestMessage message)
    {
      var mapView = MapView;
      if (mapView != null && message.Feature != null && message.FieldDescriptor != null)
      {
        StartEditingGeometry(message.Feature, message.FieldDescriptor);
      }
    }

    /// <summary>
    /// Handler for request to stop editing geometry
    /// </summary>
    private void HandleStopEditGeometryRequest(LiteStopEditGeometryRequestMessage message)
    {
      StopEditingGeometry(message.FieldDescriptor);
    }
    #endregion

    #region Properties
    /// <summary>
    /// The map view we are currently creating trails for
    /// </summary>
    public MapViewModel MapView
    {
      get { return _mapView; }
      set
      {
        if (_mapView != value)
        {
          if (_mapView != null)
          {
            _mapView.PropertyChanged -= MapViewPropertyChanged;

            var editLayer = _mapView.EditGeometryLayer;
            if (editLayer != null)
            {
              editLayer.Drawing.ContentsChanged -= Drawing_ContentsChanged;
              editLayer.DrawingSelection.ContentsChanged -= DrawingSelection_ContentsChanged;
              editLayer.DoneInteracting -= EditGeometryLayer_DoneInteracting;
            }
          }

          _mapView = value;

          if (_mapView != null)
          {
            _mapView.PropertyChanged += MapViewPropertyChanged;

            var editLayer = _mapView.EditGeometryLayer;
            if (editLayer != null)
            {
              editLayer.Drawing.ContentsChanged += Drawing_ContentsChanged;
              editLayer.DrawingSelection.ContentsChanged += DrawingSelection_ContentsChanged;
              editLayer.DoneInteracting += EditGeometryLayer_DoneInteracting;
            }
          }
        }
      }
    }

    /// <summary>
    /// Internal property to help trail construction be less verbose
    /// </summary>
    private CoordinateSystem CoordinateSystem
    {
      get { return MapView != null ? MapView.CoordinateSystem : null; }
    }

    /// <summary>
    /// Returns the edit geometry layer that is defined on the MapViewModel
    /// </summary>
    private DrawingMapLayerViewModel<RedliningElement> EditGeometryLayer
    {
      get { return MapView != null ? MapView.EditGeometryLayer : null; }
    }

    /// <summary>
    /// The drawing model
    /// </summary>
    private Drawing<RedliningElement> DrawingModel
    {
      get { return MapView != null ? MapView.EditGeometryLayer.Drawing : null; }
    }

    private GeometryModel<RedliningElement> DrawingSelection
    {
      get { return MapView != null ? MapView.EditGeometryLayer.DrawingSelection : null; }
    }

    /// <summary>
    /// Are we setting up a new redlining element
    /// </summary>
    private bool SettingUp { get; set; }

    /// <summary>
    /// The edit feature target
    /// </summary>
    private EditableFeature EditFeature
    {
      get;
      set;
    }

    /// <summary>
    /// Are we editing
    /// </summary>
    private bool IsEditing
    {
      get;
      set;
    }
    #endregion

    #region Editing Geometry

    /// <summary>
    /// Clear the model completely
    /// </summary>
    private void ClearModel()
    {
      var model = this.DrawingModel;
      if (model != null)
      {
        model.Clear();
      }
    }

    /// <summary>
    /// Returns an edit element for the specified fieldDescriptor
    /// </summary>
    private RedliningElement ElementFor(FeatureGeometryFieldDescriptor fieldDescriptor, IFeatureGeometry geometry)
    {
      RedliningElement redliningElement = null;
      var model = this.DrawingModel;
      var selection = this.DrawingSelection;

      if (model != null && selection != null)
      {
        foreach (var element in model)
        {
          if ((element.Tag as FeatureGeometryFieldDescriptor) == fieldDescriptor)
          {
            redliningElement = element;
            break;
          }
        }

        if (redliningElement != null)
        {
          selection.Set(redliningElement);
        }
        else
        {
          if (geometry != null)
          {
            // Set the geometry to the correct coordinate system
            geometry = geometry.InCoordinateSystem(this.CoordinateSystem);

            // Get the redlining element
            redliningElement = RedliningElement.ElementFor(geometry);

            // Make sure the element is decorated
            MapView.EditGeometryLayer.DecorateElement(redliningElement);

            model.Add(redliningElement);
            selection.Set(redliningElement);
          }
          else
          {
            redliningElement = RedliningElement.Create(fieldDescriptor.FieldType.PhysicalType, CoordinateSystem);
            MapView.EditGeometryLayer.NewElement(redliningElement);
          }

          // Make sure we are using the cache properly
          redliningElement.Tag = fieldDescriptor;
        }
      }

      return redliningElement;
    }

    /// <summary>
    /// Handles the starting of editing geometry
    /// </summary>
    private void StartEditingGeometry(EditableFeature feature, FeatureGeometryFieldDescriptor fieldDescriptor)
    {
      // Get the mapView to interact upon
      var mapView = MapView;

      // Make sure we stop editing geometry first
      StopEditingGeometry();

      if (mapView != null)
      {
        mapView.ClearSelection();

        if (EditFeature != feature)
        {
          // A new feature; clear the model completely
          ClearModel();
        }

        IsEditing = false;
        this.EditFeature = null;

        if (feature != null && MapView != null)
        {
          var geometry = feature[fieldDescriptor] as IFeatureGeometry;

          // Set the edit properties
          EditFeature = feature;
          IsEditing = true;

          // Now do the thing, dependent on availability of a geometry
          try
          {
            SettingUp = true;
            var redliningElement = ElementFor(fieldDescriptor, geometry);
          }
          finally
          {
            SettingUp = false;
          }
        }
      }
    }

    /// <summary>
    /// Handles the stopping of editing geometry
    /// </summary>
    private void StopEditingGeometry(FeatureGeometryFieldDescriptor fieldDescriptor = null)
    {
      // Get the mapView to interact upon
      var mapView = MapView;
      var editLayer = EditGeometryLayer;
      var model = this.DrawingModel;
      var selection = this.DrawingSelection;

      IsEditing = false;

      if (model != null && selection != null)
      {
        selection.Clear();

        if (fieldDescriptor != null)
        {
          RedliningElement elementToRemove = null;

          foreach (var element in model.Elements())
          {
            if (element.Tag == fieldDescriptor)
            {
              elementToRemove = element;
              break;
            }
          }

          if (elementToRemove != null)
          {
            model.Remove(elementToRemove);
          }
        }
        else
        {
          // No specific geometry; just clear the model
          ClearModel();
        }
      }

      if (editLayer != null)
      {
        // Make sure we explicitly cancel all interaction in case the last interaction
        // mode was activated by a New-Geometry action
        editLayer.StopInteraction();
      }
    }
    #endregion
  }
}
