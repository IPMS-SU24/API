using AutoFilterer.Types;
using IPMS.DataAccess.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.Topic
{
    public class SuggestedTopicsResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
    }
}
