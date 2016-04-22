﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2016 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Model
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;

    using Ember;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Temporary, TODO")]
    public abstract partial class FieldNode<TMostDerived> where TMostDerived : FieldNode<TMostDerived>
    {
        private sealed class MetaElement<TProperty> : MetaElement where TProperty : Element<TProperty>
        {
            private readonly Func<TMostDerived, TProperty> get;
            private readonly Action<TMostDerived, TProperty> set;

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            internal MetaElement(PropertyInfo property) : base(property)
            {
                var objParam = Expression.Parameter(typeof(TMostDerived));
                this.get = Expression.Lambda<Func<TMostDerived, TProperty>>(
                    Expression.Convert(Expression.Property(objParam, property), typeof(TProperty)), objParam).Compile();
                var valueParam = Expression.Parameter(typeof(TProperty));
                var assignee = Expression.Property(objParam, property);
                this.set = Expression.Lambda<Action<TMostDerived, TProperty>>(
                    Expression.Assign(assignee, valueParam), objParam, valueParam).Compile();
            }

            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
            internal sealed override Element ReadContents(
                EmberReader reader, ElementType actualType, Context context, out RequestState requestState)
            {
                return Element<TProperty>.ReadContents(reader, actualType, context, out requestState);
            }

            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
            internal sealed override bool IsAvailable(IParent parent, bool throwIfMissing)
            {
                var value = this.get((TMostDerived)parent);

                if (value == null)
                {
                    if (!this.IsOptional && throwIfMissing)
                    {
                        const string Format =
                            "No data value available for the required property {0}.{1} in the node with the path {2}.";
                        throw CreateRequiredPropertyException(parent, Format);
                    }

                    return this.IsOptional;
                }
                else
                {
                    return true;
                }
            }

            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Method is not public, CA bug?")]
            internal sealed override void ChangeOnlineStatus(IParent parent, IElement element)
            {
                if (!this.IsOptional && !element.IsOnline)
                {
                    const string Format =
                        "The required property {0}.{1} in the node with the path {2} has been set offline by the provider.";
                    throw CreateRequiredPropertyException(parent, Format);
                }

                this.set((TMostDerived)parent, (TProperty)(element.IsOnline ? element : null));
                parent.OnPropertyChanged(new PropertyChangedEventArgs(this.Property.Name));
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            private ModelException CreateRequiredPropertyException(IParent parent, string format)
            {
                return new ModelException(string.Format(
                    CultureInfo.InvariantCulture,
                    format,
                    this.Property.DeclaringType.FullName,
                    this.Property.Name,
                    parent.GetPath()));
            }
        }
    }
}