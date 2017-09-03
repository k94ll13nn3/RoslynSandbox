namespace RoslynTestLibrary
{
    public abstract class BaseClass
    {
        public abstract void AbstractMethod();

        public virtual void VirtualMethod()
        {
        }

        public static int Number()
        {
            return 0;
        }

        protected void MyProtectedMethod()
        {

        }

        private void MyPrivateMethod()
        {

        }
    }
}