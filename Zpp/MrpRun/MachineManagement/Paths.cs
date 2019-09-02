using System.Collections.Generic;

namespace Zpp.MrpRun.MachineManagement
{
    public class Paths<T>
    {
        private List<Stack<T>> _paths = new List<Stack<T>>();
        
        public void AddAll(Paths<T> paths)
        {
            _paths.AddRange(paths._paths);
        }
        
        public void AddPath(Stack<T> path)
        {
            _paths.Add(path);
        }
        
        /**
         * starting by 0
         */
        public IStackSet<T> PopLevel()
        {
            IStackSet<T> stackSet = new StackSet<T>();
            foreach (var path in _paths)
            {
                if (path.Count > 0)
                {
                    stackSet.Push(path.Pop());
                }
            }

            return stackSet;
        }
    }
}