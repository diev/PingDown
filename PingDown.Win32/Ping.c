// Ping.c: Sends an ICMP echo request to the IP address specified
//

#include "stdafx.h"
#include <winsock2.h>
#include <iphlpapi.h>
#include <icmpapi.h>
#include <stdio.h>
#include "Ping.h"

// https://msdn.microsoft.com/en-us/library/aa366050(VS.85).aspx

//
// Purpose: 
//   Sends an ICMP echo request to the IP address specified
//
// Parameters:
//   IP address
// 
// Return value:
//   True if the information received from the first response
//
BOOL WINAPI Ping(char *ip) 
{
	// Declare and initialize variables

	HANDLE hIcmpFile;
	unsigned long ipaddr = INADDR_NONE;
	DWORD dwRetVal = 0;
	char SendData[32] = "Data Buffer";
	LPVOID ReplyBuffer = NULL;
	DWORD ReplySize = 0;

	ipaddr = inet_addr(ip);
	if (ipaddr == INADDR_NONE) 
	{
		//printf("usage: IP address\n");
		return FALSE;
	}

	hIcmpFile = IcmpCreateFile();
	if (hIcmpFile == INVALID_HANDLE_VALUE) 
	{
		//printf("\tUnable to open handle.\n");
		//printf("IcmpCreatefile returned error: %ld\n", GetLastError());
		return FALSE;
	}

	ReplySize = sizeof(ICMP_ECHO_REPLY) + sizeof(SendData);
	ReplyBuffer = (VOID*)malloc(ReplySize);
	if (ReplyBuffer == NULL) 
	{
		//printf("\tUnable to allocate memory\n");
		return FALSE;
	}

	dwRetVal = IcmpSendEcho(hIcmpFile, ipaddr, SendData, sizeof(SendData),
		NULL, ReplyBuffer, ReplySize, 1000);
	if (dwRetVal != 0) 
	{
		//PICMP_ECHO_REPLY pEchoReply = (PICMP_ECHO_REPLY)ReplyBuffer;
		//struct in_addr ReplyAddr;
		//ReplyAddr.S_un.S_addr = pEchoReply->Address;
		//printf("\tSent icmp message to %s\n", ip);
		//if (dwRetVal > 1) 
		//{
		//	printf("\tReceived %ld icmp message responses\n", dwRetVal);
		//	printf("\tInformation from the first response:\n");
		//}
		//else 
		//{
		//	printf("\tReceived %ld icmp message response\n", dwRetVal);
		//	printf("\tInformation from this response:\n");
		//}
		//printf("\t  Received from %s\n", inet_ntoa(ReplyAddr));
		//printf("\t  Status = %ld\n", pEchoReply->Status);
		//printf("\t  Roundtrip time = %ld milliseconds\n", pEchoReply->RoundTripTime);
	}
	else 
	{
		//printf("\tCall to IcmpSendEcho failed.\n");
		//printf("\tIcmpSendEcho returned error: %ld\n", GetLastError());
		return FALSE;
	}

	return TRUE;
}
