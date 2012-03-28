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
    /// Atom metadata description for a person.
    /// </summary>
#if INTERNAL_DROP
    internal sealed class AtomPersonMetadata : ODataAnnotatable
#else
    public sealed class AtomPersonMetadata : ODataAnnotatable
#endif
    {
        /// <summary>The name of the person.</summary>
        private string name;

        /// <summary>The email of the person.</summary>
        private string email;

        /// <summary>The URI value comming from EPM.</summary>
        /// <remarks>In WCF DS when mapping a property through EPM to person/uri element we convert the value of the property to string
        /// and then set the syndication APIs Uri property which is also of type string. Syndication API doesn't do any validation on the value
        /// and just writes it out. So it's risky to try to convert the string to a Uri instance due to the unknown validation the Uri class
        /// might be doing. Instead we use internal property to set from EPM.</remarks>
        private string uriFromEpm;

        /// <summary>
        /// The name of the person (required).
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                // TODO ckerer: validate that the value is not null
                this.name = value;
            }
        }

        /// <summary>
        /// An IRI associated with the person.
        /// </summary>
        public Uri Uri
        {
            get;
            set;
        }

        /// <summary>
        /// An email address associated with the person.
        /// </summary>
        public string Email
        {
            get
            {
                return this.email;
            }

            set
            {
                // TODO ckerer: validate required format
                // xsd:string { pattern = ".+@.+" }
                // If we add this validation we will have to make an exception for EPM, so either some internal setter
                // or internal property as for Uri.
                this.email = value;
            }
        }

        /// <summary>The URI value comming from EPM.</summary>
        /// <remarks>In WCF DS when mapping a property through EPM to person/uri element we convert the value of the property to string
        /// and then set the syndication APIs Uri property which is also of type string. Syndication API doesn't do any validation on the value
        /// and just writes it out. So it's risky to try to convert the string to a Uri instance due to the unknown validation the Uri class
        /// might be doing. Instead we use internal property to set from EPM.</remarks>
        internal string UriFromEpm
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.uriFromEpm;
            }

            set
            {
                DebugUtils.CheckNoExternalCallers();
                this.uriFromEpm = value;
            }
        }
    }
}
