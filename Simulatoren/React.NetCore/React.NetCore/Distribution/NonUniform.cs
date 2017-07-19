//=============================================================================
//=  $Id: NonUniform.cs 185 2006-10-14 18:53:40Z eroe $
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
    /// Base class for all React.NET non-uniform random number generators.
    /// </summary>
    public abstract class NonUniform : IRandom, IUniformSource
    {
        /// <summary>
        /// The <see cref="IUniform"/> random number generator used to
        /// generate non-uniformly distributed values.
        /// </summary>
        private IUniform _rng;

        /// <summary>
        /// Create a new <see cref="NonUniform"/> that obtains its
        /// <see cref="IUniform"/> generator from the set of default random
        /// numbers.
        /// <seealso cref="UniformStreams.DefaultStreams"/>
        /// </summary>
        protected NonUniform()
        {
            _rng = UniformStreams.DefaultStreams.GetUniform();
        }

        /// <summary>
        /// Create a new <see cref="NonUniform"/> that obtains its
        /// <see cref="IUniform"/> generator from the given
        /// <see cref="IUniformSource"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="source"/> is <see langword="null"/>, this
        /// constructor behaves exactly like the no-arg constructor and
        /// obtains an <see cref="IUniform"/> generator from the set of
        /// default random numbers.
        /// </remarks>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which to obtain the
        /// <see cref="IUniform"/> generator.
        /// </param>
        protected NonUniform(IUniformSource source)
        {
            if (source != null)
            {
                _rng = source.GetUniform();
            }
            else
            {
                _rng = UniformStreams.DefaultStreams.GetUniform();
            }
        }

        /// <summary>
        /// Generate and return the next non-uniformly distributed
        /// random <see cref="double"/> value.
        /// </summary>
        /// <returns>
        /// The next random value as a <see cref="double"/>.
        /// </returns>
        public abstract double NextDouble();

        /// <summary>
        /// Generate and return the next non-uniformly distributed
        /// random <see cref="float"/> value.
        /// </summary>
        /// <remarks>
        /// The default implementation of this method simply returns
        /// <see cref="NextDouble"/> cast as a <see cref="float"/>.
        /// </remarks>
        /// <returns>
        /// The next random value as a <see cref="float"/>.
        /// </returns>
        public virtual float NextSingle()
        {
            return (float)NextDouble();
        }

        /// <summary>
        /// Returns the <see cref="IUniform"/> instance used by the
        /// <see cref="NonUniform"/> as its generator.
        /// </summary>
        /// <remarks>
        /// Multiple calls to this method will <b>always</b> return the
        /// same <see cref="IUniform"/> instance.
        /// </remarks>
        /// <returns>
        /// The <see cref="NonUniform"/> instance's uniform
        /// random number generator.
        /// </returns>
        public IUniform GetUniform()
        {
            return _rng;
        }
    }
}
