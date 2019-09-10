﻿using Akka.Actor;

namespace Master40.SimulationCore.Helper
{

    /// <summary>
    /// Meta-data class. Nested/child actors can build path
    /// based on their parent(s) / position in hierarchy.
    /// </summary>
    public class ActorMetaData
    {
        public ActorMetaData(string name, IActorRef actorRef, ActorMetaData parent = null)
        {
            Name = name;
            Parent = parent;
            Ref = actorRef;
            // if no parent, we assume a top-level actor
            var parentPath = parent != null ? parent.Path : "/user";
            Path = string.Format(format: "{0}/{1}", arg0: parentPath, arg1: Name);
        }

        public string Name { get; private set; }

        public ActorMetaData Parent { get; set; }

        public string Path { get; private set; }
        public IActorRef Ref { get; private set; }
    }
}
