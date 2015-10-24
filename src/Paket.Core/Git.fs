module Paket.Git

open System

type private ProcessResult = { exitCode : int; stdout : string; stderr : string }

type GitLink = {
    Url: string
    Name: string
}

type GitLink with
    static member Empty
         with get() = { Url = ""; Name = "" }

let private parseHash (branch, output:string):string =
    let split = output.Split ([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
    let find = split |> Array.map (fun line -> line.Split([|' '; '\t'|], StringSplitOptions.RemoveEmptyEntries )) |> Array.tryFind (fun line -> line.[1].Contains branch)
    match find with // FIXME rewrite
        | Some x -> x.[0]
        | None -> failwithf "Cannot find sha1 hash for %s" branch

type private Repo(project, url) =
    let projectDir = IO.Path.Combine(Constants.PaketFilesFolderName, project)

    let log (res:ProcessResult) =
        if res.exitCode <> 0 then
            failwithf "git exit code: %i. Error: %s" res.exitCode res.stderr
        res.stdout
    
    let executeCommand setWD command =
        let mutable proc = null
        try
            let psi = new System.Diagnostics.ProcessStartInfo("git.exe", command) 
            psi.UseShellExecute <- false
            psi.RedirectStandardOutput <- true
            psi.RedirectStandardError <- true
            psi.CreateNoWindow <- true
            if setWD && IO.Directory.Exists projectDir then
                psi.WorkingDirectory <- projectDir
            proc <- System.Diagnostics.Process.Start(psi)
            let output = new System.Text.StringBuilder()
            let error = new System.Text.StringBuilder()
            proc.OutputDataReceived.Add(fun args -> output.AppendLine(args.Data) |> ignore)
            proc.ErrorDataReceived.Add(fun args -> error.AppendLine(args.Data) |> ignore)
            proc.BeginErrorReadLine()
            proc.BeginOutputReadLine()
            proc.WaitForExit()
            { exitCode = proc.ExitCode; stdout = output.ToString(); stderr = error.ToString() }
        finally
            if not (isNull proc) then 
                proc.Dispose()

    let clone url target =
        sprintf "clone %s %s" url target |> executeCommand false |> log

    member self.lsRemote =
        sprintf "ls-remote %s" url |> executeCommand true |> log

    member self.tryClone =
        if not (IO.Directory.Exists (projectDir + "/.git")) then
            clone url projectDir |> ignore
    
    member self.checkout branch =
        sprintf "checkout -f %s" branch |> executeCommand true |> log

    member self.readFile fileName =
        let path = IO.Path.Combine (projectDir, fileName)
        if IO.File.Exists(path) then
            Some (IO.File.ReadAllText path)
        else
            None

let getSha1 (link:GitLink, branch) = 
    let repo = Repo(link.Name, link.Url)
    parseHash (branch, repo.lsRemote)

let getFile (link:GitLink, branch, fileName) = async {
    let repo = Repo(link.Name, link.Url)
    repo.tryClone
    repo.checkout branch |> ignore
    return repo.readFile fileName
}

let clone (link:GitLink) branch = 
    let repo = Repo(link.Name, link.Url)
    repo.tryClone
    repo.checkout branch |> ignore