clean:
	rm -rf ./bin
	rm -rf ./build
	rm -rf ./Libs/bs_janitor/.cache
	rm -rf ./Libs/bs_janitor/build
	rm -rf ./obj

build:
	dotnet build ./BS_Janitor.csproj --configuration Release
