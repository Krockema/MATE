//=============================================================================
//=  $Id: Count.cs 184 2006-10-14 18:46:48Z eroe $
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

namespace React.Monitoring
{
    /// <summary>
    /// A <see cref="Statistic"/> that keeps a running count of a series of
    /// observations.
    /// </summary>
    public class Count : Statistic
    {
        /// <summary>
        /// The current count.
        /// </summary>
        private int _count;
        /// <summary>
        /// The value to increment <see cref="_count"/> by on each observation.
        /// </summary>
        private int _increment;

        /// <overloads>
        /// Create and initialize a Count summary statistic instance.
        /// </overloads>
        /// <summary>
        /// Create a new <see cref="Count"/> statistic that begins counting at
        /// zero (0) and increments by one (1).
        /// </summary>
        public Count() : this(0, 1)
        {
        }

        /// <summary>
        /// Create a new <see cref="Count"/> statistic that begins counting at
        /// the specified value and increments by one (1).
        /// </summary>
        /// <param name="initialValue">
        /// The starting value for the <see cref="Count"/>.
        /// </param>
        public Count(int initialValue) : this(initialValue, 1)
        {
        }

        /// <summary>
        /// Create a new <see cref="Count"/> statistic that counts using the
        /// specified starting value and increment.
        /// </summary>
        /// <param name="initialValue">
        /// The starting value for the <see cref="Count"/>.
        /// </param>
        /// <param name="increment">
        /// The value to increment by.</param>
        public Count(int initialValue, int increment)
        {
            _count = initialValue;
            _increment = increment;
        }

        /// <summary>
        /// Gets or sets the value to increment the count by on each
        /// observation.
        /// </summary>
        /// <value>
        /// The value to increment the count by on each observation as an
        /// <see cref="int"/>.
        /// </value>
        public int IncrementBy
        {
            get { return _increment; }
            set { _increment = value; }
        }

        /// <summary>
        /// Record an observation.
        /// </summary>
        /// <remarks>
        /// This method increments the current count <see cref="Value"/> by
        /// <see cref="IncrementBy"/>.
        /// </remarks>
        /// <param name="value">Not used.</param>
        public override void Observe(double value)
        {
            _count += _increment;
            base.Observe(Value);
        }

        /// <summary>
        /// Gets the <see cref="Count"/> value as an <see cref="int"/>.
        /// </summary>
        /// <value>
        /// The count as an <see cref="int"/>.
        /// </value>
        public int IntegerValue
        {
            get { return _count; }
        }

        /// <summary>
        /// Gets the <see cref="Count"/> value as a <see cref="double"/>.
        /// </summary>
        /// <value>
        /// The count as a <see cref="double"/>.
        /// </value>
        public override double Value
        {
            get { return _count; }
        }

        /// <summary>
        /// Method that allows counting of arbitrary events raised providing
        /// the event takes a <see cref="EventHandler"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method allows the <see cref="Count"/> instance to count
        /// pretty much any event raised during a program run.  All that is
        /// required is that the event have the signature
        /// </para>
        /// <code>public event MyEvent(object sender, EventArgs args);</code>
        /// <para>
        /// which should be true for most .NET events.
        /// </para>
        /// </remarks>
        /// <param name="sender">Not used.</param>
        /// <param name="args">Not used.</param>
        public void OnEvent(object sender, EventArgs args)
        {
            Observe(0);
        }
    }
}
