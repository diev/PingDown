// Service.h
//

#pragma once

#pragma comment(lib, "advapi32.lib")

#define SVCNAME "PingDown"
#define LOGFILE "PingDown.log"

BOOL WINAPI GetSCManager(int);

VOID WINAPI SvcInstall(void);
VOID WINAPI SvcUninstall(void);

VOID WINAPI SvcCtrlHandler(DWORD);
VOID WINAPI SvcMain(DWORD, LPTSTR *);

VOID WINAPI ReportSvcStatus(DWORD, DWORD, DWORD);
VOID WINAPI SvcInit(DWORD, LPTSTR *);

VOID WINAPI SvcReportEvent(LPTSTR);
VOID WINAPI SvcReportLog(char *);
