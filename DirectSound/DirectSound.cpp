
#include "stdafx.h"
#include <stdio.h>
#include <dsound.h>
#include <Windows.h>
#include <WinBase.h>

LPDIRECTSOUNDCAPTURE8 directSoundCapture8;
LPDIRECTSOUNDCAPTUREBUFFER8 directSoundCaptureBuffer8;
LPDIRECTSOUNDCAPTUREBUFFER *ppDSCBuffer;
HANDLE notifyHwnd;
#define CODE_DSCAPTURE_FAILED 1
#define CODE_DSCAPTURE_SUCCESS 2
bool isCapturing;


int CreateCaptureDevice()
{
	/*
	* lpcGUID: 声音捕获设备的ID, NULL是默认设备, DSDEVID_DefaultCapture系统默认音频捕获设备（和NULL似乎一样）, DSDEVID_DefaultVoiceCapture默认语音捕获设备（比如戴在头上的麦克风设备）
	* ppDSC8 : IDirectSoundCapture8接口指针
	*/
	HRESULT result = ::DirectSoundCaptureCreate8(NULL, &directSoundCapture8, NULL);
	if (result != DS_OK)
	{
		/*
		* 失败, 返回值可能是:
		* DSERR_ALLOCATED
		* DSERR_INVALIDPARAM
		* DSERR_NOAGGREGATION
		* DSERR_OUTOFMEMORY
		* 如果声卡不支持full duplex, 那么返回DSERR_ALLOCATED
		*/
		return CODE_DSCAPTURE_FAILED;
	}

	// 成功
	return CODE_DSCAPTURE_SUCCESS;
}

/*
采样率是指每秒采样多少次
采样位数是指每次采样的位数,常见有8位或是16位, 采样位数又称量化位数
通道常见单声道音频，立体声道,若是单声道,那么每次采样一个通道,若是立体声道,那么每次采样两个不同声道
比特率是每秒采样多少bit
比特率 = 采样率 * 采样位数 * 通道数, 换算成字节再除以8
*/
int CreateCaptureBuffer()
{
	// 要录制的波形声音格式描述
	WAVEFORMATEX waveFormat;
	waveFormat.wFormatTag = WAVE_FORMAT_PCM;
	// 通道数量
	waveFormat.nChannels = 2;
	// 采样率, 每秒采样次数
	waveFormat.nSamplesPerSec = 44100;
	/*
	* 采样位数, 每个采样的位数
	* 如果wFormatTag是WAVE_FORMAT_PCM, 必须设置为8或者16, 其他的不支持
	* 如果wFormatTag是WAVE_FORMAT_EXTENSIBLE, 必须设置为8的倍数, 一些压缩方法不定义此值, 所以此值可以为0
	*/
	waveFormat.wBitsPerSample = 16;
	// 以字节为单位设置块对齐。块对齐是指最小数据的原子大小，如果wFormatTag = WAVE_FORMAT_PCM, nBlockAlign为(nChannels * wBitsPerSample) / 8, 对于非PCM格式请根据厂商的说明计算
	waveFormat.nBlockAlign = waveFormat.nChannels * waveFormat.wBitsPerSample / 8;
	// 设置声音数据的传输速率, 每秒平均传输的字节数, 单位byte/s, 如果wFormatTag = WAVE_FORMAT_PCM, nAvgBytesPerSec为nBlockAlign * nSamplesPerSec, 对于非PCM格式请根据厂商的说明计算
	waveFormat.nAvgBytesPerSec = waveFormat.nBlockAlign * waveFormat.nSamplesPerSec;
	// 额外信息的大小，以字节为单位，额外信息添加在WAVEFORMATEX结构的结尾。这个信息可以作为非PCM格式的wFormatTag额外属性，如果wFormatTag不需要额外的信息，此值必需为0，对于PCM格式此值被忽略。
	waveFormat.cbSize = 0;

	// 录音缓冲区的描述
	DSCBUFFERDESC captureBufferDesc;
	captureBufferDesc.dwSize = sizeof(DSCBUFFERDESC);
	/*
	* 指定设备能力, 可以为0,
	* DSCBCAPS_CTRLFX:（支持效果的Buffer）
	*     只支持从DirectSoundCaptureCreate8函数创建的设备对象, 需要WindowsXP版本（Capture effects require Microsoft Windows XP）
	* DSCBCAPS_WAVEMAPPED（The Win32 wave mapper will be used for formats not supported by the device.）
	*/
	captureBufferDesc.dwFlags = 0;
	// 捕获缓冲区大小, 字节为单位
	captureBufferDesc.dwBufferBytes = waveFormat.nAvgBytesPerSec; // 缓冲区大小设置为传输速率, 那么每一个缓冲区就存储了一秒钟的声音数据
																  // 保留字段, 供以后使用
	captureBufferDesc.dwReserved = 0;
	// 要捕获的波形声音的格式信息
	captureBufferDesc.lpwfxFormat = &waveFormat;
	// 一定为0, 除非dwFlag字段设置了DSCBCAPS_CTRLFX标志
	captureBufferDesc.dwFXCount = 0;
	captureBufferDesc.lpDSCFXDesc = NULL;

	/*
	* CreateCaptureBuffer函数:
	* pcDSCBufferDesc: DSCBUFFERDESC（缓冲区描述）的地址
	* ppDSCBuffer:接收IDirectSoundCaptureBuffer接口的地址, 使用QueryInterface创建IDirectSoundCaptureBuffer8
	* pUnkOuter:为NULL
	*/
	LPDIRECTSOUNDCAPTUREBUFFER directSoundCaptuerBuffer;
	HRESULT result = directSoundCapture8->CreateCaptureBuffer(&captureBufferDesc, &directSoundCaptuerBuffer, NULL);
	if (result == DS_OK)
	{
		directSoundCaptuerBuffer->QueryInterface(IID_IDirectSoundCaptureBuffer8, (LPVOID*)&directSoundCaptureBuffer8); // 使用IDirectSoundCaptureBuffer的QueryInterface创建IDirectSoundCaptureBuffer8接口
		directSoundCaptuerBuffer->Release();
		return CODE_DSCAPTURE_SUCCESS;
	}
	else
	{
		/*
		DSERR_INVALIDPARAM
		DSERR_BADFORMAT
		DSERR_GENERIC
		DSERR_NODRIVER
		DSERR_OUTOFMEMORY
		DSERR_UNINITIALIZED
		*/
		return CODE_DSCAPTURE_FAILED;
	}
}

int SetCaptureNotifications()
{
	// 在directSoundCaptureBuffer8对象里创建一个录音通知对象
	LPDIRECTSOUNDNOTIFY8 directSoundNotify8;
	HRESULT result = directSoundCaptureBuffer8->QueryInterface(IID_IDirectSoundNotify, (LPVOID*)&directSoundNotify8);
	if (result != DS_OK)
	{
		return CODE_DSCAPTURE_FAILED;
	}

	// 获取波形声音的信息
	WAVEFORMATEX waveFormat;
	result = directSoundCaptureBuffer8->GetFormat(&waveFormat, sizeof(WAVEFORMATEX), NULL);
	if (result != DS_OK)
	{
		// 获取录音格式信息失败
		return CODE_DSCAPTURE_FAILED;
	}

	// 创建录音通知位置, 当录音到了指定的缓冲区位置的时候, 会触发通知事件, 在通知事件回调里可以读取缓冲区中的音频数据
	notifyHwnd = CreateEvent(NULL, 1, 0, NULL);
	if (notifyHwnd == NULL)
	{
		// 创建通知对象失败
		return CODE_DSCAPTURE_FAILED;
	}
	// 创建DirectSound使用的通知对象
	DSBPOSITIONNOTIFY dsPositionNotify[1];
	// 设置缓冲区中的通知位置
	dsPositionNotify[0].dwOffset = waveFormat.nAvgBytesPerSec / 2; // 设置通知位置为每秒传输速率, 意思就是当录完一秒钟的音频数据之后就会触发通知事件, 如果直接设置为nAvgBytesPerSec的话, 会报个错
	dsPositionNotify[0].hEventNotify = notifyHwnd;

	/*
	* 设置通知对象
	* dwPositionNotifies:DSBPOSITIONNOTIFY结构体的数量
	* pcPositionNotifies:DSBPOSITIONNOTIFY数组的指针, 数组最大大小为DSBNOTIFICATIONS_MAX
	*/
	result = directSoundNotify8->SetNotificationPositions(1, dsPositionNotify);
	directSoundNotify8->Release();
	if (result != DS_OK)
	{
		// 设置通知位置失败
		return CODE_DSCAPTURE_FAILED;
	}

	return CODE_DSCAPTURE_SUCCESS;
}

int StartCapture()
{
	/*
	* 开始录音
	* dwFlags:指定录音缓冲区的动作, DSCBSTART_LOOPING表示当录音缓冲区满了的时候, 会从头开始继续录音直到调用Stop函数结束录音
	* 如果使用多线程录音, 在捕获缓冲区的时候, 调用Start函数的线程必须存在
	*/
	HRESULT result = directSoundCaptureBuffer8->Start(DSCBSTART_LOOPING);
	if (result != DS_OK)
	{
		// DSERR_INVALIDPARAM, DSERR_NODRIVER, DSERR_OUTOFMEMORY
		return CODE_DSCAPTURE_FAILED;
	}

	return CODE_DSCAPTURE_SUCCESS;
}

int StopCapture()
{
	isCapturing = false;
	directSoundCaptureBuffer8->Stop();

	return CODE_DSCAPTURE_SUCCESS;
}

int Dispose()
{
	if (isCapturing)
	{
		::StopCapture();
	}
	directSoundCaptureBuffer8->Release();
	directSoundCapture8->Release();
	CloseHandle(notifyHwnd);

	return CODE_DSCAPTURE_SUCCESS;
}


int DSCaptureInitialize()
{
	int resultCode = CODE_DSCAPTURE_SUCCESS;

	resultCode = CreateCaptureDevice();
	if (resultCode != CODE_DSCAPTURE_SUCCESS)
	{
		return resultCode;
	}

	resultCode = CreateCaptureBuffer();
	if (resultCode != CODE_DSCAPTURE_SUCCESS)
	{
		return resultCode;
	}

	resultCode = SetCaptureNotifications();
	if (resultCode != CODE_DSCAPTURE_SUCCESS)
	{
		return resultCode;
	}

	resultCode = StartCapture();
	if (resultCode != CODE_DSCAPTURE_SUCCESS)
	{
		return resultCode;
	}

	return CODE_DSCAPTURE_SUCCESS;
}

void DSCaptureStart()
{
	HRESULT result = DS_OK;

	isCapturing = true;

	int lockOffset = 0;

	while (isCapturing)
	{
		// 等待DirectSound的录音通知事件
		WaitForSingleObject(notifyHwnd, 3000);

		/*
		* 获取当前捕获位置和读取位置, 确保不会读取捕获使用的缓冲区位置, 如果不需要第一个参数, 可以设置为NULL
		* pdwCapturePosition:从硬件复制出来的数据结束位置
		* pdwReadPosition:已经完全捕获到缓冲区的音频数据结束位置
		*/
		DWORD capturePosition, readPosition;
		result = directSoundCaptureBuffer8->GetCurrentPosition(&capturePosition, &readPosition);
		if (result != DS_OK)
		{
			// DSERR_INVALIDPARAM, DSERR_NODRIVER, DSERR_OUTOFMEMORY  
			break;
		}

		if (readPosition == 0)
		{
			continue;
		}

		// 锁定缓冲区并读取音频数据
		LPVOID audioPtr1, audioPtr2;
		DWORD  captureLength1 = 0, captureLength2 = 0;

		result = directSoundCaptureBuffer8->Lock(lockOffset, readPosition, &audioPtr1, &captureLength1, &audioPtr2, &captureLength2, NULL);
		if (result != DS_OK)
		{
			// DSERR_INVALIDPARAM, DSERR_INVALIDCALL
			break;
		}

		lockOffset = readPosition;

		//printf("dataLength1 = %i, dataLength2 = %i\n", captureLength1, captureLength2);

		// 如果读取到的大小小于预定义的缓冲大小, 那么说明缓冲区已经读完了, 这时audioPtr2会存储audioPtr1未读取完的数据
		if (captureLength1 < readPosition)
		{
		}

		result = directSoundCaptureBuffer8->Unlock(audioPtr1, captureLength1, audioPtr2, captureLength2);
		if (result != DS_OK)
		{
			//DSERR_INVALIDPARAM, DSERR_INVALIDCALL  
			break;
		}

		// 继续等待通知事件
		ResetEvent(notifyHwnd);
	}
}

void DSCaptureStop()
{
	::StopCapture();
}




#ifdef __cplusplus
extern "C"
#endif
{
	__declspec(dllexport) int __stdcall TestInvoke(LPCDSCBUFFERDESC pcDSCBufferDesc, LPDIRECTSOUNDCAPTUREBUFFER *ppDSCBuffer, LPUNKNOWN pUnkOuter)
	{
		//HRESULT result = DirectSoundCaptureCreate8(NULL, &directSoundCapture8, NULL);

		//result = directSoundCapture8->CreateCaptureBuffer(pcDSCBufferDesc, ppDSCBuffer, pUnkOuter);

		return 0;
	}

	__declspec(dllexport) int __stdcall TestInvoke2(LPDIRECTSOUNDCAPTURE8 pdsc8, LPCDSCBUFFERDESC pcDSCBufferDesc, LPDIRECTSOUNDCAPTUREBUFFER *ppDSCBuffer, LPUNKNOWN pUnkOuter)
	{
		HRESULT re = pdsc8->CreateCaptureBuffer(pcDSCBufferDesc, ppDSCBuffer, pUnkOuter);

		return 0;
	}

	__declspec(dllexport) int __stdcall TestInvoke3(LPDIRECTSOUNDCAPTUREBUFFER8 buffer)
	{
		//::DSCaptureInitialize();

		//::DSCaptureStart();

		DWORD cursorPos, readPos;
		HRESULT result = buffer->GetCurrentPosition(&cursorPos, &readPos);

		return 0;
	}
}


