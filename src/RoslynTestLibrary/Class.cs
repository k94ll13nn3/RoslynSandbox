using System;

namespace RoslynTestLibrary
{
    public class Class : BaseClass
    {
        public override void AbstractMethod() => throw new NotImplementedException();

        private int Thing() => 5;
    }
}