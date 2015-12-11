using System;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace IGotRhythm.ServiceFabric.Instrumentation
{
    internal static class ExceptionUtility
    {
        internal delegate bool ExceptionHandlerDelegate(Exception exception, out ExceptionHandlingResult result);

        internal static bool HandleAggregateException(Exception exception, ExceptionHandlerDelegate exceptionHandler, out ExceptionHandlingResult result)
        {
            if (!(exception is AggregateException))
            {
                return exceptionHandler(exception, out result);
            }
            var ex = (AggregateException)exception;
            foreach (var current in ex.InnerExceptions)
            {
                if (HandleAggregateException(current, exceptionHandler, out result))
                {
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}