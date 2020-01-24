@echo off

setlocal
	FOR /F "tokens=*" %%i in ('type .env') do set %%i
	netstat -an | findstr /RC:":%SQLSERVER_PORT% .*LISTENING" && echo Порт для SQL SERVER %SQLSERVER_PORT% занят, укажите в файле .env SQLSERVER_PORT любой свободный порт. Необходимо выполнить clean.bat. && goto end
	netstat -an | findstr /RC:":%API_PORT% .*LISTENING" && echo Порт для API %API_PORT% занят, укажите в файле .env API_PORT любой свободный порт. Необходимо выполнить clean.bat. && goto end
endlocal

docker-compose up -d

if exist .init (
    timeout /t 5
) else (
    timeout /t 60
	touch .init
)

setlocal
	FOR /F "tokens=*" %%i in ('type .env') do set %%i
	start http://localhost:%API_PORT%/swagger
endlocal

set /p asd="Нажмите Enter для остановки."

docker-compose stop 

:end