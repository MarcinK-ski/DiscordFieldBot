#!/bin/bash

#MAIN VARs
MAIN_DIR="/home/user"
PROJECT_NAME="DiscordBot"

#WATCH FILES
WATCH_FILE="${MAIN_DIR}/DFBWatch"
CURRENT_TIME_FILE="${MAIN_DIR}/.DFBWatchCurrentTime"

#PROJECT VARS
PROJECT_DIR="${MAIN_DIR}/${PROJECT_NAME}/${PROJECT_NAME}/"
PROJECT_FILE="${PROJECT_NAME}.csproj"
PROJECT_DLL="${PROJECT_NAME}.dll"

APP_LOGS_LOCATION="${MAIN_DIR}/${PROJECT_NAME}/${PROJECT_NAME}.log"

#TMUX VARs
TMUX_NAME="DFBDotnet"
TMUX_CMD="dotnet run --project $PROJECT_DIR$PROJECT_FILE > $APP_LOGS_LOCATION"

#MISC VARs
WAIT_TIME=140
SHOULD_APP_BE_KILLED=false
DT=$(date '+%d/%m/%Y %H:%M:%S');
#------------------------------

KillInsideScreenProcess()
{
	echo "First kill"
        kill $(ps aux | grep 'dotnet exec' | grep $PROJECT_DLL | awk '{print $2}')
	sleep 1
	echo "Second kill (redundantly)"
        kill $(ps aux | grep 'dotnet run' | grep $PROJECT_FILE | awk '{print $2}')  #REDUNDANT!!!!
        printf "Killed dotnet process.\nNow - waiting..."
        sleep 10
}

#------------------------------

printf "SCRIPT START $DT !\n"

echo "Checking tmux list-sessions"
TMUX_NAME_PIDs=`tmux list-sessions | grep $TMUX_NAME | cut -d: -f1 | awk '{print $1}'`

touch $CURRENT_TIME_FILE  #File to compare timestamps: this with watchdog file from my app

if [ ! -z "$TMUX_NAME_PIDs" ]
then
        SHOULD_APP_BE_KILLED=true
        echo "Screen > $TMUX_NAME < is exists"
        echo "CTFile touched. Waiting -> $WAIT_TIME sec <-"
        sleep $WAIT_TIME  #Wait, because watchdog file is refresh in time interval
else
        echo 'CREATING new tmux!'
        /usr/bin/tmux new-session -d -s $TMUX_NAME
fi


if [ $CURRENT_TIME_FILE -nt $WATCH_FILE ]
then
        if [ "$SHOULD_APP_BE_KILLED" = true ]
		then
			KillInsideScreenProcess
		fi
		
        echo "Runing process inside > $TMUX_NAME < tmux"
        /usr/bin/tmux  send -t  $TMUX_NAME "$TMUX_CMD" ENTER
else
        echo 'Watch is newer'
fi

printf 'DONE!\n\n'

