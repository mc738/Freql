namespace Freql.Tools.CodeFirst.Core

module Operations =

    type RecordTrackingOperation = UpdateField of UpdateFieldOperation

    and UpdateFieldOperation =
        { TableName: string
          FieldName: string
          NewValue: obj }


    type UpdateField = { FieldName: string; NewValue: obj }
