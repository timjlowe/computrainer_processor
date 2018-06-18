using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using ZoneFiveSoftware.Common.Data;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using KnowledgeFox.SportTracks.Computrainer;

namespace ZoneFiveSoftware.SportTracks.IO.Import
{
    class ExtendFileImporters : IExtendDataImporters
    {
        #region IExtendDataImporters Members

        public IList<IFileImporter> FileImporters
        {
            get
            {
                return new IFileImporter[] { new KnowledgeFox.SportTracks.Computrainer.ComputrainerProcessor() };
            }
        }

        public void BeforeImport(IList items)
        {
        }

        public void AfterImport(IList added, IList updated)
        {
        }
        #endregion
    }
}
