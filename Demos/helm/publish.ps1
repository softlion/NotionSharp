#requires -PSEdition Core
$ErrorActionPreference = "Stop";

. ./version.ps1

Push-Location ..

try 
{
    ####
    #### build and publish docker image
    ####
    docker build -t $repoFull .
    if (!$?) { exit $lastexitcode; }
    docker push $repoFull
    if (!$?) { exit $lastexitcode; }
}
finally
{
    Pop-Location
}

# Set version of docker image in helm chart
$valuesfile = "$sourceName\values.yaml"
(Get-Content $valuesfile) -replace 'repository: (.*)$', "repository: $repo" | Set-Content $valuesfile
if (!$?) { exit $lastexitcode; }
(Get-Content $valuesfile) -replace 'tag: (.*)$', "tag: $version" | Set-Content $valuesfile
if (!$?) { exit $lastexitcode; }
$valuesfile = "$sourceName\Charts.yaml"
(Get-Content $valuesfile) -replace 'name: (.*)$', "name: $imageName" | Set-Content $valuesfile
if (!$?) { exit $lastexitcode; }

####
#### publish heml chart on server
####
$session = New-PSSession -HostName $sshHostName -UserName $sshHostUserName
if (!$?) { exit $lastexitcode; }

#copy chart
Copy-Item -ToSession $session -Force -Recurse "$sourceName" $targetChartFolder
if (!$?) { exit $lastexitcode; }

#upgrade kubernetes from this updated heml chart on server
Invoke-Command -Session $session -ScriptBlock { iex "helm upgrade $imageName --install $helmOptions" }
if (!$?) { exit $lastexitcode; }

Remove-PSSession $session
if (!$?) { exit $lastexitcode; }
