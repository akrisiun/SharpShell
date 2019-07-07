@echo off

@:Build
@REM "c:\Program Files (x86)\"
@set MSBuild="%ProgramFiles% (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe"
@REM @if not exist %MSBuild% @set msbuild="%ProgramFiles%\MSBuild\14.0\Bin\MSBuild.exe"

set out=%~dp0lib\
echo Out %out%
@REM $(Configuration)|$(Platform)' == 'Debug|x86'  /p:Platform="Any CPU"
@REM /p:OutDir="%out%\" 
@REM MSBuild SharpShellLib/SharpShellLib.csproj /m /p:Configuration="Debug" /v:M

@echo %MSBuild% SharpShell.sln /m /nr:false /p:Platform="Any CPU" /v:M
%MSBuild% SharpShell.sln /m /nr:false /p:Platform="Any CPU" /v:M

@REM if %ERRORLEVEL% neq 0 goto BuildFail
