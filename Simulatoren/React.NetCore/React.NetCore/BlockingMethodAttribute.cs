//=============================================================================
//=  $Id: BlockingMethodAttribute.cs 178 2006-10-07 16:50:58Z eroe $
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

namespace React
{
	/// <summary>
	/// Attribute used to flag a method that has blocking semantics.
	/// </summary>
	/// <remarks>
    /// <para>
    /// Methods that have this attribute must return a <see cref="Task"/>
    /// instance.  This attribute is used solely to document methods that
    /// return a <see cref="Task"/> that must be blocked upon.  The returned
    /// <see cref="Task"/> will perform some action on behalf of another
    /// <see cref="Task"/>.
    /// </para>
    /// <para>
    /// This attribute can only be applied to methods.
    /// </para>
    /// <example>
    /// <para>
    /// A typical use of a blocking method from a <see cref="Process"/>
    /// (specifically from the <see cref="Process.GetProcessSteps"/> method)
    /// might look something like this:
    /// </para>
    /// <para>
    /// <code>
    /// IResource resource = /* get a resource */
    /// yield return resource.Acquire(this);</code>
    /// </para>
    /// <para>
    /// In the above case, the <see cref="IResource.Acquire(Task)"/> method is
    /// a blocking method.  The <see cref="Task"/> it returns is blocked upon
    /// by the <see cref="Process"/> (by virtue of the <c>yield return</c>
    /// statement).
    /// </para>
    /// </example>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method, Inherited=true, AllowMultiple=false)]
	public sealed class BlockingMethodAttribute : Attribute
	{
        /// <summary>
        /// Create a new <see cref="BlockingMethodAttribute"/> instance.
        /// </summary>
		public BlockingMethodAttribute() {}
	}
}
