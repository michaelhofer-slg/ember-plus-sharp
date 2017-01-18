﻿////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// <copyright>Copyright 2012-2017 Lawo AG (http://www.lawo.com).</copyright>
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Lawo.EmberPlusSharp.Glow
{
    using System.Diagnostics.CodeAnalysis;

    using Ember;

    internal static class GlowQualifiedFunction
    {
        internal const int InnerNumber = Ember.InnerNumber.FirstApplication + 20;
        internal const string Name = "QualifiedFunction";

        internal static class Path
        {
            internal const int OuterNumber = 0;
            internal const string Name = "path";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class Contents
        {
            internal const int OuterNumber = 1;
            internal const string Name = "contents";

            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Used through reflection.")]
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }

        internal static class Children
        {
            internal const int OuterNumber = 2;
            internal const string Name = "children";
            internal static readonly EmberId OuterId = EmberId.CreateContextSpecific(OuterNumber);
        }
    }
}
