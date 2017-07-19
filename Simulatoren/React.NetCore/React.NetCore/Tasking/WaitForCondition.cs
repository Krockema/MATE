//=============================================================================
//=  $Id: WaitForCondition.cs 184 2006-10-14 18:46:48Z eroe $
//=
//=  React.NET: A discrete-event simulation library for the .NET Framework.
//=  Copyright (c) 2005, Eric K. Roe.  All rights reserved.
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
using System.Collections.Generic;
using System.Text;

namespace React.Tasking
{
    /// <summary>
    /// A <see cref="Task"/> used to wait for a <see cref="Condition"/> to
    /// become signalled (true).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Normally client code should not have to instantiate objects of this
    /// class.  Rather, they should use the <see cref="ICondition.Block"/>
    /// method which will return the appropriate <see cref="Task"/> for
    /// acquiring consumable items on behalf of a client <see cref="Task"/>.
    /// </para>
    /// <para>
    /// This class is declared public to allow third parties to create
    /// their own derivatives of <see cref="Condition"/>.
    /// </para>
    /// </remarks>
    public class WaitForCondition : ProxyTask<Condition>
    {
        /// <summary>
        /// Create a new <see cref="WaitForCondition"/> task that will wait on
        /// the specified <see cref="Condition"/>.
        /// </summary>
        /// <param name="sim">The simulation context.</param>
        /// <param name="condition">
        /// The <see cref="Condition"/> to wait upon.
        /// </param>
        public WaitForCondition(Simulation sim, Condition condition)
            : base(sim, condition)
        {
        }

        /// <summary>
        /// Begins waiting on the associated <see cref="Condition"/>.
        /// </summary>
        /// <param name="activator">
        /// The object that activated this <see cref="WaitForCondition"/> task.
        /// </param>
        /// <param name="data">Not used.</param>
        protected override void ExecuteTask(object activator, object data)
        {
            if (Interrupted)
            {
                ResumeAll();
            }
            else
            {
                if (Blocker.Signalled)
                {
                    Activate(Blocker);
                }
                else
                {
                    Blocker.BlockTask(this);
                }
            }
        }
    }
}
