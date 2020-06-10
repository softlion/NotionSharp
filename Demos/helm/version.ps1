$version = "1.0.0"

$sourceName = "notionsharpblog"

$imageName = "raids"
$sshHostName = "ns3059218.ip-193-70-33.eu"
$sshHostUserName = "root"
$targetChartFolder = "~/k8s/"
$helmOptions = "~/k8s/$imageName -n vapolia"

$repo = "dhub.vapolia.eu/${name}"
$repoFull = "${repo}:$version"
