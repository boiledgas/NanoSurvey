@echo off
docker-compose rm -s -v -f
docker-compose down --rmi all --remove-orphans
del ".init"