using BExIS.Dim.Entities.Submissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Services.Providers
{
    public interface ISubmissionProvider
    {
        Submission Create(long datasetVersionId);
        bool AcceptById(long submissionId);
        bool RejectById(long submissionId);
        bool DeleteById(long submissionId);
        Submission Update(Submission submission);
    }
}
