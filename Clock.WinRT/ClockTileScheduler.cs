namespace Clock.WinRT
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Windows.Data.Xml.Dom;
    using Windows.System.UserProfile;
    using Windows.UI.Notifications;
    using Windows.Storage;          // for ApplicationData

    public static class ClockTileScheduler
    {
        private static Windows.Foundation.Collections.IPropertySet appSettings;
        private const string dateKey = "dateKey";
        private const string nameKey = "nameKey";
        private static DateTime end_date;
        private static bool _countdownHasName;
        private static string _countdownName;

        public static string CountdownName
        {
            get { return _countdownName; }
            set { _countdownName = value; }
        }

        public static bool CountdownHasName
        {
            get { return _countdownHasName; }
            set { _countdownHasName = value; }
        }

        public static void CreateSchedule()
        {
            appSettings = ApplicationData.Current.LocalSettings.Values;

            if (appSettings.ContainsKey(nameKey))    // If a name is given 
            {
                CountdownHasName = true;
                CountdownName = appSettings[nameKey].ToString();
            }
            else
                CountdownHasName = false;


            #region Get the date if there is one
            if (appSettings.ContainsKey(dateKey))
            {

                string date = appSettings[dateKey].ToString();
                var tempArray = date.Split(' ');    // Results in  tempArray[0] = xx/xx/xx
                var dateArray = tempArray[0].Split('/');
                int month = Convert.ToInt32(dateArray[0]);
                int day = Convert.ToInt32(dateArray[1]);
                int year = Convert.ToInt32(dateArray[2]);
                end_date = new DateTime(year, month, day);
            }
            else
            {
                // We don't have a date saved.
            }
            #endregion 

            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            var plannedUpdated = tileUpdater.GetScheduledTileNotifications();

            string language = GlobalizationPreferences.Languages.First();
            CultureInfo cultureInfo = new CultureInfo(language);

            DateTime now = DateTime.Now;
            DateTime planTill = now.AddHours(4);

            DateTime updateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
            if (plannedUpdated.Count > 0)
                updateTime = plannedUpdated.Select(x => x.DeliveryTime.DateTime).Union(new [] { updateTime }).Max();

            const string xml = @"<tile><visual>
                                        <binding template=""TileSquareText01""><text id=""1"">{0} days</text><text id=""2"">until {1}!</text></binding>
                                        <binding template=""TileWideText01""><text id=""1"">{0} days</text><text id=""2"">until {1}!</text><text id=""3""></text><text id=""4""></text><text id=""5""></text></binding>
                                   </visual></tile>";

            var timeLeft = end_date - DateTime.Now;
            if (timeLeft.Days > 0)
            {
                var tileXmlNow = string.Format(xml, timeLeft.Days.ToString(), CountdownName);
                XmlDocument documentNow = new XmlDocument();
                documentNow.LoadXml(tileXmlNow);

                tileUpdater.Update(new TileNotification(documentNow) { ExpirationTime = now.AddMinutes(1) });


                for (var startPlanning = updateTime; startPlanning < planTill; startPlanning = startPlanning.AddMinutes(1))
                {
                    Debug.WriteLine(startPlanning);
                    Debug.WriteLine(planTill);

                    try
                    {
                        var tileXml = string.Format(xml, timeLeft.Days.ToString(), CountdownName);
                        XmlDocument document = new XmlDocument();
                        document.LoadXml(tileXml);

                        ScheduledTileNotification scheduledNotification = new ScheduledTileNotification(document, new DateTimeOffset(startPlanning)) { ExpirationTime = startPlanning.AddMinutes(1) };
                        tileUpdater.AddToSchedule(scheduledNotification);

                        Debug.WriteLine("schedule for: " + startPlanning);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("exception: " + e.Message);
                    }
                }
            }
        }
    }
}
