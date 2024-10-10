using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace proj_minimal_api.Domain.ModelViews
{
    public class ValidationError
    {
        public ValidationError()
        {

            Messages = new List<string>();
        }

        public List<string> Messages { get; set; }
    }
}