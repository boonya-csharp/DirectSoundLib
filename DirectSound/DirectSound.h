#include "stdafx.h"
#include <dsound.h>

__declspec(dllexport) int __stdcall TestInvoke(LPCDSCBUFFERDESC pcDSCBufferDesc, LPDIRECTSOUNDCAPTUREBUFFER *ppDSCBuffer, LPUNKNOWN pUnkOuter);

__declspec(dllexport) int __stdcall TestInvoke2(LPDIRECTSOUNDCAPTURE8 pdsc8, LPCDSCBUFFERDESC pcDSCBufferDesc, LPDIRECTSOUNDCAPTUREBUFFER *ppDSCBuffer, LPUNKNOWN pUnkOuter);

__declspec(dllexport) int __stdcall TestInvoke3(LPDIRECTSOUNDCAPTUREBUFFER8 buffer);