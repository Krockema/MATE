using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;

namespace Zpp.Util.StackSet
{
    /**
     * optimized version of StackSet2
     */
    public class StackSet<T> : IStackSet<T> where T : IId
    {
        private readonly Dictionary<Id, T> _dictionary = new Dictionary<Id, T>();

        // index to find Element in list via the element itself
        private readonly Dictionary<T, Id> _index = new Dictionary<T, Id>();

        public StackSet()
        {
        }

        public StackSet(IEnumerable<T> list)
        {
            PushAll(list);
        }

        public void Push(T element)
        {
            if (element == null)
            {
                return;
            }

            // a set contains the element only once, else skip adding
            if (Contains(element) == false)
            {
                Id id = element.GetId();
                _dictionary.Add(id, element);
                _index.Add(element, id);
            }
        }

        public void Remove(T element)
        {
            if (element == null)
            {
                return;
            }

            _dictionary.Remove(_index[element]);
            _index.Remove(element);
        }

        public bool Any()
        {
            return _dictionary.Any();
        }

        public T PopAny()
        {
            T element = default;
            foreach (var pair in _dictionary)
            {
                element = pair.Value;
                break;
            }

            Remove(element);
            return element;
        }

        public T GetAny()
        {
            foreach (var pair in _dictionary)
            {
                return pair.Value;
            }

            return default; // if empty
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        public void PushAll(IEnumerable<T> elements)
        {
            foreach (var element in elements)
            {
                Push(element);
            }
        }

        public int Count()
        {
            return _dictionary.Count;
        }

        public List<T> GetAll()
        {
            // create a copy of list
            List<T> all = new List<T>();
            all.AddRange(_dictionary.Values);
            return all;
        }

        public List<T2> GetAllAs<T2>() where T2 : T
        {
            List<T2> list = new List<T2>();
            foreach (var item in _dictionary.Values)
            {
                list.Add((T2) item);
            }

            return list;
        }

        public IStackSet<T2> As<T2>() where T2 : T
        {
            return new StackSet<T2>(_dictionary.Values.Select(x => (T2) x));
        }

        public void Clear()
        {
            _dictionary.Clear();
            _index.Clear();
        }

        public override string ToString()
        {
            string result = "";

            foreach (var item in _dictionary.Values)
            {
                result += item.ToString() + Environment.NewLine;
            }

            return result;
        }

        public T GetById(Id id)
        {
            if (_dictionary.ContainsKey(id))
            {
                return _dictionary[id];
            }
            else
            {
                return default;
            }
        }

        public void RemoveById(Id id)
        {
            if (_dictionary.ContainsKey(id) == false)
            {
                throw new  MrpRunException($"Id {id} doesn't exists.");
            }

            T t = _dictionary[id];
            Remove(t);
        }

        public bool Contains(T t)
        {
            return _index.ContainsKey(t);
        }

        public bool Contains(Id id)
        {
            return _dictionary.ContainsKey(id);
        }
    }
}