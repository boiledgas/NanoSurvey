# NanoSurvey

# Запуск демо:
1. **Необходимо** установить [Docker](https://download.docker.com/win/stable/Docker%20Desktop%20Installer.exe) (**linux контейнеры**)
- Для запуска используется файл start.bat.
- Для остановки нужно нажать CTRL+C в окне работы скрипта.
- Для повторного запуска нужно запустить start.bat, состояние БД между запусками не изменяется.
- Для отчистки рабочей машины необходимо выполнить скрипт clean.bat
> БД доступна по localhost, порт и название БД берется из .env.

> После изменения файла .env необходимо выполнить скрипт clean.bat

___

# Разработка
1. Необходимо установить [dotnet sdk 2.1.803](https://dotnet.microsoft.com/download/dotnet-core/2.1)
2. Настройка sql
- Можно запустить docker-start-sql.bat для загрузки БД с тестовыми данными для этого нужен docker
- Если использовать свой SQL Server инициализировать БД нужно вручную скрипт /sql/setup.sql
3. Перед первым запуском отладчика нужно запустить в директории *src* из cmd
	updateDbContext.bat "Server=.\;Database=NanoSurveyDB;User Id=sa;Password=1qaz@WSX"
  
___

# Описание проекта
	NanoSurvey.DB - библиотека классов мэппинга сущностей БД
	NanoSurvey.Generator - генератор данных БД и тестов
	NanoSurvey.Test - проект модульных тестов
	NanoSurvey.Web - веб-приложение апи
# Стек
- aspnet core 2.1
- entity framework core 2.2
- xunit 2.4
- docker 
