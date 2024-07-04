namespace BExIS.Ext.Model.DB
{
    public abstract class Migration
    {
        public abstract bool BeforeUp();

        public abstract bool AfterUp();

        public abstract bool Up();

        /// <summary>
        /// It should be done in conect of a single transaction! or even all the migrations of a version should be wrapped in one transaction
        /// </summary>
        /// <returns></returns>
        public bool Install() // make it sealed
        {
            if (BeforeUp())
            {
                if (Up())
                {
                    return AfterUp();
                }
            }
            return false;
        }

        public abstract bool BeforeDown();

        public abstract bool AfterDown();

        public abstract bool Down();

        public bool Uninstall() // make ir sealed
        {
            if (BeforeDown())
            {
                if (Down())
                {
                    return AfterDown();
                }
            }
            return false;
        }
    }
}