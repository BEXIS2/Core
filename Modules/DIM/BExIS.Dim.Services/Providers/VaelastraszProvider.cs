using BExIS.Dim.Entities.Contents;
using BExIS.Dim.Entities.Submissions;
using BExIS.Dim.Services.Submissions;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Services.Providers
{
    public class VaelastraszProvider : ISubmissionProvider
    {
        private long _agentId;

        public VaelastraszProvider(long agentId) 
        {
            _agentId = agentId;
        }

        public bool AcceptById(long submissionId)
        {
            using (var submissionManager = new SubmissionManager())
            {
                var submission = submissionManager.FindById(submissionId);

                if(submission != null && submission.Status == SubmissionStatus.Pending)
                {
                    submission.Status = SubmissionStatus.Accepted;
                    submission.LastModificationDate = DateTimeOffset.UtcNow;

                    submissionManager.Update(submission);
                }

                return false;
            }
        }

        public Submission Create(long datasetVersionId)
        {
            using (var agentManager = new AgentManager())
            {
                var agent = agentManager.FindById(_agentId);

                if (agent == null)
                    throw new InvalidOperationException();

                using (var submissionManager = new SubmissionManager())
                using (var datasetManager = new DatasetManager())
                {
                    var submission = new Submission()
                    {
                        DatasetVersion = datasetManager.GetDatasetVersion(datasetVersionId),
                        Content = new DataCiteContent()
                    };

                    return submissionManager.Create(submission);
                }
            }
        }

        public bool DeleteById(long submissionId)
        {
            throw new NotImplementedException();
        }

        public bool RejectById(long submissionId)
        {
            throw new NotImplementedException();
        }

        public Submission Update(Submission submission)
        {
            throw new NotImplementedException();
        }
    }
}
