#####set /p nugetServer=Enter base nuget server url (with /): 
if ($IsMacOS) {
    $msbuild = "msbuild"
} else {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    $msbuild = join-path $msbuild 'MSBuild\Current\Bin\MSBuild.exe'
}

#####################
#Build release config
cd $PSScriptRoot
del *.nupkg
& $msbuild "NotionSharp.csproj" /restore /p:Configuration=Release /p:Platform="Any CPU" /p:Deterministic=false /p:PackageOutputPath="$PSScriptRoot" --% /t:Clean;Build;Pack
if ($lastexitcode -ne 0) { exit $lastexitcode; }

####################
#Distribute

nuget push "Softlion.NotionSharp.*.nupkg"
