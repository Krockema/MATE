//=============================================================================
//=  $Id: InterruptTask.cs 172 2006-09-10 21:19:08Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2004, Eric K. Roe.  All rights reserved.
//=
//=  React.NET is free software; you can redistribute it and/or modify it
//=  under the terms of the GNU General Public License as published by the
//=  Free Software Foundation; either version 2 of the License, or (at your
//=  option) any later version.
//=
//=  React.NET is distributed in the hope that it will be useful, but WITHOUT
//=  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
//=  FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for
//=  more details.
//=
//=  You should have received a copy of the GNU General Public License along
//=  with React.NET; if not, write to the Free Software Foundation, Inc.,
//=  51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//=============================================================================

using System;

namespace React.Tasking
{
    /// <summary>
    /// A <see cref="Task"/> implementation that can be used to interrupt other
    /// <see cref="Task"/>s.
    /// </summary>
    public class InterruptTask : Task
    {
        /// <summary>
        /// The object given as the <see cref="Task"/> interruptor.
        /// </summary>
        private object _interruptor;

        /// <overloads>Create and initialize an IterruptTask.</overloads>
        /// <summary>
        /// Create a new <see cref="InterruptTask"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        public InterruptTask(Simulation sim) : this(sim, null)
        {
        }

        /// <summary>
        /// Create a new <see cref="InterruptTask"/> specifiying the
        /// interruptor object.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="interruptor">The interruptor object.</param>
        public InterruptTask(Simulation sim, object interruptor)
            : base(sim)
        {
            _interruptor = interruptor;
        }

        /// <summary>
        /// Gets or sets the interruptor object.
        /// </summary>
        /// <remarks>
        /// The interruptor object is passed to each blocked <see cref="Task"/>'s
        /// <see cref="Task.Interrupt"/> method when this <see cref="Task"/> is
        /// executed.  By default it is set to <c>this</c>.
        /// </remarks>
        /// <value>
        /// The interruptor as an <see cref="object"/>.  Setting this property
        /// to <see langword="null"/> will result in the property getter
        /// returning <c>this</c>.
        /// </value>
        public object Interruptor
        {
            get { return _interruptor != null ? _interruptor : this; }
            set { _interruptor = value; }
        }

        /// <summary>
        /// Interrupt all blocked <see cref="Task"/> instances.
        /// </summary>
        /// <param name="activator">Not used.</param>
        /// <param name="data">Not used.</param>
        protected override void ExecuteTask(object activator, object data)
        {
            ResumeAll(Interruptor, null);
        }

        /// <summary>
        /// Resume the given <see cref="Task"/> using an interrupt.
        /// </summary>
        /// <param name="task">The <see cref="Task"/> to interrupt.</param>
        /// <param name="activator">The interruptor object.</param>
        /// <param name="data">Not used.</param>
        protected override void ResumeTask(Task task, object activator,
            object data)
        {
            // DO NOT INVOKE THE BASE CLASS IMPLEMENTATION!

            System.Diagnostics.Debug.Assert(Interruptor == activator);
            task.Interrupt(activator);
        }
    }
}
