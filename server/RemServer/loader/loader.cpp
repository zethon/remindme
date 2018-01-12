// loader.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "loader.h"
#ifdef _DEBUG
#define new DEBUG_NEW
#endif

static BOOL CALLBACK myfunc(HWND Handle,LPARAM lParam);
DWORD CreateProcessEx ( LPCSTR lpAppPath, LPCSTR lpCmdLine,BOOL bAppInCmdLine, BOOL bCompletePath,BOOL bWaitForProcess, BOOL bMinimizeOnWait, HWND hMainWnd );


// The one and only application object

CWinApp theApp;

using namespace std;

int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	int nRetCode = 0;

	// initialize MFC and print and error on failure
	if (!AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0))
	{
		// TODO: change error code to suit your needs
		_tprintf(_T("Fatal Error: MFC initialization failed\n"));
		nRetCode = 1;
	}
	else
	{
		
		// get the file name of loader
		TCHAR sFileName[MAX_PATH];
		GetModuleFileName(NULL,sFileName,MAX_PATH);
		
		CString strCmdLine = theApp.m_lpCmdLine;
		strCmdLine.Replace(sFileName,"");
		strCmdLine.Replace("\"\"","");

		// launch the server
		PROCESS_INFORMATION pInfo;
		STARTUPINFO sInfo;
		DWORD exitCode;

		sInfo.cb = sizeof(STARTUPINFO);
		sInfo.lpReserved = NULL;
		sInfo.lpReserved2 = NULL;
		sInfo.cbReserved2 = 0;
		sInfo.lpDesktop = NULL;
		sInfo.lpTitle = NULL;
		sInfo.dwFlags = 0;
		sInfo.dwX = 0;
		sInfo.dwY = 0;
		sInfo.dwFillAttribute = 0;
		sInfo.wShowWindow = SW_SHOW;

		BOOL bOK;
		printf("Starting mpserver...\n");
	    
		bOK = CreateProcess(NULL,"perl.exe mpserver.pl",NULL, NULL, FALSE, CREATE_NEW_CONSOLE, NULL, ".\\mpserver", &sInfo, &pInfo);
		if (!bOK)
		{
			printf("Failed to start mpserver...\n");
            return nRetCode;
		}
		printf ("mpserver started...\n");

		printf("Preparing to start server...\n");		
		Sleep(1000*5);

		printf("Starting server...\n");		
		CString strApp = "server.exe "+strCmdLine;
		
		bOK = CreateProcess(NULL,strApp.GetBuffer(),NULL, NULL, FALSE, CREATE_NEW_CONSOLE , NULL, ".", &sInfo, &pInfo);
		if (!bOK)
		{
			printf("Failed to start server...\n");
            return nRetCode;
		}
		printf ("server started...\n");
	}

	printf("Exiting loader...\n");
	return nRetCode;

}
