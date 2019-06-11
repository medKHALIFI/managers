using System;
using SpatialEye.Framework.Client;
using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The lite print view model that holds the available templates
  /// </summary>
  public class LitePrintViewModel : PrintViewModel
  {
    #region Constructor
    /// <summary>
    /// The default constructor for the print viewModel
    /// </summary>
    public LitePrintViewModel()
    {
      AttachToMessenger();
      SetupTemplates();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attaches the printViewModel to the Messenger, reacting to mapview model changes
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, MapViewModelChanged);
      }
    }

    /// <summary>
    /// Callback from the the messenger
    /// </summary>
    /// <param name="geometryMessage"></param>
    private void MapViewModelChanged(PropertyChangedMessage<LiteMapViewModel> mapViewModelMessage)
    {
      if (mapViewModelMessage.PropertyName == LiteMapsViewModel.CurrentMapPropertyName)
      {
        var model = mapViewModelMessage.NewValue;

        if (model != null)
        {
          // Set the activate Map View for the printViewModel
          this.MapView = model;
        }
      }
    }
    #endregion

    #region Setup of Templates
    /// <summary>
    /// Sets up the templates
    /// </summary>
    private void SetupTemplates()
    {
      if (!IsInDesignMode)
      {
        SetupA4Template1();
        SetupA4Template2();
        SetupA4Template3();
      }
    }

    /// <summary>
    /// Sets up the A4 Template1 template
    /// </summary>
    private void SetupA4Template1()
    {
      // Set up the Default Template
      Func<PrintContext> createContextModel = () => new LitePrintA4Template1SettingsContext(ViewModelLocator.LiteName);

      var template = new PrintTemplate(string.Format(ApplicationResources.PrintTemplateDefault, ViewModelLocator.LiteName), createContextModel, typeof(LitePrintA4Template1SettingsControl))
      {
        Description = ApplicationResources.PrintTemplateDefaultDescription,
        DefaultPageOrientation = PrintPageOrientation.Landscape,
        DefaultPageSize = PrintPageSize.Letter,
        DefaultPageMargins = PrintPageMargins.None,
        DefaultPageQuality = PrintPageQuality.Dpi75
      };

      template.AllowedPageSizes.Clear();
      template.AllowedPageSizes.Add(PrintPageSize.Letter);
      template.AllowedPageSizes.Add(PrintPageSize.A3);
      template.AllowedPageSizes.Add(PrintPageSize.A4);

      // DEFAULT PRINT TEMPLATE
      Func<PrintPageContext> createHeaderModel = () => new LitePrintA4Template1HeaderPageContext();
      Func<PrintPageContext> createMainModel = () => new LitePrintA4Template1MainPageContext();
      Func<PrintPageContext> createFooterModel = () => new LitePrintA4Template1FooterPageContext();

      // The header page
      //template.Pages[PrintTemplatePageType.Header] = new PrintTemplatePage<LitePrintA4Template1HeaderPageView>(createHeaderModel) { ForcePageOrientation = PrintPageOrientation.Portrait };

      // The main page
      template.Pages[PrintTemplatePageType.Main] = new PrintTemplatePage<LitePrintA4Template1MainPageView>(createMainModel);// { ForcePageOrientation = PrintPageOrientation.Landscape};

      // The footer page
      //template.Pages[PrintTemplatePageType.Footer] = new PrintTemplatePage<LitePrintA4Template1FooterPageView>(createFooterModel) { ForcePageOrientation = PrintPageOrientation.Landscape };

      // Add to the templates, to be picked up a Print dialog
      this.Templates.Add(template);
    }

    /// <summary>
    /// Sets up the A4 Template2 template
    /// </summary>
    private void SetupA4Template2()
    {
      // Set up the Default Template
      Func<PrintContext> createContextModel = () => new LitePrintA4Template2SettingsContext(ViewModelLocator.LiteName);

      var template = new PrintTemplate(string.Format(ApplicationResources.PrintTemplateDefaultWithLegend, ViewModelLocator.LiteName), createContextModel, typeof(LitePrintA4Template2SettingsControl))
      {
        Description = ApplicationResources.PrintTemplateDefaultWithLegendDescription,
        DefaultPageOrientation = PrintPageOrientation.Landscape,
        DefaultPageSize = PrintPageSize.Letter,
        DefaultPageMargins = PrintPageMargins.None,
        DefaultPageQuality = PrintPageQuality.Dpi75
      };

      template.AllowedPageSizes.Clear();
      template.AllowedPageSizes.Add(PrintPageSize.Letter);
      template.AllowedPageSizes.Add(PrintPageSize.A3);
      template.AllowedPageSizes.Add(PrintPageSize.A4);

      // DEFAULT PRINT TEMPLATE
      Func<PrintPageContext> createHeaderModel = () => new LitePrintA4Template2HeaderPageContext();
      Func<PrintPageContext> createMainModel = () => new LitePrintA4Template2MainPageContext();
      Func<PrintPageContext> createFooterModel = () => new LitePrintA4Template2FooterPageContext();

      // The header page
      //template.Pages[PrintTemplatePageType.Header] = new PrintTemplatePage<LitePrintA4Template2HeaderPageView>(createHeaderModel) { ForcePageOrientation = PrintPageOrientation.Portrait };

      // The main page
      template.Pages[PrintTemplatePageType.Main] = new PrintTemplatePage<LitePrintA4Template2MainPageView>(createMainModel);// { ForcePageOrientation = PrintPageOrientation.Landscape};

      // The footer page
      //template.Pages[PrintTemplatePageType.Footer] = new PrintTemplatePage<LitePrintA4Template2FooterPageView>(createFooterModel) { ForcePageOrientation = PrintPageOrientation.Landscape };

      // Add to the templates, to be picked up a Print dialog
      this.Templates.Add(template);
    }

    /// <summary>
    /// Sets up the A4 Template3 template
    /// </summary>
    private void SetupA4Template3()
    {
      // Set up the Default Template
      Func<PrintContext> createContextModel = () => new LitePrintA4Template3SettingsContext(ViewModelLocator.LiteName);

      var template = new PrintTemplate(string.Format(ApplicationResources.PrintTemplateLegendOnly, ViewModelLocator.LiteName), createContextModel, typeof(LitePrintA4Template3SettingsControl))
      {
        Description = ApplicationResources.PrintTemplateLegendOnlyDescription,
        DefaultPageOrientation = PrintPageOrientation.Landscape,
        DefaultPageSize = PrintPageSize.Letter,
        DefaultPageMargins = PrintPageMargins.None,
        DefaultPageQuality = PrintPageQuality.Dpi75
      };

      template.AllowedPageSizes.Clear();
      template.AllowedPageSizes.Add(PrintPageSize.Letter);
      template.AllowedPageSizes.Add(PrintPageSize.A3);
      template.AllowedPageSizes.Add(PrintPageSize.A4);

      // DEFAULT PRINT TEMPLATE
      Func<PrintPageContext> createHeaderModel = () => new LitePrintA4Template3HeaderPageContext();
      Func<PrintPageContext> createMainModel = () => new LitePrintA4Template3MainPageContext();
      Func<PrintPageContext> createFooterModel = () => new LitePrintA4Template3FooterPageContext();

      // The header page
      //template.Pages[PrintTemplatePageType.Header] = new PrintTemplatePage<LitePrintA4Template3HeaderPageView>(createHeaderModel) { ForcePageOrientation = PrintPageOrientation.Portrait };

      // The main page
      template.Pages[PrintTemplatePageType.Main] = new PrintTemplatePage<LitePrintA4Template3MainPageView>(createMainModel);// { ForcePageOrientation = PrintPageOrientation.Landscape};

      // The footer page
      //template.Pages[PrintTemplatePageType.Footer] = new PrintTemplatePage<LitePrintA4Template3FooterPageView>(createFooterModel) { ForcePageOrientation = PrintPageOrientation.Landscape };

      // Add to the templates, to be picked up a Print dialog
      this.Templates.Add(template);
    }
    #endregion

    #region Notification
    /// <summary>
    /// The active document is about to be sent to the printer
    /// </summary>
    protected override void OnSendToPrinter()
    {
      base.OnSendToPrinter();

      LiteAnalyticsTracker.TrackMapPrint();
    }
    #endregion
  }
}
