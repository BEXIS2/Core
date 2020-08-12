// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Codequalität", "IDE0067:Objekte verwerfen, bevor Bereich verloren geht", Justification = "Dispose did nothing, so its not needed", Scope = "type", Target = "System.Data.DataTable")]
