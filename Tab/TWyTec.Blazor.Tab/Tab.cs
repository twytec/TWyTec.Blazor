using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TWyTec.Blazor
{
    public class Tab : IComponent, IHandleEvent
    {
        List<TabTree> _tabTrees;
        private bool rendererIsWorked = false;
        private RenderHandle _renderHandle;
        private IReadOnlyDictionary<string, object> _dict;
        private string _cssClass;
        private string _cssStyle;
        private RenderFragment _childContent;

        private int _selectedIndex = 0;
        private string _tabClass = "TWyTecTab";
        private string _tabNavClass = "TWyTecTabNav";
        private string _tabNavBtnClass = "TWyTecTabNavBtn";
        private string _tabNavBtnActiveClass = "TWyTecTabNavBtnActive";
        private string _tabContentClass = "TWyTecTabContent";
        private string _tabContentItemClass = "TWyTecTabContentItem";
        private string _tabContentItemActiveClass = "TWyTecTabContentItemActive";
        private bool _tabNavBtnStretch = false;
        private TabNavPosition _tabNavPosition;

        #region Propertys

        public int SelectedIndex { get; set; }

        /// <summary>
        /// Top or Bottom. Default is Top
        /// </summary>
        [Parameter]
        protected TabNavPosition TabNavPosition { get; set; }
        
        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string TabClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string TabNavClass { get; set; }

        /// <summary>
        /// Default is false
        /// </summary>
        [Parameter]
        protected bool TabNavBtnStretch { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string TabNavBtnClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string TabNavBtnActiveClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string TabContentClass { get; set; }

        #endregion

        #region public Methods

        public void ChangeSelectedIndex(int index)
        {
            if (index < 0)
                return;
            else if (index >= _tabTrees.Count)
                return;

            _selectedIndex = index;
            StateHasChanged();
        }

        public int GetSelectedIndex()
            => _selectedIndex;

        #endregion

        #region GetTabContentId

        Dictionary<int, string> tabContentIds;

        internal string GetTabContentId(int index)
        {
            if (tabContentIds == null)
                tabContentIds = new Dictionary<int, string>();

            if (tabContentIds.ContainsKey(index))
                return tabContentIds[index];
            else
            {
                var id = Guid.NewGuid().ToString();
                tabContentIds.Add(index, id);
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
            p.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);
            p.TryGetValue("class", out _cssClass);

            _tabNavPosition = p.GetValueOrDefault(nameof(TabNavPosition), TabNavPosition.Top);

            if (_tabNavPosition == TabNavPosition.Bottom)
            {
                _tabNavClass = "TWyTecTabNavBottom";
                _tabNavBtnActiveClass = "TWyTecTabNavBottomBtnActive";
                _tabNavBtnClass = "TWyTecTabNavBottomBtn";
            }

            _tabClass = p.GetValueOrDefault(nameof(TabClass), _tabClass);
            _tabNavClass = p.GetValueOrDefault(nameof(TabNavClass), _tabNavClass);
            _tabNavBtnClass = p.GetValueOrDefault(nameof(TabNavBtnClass), _tabNavBtnClass);
            _tabNavBtnActiveClass = p.GetValueOrDefault(nameof(TabNavBtnActiveClass), _tabNavBtnActiveClass);
            _tabContentClass = p.GetValueOrDefault(nameof(TabContentClass), _tabContentClass);
            _selectedIndex = p.GetValueOrDefault(nameof(SelectedIndex), _selectedIndex);
            _tabNavBtnStretch = p.GetValueOrDefault(nameof(TabNavBtnStretch), _tabNavBtnStretch);

            _dict = p.ToDictionary();
            _renderHandle.Render(CreateTree);
        }

        void IHandleEvent.HandleEvent(EventHandlerInvoker binding, UIEventArgs args)
            => Based.IHandleEvent.HandleEvent(binding, args, StateHasChanged);

        #region Tree

        int seqIndex;

        #region CreateTree

        private void CreateTree(RenderTreeBuilder builder)
        {
            _tabTrees = new List<TabTree>();
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

                if (frame.FrameType == RenderTreeFrameType.Component && frame.ComponentType == typeof(TabItem))
                {
                    var tabTree = new TabTree(this, si);

                    for (int f = 0; f < frame.ComponentSubtreeLength; f++)
                    {
                        i++;
                        var nextFrame = frames[i];

                        if (nextFrame.FrameType == RenderTreeFrameType.Attribute && nextFrame.AttributeName == "Header")
                        {
                            tabTree.Header = nextFrame.AttributeValue.ToString();
                        }
                        else if (nextFrame.FrameType == RenderTreeFrameType.Attribute && nextFrame.AttributeName == RenderTreeBuilder.ChildContent)
                        {
                            if (nextFrame.AttributeValue is RenderFragment nextChild)
                            {
                                tabTree.Child = nextChild;
                            }
                        }
                        else if (nextFrame.FrameType == RenderTreeFrameType.Attribute)
                        {
                            tabTree.AnyAttrDict.Add(nextFrame.AttributeName, nextFrame.AttributeValue);
                        }
                    }

                    _tabTrees.Add(tabTree);
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

            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "class", $"{_tabClass} {_cssClass}");

            var anyAttr = _dict.Where(
                k => k.Key != RenderTreeBuilder.ChildContent && k.Key != "class" &&
                k.Key != nameof(TabClass) &&
                k.Key != nameof(TabNavClass) &&
                k.Key != nameof(TabNavBtnClass) &&
                k.Key != nameof(TabNavBtnActiveClass) &&
                k.Key != nameof(TabContentClass) &&
                k.Key != nameof(TabNavBtnStretch) &&
                k.Key != nameof(TabNavPosition) &&
                k.Key != nameof(SelectedIndex));

            foreach (var item in anyAttr)
            {
                builder.AddAttribute(seqIndex, item.Key, item.Value);
            }

            if (_tabNavPosition == TabNavPosition.Top)
            {
                RenderNav(builder);
                RenderContent(builder);
            }
            else if (_tabNavPosition == TabNavPosition.Bottom)
            {
                RenderContent(builder);
                RenderNav(builder);
            }

            builder.CloseElement();

            rendererIsWorked = false;
        }

        private void RenderNav(RenderTreeBuilder builder)
        {
            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "class", $"{_tabNavClass}");

            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "role", "tablist");
            builder.AddAttribute(seqIndex, "style", "height: 100%;");

            var btnWidth = 100d / _tabTrees.Count;

            foreach (var item in _tabTrees)
            {
                Action onclick = item.OnClick;

                builder.OpenElement(seqIndex++, "button");
                builder.AddAttribute(seqIndex, "role", "tab");
                builder.AddAttribute(seqIndex, "aria-controls", item.Id);

                if (_tabNavBtnStretch)
                    builder.AddAttribute(seqIndex, "style", $"width: {btnWidth}%");

                builder.AddAttribute(seqIndex, "onclick", onclick);

                if (item.Index == _selectedIndex)
                {
                    builder.AddAttribute(seqIndex, "class", $"{_tabNavBtnClass} {_tabNavBtnActiveClass}");
                    builder.AddAttribute(seqIndex, "aria-expanded", "true");
                }
                else
                {
                    builder.AddAttribute(seqIndex, "class", _tabNavBtnClass);
                    builder.AddAttribute(seqIndex, "aria-expanded", "false");
                }

                builder.AddContent(seqIndex, item.Header);

                builder.CloseElement();
            }

            builder.CloseElement();
            builder.CloseElement();
        }

        private void RenderContent(RenderTreeBuilder builder)
        {
            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "class", _tabContentClass);

            foreach (var item in _tabTrees)
            {
                builder.OpenElement(seqIndex++, "div");
                builder.AddAttribute(seqIndex, "role", "tabpanel");
                builder.AddAttribute(seqIndex, "aria-labelledby", item.Header);
                builder.AddAttribute(seqIndex, "id", item.Id);

                if (item.Index == _selectedIndex)
                {
                    builder.AddAttribute(seqIndex, "class", $"{_tabContentItemClass} {_tabContentItemActiveClass}");
                }
                else
                {
                    builder.AddAttribute(seqIndex, "class", $"{_tabContentItemClass}");
                }

                if (item.Index == _selectedIndex)
                    builder.AddAttribute(seqIndex, "style", $"transform: translate(0%, 0px); min-height: 1px;");
                else if (item.Index > _selectedIndex)
                    builder.AddAttribute(seqIndex, "style", $"transform: translate(100%, 0px); min-height: 1px;");
                else if (item.Index < _selectedIndex)
                    builder.AddAttribute(seqIndex, "style", $"transform: translate(-100%, 0px); min-height: 1px;");

                builder.AddContent(seqIndex, item.Child);
                
                builder.CloseElement();
            }

            builder.CloseElement();
        }

        #endregion

        #endregion
    }

    internal class TabTree
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public double Height { get; set; }
        public string Header { get; set; }
        public RenderFragment Child { get; set; }
        public Dictionary<string, object> AnyAttrDict;

        private Tab _tab;

        public TabTree(Tab tab, int index)
        {
            Index = index;
            _tab = tab;
            Id = tab.GetTabContentId(index);
            AnyAttrDict = new Dictionary<string, object>();
        }

        public void OnClick()
        {
            _tab.ChangeSelectedIndex(Index);
        }
    }
}
