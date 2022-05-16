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

    member _.getTodos() =
        http { GET "http://localhost:5228/todos" }
        |> Request.send
        |> Response.deserializeJson<List<Todo>>


    member _.createTodo() =
        let response =
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

        response.headers.Location.AbsolutePath

    [<Fact>]
    member _.``Get Todos``() =
        let response =
            http { GET "http://localhost:5228/todos" }
            |> Request.send

        let todos = response |> Response.deserializeJson<List<Todo>>

        Assert.Equal(HttpStatusCode.OK, response.statusCode)
        Assert.Empty(todos)


    [<Fact>]
    member this.``Post Todos``() =
        let response =
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

        let todos = this.getTodos ()

        Assert.Equal(HttpStatusCode.Created, response.statusCode)
        Assert.Equal(1, todos.Length)

    [<Fact>]
    member _.``Get Single Todo``() = failwith "Not implemented"

    [<Fact>]
    member _.``Delete Todo``() = failwith "Not implemented"

    [<Fact>]
    member _.``Check Todo``() = failwith "Not implemented"

    [<Fact>]
    member _.``Uncheck Todo``() = failwith "Not implemented"


    interface IClassFixture<RespawnFixture>
