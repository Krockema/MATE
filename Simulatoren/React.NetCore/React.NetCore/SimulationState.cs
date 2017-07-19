//=============================================================================
//=  $Id: SimulationState.cs 184 2006-10-14 18:46:48Z eroe $
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

namespace React
{
    /// <summary>
    /// Describes the various run-time states of a <see cref="Simulation"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Normally, a <see cref="Simulation"/> will progress through its
    /// states in the following order:
    /// </para>
    /// <list type="number">
    ///     <item><description>Ready</description></item>
    ///     <item><description>Initializing</description></item>
    ///     <item><description>Running</description></item>
    ///     <item><description>Stopping (optional)</description></item>
    ///     <item><description>Completed</description></item>
    /// </list>
    /// <para>
    /// Note that the <b>Stopping</b> state is often bypassed with the
    /// <see cref="Simulation"/> progressing directly from the <b>Running</b>
    /// to the <b>Completed</b> state.
    /// </para>
    /// </remarks>
    public enum SimulationState
    {
        /// <summary>
        /// The <see cref="Simulation"/> is new and ready to be run.
        /// </summary>
        Ready,

        /// <summary>
        /// The <see cref="Simulation"/> is initializing.  This indicates
        /// that the generator <see cref="Task"/>s are being
        /// activated.
        /// </summary>
        Initializing,

        /// <summary>
        /// The <see cref="Simulation"/> is running.  This state indicates
        /// that <see cref="ActivationEvent"/>s are being processed.
        /// </summary>
        Running,

        /// <summary>
        /// The <see cref="Simulation"/> is stopping.  This state indicates
        /// that one of the <b>Simulation.Stop</b> methods was called.
        /// </summary>
        Stopping,

        /// <summary>
        /// The <see cref="Simulation"/> has completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The <see cref="Simulation"/> terminated with errors.
        /// </summary>
        Failed
    }
}
