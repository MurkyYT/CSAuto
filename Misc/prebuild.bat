if not exist "%1src\CSAuto\APIKeys.resx" (
	echo f | xcopy /y "%1Misc\_APIKeys.resx" "%1src\CSAuto\APIKeys.resx"
	echo f | xcopy /y "%1Misc\_APIKeys.Designer.cs" "%1src\CSAuto\APIKeys.Designer.cs"
	
)