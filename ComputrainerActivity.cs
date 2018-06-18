using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace KnowledgeFox.SportTracks.Computrainer
{
    class ComputrainerActivity
    {
        // Map computrainer field names to standard field names
        public const string milliseconds = "ms";
        public const string speedMiles = "speed";
        public const string power = "watts";
        public const string cadence = "rpm";
        public const string heartRate = "hr";
        public const string spinscanAverage = "ss";
        public const string spinscanLeft = "lss";
        public const string spinscanRight = "rss";
        public const string leftPower = "lpwr";
        public const string rightPower = "rpwr";
        public const string distanceMiles = "miles";
        public const string distanceKM = "KM";
        public const string wind = "wind";
        public const string grade = "grade";
        public const string load = "load";
        public const string leftAta = "lata";
        public const string rightAta = "rata";
        public const string pulsePower = "pp";
        public const string cadence2 = "cadence";
        public const string spinscanRaw = "ss_raw";

        public const float metersPerMile = 1609.344F;
        
        //File Data
        private FileInfo txtDataFile;
        private Logger logger;
  
        //Computrainer User Data
        private string athlete;
        private int age;
        private string weight; //Should we convert this to a float?
        private int lowerHR;
        private int upperHR;
        private int dragFactor;
            //TODO : Add accessors for user data

        //Computrainer Activity Data
        private string[] activityFieldNames;
        private float[,] activityData;
        private DateTime activityEndTime;
        private DateTime activityStartTime;
        private String workoutFile;
        private String activityLocation = "Computrainer";

        //Processing data
        private StreamReader txtDataStream = null;
        private int numberOfRecords;

        public ComputrainerActivity(String txtFile)
        {
            logger = Logger.GetLogger();
            logger.writeLog("Creating computrainer activity.");
            try
            {
                txtDataFile = new FileInfo(txtFile);
                parseFileName(txtDataFile);
                StreamReader txtDataStream = txtDataFile.OpenText();
                processUserData(txtDataStream);
                processActivityData(txtDataStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (txtDataStream != null)
                {
                    txtDataStream.Close();
                }
            }
        }

        public DateTime ActivityStartTime
        {
            get {
                logger.writeLog("Get ActivityStartTime : " + activityStartTime.ToString());
                return activityStartTime; }
        }

        public string Filename
        {
            get { return txtDataFile.Name; }
        }
        
        public string Athlete
        {
            get { return athlete; }
        }

        public string Weight
        {
            get { return weight; }
        }

        public int LowerHR
        {
            get { return lowerHR; }
        }

        public int UpperHR
        {
            get { return upperHR; }
        }

        public int DragFactor
        {
            get { return dragFactor; }
        }

        public String ActivityLocation
        {
            get { return activityLocation; }
        }

        public String WorkoutFile
        {
            get { return workoutFile; }
        }

        public int ActivityDataCount
        {
            get { return activityData.GetLength(1); }
        }

        public float[,] getActivityDataPower()
        {
            return getActivityData(power);
        }

        public float[,] getActivityDataHeartRate()
        {
            return getActivityData(heartRate);
        }

        public float[,] getActivityDataCadence()
        {
            return getActivityData(cadence);
        }

        public float[,] getActivityDataGrade()
        {
            return getActivityData(grade);
        }

        public float[,] getActivityDataDistanceMiles()
        {
            return getActivityData(distanceMiles);
        }

        public float[,] getActivityDataDistanceMeters()
        {
            float[,] activityDataDistanceMeters = getActivityData(distanceKM);

            if (activityDataDistanceMeters.GetLength(1) >= 1)
            {
                for (int i = 0; i < activityDataDistanceMeters.GetLength(1); i++)
                {
                    activityDataDistanceMeters[1, i] = activityDataDistanceMeters[1, i] * 1000;
                }
            }
            else
            {
                activityDataDistanceMeters = getActivityData(distanceMiles);
                for (int i = 0; i < activityDataDistanceMeters.GetLength(1); i++)
                {
                    //logger.writeLog("Activity Distance Meters : "
                    //    + activityDataDistanceMeters[0, i].ToString() + " "
                    //    + activityDataDistanceMeters[1, i].ToString());

                    activityDataDistanceMeters[1, i] = activityDataDistanceMeters[1, i] * metersPerMile;
                }
            }
            return activityDataDistanceMeters;
        }

        private float[,] getActivityData(string requestedDataField)
        {
            float[,] dataSeries = new float[0,0];
            int fieldIndex = 0;
            while (fieldIndex < activityFieldNames.GetLength(0) &&
                String.Compare(requestedDataField, activityFieldNames[fieldIndex]) != 0)
                fieldIndex++;
            
            if (fieldIndex < activityFieldNames.GetLength(0))
            {
                dataSeries = new float[2, activityData.GetLength(1)];
                for (int i = 0; i < activityData.GetLength(1); i++)
                {
                    dataSeries[0, i] = activityData[0, i];
                    dataSeries[1, i] = activityData[fieldIndex, i];
                }
            }
            return dataSeries;
        }
        //TODO Write accessors for each type of time series data.

        private bool parseFileName(FileInfo fileInfo)
        {
            int year;
            int month;
            int day;
            int hour;
            int minute;
            int second;
            Regex fileFormat;
            Match m;
            String filename = fileInfo.Name;

            //Default filename from CS software
            fileFormat = new Regex
                (@"-(?<file>.*)-(?<year>20[01][0123456789])-(?<month>(0[123456789]|1[012]))-(?<day>([012]\d|3[01]))-(?<hour>([01]\d|2[0123]))-(?<minute>[012345]\d)");
            m = fileFormat.Match(filename);
            
            //Typical manual filename: ddmmyy
            if (!m.Success)
            {
                logger.writeLog("Default CS filename NOT matched");
                fileFormat = new Regex(@"(?<file>.*)(?<day>([0-2]\d|3[01]))(?<month>(0[0-9]|1[0-2]))(?<year>(0[0-9]|1[0-5]))");
                m = fileFormat.Match(filename);
            }
            //Typical manual filename: yymmdd
            if (!m.Success)
            {
                fileFormat = new Regex(@"(?<file>.*)(?<year>(0[0-9]|1[0-5]))(?<month>(0[0-9]|1[0-2]))(?<day>([0-2]\d|3[01]))");
                m = fileFormat.Match(filename);
            }
            //Typical manual American filename: yyddmm
            if (!m.Success)
            {
                fileFormat = new Regex(@"(?<file>.*)(?<year>(0[0-9]|1[0-5]))(?<day>([0-2]\d|3[01]))(?<month>(0[0-9]|1[0-2]))");  
                m = fileFormat.Match(filename);
            }

            if (m.Success)
            {
                year = m.Groups["year"].Value != "" ? Int16.Parse(m.Groups["year"].Value) : 0;
                if (year <= 50) year += 2000;
                else if (year >= 50 && year < 1900) year += 1900;

                month = m.Groups["month"].Value != "" ? Int16.Parse(m.Groups["month"].Value) : 0;
                day = m.Groups["day"].Value != "" ? Int16.Parse(m.Groups["day"].Value) : 0;
                hour = m.Groups["hour"].Value != "" ? Int16.Parse(m.Groups["hour"].Value) : 12;
                minute = m.Groups["minute"].Value != "" ? Int16.Parse(m.Groups["minute"].Value) : 0;
                second = m.Groups["second"].Value != "" ? Int16.Parse(m.Groups["second"].Value) : 0;

                workoutFile = m.Groups["file"].Value != "" ? m.Groups["file"].Value : "";

                activityEndTime = new DateTime(year, month, day, hour, minute, second);
            }
            else
            {
                //Take from file timestamp.
                activityEndTime= fileInfo.CreationTime;
            }
            logger.writeLog("Activity End Time : " + activityEndTime.ToString());
            return true;
        }

        
        private bool processCourseData(StreamReader txtFileStream)
        {
            //TODO: Course user data
            return true;
        }

        private bool processUserData(StreamReader txtFileStream)
        {
            //TODO: Process user data
            return true;
        }
 
        private bool processActivityData(StreamReader txtFileStream)
        {      
            char[] headerDelimiter = new char[] { ' ' };
            string numberOfRecordsText = "number of records";
            string read;
            int recordCount = 0;
            string activityDataLine;
            char[] datafieldDelimiter = " \t\n\r".ToCharArray();

            do
            {
                read = txtFileStream.ReadLine();
            } while (read.Contains(numberOfRecordsText) != true);

            // Read Number of Records field
            numberOfRecords = int.Parse(read.Substring(read.LastIndexOf("=") + 1));
            if (numberOfRecords > 0)
            {
                //Assume field list comes after Number of Records with only blanks in between
                do
                {
                    read = txtFileStream.ReadLine();
                } while (read == "");

                activityFieldNames = read.TrimEnd(' ').Split(headerDelimiter);
                //Create an array of the appropriate size for the data.
                activityData = new float[activityFieldNames.Count(), numberOfRecords];

                do
                {
                    read = txtFileStream.ReadLine();
                } while (read == "");

                Regex spaces = new Regex("[, \t\"]+");
                try
                {

                    for (recordCount = 0; recordCount < numberOfRecords && read != ""; recordCount++)
                    {
                        string[] fields = spaces.Split(read.Replace('"', ' ').Trim());

                        for (int j = 0; j < activityFieldNames.Count(); j++)
                        {
                            activityData[j, recordCount] = float.Parse(fields[j]);
                        }
                        read = txtFileStream.ReadLine();
                    }
                }
                catch (Exception e)
                {
  //                  logger.writeLog(e.ToString());
                }

                activityStartTime = activityEndTime.Subtract(
                    new TimeSpan(0, 0, 0, 0, ((int)activityData[0, recordCount - 1])));
            }

            ////TO DO: Capture total values from last line of file e.g. milage
            return true;

        }
    }
}
