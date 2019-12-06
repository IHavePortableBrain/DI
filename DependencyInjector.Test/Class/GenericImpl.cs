using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjector.Test.Class
{
    internal class GenericImpl<T> : IGeneric<T>
        where T : ArrayList
    {
        public void DoIt()
        {
            throw new NotImplementedException();
        }
    }
}