//=============================================================================
//=  $Id: Uniform.cs 139 2006-03-19 22:46:17Z Eric Roe $
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

namespace React.Distribution
{
    /// <summary>
    /// Base class for all React.NET uniform random number generators.
    /// </summary>
    public abstract class Uniform : IUniform, IUniformSource
    {
        /// <summary>
        /// Create a new <see cref="Uniform"/> instance.
        /// </summary>
        protected Uniform()
        {
        }

        /// <summary>
        /// Returns this <see cref="Uniform"/> instance.
        /// </summary>
        /// <returns>
        /// Returns <c>this</c>.
        /// </returns>
        public IUniform GetUniform()
        {
            return this;
        }

        /// <summary>
        /// Generates and returns the next uniformly distributed
        /// <see cref="double"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as a <see cref="double"/>.
        /// </returns>
        public virtual double NextDouble()
        {
            long l = NextLong();
            return (double)l / (double)Int64.MaxValue;
        }

        /// <summary>
        /// Generates and returns the next uniformly distributed
        /// <see cref="float"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as a <see cref="float"/>.
        /// </returns>
        public virtual float NextSingle()
        {
            int i = NextInteger();
            return (float)i / (float)Int32.MaxValue;
        }

        /// <summary>
        /// Generates and returns the next uniformly distributed
        /// <see cref="int"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as an <see cref="int"/>.
        /// </returns>
        public abstract int NextInteger();

        /// <summary>
        /// Generates and returns the next uniformly distributed
        /// <see cref="long"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as an <see cref="long"/>.
        /// </returns>
        public abstract long NextLong();

        /// <summary>
        /// Create a new <see cref="Uniform"/> instance having a seed value
        /// based on the current system time.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="Uniform"/> uses a
        /// <see cref="System.Random"/> instance to do the actual random number
        /// generation.
        /// </remarks>
        /// <returns>
        /// A new <see cref="Uniform"/> instance having a seed value based on
        /// the current system time.</returns>
        public static Uniform Create()
        {
            return new SystemUniform();
        }

        /// <summary>
        /// Create a new <see cref="Uniform"/> instance using the given seed
        /// value.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="Uniform"/> uses a
        /// <see cref="System.Random"/> instance to do the actual random number
        /// generation.
        /// </remarks>
        /// <param name="seed">The seed value.</param>
        /// <returns>
        /// A new <see cref="Uniform"/> instance using <paramref name="seed"/>
        /// for the seed value.
        /// </returns>
        public static Uniform Create(int seed)
        {
            return new SystemUniform(seed);
        }
    }
}
