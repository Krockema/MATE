using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Helper.Types
{
    public class TEnumerator<T> : IEnumerator
    {
        public TEnumerator(T[] obj)
        {
            _internal = obj;
        }


        public T[] _internal;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public void Reset()
        {
            position = 0;
        }

        public T GetNext()
        {
            position++;
            if (!(position < _internal.Length))
                Reset();
            return Current;
        }

        bool IEnumerator.MoveNext()
        {
            throw new InvalidOperationException();
            position++;
            if (!(position <= _internal.Length))
                Reset();
            return true;
        }

        public T Current
        {
            get
            {
                try
                {
                    return _internal[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => throw new NotImplementedException();
    }
}
