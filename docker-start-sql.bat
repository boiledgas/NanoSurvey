@echo off

setlocal
	FOR /F "tokens=*" %%i in ('type .env') do set %%i
	netstat -an | findstr /RC:":%SQLSERVER_PORT% .*LISTENING" && echo ���� ��� SQL SERVER %SQLSERVER_PORT% �����, 㪠��� � 䠩�� .env SQLSERVER_PORT �� ᢮����� ����. ����室��� �믮����� clean.bat. && goto end
endlocal

docker-compose -f docker-compose-dev.yml up -d

set /p asd="������ Enter ��� ��⠭����."

docker-compose stop 

:end