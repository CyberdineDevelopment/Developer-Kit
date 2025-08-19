// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Roslyn", "RS1042", Justification = "ISourceGenerator usage is acceptable for this specific use case")]
[assembly: SuppressMessage("Roslyn", "RS1036", Justification = "EnforceExtendedAnalyzerRules not required for this project")]
[assembly: SuppressMessage("Meziantou.Analyzer", "MA0002", Justification = "StringComparer not required in all cases")]
[assembly: SuppressMessage("Meziantou.Analyzer", "MA0051", Justification = "Method length is acceptable for generated code")]