




#include <dsound.h>
#include "DSLibConsts.h"
#include "DSLibUtils.h"
#include "DSLibCapture.h"



LPDIRECTSOUNDCAPTURE8 pdsc8;
LPCDSCBUFFERDESC pdsbd;
LPDIRECTSOUNDCAPTUREBUFFER8 pdscb8;
DSBPOSITIONNOTIFY rgdsbpn[NOTIFY_CNT];
HANDLE threadHandle;
BOOL isRunning = FALSE;



BOOL CreateIDirectSoundCapture8(HRESULT *dsErr)
{
	if (!SUCCEEDED(*dsErr = DirectSoundCaptureCreate8(NULL, &pdsc8, NULL)))
	{
		dslib_utils_printlog("DirectSoundCaptureCreate失败, DSERR = %d", *dsErr);
		return FALSE;
	}

	return TRUE;
}

BOOL CreateCaptureBuffer(HRESULT *dsErr)
{
	DSCBUFFERDESC dsbd;
	WAVEFORMATEX wft;
	LPDIRECTSOUNDCAPTUREBUFFER pdscb;

	#pragma region 初始化缓冲区格式

	wft.wFormatTag = WAVE_FORMAT_PCM;
	wft.nChannels = WFT_CHANNELS;
	wft.nSamplesPerSec = WFT_SAMPLES_PER_SEC;
	wft.wBitsPerSample = WFT_BITS_PER_SAMPLE;
	wft.nBlockAlign = WFT_BLOCKALIGN;
	wft.nAvgBytesPerSec = WFT_BUFFERSIZE;
	wft.cbSize = 0;

	dsbd.dwSize = sizeof(DSCBUFFERDESC);
	dsbd.dwFlags = 0;
	dsbd.dwBufferBytes = wft.nAvgBytesPerSec;
	dsbd.dwReserved = 0;
	dsbd.lpDSCFXDesc = 0;
	dsbd.dwFXCount = 0;
	dsbd.lpwfxFormat = &wft;

	#pragma endregion

	if (!SUCCEEDED(*dsErr = pdsc8->lpVtbl->CreateCaptureBuffer(pdsc8, &dsbd, &pdscb, NULL)))
	{
		dslib_utils_printlog("CreateCaptureBuffer失败, DSERR = %d", *dsErr);
		return FALSE;
	}

	if (!SUCCEEDED(*dsErr = pdscb->lpVtbl->QueryInterface(pdscb, &IID_IDirectSoundCaptureBuffer8, (LPVOID)pdscb8)))
	{
		dslib_utils_printlog("创建IDirectSoundCaptureBuffer8接口失败, DSERR = %d", *dsErr);
		return FALSE;
	}

	pdscb->lpVtbl->Release(pdscb);

	return TRUE;
}

BOOL CreateBufferNotifications(HRESULT *dsErr)
{
	LPDIRECTSOUNDNOTIFY8 pdsNotify8;
	WAVEFORMATEX wfx;

	if (SUCCEEDED(*dsErr = pdscb8->lpVtbl->QueryInterface(pdscb8, &IID_IDirectSoundNotify, (LPVOID)pdsNotify8)))
	{
		if (SUCCEEDED(*dsErr = pdscb8->lpVtbl->GetFormat(pdscb8, &wfx, sizeof(WAVEFORMATEX), NULL)))
		{
			rgdsbpn[0].dwOffset = wfx.nAvgBytesPerSec - 1;
			rgdsbpn[0].hEventNotify = CreateEvent(NULL, TRUE, FALSE, NULL);

			rgdsbpn[1].dwOffset = DSBPN_OFFSETSTOP;
			rgdsbpn[1].hEventNotify = CreateEvent(NULL, TRUE, FALSE, NULL);

			if (!SUCCEEDED(*dsErr = pdsNotify8->lpVtbl->SetNotificationPositions(pdsNotify8, NOTIFY_CNT, rgdsbpn)))
			{
				dslib_utils_printlog("SetNotificationPositions失败, DSERR = %d", *dsErr);
			}
		}
		else
		{
			dslib_utils_printlog("GetFormat失败, DSERR = %d", *dsErr);
		}

		pdsNotify8->lpVtbl->Release(pdsNotify8);
	}
	else
	{
		dslib_utils_printlog("创建IDirectSoundNotify接口失败, DSERR = %d", *dsErr);
		return FALSE;
	}

	return TRUE;
}

void ClearBufferNotifications()
{
	int idx;
	for (idx = 0; idx < NOTIFY_CNT; idx++)
	{
		CloseHandle(rgdsbpn[idx].hEventNotify);
	}
}

DWORD WINAPI CaptureThreadProcess(LPVOID lpParameter)
{
	while (isRunning)
	{
		switch (WaitForMultipleObjects(NOTIFY_CNT, rgdsbpn[0].hEventNotify, FALSE, INFINITE))
		{
		case WAIT_OBJECT_0:
		{
		}
		break;

		case WAIT_OBJECT_0 + 1:
		{
			isRunning = FALSE;
		}
		break;

		case WAIT_FAILED:
		{
			isRunning = FALSE;

			CloseHandle(threadHandle);
		}
		break;
		}
	}

	return 0;
}



DSLIBAPI dslib_capture_init()
{
	HRESULT dsErr;
	if ((CreateIDirectSoundCapture8(&dsErr) &&
		CreateCaptureBuffer(&dsErr) &&
		CreateBufferNotifications(&dsErr)))
	{

	}

	return dsErr;
}

DSLIBAPI dslib_capture_start()
{
	HRESULT dsErr;
	DWORD lastErr;

	if ((dsErr = pdscb8->lpVtbl->Start(pdscb8, DSCBSTART_LOOPING)) != DS_OK)
	{
		dslib_utils_printlog("启动录音失败, DSERR = %d", dsErr);
		return dsErr;
	}

	if (!(threadHandle = CreateThread(NULL, 0, CaptureThreadProcess, NULL, NULL, NULL)))
	{
		lastErr = GetLastError();
		dslib_utils_printlog("创建录音线程失败, LastErrorCode = %d", lastErr);
		return lastErr;
	}

	return DS_OK;
}

DSLIBAPI dslib_capture_stop()
{
	HRESULT dsErr;
	if ((dsErr = pdscb8->lpVtbl->Stop(pdscb8)) != DS_OK)
	{
		dslib_utils_printlog("停止录音失败, DSERR = %d", dsErr);
	}

	CloseHandle(threadHandle);

	return dsErr;
}

DSLIBAPI dslib_capture_release()
{
	ClearBufferNotifications();

	return DS_OK;
}