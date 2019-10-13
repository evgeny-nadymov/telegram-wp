

/* this ALWAYS GENERATED file contains the IIDs and CLSIDs */

/* link this file in with the server and any clients */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Mon Jun 19 00:49:35 2017
 */
/* Compiler settings for C:\Users\evgeny\AppData\Local\Temp\libtgvoip.idl-c4a72767:
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

MIDL_DEFINE_GUID(IID, IID___x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals,0x95221757,0xDCCC,0x3C3B,0xA5,0xAF,0x51,0x4F,0xC2,0x08,0xEA,0xF2);


MIDL_DEFINE_GUID(IID, IID___x_ABI_Clibtgvoip_CIStateCallback,0xA11CB322,0x951E,0x3651,0x8A,0xA4,0xEF,0x42,0xD1,0xE1,0x42,0xF3);


MIDL_DEFINE_GUID(IID, IID___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals,0xA4312674,0xFAEC,0x3E92,0xB8,0x4B,0x63,0x1A,0x08,0xBE,0xA9,0x63);


MIDL_DEFINE_GUID(IID, IID___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics,0xB855DB3B,0x9FD9,0x3215,0x8F,0x33,0x73,0x17,0x3C,0x2A,0xD6,0xCD);

#undef MIDL_DEFINE_GUID

#ifdef __cplusplus
}
#endif



