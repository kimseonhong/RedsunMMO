using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedsunLibrary.Utils
{
    public class SyncState<T>
    {
        private object _lockObj = new object();

        private T _state;

        public T State
        {
            get
            {
                lock (_lockObj)
                {
                    return _state;
                }
            }
            set
            {
                lock (_lockObj)
                {
                    _state = value;
                }
            }
        }


        public SyncState(T in_state)
        {
            _state = in_state;
        }

        public bool IsState(T in_state)
        {
            lock (_lockObj)
            {
                return (_state.Equals(in_state));
            }
        }

        public bool Exchange(T in_state)
        {
            lock (_lockObj)
            {
                _state = in_state;
            }
            return true;
        }

        public bool ExchangeNotEqual(T in_toState, out T out_oldState)
        {
            lock (_lockObj)
            {
                if (false == _state.Equals(in_toState))
                {
                    out_oldState = _state;
                    _state = in_toState;
                    return true;
                }

                out_oldState = _state;
                return false;
            }
        }

        public bool ExchangeNotEqualExcept(T in_toState, T in_exceptState, out T out_oldState)
        {
            lock (_lockObj)
            {
                if (true == _state.Equals(in_exceptState))
                {
                    out_oldState = _state;
                    return false;
                }

                if (false == _state.Equals(in_toState))
                {
                    out_oldState = _state;
                    _state = in_toState;
                    return true;
                }

                out_oldState = _state;
                return false;
            }
        }

        public override string ToString()
        {
            lock (_lockObj)
            {
                return Enum.GetName(typeof(T), _state);
            }
        }
    }
}
