//=============================================================================
//=  $Id: StatisticsSet.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace React.Monitoring
{
    /// <summary>
    /// A set of <see cref="Statistic"/>s collected and computed from the
    /// same monitorable property.
    /// </summary>
    [
        SuppressMessage("Microsoft.Naming",
            "CA1710:IdentifiersShouldHaveCorrectSuffix")
    ]
    public class StatisticsSet : Monitor, IDictionary<Type, Statistic>
    {
        /// <summary>
        /// The set of <see cref="Statistic"/> instances keyed by their
        /// <see cref="Type"/>.
        /// </summary>
        private Dictionary<Type, Statistic> _statistics =
            new Dictionary<Type,Statistic>();

        /// <overloads>
        /// Create and initialize a StatisticsSet instance.
        /// </overloads>
        /// <summary>
        /// Create a new, empty <see cref="StatisticsSet"/> instance.
        /// </summary>
        public StatisticsSet()
        {
        }

        /// <summary>
        /// Create a new <see cref="StatisticsSet"/> that contains the
        /// given <see cref="Statistic"/> instances.
        /// </summary>
        /// <remarks>
        /// Any <see langword="null"/> references in <paramref name="statistics"/>
        /// are silently ignored.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="statistics"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="statistics">
        /// An <see cref="IEnumerable&lt;Statistic&gt;"/> instance that
        /// contains one or more <see cref="Statistic"/> objects.
        /// </param>
        public StatisticsSet(IEnumerable<Statistic> statistics)
        {
            if (statistics == null)
                throw new ArgumentNullException("Cannot be null.",
                    "statistics");

            foreach (Statistic stat in statistics)
            {
                if (stat != null)
                    _statistics[stat.GetType()] = stat;
            }
        }

        /// <summary>
        /// Create a new <see cref="StatisticsSet"/> that contains
        /// <see cref="Statistic"/> instances of the given <see cref="Type"/>s.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This constructor is most useful for creating a new
        /// <see cref="StatisticsSet"/> instance that will contain the same
        /// type of <see cref="Statistic"/> in another
        /// <see cref="StatisticsSet"/>.
        /// </para>
        /// <para>
        /// All of the <see cref="Type"/>s in <paramref name="statisticTypes"/>
        /// must have a public, no-arg constructor for the
        /// <see cref="StatisticsSet"/> to be able to instantiate the actual
        /// <see cref="Statistic"/> instances.
        /// </para>
        /// <para>
        /// Any <see langword="null"/> references in
        /// <paramref name="statisticTypes"/> are silently ignored.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="statisticTypes"/> is <see langword="null"/>.
        /// </exception>
        /// <param name="statisticTypes">
        /// An <see cref="IEnumerable&lt;Type&gt;"/> instances that contains the
        /// <see cref="Type"/> of <see cref="Statistic"/>s to include in the
        /// <see cref="StatisticsSet"/>.
        /// </param>
        public StatisticsSet(IEnumerable<Type> statisticTypes)
        {
            if (statisticTypes == null)
                throw new ArgumentNullException("Cannot be null.",
                    "statisticTypes");

            foreach (Type t in statisticTypes)
            {
                if (t != null)
                    _statistics[t] = null;
            }

            EnsureCreated();
        }

        /// <summary>
        /// Begin monitoring a property on the given object.
        /// </summary>
        /// <remarks>
        /// This method invokes the <see cref="Statistic.Attach"/> method
        /// for each of the contained <see cref="Statistic"/> instances.
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// will be monitored.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property of <paramref name="target"/> to monitor.
        /// </param>
        public override void Attach(object target, string propertyName)
        {
            foreach (Statistic stat in Values)
            {
                stat.Attach(target, propertyName);
            }
        }

        /// <summary>
        /// End monitoring a property on the given object.
        /// </summary>
        /// <remarks>
        /// This method invokes the <see cref="Statistic.Detach"/> method
        /// for each of the contained <see cref="Statistic"/> instances.
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// will stop being monitored.</param>
        /// <param name="propertyName">
        /// The name of a property of <paramref name="target"/> to stop
        /// monitoring.
        /// </param>
        public override void Detach(object target, string propertyName)
        {
            foreach (Statistic stat in Values)
            {
                stat.Detach(target, propertyName);
            }
        }

        #region IDictionary<Type,Statistic> Members

        /// <summary>
        /// Adds a <see cref="Statistic"/> to the <see cref="StatisticsSet"/>.
        /// </summary>
        /// <param name="key">
        /// The <see cref="Type"/> of the <see cref="Statistic"/> to add.
        /// </param>
        /// <param name="value">
        /// The <see cref="Statistic"/> to add to the
        /// <see cref="StatisticsSet"/>.
        /// </param>
        public void Add(Type key, Statistic value)
        {
            _statistics.Add(key, value);
        }

        /// <summary>
        /// Checks of a <see cref="Statistic"/> of the given <see cref="Type"/>
        /// is present in the <see cref="StatisticsSet"/>.
        /// </summary>
        /// <param name="key">
        /// The <see cref="Type"/> to look for in the
        /// <see cref="StatisticsSet"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if the <see cref="StatisticsSet"/> contains a
        /// <see cref="Statistic"/> of the given <see cref="Type"/>.
        /// </returns>
        public bool ContainsKey(Type key)
        {
            return _statistics.ContainsKey(key);
        }

        /// <summary>
        /// Gets the <see cref="Type"/>s of the <see cref="Statistic"/>s
        /// contained in the <see cref="StatisticsSet"/>.
        /// </summary>
        /// <value>
        /// An <see cref="ICollection&lt;T&gt;"/> containing the
        /// <see cref="Type"/>s of <see cref="Statistic"/> instances in the
        /// <see cref="StatisticsSet"/>.
        /// </value>
        public ICollection<Type> Keys
        {
            get { return _statistics.Keys; }
        }

        /// <summary>
        /// Remove the <see cref="Statistic"/> of the given <see cref="Type"/>
        /// from the <see cref="StatisticsSet"/>.
        /// </summary>
        /// <param name="key">
        /// The <see cref="Type"/> of <see cref="Statistic"/> to remove.
        /// </param>
        /// <returns>
        /// <b>true</b> if the <see cref="Statistic"/> was removed.
        /// </returns>
        public bool Remove(Type key)
        {
            return _statistics.Remove(key);
        }

        /// <summary>
        /// Attempts to get the <see cref="Statistic"/> of the specified
        /// <see cref="Type"/>.
        /// </summary>
        /// <param name="key">
        /// The <see cref="Type"/> of <see cref="Statistic"/> to retrieve.
        /// </param>
        /// <param name="value">
        /// When this method returns, the value associated with
        /// <paramref name="key"/>, if the key is found; otherwise,
        /// <see langword="null"/>.  This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <b>true</b> if the <see cref="StatisticsSet"/> contains a
        /// <see cref="Statistic"/> with the specified key; otherwise,
        /// <b>false</b>.
        /// </returns>
        public bool TryGetValue(Type key, out Statistic value)
        {
            return _statistics.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the <see cref="Statistic"/>s contained in the
        /// <see cref="StatisticsSet"/>.
        /// </summary>
        /// <value>
        /// An <see cref="ICollection&lt;T&gt;"/> containing all the
        /// <see cref="Statistic"/> instances in the
        /// <see cref="StatisticsSet"/>.
        /// </value>
        public ICollection<Statistic> Values
        {
            get { return _statistics.Values; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Statistic"/> having the
        /// specified <see cref="Type"/>.
        /// </summary>
        /// <param name="key">
        /// The <see cref="Type"/> <see cref="Statistic"/> to get or set.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If an attempt is made to set a value to <see langword="null"/>.
        /// </exception>
        /// <returns>
        /// The <see cref="Statistic"/> having the <see cref="Type"/> specified
        /// by <paramref name="key"/>.
        /// </returns>
        public Statistic this[Type key]
        {
            get
            {
                return _statistics[key];
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Value cannot be null.");
                _statistics[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<Type,Statistic>> Members

        /// <summary>
        /// Adds the given <see cref="KeyValuePair&lt;K,V&gt;"/> to the
        /// <see cref="StatisticsSet"/>.
        /// </summary>
        /// <remarks>
        /// This method is not normally called from client code.
        /// </remarks>
        /// <param name="item">
        /// The <see cref="KeyValuePair&lt;K,V&gt;"/> to add.
        /// </param>
        public void Add(KeyValuePair<Type, Statistic> item)
        {
            ICollection<KeyValuePair<Type, Statistic>> c =
                (ICollection<KeyValuePair<Type, Statistic>>)_statistics;
            c.Add(item);
        }

        /// <summary>
        /// Removes all <see cref="Statistic"/> instances from the
        /// <see cref="StatisticsSet"/>.
        /// </summary>
        public void Clear()
        {
            _statistics.Clear();
        }

        /// <summary>
        /// Check if the given <see cref="KeyValuePair&lt;K,V&gt;"/> is
        /// contained in the <see cref="StatisticsSet"/>.
        /// </summary>
        /// <remarks>
        /// This method is not normally called from client code.
        /// </remarks>
        /// <param name="item">
        /// The <see cref="KeyValuePair&lt;K,V&gt;"/> to search for in the
        /// <see cref="StatisticsSet"/>.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="item"/> is in the
        /// <see cref="StatisticsSet"/>.
        /// </returns>
        public bool Contains(KeyValuePair<Type, Statistic> item)
        {
            ICollection<KeyValuePair<Type,Statistic>> c =
                (ICollection<KeyValuePair<Type, Statistic>>)_statistics;
            return c.Contains(item);
        }

        /// <summary>
        /// Copies all the <see cref="KeyValuePair&lt;K,V&gt;"/> in the
        /// <see cref="StatisticsSet"/> to the given <see cref="Array"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of
        /// the elements copied from <see cref="StatisticsSet"/>. The
        /// <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying
        /// begins.
        /// </param>
        public void CopyTo(KeyValuePair<Type, Statistic>[] array, int arrayIndex)
        {
            ICollection<KeyValuePair<Type, Statistic>> c =
                (ICollection<KeyValuePair<Type, Statistic>>)_statistics;
            c.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of <see cref="Statistic"/> instances that are
        /// members of the <see cref="StatisticsSet"/>.
        /// </summary>
        /// <value>
        /// The number of <see cref="Statistic"/> instances in the
        /// <see cref="StatisticsSet"/> as an <see cref="int"/>.
        /// </value>
        public int Count
        {
            get { return _statistics.Count; }
        }

        /// <summary>
        /// Gets whether or not the <see cref="StatisticsSet"/> is read-only.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <see cref="StatisticsSet"/> is read-only;
        /// otherwise <b>false</b>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                ICollection<KeyValuePair<Type, Statistic>> c =
                    (ICollection<KeyValuePair<Type, Statistic>>)_statistics;
                return c.IsReadOnly;
            }
        }

        /// <summary>
        /// Removes a <see cref="KeyValuePair&lt;K,V&gt;"/> from the
        /// <see cref="StatisticsSet"/>.
        /// </summary>
        /// <remarks>
        /// This method is not normally called from client code.
        /// </remarks>
        /// <param name="item">
        /// The <see cref="KeyValuePair&lt;K,V&gt;"/> to remove.
        /// </param>
        /// <returns>
        /// <b>true</b> if <paramref name="item"/> was removed.
        /// </returns>
        public bool Remove(KeyValuePair<Type, Statistic> item)
        {
            ICollection<KeyValuePair<Type,Statistic>> c =
                (ICollection<KeyValuePair<Type, Statistic>>)_statistics;
            return c.Remove(item);
        }

        #endregion

        #region IEnumerable-related Members

        /// <summary>
        /// Returns the enumerator that allows iteration over the
        /// <see cref="Statistic"/> instances.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator&lt;T&gt;"/> that allows iteration over the
        /// <see cref="Statistic"/> instances.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, Statistic>> GetEnumerator()
        {
            return _statistics.GetEnumerator();
        }

        /// <summary>
        /// Returns the enumerator that allows iteration over the
        /// <see cref="Statistic"/> instances.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"/> that allows
        /// iteration over the <see cref="Statistic"/> instances.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _statistics.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Ensure that all the <see cref="Statistic"/> instances are
        /// instantiated.
        /// </summary>
        private void EnsureCreated()
        {
            foreach (Type t in Keys)
            {
                if (this[t] == null)
                {
                    object obj = Activator.CreateInstance(t);
                    if (obj != null)
                    {
                        this[t] = (Statistic)obj;
                    }
                    else
                    {
                        // Silently discard any types we can't create and
                        // haven't already thrown an exception.
                        Remove(t);
                    }
                }
            }
        }
    }
}
