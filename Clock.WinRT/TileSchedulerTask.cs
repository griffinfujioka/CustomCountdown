namespace Clock.WinRT
{
    using System;
    using System.Linq;
    using Windows.ApplicationModel.Background;
    using Windows.UI.Notifications;

    public sealed class TileSchedulerTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            ClockTileScheduler.CreateSchedule();
            deferral.Complete();
        }
    }
}
