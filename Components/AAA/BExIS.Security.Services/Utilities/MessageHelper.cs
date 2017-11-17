namespace BExIS.Security.Services.Utilities
{
    public class MessageHelper
    {

        public static string GetErrorHeader()
        {
            return $"Error in System";
        }

        public static string GetCreateDatasetHeader()
        {
            return $"Dataset was created";
        }

        public static string GetCreateDatasetMessage(long datasetid, string title, string userName)
        {
            string message = $"Dataset <b>\"{title}\"</b> with id <b>({datasetid})</b> was created";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }


        public static string GetUpdateDatasetHeader()
        {
            return $"Dataset was updated";
        }

        public static string GetUpdateDatasetMessage(long datasetid, string title, string userName)
        {
            string message = $"Dataset <b>\"{title}\"</b> with id <b>({datasetid})</b> was updated";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetDownloadDatasetHeader()
        {
            return $"Dataset was downloaded";
        }

        public static string GetDownloadDatasetMessage(long datasetid, string title, string userName)
        {
            string message = $"Dataset <b>\"{title}\"</b> with id <b>({datasetid})</b> was downloaded";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetDeleteDatasetHeader()
        {
            return $"Dataset was deleted";
        }

        public static string GetDeleteDatasetMessage(long datasetid, string title, string userName)
        {
            string message = $"Dataset <b>\"{title}\"</b> with id <b>({datasetid})</b> was deleted";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }
    }
}
