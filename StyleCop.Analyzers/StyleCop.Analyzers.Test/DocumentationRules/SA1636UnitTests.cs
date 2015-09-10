﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Test.DocumentationRules
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CodeFixes;
    using StyleCop.Analyzers.DocumentationRules;
    using Xunit;

    /// <summary>
    /// Unit tests for the SA1636 diagnostic.
    /// </summary>
    public class SA1636UnitTests : FileHeaderTestBase
    {
        private const string MultiLineHeaderTestSettings = @"
{
  ""settings"": {
    ""documentationRules"": {
      ""companyName"": ""FooCorp"",
      ""copyrightText"": ""copyright (c) {companyName}. All rights reserved.\n\nLine #3""
    }
  }
}
";

        private const string NoXmlMultiLineHeaderTestSettings = @"
{
  ""settings"": {
    ""documentationRules"": {
      ""companyName"": ""FooCorp"",
      ""copyrightText"": ""copyright (c) {companyName}. All rights reserved.\n\nLine #3"",
      ""xmlHeader"": false
    }
  }
}
";

        private bool useMultiLineHeaderTestSettings;
        private bool useNoXmlMultiLineHeaderTestSettings;

        /// <summary>
        /// Verifies that a file header with a copyright message that is different than in the settings will produce the expected diagnostic message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestFileHeaderWithDifferentCopyrightMessageAsync()
        {
            var testCode = @"// <copyright file=""Test0.cs"" company=""FooCorp"">
//   My custom copyright message.
// </copyright>

namespace Bar
{
}
";
            var fixedCode = @"// <copyright file=""Test0.cs"" company=""FooCorp"">
// Copyright (c) FooCorp. All rights reserved.
// </copyright>

namespace Bar
{
}
";

            var expectedDiagnostic = this.CSharpDiagnostic(FileHeaderAnalyzers.SA1636Descriptor).WithLocation(1, 4);
            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnostic, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that a file header with a copyright message that differs only in case from the settings will produce the expected diagnostic message.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestFileHeaderWithInvalidCaseCopyrightMessageAsync()
        {
            var testCode = @"// <copyright file=""Test0.cs"" company=""FooCorp"">
//   copyright (c) FooCorp. All rights reserved.
// </copyright>

namespace Bar
{
}
";
            var fixedCode = @"// <copyright file=""Test0.cs"" company=""FooCorp"">
// Copyright (c) FooCorp. All rights reserved.
// </copyright>

namespace Bar
{
}
";

            var expectedDiagnostic = this.CSharpDiagnostic(FileHeaderAnalyzers.SA1636Descriptor).WithLocation(1, 4);
            await this.VerifyCSharpDiagnosticAsync(testCode, expectedDiagnostic, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that a file header will ignore spurious leading / trailing whitespaces (for multiple line comments)
        /// This is a regression for #1356
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestFileHeaderWillIgnoreLeadingAndTrailingWhitespaceAroundCopyrightMessageAsync()
        {
            this.useMultiLineHeaderTestSettings = true;

            var testCode1 = @"// <copyright file=""Test0.cs"" company=""FooCorp"">
//   copyright (c) FooCorp. All rights reserved.
//
//   Line #3
// </copyright>

namespace Bar
{
}
";

            var testCode2 = @"/* <copyright file=""Test1.cs"" company=""FooCorp"">
  copyright (c) FooCorp. All rights reserved.

  Line #3
</copyright> */

namespace Bar
{
}
";

            var testCode3 = @"/*
 * <copyright file=""Test2.cs"" company=""FooCorp"">
 *   copyright (c) FooCorp. All rights reserved.
 *
 *   Line #3
 * </copyright>
 */

namespace Bar
{
}
";

            await this.VerifyCSharpDiagnosticAsync(new[] { testCode1, testCode2, testCode3 }, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Verifies that a file header without XML header will ignore spurious leading / trailing whitespaces (for multiple line comments)
        /// This is a regression for #1356
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task TestNoXmlFileHeaderWillIgnoreLeadingAndTrailingWhitespaceAroundCopyrightMessageAsync()
        {
            this.useNoXmlMultiLineHeaderTestSettings = true;

            var testCode1 = @"//   copyright (c) FooCorp. All rights reserved.
//
//   Line #3

namespace Bar
{
}
";

            var testCode2 = @"/*
 *   copyright (c) FooCorp. All rights reserved.
 *
 *   Line #3
 */

namespace Bar
{
}
";

            var testCode3 = @"/*
  copyright (c) FooCorp. All rights reserved.

  Line #3
*/

namespace Bar
{
}
";

            await this.VerifyCSharpDiagnosticAsync(new[] { testCode1, testCode2, testCode3 }, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new FileHeaderCodeFixProvider();
        }

        /// <inheritdoc/>
        protected override string GetSettings()
        {
            if (this.useMultiLineHeaderTestSettings)
            {
                return MultiLineHeaderTestSettings;
            }

            if (this.useNoXmlMultiLineHeaderTestSettings)
            {
                return NoXmlMultiLineHeaderTestSettings;
            }

            return base.GetSettings();
        }
    }
}
