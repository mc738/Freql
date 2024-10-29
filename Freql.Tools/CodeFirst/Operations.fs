namespace Freql.Tools.CodeFirst

module Operations =
    
    type RecordTrackingOperation = UpdateField of UpdateFieldOperation


    and UpdateFieldOperation =
        { TableName: string
          FieldName: string
          NewValue: obj }



    
    