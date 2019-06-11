using System;
using System.Windows.Browser;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpatialEye.Framework.Documents.Services;
using SpatialEye.Framework.Features.Joins;

namespace Lite
{
  /// <summary>
  /// The Lite FeatureValue Activation view model, handles all value activations in the application.
  /// Value activation comes down to pressing a hyperlink value (geometry, join or smartLink). Any viewModel
  /// that wishes such an activation to be handled, puts it on the messenger as a ActivateFeatureValue 
  /// Request Message. Those request messages are being picked up by this viewModel and dealt with here.
  /// Some requests result in direct actions being carried out, whilst other requests are merely being
  /// morphed into new requests that are themselves placed on the messenger to be dealt with by other
  /// view models.
  /// </summary>
  public class LiteFeatureValueActivationViewModel : ViewModelBase
  {
    #region Property Names
    /// <summary>
    /// Property changed for the Smart Links that are available for selection
    /// </summary>
    public const string SmartLinksPropertyName = "SmartLinks";

    /// <summary>
    /// The selected smart link
    /// </summary>
    public const string SelectedSmartLinkPropertyName = "SelectedSmartLink";

    /// <summary>
    /// The visibility of a corresponding view, dependent on availability of smart links to select from
    /// </summary>
    public const string ViewVisibilityPropertyName = "ViewVisibility";
    #endregion

    #region Fields
    /// <summary>
    /// The Smart Links that are available for selection
    /// </summary>
    private IList<SingleSmartLink> _smartLinks;

    /// <summary>
    /// The sender of the smart links 
    /// </summary>
    private object _smartLinksSender;

    /// <summary>
    /// The feature the smart links belong to
    /// </summary>
    private Feature _smartLinksFeature;

    /// <summary>
    /// The field the smart links belong to
    /// </summary>
    private FeatureFieldDescriptor _smartLinksField;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the Activation (handler) view model
    /// </summary>
    public LiteFeatureValueActivationViewModel(Messenger messenger = null)
      : base(messenger)
    {
      AttachToMessenger();
      SetupCommands();

      Resources = new Lite.Resources.Localization.ApplicationResources();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attaches the view model to the messenger, responding to value activation requests
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<LiteActivateFeatureValueRequestMessage>(this, HandleActivateFeatureValueRequest);
      }
    }

    /// <summary>
    /// The handler for activation for feature values
    /// </summary>
    private void HandleActivateFeatureValueRequest(LiteActivateFeatureValueRequestMessage request)
    {
      if (request.FieldDescriptor != null)
      {
        if (request.FieldDescriptor.IsGeometry)
        {
          // Geometry
          var element = request.Value as IFeatureGeometry;
          if (element != null && element.World != null)
          {
            HandleGeometryActivation(request.Sender, request.Feature, request.FieldDescriptor, element);
          }
        }
        else if (request.FieldDescriptor.IsJoin)
        {
          // Joins
          var element = request.Value as IJoinElement;

          if (element != null)
          {
            HandleJoinActivation(request.Sender, request.Feature, request.FieldDescriptor, element);
          }
        }

        if (request.FieldDescriptor.IsSmartLink)
        {
          // Smart Links
          var smartLinkType = request.FieldDescriptor.FieldType as FeatureSmartLinkType;

          if (smartLinkType.IsMulti)
          {
            // Multi Smart Link
            var multiSmartLink = request.Value as MultiSmartLink;
            var numberOfLinks = multiSmartLink.Count;

            if (numberOfLinks == 1)
            {
              // Multi Smart Link with a single linke
              HandleSingleSmartLinkActivation(request.Sender, request.Feature, request.FieldDescriptor, multiSmartLink.SmartLinks()[0]);
            }
            else if (numberOfLinks > 1)
            {
              // Multi Smart Link with multiple links
              HandleMultiSmartLinkActivation(request.Sender, request.Feature, request.FieldDescriptor, multiSmartLink);
            }
          }
          else
          {
            // Single Smart Link
            HandleSingleSmartLinkActivation(request.Sender, request.Feature, request.FieldDescriptor, request.Value as SingleSmartLink);
          }
        }
      }
    }
    #endregion

    #region Commands
    /// <summary>
    /// The cancel command, closing the view
    /// </summary>
    public RelayCommand CancelCommand
    {
      get;
      private set;
    }

    /// <summary>
    /// Sets up the Cancel command
    /// </summary>
    private void SetupCommands()
    {
      // Use an async lambda to carry out the command
      this.CancelCommand = new RelayCommand(async () =>
        {
          // Give some time to the current context; allowing for handling/ending the UI interaction before initiating changes (via DataBinding)
          // (or more direct: give the UI-thread time to finish the selection in the list before driving the selection itself from here.)
          await TaskFunctions.Yield();

          // Make sure we are removed from display
          SmartLinks = null;
        });
    }
    #endregion

    #region Geometry Activation
    /// <summary>
    /// Handle an activation request for geometry; actually transforming this into a new Go-To request on the databus
    /// </summary>
    private void HandleGeometryActivation(object sender, Feature feature, FeatureFieldDescriptor featureFieldDescriptor, IFeatureGeometry geometry)
    {
      if (geometry != null)
      {
        var geometryField = featureFieldDescriptor as FeatureGeometryFieldDescriptor;
        var envelope = geometry.Envelope;

        if (envelope != null && feature != null)
        {
          var request = new LiteGoToGeometryRequestMessage(sender, new FeatureTargetGeometry(feature, geometryField, geometry))
          {
            DoHighlight = true,
            StoreInHistory = true
          };

          Messenger.Send(request);
        }
      }
    }
    #endregion

    #region SmartLink Activation
    /// <summary>
    /// Handler for multi smart-links, should activate the possibility to choose one of the links from within a UI
    /// </summary>
    private void HandleMultiSmartLinkActivation(object sender, Feature feature, FeatureFieldDescriptor field, MultiSmartLink smartLink)
    {
      _smartLinksSender = sender;
      _smartLinksFeature = feature;
      _smartLinksField = field;

      // And set the smart links
      SmartLinks = smartLink.SmartLinks();
    }

    /// <summary>
    /// Single smart-link activation, actually activating some bits dependent on the type of Smart Link
    /// </summary>
    private async void HandleSingleSmartLinkActivation(object sender, Feature feature, FeatureFieldDescriptor field, SingleSmartLink smartLink)
    {
      if (smartLink != null)
      {
        switch (smartLink.SmartLinkType.PhysicalType)
        {
          case FeaturePhysicalFieldType.SmartLinkToWebPage:

            if (smartLink.Url != null)
            {
              try
              {
                var location = smartLink.Url;
                if (!location.StartsWith("http"))
                {
                  location = string.Concat("http://", location);
                }
                var uri = new Uri(location);
                string str = Guid.NewGuid().ToString("N");
                //string features = "directories=no,location=no,menubar=no,status=no,toolbar=no";
                HtmlPage.Window.Navigate(uri, str);
              }
              catch (InvalidOperationException) { }
              catch (UriFormatException) { }
            }
            break;

          case FeaturePhysicalFieldType.SmartLinkToMailAddress:
            if (smartLink.Url != null)
            {
              var location = new Uri(string.Format("mailto:{0}", smartLink.Url));
              string str = Guid.NewGuid().ToString("N");
              //string features = "directories=no,location=no,menubar=no,status=no,toolbar=no";

              try
              {
                HtmlPage.Window.Navigate(location, str);
              }
              catch (InvalidOperationException)
              {
                //var uriActivator = new InternalHyperlinkButton(location);
                //uriActivator.Activate();
              }
            }
            break;

          case FeaturePhysicalFieldType.SmartLinkToDocument:
            if (smartLink.Url != null && smartLink.ServiceProvider != null)
            {
              var serviceProvider = smartLink.ServiceProvider;
              var service = serviceProvider.GetService<IServerDocumentService>();

              var document = await service.GetServerDocumentAsync(smartLink.Url);
              if (document != null)
              {
                var viewModel = new DocumentViewModel(document);

                // Open the document
                viewModel.View();
              }
            }
            break;

          case FeaturePhysicalFieldType.SmartLinkToWorld:
            var smartLinkToWorld = smartLink as SmartLinkToWorld;
            var world = smartLinkToWorld.World;

            if (world != null)
            {
              Messenger.Send(new LiteGoToWorldRequestMessage(sender, world, feature));
            }
            break;
        }
      }
    }
    #endregion

    #region Join Activation
    /// <summary>
    /// Activation of a Join - which is transformed into a new request for displaying the result feature/featureCollection
    /// </summary>
    private void HandleJoinActivation(object sender, Feature feature, FeatureFieldDescriptor featureFieldDescriptor, IJoinElement element)
    {
      if (element.ResultCardinality.IsMultiple)
      {
        // Multiple result, which is sent as a display featureCollection request
        Messenger.Send(new LiteDisplayFeatureCollectionRequestMessage(sender, element as IJoinToFeatureCollection));
      }
      else
      {
        // Single result, which is sent as a display feature details request
        Messenger.Send(new LiteDisplayFeatureDetailsRequestMessage(sender, element as IJoinToFeature));
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Jump to the smart link
    /// </summary>
    /// <param name="value"></param>
    private async void JumpToSmartLink(SingleSmartLink value)
    {
      // Give some time to the current context; allowing for handling/ending the UI interaction before initiating changes (via DataBinding)
      // (or more direct: give the UI-thread time to finish the selection in the list before driving the selection itself from here.)
      await TaskFunctions.Yield();

      // Make sure we are removed from display
      SmartLinks = null;

      if (value != null)
      {
        // And put the request on the databus
        HandleSingleSmartLinkActivation(_smartLinksSender, _smartLinksFeature, _smartLinksField, value);
      }
    }

    /// <summary>
    /// The Smart Links that are available for selection
    /// </summary>
    public IList<SingleSmartLink> SmartLinks
    {
      get { return _smartLinks; }
      set
      {
        if (value != _smartLinks)
        {
          _smartLinks = value;

          RaisePropertyChanged(SmartLinksPropertyName);
          RaisePropertyChanged(SelectedSmartLinkPropertyName);
          RaisePropertyChanged(ViewVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// The selected smart link
    /// </summary>
    public SingleSmartLink SelectedSmartLink
    {
      get { return null; }
      set
      {
        if (value != null)
        {
          JumpToSmartLink(value);
        }
      }
    }

    /// <summary>
    /// We are visible in case there are smart links
    /// </summary>
    public Visibility ViewVisibility
    {
      get { return _smartLinks != null ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion
  }
}
