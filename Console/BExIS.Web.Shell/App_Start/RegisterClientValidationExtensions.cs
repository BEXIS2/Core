using DataAnnotationsExtensions.ClientValidation;

[assembly: WebActivator.PreApplicationStartMethod(typeof(BExIS.Web.Shell.App_Start.RegisterClientValidationExtensions), "Start")]

namespace BExIS.Web.Shell.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();
        }
    }
}