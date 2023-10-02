open System.Text.RegularExpressions
open System.IO
open System.Collections.Generic

type Table = { TableName: string; ColumnNames: string; Values: string seq }

let parseInsert (input: string) =
    let pattern = @"INSERT INTO `([^`]+)`\(([^)]+)\) VALUES"
    let regex = Regex(pattern)
    let matchResult = regex.Match(input)

    match matchResult.Success with
    | true ->
        let tableName = matchResult.Groups.[1].Value
        let columnNamesStr = matchResult.Groups.[2].Value
        { TableName = tableName; ColumnNames = columnNamesStr; Values = Seq.empty }
    | false -> failwith "Invalid SQL query"

let addValueRow (table: Table) (valueRow: string) =
    { table with Values = Seq.append table.Values (Seq.singleton valueRow) }

let processLines (lines: string seq) =
    let mutable currentTable = None
    let tableDict = Dictionary<string, Table>()

    for line in lines do
        if line.StartsWith("INSERT") then
            let newTable = parseInsert line
            currentTable <- Some newTable
            printfn "Parsing %s" newTable.TableName
            if not (tableDict.ContainsKey newTable.TableName) then
                tableDict.Add(newTable.TableName, newTable)
        else if line.StartsWith('(') then
            match currentTable with
                | Some table ->
                    let updatedTable = addValueRow table line[1..line.Length-3]
                    currentTable <- Some updatedTable
                    tableDict.[table.TableName] <- updatedTable
                | None -> ()
        else 
            currentTable <- None


    tableDict.Values |> Seq.toList

let exportTables (table: Table) = 
    printfn "Exporting %s" table.TableName 
    let content = Seq.append  (Seq.singleton table.ColumnNames) table.Values
    File.WriteAllLines(table.TableName+".csv",content)

let filePath = "test.dump"
let lines = File.ReadLines(filePath)
let tables = processLines lines

for table in tables do
    exportTables table
// Now, 'tables' contains a list of tables, each with TableName, ColumnNames, and Values as string seq
