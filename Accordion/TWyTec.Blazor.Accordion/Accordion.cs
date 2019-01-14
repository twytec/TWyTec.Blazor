using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class Accordion : IComponent, IHandleEvent, IHandleAfterRender
    {
        private int _selectedIndex = 0;
        private bool setNewParameters = true;
        private List<AccordionTree> _accTrees;
        private bool rendererIsWorked = false;
        private RenderHandle _renderHandle;
        private IReadOnlyDictionary<string, object> _dict;
        private string _cssClass;
        private RenderFragment _childContent;

        private string _accordionClass = "TWyTecAccordion";
        private string _accordionItemClass = "TWyTecAccordionItem";
        private string _accordionBtnClass = "TWyTecAccordionItemButton";
        private string _accordionNavBtnActiveClass = "TWyTecAccordionItemButtonActive";
        private string _accordionContentClass = "TWyTecAccordionItemContent";
        private string _accordionContentAnimateClass = "TWyTecAccordionContentTranslation";

        public async void ChangeSelectedIndex(int index)
        {
            if (index < 0)
                return;
            else if (index >= _accTrees.Count)
                return;

            _selectedIndex = index;
            var item = _accTrees[index];
            item.Height = await JSRuntime.Current.InvokeAsync<double>("twytecAccordionGetPanelHeight", item.Id);
            StateHasChanged();
        }

        public int GetSelectedIndex()
            => _selectedIndex;

        #region Propertys

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string AccordionClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string AccordionItemClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string AccordionItemButtonClass { get; set; }
        
        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string AccordionItemButtonActiveClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        string AccordionItemContentClass { get; set; }

        [Parameter]
        int SelectedIndex { get; set; }

        #endregion

        #region GetAccContentId

        Dictionary<int, string> accContentIds;

        internal string GetAccContentId(int index)
        {
            if (accContentIds == null)
                accContentIds = new Dictionary<int, string>();

            if (accContentIds.ContainsKey(index))
                return accContentIds[index];
            else
            {
                var id = Guid.NewGuid().ToString();
                accContentIds.Add(index, id);
                return id;
            }
        }

        #endregion

        internal void StateHasChanged()
        {
            if (rendererIsWorked)
            {
                return;
            }

            rendererIsWorked = true;
            _renderHandle.Render(RenderTree);
        }

        void IComponent.Init(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        void IComponent.SetParameters(ParameterCollection p)
        {
            setNewParameters = true;
            p.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            p.TryGetValue("class", out _cssClass);

            _accordionClass = p.GetValueOrDefault(nameof(AccordionClass), _accordionClass);
            _accordionItemClass = p.GetValueOrDefault(nameof(AccordionItemClass), _accordionItemClass);
            _accordionBtnClass = p.GetValueOrDefault(nameof(AccordionItemButtonClass), _accordionBtnClass);
            _accordionNavBtnActiveClass = p.GetValueOrDefault(nameof(AccordionItemButtonActiveClass), _accordionNavBtnActiveClass);
            _accordionContentClass = p.GetValueOrDefault(nameof(AccordionItemContentClass), _accordionContentClass);
            _selectedIndex = p.GetValueOrDefault(nameof(SelectedIndex), _selectedIndex);

            _dict = p.ToDictionary();
            _renderHandle.Render(CreateTree);
        }

        void IHandleEvent.HandleEvent(EventHandlerInvoker binding, UIEventArgs args)
        {
            var task = binding.Invoke(args);

            if (task.Status == TaskStatus.RanToCompletion)
            {
                StateHasChanged();
                return;
            }

            task.ContinueWith(t => {
                if (t.Exception != null)
                    HandleException(t.Exception);
                else
                    StateHasChanged();
            });
        }

        #region Tree

        int seqIndex;
        
        #region CreateTree
        
        private void CreateTree(RenderTreeBuilder builder)
        {
            _accTrees = new List<AccordionTree>();
            ExploreTree(builder);
            StateHasChanged();
        }

        private void ExploreTree(RenderTreeBuilder builder)
        {
            builder.Clear();
            _childContent(builder);
            var frames = builder.GetFrames().ToArray();

            int si = 0;

            for (int i = 0; i < frames.Count(); i++)
            {
                var frame = frames[i];

                if (frame.FrameType == RenderTreeFrameType.Component && frame.ComponentType == typeof(AccordionItem))
                {
                    var accTree = new AccordionTree(this, si);

                    for (int f = 0; f < frame.ComponentSubtreeLength; f++)
                    {
                        i++;
                        var nextFrame = frames[i];

                        if (nextFrame.FrameType == RenderTreeFrameType.Attribute && nextFrame.AttributeName == "Header")
                        {
                            accTree.Header = nextFrame.AttributeValue.ToString();
                        }
                        else if (nextFrame.FrameType == RenderTreeFrameType.Attribute && nextFrame.AttributeName == RenderTreeBuilder.ChildContent)
                        {
                            if (nextFrame.AttributeValue is RenderFragment nextChild)
                            {
                                accTree.Child = nextChild;
                            }
                        }
                        else if (nextFrame.FrameType == RenderTreeFrameType.Attribute)
                        {
                            accTree.AnyAttrDict.Add(nextFrame.AttributeName, nextFrame.AttributeValue);
                        }
                    }

                    _accTrees.Add(accTree);
                    si++;
                }
            }
        }

        #endregion

        #region RenderTree

        private void RenderTree(RenderTreeBuilder builder)
        {
            builder.Clear();
            seqIndex = 0;

            RenderContent(builder);

            rendererIsWorked = false;
        }

        private void RenderContent(RenderTreeBuilder builder)
        {
            builder.OpenElement(seqIndex++, "div");
            if (_cssClass == null)
                builder.AddAttribute(seqIndex, "class", _accordionClass);
            else
                builder.AddAttribute(seqIndex, "class", $"{_accordionClass} {_cssClass}");

            var anyAttr = _dict.Where(
                k => k.Key != RenderTreeBuilder.ChildContent && k.Key != "class" && 
                k.Key != nameof(AccordionClass) &&
                k.Key != nameof(AccordionItemClass) &&
                k.Key != nameof(AccordionItemButtonClass) &&
                k.Key != nameof(AccordionItemButtonActiveClass) &&
                k.Key != nameof(AccordionItemContentClass) &&
                k.Key != nameof(SelectedIndex));

            foreach (var item in anyAttr)
            {
                builder.AddAttribute(seqIndex, item.Key, item.Value);
            }

            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "role", "tablist");

            foreach (var item in _accTrees)
            {
                Action onclick = item.OnClick;

                builder.OpenElement(seqIndex++, "div");

                if (item.AnyAttrDict.ContainsKey("class"))
                    builder.AddAttribute(seqIndex, "class", $"{_accordionItemClass} {item.AnyAttrDict["class"]}");
                else
                    builder.AddAttribute(seqIndex, "class", $"{_accordionItemClass}");

                foreach (var a in item.AnyAttrDict)
                {
                    builder.AddAttribute(seqIndex, a.Key, a.Value);
                }

                builder.OpenElement(seqIndex++, "button");
                builder.AddAttribute(seqIndex, "role", "tab");
                builder.AddAttribute(seqIndex, "aria-controls", item.Id);
                builder.AddAttribute(seqIndex, "onclick", onclick);
                
                if (item.Index == _selectedIndex)
                {
                    builder.AddAttribute(seqIndex, "class", $"{_accordionBtnClass} {_accordionNavBtnActiveClass}");
                    builder.AddAttribute(seqIndex, "aria-expanded", "true");
                }
                else
                {
                    builder.AddAttribute(seqIndex, "class", $"{_accordionBtnClass}");
                    builder.AddAttribute(seqIndex, "aria-expanded", "false");
                }


                builder.AddContent(seqIndex, item.Header);
                builder.CloseElement();

                builder.OpenElement(seqIndex++, "div");
                builder.AddAttribute(seqIndex, "role", "tabpanel");
                builder.AddAttribute(seqIndex, "aria-labelledby", item.Header);
                builder.AddAttribute(seqIndex, "id", item.Id);
                builder.AddAttribute(seqIndex, "data-AnimateClass", $"{_accordionContentAnimateClass}");
                builder.AddAttribute(seqIndex, "class", $"{_accordionContentClass} {_accordionContentAnimateClass}");

                if (item.Index == _selectedIndex)
                {
                    if (item.Height > 0)
                        builder.AddAttribute(seqIndex, "style", $"max-height: {item.Height}px;");
                    else
                        builder.AddAttribute(seqIndex, "style", $"max-height: 300px;");
                }
                else
                    builder.AddAttribute(seqIndex, "style", $"max-height: 0px;");

                builder.OpenElement(seqIndex++, "div");
                builder.AddAttribute(seqIndex, "style", "margin: 10px;");
                builder.AddContent(seqIndex, item.Child);
                builder.CloseElement();

                builder.CloseElement();

                builder.CloseElement();
            }

            builder.CloseElement();
            builder.CloseElement();
        }

        #endregion

        #endregion
        
        public void OnAfterRender()
        {
            if (setNewParameters)
            {
                setNewParameters = false;
                Task.Run(() => SetPaneHeightSelectedItem());
            }
        }

        async void SetPaneHeightSelectedItem()
        {
            var item = _accTrees[_selectedIndex];
            await JSRuntime.Current.InvokeAsync<bool>("twytecAccordionSetPanelHeight", item.Id);
        }

        #region Helper

        public static void HandleException(Exception ex)
        {
            if (ex is AggregateException && ex.InnerException != null)
            {
                ex = ex.InnerException; // It's more useful
            }
            Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
        }

        #endregion
    }

    internal class AccordionTree
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public double Height { get; set; }
        public string Header { get; set; }
        public RenderFragment Child { get; set; }
        public Dictionary<string, object> AnyAttrDict;

        private Accordion _accordion;

        public AccordionTree(Accordion accordion, int index)
        {
            Index = index;
            _accordion = accordion;
            Id = _accordion.GetAccContentId(Index);
            AnyAttrDict = new Dictionary<string, object>();
        }

        public void OnClick()
        {
            _accordion.ChangeSelectedIndex(Index);
        }
    }
}
