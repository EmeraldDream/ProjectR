using System.Collections.Generic;

namespace FlowInLib
{
    public class ObjectPool<T> where T : new()
    {
        private static Stack<T> _objStack = new Stack<T>();
        private static int _limit = 10;

        public static void Push(T obj)
        {
            lock (_objStack)
            {
                if (_objStack.Count < _limit)
                    _objStack.Push(obj);
            }
        }

        public static T Pop()
        {
            lock (_objStack)
            {
                if (_objStack.Count > 0)
                    return _objStack.Pop();
            }
            return new T();
        }

        public static void Clear()
        {
            lock (_objStack)
            {
                _objStack.Clear();
            }
        }

        public static void SetLimit(int count)
        {
            if (count == _limit)
                return;

            lock (_objStack)
            {
                int num = _objStack.Count - count;
                if (num > 0)
                {
                    while (num-- > 0) _objStack.Pop();
                    _objStack.TrimExcess();
                }   

                _limit = count;
            }
        }
    }
}