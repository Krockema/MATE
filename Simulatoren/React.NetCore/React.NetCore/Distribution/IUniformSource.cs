//=============================================================================
//=  $Id: IUniformSource.cs 178 2006-10-07 16:50:58Z eroe $
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

namespace React.Distribution
{
    /// <summary>
    /// An object that can supply an <see cref="IUniform"/> random number
    /// generator on demand.
    /// </summary>
    /// <remarks>
    /// An <see cref="IUniformSource"/> may be a <em>factory</em> object that
    /// creates a new <see cref="IUniform"/> each time <see cref="GetUniform"/>
    /// is called; or it may return an <see cref="IUniform"/> from a pool of
    /// one or more <see cref="IUniform"/> instances.
    /// </remarks>
    public interface IUniformSource
    {
        /// <summary>
        /// Returns an <see cref="IUniform"/> random number generator.
        /// </summary>
        /// <returns>
        /// An <see cref="IUniform"/> random number generator.
        /// </returns>
        IUniform GetUniform();
    }
}
