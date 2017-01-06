REM ********************************************************************************************************************
REM * Hi-Jack the Auto Build Variables by QtAgent since this is injected after it has REM * setup
REM * Open the autogenerated qtREM * setup in the test run location of
REM * C:\Users\IntegrationTester\AppData\Local\VSEQT\QTAgent\...
REM * For example:
REM * set DeploymentDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\DEPLOY~1
REM * set TestRunDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1
REM * set TestRunResultsDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results\RSAKLF~1
REM * set TotalAgents=5
REM * set AgentWeighting=100
REM * set AgentLoadDistributor=Microsoft.VisualStudio.TestTools.Execution.AgentLoadDistributor
REM * set AgentId=1
REM * set TestDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1
REM * set ResultsDirectory=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results
REM * set DataCollectionEnvironmentContext=Microsoft.VisualStudio.TestTools.Execution.DataCollectionEnvironmentContext
REM * set TestLogsDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\Results\RSAKLF~1
REM * set ControllerName=rsaklfsvrtfsbld:6901
REM * set TestDeploymentDir=C:\Users\INTEGR~1\AppData\Local\VSEQT\QTAgent\54371B~1\RSAKLF~1\DEPLOY~1
REM * set AgentName=RSAKLFTST7X64-3
REM ********************************************************************************************************************

REM ** Check for admin **
@echo off
echo Administrative permissions required. Detecting permissions...
REM using the "net session" command to detect admin, it requires elevation in the most operating systems - Ashley
IF EXIST %windir%\nircmd.exe (nircmd elevate net session >nul 2>&1) else (net session >nul 2>&1)
if %errorLevel% == 0 (
	echo Success: Administrative permissions confirmed.
) else (
	echo Failure: Current permissions inadequate.
	exit 1
)

REM ** Cleanup **
call "%~dp0Cleanup.bat"

REM Init paths to Warewolf server under test
IF EXIST "%DeploymentDirectory%\DebugServer.zip" powershell.exe -nologo -noprofile -command "& { Expand-Archive '%DeploymentDirectory%\DebugServer.zip' '%DeploymentDirectory%\Server' -Force }"
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\..\Dev2.Server\bin\Debug\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\..\Dev2.Server\bin\Debug
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0Server\Warewolf Server.exe" SET DeploymentDirectory=%~dp0Server
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0Warewolf Server.exe" SET DeploymentDirectory=%~dp0
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\..\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\..
IF "%DeploymentDirectory%"=="" IF EXIST "%~dp0..\..\..\..\Warewolf Server.exe" SET DeploymentDirectory=%~dp0..\..\..\..
IF EXIST "%DeploymentDirectory%\Server\Warewolf Server.exe" SET DeploymentDirectory=%DeploymentDirectory%\Server
IF EXIST "%DeploymentDirectory%\ServerStarted" DEL "%DeploymentDirectory%\ServerStarted"

sc interrogate "Warewolf Server"
if %ERRORLEVEL% EQU 1060 GOTO NotInstalled
if %ERRORLEVEL% EQU 1061 GOTO NotReady
if %ERRORLEVEL% EQU 1062 GOTO NotStarted
if %ERRORLEVEL% EQU 0 GOTO Running

:NotInstalled
@echo on
@echo Creating Warewolf Server Service... Please any key to continue. Consider changing the user the service runs as to ensure accurate test results.
PAUSE
IF EXIST %windir%\nircmd.exe (nircmd elevate sc create "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand) else (sc create "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand)
GOTO StartService

:NotReady
set /a LoopCounter=0
:WaitForServiceReadyLoopBody
IF NOT %ERRORLEVEL% EQU 1061 GOTO Running
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 60 exit 1
rem wait for 10 seconds before trying again
@echo %AgentName% is attempting number %LoopCounter% out of 60: Waiting 10 more seconds for server service to be ready...
waitfor ServiceReady /t 10 2>NUL
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq RUNNING")
IF EXIST %windir%\nircmd.exe (nircmd elevate taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN") else (taskkill /f /im "Warewolf Server.exe" /fi "STATUS eq UNKNOWN")
sc interrogate "Warewolf Server"
goto WaitForServiceReadyLoopBody

:Running
IF EXIST %windir%\nircmd.exe (nircmd elevate sc stop "Warewolf Server") else (sc stop "Warewolf Server")
IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand) else (sc config "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand)
GOTO StartService

:NotStarted
IF EXIST %windir%\nircmd.exe (nircmd elevate sc config "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand) else (sc config "Warewolf Server" binPath= "%DeploymentDirectory%\Warewolf Server.exe" start= demand)
GOTO StartService

:StartService
REM ** Try Refresh Warewolf Server Bin Resources and Tests
IF EXIST "%~dp0..\..\..\Resources - ServerTests" echo d | xcopy /S /Y "%~dp0..\..\..\Resourses - ServerTests" "%DeploymentDirectory%\Resources - ServerTests"

REM ** Try Refresh Warewolf ProgramData Resources and Tests
IF NOT EXIST "%ProgramData%\Warewolf\Resources" IF EXIST "%DeploymentDirectory%\Resources - ServerTests" echo d | xcopy /S /Y "%DeploymentDirectory%\Resources - ServerTests" "%ProgramData%\Warewolf"

REM ** Start the server service
IF EXIST %windir%\nircmd.exe (nircmd elevate sc start "Warewolf Server") else (sc start "Warewolf Server")

:WaitForServerStart
set /a LoopCounter=0
:WaitForServerStartLoopBody
IF EXIST "%DeploymentDirectory%\ServerStarted" GOTO ServerStarted
set /a LoopCounter=LoopCounter+1
IF %LoopCounter% EQU 60 exit 1
rem wait for 10 seconds before trying again
@echo %AgentName% is attempting number %LoopCounter% out of 60: Waiting 10 more seconds for "%DeploymentDirectory%\ServerStarted" file to appear...
waitfor ServerStart /t 10 2>NUL
goto WaitForServerStartLoopBody

:ServerStarted
IF EXIST "%programdata%\Warewolf\Server Log\wareWolf-Server.log" TYPE "%programdata%\Warewolf\Server Log\wareWolf-Server.log"
exit 0
