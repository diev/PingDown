// Service.c: Windows Service code
//

#include "stdafx.h"
#include <windows.h>
#include <tchar.h>
#include <strsafe.h>
#include <share.h>
#include "Messages.h" // make: mc [-U] Messages.mc
#include "Ping.h"
#include "MySystemShutdown.h"
#include "Service.h"

// https://msdn.microsoft.com/en-us/library/bb540475(v=vs.85).aspx

SERVICE_STATUS          gSvcStatus;
SERVICE_STATUS_HANDLE   gSvcStatusHandle;
//HANDLE                  ghSvcStopEvent = NULL;

//
// Purpose: 
//   Entry point for the service
//
// Parameters:
//   dwArgc - Number of arguments in the lpszArgv array
//   lpszArgv - Array of strings. The first string is the name of
//     the service and subsequent strings are passed by the process
//     that called the StartService function to start the service.
// 
// Return value:
//   None.
//
VOID WINAPI SvcMain(DWORD dwArgc, LPTSTR *lpszArgv)
{
	SvcReportLog("SvcMain");

	// Register the handler function for the service

	gSvcStatusHandle = RegisterServiceCtrlHandler(
		TEXT(SVCNAME),
		SvcCtrlHandler);

	if (!gSvcStatusHandle)
	{
		//SvcReportEvent(TEXT("RegisterServiceCtrlHandler"));
		SvcReportLog("RegisterServiceCtrlHandler failed");
		return;
	}

	// These SERVICE_STATUS members remain as set here

	gSvcStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
	gSvcStatus.dwServiceSpecificExitCode = 0;

	// Report initial status to the SCM

	ReportSvcStatus(SERVICE_START_PENDING, NO_ERROR, 3000);
	SvcReportLog("Service start pending");

	// Perform service-specific initialization and work.

//	SvcInit(dwArgc, lpszArgv);
//}
//
////
//// Purpose: 
////   The service code
////
//// Parameters:
////   dwArgc - Number of arguments in the lpszArgv array
////   lpszArgv - Array of strings. The first string is the name of
////     the service and subsequent strings are passed by the process
////     that called the StartService function to start the service.
//// 
//// Return value:
////   None
////
//VOID WINAPI SvcInit(DWORD dwArgc, LPTSTR *lpszArgv)
//{
	SvcReportLog("SvcInit");

	// TO_DO: Declare and set any required variables.
	//   Be sure to periodically call ReportSvcStatus() with 
	//   SERVICE_START_PENDING. If initialization fails, call
	//   ReportSvcStatus with SERVICE_STOPPED.

	ReportSvcStatus(SERVICE_START_PENDING, NO_ERROR, 3000);
	SvcReportLog("Service start pending");

	// Create an event. The control handler function, SvcCtrlHandler,
	// signals this event when it receives the stop control code.

	//ghSvcStopEvent = CreateEvent(
	//	NULL,    // default security attributes
	//	TRUE,    // manual reset event
	//	FALSE,   // not signaled
	//	NULL);   // no name

	//if (ghSvcStopEvent == NULL)
	//{
	//	ReportSvcStatus(SERVICE_STOPPED, NO_ERROR, 0);
	//	SvcReportLog("Service stopped");
	//	return;
	//}

	// Report running status when initialization is complete.

	ReportSvcStatus(SERVICE_RUNNING, NO_ERROR, 0);
	SvcReportLog("Service running");

	// TO_DO: Perform work until service stops.

	while (gSvcStatus.dwCurrentState == SERVICE_RUNNING)
	{
		// Check whether to stop the service.

		SvcReportLog("Service works...");
		Sleep(5000);
		SvcReportLog("Service works.....");
		Sleep(5000);

		//WaitForSingleObject(ghSvcStopEvent, INFINITE);

		if (TRUE)
		{
			SvcReportLog("Service fireworks!");
			ReportSvcStatus(SERVICE_STOPPED, NO_ERROR, 0);
			SvcReportLog("Service stopped");
			return;
		}
	}
}

//
// Purpose: 
//   Sets the current service status and reports it to the SCM.
//
// Parameters:
//   dwCurrentState - The current state (see SERVICE_STATUS)
//   dwWin32ExitCode - The system error code
//   dwWaitHint - Estimated time for pending operation, 
//     in milliseconds
// 
// Return value:
//   None
//
VOID WINAPI ReportSvcStatus(DWORD dwCurrentState,
	DWORD dwWin32ExitCode,
	DWORD dwWaitHint)
{
	static DWORD dwCheckPoint = 1;

	// Fill in the SERVICE_STATUS structure.

	gSvcStatus.dwCurrentState = dwCurrentState;
	gSvcStatus.dwWin32ExitCode = dwWin32ExitCode;
	gSvcStatus.dwWaitHint = dwWaitHint;

	if (dwCurrentState == SERVICE_START_PENDING)
	{
		gSvcStatus.dwControlsAccepted = 0;
	}
	else
	{
		gSvcStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP;
	}

	if ((dwCurrentState == SERVICE_RUNNING) ||
		(dwCurrentState == SERVICE_STOPPED))
	{
		gSvcStatus.dwCheckPoint = 0;
	}
	else
	{
		gSvcStatus.dwCheckPoint = dwCheckPoint++;
	}

	// Report the status of the service to the SCM.
	SetServiceStatus(gSvcStatusHandle, &gSvcStatus);
}

//
// Purpose: 
//   Called by SCM whenever a control code is sent to the service
//   using the ControlService function.
//
// Parameters:
//   dwCtrl - control code
// 
// Return value:
//   None
//
VOID WINAPI SvcCtrlHandler(DWORD dwCtrl)
{
	// Handle the requested control code. 

	switch (dwCtrl)
	{
	case SERVICE_CONTROL_STOP:
		ReportSvcStatus(SERVICE_STOP_PENDING, NO_ERROR, 0);
		SvcReportLog("Service stop pending");

		// Signal the service to stop.

		//SetEvent(ghSvcStopEvent);
		ReportSvcStatus(gSvcStatus.dwCurrentState, NO_ERROR, 0);

		return;

	case SERVICE_CONTROL_INTERROGATE:
		break;

	default:
		break;
	}
}

//
// Purpose: 
//   Logs messages to the event log
//
// Parameters:
//   szFunction - name of function that failed
// 
// Return value:
//   None
//
// Remarks:
//   The service must have an entry in the Application event log.
//
VOID WINAPI SvcReportEvent(LPTSTR szFunction)
{
	HANDLE hEventSource;
	LPCTSTR lpszStrings[2];
	TCHAR Buffer[80];

	hEventSource = RegisterEventSource(NULL, TEXT(SVCNAME));

	if (NULL != hEventSource)
	{
		StringCchPrintf(Buffer, 80, TEXT("%s failed with %d"), szFunction, GetLastError());

		lpszStrings[0] = TEXT(SVCNAME);
		lpszStrings[1] = Buffer;

		ReportEvent(
			hEventSource,        // event log handle
			EVENTLOG_ERROR_TYPE, // event type
			0,                   // event category
			// https://msdn.microsoft.com/en-us/library/bb540472(v=vs.85).aspx
			SVC_ERROR,           // event identifier
			NULL,                // no security identifier
			2,                   // size of lpszStrings array
			0,                   // no binary data
			lpszStrings,         // array of strings
			NULL);               // no binary data

		DeregisterEventSource(hEventSource);
	}
}

//
// Purpose: 
//   Logs messages to the file log
//
// Parameters:
//   szMessage - text to output
// 
// Return value:
//   None
//
VOID WINAPI SvcReportLog(char *szMessage)
{
	FILE *log = _fsopen(LOGFILE, "a", _SH_DENYNO); // open for append shared read/write
	if (log == NULL)
	{
		return;
	}
	SYSTEMTIME stNow;
	GetLocalTime(&stNow);
	fprintf(log, "%d-%02d-%02d %2d:%02d:%02d %s\n", 
		stNow.wYear, stNow.wMonth, stNow.wDay, 
		stNow.wHour, stNow.wMinute, stNow.wSecond, 
		szMessage);
	fclose(log);
}
