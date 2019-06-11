using System.Linq;
using System.Linq.Expressions;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features.Expressions;
using SpatialEye.Framework.Parameters;

using Lite.Resources.Localization;

using glf = SpatialEye.Framework.Features.Expressions.GeoLinqExpressionFactory;
using SpatialEye.Framework.Features;
using System.Collections.Generic;

namespace Lite
{
  /// <summary>
  /// A viewModel that handles setting up a New Client Query using an Expression Builder.
  /// </summary>
  internal class LiteNewUserQueryViewModelBuilderMode : LiteNewUserQueryViewModelMode
  {
    #region Static Behavior
    /// <summary>
    /// A flag indicating whether the like should be interpreted case independent
    /// </summary>
    public static bool LikeIsCaseIndependent = true;
    #endregion

    #region Fields
    /// <summary>
    /// The parameter definitions
    /// </summary>
    private ParameterDefinitionCollection _parameters;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the lot using 
    /// </summary>
    internal LiteNewUserQueryViewModelBuilderMode()
      : base(ApplicationResources.QueryViaExpressionBuilder)
    {
      IsEnabled = true;
      this.ExpressionBuilder = new ExpressionBuilderViewModel();
    }
    #endregion

    #region API
    /// <summary>
    /// The Expression Builder to set up an expression with
    /// </summary>
    internal ExpressionBuilderViewModel ExpressionBuilder
    {
      get;
      private set;
    }

    /// <summary>
    /// The table descriptor has changed
    /// </summary>
    internal override void OnTableDescriptorChanged()
    {
      this.ExpressionBuilder.Setup(TableDescriptor);
    }


    /// <summary>
    /// Returns the expression for the new query
    /// </summary>
    /// <returns></returns>
    internal override Expression NewQueryExpression()
    {
      var expression = ExpressionBuilder.Expression;

      // Set up default parameters
      _parameters = new ParameterDefinitionCollection();

      if (expression != null)
      {
        expression = expression.GeoLinqParameterizeFields(out _parameters, 
                                                          includeSystemParameters: true, 
                                                          includeLookupsForEnumeratedFields: true);

        if (LikeIsCaseIndependent)
        {
          // Use a visitor to replace the individual bits of the expression
          expression = expression.GeoLinqReplace((e) =>
            {
              if (e != null && e.GeoLinqNodeType() == GeoLinqExpressionType.TextLike)
              {
                // This is a:
                // <Field>.Like(<datum>) expression
                //    morph this expression into 
                // <Field>.Like(<datum>, true) expression to make case independent
                var function = (GeoLinqFunctionExpression)e;
                return glf.Text.Like(function.ParameterExpressions[0], function.ParameterExpressions[1], true);
              }

              // Default visitor behavior - returning the expression itself
              return e;
            });
        }
      }

      return expression;
    }

    /// <summary>
    /// The parameter definitions that are required for running the query
    /// </summary>
    /// <returns></returns>
    internal override ParameterDefinitionCollection NewQueryParameterDefinitions()
    {
      return _parameters;
    }
    #endregion
  }
}
