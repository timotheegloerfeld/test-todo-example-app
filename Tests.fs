module Tests

open System
open Xunit
open FsHttp
open System.Text.Json
open System.Text.Json.Serialization
open System.Net
open Respawn
open Npgsql
open Fake.Core
open Xunit.Abstractions

GlobalConfig.Json.defaultJsonSerializerOptions <-
    let options = new JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter())
    options

type RespawnFixture() =
    member this.Reset() =
        let checkpoint = new Checkpoint(DbAdapter = DbAdapter.Postgres)

        use conn =
            new NpgsqlConnection(
                "Server=localhost;Port=5432;Database=postgres;User Id=postgres;Password=mysecretpassword;"
            )

        conn.Open()

        checkpoint.Reset conn
        |> Async.AwaitTask
        |> Async.RunSynchronously

    interface IDisposable with
        member this.Dispose() = this.Reset()

type Todo =
    { Id: Guid
      Text: string
      IsChecked: bool
      CreatedAt: DateTime
      UpdatedAt: DateTime }

type IntegrationTests(respawnFixture: RespawnFixture) =
    do respawnFixture.Reset()

    [<Fact>]
    member _.``Get Todos``() =
        let response =
            http { GET "http://localhost:5228/todos" }
            |> Request.send

        let todos = response |> Response.deserializeJson<List<Todo>>

        Assert.Equal(HttpStatusCode.OK, response.statusCode)
        Assert.Empty(todos)


    [<Fact>]
    member _.``Post Todos``() =
        http {
            POST "http://localhost:5228/todos"
            body

            json
                """
            {
                "text": "new todo"
            }
            """
        }
        |> Request.send
        |> ignore

        let response =
            http { GET "http://localhost:5228/todos" }
            |> Request.send

        let todos = response |> Response.deserializeJson<List<Todo>>

        Assert.Equal(HttpStatusCode.OK, response.statusCode)
        Assert.Equal(1, todos.Length)
        Assert.NotEmpty(todos)

    interface IClassFixture<RespawnFixture>
