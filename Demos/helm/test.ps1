. ./version.ps1

docker build -t $repoFull .

Start-Process http://localhost:8080
docker run -it -p8080:5000 $repoFull
