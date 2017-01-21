#r "src/packages/FAKE/tools/FakeLib.dll"
open Fake
open System.IO
open System.IO
open System.Text.RegularExpressions
let buildDir = FullName "build"
let srcDir = FullName "src"

let noFilter = (fun _ -> true)
let copyAssetsDir () =
  let srcAssetsDir = Path.Combine(srcDir, "FsTweet.Web", "assets")
  let targetAssetsDir = Path.Combine(buildDir, "assets")
  DeleteDir targetAssetsDir
  CopyDir targetAssetsDir srcAssetsDir noFilter

let copyViewsDir () =
  let srcViewsDir = Path.Combine(srcDir, "FsTweet.Web", "views")
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

let rec watch() =
    let codeFolder = FullName "src"
    use watcher = new FileSystemWatcher(codeFolder, "*.*")    
    watcher.EnableRaisingEvents <- true
    watcher.IncludeSubdirectories <- true
    watcher.Changed.Add(handleWatcherEvents)
    watcher.Created.Add(handleWatcherEvents)
    watcher.Renamed.Add(handleWatcherEvents)
    System.Console.ReadLine() |> ignore
    watcher.Dispose()

and handleWatcherEvents (e:System.IO.FileSystemEventArgs) =
    let fileExtension = Path.GetExtension(e.FullPath)
    match fileExtension with
    | ".fs" -> build ()
    | ".liquid" -> copyViewsDir ()
    | ".css" | ".png" | ".js" -> copyAssetsDir ()
    | _ -> ()
    watch()

Target "Watch" (fun _ -> watch())

"Clean"
  ==> "UI"
  ==> "Build"

"Build"
  ==> "Watch"
 
RunTargetOrDefault "Watch"