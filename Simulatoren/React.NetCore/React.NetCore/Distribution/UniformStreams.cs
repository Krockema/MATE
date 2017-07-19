//=============================================================================
//=  $Id: UniformStreams.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Collections;
using System.Collections.Generic;

namespace React.Distribution
{
    /// <summary>
    /// A set of <see cref="IUniform"/> random number generators.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is used to provide different <see cref="IUniform"/>
    /// generators to non-uniform random variates.  Essentially, calls
    /// to <see cref="GetUniform"/> operate as if the set was a ring
    /// buffer.  Each call to <see cref="GetUniform"/> returns the next
    /// <see cref="IUniform"/> in the ring buffer, starting over at the
    /// first <see cref="IUniform"/> when the last <see cref="IUniform"/>
    /// is returned.
    /// </para>
    /// <para>
    /// The above behavior allows the various non-uniform random number
    /// generators used in a <see cref="Simulation"/> to draw from several
    /// <see cref="IUniform"/> generators in a well-defined manner.
    /// </para>
    /// <para>
    /// Unless explicitly set, the default (system-wide)
    /// <see cref="UniformStreams"/> will contain
    /// <see cref="DefaultStreamCount"/> <see cref="IUniform"/> random
    /// number generators.
    /// </para>
    /// </remarks>
    public class UniformStreams : IUniformSource
    {
        /// <summary>
        /// Default quantity of random number generators.
        /// </summary>
        public const int DefaultStreamCount = 10;

        /// <summary>
        /// The default <see cref="UniformStreams"/> instance.
        /// </summary>
        private static UniformStreams _defaultStreams;

        /// <summary>
        /// Index of the next <see cref="IUniform"/> that
        /// <see cref="GetUniform"/> will return.
        /// </summary>
        private int _index;
        /// <summary>
        /// Array containing all <see cref="IUniform"/> instances.
        /// </summary>
        private IUniform[] _prngs;

        /// <overloads>
        /// Create and initialize a UniformStreams instance.
        /// </overloads>
        /// <summary>
        /// <remarks>
        /// The <see cref="IUniform"/> instances are seeded based on the
        /// current system time.
        /// </remarks>
        /// Create a <see cref="UniformStreams"/> containing
        /// <see cref="DefaultStreamCount"/> <see cref="IUniform"/> instances.
        /// </summary>
        public UniformStreams() : this(DefaultStreamCount)
        {
        }

        /// <summary>
        /// Create a <see cref="UniformStreams"/> containing the specified
        /// number of <see cref="IUniform"/> instances.
        /// </summary>
        /// <remarks>
        /// The <see cref="IUniform"/> instances are seeded based on the
        /// current system time.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If <paramref name="count"/> is less than one (1).
        /// </exception>
        /// <param name="count">
        /// The number of <see cref="IUniform"/> instances the newly created
        /// <see cref="UniformStreams"/> will contain.
        /// </param>
        public UniformStreams(int count)
            : this(count, (int)(DateTime.Now.Ticks & 0xFFFFFFFFL))
        {
        }

        /// <summary>
        /// Create a <see cref="UniformStreams"/> containing the specified
        /// number of <see cref="IUniform"/> instances and having the given
        /// starting seed value.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="count"/> is less than one (1).
        /// </exception>
        /// <param name="count">
        /// The number of <see cref="IUniform"/> instances the newly created
        /// <see cref="UniformStreams"/> will contain.
        /// </param>
        /// <param name="seed">
        /// The starting seed value.  The first <see cref="IUniform"/> is
        /// seeded with this value.  Subsequent <see cref="IUniform"/>
        /// instances are seeded based on the <see cref="IUniform.NextInteger"/>
        /// random value obtained from their predecessor.
        /// </param>
        public UniformStreams(int count, int seed)
        {
            if (count < 1)
                throw new ArgumentException("Must be 1 or more.", "count");
            _prngs = new IUniform[count];
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    seed = _prngs[i - 1].NextInteger();
                }

                _prngs[i] = Uniform.Create(seed);
            }
        }

        /// <summary>
        /// Create a <see cref="UniformStreams"/> that contains the given
        /// <see cref="IUniform"/> instances.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="rngs"/> contains less than one
        /// <see cref="IUniform"/>.
        /// </exception>
        /// <param name="rngs">
        /// An <see cref="IEnumerable"/> that can be iterated over to obtain
        /// one or more <see cref="IUniform"/> instances.
        /// </param>
        public UniformStreams(IEnumerable rngs)
        {
            List<IUniform> list = new List<IUniform>();
            foreach (IUniform u in rngs)
            {
                if (u != null)
                    list.Add(u);
            }

            _prngs = new IUniform[list.Count];
            list.CopyTo(_prngs);

            if (_prngs.Length < 1)
                throw new ArgumentException(
                    "Must contain 1 or more IUniform instances.", "rngs");
        }

        /// <summary>
        /// Gets the number of <see cref="IUniform"/> instances contained by
        /// this <see cref="UniformStreams"/>.
        /// </summary>
        /// <value>
        /// The the number of <see cref="IUniform"/> instances contained by
        /// this <see cref="UniformStreams"/> as an <see cref="int"/>.
        /// </value>
        public int Length
        {
            get { return _prngs.Length; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IUniform"/> at the specified index.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// If <paramref name="index"/> is not greater or equal to zero and
        /// less than <see cref="Length"/>.
        /// </exception>
        /// <param name="index">
        /// The index.  Must be greater or equal to zero and less than
        /// <see cref="Length"/>.
        /// </param>
        /// <value>
        /// The <see cref="IUniform"/> at the specified index.
        /// </value>
        public IUniform this[int index]
        {
            get
            {
                return _prngs[index];
            }

            set
            {
                _prngs[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the default, system-wide <see cref="UniformStreams"/>
        /// instance.
        /// </summary>
        /// <remarks>
        /// If this property is set to <see langword="null"/>, a new default
        /// <see cref="UniformStreams"/> instance will automatically created
        /// the next time this property is queried.
        /// </remarks>
        /// <value>
        /// The default, system-wide <see cref="UniformStreams"/> instance.
        /// </value>
        public static UniformStreams DefaultStreams
        {
            get
            {
                if (_defaultStreams == null)
                {
                    _defaultStreams = new UniformStreams();
                }
                return _defaultStreams;
            }

            set
            {
                _defaultStreams = value;
            }
        }

        /// <summary>
        /// Returns the next <see cref="IUniform"/> random number generator.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each time this method is called a different <see cref="IUniform"/>
        /// is returned until the last <see cref="IUniform"/> in the set is
        /// returned.  At that point, the cycle begins again with the first
        /// <see cref="IUniform"/> in the set.
        /// </para>
        /// <para>
        /// If <see cref="Length"/> is one (1), then this method will always
        /// return the same <see cref="IUniform"/>.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The next <see cref="IUniform"/> random number generator.
        /// </returns>
        public IUniform GetUniform()
        {
            IUniform rng = _prngs[_index++];
            if (_index >= _prngs.Length)
                _index = 0;
            return rng;
        }
    }
}
