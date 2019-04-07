#!/bin/bash

#MAIN VARs
MAIN_DIR="/home/user"
PROJECT_NAME="DiscordBot"
DOTNET_CORE_VERSION="2.1"

#WATCH FILES
WATCH_FILE="${MAIN_DIR}/DFBWatch"
CURRENT_TIME_FILE="${MAIN_DIR}/.DFBWatchCurrentTime"

#PROJECT VARS
PROJECT_DIR="${MAIN_DIR}/${PROJECT_NAME}/${PROJECT_NAME}"
PROJECT_FILE="${PROJECT_NAME}.csproj"
PROJECT_DLL="${PROJECT_NAME}.dll"
PROJECT_DLL_DIR="${PROJECT_DIR}/bin/Debug/netcoreapp${DOTNET_CORE_VERSION}"

APP_LOGS_LOCATION="${MAIN_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.log"

#MISC VARs
WAIT_TIME=140
DT=$(date '+%d/%m/%Y %H:%M:%S');
#------------------------------

RUN_CMD()
{
	(/opt/dotnet/dotnet run --project ${PROJECT_DIR}/${PROJECT_FILE} &> $APP_LOGS_LOCATION)&
}

EXE_CMD()
{
	(/opt/dotnet/dotnet exec ${PROJECT_DLL_DIR}/${PROJECT_DLL} &> $APP_LOGS_LOCATION)&
}

KillInsideScreenProcess()
{
	echo "First kill"
        kill $(ps aux | grep 'dotnet exec' | grep $PROJECT_DLL | awk '{print $2}')
	sleep 1
	echo "Second kill (redundantly)"
        kill $(ps aux | grep 'dotnet run' | grep $PROJECT_FILE | awk '{print $2}')  #REDUNDANT!!!!
        printf "Killed dotnet process. Now - waiting..."
        sleep 10
}
#------------------------------

printf "SCRIPT START $DT !\n"

if [ -z $1 ]
then
	touch $CURRENT_TIME_FILE  #File to compare timestamps: this with watchdog file from my app
	echo "CTFile touched. Waiting -> $WAIT_TIME sec <-"
	sleep $WAIT_TIME  #Wait, because watchdog file is refresh in time interval

	if [ $CURRENT_TIME_FILE -nt $WATCH_FILE ]
	then
			KillInsideScreenProcess
			echo "Running app"
			EXE_CMD
	else
			echo 'Watch is newer'
	fi
else
	echo "Compile and run"
	RUN_CMD
fi

printf 'DONE!\n\n'


