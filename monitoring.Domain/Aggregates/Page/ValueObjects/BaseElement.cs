#region using 
using FluentResults;
using Monitoring.Domain.SeedWork;
using Domain.SharedKernel;
using System.Reflection;
#endregion

namespace Domain.Aggregates.Page.ValueObjects
{
    public class BaseElement : Entity
    {
        #region Properties

        public Guid ToolId { get; private set; }
        public int Order { get; private set; }
        public TemplateBody TemplateBody { get; private set; }
        public Asset Asset { get; private set; } // Single asset associated with the element
        
        // Configuration properties for dynamic styling and content management
        public Dictionary<string, object> ContentConfig { get; private set; }
        public Dictionary<string, object> StyleConfig { get; private set; }

        #endregion

        #region Constructor

        private BaseElement()
        {
            // Required by EF Core
            ContentConfig = new Dictionary<string, object>();
            StyleConfig = new Dictionary<string, object>();
        }

        private BaseElement(
            Guid toolId,
            int order,
            TemplateBody templateBody,
            Asset asset,
            Dictionary<string, object> contentConfig = null,
            Dictionary<string, object> styleConfig = null)
        {
            ToolId = toolId;
            Order = order;
            TemplateBody = templateBody ?? throw new ArgumentNullException(nameof(templateBody));
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
            ContentConfig = contentConfig ?? new Dictionary<string, object>();
            StyleConfig = styleConfig ?? new Dictionary<string, object>();
        }

        #endregion

        #region Methods

        public static Result<BaseElement> Create(
            Guid toolId,
            int order,
            TemplateBody templateBody,
            Asset asset,
            Dictionary<string, object> contentConfig = null,
            Dictionary<string, object> styleConfig = null)
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
                asset: asset,
                contentConfig: contentConfig,
                styleConfig: styleConfig);

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

        public void UpdateContentConfig(Dictionary<string, object> contentConfig)
        {
            ContentConfig = contentConfig ?? new Dictionary<string, object>();
        }

        public void UpdateStyleConfig(Dictionary<string, object> styleConfig)
        {
            StyleConfig = styleConfig ?? new Dictionary<string, object>();
        }

        public void UpdateContentProperty(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            
            ContentConfig[key] = value;
        }

        public void UpdateStyleProperty(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            
            StyleConfig[key] = value;
        }

        public T GetContentProperty<T>(string key, T defaultValue = default(T))
        {
            if (ContentConfig.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }

        public T GetStyleProperty<T>(string key, T defaultValue = default(T))
        {
            if (StyleConfig.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }
        
        // New overload for rehydration that preserves an existing Id.
        public static Result<BaseElement> Rehydrate(
            Guid id,
            Guid toolId,
            int order,
            TemplateBody templateBody,
            Asset asset,
            Dictionary<string, object> contentConfig = null,
            Dictionary<string, object> styleConfig = null)
        {
            var result = Create(toolId, order, templateBody, asset, contentConfig, styleConfig);
            if (result.IsSuccess)
            {
                // Use reflection to set the Id on the new domain object.
                SetEntityId(result.Value, id);
            }
            return result;
        }

        // (Include the SetEntityId method here as a private static helper, or reference an external helper)
        private static void SetEntityId(BaseElement element, Guid id)
        {
            var idProperty = typeof(Entity).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (idProperty != null)
            {
                var setMethod = idProperty.GetSetMethod(true);
                if (setMethod != null)
                {
                    setMethod.Invoke(element, new object[] { id });
                }
            }
        }
        #endregion
    }


}
