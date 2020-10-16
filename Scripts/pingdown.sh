#!/bin/bash
IP='10.0.2.1'

declare -i n=0
while ! ping -c1 -w2 $IP &> /dev/null ;
do
  echo -e "DOWN!" \\a
  n=n+1
  if test $n -ge 10; then
    echo "shutdown now"
    shutdown --poweroff now
  fi
  sleep 10
done
#echo "ok"
