using System;
using System.Collections;

namespace Master40.DB.Data.Helper.Types
{
    public class TEnumerator<T> : IEnumerator
    {
        //TODO replace bool with double -> = 1 normal distribution ; 2 = expo ; usw
        public TEnumerator(T[] obj, bool expDistributed = false)
        {
            _internal = obj;
            _maxPosition = _internal.Length;
            _expDistributed = expDistributed;
        }

        
        public T[] _internal;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;
        private double _maxPosition;
        private bool _expDistributed;

        public void Reset()
        {
            position = 0;
        }

        public T GetNext()
        {
            position++;
            if (!(position < _maxPosition)) {
                Reset();
                if (_expDistributed)
                {
                    var newMax = Convert.ToInt32(Math.Round((_maxPosition / 2), 0));
                    if (newMax == _maxPosition)
                    {
                        _maxPosition = _internal.Length;
                    }
                    else
                    {
                        _maxPosition = newMax;
                    }
                }
                
            }
            return Current;
        }

        bool IEnumerator.MoveNext()
        {
            throw new InvalidOperationException();
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
