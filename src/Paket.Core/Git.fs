module Paket.Git

open System

type private ProcessResult = { exitCode : int; stdout : string; stderr : string }

let private log (res:ProcessResult) =
    if res.exitCode <> 0 then
        failwithf "git exit code: %i. Error: %s" res.exitCode res.stderr
    res.stdout
    
let private executeCommand command =
    let psi = new System.Diagnostics.ProcessStartInfo("git.exe", command) 
    psi.UseShellExecute <- false
    psi.RedirectStandardOutput <- true
    psi.RedirectStandardError <- true
    psi.CreateNoWindow <- true
    //psi.WorkingDirectory <- Constants.PaketFilesFolderName
    let proc = System.Diagnostics.Process.Start(psi)
    let output = new System.Text.StringBuilder()
    let error = new System.Text.StringBuilder()
    proc.OutputDataReceived.Add(fun args -> output.AppendLine(args.Data) |> ignore)
    proc.ErrorDataReceived.Add(fun args -> error.AppendLine(args.Data) |> ignore)
    proc.BeginErrorReadLine()
    proc.BeginOutputReadLine()
    proc.WaitForExit()
    { exitCode = proc.ExitCode; stdout = output.ToString(); stderr = error.ToString() }

let private clone url target =
    sprintf "clone %s %s" url target |> executeCommand |> log

let private lsRemote url =
    sprintf "ls-remote %s" url |> executeCommand |> log

let private parseHash (branch, output:string):string =
    let split = output.Split ([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
    let find = split |> Array.map (fun line -> line.Split([|' '; '\t'|], StringSplitOptions.RemoveEmptyEntries )) |> Array.tryFind (fun line -> line.[1].Contains branch)
    match find with // FIXME rewrite
        | Some x -> x.[0]
        | None -> failwithf "Cannot find sha1 hash for %s" branch

type private Repo(project, url) =
    let projectDir = IO.Path.Combine(Constants.PaketFilesFolderName, project)
    member self.tryClone =
        if not (IO.Directory.Exists projectDir) then
            clone url projectDir |> ignore

let getSha1 project url branch = 
    parseHash(branch, lsRemote(url))