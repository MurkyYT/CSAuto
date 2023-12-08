if not exist "%1src\CSAuto\APIKeys.resx" (
	echo f | xcopy /y "%1_APIKeys.resx" "%1src\CSAuto\APIKeys.resx"
	echo f | xcopy /y "%1_APIKeys.Designer.cs" "%1src\CSAuto\APIKeys.Designer.cs"
	
)