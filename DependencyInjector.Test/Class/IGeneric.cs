using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace DependencyInjector.Test.Class
{
    internal interface IGeneric<T> where T : IEnumerable
    {
        void DoIt();
    }
}