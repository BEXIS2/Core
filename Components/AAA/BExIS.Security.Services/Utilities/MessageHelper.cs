using System.Configuration;
using System.Text;

namespace BExIS.Security.Services.Utilities
{
    public class MessageHelper
    {
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

        public static string GetDeleteDatasetHeader()
        {
            return $"Dataset was deleted";
        }

        public static string GetDeleteDatasetMessage(long datasetid, string userName)
        {
            string message = $"Dataset with id <b>({datasetid})</b> was deleted";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetTryToDeleteDatasetHeader()
        {
            return $"Someone tried to delete a dataset";
        }

        public static string GetTryToDeleteDatasetMessage(long datasetid, string userName)
        {
            string message = $"An unsuccessful attempt was made to delete a Dataset with id <b>({datasetid})</b>";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetPurgeDatasetHeader()
        {
            return $"Dataset was purged";
        }

        public static string GetPurgeDatasetMessage(long datasetid, string userName)
        {
            string message = $"Dataset with id <b>({datasetid})</b> was purged";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetTryToPurgeDatasetHeader()
        {
            return $"Someone tried to purge a dataset";
        }

        public static string GetTryToPurgeDatasetMessage(long datasetid, string userName)
        {
            string message = $"An unsuccessful attempt was made to purge a Dataset with id <b>({datasetid})</b>";

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

        public static string GetErrorHeader()
        {
            return $"Error in System";
        }

        public static string GetRegisterUserHeader()
        {
            return $"User has registered";
        }

        public static string GetRegisterUserMessage(long userId, string userName, string email)
        {
            return $"User <b>\"{userName}\"</b>(Id: {userId}) with email <b>({email})</b> has registered.";
        }

        public static string GetSendRequestHeader(long datasetid, string requester)
        {
            return $"Data request from {requester} for dataset with id = {datasetid}";
        }

        public static string GetSendRequestMessage(long datasetid, string title, string requester)
        {
            return $"User \"{requester}\" sent a data request for Dataset <b>\"{title}\"</b> with id <b>({datasetid})</b>";
        }

        public static string GetSendRequestMessage(long datasetid, string title, string requester, string reason, string email)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"User \"{requester}\" (\"{email}\") sent a data request for dataset <b>\"{title}\"</b> with id <b>{datasetid}</b> <br/>");
            stringBuilder.AppendLine($"<b>Intention:</b> \"{reason}\" <br/><br/>");
            stringBuilder.AppendLine("To decide on this request login to  " + ConfigurationManager.AppSettings["ApplicationName"] + ". You will find all pending requests under My Data/Dashboard -> Datasets -> Decisions.");


            return stringBuilder.ToString();
        }

        public static string GetWithdrawRequestHeader(long datasetid, string requester)
        {
            return $"Data request from {requester} for dataset with id = {datasetid} withdrawn";
        }

        public static string GetWithdrawRequestMessage(long datasetid, string title, string requester)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Dataset request from User \"{requester}\" for dataset <b>\"{title}\"</b> with id <b>{datasetid}</b> was withdrawn.<br/>");
         
            return stringBuilder.ToString();
        }

        public static string GetAcceptRequestHeader(long datasetid, string requester)
        {
            return $"Data request from {requester} for dataset with id = {datasetid} granted";
        }

        public static string GetAcceptRequestMessage(long datasetid, string title)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Permission for Dataset <b>\"{title}\"</b> with id <b>{datasetid}</b> granted.<br/>");

            return stringBuilder.ToString();
        }

        public static string GetRejectedRequestHeader(long datasetid, string requester)
        {
            return $"Data request from {requester} for dataset with id = {datasetid} rejected";
        }

        public static string GetRejectedRequestMessage(long datasetid, string title)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Data request for dataset <b>\"{title}\"</b> with id <b>{datasetid}</b> rejected.<br/>");

            return stringBuilder.ToString();
        }

        public static string GetTryToRegisterUserHeader()
        {
            return $"User tries to register";
        }

        public static string GetTryToRegisterUserMessage(long userId, string userName, string email)
        {
            return $"User <b>\"{userName}\"</b>(Id: {userId}) with email <b>({email})</b> tries to register.";
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

        public static string GetUpdateProfileHeader()
        {
            return $"User has modified profile";
        }

        public static string GetUpdaterProfileMessage(long userId, string userName)
        {
            return $"User <b>\"{userName}\"</b>(Id: {userId}) has updated his/her profile.";
        }

        #region upload api

        public static string GetPushApiStoreHeader(long datasetid, string title)
        {
            return $"Receive data for dataset '{title}' ({datasetid})";
        }

        public static string GetPushApiStoreMessage(long datasetid, string userName, string[] errors = null)
        {
            if (errors == null) return $"Data for dataset with id: {datasetid} received and successfully buffered.";
            else
            {
                StringBuilder builder = new StringBuilder($"An error has occurred during the transmission of the data.");
                builder.Append("Errors: <br>");

                foreach (string error in errors)
                {
                    builder.Append(error + "<br>");
                }

                return builder.ToString();
            }
        }

        public static string GetPushApiValidateHeader(long datasetid, string title)
        {
            return $"Validation completed for dataset '{title}' ({datasetid})";
        }

        public static string GetPushApiValidateMessage(long datasetid, string userName, string[] errors = null)
        {
            if (errors == null) return $"The data for the dataset <b>{datasetid}</b> sent by the user <b><{userName}/b> is valid.";
            else
            {
                StringBuilder builder = new StringBuilder($"The data for the dataset <b>{datasetid}</b> sent by the user <b><{userName}/b> not valid. <br>");
                builder.Append("Errors: <br>");

                foreach (string error in errors)
                {
                    builder.Append(error + "<br>");
                }

                return builder.ToString();
            }
        }

        public static string GetPushApiUploadSuccessHeader(long datasetid, string title)
        {
            return $"Upload <b>completed</b> for dataset: '{title}' ({datasetid})";
        }

        public static string GetPushApiUploadSuccessMessage(long datasetid, string userName)
        {
            return $"The data for the dataset <b>{datasetid}</b> sent by the user <b><{userName}/b> is uploaded.";
        }

        public static string GetPushApiUploadFailHeader(long datasetid, string title)
        {
            return $"Upload  was not successful for dataset '{title}' ({datasetid})";
        }

        public static string GetPushApiUploadFailMessage(long datasetid, string userName, string[] errors)
        {
            StringBuilder builder = new StringBuilder($"The data for the dataset <b>{datasetid}</b> sent by the user <b><{userName}/b> not uploaded. <br>");
            builder.Append("Errors: <br>");

            foreach (string error in errors)
            {
                builder.Append(error + "<br>");
            }

            return builder.ToString();
        }

        public static string GetPushApiPKCheckHeader(long datasetid, string title)
        {
            return $"The verification of the primary key/s for the dataset '{title}' ({datasetid}) is completed.";
        }

        public static string GetPushApiPKCheckMessage(long datasetid, string userName, string[] errors)
        {
            if (errors == null || errors.Length == 0) return $"The verification of the primary key/s was successful.";
            else
            {
                StringBuilder builder = new StringBuilder($"The verification of the primary key was not successful.<br>");
                builder.AppendLine("<br>");
                builder.AppendLine("<b>Errors:</b> <br>");

                foreach (string error in errors)
                {
                    builder.AppendLine(error + "<br>");
                }

                return builder.ToString();
            }
        }

        #endregion upload api

        #region upload async

        public static string GetASyncStartUploadHeader(long datasetid, string title)
        {
            return $"Data upload started";
        }

        public static string GetASyncStartUploadMessage(long datasetid, string title, int numberOfRows)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Your upload to the dataset <b>\"{title}\"</b> with id <b>(\"{datasetid}\")</b> has started. <br/>");
            stringBuilder.AppendLine($"<b>\"{numberOfRows}\"</b> lines will be added/edited.");

            return stringBuilder.ToString();
        }

        public static string GetASyncFinishUploadHeader(long datasetid, string title)
        {
            return $"Data upload completed";
        }

        public static string GetASyncFinishUploadMessage(long datasetid, string title, int numberOfRows)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Your upload to the dataset <b>\"{title}\"</b> with id <b>(\"{datasetid}\")</b> is completed. <br/>");
            stringBuilder.AppendLine($"<b>\"{numberOfRows}\"</b> lines have been successfully added/edited.");

            return stringBuilder.ToString();
        }

        #endregion
    }
}