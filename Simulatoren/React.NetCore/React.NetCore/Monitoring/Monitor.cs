//=============================================================================
//=  $Id: Monitor.cs 184 2006-10-14 18:46:48Z eroe $
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
using System.Reflection;

namespace React.Monitoring
{
    /// <summary>
    /// An object that monitors property value changes that occur on another
    /// object.
    /// </summary>
    /// <remarks>
    /// A monitoring object is an object that wants to be informed that some
    /// property on another object has changed.
    /// </remarks>
    public abstract class Monitor
    {
        /// <summary>
        /// Create a new <see cref="Monitor"/> instance.
        /// </summary>
        protected Monitor()
        {
        }

        /// <summary>
        /// Attach the <see cref="Monitor"/> to an object's property by name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Attaching a <see cref="Monitor"/> to a property starts the
        /// <see cref="Monitor"/> receiving value change notifications.
        /// </para>
        /// <para>
        /// Subclasses should implement this method to create an
        /// <b>EventHandler&lt;</b>
        /// <see cref="ValueChangedEventArgs"/><b>&gt;</b> delegate and then call the
        /// protected
        /// <see cref="Attach(object,string,EventHandler&lt;ValueChangedEventArgs&gt;)"/>
        /// method.  A typical implementation might look like the following:
        /// </para>
        /// <code><![CDATA[
        /// public override void Attach(object target, string propertyName)
        /// {
        ///     // Create the delegate.
        ///     EventHandler<ValueChangedEventArgs> handler =
        ///         new EventHandler<ValueChangedEventArgs>(HandlerMethod);
        /// 
        ///     // Attach the delegate, 'handler', to the monitorable property.
        ///     object value = Attach(target, propertyName, handler);
        /// 
        ///     // do something with 'value' if required
        /// }
        /// 
        /// private void HandlerMethod(object sender, ValueChangedEventArgs args)
        /// {
        ///     // handle the notification here...
        /// }]]></code>
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// will be monitored.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property on <paramref name="target"/> to monitor.
        /// </param>
        public abstract void Attach(object target, string propertyName);

        /// <summary>
        /// Attach the given value changed event handler delegate to monitor
        /// a property on the given target object.
        /// </summary>
        /// <remarks>
        /// This method is normally invoked by
        /// <see cref="Attach(object,string)"/> to start the given
        /// <paramref name="valueChangedHandler"/> receiving property
        /// notifications from <paramref name="target"/>.
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// will be monitored.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property on <paramref name="target"/> to monitor.
        /// </param>
        /// <param name="valueChangeHandler">
        /// The delegate that will handle property change notifications sent by
        /// <paramref name="target"/>.
        /// </param>
        /// <returns>
        /// The current value of <paramref name="target"/>'s property named
        /// <paramref name="propertyName"/>
        /// </returns>
        protected object Attach(object target, string propertyName,
            EventHandler<ValueChangedEventArgs> valueChangeHandler)
        {
            return ConnectHandler(target, propertyName,
                valueChangeHandler, true);
        }

        /// <summary>
        /// Detach the <see cref="Monitor"/> from an object's property by name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Detaching a <see cref="Monitor"/> from a property stops the
        /// <see cref="Monitor"/> receiving value change notifications.
        /// </para>
        /// <para>
        /// Subclasses should implement this method to create a
        /// <b>EventHandler&lt;</b>
        /// <see cref="ValueChangedEventArgs"/><b>&gt;</b> delegate and then
        /// call the protected
        /// <see cref="Detach(object,string,EventHandler&lt;ValueChangedEventArgs&gt;)"/>
        /// method.  A typical implementation might look like the following:
        /// </para>
        /// <code><![CDATA[
        /// public override void Detach(object target, string propertyName)
        /// {
        ///     EventHandler<ValueChangedEventArgs> handler =
        ///         new EventHandler<ValueChangedEventArgs>(HandlerMethod);
        ///     object value = Detach(target, propertyName, handler);
        ///     // do something with 'value' if required
        /// }
        /// 
        /// private void HandlerMethod(object sender, ValueChangedEventArgs args)
        /// {
        ///     // handle the notification here...
        /// }]]></code>
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/> is
        /// to stop being monitored.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property on <paramref name="obj"/> to stop
        /// monitoring.
        /// </param>
        public abstract void Detach(object target, string propertyName);

        /// <summary>
        /// Detach the given value changed event handler delegate from a
        /// property on the given target object.
        /// </summary>
        /// <remarks>
        /// This method is normally invoked by
        /// <see cref="Detach(object,string)"/> to stop the given
        /// <paramref name="valueChangedHandler"/> from receiving property
        /// notifications from <paramref name="target"/>.
        /// </remarks>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// will be monitored.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property on <paramref name="target"/> to monitor.
        /// </param>
        /// <param name="valueChangedHandler">
        /// The delegate that will handle property change notifications sent by
        /// <paramref name="target"/>.
        /// </param>
        /// <returns>
        /// The current value of <paramref name="target"/>'s property named
        /// <paramref name="propertyName"/>
        /// </returns>
        protected object Detach(object target, string propertyName,
            EventHandler<ValueChangedEventArgs> valueChangedHandler)
        {
            return ConnectHandler(target, propertyName,
                valueChangedHandler, false);
        }

        /// <summary>
        /// Connect or disconnect a value changed event handler (delegate)
        /// to/from an object.
        /// </summary>
        /// <param name="target">
        /// The object whose property named <paramref name="propertyName"/>
        /// which will have <paramref name="valueChangedHandler"/> added to or
        /// removed from receiving property change notifications.
        /// </param>
        /// <param name="propertyName">
        /// The name of a monitorable property on <paramref name="target"/>
        /// that <paramref name="valueChangedHandler"/> will start or stop
        /// monitoring.
        /// </param>
        /// <param name="valueChangedHandler">
        /// The delegate to add or remove.
        /// </param>
        /// <param name="connect">
        /// <b>true</b> to start monitoring; or <b>false</b> to stop monitoring.
        /// </param>
        /// <returns></returns>
        private static object ConnectHandler(
            object target,
            string propertyName,
            EventHandler<ValueChangedEventArgs> valueChangedHandler,
            bool connect)
        {
            Type targetType = target as Type;
            if (targetType == null)
                targetType = target.GetType();

            PropertyInfo propinfo = FindProperty(targetType, propertyName);
            EventInfo evtinfo = FindMonitoringEvent(targetType, propinfo);

            object propValue;
            
            if (evtinfo != null)
            {
                if (connect)
                    evtinfo.AddEventHandler(target, valueChangedHandler);
                else
                    evtinfo.RemoveEventHandler(target, valueChangedHandler);

                propValue = propinfo.GetValue(target, null);
            }
            else
            {
                propValue = null;
            }

            return propValue;
        }

        /// <summary>
        /// Finds and returns the <see cref="PropertyInfo"/> for a named
        /// property on the given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/> to search for the <see cref="PropertyInfo"/>
        /// for a property named <paramref name="propertyName"/>.
        /// </param>
        /// <param name="propertyName">
        /// The name of a property on <paramref name="type"/>.
        /// </param>
        /// <returns>
        /// The <see cref="PropertyInfo"/> for the requested property.
        /// </returns>
        private static PropertyInfo FindProperty(Type type, string propertyName)
        {
            if (type == null)
                throw new ArgumentNullException("Target type was null", "type");
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException("Invalid property name", "propertyName");

            PropertyInfo propinfo = null;

            MemberInfo[] props = type.GetMember(propertyName, BindingFlags.Instance|BindingFlags.Public|BindingFlags.IgnoreCase);
                //MemberTypes.Property,
                //BindingFlags.Instance | BindingFlags.Public,
                //Type.FilterNameIgnoreCase,
                //propertyName);

            if (props.Length == 1)
            {
                propinfo = (PropertyInfo)props[0];
                if (!propinfo.CanRead)
                    throw new ArgumentException("Property is write-only.");
            }

            return propinfo;
        }

        /// <summary>
        /// Find and return the <see cref="EventInfo"/> for the value changed
        /// event associated with the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/> to search for the value changed event
        /// associated with <paramref name="propinfo"/>.
        /// </param>
        /// <param name="propinfo">
        /// The <see cref="PropertyInfo"/> for the monitored property.
        /// </param>
        /// <returns>
        /// The <see cref="EventInfo"/> for the value changed
        /// event associated with <paramref name="propinfo"/>.
        /// </returns>
        private static EventInfo FindMonitoringEvent(Type type,
            PropertyInfo propinfo)
        {
            if (type == null)
                throw new ArgumentNullException("Target type was null", "type");
            if (propinfo == null)
                throw new ArgumentNullException("Property info was null", "propinfo");

            EventInfo evtinfo = null;
//            propinfo.GetTypeInfo().GetCustomAttribute<WrapperObjectAttribute>();
            Attribute attr = typeof(Attribute).GetTypeInfo().GetCustomAttribute<MonitorUsingAttribute>();

            if (attr == null)
                throw new ArgumentException("Property is not monitorable");

            string evtName = ((MonitorUsingAttribute)attr).EventName;

            MemberInfo[] evts = type.GetMember(evtName, BindingFlags.Instance|BindingFlags.Public|BindingFlags.IgnoreCase);


            if (evts.Length == 1)
            {
                Type eventType = typeof(EventHandler<ValueChangedEventArgs>);
                evtinfo = (EventInfo)evts[0];
                if (!evtinfo.EventHandlerType.Equals(eventType))
                {
                    throw new ArgumentException(
                        "Event type is not EventHandler<ValueChangedEventArgs>");
                }
            }

            return evtinfo;
        }
    }
}
