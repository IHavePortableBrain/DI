using System;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjector.Test.Class
{
    internal class DependentImpl : I
    {
        public void SomeMethod()
        {
            throw new NotImplementedException();
        }

        public DependentImpl(NotImpl notImpl)
        {
        }
    }
}