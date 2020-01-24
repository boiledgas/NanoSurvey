#!/bin/bash

if [ "$1" = '/opt/mssql/bin/sqlservr' ]; then
  if [ ! -f /tmp/app-initialized ]; then
    function initialize_app_database() {
      sleep 15s
	  echo "Создаем БД ${SQL_DATABASE}"
	  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -d master -Q "CREATE DATABASE ${SQL_DATABASE}"
      /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -d ${SQL_DATABASE} -i /home/setup.sql
	  /home/NanoSurvey.Generator
      touch /tmp/app-initialized
    }
    initialize_app_database &
  fi
fi
exec "$@"