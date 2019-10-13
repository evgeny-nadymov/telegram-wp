

/* this ALWAYS GENERATED file contains the IIDs and CLSIDs */

/* link this file in with the server and any clients */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Tue Jan 29 08:48:50 2019
 */
/* Compiler settings for C:\Users\evgeny\AppData\Local\Temp\PhoneVoIPApp.BackEnd.idl-35395493:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=ARM 8.00.0603 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


#ifdef __cplusplus
extern "C"{
#endif 


#include <rpc.h>
#include <rpcndr.h>

#ifdef _MIDL_USE_GUIDDEF_

#ifndef INITGUID
#define INITGUID
#include <guiddef.h>
#undef INITGUID
#else
#include <guiddef.h>
#endif

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        DEFINE_GUID(name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8)

#else // !_MIDL_USE_GUIDDEF_

#ifndef __IID_DEFINED__
#define __IID_DEFINED__

typedef struct _IID
{
    unsigned long x;
    unsigned short s1;
    unsigned short s2;
    unsigned char  c[8];
} IID;

#endif // __IID_DEFINED__

#ifndef CLSID_DEFINED
#define CLSID_DEFINED
typedef IID CLSID;
#endif // CLSID_DEFINED

#define MIDL_DEFINE_GUID(type,name,l,w1,w2,b1,b2,b3,b4,b5,b6,b7,b8) \
        const type name = {l,w1,w2,{b1,b2,b3,b4,b5,b6,b7,b8}}

#endif !_MIDL_USE_GUIDDEF_

MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler,0xF2035E6A,0x8067,0x3ABB,0xA7,0x95,0x7B,0x33,0x4C,0x67,0xA2,0xED);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler,0x1698B961,0xF90E,0x30D0,0x80,0xFF,0x22,0xE9,0x4C,0xF6,0x6D,0x7B);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback,0x91DDEE70,0xAA90,0x38E7,0xB4,0xE5,0xF7,0x95,0x95,0x69,0xCB,0x5C);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals,0xF5A3C2AE,0xEF7B,0x3DE2,0x8B,0x0E,0x8E,0x8B,0x3C,0xD2,0x0D,0x9D);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals,0x044DEA28,0x0E8D,0x3A16,0xA2,0xC1,0xBE,0x95,0xC0,0xBE,0xD5,0xE5);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals,0x0CC88A54,0x89AF,0x3CC6,0x9B,0x95,0xF8,0xF2,0x24,0x28,0xAB,0xED);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener,0x39126060,0x0292,0x36D6,0xB3,0xF8,0x9A,0xC4,0x15,0x6C,0x65,0x1D);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals,0x8313DBEA,0xFD3B,0x3071,0x80,0x35,0x7B,0x61,0x16,0x58,0xDA,0xD8);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals,0x64B31D5B,0x1A27,0x37A8,0xBC,0xBC,0xC0,0xBB,0xD5,0x31,0x4C,0x79);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig,0xA9F22E31,0xD4E1,0x3940,0xBA,0x20,0xDC,0xB2,0x09,0x73,0xB0,0x9F);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals,0x06B50718,0x3528,0x3B66,0xBE,0x76,0xE1,0x83,0xAA,0x80,0xD4,0xA5);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer,0x6928CA7B,0x166D,0x3B37,0x90,0x10,0xFB,0xAB,0x2C,0x7E,0x92,0xB0);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater,0x4FA5F2C4,0x8612,0x35C9,0xBF,0xAA,0x96,0x7C,0x2C,0x81,0x9F,0xA7);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals,0xC8AFE1A8,0x92FC,0x3783,0x95,0x20,0xD6,0xBB,0xC5,0x07,0xB2,0x4A);


MIDL_DEFINE_GUID(IID, IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics,0x2C1E9C37,0x6827,0x38F7,0x85,0x7C,0x02,0x16,0x42,0xCA,0x42,0x8B);

#undef MIDL_DEFINE_GUID

#ifdef __cplusplus
}
#endif



