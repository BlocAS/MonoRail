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

namespace System.Data.Services.Common
{
    /// <summary>
    /// List of syndication properties settable through <see cref="EntityPropertyMappingAttribute"/>
    /// </summary>
    /// <remarks>
    /// Potentially the following atom specific elements could also be considered:
    /// * Content?
    /// * Id
    /// * Source?
    /// </remarks>
#if INTERNAL_DROP
    internal enum SyndicationItemProperty
#else
    public enum SyndicationItemProperty
#endif
    {
        /// <summary>
        /// User specified a non-syndication property
        /// </summary>
        CustomProperty,

        /// <summary>
        /// author/email
        /// </summary>
        AuthorEmail,

        /// <summary>
        /// author/name
        /// </summary>
        AuthorName,         // Author*

        /// <summary>
        /// author/uri
        /// </summary>
        AuthorUri,

        /// <summary>
        /// contributor/email
        /// </summary>
        ContributorEmail,

        /// <summary>
        /// contributor/name
        /// </summary>
        ContributorName,    // Contributor*

        /// <summary>
        /// contributor/uri
        /// </summary>
        ContributorUri,

        /// <summary>
        /// updated
        /// </summary>
        Updated,

        /// <summary>
        /// published
        /// </summary>
        Published,

        /// <summary>
        /// rights
        /// </summary>
        Rights,

        /// <summary>
        /// summary
        /// </summary>
        Summary,

        /// <summary>
        /// title
        /// </summary>
        Title,

        /// <summary>
        /// category/@label
        /// </summary>
        CategoryLabel,

        /// <summary>
        /// category/@scheme
        /// </summary>
        CategoryScheme,

        /// <summary>
        /// category/@term
        /// </summary>
        CategoryTerm,

        /// <summary>
        /// link/@href
        /// </summary>
        LinkHref,

        /// <summary>
        /// link/@hreflang
        /// </summary>
        LinkHrefLang,

        /// <summary>
        /// link/@length
        /// </summary>
        LinkLength,

        /// <summary>
        /// link/@rel
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "LinkRel is not Hungarian notation")]
        LinkRel,

        /// <summary>
        /// link/@title
        /// </summary>
        LinkTitle,

        /// <summary>
        /// link/@type
        /// </summary>
        LinkType,
    }
}
