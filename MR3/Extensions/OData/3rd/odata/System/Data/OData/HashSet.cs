//   Copyright 2011 Microsoft Corporation
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace System.Data.OData
{
#if WINDOWS_PHONE
    #region Namespaces
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    #endregion Namespaces

    /// <summary>
    /// Simplistic implementation of HashSet for platforms where it is not available
    /// </summary>
    internal class HashSet<T>
    {
        /// <summary>
        /// We're using Dictionary for the real implementation.
        /// </summary>
        private Dictionary<T, bool> dictionary;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="equalityComparer">The equality comparer to use.</param>
        internal HashSet(IEqualityComparer<T> equalityComparer)
        {
            this.dictionary = new Dictionary<T, bool>(equalityComparer);
        }

        /// <summary>
        /// Determines if the hashset contains the specified item.
        /// </summary>
        /// <param name="item">The item to look for.</param>
        /// <returns>true if the item was found, false otherwise.</returns>
        internal bool Contains(T item)
        {
            return this.dictionary.ContainsKey(item);
        }

        /// <summary>
        /// Adds an item to the hashset.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>false if the item already exists in the set or true otherwise.</returns>
        internal bool Add(T item)
        {
            if (this.Contains(item))
            {
                return false;
            }

            this.dictionary.Add(item, true);
            return true;
        }
    }
#endif
}
