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
    #region Namespaces.
    using System.Diagnostics;
    #endregion Namespaces.

    /// <summary>
    /// Represents an associated entity link (the result of a $link query).
    /// </summary>
    [DebuggerDisplay("{Link.OriginalString}")]
#if INTERNAL_DROP
    internal class ODataAssociatedEntityLink : ODataAnnotatable
#else
    public class ODataAssociatedEntityLink : ODataAnnotatable
#endif
    {
        /// <summary>
        /// URI representing the Unified Resource Locator (Url) of the associated entity.
        /// </summary>
        /// <remarks>This URL should be usable to retrieve or modify the associated entity.</remarks>
        public Uri Url
        {
            get;
            set;
        }
    }
}
