// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#pragma once



namespace TelegramClient_Opus
{
	//public ref class TestClass sealed{
	//public:
	//	//property int IntValue;
	//	//property String^ StringValue;
	//};

    public ref class WindowsPhoneRuntimeComponent sealed
    {

	public:
		//property int IntField;
		//property TestClass ClassField;
		//TestClass ClassField;
        WindowsPhoneRuntimeComponent();
		int Sum(int a, int b);
		int InitPlayer(Platform::String^ path);
		void CleanupPlayer();
		void FillBuffer(Platform::WriteOnlyArray<uint8>^ buffer, int capacity, Platform::WriteOnlyArray<int>^ args);
		int64 GetTotalPcmDuration();

		int StartRecord(Platform::String^ path);
		int WriteFrame(const Platform::Array<uint8>^ buffer, int length);
		void StopRecord();

		bool IsOpusFile(Platform::String^ path);

		Platform::Array<uint8>^ GetWaveform(Platform::String^ path);

		//void WriteFile( String^ strFile, String^ strContent );
		//void LoadFile(String^ strFile);
    };

}