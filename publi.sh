set -uo pipefail

projectPath="Quartz.Presentation/Quartz.Presentation.csproj"
projectName="Quartz.Presentation"
targetFramework="net10.0"
buildConfiguration="Release"

runtimeIdentifiers=(
	win-x86
	win-x64
	win-arm64
	osx-x64
	osx-arm64
	linux-x64
	linux-arm
	linux-arm64
)

outputDirectory="publish"
mkdir -p "$outputDirectory"

failedIdentifiers=()

for runtimeIdentifier in "${runtimeIdentifiers[@]}"; do
	echo "Publishing $projectName for RuntimeIdentifier=$runtimeIdentifier"
	
	temporaryDirectory=$(mktemp -d)
	logFilePath="$temporaryDirectory/publish.log"
	
	executableExtension=""
	if [[ "$runtimeIdentifier" == win-* ]]; then
		executableExtension=".exe"
	fi

	if dotnet publish "$projectPath" \
		-c "$buildConfiguration" \
		-f "$targetFramework" \
		-r "$runtimeIdentifier" \
		--self-contained true \
		-p:PublishSingleFile=true \
		-p:PublishTrimmed=true \
		-o "$temporaryDirectory" > "$logFilePath" 2>&1 && \
		[ -f "$temporaryDirectory/$projectName$executableExtension" ]; then
		
		targetArtifactPath="$outputDirectory/$runtimeIdentifier$executableExtension"
		mv "$temporaryDirectory/$projectName$executableExtension" "$targetArtifactPath"
		echo "  -> OK"
		
	else
		targetLogPath="$outputDirectory/$runtimeIdentifier.log"
		mv "$logFilePath" "$targetLogPath"
		echo "  -> FAILED"
		failedIdentifiers+=("$runtimeIdentifier")
	fi

	rm -rf "$temporaryDirectory"
done

if [ ${#failedIdentifiers[@]} -eq 0 ]; then
	echo "Publish finished successfully. All artifacts are strictly in $outputDirectory/"
	exit 0
fi

echo "Publish finished with failures: ${failedIdentifiers[*]}"
exit 0
