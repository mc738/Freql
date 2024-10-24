namespace Freql.Core

open System

module Diagnostics =

    type DiagnosticsSettings =
        { Enabled: bool
          IncludeQueries: bool
          IncludeParameters: bool
          Truncation: int
          DefaultMask: string }

    let truncateString (settings: DiagnosticsSettings) (value: string) =
        if settings.Truncation > -1 then
            value.AsSpan(0, settings.Truncation)
        else
            value.AsSpan()

    type FieldDiagnosticSettings = { Sensitive: bool; Mask: string }
