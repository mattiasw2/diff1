@echo off
echo Cleaning previous test results...
if exist "tests\DiffTests\TestResults" rmdir /s /q "tests\DiffTests\TestResults"
if exist "coverage-report" rmdir /s /q "coverage-report"

echo Running tests with coverage...
dotnet test /p:CollectCoverage=true /p:CoverletOutput=./TestResults/ /p:CoverletOutputFormat=cobertura

echo Generating HTML report...
reportgenerator -reports:tests/DiffTests/TestResults/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html

echo.
echo Coverage report generated in coverage-report/index.html
echo.
