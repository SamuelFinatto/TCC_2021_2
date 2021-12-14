using DataCommunicator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCommunicator.DTOs
{
    public class ModulesDTO
    {
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public ModulesType Type { get; set; }
        public bool Status { get; set; }
        public string Name { get; set; }
    }
}
