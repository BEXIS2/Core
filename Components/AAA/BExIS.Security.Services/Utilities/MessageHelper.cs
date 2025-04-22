using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace BExIS.Security.Services.Utilities
{
    public class MessageHelper
    {
        public static string GetCreateDatasetHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Created (ID {datasetid})";
        }

        public static string GetCreateDatasetMessage(long datasetid, string title, string userName, string entityname)
        {
            string message = $"{entityname} <b>\"{title}\"</b> with ID <b>{datasetid}</b> was created";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetDeleteDatasetHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Deleted (ID {datasetid})";
        }

        public static string GetDeleteDatasetMessage(long datasetid, string userName, string entityname)
        {
            string message = $"{entityname} with ID <b>{datasetid}</b> was deleted";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetTryToDeleteDatasetHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Try to delete ID {datasetid}";
        }

        public static string GetTryToDeleteDatasetMessage(long datasetid, string userName, string entityname)
        {
            string message = $"An unsuccessful attempt was made to delete a {entityname} with ID <b>{datasetid}</b>";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetPurgeDatasetHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Purged (ID {datasetid})";
        }

        public static string GetPurgeDatasetMessage(long datasetid, string userName, string entityname)
        {
            string message = $"{entityname} with ID <b>{datasetid}</b> was purged";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetTryToPurgeDatasetHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Try to purge (ID {datasetid})";
        }

        public static string GetTryToPurgeDatasetMessage(long datasetid, string userName, string entityname)
        {
            string message = $"An unsuccessful attempt was made to purge a {entityname} with ID <b>{datasetid}</b>";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetDownloadDatasetHeader(long datasetid, long version)
        {
            return $"File was downloaded (ID {datasetid}, Version: {version})";
        }

        public static string GetFileDatasetHeader(long datasetid, long version, string v)
        {
            return $"Dataset was downloaded (ID {datasetid}, Version: {version})";
        }

        public static string GetDownloadDatasetMessage(long datasetid, string title, string userName, string format, long version)
        {
            string message = $"Dataset <b>\"{title}\"</b> with ID <b>{datasetid}</b> version <b>({version})</b> was downloaded as <b>{format}</b>";

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
            return $"User <b>\"{userName}\"</b>(ID {userId}) with email <b>({email})</b> has registered.";
        }

        public static string GetSendRequestHeader(long datasetid, string requester)
        {
            return $"Data request from {requester} for dataset with ID = {datasetid}";
        }

        public static string GetSendRequestMessage(long datasetid, string title, string requester)
        {
            return $"User \"{requester}\" sent a data request for dataset <b>\"{title}\"</b> with ID <b>{datasetid}</b>";
        }

        public static string GetSendRequestMessage(long datasetid, string title, string requester, string reason, string email)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"User \"{requester}\" (\"{email}\") sent a data request for dataset <b>\"{title}\"</b> with ID <b>{datasetid}</b> <br/>");
            stringBuilder.AppendLine($"<b>Intention:</b> \"{reason}\" <br/><br/>");
            stringBuilder.AppendLine("To decide on this request login to  " + ConfigurationManager.AppSettings["ApplicationName"] + ". You will find all pending requests under My Data/Dashboard -> Datasets -> Decisions.");

            return stringBuilder.ToString();
        }

        public static string GetWithdrawRequestHeader(long datasetid, string requester)
        {
            return $"Data request from {requester} for dataset with ID <b>{datasetid}</b> withdrawn";
        }

        public static string GetWithdrawRequestMessage(long datasetid, string title, string requester)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Dataset request from user \"{requester}\" for dataset <b>\"{title}\"</b> with ID <b>{datasetid}</b> was withdrawn.<br/>");

            return stringBuilder.ToString();
        }

        public static string GetAcceptRequestHeader(long datasetid, string requester)
        {
            return $"Data request from {requester} for dataset with ID <b>{datasetid}</b> granted";
        }

        public static string GetAcceptRequestMessage(long datasetid, string title)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Permission for dataset <b>\"{title}\"</b> with ID <b>{datasetid}</b> granted.<br/>");

            return stringBuilder.ToString();
        }

        public static string GetRejectedRequestHeader(long datasetid, string requester)
        {
            return $"Data request from {requester} for dataset with ID <b>{datasetid}</b> rejected";
        }

        public static string GetRejectedRequestMessage(long datasetid, string title)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Data request for dataset <b>\"{title}\"</b> with ID <b>{datasetid}</b> rejected.<br/>");

            return stringBuilder.ToString();
        }

        public static string GetTryToRegisterUserHeader()
        {
            return $"User tries to register";
        }

        public static string GetTryToRegisterUserMessage(long userId, string userName, string email)
        {
            return $"User <b>\"{userName}\"</b>(ID {userId}) with email <b>({email})</b> tries to register.";
        }

        public static string GetMetadataUpdatHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Metadata updated (ID {datasetid})";
        }

        public static string GeFileUpdatHeader(long datasetid)
        {
            return $"File was uploaded (ID {datasetid})";
        }

        public static string GetFileUploaddMessage(long userId, string title, string userName, string filename)
        {
            return $"User <b>\"{userName}\"</b>(ID {userId}) has uploaded to the dataset <b>\"{title}\"</b> a file: <b>{filename}</b>.";
        }

        public static string GetFilesUploaddMessage(long userId,string title, string userName, string[] filenames)
        {
            var fnames = string.Join(",", filenames);
            return $"User <b>\"{userName}\"</b>(ID {userId}) has uploaded to the dataset <b>\"{title}\"</b> this files: <b>{fnames}</b>.";
        }

        public static string GetFileDownloadHeader(long datasetid, long version)
        {
            return $"File was downloaded (ID {datasetid}, Version: {version})";
        }

        public static string GetFileDownloadMessage(string userName, long id, string filename)
        {
            return $"User <b>\"{userName}\"</b> has downloaded a file: <b>{filename}</b> (ID {id}).";
        }

        public static string GetUpdateDatasetHeader(long datasetid)
        {
            return $"Data uploaded (ID {datasetid})";
        }

        public static string GetUpdateDatasetMessage(long datasetid, string title, string userName, string entityname, int numberOfRows = 0, int numberOfSkippedRows = 0)
        {

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{entityname} <b>\"{title}\"</b> with ID <b>{datasetid}</b> was updated");
            if(numberOfRows>0)
                stringBuilder.AppendLine($"<b>\"{numberOfRows}\"</b> rows have been successfully added/edited.");
            if(numberOfSkippedRows>0)
                stringBuilder.AppendLine($"<b>\"{numberOfSkippedRows}\"</b> rows were be skipped.");

            return stringBuilder.ToString();
        }

        public static string GetAttachmentUploadHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Attachment uploaded (ID {datasetid})";
        }

        public static string GetAttachmentUploadMessage(long datasetid, string fileNames, string userName)
        {
            string message = $"Attachments <b>\"{fileNames}\"</b> uploaded at ID <b>{datasetid}</b>";

            if (!string.IsNullOrEmpty(userName))

                return message += $" by <b>{userName}</b>";

            return message + ".";
        }

        public static string GetAttachmentDeleteHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Attachment deleted (ID {datasetid})";
        }

        public static string GetAttachmentDeleteMessage(long datasetid, string fileName, string userName)
        {
            string message = $"Attachments <b>\"{fileName}\"</b> deleted at ID <b>{datasetid}</b>";

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
            return $"User <b>\"{userName}\"</b>(ID {userId}) has updated his/her profile.";
        }

        public static string GetUpdateEmailHeader()
        {
            return $"User has changed email";
        }

        public static string GetUpdaterEmailMessage(string userName, string emailOld, string emailNew)
        {
            return $"User <b>{userName} has updated his/her email. Old: {emailOld} New: {emailNew}";
        }

        public static string GetChangedRoleHeader(string userName, string newRole, string changeType)
        {
            return $"Account of {userName} {changeType} {newRole} status";
        }

        public static string GetChangedRoleAppliedMessage(string userName, string newRole, string changeType, string additionalInformation)
        {
            return $"The {newRole} role has been {changeType} your account.<br/><br/>{additionalInformation}";
        }

        public static string GetSetPublicHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Set public (ID {datasetid})";
        }

        public static string GetSetPublicMessage(string userName, long datasetid, string entityname)
        {
            return $"The {entityname} with ID <b>{datasetid}</b> was set to public by {userName}.";
        }

        public static string GetUnsetPublicHeader(long datasetid, string entityname)
        {
            return $"{entityname}: Unset public (ID {datasetid})";
        }

        public static string GetUnsetPublicMessage(string userName, long datasetid, string entityname)
        {
            return $"The {entityname} with ID {datasetid} was unset from public by {userName}.";
        }

        #region upload api

        public static string GetPushApiStoreHeader(long datasetid, string title)
        {
            return $"Receive data for dataset '{title}' with ID <b>{datasetid}</b>";
        }

        public static string GetPushApiStoreMessage(long datasetid, string userName, string[] errors = null)
        {
            if (errors == null) return $"Data for dataset with ID <b>{datasetid}</b> received and successfully buffered.";
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
            return $"Validation completed for dataset '{title}' {datasetid}";
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
            return $"Upload <b>completed</b> for dataset: '{title}' with ID <b>{datasetid}</b>";
        }

        public static string GetPushApiUploadSuccessMessage(long datasetid, string userName)
        {
            return $"The data for the dataset <b>{datasetid}</b> sent by the user <b><{userName}/b> is uploaded.";
        }

        public static string GetPushApiUploadFailHeader(long datasetid, string title)
        {
            return $"Upload  was not successful for dataset '{title}' with ID <b>{datasetid}</b>";
        }

        public static string GetPushApiUploadFailMessage(long datasetid, string userName, string[] errors)
        {
            StringBuilder builder = new StringBuilder($"The data for the dataset with ID <b>{datasetid}</b> sent by the user <b><{userName}/b> not uploaded. <br>");
            builder.Append("Errors: <br>");

            foreach (string error in errors)
            {
                builder.Append(error + "<br>");
            }

            return builder.ToString();
        }

        public static string GetPushApiPKCheckHeader(long datasetid, string title)
        {
            return $"The verification of the primary key/s for the dataset '{title}' with ID <b>{datasetid}</b> is completed.";
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
            return $"Data upload started (ID {datasetid})";
        }

        public static string GetASyncStartUploadMessage(long datasetid, string title, IEnumerable<string> files)
        {
            string fNames = string.Join(", ", files);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"The upload of the file(s) <b>\"{fNames}\"</b> to your dataset <b>\"{title}\"</b> with ID <b>{datasetid}</b> has started.");


            return stringBuilder.ToString();
        }

        public static string GetASyncFinishUploadHeader(long datasetid, string title)
        {
            return $"Data upload completed (ID {datasetid})";
        }

        public static string GetASyncFinishUploadMessage(long datasetid, string title, int numberOfRows, int numberOfSkippedRows)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Your upload of the file(s) to the dataset <b>\"{title}\"</b> with ID <b>\"{datasetid}\"</b> is complete. <br/>");
            stringBuilder.AppendLine($"<b>\"{numberOfRows}\"</b> rows have been successfully added/edited.");
            stringBuilder.AppendLine($"<b>\"{numberOfSkippedRows}\"</b> rows were skipped.");

            return stringBuilder.ToString();
        }

        #endregion upload async

        #region serach index

        public static string GetSearchReIndexHeader()
        {
            return $"Search index was completed ";
        }

        public static string GetSearchReIndexMessage(List<string> errors = null)
        {
            string message = $"The creation of the search index is finished.";

            if (errors != null && errors.Count > 0)
            {
                message += $"the following errors have occurred. </br>";

                foreach (var item in errors)
                {
                    message += $"" + errors + "</br>";
                }
            }

            return message;
        }

        #endregion serach index
    }
}
