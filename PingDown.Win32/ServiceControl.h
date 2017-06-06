// ServiceControl.h
//

#pragma once

VOID WINAPI SvcQuery(void);

VOID WINAPI SvcInstall(void);
VOID WINAPI SvcUninstall(void);

VOID WINAPI SvcStart(void);
VOID WINAPI SvcStop(void);

BOOL WINAPI SvcStopDependentServices(void);
