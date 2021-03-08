// TestFileIdentifier.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include <iostream>
#include <windows.h>
#include <stdio.h>

int main()
{
    std::cout << "Hello World!\n";

	FILE_ID_INFO data;
	const wchar_t* path = L"h:\\index.json";

	HANDLE handle = CreateFileW(path, GENERIC_READ, FILE_SHARE_READ, nullptr, OPEN_EXISTING, 0, nullptr);
	if (handle != INVALID_HANDLE_VALUE)
	{
		BOOL ok = GetFileInformationByHandleEx(handle, FileIdInfo, &data, sizeof(data));
		if (ok)
		{
			DWORD high = (data.VolumeSerialNumber >> 32) & 0xffffffff;
			DWORD low = (data.VolumeSerialNumber) & 0xffffffff;

			printf("VSN: 0x%08lx 0x%08lx\n", high, low);

			unsigned long* ids = ((unsigned long*)data.FileId.Identifier);
			for (size_t ct = 0; ct < 4; ct++)
			{
				printf("%08x ", ids[ct]);
			}
			
			//unsigned long long* ids = ((unsigned long long*)data.FileId.Identifier);
			unsigned long long idhigh = *((unsigned long long *)data.FileId.Identifier);
			unsigned long long idlow = *((unsigned long long*)data.FileId.Identifier + 8);
		}
		else
		{
			fprintf(stderr, "Failed to get ID with %lu\n", GetLastError());
		}
	}
	else
	{
		fprintf(stderr, "Failed with %lu\n", GetLastError());
	}
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
