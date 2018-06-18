using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Drawing;

using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Data;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using KnowledgeFox;
//using ZoneFiveSoftware.SportTracks.IO.Import;

namespace KnowledgeFox.SportTracks.Computrainer
{
    class ComputrainerProcessor : IFileImporter, IPlugin
    {
        #region IDataImporter Members

       // private ComputrainerActivity activity;
        private Logger logger;

        public Guid Id
        {
            get { return new Guid("{c9a2c5e6-c47e-4927-8158-88f22dc24764}"); }
        }   

        public string Name
        {
            get { return "Computrainer File Processor"; }
        }

        public string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(3); }
        }

        public string FileExtension
        {
            get { return "txt"; }
        }

        public System.Drawing.Image Image
        {
            get { throw new NotImplementedException(); }
        }


     //  public Image Image
      //  {
      //      get { return Properties.Resources.Image_24_FileHRM; }
      //  }

        public bool Import(string configurationInfo, IJobMonitor monitor, IImportResults importResults)
        {
            //TJL Open Log File
            logger = Logger.GetLogger();
            //logger.writeLog("Starting computrainer import");
            //logger.writeLog(configurationInfo.ToString());

            //TODO: 50% of the progress should come from computrainerActivity
            ComputrainerActivity computrainerActivity = new ComputrainerActivity(configurationInfo);

            DateTime activityStartTime = computrainerActivity.ActivityStartTime;
            logger.writeLog("Got activity start time" + activityStartTime.ToString());
            IActivity activity = importResults.AddActivity(activityStartTime);
            activity.HasStartTime = true;
            
            //POWER
            float[,] activityDataPower = computrainerActivity.getActivityDataPower();
            activity.PowerWattsTrack = populateNumericDataTrack(activityDataPower, activityStartTime);

            //HEARTRATE
            float[,] activityDataHeartRate = computrainerActivity.getActivityDataHeartRate();
            activity.HeartRatePerMinuteTrack = populateNumericDataTrack(activityDataHeartRate, activityStartTime);
            monitor.PercentComplete = 0.8F;

            //CADENCE
            float[,] activityDataCadence = computrainerActivity.getActivityDataCadence();
            activity.CadencePerMinuteTrack = populateNumericDataTrack(activityDataCadence, activityStartTime);

            //TODO: 50% of the progress should come from here
            //DISTANCE IN METERS
            float[,] activityDataDistanceMeters = computrainerActivity.getActivityDataDistanceMeters();
            activity.DistanceMetersTrack = populateDistanceDataTrack(activityDataDistanceMeters, activityStartTime); 
        
            activity.Name = computrainerActivity.WorkoutFile;
            activity.Location = computrainerActivity.ActivityLocation;
            activity.Notes = computrainerActivity.Filename;
            activity.UseEnteredData = false;

        //    logger.closeLog();
            return true;
        }

        public INumericTimeDataSeries populateNumericDataTrack(float[,] activityData, DateTime activityStartTime)
        {
            INumericTimeDataSeries dataSeries = new NumericTimeDataSeries();
            for (int i = 0; i < activityData.GetLength(1); i++)
            {
                DateTime activityDataPointTime = activityStartTime.AddMilliseconds((double)activityData[0, i]); ;
                dataSeries.Add(activityDataPointTime, activityData[1, i]);
               // logger.writeLog("populateNumericDataTrack - activity data point time" + activityDataPointTime.ToString() + i.ToString());
            }
            return dataSeries;
        }

        public IDistanceDataTrack populateDistanceDataTrack(float[,] activityData, DateTime activityStartTime)
        {
            IDistanceDataTrack dataSeries = new DistanceDataTrack();
            for (int i = 0; i < activityData.GetLength(1); i++)
            {
                DateTime activityDataPointTime = activityStartTime.AddMilliseconds((double)activityData[0, i]); ;
                dataSeries.Add(activityDataPointTime, activityData[1, i]);
            }
            return dataSeries;
        }
        #endregion

        #region IPlugin Members

        public IApplication Application
        {
            set { application = value; }
        }

        public void ReadOptions(XmlDocument xmlDoc, XmlNamespaceManager nsmgr, XmlElement pluginNode)
        {
        }

        public void WriteOptions(XmlDocument xmlDoc, XmlElement pluginNode)
        {
        }

        #endregion

        private static IApplication application;
    }
       


}
