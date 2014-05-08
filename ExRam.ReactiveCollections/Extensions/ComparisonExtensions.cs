﻿// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System
{
    public static class ComparisonExtensions
    {
        #region ComparerImpl
        private sealed class ComparerImpl<T> : IComparer<T>
        {
            private readonly Comparison<T> _comparison;

            public ComparerImpl(Comparison<T> comparison)
            {
                Contract.Requires(comparison != null);

                this._comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return this._comparison(x, y);
            }
        }
        #endregion

        public static IComparer<T> ToComparer<T>(this Comparison<T> comparison)
        {
            Contract.Requires(comparison != null);

            return new ComparerImpl<T>(comparison);
        }
    }
}
