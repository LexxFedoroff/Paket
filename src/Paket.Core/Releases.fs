﻿module Paket.Releases

open System.IO
open System.Diagnostics
open Logging
open System
open Chessie.ErrorHandling
open Paket.Domain

let rec private getLatestVersionFromJson (data : string, startPos: int) =
    try 
        let start = data.IndexOf("tag_name", startPos) + 11
        let end' = data.IndexOf("\"", start)
        let v = (data.Substring(start, end' - start)) |> SemVer.Parse
        match v.PreRelease with
        | None -> ok v
        | _ -> getLatestVersionFromJson (data, start)
    with _ ->
        fail ReleasesJsonParseError

let private download version (file:FileInfo) client = 
    trial {
        tracen (sprintf "%A" file)

        do! createDir(file.DirectoryName)
        let url = sprintf "%s/%s/%s" Constants.GithubReleaseDownloadUrl (string version) file.Name
        
        do! downloadFileSync url file.FullName client
    }

let private doesNotExistsOrIsNewer (file : FileInfo) latest = 
    if not file.Exists then true else 
    let verInfo = FileVersionInfo.GetVersionInfo file.FullName
    if isNull verInfo || isNull verInfo.FileVersion  then false else
    let currentVersion = SemVer.Parse verInfo.FileVersion
    currentVersion < latest

/// Downloads the latest version of the given files to the destination dir
let private downloadLatestVersionOf files destDir =
    use client = createWebClient(Constants.GitHubUrl, None)

    trial {
        let! data = client |> downloadStringSync Constants.GitHubReleasesUrl
        let! latestVersion = getLatestVersionFromJson(data,0)

        let files =
            files
            |> List.map (fun file -> FileInfo(Path.Combine(destDir, file)))

        let isOudated =
            files
            |> List.exists (fun file -> doesNotExistsOrIsNewer file latestVersion)

        let! downloads =
            if isOudated then files else []
            |> List.map (fun file -> download latestVersion file client)
            |> collect
        
        ()
    }

/// Downloads the latest version of the paket.bootstrapper to the .paket dir
let downloadLatestBootstrapper environment =
    let exeDir = Path.Combine(environment.RootDirectory.FullName, Constants.PaketFolderName)

    downloadLatestVersionOf [Constants.BootstrapperFileName] exeDir

/// Downloads the latest version of the paket.bootstrapper and paket.targets to the .paket dir
let downloadLatestBootstrapperAndTargets environment =
    let exeDir = Path.Combine(environment.RootDirectory.FullName, Constants.PaketFolderName)

    downloadLatestVersionOf [Constants.TargetsFileName; Constants.BootstrapperFileName] exeDir
