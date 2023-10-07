open System.Text.RegularExpressions
open System.IO

let table_name_pattern = Regex(@"^.*?`([^`]+)")
let values_pattern = Regex(@"\((.*)\)")

let mutable curr: FileStream option = None

let filePath = "test.dump"
for line in File.ReadLines(filePath) do
    if line.StartsWith("INSERT") then
        let table_name = table_name_pattern.Match(line).Groups[1].Value
        let start, stop = line.IndexOf("(") + 1, line.IndexOf(")")
        let values = line.Substring(start, stop-start).Replace("`", "") + System.Environment.NewLine |> System.Text.Encoding.UTF8.GetBytes
        curr <- Some <| File.OpenWrite($"{table_name}.csv")
        curr |> Option.iter (fun f -> f.Write values)
    else if curr.IsSome && not (line.StartsWith("--")) then
        let stripped = line.Trim([|'('; ')'; ';'; ','|]) + System.Environment.NewLine |> System.Text.Encoding.UTF8.GetBytes
        curr |> Option.iter (fun f -> f.Write stripped)
    else if curr.IsSome then
        curr |> Option.iter (fun f -> f.Close())
        curr <- None