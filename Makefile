build: 
	dotnet publish --ucr -c Debug -o . -p:PublishSingleFile=true -p:AssemblyName=ipk24chat-server -p:DebugType=None -p:DebugSymbols=false	