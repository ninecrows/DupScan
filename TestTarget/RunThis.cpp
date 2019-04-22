#include "pch.h"
#include "RunThis.h"

extern "C"
{
	struct MyData
	{
		BYTE FileId[128 / 8];
		unsigned long long VolumeId;
	};

	__declspec(dllexport)
		DWORD FakeFileId(const wchar_t* where,
			MyData* result)
	{
		DWORD status = ERROR_SUCCESS;

		if (where == nullptr) 
		{
			status = E_ACCESSDENIED;
		}
		else
		{
			if (result != nullptr)
			{
				for (int ct = 0; ct < sizeof(result->FileId); ct++)
				{
					result->FileId[ct] = (BYTE)(ct * 2);
				}
				result->VolumeId = 0xddcb0134;
			}
		}

		return (status);
	}

	__declspec(dllexport) 
		DWORD RunThis(const wchar_t *MyPath,
			MyData* resultHere)
	{
		HANDLE fileHandle = CreateFileW(MyPath,
			GENERIC_READ,
			FILE_SHARE_READ | FILE_SHARE_WRITE,
			nullptr,
			OPEN_EXISTING,
			FILE_ATTRIBUTE_NORMAL,
			nullptr);

		if (fileHandle != INVALID_HANDLE_VALUE)
		{
			{
				FILE_ID_INFO data;
				memset(&data, 0, sizeof(data));
				DWORD length = sizeof(data);
				BOOL result = GetFileInformationByHandleEx(fileHandle,
					FileIdInfo,
					&data,
					length);

				if (result)
				{
					resultHere->VolumeId = data.VolumeSerialNumber;
					memcpy(&(resultHere->FileId), &(data.FileId), sizeof(data.FileId));
				}
				else
				{
					return GetLastError();
				}
			}

			{
				BOOL result = CloseHandle(fileHandle);
				if (!result)
				{
					return (GetLastError());
				}
			}
		}
		else
		{
			return (GetLastError());
		}

		return (ERROR_SUCCESS);
	}

	_declspec(dllexport)
		DWORD GetFileInformation(const wchar_t* path,
			BY_HANDLE_FILE_INFORMATION *results)
	{
		DWORD status = ERROR_SUCCESS;

		if (results == nullptr || path == nullptr)
		{
			return ERROR_INVALID_PARAMETER;
		}

		HANDLE handle = CreateFileW(path,
			GENERIC_READ,
			FILE_SHARE_READ | FILE_SHARE_WRITE,
			nullptr,
			OPEN_EXISTING,
			FILE_ATTRIBUTE_NORMAL,
			nullptr);

		if (handle != INVALID_HANDLE_VALUE)
		{
			BOOL ok = GetFileInformationByHandle(handle, results);
			if (!ok)
			{
				status = GetLastError();
			}

			return (status);
		}
		else
		{
			return GetLastError();
		}

		return (status);
	}
}