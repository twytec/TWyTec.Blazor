using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class StepperItem : IComponent
    {
        [Parameter]
        protected string Header { get; set; }

        [Parameter]
        protected string HeaderCompleted { get; set; }

        public void Init(RenderHandle renderHandle)
        {
        }

        public void SetParameters(ParameterCollection parameters)
        {
        }
    }
}
