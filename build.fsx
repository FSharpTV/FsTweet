#r "src/packages/FAKE/tools/FakeLib.dll"
open Fake
open System.IO
open System.Diagnostics
let buildDir = FullName "build"
let srcDir = FullName "src"
let appName = "FsTweet.Web"

let noFilter = (fun _ -> true)
let copyAssetsDir () =
  let srcAssetsDir = Path.Combine(srcDir, appName, "assets")
  let targetAssetsDir = Path.Combine(buildDir, "assets")
  DeleteDir targetAssetsDir
  CopyDir targetAssetsDir srcAssetsDir noFilter

let copyViewsDir () =
  let srcViewsDir = Path.Combine(srcDir, appName, "views")
  let targetViewsDir = Path.Combine(buildDir, "views")
  DeleteDir targetViewsDir
  CopyDir targetViewsDir srcViewsDir noFilter

Target "Clean" (fun _ -> CleanDirs [buildDir;])

Target "UI" (fun _ ->    
              copyAssetsDir()
              copyViewsDir() ) 
let build () =
  !! "src/FsTweet.sln"
    |> MSBuildRelease buildDir "Build"
    |> ignore  

Target "Build" (fun _ -> build())
let runApp() =
  let app = Path.Combine(buildDir, appName + ".exe")    
  if fileExists app then
    execProcess (fun info -> info.FileName <- app) System.TimeSpan.MaxValue |> ignore
let onFileChange f =
  killAllCreatedProcesses()
  f()
  runApp()
let handleWatcherEvents (e:System.IO.FileSystemEventArgs) =
  let fileExtension = Path.GetExtension(e.FullPath)
  match fileExtension with
  | ".fs" -> onFileChange build 
  | ".liquid" -> onFileChange copyViewsDir
  | ".css" | ".png" | ".js" -> onFileChange copyAssetsDir
  | _ -> ()

Target "Watch" (fun _ -> 
  use watcher = new FileSystemWatcher(srcDir, "*.*", EnableRaisingEvents = true, IncludeSubdirectories = true)
  watcher.Changed.Add(handleWatcherEvents)
  watcher.Created.Add(handleWatcherEvents)
  watcher.Renamed.Add(handleWatcherEvents)
  runApp()
  System.Console.ReadLine() |> ignore
)

"Clean"
  ==> "UI"
  ==> "Build"
  ==> "Watch"
 
RunTargetOrDefault "Watch"