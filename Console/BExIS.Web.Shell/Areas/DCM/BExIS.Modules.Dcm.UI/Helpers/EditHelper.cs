using BExIS.IO;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models.Edit;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class EditHelper
    {
        public static bool IsReadable(BExIS.UI.Hooks.Caches.FileInfo file)
        {
            IOUtility iou = new IOUtility();
            return iou.IsSupportedAsciiFile(Path.GetExtension(file.Name));
        }

        public static List<SortedError> SortFileErrors(List<Error> errors)
        {
            if (errors.Count > 0)
            {
                // split up the error messages for a btter overview-- >
                // set all value error with the same var name, datatypoe and issue-- >
                // create a dictionary for error messages

                // variable issues
                var varNames = errors.Where(e => e.GetType().Equals(ErrorType.Value)).Select(e => e.getName()).Distinct();
                var varIssues = errors.Where(e => e.GetType().Equals(ErrorType.Value)).Select(e => e.GetMessage()).Distinct();

                List<SortedError> sortedErrors = new List<SortedError>();

                foreach (string vn in varNames)
                {
                    foreach (string i in varIssues)
                    {
                        int c = errors.Where(e => e.getName().Equals(vn) && e.GetMessage().Equals(i)).Count();

                        if (c > 0)
                        {
                            var errs = errors.Where(e => e.getName().Equals(vn) && e.GetMessage().Equals(i));
                            List<string> errorMessages = new List<string>();
                            errs.ToList().ForEach(e => errorMessages.Add(e.ToHtmlString()));
                            sortedErrors.Add(new SortedError(vn, c, i, errs.FirstOrDefault().GetType(), errorMessages));
                        }
                    }
                }

                // others
                var othersNames = errors.Where(e => e.GetType() != ErrorType.Value).Select(e => e.getName()).Distinct();
                var othersIssues = errors.Where(e => e.GetType() != ErrorType.Value).Select(e => e.GetMessage()).Distinct();

                foreach (string vn in othersNames)
                {
                    foreach (string i in othersIssues)
                    {
                        int c = errors.Where(e => e.getName().Equals(vn) && e.GetMessage().Equals(i)).Count();

                        if (c > 0)
                        {
                            var errs = errors.Where(e => e.getName().Equals(vn) && e.GetMessage().Equals(i));
                            List<string> errorMessages = new List<string>();
                            errs.ToList().ForEach(e => errorMessages.Add(e.ToHtmlString()));
                            sortedErrors.Add(new SortedError(vn, c, i, errs.FirstOrDefault().GetType(), errorMessages));
                        }
                    }
                }

                if (sortedErrors.Count > 0)
                {
                    return sortedErrors;
                }
            }

            return new List<SortedError>();
        }
    }
}