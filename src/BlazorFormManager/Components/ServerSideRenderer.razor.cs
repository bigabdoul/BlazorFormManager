using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorFormManager.Components
{
    /// <summary>
    /// Represents a component with server-side pre-rendering detection capabilities.
    /// </summary>
    /// <remarks>
    /// Only server-side rendered components should inherit from this one
    /// (i.e., @rendermode InteractiveServer).
    /// </remarks>
    public abstract class ServerSideRendererBase : ComponentBase
    {
        /// <summary>
        /// Gets or sets the component type to monitor.
        /// </summary>
#if NET6_0_OR_GREATER
        [EditorRequired]
#endif
        [Parameter] public virtual Type Component { get; set; } = default!;

        /// <summary>
        /// Gets or sets the child content to render after the component has been rendered.
        /// </summary>
        [Parameter] public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the fragment to render when pre-rendering.
        /// </summary>
        [Parameter] public RenderFragment? PreRenderContent { get; set; }

        /// <summary>
        /// Gets or sets the event callback to invoke when pre-rendering.
        /// </summary>
        [Parameter] public EventCallback<int> OnPreRendering { get; set; }

        /// <summary>
        /// Gest or sets the event callback to invoke when done rendering.
        /// </summary>
        [Parameter] public EventCallback OnRendered { get; set; }

        /// <summary>
        /// Gets the component type to monitor.
        /// </summary>
        public virtual Type ComponentType => Component;

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, int> State = new();

        private int InitCount
        {
            get => State.TryGetValue(ComponentType, out var value) ? value : 0;
            set
            {
                if (value <= int.MaxValue)
                {
                    State[ComponentType] = value;
                }
                else
                {
                    State[ComponentType] = 0;
                }
            }
        }

        /// <summary>
        /// Indicates whether the component is not pre-rendering.
        /// </summary>
        public bool IsNotPreRendering => InitCount != 0 && InitCount % 2 == 0;

        /// <summary>
        /// Indicates whether the component is pre-rendering.
        /// </summary>
        public bool IsPreRendering => InitCount % 2 == 1;

        /// <summary>
        /// Gets the number of times the component type identified 
        /// by <see cref="ComponentType"/> has been initialized.
        /// </summary>
        public int InitializationCount => InitCount;

        /// <inheritdoc/>
        /// <remarks>
        /// If this method is overridden and if detecting pre-rendering is 
        /// of any concern, it should be called before doing anything else.
        /// </remarks>
        protected override Task OnInitializedAsync()
        {
            if (ComponentType != null)
            {
                unchecked
                {
                    var count = InitCount;
                    count++;
                    InitCount = count;
                }
                if (IsPreRendering)
                {
                    return OnPreRenderingAsync(InitializationCount);
                }
            }
            return base.OnInitializedAsync();
        }

        /// <summary>
        /// Method invoked each time the component is pre-rendering.
        /// </summary>
        /// <param name="count">The number of times the component identified 
        /// by <see cref="ComponentType"/> has been pre-rendered.</param>
        /// <returns></returns>
        protected virtual Task OnPreRenderingAsync(int count)
            => OnPreRendering.HasDelegate ? OnPreRendering.InvokeAsync(count) : Task.CompletedTask;

        /// <inheritdoc/>
        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return firstRender ? OnRenderedAsync() : base.OnAfterRenderAsync(firstRender);
        }

        /// <summary>
        /// Method invoked after each time the component has been rendered for the first time.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnRenderedAsync()
        {
                if (OnRendered.HasDelegate)
#if NET6_0_OR_GREATER
                    await OnRendered.InvokeAsync();
#else
                    await OnRendered.InvokeAsync(this);
#endif
        }
    }

    /// <summary>
    /// Represents a <see cref="ServerSideRendererBase"/> for a component of type <typeparamref name="TComponent"/>.
    /// </summary>
    /// <typeparam name="TComponent">The component type to monitor.</typeparam>
    /// <remarks><typeparamref name="TComponent"/> usually represents the class that inherits this one.</remarks>
    public abstract class ServerSideRendererBase<TComponent> : ServerSideRendererBase where TComponent : IComponent
    {
        /// <inheritdoc/>
        public override Type ComponentType => typeof(TComponent);
    }
}
