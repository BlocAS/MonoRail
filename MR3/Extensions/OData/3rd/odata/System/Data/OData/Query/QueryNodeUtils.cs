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

namespace System.Data.OData.Query
{
    #region Namespaces.
    using System.Data.Services.Providers;
    using System.Diagnostics;
    #endregion Namespaces.

    /// <summary>
    /// Helper methods for working with query nodes.
    /// </summary>
    internal static class QueryNodeUtils
    {
        /// <summary>
        /// Checks whether a query node is a collection query node representing a collection of entities.
        /// </summary>
        /// <param name="query">The <see cref="QueryNode"/> to check.</param>
        /// <returns>The converted <see cref="CollectionQueryNode"/> or null if <paramref name="query"/> is not an entity collection node.</returns>
        internal static CollectionQueryNode AsEntityCollectionNode(this QueryNode query)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(query != null, "query != null");

            CollectionQueryNode collectionNode = query as CollectionQueryNode;
            if (collectionNode != null &&
                collectionNode.ItemType != null &&
                collectionNode.ItemType.ResourceTypeKind == ResourceTypeKind.EntityType)
            {
                return collectionNode;
            }

            return null;
        }

        /// <summary>
        /// Compute the result type of a binary operator based on the type of its operands and the operator kind.
        /// </summary>
        /// <param name="type">The type of the operators.</param>
        /// <param name="operatorKind">The kind of operator.</param>
        /// <returns>The result type of the binary operator.</returns>
        internal static ResourceType GetBinaryOperatorResultType(ResourceType type, BinaryOperatorKind operatorKind)
        {
            DebugUtils.CheckNoExternalCallers();
            Debug.Assert(type != null, "type != null");

            switch (operatorKind)
            {
                case BinaryOperatorKind.Or:                 // fall through
                case BinaryOperatorKind.And:                // fall through
                case BinaryOperatorKind.Equal:              // fall through
                case BinaryOperatorKind.NotEqual:           // fall through
                case BinaryOperatorKind.GreaterThan:        // fall through
                case BinaryOperatorKind.GreaterThanOrEqual: // fall through
                case BinaryOperatorKind.LessThan:           // fall through
                case BinaryOperatorKind.LessThanOrEqual:
                    Type resultType = Nullable.GetUnderlyingType(type.InstanceType) == null
                        ? typeof(bool)
                        : typeof(bool?);
                    return ResourceType.GetPrimitiveResourceType(resultType);

                case BinaryOperatorKind.Add:        // fall through
                case BinaryOperatorKind.Subtract:   // fall through
                case BinaryOperatorKind.Multiply:   // fall through
                case BinaryOperatorKind.Divide:     // fall through
                case BinaryOperatorKind.Modulo:
                    return type;

                default:
                    throw new ODataException(Strings.General_InternalError(InternalErrorCodes.QueryNodeUtils_BinaryOperatorResultType_UnreachableCodepath));
            }
        }
    }
}
