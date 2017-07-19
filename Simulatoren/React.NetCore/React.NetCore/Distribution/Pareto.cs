//=============================================================================
//=  $Id: Pareto.cs 185 2006-10-14 18:53:40Z eroe $
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
    /// Generates random values according to a <em>Pareto</em>
    /// distribution.
    /// </summary>
    public class Pareto : NonUniform
    {
        /// <summary>
        /// The default shape parameter.
        /// </summary>
        /// <remarks>
        /// This value is used by the constructors that do not take an explicit
        /// shape value.
        /// </remarks>
        public const double DefaultShape = 1.0;

        /// <summary>
        /// The shape parameter.  Also referred to as <em>alpha</em>.
        /// </summary>
        private double _shape;  // alpha

        /// <overloads>
        /// Create and initialize a Pareto random number generator.
        /// </overloads>
        /// <summary>
        /// Create a <see cref="Pareto"/> random number generator.
        /// </summary>
        /// <remarks>
        /// The <see cref="Shape"/> for the <see cref="Pareto"/> is set to
        /// <see cref="DefaultShape"/> and the underlying
        /// <see cref="Uniform"/> generator is obtained from the
        /// default set of <see cref="Uniform"/> generators (see
        /// <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        public Pareto() : this(DefaultShape)
        {
        }

        /// <summary>
        /// Create an <see cref="Pareto"/> random number generator that
        /// has the given shape parameter.
        /// </summary>
        /// <remarks>
        /// The underlying <see cref="Uniform"/> generator is obtained from the
        /// default set of <see cref="Uniform"/> generators (see
        /// <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        /// <param name="shape">
        /// The shape parameter.  This value is often referred to as
        /// <em>alpha</em>.
        /// </param>
        public Pareto(double shape)
        {
            this.Shape = shape;
        }

        /// <summary>
        /// Create an <see cref="Pareto"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Shape"/> for the <see cref="Pareto"/> is set to
        /// <see cref="DefaultShape"/>.
        /// </remarks>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Pareto"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        public Pareto(IUniformSource source)
            : this(source, DefaultShape)
        {
        }

        /// <summary>
        /// Create an <see cref="Pareto"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/> and has the given shape
        /// parameter.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Pareto"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        /// <param name="shape">
        /// The shape parameter.  This value is often referred to as
        /// <em>alpha</em>.
        /// </param>
        public Pareto(IUniformSource source, double shape)
            : base(source)
        {
            this.Shape = shape;
        }

        /// <summary>
        /// Gets or sets the shape parameter.
        /// </summary>
        /// <remarks>
        /// The shape parameter is often shown as <em>alpha</em>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If an attempt is made to set this property to a value less than or
        /// equal to zero (0.0).
        /// </exception>
        /// <value>
        /// The shape parameter as a <see cref="double"/>.
        /// </value>
        public double Shape
        {
            get { return _shape; }
            set
            {
                if (value == 0.0)
                    throw new ArgumentException("Shape cannot be zero.");
                _shape = value;
            }
        }

        /// <summary>
        /// Generates the next random value according to a Pareto
        /// distribution.
        /// </summary>
        /// <returns>
        /// The next random value.
        /// </returns>
        public override double NextDouble()
        {
            IUniform u = GetUniform();
            double d = 1.0 - u.NextDouble();
            return 1.0 / Math.Pow(d, 1.0 / Shape);
        }
    }
}
