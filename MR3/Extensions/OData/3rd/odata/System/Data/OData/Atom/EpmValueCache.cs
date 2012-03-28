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

namespace System.Data.OData.Atom
{
    #region Namespaces.
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    #endregion Namespaces.

    /// <summary>
    /// Caches values of properties and items enumerations so that we only ever enumerate these once even if they were use in EPM.
    /// </summary>
    internal class EpmValueCache
    {
        /// <summary>
        /// Caches either ComplexValue properties enumeration or MultiValue items enumeration.
        /// </summary>
        /// <remarks>The key is the EPM source path segment for the property in question (complex or multivalue).
        /// For complex property, the value is a List of ODataProperty which stores the enumeration ODataComplexValue.Properties cache.
        /// For multivalue property, the value is a List of object which stores the enumeration ODataMultiValue.Items cache.
        /// The items are either EpmMultiValueItemCache instances in which case the value of the item is cached inside that instance,
        /// or it's any other type in which case the value of the item is that instance.</remarks>
        private Dictionary<EpmSourcePathSegment, object> epmValuesCache;

        /// <summary>
        /// Creates a new empty cache.
        /// </summary>
        internal EpmValueCache()
        {
            DebugUtils.CheckNoExternalCallers();
        }

        /// <summary>
        /// Returns the properties for the specified complex value.
        /// </summary>
        /// <param name="epmValueCache">The EPM value cache to use (can be null).</param>
        /// <param name="sourcePathSegment">The source path segment for the property which has this complex value.</param>
        /// <param name="complexValue">The complex value to get the properties for.</param>
        /// <param name="writingContent">If we're writing content of an entry or not.</param>
        /// <returns>The properties enumeration for the complex value.</returns>
        internal static IEnumerable<ODataProperty> GetComplexValueProperties(
            EpmValueCache epmValueCache, 
            EpmSourcePathSegment sourcePathSegment, 
            ODataComplexValue complexValue, 
            bool writingContent)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(complexValue != null, "complexValue != null");
            Debug.Assert(writingContent || epmValueCache != null, "If we're not writing content, then the EPM value cache must exist.");

            if (epmValueCache == null)
            {
                return complexValue.Properties;
            }
            else
            {
                return epmValueCache.GetComplexValueProperties(sourcePathSegment, complexValue, writingContent);
            }
        }

        /// <summary>
        /// Returns the items for the specified multivalue.
        /// </summary>
        /// <param name="epmValueCache">The EPM value cache to use (can be null).</param>
        /// <param name="sourcePathSegment">The source path segment for the property which has this multivalue.</param>
        /// <param name="multiValue">The multivalue to get the items for.</param>
        /// <param name="writingContent">If we're writing content of an entry or not.</param>
        /// <returns>The items enumeration for the multivalue.</returns>
        internal static IEnumerable GetMultiValueItems(
            EpmValueCache epmValueCache,
            EpmSourcePathSegment sourcePathSegment, 
            ODataMultiValue multiValue, 
            bool writingContent)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(multiValue != null, "multiValue != null");
            Debug.Assert(writingContent || epmValueCache != null, "If we're not writing content, then the EPM value cache must exist.");

            if (epmValueCache == null)
            {
                return multiValue.Items;
            }
            else
            {
                return epmValueCache.GetMultiValueItems(sourcePathSegment, multiValue, writingContent);
            }
        }

        /// <summary>
        /// Returns the properties for the specified complex value.
        /// </summary>
        /// <param name="sourcePathSegment">The source path segment for the property which has this complex value.</param>
        /// <param name="complexValue">The complex value to get the properties for.</param>
        /// <param name="writingContent">true if we're writing entry content or false when writing out-of-content EPM.</param>
        /// <returns>The properties enumeration for the complex value.</returns>
        private IEnumerable<ODataProperty> GetComplexValueProperties(EpmSourcePathSegment sourcePathSegment, ODataComplexValue complexValue, bool writingContent)
        {
            Debug.Assert(writingContent || sourcePathSegment != null, "sourcePathSegment must be specified when writing out-of-content.");
            Debug.Assert(complexValue != null, "complexValue != null");

            // If we're writing into content we don't want to populate the cache if it's not already populated.
            // The goal is to behave the same with and without EPM.
            if (writingContent && this.epmValuesCache == null)
            {
                return complexValue.Properties;
            }

            object cachedPropertiesValue;
            if (this.epmValuesCache != null && this.epmValuesCache.TryGetValue(sourcePathSegment, out cachedPropertiesValue))
            {
                Debug.Assert(cachedPropertiesValue is List<ODataProperty>, "The cached value for complex type must be a List of ODataProperty");
                return (IEnumerable<ODataProperty>)cachedPropertiesValue;
            }

            IEnumerable<ODataProperty> properties = complexValue.Properties;
            List<ODataProperty> cachedProperties = null;
            if (properties != null)
            {
                cachedProperties = new List<ODataProperty>(properties);
            }

            if (this.epmValuesCache == null)
            {
                this.epmValuesCache = new Dictionary<EpmSourcePathSegment, object>(ReferenceEqualityComparer<EpmSourcePathSegment>.Instance);
            }

            this.epmValuesCache.Add(sourcePathSegment, cachedProperties);
            return cachedProperties;
        }

        /// <summary>
        /// Returns the items for the specified multivalue.
        /// </summary>
        /// <param name="sourcePathSegment">The source path segment for the property which has this multivalue.</param>
        /// <param name="multiValue">The multivalue to get the items for.</param>
        /// <param name="writingContent">true if we're writing entry content or false when writing out-of-content EPM.</param>
        /// <returns>The items enumeration for the multivalue.</returns>
        private IEnumerable GetMultiValueItems(EpmSourcePathSegment sourcePathSegment, ODataMultiValue multiValue, bool writingContent)
        {
            Debug.Assert(writingContent || sourcePathSegment != null, "sourcePathSegment must be specified when writing out-of-content.");
            Debug.Assert(multiValue != null, "multiValue != null");

            // If we're writing into content we don't want to populate the cache if it's not already populated.
            // The goal is to behave the same with and without EPM.
            if (writingContent && this.epmValuesCache == null)
            {
                return multiValue.Items;
            }

            object cachedItemsValue;
            if (this.epmValuesCache != null && this.epmValuesCache.TryGetValue(sourcePathSegment, out cachedItemsValue))
            {
                Debug.Assert(cachedItemsValue is List<object>, "The cached value for multi value must be a List of object");
                return (IEnumerable)cachedItemsValue;
            }

            IEnumerable items = multiValue.Items;
            List<object> cachedItems = null;
            if (items != null)
            {
                cachedItems = new List<object>();
                foreach (object item in items)
                {
                    // If the value is a complex value, store it as EpmMultiValueItemCache instance, so that we have a place
                    // to cache the enumeration of properties on that complex value (and possible other nested complex/multi values).
                    if (item is ODataComplexValue)
                    {
                        cachedItems.Add(new EpmMultiValueItemCache(item));
                    }
                    else
                    {
                        // Otherwise it should be a primitive value and thus we can just cache the value itself as it won't have any children
                        cachedItems.Add(item);
                    }
                }
            }

            if (this.epmValuesCache == null)
            {
                this.epmValuesCache = new Dictionary<EpmSourcePathSegment, object>(ReferenceEqualityComparer<EpmSourcePathSegment>.Instance);
            }

            this.epmValuesCache.Add(sourcePathSegment, cachedItems);
            return cachedItems;
        }
    }
}
