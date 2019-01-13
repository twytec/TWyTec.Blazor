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
    public class Affix : IComponent, IHandleEvent, IHandleAfterRender
    {
        internal int _selectedIndex = 0;
        internal List<AffixTree> _affixTrees;
        internal int _animateDuration = 300;

        private string _affixClass = "TWyTecAffix";
        private string _affixPaneClass = "TWyTecAffixPane";
        private string _affixPaneBtnClass = "TWyTecAffixPaneButton";
        private string _affixPaneBtnActiveClass = "TWyTecAffixPaneButtonActive";
        private string _affixContentClass = "TWyTecAffixContent";
        private string _affixContentItemClass = "TWyTecAffixContentItem";

        #region GoTo

        public void GoTo(int index)
        {
            if (index > -1 && index < _affixTrees.Count)
            {
                _affixTrees[index].GoTo();
            }

            if (_selectedIndex == _affixTrees.Count)
                _selectedIndex = _affixTrees.Count - 1;
            else if (_selectedIndex < 0)
                _selectedIndex = 0;
        }

        public void GoToNext()
        {
            GoTo(++_selectedIndex);
        }

        public void GoToLast()
        {
            GoTo(--_selectedIndex);
        }

        #endregion

        #region Protected Propertys

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string AffixClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string AffixPaneClass { get; set; }
        
        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string AffixPaneButtonClass { get; set; }
        
        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string AffixPaneButtonActiveClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string AffixContentClass { get; set; }

        /// <summary>
        /// CSS Class
        /// </summary>
        [Parameter]
        protected string AffixContentItemClass { get; set; }

        /// <summary>
        /// Duration in milliseconds. Default is 300
        /// </summary>
        [Parameter]
        protected int AnimateDuration { get; set; }

        /// <summary>
        /// Duration in milliseconds. Default is 300
        /// </summary>
        [Parameter]
        protected int SelectedIndex { get; set; }

        #endregion

        #region OnInit OnInitAsync OnAfterRender

        protected virtual void OnInit()
        {
        }

        protected virtual Task OnInitAsync()
            => null;

        protected virtual void OnAfterRender()
        {
        }

        #endregion

        public void StateHasChanged()
        {
            if (rendererIsWorked)
            {
                return;
            }

            rendererIsWorked = true;
            _renderHandle.Render(RenderTree);
        }

        #region Private

        private RenderHandle _renderHandle;
        private RenderFragment _childContent;
        private bool hasCalledInit = false;
        bool rendererIsWorked = false;
        IReadOnlyDictionary<string, object> _lastParameters = null;

        #region IComponent Init SetParameters

        void IComponent.Init(RenderHandle renderHandle)
        {
            _renderHandle = renderHandle;
        }

        void IComponent.SetParameters(ParameterCollection parameters)
        {
            if (hasCalledInit == false)
            {
                hasCalledInit = true;
                OnInit();

                var initTask = OnInitAsync();
                if (initTask != null && initTask.Status != TaskStatus.RanToCompletion)
                {
                    initTask.ContinueWith(ContinueAfterLifecycleTask);
                }
            }

            bool createTree = false;
            var lp = parameters.ToDictionary();

            if (_lastParameters != null && _lastParameters.Count == lp.Count)
            {
                for (int i = 0; i < _lastParameters.Count; i++)
                {
                    var p1 = lp.ElementAt(i);
                    var p2 = _lastParameters.ElementAt(i);
                    if (p1.Key != p2.Key || p1.Value.Equals(p2.Value) == false)
                    {
                        createTree = true;
                        break;
                    }
                }
            }
            else
                createTree = true;

            if (createTree)
            {
                parameters.TryGetValue(RenderTreeBuilder.ChildContent, out _childContent);

                _affixClass = parameters.GetValueOrDefault(nameof(AffixClass), _affixClass);
                _affixPaneClass = parameters.GetValueOrDefault(nameof(AffixPaneClass), _affixPaneClass);
                _affixPaneBtnClass = parameters.GetValueOrDefault(nameof(AffixPaneButtonClass), _affixPaneBtnClass);
                _affixPaneBtnActiveClass = parameters.GetValueOrDefault(nameof(AffixPaneButtonActiveClass), _affixPaneBtnActiveClass);
                _affixContentClass = parameters.GetValueOrDefault(nameof(AffixContentClass), _affixContentClass);
                _affixContentItemClass = parameters.GetValueOrDefault(nameof(AffixContentItemClass), _affixContentItemClass);
                _animateDuration = parameters.GetValueOrDefault(nameof(AnimateDuration), _animateDuration);
                _selectedIndex = parameters.GetValueOrDefault(nameof(SelectedIndex), _selectedIndex);

                _lastParameters = lp;
                _renderHandle.Render(CreateTree);
            }
        }

        #endregion

        #region IHandleEvent

        void IHandleEvent.HandleEvent(EventHandlerInvoker binding, UIEventArgs args)
        {
            var task = binding.Invoke(args);

            if (task.Status == TaskStatus.RanToCompletion)
            {
                return;
            }

            task.ContinueWith(ContinueAfterLifecycleTask);
        }

        #endregion

        #region IHandleAfterRender

        void IHandleAfterRender.OnAfterRender()
        {
            JSRuntime.Current.InvokeAsync<bool>("twytecAffixAfterRender");
            OnAfterRender();
        }

        #endregion

        #region Tree

        int seqIndex;
        int si = 0;

        #region CreateTree

        private void CreateTree(RenderTreeBuilder builder)
        {
            _affixTrees = new List<AffixTree>();
            ExploreTree(builder);
            StateHasChanged();
        }

        private void ExploreTree(RenderTreeBuilder builder)
        {
            builder.Clear();
            _childContent(builder);
            var frames = builder.GetFrames().ToArray();

            for (int i = 0; i < frames.Count(); i++)
            {
                var frame = frames[i];

                if (frame.FrameType == RenderTreeFrameType.Component && frame.ComponentType == typeof(AffixItem))
                {
                    var affixTree = new AffixTree(this)
                    {
                        Index = si++
                    };

                    for (int f = 0; f < frame.ComponentSubtreeLength; f++)
                    {
                        i++;
                        var nextFrame = frames[i];

                        if (nextFrame.FrameType == RenderTreeFrameType.Attribute && nextFrame.AttributeName == "Header")
                        {
                            affixTree.Header = nextFrame.AttributeValue.ToString();
                        }
                        else if (nextFrame.FrameType == RenderTreeFrameType.Attribute && nextFrame.AttributeName == RenderTreeBuilder.ChildContent)
                        {
                            if (nextFrame.AttributeValue is RenderFragment nextChild)
                            {
                                ExploreAffixTreeChild(builder, nextChild, affixTree);
                            }
                        }
                    }

                    _affixTrees.Add(affixTree);
                }
            }
        }

        private void ExploreAffixTreeChild(RenderTreeBuilder builder, RenderFragment child, AffixTree affixTree)
        {
            builder.Clear();
            child(builder);
            var frames = builder.GetFrames().ToArray();

            for (int i = 0; i < frames.Count(); i++)
            {
                var frame = frames[i];

                if (frame.FrameType == RenderTreeFrameType.Component && frame.ComponentType == typeof(AffixItem))
                {
                    var nextAffixTree = new AffixTree(this)
                    {
                        Index = si++
                    };

                    for (int f = 0; f < frame.ComponentSubtreeLength; f++)
                    {
                        i++;
                        var nextFrame = frames[i];

                        if (nextFrame.FrameType == RenderTreeFrameType.Attribute && nextFrame.AttributeName == "Header")
                        {
                            nextAffixTree.Header = nextFrame.AttributeValue.ToString();
                        }
                        else if (nextFrame.FrameType == RenderTreeFrameType.Attribute && nextFrame.AttributeName == RenderTreeBuilder.ChildContent)
                        {
                            if (nextFrame.AttributeValue is RenderFragment nextChild)
                            {
                                ExploreAffixTreeChild(builder, nextChild, nextAffixTree);
                            }
                        }
                    }

                    affixTree.Tree.Add(nextAffixTree);
                }
                else
                {
                    affixTree.Frames.Add(frame);
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
            builder.AddAttribute(seqIndex, "class", _affixClass);
            builder.AddAttribute(seqIndex, "name", "TWyTecAffix");

            RenderContent(builder);
            RenderPane(builder);
            
            builder.CloseElement();

            rendererIsWorked = false;
        }

        #region RenderPane

        void RenderPane(RenderTreeBuilder builder)
        {
            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "class", _affixPaneClass);

            builder.OpenElement(seqIndex++, "ul");

            foreach (var item in _affixTrees)
            {
                RenderPaneTree(builder, item);
            }

            builder.CloseElement();
            builder.CloseElement();
        }

        void RenderPaneTree(RenderTreeBuilder builder, AffixTree affixTree)
        {
            builder.OpenElement(seqIndex++, "li");

            builder.OpenElement(seqIndex++, "button");
            builder.AddAttribute(seqIndex, "class", _affixPaneBtnClass);
            builder.AddAttribute(seqIndex, "aria-controls", affixTree.Id);
            builder.AddAttribute(seqIndex, "id", $"btn-{affixTree.Id}");
            builder.AddAttribute(seqIndex, "data-activeclass", _affixPaneBtnActiveClass);
            builder.AddAttribute(seqIndex, "onclick", $"twytecBtnAffixPaneClick('{affixTree.Id}')");
            builder.AddContent(seqIndex, affixTree.Header);
            builder.CloseElement();

            if (affixTree.Tree.Count > 0)
            {
                builder.OpenElement(seqIndex++, "ul");
                builder.AddAttribute(seqIndex, "class", $"TWyTecAffixPaneItemHidden");

                foreach (var item in affixTree.Tree)
                {
                    RenderPaneTree(builder, item);
                }

                builder.CloseElement();
            }

            
            builder.CloseElement();
        }

        #endregion

        #region RenderContent

        void RenderContent(RenderTreeBuilder builder)
        {
            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "name", "TWyTecAffixContent");
            builder.AddAttribute(seqIndex, "class", _affixContentClass);

            foreach (var item in _affixTrees)
            {
                RenderContentTree(builder, item);
            }

            builder.CloseElement();
        }

        void RenderContentTree(RenderTreeBuilder builder, AffixTree affixTree)
        {
            builder.OpenElement(seqIndex++, "div");
            builder.AddAttribute(seqIndex, "id", affixTree.Id);
            builder.AddAttribute(seqIndex, "class", _affixContentItemClass);
            builder.AddAttribute(seqIndex, "name", "AffixContentItem");

            if (affixTree.Frames.Count > 0)
            {
                for (int i = 0; i < affixTree.Frames.Count; i++)
                {
                    var frame = affixTree.Frames[i];
                    i = AddFrame(builder, affixTree, frame, i);
                }
            }
            if (affixTree.Tree.Count > 0)
            {
                foreach (var item in affixTree.Tree)
                {
                    RenderContentTree(builder, item);
                }
            }

            builder.CloseElement();
        }

        int AddFrame(RenderTreeBuilder builder, AffixTree affixTree, RenderTreeFrame frame, int i)
        {
            if (frame.FrameType == RenderTreeFrameType.Component || frame.FrameType == RenderTreeFrameType.Element || frame.FrameType == RenderTreeFrameType.Region)
            {
                int sc = seqIndex++;
                int count = 0;

                if (frame.FrameType == RenderTreeFrameType.Component)
                {
                    count = frame.ComponentSubtreeLength;
                    builder.OpenComponent(sc, frame.ComponentType);

                }
                else if (frame.FrameType == RenderTreeFrameType.Element)
                {
                    count = frame.ElementSubtreeLength;
                    builder.OpenElement(sc, frame.ElementName);
                }
                else
                {
                    count = frame.RegionSubtreeLength;
                }


                if (count != 0)
                {
                    for (int ii = 0; ii < count; ii++)
                    {
                        i++;
                        if (i == affixTree.Frames.Count)
                        {
                            Console.WriteLine("Error TreeBuilder > AddFrame loop index == Frames count");
                            break;
                        }

                        var nf = affixTree.Frames[i];

                        if (
                            nf.FrameType == RenderTreeFrameType.Component ||
                            nf.FrameType == RenderTreeFrameType.Element ||
                            nf.FrameType == RenderTreeFrameType.Region)
                        {
                            int ai = AddFrame(builder, affixTree, nf, i);
                            ii += ai - i;
                            i = ai;
                        }
                        else
                        {
                            AddContent(builder, nf, sc);
                        }
                    }
                }

                if (frame.FrameType == RenderTreeFrameType.Component)
                {
                    builder.CloseComponent();
                }
                else if (frame.FrameType == RenderTreeFrameType.Element)
                {
                    builder.CloseElement();
                }

                return i;
            }
            else
            {
                AddContent(builder, frame, seqIndex);
                return i;
            }
        }

        void AddContent(RenderTreeBuilder builder, RenderTreeFrame frame, int sc)
        {
            if (frame.FrameType == RenderTreeFrameType.Attribute)
            {
                builder.AddAttribute(sc, frame);
            }
            else if (frame.FrameType == RenderTreeFrameType.Markup)
            {
                builder.AddMarkupContent(sc, frame.MarkupContent);
            }
            else if (frame.FrameType == RenderTreeFrameType.Text)
            {
                builder.AddContent(sc, frame.TextContent);
            }
            else if (frame.FrameType == RenderTreeFrameType.ComponentReferenceCapture)
            {
                builder.AddComponentReferenceCapture(sc, frame.ComponentReferenceCaptureAction);
            }
            else if (frame.FrameType == RenderTreeFrameType.ElementReferenceCapture)
            {
                builder.AddElementReferenceCapture(sc, frame.ElementReferenceCaptureAction);
            }
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #region Helper

        private void ContinueAfterLifecycleTask(Task task)
        {
            if (task.Exception == null)
            {
                StateHasChanged();
            }
        }

        #endregion
    }

    internal class AffixTree
    {
        public string Id { get; set; }
        public string Header { get; set; }
        public int Index { get; set; }
        public List<RenderTreeFrame> Frames { get; set; }
        public List<AffixTree> Tree { get; set; }


        public AffixTree(Affix affix)
        {
            Id = Guid.NewGuid().ToString();
            Frames = new List<RenderTreeFrame>();
            Tree = new List<AffixTree>();
        }

        public async void GoTo()
        {
            await JSRuntime.Current.InvokeAsync<bool>("twytecBtnAffixPaneClick", Id);
        }
    }
}
