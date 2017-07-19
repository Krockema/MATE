//=============================================================================
//=  $Id: Triangular.cs 185 2006-10-14 18:53:40Z eroe $
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
    /// Generates random values according to a <em>triangular</em>
    /// distribution.
    /// </summary>
    public class Triangular : NonUniform
    {
        /// <summary>
        /// The default minimum value.
        /// </summary>
        /// <remarks>
        /// This value is used by the constructors that do not take an explicit
        /// minimum value.
        /// </remarks>
        public const double DefaultMinimum = -1.0;
        /// <summary>
        /// The default mode value.
        /// </summary>
        /// <remarks>
        /// This value is used by the constructors that do not take an explicit
        /// mode value.
        /// </remarks>
        public const double DefaultMode = 0.0;
        /// <summary>
        /// The default maximum value.
        /// </summary>
        /// <remarks>
        /// This value is used by the constructors that do not take an explicit
        /// maximum value.
        /// </remarks>
        public const double DefaultMaximum = 1.0;

        /// <summary>The minimum value.</summary>
        private double _min;
        /// <summary>The mode.</summary>
        private double _mode;
        /// <summary>The maximum value.</summary>
        private double _max;

        /// <overloads>
        /// Create and initialize a Triangular random number generator.
        /// </overloads>
        /// <summary>
        /// Create a <see cref="Triangular"/> random number generator.
        /// </summary>
        /// <remarks>
        /// The <see cref="Minimum"/>, <see cref="Mode"/>, and
        /// <see cref="Maximum"/> are set to <see cref="DefaultMinimum"/>,
        /// <see cref="DefaultMode"/>, and <see cref="DefaultMaximum"/>
        /// respectively.  The underlying <see cref="Uniform"/> generator is
        /// obtained from the default set of <see cref="Uniform"/> generators
        /// (see <see cref="UniformStreams.DefaultStreams"/>).
        /// </remarks>
        public Triangular() : this(DefaultMinimum, DefaultMode, DefaultMaximum)
        {
        }

        /// <summary>
        /// Create a <see cref="Triangular"/> random number generator having
        /// specified minimum, mode, and maximum.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="min"/> is not less than <paramref name="max"/>;
        /// or if <paramref name="mode"/> does not lie between
        /// <paramref name="min"/> and <paramref name="max"/>.
        /// </exception>
        /// <param name="min">The minimum value.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="max">The maximum value.</param>
        public Triangular(double min, double mode, double max)
        {
            Reset(min, mode, max);
        }

        /// <summary>
        /// Create an <see cref="Triangular"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Minimum"/>, <see cref="Mode"/>, and
        /// <see cref="Maximum"/> are set to <see cref="DefaultMinimum"/>,
        /// <see cref="DefaultMode"/>, and <see cref="DefaultMaximum"/>
        /// respectively.
        /// </remarks>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Triangular"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        public Triangular(IUniformSource source)
            : this(source, DefaultMinimum, DefaultMode, DefaultMaximum)
        {
        }

        /// <summary>
        /// Create an <see cref="Triangular"/> random number generator that
        /// obtains its underlying <see cref="Uniform"/> generator from the
        /// given <see cref="IUniformSource"/> and has the given minimum,
        /// mode, and maximum.
        /// parameter.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// If <paramref name="min"/> is not less than <paramref name="max"/>;
        /// or if <paramref name="mode"/> does not lie between
        /// <paramref name="min"/> and <paramref name="max"/>.
        /// </exception>
        /// <param name="source">
        /// The <see cref="IUniformSource"/> from which this
        /// <see cref="Triangular"/> can obtain its underlying
        /// <see cref="Uniform"/> generator.
        /// </param>
        /// <param name="min">The minimum value.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="max">The maximum value.</param>
        public Triangular(IUniformSource source,
            double min, double mode, double max)
            : base(source)
        {
            Reset(min, mode, max);
        }

        /// <summary>
        /// Gets the minimum value the <see cref="Triangular"/> will generate.
        /// </summary>
        /// <remarks>
        /// Because <see cref="Minimum"/>, <see cref="Mode"/>, and
        /// <see cref="Maximum"/> are interdependent, this property can only
        /// be set in the constructor or by the <see cref="Reset"/> method.
        /// </remarks>
        /// <value>
        /// The minimum value as a <see cref="double"/>.
        /// </value>
        public double Minimum
        {
            get { return _min; }
        }

        /// <summary>
        /// Gets the mode for the <see cref="Triangular"/> distribution.
        /// </summary>
        /// <remarks>
        /// Approximately half of all generated values will be less than
        /// <see cref="Mode"/>, and half will be greater.
        /// <para>
        /// Because <see cref="Minimum"/>, <see cref="Mode"/>, and
        /// <see cref="Maximum"/> are interdependent, this property can only
        /// be set in the constructor or by the <see cref="Reset"/> method.
        /// </para>
        /// </remarks>
        /// <value>
        /// The mode as a <see cref="double"/>.
        /// </value>
        public double Mode
        {
            get { return _mode; }
        }

        /// <summary>
        /// Gets the maximum value the <see cref="Triangular"/> will generate.
        /// </summary>
        /// <remarks>
        /// Because <see cref="Minimum"/>, <see cref="Mode"/>, and
        /// <see cref="Maximum"/> are interdependent, this property can only
        /// be set in the constructor or by the <see cref="Reset"/> method.
        /// </remarks>
        /// <value>
        /// The maximum value as a <see cref="double"/>.
        /// </value>
        public double Maximum
        {
            get { return _max; }
        }

        /// <summary>
        /// Gets the width of the <see cref="Triangular"/> random number
        /// generator.
        /// </summary>
        /// <remarks>
        /// The width is the <see cref="Maximum"/> less the
        /// <see cref="Minimum"/>.
        /// </remarks>
        /// <value>
        /// The width as a <see cref="double"/>.
        /// </value>
        public double Width
        {
            get { return _max - _min; }
        }

        /// <summary>
        /// Resets the <see cref="Triangular"/> to have the specified minimum,
        /// mode, and maximum.
        /// </summary>
        /// <remarks>
        /// Because <see cref="Minimum"/>, <see cref="Mode"/>, and
        /// <see cref="Maximum"/> are interdependent, this method is used to
        /// set all three values simultaneously with appropriate error
        /// checking.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// If <paramref name="min"/> is not less than <paramref name="max"/>;
        /// or if <paramref name="mode"/> does not lie between
        /// <paramref name="min"/> and <paramref name="max"/>.
        /// </exception>
        /// <param name="min">The minimum value.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="max">The maximum value.</param>
        public void Reset(double min, double mode, double max)
        {
            if (min >= max)
                throw new ArgumentException(
                    "Cannot be greater or equal to max.", "min");
            if (mode <= min || mode >= max)
                throw new ArgumentException(
                    "Must be between min and max.", "mode");

            _min = min;
            _mode = mode;
            _max = max;
        }

        /// <summary>
        /// Generates the next random value according to a triangular
        /// distribution.
        /// </summary>
        /// <returns>
        /// The next random value.
        /// </returns>
        public override double NextDouble()
        {
            IUniform u = GetUniform();
            double d = u.NextDouble();
            double w = Width;
            double split = (_mode - _min) / w;
            double result;

            if (d <= split)
            {
                result = _min + Math.Sqrt(w * (_mode - _min) * d);
            }
            else
            {
                result = _max - Math.Sqrt(w * (_max - _mode) * (1.0 - d));
            }

            return result;

        }
    }
}
