@echo off

setlocal
	FOR /F "tokens=*" %%i in ('type .env') do set %%i
	netstat -an | findstr /RC:":%SQLSERVER_PORT% .*LISTENING" && echo ���� ��� SQL SERVER %SQLSERVER_PORT% �����, 㪠��� � 䠩�� .env SQLSERVER_PORT �� ᢮����� ����. ����室��� �믮����� clean.bat. && goto end
	netstat -an | findstr /RC:":%API_PORT% .*LISTENING" && echo ���� ��� API %API_PORT% �����, 㪠��� � 䠩�� .env API_PORT �� ᢮����� ����. ����室��� �믮����� clean.bat. && goto end
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

set /p asd="������ Enter ��� ��⠭����."

docker-compose stop 

:end