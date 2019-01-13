using System;
using System.Collections.Generic;
using System.Text;

namespace TWyTec.Blazor.Based
{
    public static class HandlingException
    {
        public static void HandleException(Exception ex)
        {
            if (ex is AggregateException && ex.InnerException != null)
            {
                ex = ex.InnerException; // It's more useful
            }
            Console.Error.WriteLine($"[{ex.GetType().FullName}] {ex.Message}\n{ex.StackTrace}");
        }
    }
}
