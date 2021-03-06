﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Editor.CSharp.Completion.FileSystem;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Completion.CompletionProviders;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.IntelliSense.CompletionSetSources
{
    public class ReferenceDirectiveCompletionProviderTests : AbstractCSharpCompletionProviderTests
    {
        public ReferenceDirectiveCompletionProviderTests(CSharpTestWorkspaceFixture workspaceFixture) : base(workspaceFixture)
        {
        }

        internal override CompletionListProvider CreateCompletionProvider()
        {
            return new ReferenceDirectiveCompletionProvider();
        }

        protected override bool CompareItems(string actualItem, string expectedItem)
        {
            return actualItem.Equals(expectedItem, StringComparison.OrdinalIgnoreCase);
        }

        protected override void VerifyWorker(string code, int position, string expectedItemOrNull, string expectedDescriptionOrNull, SourceCodeKind sourceCodeKind, bool usePreviousCharAsTrigger, bool checkForAbsence, bool experimental, int? glyph)
        {
            BaseVerifyWorker(code,
                position,
                expectedItemOrNull,
                expectedDescriptionOrNull,
                sourceCodeKind,
                usePreviousCharAsTrigger,
                checkForAbsence,
                glyph);
        }

        private void VerifyItemsExistInScriptAndInteractive(string code, params string[] expected)
        {
            foreach (var ex in expected)
            {
                VerifyItemExists(code, ex, expectedDescriptionOrNull: null, sourceCodeKind: SourceCodeKind.Script);
            }
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Completion)]
        public void IsCommitCharacterTest()
        {
            var commitCharacters = new[] { '"', '\\', ',' };
            VerifyCommitCharacters("#r \"$$", textTypedSoFar: "", validChars: commitCharacters);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Completion)]
        public void IsTextualTriggerCharacterTest()
        {
            var validMarkupList = new[]
            {
                "#r \"$$\\",
                "#r \"$$,",
                "#r \"$$A"
            };

            foreach (var markup in validMarkupList)
            {
                VerifyTextualTriggerCharacter(markup, shouldTriggerWithTriggerOnLettersEnabled: true, shouldTriggerWithTriggerOnLettersDisabled: true);
            }

            var invalidMarkupList = new[]
            {
                "#r \"$$/",
                "#r \"$$!",
                "#r \"$$(",
            };

            foreach (var markup in invalidMarkupList)
            {
                VerifyTextualTriggerCharacter(markup, shouldTriggerWithTriggerOnLettersEnabled: false, shouldTriggerWithTriggerOnLettersDisabled: false);
            }
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Completion)]
        public void SendEnterThroughToEditorTest()
        {
            VerifySendEnterThroughToEnter("#r \"System$$", "System", sendThroughEnterEnabled: false, expected: false);
            VerifySendEnterThroughToEnter("#r \"System$$", "System", sendThroughEnterEnabled: true, expected: false);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Completion)]
        public void RootDrives()
        {
            // ensure drives are listed without the trailing backslash
            var drive = Environment.GetLogicalDrives().First().TrimEnd('\\');
            VerifyItemsExistInScriptAndInteractive(
                "#r \"$$",
                drive);
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Completion)]
        public void RelativeDirectories()
        {
            VerifyItemsExistInScriptAndInteractive(
                "#r \"$$",
                ".",
                "..");
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Completion)]
        public void GACReference()
        {
            VerifyItemsExistInScriptAndInteractive(
                "#r \"$$",
                "System.Windows.Forms");
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Completion)]
        public void GACReferenceFullyQualified()
        {
            VerifyItemsExistInScriptAndInteractive(
                "#r \"System.Windows.Forms,$$",
                "System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.Completion)]
        public void FileSystemReference()
        {
            string systemDir = Path.GetFullPath(Environment.SystemDirectory);
            DirectoryInfo windowsDir = System.IO.Directory.GetParent(systemDir);
            string windowsDirPath = windowsDir.FullName;
            string windowsRoot = System.IO.Directory.GetDirectoryRoot(systemDir);

            // we need to get the exact casing from the file system:
            var normalizedWindowsPath = Directory.GetDirectories(windowsRoot, windowsDir.Name).Single();
            var windowsFolderName = Path.GetFileName(normalizedWindowsPath);

            var code = "#r \"" + windowsRoot + "$$";
            VerifyItemsExistInScriptAndInteractive(
                code,
                windowsFolderName);
        }
    }
}
