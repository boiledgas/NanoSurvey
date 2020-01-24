@echo off

setlocal
	FOR /F "tokens=*" %%i in ('type .env') do set %%i
	netstat -an | findstr /RC:":%SQLSERVER_PORT% .*LISTENING" && echo Порт для SQL SERVER %SQLSERVER_PORT% занят, укажите в файле .env SQLSERVER_PORT любой свободный порт. Необходимо выполнить clean.bat. && goto end
endlocal

docker-compose -f docker-compose-dev.yml up -d

set /p asd="Нажмите Enter для остановки."

docker-compose stop 

:end