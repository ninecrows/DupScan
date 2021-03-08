#pragma once
extern "C"
{
	_declspec(dllexport)
	BOOL GetVolumePathNamesForVolumeNameZX(
		LPCWSTR lpszVolumeName,
		LPWCH   lpszVolumePathNames,
		DWORD   cchBufferLength,
		PDWORD  lpcchReturnLength
	);
}