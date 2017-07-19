//=============================================================================
//=  $Id: Exponential.cs 185 2006-10-14 18:53:40Z eroe $
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
    /// Generates random values according to an <em>exponential</em>
    /// distribution.
    /// </summary>
    public class Exponential : NonUniform
    {
        /// <summary>
        /// The default lambda value.
        /// </summary>
        /// <remarks>
        /// This value is used by the constructors that do not take an explicit
        /// lambda value.
        /// </remarks>
        public const double DefaultLambda = 1.0;

        /// <summary>
        /// The lambda value.
        /// </summary>
        private double _lambda;

        /// <overloads>
        /// Create and initialize an Exponential random number generator.
        /// </overloads>
        /// <summary>
        /// Create an <see cref="Exponential"/> random number generator.
        /// </summary>
        /// <remarks>
        /// The <see cref="Lambda"/> for the <see cref="Exponential"/> is
        /// set to <see cref="DefaultLambda"/> and the underlying
        /// <see cref="Uniform"/> generator is obtained from the
        /// default set of <see cref="Uniform"/> generators (see
        /// <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        public Exponential() : this(DefaultLambda)
        {
        }

        /// <summary>
        /// Create an <see cref="Exponential"/> random number generator that
        /// has the given lambda value.
        /// </summary>
        /// <remarks>
        /// The underlying <see cref="Uniform"/> generator is obtained from the
        /// default set of <see cref="Uniform"/> generators (see
        /// <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        /// <param name="lambda">
        /// The lambda value.  This value is defined as 1/Mean.
        /// </param>
        public Exponential(double lambda)
        {
            this.Lambda = lambda;
        }

        /// <summary>
        /// Create an <see cref="Exponential"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/>.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Exponential"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        public Exponential(IUniformSource source)
            : this(source, DefaultLambda)
        {
        }

        /// <summary>
        /// Create an <see cref="Exponential"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/> and has the given lamdba value.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Exponential"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        /// <param name="lambda">
        /// The lambda value.  This value is defined as 1/Mean.
        /// </param>
        public Exponential(IUniformSource source, double lambda)
            : base(source)
        {
            this.Lambda = lambda;
        }

        /// <summary>
        /// Gets or sets the lambda value.
        /// </summary>
        /// <remarks>
        /// The lambda value is defined as 1/<see cref="Mean"/>.  It is not
        /// necessary to set both <see cref="Lambda"/> and <see cref="Mean"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If an attempt is made to set this property to a value less than or
        /// equal to zero (0.0).
        /// </exception>
        /// <value>
        /// The lambda of the distribution as a <see cref="double"/>.
        /// </value>
        public double Lambda
        {
            get
            {
                return _lambda;
            }
            set
            {
                if (value <= 0.0)
                    throw new ArgumentException(
                        "Lambda must be greater than zero.");

                _lambda = value;
            }
        }

        /// <summary>
        /// Gets or sets the desired mean value of the distribution.
        /// </summary>
        /// <remarks>
        /// The mean value is defined as 1/<see cref="Lambda"/>.  It is not
        /// necessary to set both <see cref="Mean"/> and <see cref="Lambda"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If an attempt is made to set this property to a value less than or
        /// equal to zero (0.0).
        /// </exception>
        /// <value>
        /// The desired mean of the distribution as a <see cref="double"/>.
        /// </value>
        public double Mean
        {
            get { return 1.0 / Lambda; }
            set
            {
                if (value <= 0.0)
                    throw new ArgumentException(
                        "Mean must be greater than zero.");

                this.Lambda = 1.0 / value;
            }
        }

        /// <summary>
        /// Generates the next random value according to an exponential
        /// distribution.
        /// </summary>
        /// <returns>
        /// The next random value.
        /// </returns>
        public override double NextDouble()
        {
            IUniform u = GetUniform();
            double d = u.NextDouble();
            for (; d <= 1e-10; d = u.NextDouble()) ; // <-- SEMI!
            return -Math.Log(d) / Lambda;
        }
    }
}
