using System;
/// <summary>
/// Extension to simplify an Event being raised
/// </summary>
public static class EventHandlerExtensions
{
    /// <summary>
    /// Safely invoke an event
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="evt"></param>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void SafeInvoke<T>(this EventHandler<T> evt, object sender, T e) where T : EventArgs
    {
        if (evt != null)
        {
            evt(sender, e);
        }
    }
}

