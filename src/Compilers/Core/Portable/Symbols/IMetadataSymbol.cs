// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// Provides the underlying handle for symbols raised from metadata.
    /// </summary>
    public interface IMetadataSymbol
    {
        Handle MetadataHandle { get; }
    }
}
