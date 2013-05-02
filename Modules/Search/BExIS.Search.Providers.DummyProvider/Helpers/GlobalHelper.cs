using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Search.Providers.DummyProvider.Helpers
{
    public static class GlobalHelper
    {
        static int _counter = 0;

        public static int Counter
        {
            get
            {
                return _counter;
            }
            set
            {
                _counter = value;
            }
        }

    }
}
