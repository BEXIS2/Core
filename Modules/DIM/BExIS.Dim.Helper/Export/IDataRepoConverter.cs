using System.Collections.Generic;

namespace BExIS.Dim.Helpers.Export
{
    public interface IDataRepoConverter
    {
        /// <summary>
        /// Generate a File based on the Datarepo needs
        /// and store it on the server and send a filepath back
        /// </summary>
        /// <param name="datasetVersionId"></param>
        /// <returns></returns>
        string Convert(long datasetVersionId);

        /// <summary>
        /// Validate datasetversion against the requirements of the datarepo
        /// </summary>
        /// <param name="datasetVersionId"></param>
        /// <returns></returns>
        bool Validate(long datasetVersionId, out List<string> errors);
    }
}
