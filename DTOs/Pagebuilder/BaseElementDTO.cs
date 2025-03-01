using System;
using System.Collections.Generic;

namespace DTOs.Pagebuilder
{
    public class BaseElementDTO
    {
        public Guid Id { get; set; }
        public Guid ToolId { get; set; }
        public int Order { get; set; }
        public TemplateBodyDTO TemplateBody { get; set; } // Assuming TemplateBody can be represented as a string
        public AssetDTO Asset { get; set; } // Assuming Asset can be represented as a string
    }
}
