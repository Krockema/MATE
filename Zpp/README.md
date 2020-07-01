# Install Docker for Sql Server
- Download: 
https://github.com/docker/docker.github.io/issues/6910
- Install sql server for Unix
sudo docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=123*Start#' -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2017-latest
- install for windows: (double-quote bug)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=123*Start#" -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2017-latest
# Zeige Docker Prozesse
sudo docker ps -a
# Restart docker container
docker restart sql1
- use azuredataStudio
- See:
https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-2017&pivots=cs1-bash

# delete all random databases with prefix zpp
SELECT 'DROP DATABASE "' + NAME + '"' from master.sys.databases
where NAME like 'zpp%'
ORDER BY NAME