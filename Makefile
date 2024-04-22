build: 
	dotnet publish --property WarningLevel=0 --ucr -c Debug -o . -p:PublishSingleFile=true -p:AssemblyName=ipk24chat-server -p:DebugType=None -p:DebugSymbols=false	