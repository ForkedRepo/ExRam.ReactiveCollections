﻿// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Diagnostics.Contracts;

namespace ExRam.ReactiveCollections
{
    [ContractClass(typeof(ReactiveCollectionContracts<,>))]
    public interface IReactiveCollection<out TNotification, T>
        where TNotification : ICollectionChangedNotification<T>
    {
        IObservable<TNotification> Changes
        {
            get;
        }
    }

    [ContractClassFor(typeof(IReactiveCollection<,>))]
    public abstract class ReactiveCollectionContracts<TNotification, T> : IReactiveCollection<TNotification, T>
         where TNotification : ICollectionChangedNotification<T>
    {
        public IObservable<TNotification> Changes
        {
            get
            {
                Contract.Ensures(Contract.Result<IObservable<TNotification>>() != null);

                return default(IObservable<TNotification>);
            }
        }
    }
}
