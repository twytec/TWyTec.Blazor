using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TWyTec.Blazor.Based
{
    public static class IHandleEvent
    {
        public static void HandleEvent(EventHandlerInvoker binding, UIEventArgs args, Action stateHasChanged)
        {
            var task = binding.Invoke(args);

            if (task.Status == TaskStatus.RanToCompletion)
            {
                stateHasChanged();
                return;
            }

            task.ContinueWith(t => {
                if (t.Exception != null)
                    HandlingException.HandleException(t.Exception);
                else
                    stateHasChanged();
            });
        }
    }
}
