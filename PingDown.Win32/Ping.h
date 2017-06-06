// Ping.h
//

#pragma once

#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "ws2_32.lib")

BOOL WINAPI Ping(char *);
