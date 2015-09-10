﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2015 Lawo AG (http://www.lawo.com). All rights reserved.</copyright>
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using Lawo.Reflection;

    /// <summary>Represents a binding between two properties.</summary>
    /// <typeparam name="TSourceOwner">The type of the object owning the source property.</typeparam>
    /// <typeparam name="TSource">The type of the source property.</typeparam>
    /// <typeparam name="TTargetOwner">The type of the object owning the target property.</typeparam>
    /// <typeparam name="TTarget">The type of the target property.</typeparam>
    /// <remarks>
    /// <para><b>Thread Safety</b>: Any public static members of this type are thread safe. Any instance members are not
    /// guaranteed to be thread safe.</para>
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes", Justification = "Client code almost never needs to mention this type.")]
    public sealed class Binding<TSourceOwner, TSource, TTargetOwner, TTarget> : IDisposable
        where TSourceOwner : INotifyPropertyChanged
        where TTargetOwner : INotifyPropertyChanged
    {
        private readonly ChangeOriginatedAtEventArgs<TSourceOwner, TSource> sourceEventArgs;
        private readonly Func<TSource, TTarget> toTarget;
        private readonly ChangeOriginatedAtEventArgs<TTargetOwner, TTarget> targetEventArgs;
        private readonly Func<TTarget, TSource> toSource;
        private byte sourceUpdating;
        private byte targetUpdating;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Stops forwarding changes and altering the properties.</summary>
        /// <remarks>If the binding is intended to be permanent it is permissible to to never call
        /// <see cref="Dispose"/>.</remarks>
        public void Dispose()
        {
            this.sourceEventArgs.Property.Owner.PropertyChanged -= this.OnSourceChanged;
            this.targetEventArgs.Property.Owner.PropertyChanged -= this.OnTargetChanged;
        }

        /// <summary>Gets the source property.</summary>
        public IProperty<TSourceOwner, TSource> Source
        {
            get { return this.sourceEventArgs.Property; }
        }

        /// <summary>Gets the target property.</summary>
        public IProperty<TTargetOwner, TTarget> Target
        {
            get { return this.targetEventArgs.Property; }
        }

        /// <summary>Occurs when a change has originated at the source.</summary>
        public event EventHandler<ChangeOriginatedAtEventArgs<TSourceOwner, TSource>> ChangeOriginatedAtSource;

        /// <summary>Occurs when a change has originated at the target.</summary>
        public event EventHandler<ChangeOriginatedAtEventArgs<TTargetOwner, TTarget>> ChangeOriginatedAtTarget;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        internal Binding(
            IProperty<TSourceOwner, TSource> source,
            Func<TSource, TTarget> toTarget,
            IProperty<TTargetOwner, TTarget> target,
            Func<TTarget, TSource> toSource)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (toTarget == null)
            {
                throw new ArgumentNullException("toTarget");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            this.sourceEventArgs = new ChangeOriginatedAtEventArgs<TSourceOwner, TSource>(source);
            this.toTarget = toTarget;
            this.targetEventArgs = new ChangeOriginatedAtEventArgs<TTargetOwner, TTarget>(target);
            this.toSource = toSource;

            this.targetEventArgs.Property.Value = this.toTarget(this.sourceEventArgs.Property.Value);
            this.sourceEventArgs.Property.Owner.PropertyChanged += this.OnSourceChanged;

            if (this.toSource != null)
            {
                this.targetEventArgs.Property.Owner.PropertyChanged += this.OnTargetChanged;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void OnSourceChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnChanged(
                e,
                this.sourceEventArgs,
                this.sourceUpdating,
                this.targetEventArgs.Property,
                ref this.targetUpdating,
                this.toTarget,
                this.ChangeOriginatedAtSource);
        }

        private void OnTargetChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnChanged(
                e,
                this.targetEventArgs,
                this.targetUpdating,
                this.sourceEventArgs.Property,
                ref this.sourceUpdating,
                this.toSource,
                this.ChangeOriginatedAtTarget);
        }

        private void OnChanged<TLeaderOwner, TLeader, TFollowerOwner, TFollower>(
            PropertyChangedEventArgs e,
            ChangeOriginatedAtEventArgs<TLeaderOwner, TLeader> leaderEventArgs,
            byte leaderUpdating,
            IProperty<TFollowerOwner, TFollower> follower,
            ref byte followerUpdating,
            Func<TLeader, TFollower> toFollower,
            EventHandler<ChangeOriginatedAtEventArgs<TLeaderOwner, TLeader>> handler)
            where TLeaderOwner : INotifyPropertyChanged
            where TFollowerOwner : INotifyPropertyChanged
        {
            if (e.PropertyName == leaderEventArgs.Property.PropertyInfo.Name)
            {
                ++followerUpdating;

                try
                {
                    follower.Value = toFollower(leaderEventArgs.Property.Value);
                }
                finally
                {
                    --followerUpdating;
                }

                if ((handler != null) && (leaderUpdating == 0))
                {
                    handler(this, leaderEventArgs);
                }
            }
        }
    }
}