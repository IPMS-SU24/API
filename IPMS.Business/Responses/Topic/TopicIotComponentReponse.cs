using AutoFilterer.Types;
using IPMS.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.Topic
{
    public class TopicIotComponentReponse
    {
        public Guid Id { get; set; }
        public string TopicName { get; set; }
        public string Description { get; set; }
        public string? Detail { get; set; }
        public virtual ICollection<string> IotComponents { get; set; }  = new List<string>();
        
    }
}
