using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedsunLibrary.Utils
{
    public class SingleTon<T> where T : class, new()
    {
        public static T Instance { get; private set; } = new T();
    }
}
