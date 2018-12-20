using Master40.Agents.Agents.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.Agents.Agents.Internal
{
    public class Batch
    {
        public Batch(string setup)
        {
            Setup = setup;
            WorkItems = new List<WorkItem>();
        }
        public string Setup { get; }
        public List<WorkItem> WorkItems { get; }
    }
}
