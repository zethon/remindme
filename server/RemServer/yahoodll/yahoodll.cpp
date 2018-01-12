// yahoodll.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "libyahoo2.h"


BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
    return TRUE;
}

void __declspec(dllexport)__stdcall GetPasswordEncrypt(const char *sn, const char *pw, const char *seed, char *res6, char *res96)
{
	yahoo_process_auth_0x0b(seed,sn,pw,res6,res96);
}
