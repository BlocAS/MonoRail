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
    /// <summary>
    /// Enumerations missing from SyndicationItemProperty which can be parent of other SyndicationItemProperties.
    /// </summary>
    internal enum EpmSyndicationParent
    {
        /// <summary>
        /// atom:entry
        /// </summary>
        Entry,

        /// <summary>
        /// atom:author
        /// </summary>
        Author,

        /// <summary>
        /// atom:category
        /// </summary>
        Category,

        /// <summary>
        /// atom:contributor
        /// </summary>
        Contributor,

        /// <summary>
        /// atom:link
        /// </summary>
        Link,
    }
}
