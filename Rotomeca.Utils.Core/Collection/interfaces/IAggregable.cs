using System;
using System.Collections.Generic;
using System.Text;

namespace Rotomeca.Utils.Collections.Interfaces
{
    public interface IAggregable<T>
    {
        T Add(T b);
    }
}
