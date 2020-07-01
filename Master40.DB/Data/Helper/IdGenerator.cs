using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.DB.Data.Helper
{
    public class IdGenerator
    {
        private const int _start = 10000;
        private int _currentId = _start;
        private static  readonly Dictionary<Type, List<string>> objectTypeToIds = new Dictionary<Type, List<string>>();

        public Id GetNewId()
        {
            lock (this)
            {
                _currentId++;

                return new Id(_currentId);
            }
        }

        public Id GetNewId(Type objectType, string requester)
        {
            lock (this)
            {
                _currentId++;
                if (objectTypeToIds.ContainsKey(objectType) == false)
                {
                    objectTypeToIds.Add(objectType, new List<string>());
                }

                if (requester.Contains("lambda") == false)
                {
                    objectTypeToIds[objectType].Add($"{_currentId} ({requester})");   
                }

                return new Id(_currentId);
            }
        }

        public static string GetObjectTypeToIdsAsString()
        {
            string s = "";
            foreach (var key in objectTypeToIds.Keys)
            {
                s += $"{key.Name}:\n";
                foreach (var id in objectTypeToIds[key])
                {
                    s += $"{id}\n";
                }

                s += "\n\n";

            }

            return s;
        }

        public static string WriteToFile()
        {
            
            string usedIdsFileName =
                $"../../../Test/used_ids.txt";
            File.WriteAllText(usedIdsFileName, GetObjectTypeToIdsAsString(),
                Encoding.UTF8);
            return usedIdsFileName;
        }

        public static int CountIdsOf(Type type)
        {
            return objectTypeToIds[type].Count();
        }
        
    }
}