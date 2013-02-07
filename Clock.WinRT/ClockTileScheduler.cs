

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
        static int Live_Tile_Style = 1;  // Used to change which live tile style is shown 
        private static Windows.Foundation.Collections.IPropertySet appSettings;
        private const string dateKey = "dateKey";
        private const string nameKey = "nameKey"; 
        private static DateTime graduation_date;
        private static bool _babyHasName;
        private static string _babyName;

        public static string babyName
        {
            get { return _babyName; }
            set { _babyName = value; }
        }

        public static bool babyHasName
        {
            get { return _babyHasName; }
            set { _babyHasName = value; }
        }


        public static void SetLiveTileStyle(int style)
        {
            Live_Tile_Style = style;
        }
        public static void CreateSchedule()
        {
            appSettings = ApplicationData.Current.LocalSettings.Values;

            if (appSettings.ContainsKey(nameKey))    // If a name is given 
            {
                babyHasName = true;
                babyName = appSettings[nameKey].ToString();
            }
            else
                babyHasName = false; 


            #region Show the date PopUp if there isn't a saved date
            if (!appSettings.ContainsKey(dateKey))
            {
                // No date is registered 
                //graduation_date = new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, DateTime.Now.Day); 
            }
            else
            {
                
                string date = appSettings[dateKey].ToString();
                var tempArray = date.Split(' ');    // Results in  tempArray[0] = xx/xx/xx
                var dateArray = tempArray[0].Split('/');
                int month = Convert.ToInt32(dateArray[0]);
                int day = Convert.ToInt32(dateArray[1]);
                int year = Convert.ToInt32(dateArray[2]);
                graduation_date = new DateTime(year, month, day);
                //Clock.WinRT.ClockTileScheduler.SetGraduationDate(year, month, day); 
            }
            #endregion 
            int CurrentYear = DateTime.Now.Year;                // Current year
            DateTime NewYear = new DateTime(DateTime.Now.Year + 1, 1, 1);       // January 1st of the next year

            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            var plannedUpdated = tileUpdater.GetScheduledTileNotifications();

            string language = GlobalizationPreferences.Languages.First();
            CultureInfo cultureInfo = new CultureInfo(language);

            DateTime now = DateTime.Now;
            DateTime planTill = now.AddHours(4);



            DateTime updateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);
            if (plannedUpdated.Count > 0)
                updateTime = plannedUpdated.Select(x => x.DeliveryTime.DateTime).Union(new[] { updateTime }).Max();

            // Here is where I define a number of different live tiles 
            // This is the special tile that will display on Christmas day 
            const string Christmas_xml = @"<tile><visual>
                                        <binding template=""TileSquareText04""><text id=""1"">Merry Christmas!</text></binding>
                                        <binding template=""TileWideText03""><text id=""1"">Merry Christmas!</text></binding>
                                </visual></tile>";



            // Tile 1: XX days until Christmas!
            string xml = @"<tile><visual>
                                        <binding template=""TileSquareText01""><text id=""1"">{0}</text><text id=""2"">until the baby!</text></binding>
                                        <binding template=""TileWideText01""><text id=""1"">{0}</text><text id=""2"">until the baby!</text></binding>
                                </visual></tile>";

            // Adjust the live tile xml if the baby name is provided 
            if (babyHasName)
            {
                xml = @"<tile><visual>
                                        <binding template=""TileSquareText01""><text id=""1"">{0}</text><text id=""2"">until {1}!</text></binding>
                                        <binding template=""TileWideText01""><text id=""1"">{0}</text><text id=""2"">until {1}!</text></binding>
                                </visual></tile>";
            }
            else
            {
                xml = @"<tile><visual>
                                        <binding template=""TileSquareText01""><text id=""1"">{0}</text><text id=""2"">until the baby!</text></binding>
                                        <binding template=""TileWideText01""><text id=""1"">{0}</text><text id=""2"">until the baby!</text></binding>
                                </visual></tile>";
            }

            // Tile 2: XX days, YY hours until Christmas! 
            const string xml_2 = @"<tile><visual>
                                        <binding template=""TileSquareText01"">
                                            <text id=""1"">{0}</text>
                                            <text id=""2"">{1}</text>
                                            <text id=""3"">{2}</text>
                                        </binding>
                                        <binding template=""TileWideText05"">
                                            <text id=""1"">{0}</text>
                                            <text id=""2"">{1}</text>
                                            <text id=""3"">{2}</text>
                                        </binding>
                                </visual></tile>";

            // Tile 3: XX days, YY hours, ZZ minutes until Christmas!
            const string xml_3 = @"<tile><visual>
                                        <binding template=""TileSquareText01"">
                                            <text id=""1"">{0}</text>
                                            <text id=""2"">{1}</text>
                                            <text id=""3"">{2}</text>
                                            <text id=""4"">until Christmas!</text>
                                        </binding>
                                        <binding template=""TileWideText05"">
                                            <text id=""1"">{0}</text>
                                            <text id=""2"">{1}</text>
                                            <text id=""3"">{2}</text>
                                            <text id=""4"">until Christmas!</text>
                                        </binding>
                                </visual></tile>";

          
            var timeLeft = graduation_date - DateTime.Now;
            var tileXmlCountdown = "";
            if (Live_Tile_Style == 1)
            {
                if (babyHasName && appSettings[nameKey].ToString() != "")
                {
                    tileXmlCountdown = string.Format(xml, timeLeft.Days.ToString() + " days", babyName);
                }
                else
                {
                    tileXmlCountdown = string.Format(xml, timeLeft.Days.ToString() + " days until the big event");
                }
            }
            else if (Live_Tile_Style == 2)
            {
                tileXmlCountdown = string.Format(xml_2, timeLeft.Days.ToString() + " days,", timeLeft.Hours == 1 ? timeLeft.Hours.ToString() + " hour" : timeLeft.Hours.ToString() + " hours", "until Christmas!");
            }
            else if (Live_Tile_Style == 3)
            {
                tileXmlCountdown = string.Format(xml_3, timeLeft.Days.ToString() + " days,", timeLeft.Hours == 1 ? timeLeft.Hours.ToString() + " hour," : timeLeft.Hours.ToString() + " hours,", timeLeft.Minutes == 1 ? timeLeft.Minutes.ToString() + " minute" : timeLeft.Minutes.ToString() + " minutes");
            }
            else    // default to style 1 
            {
                if (babyHasName && appSettings[nameKey].ToString() != "")
                {
                    tileXmlCountdown = string.Format(xml, timeLeft.Days.ToString() + " days", babyName);
                }
                else
                {
                    tileXmlCountdown = string.Format(xml, timeLeft.Days.ToString() + " days until the big event");
                }
            }

            if (timeLeft.Days < 0)
                tileXmlCountdown = "No due date set."; 


            XmlDocument documentNow = new XmlDocument();
            documentNow.LoadXml(tileXmlCountdown);

            tileUpdater.Update(new TileNotification(documentNow) { ExpirationTime = now.AddMinutes(1) });

            for (var startPlanning = updateTime; startPlanning < planTill; startPlanning = startPlanning.AddMinutes(1))
            {
                Debug.WriteLine(startPlanning);
                Debug.WriteLine(planTill);

                try
                {

                    var tileXml = "";
                    if (Live_Tile_Style == 1)
                    {

                        if (babyHasName && appSettings[nameKey].ToString() != "")
                        {
                            tileXml = string.Format(xml, timeLeft.Days.ToString() + " days", babyName);
                        }
                        else
                        {
                            tileXml = string.Format(xml, timeLeft.Days.ToString() + " days until the big event");
                        }
                    }
                    else if (Live_Tile_Style == 2)
                    {
                        tileXml = string.Format(xml_2, timeLeft.Days.ToString() + " days,", timeLeft.Hours == 1 ? timeLeft.Hours.ToString() + " hour" : timeLeft.Hours.ToString() + " hours", "until Christmas!");
                    }
                    else if (Live_Tile_Style == 3)
                    {
                        tileXml = string.Format(xml_3, timeLeft.Days.ToString() + " days,", timeLeft.Hours == 1 ? timeLeft.Hours.ToString() + " hour," : timeLeft.Hours.ToString() + " hours,", timeLeft.Minutes == 1 ? timeLeft.Minutes.ToString() + " minute" : timeLeft.Minutes.ToString() + " minutes");
                    }
                    else
                    {

                        if (babyHasName && appSettings[nameKey].ToString() != "")
                        {
                            tileXml = string.Format(xml, timeLeft.Days.ToString() + " days", babyName);
                        }
                        else
                        {
                            tileXml = string.Format(xml, timeLeft.Days.ToString() + " days until the big event");
                        }
                    }

                    // Nothing will show if the days left is < 0 
                    if (timeLeft.Days < 0)
                        tileXml = "No graduation date set."; 
                    
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
