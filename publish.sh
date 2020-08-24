# "Build queue packages"

function pack() {
  echo "project $1 build and pack"
  dotnet build $1 -c Release
  dotnet pack $1 -c Release -o ./package
}

rm ./package -rf
pack "./Queue.Core/Queue.Core.csproj"
pack "./Queue.Server.Abstractions/Queue.Server.Abstractions.csproj"
pack "./Queue.Rabbit.Core/Queue.Rabbit.Core.csproj"
pack "./Queue.Rabbit.Server/Queue.Rabbit.Server.csproj"
pack "./Queue.Rabbit.Client/Queue.Rabbit.Client.csproj"
pack "./Queue.Rabbit.Client.HttpClientBuilder/Queue.Rabbit.Client.HttpClientBuilder.csproj"
pack "./Queue.Nats.Core/Queue.Nats.Core.csproj"
pack "./Queue.Nats.Client/Queue.Nats.Client.csproj"
pack "./Queue.Nats.Server/Queue.Nats.Server.csproj"

apiKey="329e6787-4ad0-3595-b829-c668a223540a"
nugetHost="http://nuget.devops.movista.ru:8081/repository/nuget-hosted/"

for f in ./package/*.nupkg; do
  dotnet nuget push $f -s $nugetHost -k $apiKey
done
