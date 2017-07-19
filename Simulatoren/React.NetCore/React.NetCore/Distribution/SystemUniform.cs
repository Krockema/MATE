//=============================================================================
//=  $Id: SystemUniform.cs 138 2006-03-19 22:45:00Z Eric Roe $
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
    /// An <see cref="IUniform"/> pseudo-random number generator that uses the
    /// <see cref="System.Random"/> class to generate random values.
    /// </summary>
    internal class SystemUniform : Uniform
    {
        /// <summary>
        /// The <see cref="System.Random"/> used to generate random values.
        /// </summary>
        private Random _gen;

        /// <overloads>
        /// Create an initialize a SystemUniform instance.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="SystemUniform"/> having a seed based on
        /// the current time.
        /// </summary>
        internal SystemUniform()
        {
            _gen = new Random();
        }

        /// <summary>
        /// Create a new <see cref="SystemUniform"/> having the given seed
        /// value.
        /// </summary>
        /// <param name="seed">
        /// The seed value.
        /// </param>
        internal SystemUniform(int seed)
        {
            _gen = new Random(seed);
        }

        /// <summary>
        /// Generates the next uniformly distributed <see cref="double"/>
        /// value.
        /// </summary>
        /// <returns>
        /// The next random value as a <see cref="double"/>.
        /// </returns>
        public override double NextDouble()
        {
            return _gen.NextDouble();
        }

        /// <summary>
        /// Generate and return the next uniformly distributed
        /// <see cref="int"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as an <see cref="int"/>.
        /// </returns>
        public override int NextInteger()
        {
            return _gen.Next();
        }

        /// <summary>
        /// Generate and return the next uniformly distributed
        /// <see cref="long"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as an <see cref="long"/>.
        /// </returns>
        public override long NextLong()
        {
            return _gen.Next() << 32 + _gen.Next();
        }
    }
}
