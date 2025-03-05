using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Pagebuilder
{
    public class SaveElementsRequest
    {
        public Guid PageId { get; set; }
        public List<BaseElementDTO> Elements { get; set; }
    }

}
