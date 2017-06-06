// Program.c: определяет точку входа для консольного приложения.
//

#include "stdafx.h"
#include <windows.h>
#include <tchar.h>
#include <strsafe.h>
#include <stdio.h>
#include "Service.h"
#include "ServiceControl.h"
#include "Program.h"

//
// Purpose: 
//   Entry point for the process. Executes specified command from user.
//
// Parameters:
//   Command-line syntax is: program [command]
// 
// Return value:
//   None
//
void __cdecl _tmain(int argc, TCHAR *argv[])
{
	//if (argc != 2)
	//{
	//	DisplayUsage(TEXT("Incorrect number of arguments"));
	//	return;
	//}

	SvcReportLog("- Main -");

	TCHAR szCommand[10];
	StringCchCopy(szCommand, 10, argv[1]);

	// If command-line parameter is "install", install the service. 
	// Otherwise, the service is probably being started by the SCM.
	// And so on.

	if (lstrcmpi(szCommand, TEXT("install")) == 0)
	{
		SvcReportLog("Install");
		SvcInstall();
		return;
	}

	if (lstrcmpi(szCommand, TEXT("uninstall")) == 0)
	{
		SvcReportLog("Uninstall");
		SvcUninstall();
		return;
	}
	
	if (lstrcmpi(szCommand, TEXT("start")) == 0)
	{
		SvcReportLog("Start");
		SvcStart();
		return;
	}
	
	if (lstrcmpi(szCommand, TEXT("stop")) == 0)
	{
		SvcReportLog("Stop");
		SvcStop();
		return;
	}
	
	if (lstrcmpi(szCommand, TEXT("query")) == 0)
	{
		SvcReportLog("Query");
		SvcQuery();
		return;
	}
	
	if (lstrcmpi(szCommand, TEXT("help")) == 0 ||
		lstrcmpi(szCommand, TEXT("/?")) == 0 ||
		lstrcmpi(szCommand, TEXT("-?")) == 0)
	{
		SvcReportLog("Usage");
		DisplayUsage(NULL);
		return;
	}
	
	if (argc > 1)
	{
		TCHAR Buffer[80];
		StringCchPrintf(Buffer, 80, TEXT("Unknown command '%s'"), szCommand);
		DisplayUsage(Buffer);
		return;
	}

	// TO_DO: Add any additional services for the process to this table.
	SERVICE_TABLE_ENTRY DispatchTable[] =
	{
		{ TEXT(SVCNAME), (LPSERVICE_MAIN_FUNCTION)SvcMain },
		{ NULL, NULL }
	};

	// This call returns when the service has stopped. 
	// The process should simply terminate when the call returns.

	SvcReportLog("DispatchTable");
	if (!StartServiceCtrlDispatcher(DispatchTable))
	{
		//SvcReportEvent(TEXT("StartServiceCtrlDispatcher"));
		SvcReportLog("StartServiceCtrlDispatcher failed");
	}

	SvcReportLog("Exit");
}

//
// Purpose: 
//   Displays the usage help screen.
//
// Parameters:
//   None
// 
// Return value:
//   None
//
VOID WINAPI DisplayUsage(TCHAR *szError)
{
	printf("Description:\n");
	printf("\tCommand-line tool to configure %s service.\n\n", SVCNAME);

	if (szError != NULL)
	{
		_tprintf(TEXT("ERROR:\t%s\n\n"), szError);
	}

	printf("Usage and list of commands:\n");
	printf("\t%s command\n\n", SVCNAME);
	printf("\t  help | -? | /?\n");
	printf("\t  query\n");
	printf("\t  install\n");
	printf("\t  start\n");
	printf("\t  stop\n");
	printf("\t  uninstall\n");
}
