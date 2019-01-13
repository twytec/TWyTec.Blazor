using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class AccordionItem : IComponent
    {
        [Parameter]
        protected string Header { get; set; }

        public void Init(RenderHandle renderHandle)
        {
        }

        public void SetParameters(ParameterCollection parameters)
        {
        }
    }
}
