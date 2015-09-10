﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using Lawo.Reflection;

    /// <summary>Provides a method to create a trigger.</summary>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    public static class Trigger
    {
        /// <summary>Creates a trigger such that <paramref name="handler"/> is called whenever
        /// <see cref="IProperty{T, U}.Value"/> has changed.</summary>
        /// <typeparam name="TOwner">The type of the owner object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> and/or
        /// <paramref name="handler"/> equal <c>null</c>.</exception>
        /// <remarks>
        /// <para>Call <see cref="IDisposable.Dispose"/> on the returned object to stop having
        /// <paramref name="handler"/> called.</para>
        /// <para>If the trigger is intended to be permanent it is permissible to to never call
        /// <see cref="IDisposable.Dispose"/>.</para>
        /// </remarks>
        public static IDisposable Create<TOwner, TProperty>(
            IProperty<TOwner, TProperty> property, Action<IProperty<TOwner, TProperty>> handler)
            where TOwner : INotifyPropertyChanged
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return new Forwarder<TOwner, TProperty>(property, handler);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private sealed class Forwarder<TOwner, TProperty> : IDisposable where TOwner : INotifyPropertyChanged
        {
            private readonly IProperty<TOwner, TProperty> property;
            private readonly Action<IProperty<TOwner, TProperty>> handler;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            public void Dispose()
            {
                this.property.Owner.PropertyChanged -= this.OnPropertyChanged;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal Forwarder(IProperty<TOwner, TProperty> property, Action<IProperty<TOwner, TProperty>> handler)
            {
                this.property = property;
                this.handler = handler;
                this.property.Owner.PropertyChanged += this.OnPropertyChanged;
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == this.property.PropertyInfo.Name)
                {
                    this.handler(this.property);
                }
            }
        }
    }
}