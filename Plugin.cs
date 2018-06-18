using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using ZoneFiveSoftware.Common.Visuals.Fitness;

namespace ComputrainerProcessor
{
    class Plugin : IPlugin

    {
        #region IPlugin Members

        public IApplication Application
        {
            set {  }
        }

        public Guid Id
        {
            get { return new Guid("{c9a2c5e6-c47e-4927-8158-88f22dc24764}"); }
        }

        public string Name
        {
            get { return "ComputrainerProcessor"; }
        }

        public void ReadOptions(XmlDocument xmlDoc, XmlNamespaceManager nsmgr, XmlElement pluginNode)
        {
            ;
        }

        public string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(3); }
        }

        public void WriteOptions(XmlDocument xmlDoc, XmlElement pluginNode)
        {
            ;
        }

        #endregion
    }
}
