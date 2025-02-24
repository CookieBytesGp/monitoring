using Domain.SeedWork;
using Domain.SharedKernel;
using Domain.SharedKernel.Domain.SharedKernel;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Page.ValueObjects
{
    public class BaseElement : Entity
    {
        #region Properties

        public Guid ToolId { get; private set; }
        public int Order { get; private set; }
        public TemplateBody TemplateBody { get; private set; }
        public Asset Asset { get; private set; } // Single asset associated with the element

        #endregion

        #region Constructor

        private BaseElement()
        {
            // Required by EF Core
        }

        private BaseElement(
            Guid toolId,
            int order,
            TemplateBody templateBody,
            Asset asset)
        {
            ToolId = toolId;
            Order = order;
            TemplateBody = templateBody ?? throw new ArgumentNullException(nameof(templateBody));
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
        }

        #endregion

        #region Methods

        public static Result<BaseElement> Create(
            Guid toolId,
            int order,
            TemplateBody templateBody,
            Asset asset)
        {
            var result = new Result<BaseElement>();

            // Validate ToolId
            if (toolId == Guid.Empty)
            {
                result.WithError("ToolId is required.");
            }

            // Validate Order
            if (order < 0)
            {
                result.WithError("Order must be zero or positive.");
            }

            // Validate TemplateBody
            if (templateBody == null)
            {
                result.WithError("TemplateBody is required.");
            }

            // Validate Asset
            if (asset == null)
            {
                result.WithError("Asset is required.");
            }

            if (result.IsFailed)
            {
                return result;
            }

            var baseElement = new BaseElement(
                toolId: toolId,
                order: order,
                templateBody: templateBody,
                asset: asset);

            result.WithValue(baseElement);

            return result;
        }

        public void UpdateTemplateBody(TemplateBody templateBody)
        {
            TemplateBody = templateBody ?? throw new ArgumentNullException(nameof(templateBody));
        }

        public void UpdateAsset(Asset asset)
        {
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
        }

        public void UpdateOrder(int order)
        {
            Order = order;
        }

        #endregion
    }


}
