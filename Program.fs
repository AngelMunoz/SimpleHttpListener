open System
open System.Net;
open System.Threading;
open System.Text

type AuthServer(?port: int) =
    let httpListener = new HttpListener()
    let port = 
        match port with 
        | Some port -> port 
        | None -> 99520
    let url = sprintf "http://localhost:%i/" port
    let mutable stopThread = false
    
    [<DefaultValue(true)>]
    val mutable responseThread: Thread

    let threadResponse _ =
        while not stopThread do
            let ctx = httpListener.GetContext()
            let mutable keyvalues = []
            for key in ctx.Request.QueryString do
                let value = ctx.Request.QueryString.Get key
                let together = sprintf "%s -> %s" key value
                keyvalues <- together :: keyvalues
            
            let bytes = 
                let content: string = 
                    sprintf ("""
                    <html>
                    <head></head>
                    <body>
                        %A <br />
                        %s
                    </body>
                    """) keyvalues (ctx.Request.RawUrl)
                Encoding.UTF8.GetBytes(content)
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length)
            ctx.Response.KeepAlive <- false
            ctx.Response.Close()
        ()
    
    member __.Start() =
        httpListener.Prefixes.Add(url)
        printfn "Starting Auth server in  %s" url
        httpListener.Start()
        stopThread <- false
        __.responseThread <- Thread(ParameterizedThreadStart(threadResponse))
        __.responseThread.Start()

    member __.Stop() =
        stopThread <- true
        httpListener.Stop()

[<EntryPoint>]
let main argv =
    let server = AuthServer(7580)
    server.Start()
    0 // return an integer exit code