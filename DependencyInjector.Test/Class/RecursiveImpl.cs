using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjector.Test.Class
{
    internal class RecursiveImpl : I
    {
        public void SomeMethod()
        {
            throw new NotImplementedException();
        }

        public RecursiveImpl(I i)
        {
        }
    }
}