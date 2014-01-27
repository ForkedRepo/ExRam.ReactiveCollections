﻿using System;
using System.Diagnostics.Contracts;

namespace ExRam.ReactiveCollections
{
    public static partial class ReactiveCollectionExtensions
    {
        #region AsReactiveCollectionImpl
        private sealed class AsReactiveCollectionImpl<TNotification, T> : IReactiveCollection<TNotification, T>
            where TNotification : ICollectionChangedNotification<T>
        {
            private readonly IReactiveCollection<TNotification, T> _reactiveCollection;

            public AsReactiveCollectionImpl(IReactiveCollection<TNotification, T> reactiveCollection)
            {
                Contract.Requires(reactiveCollection != null);

                this._reactiveCollection = reactiveCollection;
            }

            IObservable<TNotification> IReactiveCollection<TNotification, T>.Changes
            {
                get
                {
                    return this._reactiveCollection.Changes;
                }
            }
        }
        #endregion

        public static IReactiveCollection<TNotification, T> AsReactiveCollection<TNotification, T>(this IReactiveCollection<TNotification, T> reactiveCollection)
            where TNotification : ICollectionChangedNotification<T>
        {
            Contract.Requires(reactiveCollection != null);

            return new AsReactiveCollectionImpl<TNotification, T>(reactiveCollection);
        }
    }
}