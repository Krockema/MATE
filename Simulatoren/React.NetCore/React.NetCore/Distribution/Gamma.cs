//=============================================================================
//=  $Id: Gamma.cs 185 2006-10-14 18:53:40Z eroe $
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
    /// Generates random values according to a <em>gamma</em>
    /// distribution.
    /// </summary>
    public class Gamma : NonUniform
    {
        /// <summary>
        /// The default scale parameter.
        /// </summary>
        /// <remarks>
        /// This value is used by the constructors that do not take an explicit
        /// scale value.
        /// </remarks>
        public const double DefaultScale = 1.0;
        /// <summary>
        /// The default shape parameter.
        /// </summary>
        /// <remarks>
        /// This value is used by the constructors that do not take an explicit
        /// shape value.
        /// </remarks>
        public const double DefaultShape = 1.0;

        private static readonly double Log4 = Math.Log(4.0);
        private static readonly double GammaMagicConst = 1.0 + Math.Log(4.5);

        /// <summary>
        /// The scale parameter.  Also referred to as <em>alpha</em>.
        /// </summary>
        private double _scale;  // alpha
        /// <summary>
        /// The shape parameter.  Also referred to as <em>beta</em>.
        /// </summary>
        private double _shape;  // beta

        /// <overloads>
        /// Create and initialize a Gamma random number generator.
        /// </overloads>
        /// <summary>
        /// Create a <see cref="Gamma"/> random number generator.
        /// </summary>
        /// <remarks>
        /// The <see cref="Scale"/> and <see cref="Shape"/> for the
        /// <see cref="Gamma"/> are set to <see cref="DefaultScale"/> and
        /// <see cref="DefaultShape"/> respectively.  The underlying
        /// <see cref="Uniform"/> generator is obtained from the
        /// default set of <see cref="Uniform"/> generators (see
        /// <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        public Gamma()
            : this(DefaultScale, DefaultShape)
        {
        }

        /// <summary>
        /// Create an <see cref="Gamma"/> random number generator that
        /// has the given shape parameter.
        /// </summary>
        /// <remarks>
        /// The underlying <see cref="Uniform"/> generator is obtained from the
        /// default set of <see cref="Uniform"/> generators (see
        /// <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        /// <param name="scale">
        /// The scale parameter.  This value is often referred to as
        /// <em>alpha</em>.</param>
        /// <param name="shape">
        /// The shape parameter.  This value is often referred to as
        /// <em>beta</em>.
        /// </param>
        public Gamma(double scale, double shape)
        {
            this.Scale = scale;
            this.Shape = shape;
        }

        /// <summary>
        /// Create an <see cref="Gamma"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Scale"/> and <see cref="Shape"/> for the
        /// <see cref="Gamma"/> are set to <see cref="DefaultScale"/> and
        /// <see cref="DefaultShape"/> respectively.
        /// </remarks>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Gamma"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        public Gamma(IUniformSource source)
            : this(source, DefaultScale, DefaultShape)
        {
        }

        /// <summary>
        /// Create an <see cref="Gamma"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/> and has the given scale and
        /// shape parameters.
        /// </summary>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Gamma"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        /// <param name="scale">
        /// The scale parameter.  This value is often referred to as
        /// <em>alpha</em>.</param>
        /// <param name="shape">
        /// The shape parameter.  This value is often referred to as
        /// <em>beta</em>.
        /// </param>
        public Gamma(IUniformSource source, double scale, double shape)
            : base(source)
        {
            this.Scale = scale;
            this.Shape = shape;
        }

        /// <summary>
        /// Gets or sets the scale parameter.
        /// </summary>
        /// <remarks>
        /// The scale parameter is often shown as <em>alpha</em>.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If an attempt is made to set this property to a value less than or
        /// equal to zero (0.0).
        /// </exception>
        /// <value>
        /// The scale parameter as a <see cref="double"/>.
        /// </value>
        public double Scale
        {
            get { return _scale; }
            set
            {
                if (value == 0.0)
                    throw new ArgumentException("Scale cannot be zero.");
                _scale = value;
            }
        }

        /// <summary>
        /// Gets or sets the shape parameter.
        /// </summary>
        /// <remarks>
        /// The shape parameter is often shown as <em>beta</em>.
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
        /// Generates the next random value according to a Gamma
        /// distribution.
        /// </summary>
        /// <returns>
        /// The next random value.
        /// </returns>
        public override double NextDouble()
        {
            return Gamma.Generate(GetUniform(), Scale, Shape);
        }

        /// <summary>
        /// Static method used by both <see cref="Gamma"/> and
        /// <see cref="Beta"/> to generate gamma distributed pseudo
        /// random values.
        /// </summary>
        /// <param name="u">The uniform generator.</param>
        /// <param name="alpha">The scale parameter.</param>
        /// <param name="beta">The shape parameter.</param>
        /// <returns>
        /// A pseudo random value that is distributed according to a
        /// gamma distribution.
        /// </returns>
        internal static double Generate(IUniform u, double alpha, double beta)
        {
            double d = u.NextDouble();
            double result;

            if (alpha > 1.0)
            {
                double ainv = Math.Sqrt(2.0 * alpha - 1.0);
                double bbb = alpha - Log4;
                double ccc = alpha + ainv;

                for (; ; d = u.NextDouble())
                {
                    if (d > 1e-10 && d < 1.0)
                    {
                        double d2 = 1.0 - u.NextDouble();
                        double v = Math.Log(d / (1.0 - d)) / ainv;
                        double x = alpha * Math.Exp(v);
                        double z = d * d * d2;
                        double r = bbb + ccc * v - x;
                        if (r + GammaMagicConst - 4.5 * z >= 0.0 ||
                            r >= Math.Log(z))
                        {
                            result = x * beta;
                            break;
                        }
                    }
                }
            }
            else if (alpha == 1.0)
            {
                for (; d < 1e-10; d = u.NextDouble()) ;
                result = -Math.Log(d) * beta;
            }
            else
            {
                for (; ; d = u.NextDouble())
                {
                    double x;
                    double b = (Math.E + alpha) / Math.E;
                    double p = b * d;
                    if (p <= 1.0)
                        x = Math.Pow(p, 1.0 / alpha);
                    else
                        x = -Math.Log((b - p) / alpha);

                    double d2 = u.NextDouble();

                    if (!(p <= 1.0 && d2 > Math.Exp(-x) ||
                        p > 1.0 && d2 > Math.Pow(x, alpha - 1.0)))
                    {
                        result = x * beta;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
