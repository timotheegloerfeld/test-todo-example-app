module Tests

open System
open Xunit
open FsUnit.Xunit
open FsUnit
open FsHttp
open System.Text.Json
open System.Text.Json.Serialization
open System.Net

GlobalConfig.Json.defaultJsonSerializerOptions <-
    let options = JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter())
    options

type Todo =
    { Id: Guid
      Text: string
      IsChecked: bool
      CreatedAt: DateTime
      UpdatedAt: DateTime }

[<Fact>]
let ``Get Todos`` () =
    let response =
        http { GET "http://localhost:5228/todos" }
        |> Request.send

    let todos = response |> Response.deserializeJson<List<Todo>>

    response.statusCode
    |> should equal HttpStatusCode.OK

    todos |> should not' (equal Empty)
