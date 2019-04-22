// NativeFrame.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <windows.h>

struct MyData
{
	BYTE FileId[128 / 8];
	unsigned long long VolumeId;
};

extern "C"
DWORD RunThis(const wchar_t* MyPath,
	MyData* resultHere);

extern "C"
DWORD FakeFileId(const wchar_t* where,
	MyData* result);

int main()
{
    std::cout << "Hello World!\n"; 

	HMODULE module = LoadLibraryW(L"TestTarget.dll");

	auto process = GetProcAddress(module, "RunThis");
	
	typedef DWORD(RunMe)(const wchar_t* MyPath,
		MyData * resultHere);

	RunMe* here = (RunMe*)process;

	MyData d;
	DWORD result = here(L"c:\\Temp\\Foo.json", &d);

	auto fid = (RunMe*)GetProcAddress(module, "FakeFileId");
	result = fid(L"c:\\Temp\\Foo.json", &d);
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
