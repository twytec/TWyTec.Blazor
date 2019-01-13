using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class SplitViewContent : IComponent, IHandleEvent
    {
        bool rendererIsWorked = false;
        private RenderHandle _renderHandle;
        private IReadOnlyDictionary<string, object> _dict;
        private string _cssClass;
        RenderFragment _childContent;

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string ContentClass { get; set; }
        string _contentClass = "TWyTecSplitViewContent";

        public void Init(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        public void SetParameters(ParameterCollection p)
        {
            p.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            p.TryGetValue("class", out _cssClass);
            _contentClass = p.GetValueOrDefault(nameof(ContentClass), _contentClass);

            _dict = p.ToDictionary();
            StateHasChanged();
        }

        void IHandleEvent.HandleEvent(EventHandlerInvoker binding, UIEventArgs args)
                    => Based.IHandleEvent.HandleEvent(binding, args, StateHasChanged);

        private void StateHasChanged()
        {
            if (rendererIsWorked)
            {
                return;
            }

            rendererIsWorked = true;
            _renderHandle.Render(RenderTree);
        }

        private void RenderTree(RenderTreeBuilder builder)
        {
            int seqIndex = 0;

            builder.OpenElement(seqIndex++, "div");
            if (_cssClass == null)
                builder.AddAttribute(seqIndex, "class", $"{_contentClass}");
            else
                builder.AddAttribute(seqIndex, "class", $"{_contentClass} {_cssClass}");

            var anyAttr = _dict.Where(
                k => k.Key != RenderTreeBuilder.ChildContent && k.Key != "class" && k.Key != nameof(ContentClass));

            foreach (var item in anyAttr)
            {
                builder.AddAttribute(0, item.Key, item.Value);
            }

            builder.AddContent(seqIndex, _childContent);
            
            builder.CloseElement();

            rendererIsWorked = false;
        }
    }
}
