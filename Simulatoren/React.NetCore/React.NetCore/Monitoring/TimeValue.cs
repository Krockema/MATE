//=============================================================================
//=  $Id: TimeValue.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Text;

namespace React.Monitoring
{
    /// <summary>
    /// An immutable object that represents a <see cref="double"/> value
    /// observed at a particular simulation time.
    /// </summary>
    public struct TimeValue : IConvertible
    {
        /// <summary>
        /// An <see cref="IComparer"/> that can be used to sort
        /// <see cref="TimeValue"/> instances by time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This <see cref="IComparer"/> compares <see cref="TimeValue"/>s
        /// using their <see cref="TimeValue.Time"/> property.
        /// </para>
        /// <para>
        /// This object can safely be cast to an
        /// <see cref="IComparer&lt;TimeValue&gt;"/> instance.
        /// </para>
        /// </remarks>
        public static readonly IComparer SortByTime = new ByTimeComparer();

        /// <summary>
        /// An <see cref="IComparer"/> that can be used to sort
        /// <see cref="TimeValue"/> instances by value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This <see cref="IComparer"/> compares <see cref="TimeValue"/>s
        /// using their <see cref="TimeValue.Value"/> property.
        /// </para>
        /// <para>
        /// This object can safely be cast to an
        /// <see cref="IComparer&lt;TimeValue&gt;"/> instance.
        /// </para>
        /// </remarks>
        public static readonly IComparer SortByValue = new ByValueComparer();

        /// <summary>
        /// An invalid <see cref="TimeValue"/>.
        /// </summary>
        /// <remarks>
        /// This <see cref="TimeValue"/> has a <see cref="Value"/> of
        /// <see cref="Double.NaN"/> and a <see cref="Time"/> of -1L.
        /// Its <see cref="IsValid"/> property is always <b>false</b>.
        /// </remarks>
        public static readonly TimeValue Invalid = new TimeValue(-1L, Double.NaN);

        /// <summary>
        /// The simulation time <see cref="_value"/> was observed.
        /// </summary>
        private long _time;
        /// <summary>
        /// The value observed at <see cref="_time"/>.
        /// </summary>
        private double _value;

        /// <summary>
        /// Create a new <see cref="TimeValue"/> for the given time and
        /// value pair.
        /// </summary>
        /// <param name="time">
        /// The simulation time at which <paramref name="value"/> was observed.
        /// </param>
        /// <param name="value">
        /// The value observed at <paramref name="time"/>.
        /// </param>
        public TimeValue(long time, double value)
        {
            _time = time;
            _value = value;
        }

        /// <summary>
        /// Gets whether this <see cref="TimeValue"/> represents a valid
        /// observation.
        /// </summary>
        /// <remarks>
        /// A <see cref="TimeValue"/> is valid if its <see cref="Value"/> is
        /// not a <see cref="Double.NaN"/> and its <see cref="Time"/> is
        /// non-negative.
        /// </remarks>
        /// <value>
        /// <b>true</b> if valid; otherwise <b>false</b>.
        /// </value>
        public bool IsValid
        {
            get { return !Double.IsNaN(_value) && _time >= 0L; }
        }

        /// <summary>
        /// Gets the simulation time that <see cref="Value"/> was observed.
        /// </summary>
        /// <value>
        /// The simulation time that <see cref="Value"/> was observed as an
        /// <see cref="long"/>.
        /// </value>
        public long Time
        {
            get { return _time; }
        }

        /// <summary>
        /// Gets the value observed at <see cref="Time"/>.
        /// </summary>
        /// <value>
        /// The value observed at <see cref="Time"/> as a <see cref="double"/>.
        /// </value>
        public double Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Test if this <see cref="TimeValue"/> is equal to the given object.
        /// </summary>
        /// <remarks>
        /// If <paramref name="obj"/> is a <see cref="TimeValue"/> instance,
        /// this method simply delegates the test for equality to
        /// <see cref="System.Object.Equals(object)"/>.  If
        /// <paramref name="obj"/> is not a <see cref="TimeValue"/>, this
        /// method always returns <b>false</b>.
        /// </remarks>
        /// <param name="obj">
        /// The object to test for equality against.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="obj"/> is a <see cref="TimeValue"/>
        /// and is equal to this <see cref="TimeValue"/>; otherwise
        /// <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is TimeValue) ? base.Equals((TimeValue)obj) : false;
        }

        /// <summary>
        /// Returns the <see cref="int"/> hash code for the
        /// <see cref="TimeValue"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The hash code is computed as
        /// </para>
        /// <code>Time.GetHashCode() ^ Value.GetHashCode()</code>
        /// </remarks>
        /// <returns>
        /// The hash code as an <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _time.GetHashCode() ^ _value.GetHashCode();
        }

        /// <summary>
        /// Test if two <see cref="TimeValue"/> instances are equal.
        /// </summary>
        /// <remarks>
        /// Two <see cref="TimeValue"/> instances are considered to be equal if
        /// their <see cref="Time"/> and <see cref="Value"/> properties are
        /// equal.
        /// </remarks>
        /// <param name="t1">
        /// The <see cref="TimeValue"/> to test for equality against
        /// <paramref name="t2"/>.
        /// </param>
        /// <param name="t2">
        /// The <see cref="TimeValue"/> to test for equality against
        /// <paramref name="t1"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="t1"/> equals <paramref name="t2"/>
        /// or <b>false</b> if <paramref name="t1"/> does not equal
        /// <paramref name="t2"/>.
        /// </returns>
        public static bool operator ==(TimeValue t1, TimeValue t2)
        {
            return t1._time == t2._time && t1._value == t2._value;
        }

        /// <summary>
        /// Test if two <see cref="TimeValue"/> instances are not equal.
        /// </summary>
        /// <remarks>
        /// Two <see cref="TimeValue"/> instances are considered to be inequal
        /// if either their <see cref="Time"/> or <see cref="Value"/>
        /// properties are inequal.
        /// </remarks>
        /// <param name="t1">
        /// The <see cref="TimeValue"/> to test for inequality against
        /// <paramref name="t2"/>.
        /// </param>
        /// <param name="t2">
        /// The <see cref="TimeValue"/> to test for inequality against
        /// <paramref name="t1"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="t1"/> does not equal
        /// <paramref name="t2"/> or <b>false</b> if <paramref name="t1"/>
        /// equals <paramref name="t2"/>.
        /// </returns>
        public static bool operator !=(TimeValue t1, TimeValue t2)
        {
            return t1._time != t2._time || t1._value != t2._value;
        }

        /// <summary>
        /// Returns a <see cref="string"/> representation of the
        /// <see cref="TimeValue"/>.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="string"/> is formatted as
        /// <b>[time=<em>time</em>, value=<em>value</em>]</b>.
        /// </remarks>
        /// <returns>
        /// A <see cref="string"/> representation of the
        /// <see cref="TimeValue"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("[time=");
            buffer.Append(_time);
            buffer.Append(", value=");
            buffer.Append(_value);
            buffer.Append(']');
            return buffer.ToString();
        }

        #region IConvertible Members

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(_value);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return _value;
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(_value);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(_value);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(_value);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return (float)_value;
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return ToString();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException(
                "The method or operation is not implemented.");
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(_value);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(_value);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(_value);
        }

        #endregion

        /// <summary>
        /// <see cref="IComparer"/> implementation that compares two
        /// <see cref="TimeValue"/>s by <see cref="TimeValue.Time"/>.
        /// </summary>
        private class ByTimeComparer : IComparer, IComparer<TimeValue>
        {
            public int Compare(object x, object y)
            {
                if (!(x is TimeValue))
                {
                    throw new InvalidCastException("x is not a TimeValue");
                }
                if (!(y is TimeValue))
                {
                    throw new InvalidCastException("y is not a TimeValue");
                }

                return Compare((TimeValue)x, (TimeValue)y);
            }

            public int Compare(TimeValue x, TimeValue y)
            {
                if (x.Time < y.Time)
                    return -1;
                if (x.Time > y.Time)
                    return 1;
                return 0;
            }
        }

        /// <summary>
        /// <see cref="IComparer"/> implementation that compares two
        /// <see cref="TimeValue"/>s by <see cref="TimeValue.Value"/>.
        /// </summary>
        private class ByValueComparer : IComparer, IComparer<TimeValue>
        {
            public int Compare(object x, object y)
            {
                if (!(x is TimeValue))
                {
                    throw new InvalidCastException("x is not a TimeValue");
                }
                if (!(y is TimeValue))
                {
                    throw new InvalidCastException("y is not a TimeValue");
                }

                return Compare((TimeValue)x, (TimeValue)y);
            }

            public int Compare(TimeValue x, TimeValue y)
            {
                if (x.Value < y.Value)
                    return -1;
                if (x.Value > y.Value)
                    return 1;
                return 0;
            }
        }
    }
}
