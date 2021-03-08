#include "pch.h"
#include <iostream>
#include <list>
#include <string>

void VolumePathNames(
    const std::wstring &volume,
    std::list<std::wstring> &names
    )
{
    wchar_t buffer[4096];
    DWORD resultLength(0);
    BOOL ok(GetVolumePathNamesForVolumeNameW(volume.c_str(),
        buffer, sizeof(buffer) / sizeof(*buffer),
        &resultLength));

    if (!ok)
    {
        std::wcout << L"Done " << (ok ? L"Ok" : L"Failed") << L"\n";
    }
	
    wchar_t* beginsAt = buffer;
    std::wstring current(buffer);
    if (current.size() > 0)
    {
        //std::wcout << L"\t[" << current << L"]\n";
        names.push_back(current);
    }
	for (size_t ct = 0; ct < resultLength; ct++)
	{
		if (buffer[ct] == '\0' && ct < resultLength - 1)
		{
            std::wstring current(buffer + ct + 1);
            if (current.size() > 0)
            {
                //std::wcout << L"\t[" << current << L"]\n";
                names.push_back(current);
            }
		}
	}
}

int main()
{
    std::cout << "Hello World!\n";

	{
		wchar_t buffer[2048];
		HANDLE handle(FindFirstVolumeW(buffer, sizeof(buffer) / sizeof(*buffer)));
    	if (handle != INVALID_HANDLE_VALUE)
    	{
            std::wcout << L"[" << std::wstring(buffer) << L"]\n";
            std::list<std::wstring> names;
            VolumePathNames(std::wstring(buffer), names);
            bool result = true;
    		while (result = FindNextVolumeW(handle, buffer, sizeof(buffer) / sizeof(*buffer)))
    		{
                std::wcout << L"[" << std::wstring(buffer) << L"]\n";
                std::list<std::wstring> names;
    			VolumePathNames(std::wstring(buffer), names);

                {
                    UINT type(GetDriveTypeW(buffer));
                    std::wcout << "Drive type: " << type << "\n";
                }

                {
                    wchar_t vn[1024];
                    DWORD vsn(0);
                    DWORD mcl(0);
                    DWORD fsf(0);
                    wchar_t fsnt[1024];
                    BOOL ok(GetVolumeInformationW(buffer,
                        vn, 1024,
                        &vsn,
                        &mcl,
                        &fsf,
                        fsnt, 1024
                    ));
                	if (ok)
                	{
                        std::wcout << "Done ok: \"" << vn << "\" \"" << fsnt << " \"" << std::hex << vsn << std::dec << "\n";
                	}
                }
    			
                {
                    ULARGE_INTEGER avail;
                    ULARGE_INTEGER total;
                    ULARGE_INTEGER freeSpace;
                    BOOL ok(GetDiskFreeSpaceExW(buffer, &avail, &total, &freeSpace));
                	if (ok)
                	{
                        std::wcout << "Read Ok: " << total.QuadPart << " -> " << total.QuadPart / (1024 * 1024 * 1024) << " GB\n";
                	}
                    else
                    {
                        std::wcout << "Failed: " << GetLastError() << "\n";
                    }
                }
    			for (auto name : names)
    			{
                    std::wcout << L"\t[" << name << L"]\n";
    			}
    		}

            bool closeOk = FindVolumeClose(handle);
            handle = INVALID_HANDLE_VALUE;
    	}
	}
}

