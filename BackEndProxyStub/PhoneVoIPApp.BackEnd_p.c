

/* this ALWAYS GENERATED file contains the proxy stub code */


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

#if defined(_ARM_)


#pragma warning( disable: 4049 )  /* more than 64k source lines */
#if _MSC_VER >= 1200
#pragma warning(push)
#endif

#pragma warning( disable: 4211 )  /* redefine extern to static */
#pragma warning( disable: 4232 )  /* dllimport identity*/
#pragma warning( disable: 4024 )  /* array to pointer mapping*/
#pragma warning( disable: 4152 )  /* function/data pointer conversion in expression */

#define USE_STUBLESS_PROXY


/* verify that the <rpcproxy.h> version is high enough to compile this file*/
#ifndef __REDQ_RPCPROXY_H_VERSION__
#define __REQUIRED_RPCPROXY_H_VERSION__ 475
#endif


#include "rpcproxy.h"
#ifndef __RPCPROXY_H_VERSION__
#error this stub requires an updated version of <rpcproxy.h>
#endif /* __RPCPROXY_H_VERSION__ */


#include "PhoneVoIPApp.BackEnd.h"

#define TYPE_FORMAT_STRING_SIZE   575                               
#define PROC_FORMAT_STRING_SIZE   4275                              
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   1            

typedef struct _PhoneVoIPApp2EBackEnd_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } PhoneVoIPApp2EBackEnd_MIDL_TYPE_FORMAT_STRING;

typedef struct _PhoneVoIPApp2EBackEnd_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } PhoneVoIPApp2EBackEnd_MIDL_PROC_FORMAT_STRING;

typedef struct _PhoneVoIPApp2EBackEnd_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } PhoneVoIPApp2EBackEnd_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const PhoneVoIPApp2EBackEnd_MIDL_TYPE_FORMAT_STRING PhoneVoIPApp2EBackEnd__MIDL_TypeFormatString;
extern const PhoneVoIPApp2EBackEnd_MIDL_PROC_FORMAT_STRING PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString;
extern const PhoneVoIPApp2EBackEnd_MIDL_EXPR_FORMAT_STRING PhoneVoIPApp2EBackEnd__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_ProxyInfo;


extern const USER_MARSHAL_ROUTINE_QUADRUPLE UserMarshalRoutines[ WIRE_MARSHAL_TABLE_SIZE ];

#if !defined(__RPC_ARM32__)
#error  Invalid build platform for this stub.
#endif

#if !(TARGET_IS_NT50_OR_LATER)
#error You need Windows 2000 or later to run this stub because it uses these features:
#error   /robust command line switch.
#error However, your C/C++ compilation flags indicate you intend to run this app on earlier systems.
#error This app will fail with the RPC_X_WRONG_STUB_VERSION error.
#endif


static const PhoneVoIPApp2EBackEnd_MIDL_PROC_FORMAT_STRING PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure Invoke */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x3 ),	/* 3 */
/*  8 */	NdrFcShort( 0x1c ),	/* ARM Stack size/offset = 28 */
/* 10 */	NdrFcShort( 0x20 ),	/* 32 */
/* 12 */	NdrFcShort( 0x8 ),	/* 8 */
/* 14 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x4,		/* 4 */
/* 16 */	0x12,		/* 18 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 18 */	NdrFcShort( 0x0 ),	/* 0 */
/* 20 */	NdrFcShort( 0x0 ),	/* 0 */
/* 22 */	NdrFcShort( 0x0 ),	/* 0 */
/* 24 */	NdrFcShort( 0x6 ),	/* 6 */
/* 26 */	0x6,		/* 6 */
			0x80,		/* 128 */
/* 28 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 30 */	0x83,		/* 131 */
			0xfc,		/* 252 */
/* 32 */	0xfc,		/* 252 */
			0x0,		/* 0 */

	/* Parameter pBuffer */

/* 34 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 36 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 38 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter hnsPresentationTime */

/* 40 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 42 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 44 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter hnsSampleDuration */

/* 46 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 48 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 50 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 52 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 54 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 56 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Invoke */

/* 58 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 60 */	NdrFcLong( 0x0 ),	/* 0 */
/* 64 */	NdrFcShort( 0x3 ),	/* 3 */
/* 66 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 68 */	NdrFcShort( 0x8 ),	/* 8 */
/* 70 */	NdrFcShort( 0x8 ),	/* 8 */
/* 72 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 74 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 76 */	NdrFcShort( 0x0 ),	/* 0 */
/* 78 */	NdrFcShort( 0x0 ),	/* 0 */
/* 80 */	NdrFcShort( 0x0 ),	/* 0 */
/* 82 */	NdrFcShort( 0x2 ),	/* 2 */
/* 84 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 86 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __param0 */

/* 88 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 90 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 92 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 94 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 96 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 98 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Invoke */

/* 100 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 102 */	NdrFcLong( 0x0 ),	/* 0 */
/* 106 */	NdrFcShort( 0x3 ),	/* 3 */
/* 108 */	NdrFcShort( 0x20 ),	/* ARM Stack size/offset = 32 */
/* 110 */	NdrFcShort( 0x25 ),	/* 37 */
/* 112 */	NdrFcShort( 0x8 ),	/* 8 */
/* 114 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x4,		/* 4 */
/* 116 */	0x12,		/* 18 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 118 */	NdrFcShort( 0x0 ),	/* 0 */
/* 120 */	NdrFcShort( 0x0 ),	/* 0 */
/* 122 */	NdrFcShort( 0x0 ),	/* 0 */
/* 124 */	NdrFcShort( 0x7 ),	/* 7 */
/* 126 */	0x7,		/* 7 */
			0x80,		/* 128 */
/* 128 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 130 */	0x83,		/* 131 */
			0xfc,		/* 252 */
/* 132 */	0xfc,		/* 252 */
			0xfc,		/* 252 */

	/* Parameter callId */

/* 134 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 136 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 138 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter callAccessHash */

/* 140 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 142 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 144 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter rejected */

/* 146 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 148 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 150 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 152 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 154 */	NdrFcShort( 0x1c ),	/* ARM Stack size/offset = 28 */
/* 156 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure WriteAudio */

/* 158 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 160 */	NdrFcLong( 0x0 ),	/* 0 */
/* 164 */	NdrFcShort( 0x6 ),	/* 6 */
/* 166 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 168 */	NdrFcShort( 0x8 ),	/* 8 */
/* 170 */	NdrFcShort( 0x21 ),	/* 33 */
/* 172 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x3,		/* 3 */
/* 174 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 176 */	NdrFcShort( 0x0 ),	/* 0 */
/* 178 */	NdrFcShort( 0x0 ),	/* 0 */
/* 180 */	NdrFcShort( 0x0 ),	/* 0 */
/* 182 */	NdrFcShort( 0x3 ),	/* 3 */
/* 184 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 186 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter bytes */

/* 188 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 190 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 192 */	0x1,		/* FC_BYTE */
			0x0,		/* 0 */

	/* Parameter byteCount */

/* 194 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 196 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 198 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 200 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 202 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 204 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure WriteVideo */

/* 206 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 208 */	NdrFcLong( 0x0 ),	/* 0 */
/* 212 */	NdrFcShort( 0x7 ),	/* 7 */
/* 214 */	NdrFcShort( 0x24 ),	/* ARM Stack size/offset = 36 */
/* 216 */	NdrFcShort( 0x28 ),	/* 40 */
/* 218 */	NdrFcShort( 0x21 ),	/* 33 */
/* 220 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x5,		/* 5 */
/* 222 */	0x14,		/* 20 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 224 */	NdrFcShort( 0x0 ),	/* 0 */
/* 226 */	NdrFcShort( 0x0 ),	/* 0 */
/* 228 */	NdrFcShort( 0x0 ),	/* 0 */
/* 230 */	NdrFcShort( 0x8 ),	/* 8 */
/* 232 */	0x8,		/* 8 */
			0x80,		/* 128 */
/* 234 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 236 */	0x9f,		/* 159 */
			0x9d,		/* 157 */
/* 238 */	0xfc,		/* 252 */
			0x4,		/* 4 */
/* 240 */	0x0,		/* 0 */
			0x0,		/* 0 */

	/* Parameter bytes */

/* 242 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 244 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 246 */	0x1,		/* FC_BYTE */
			0x0,		/* 0 */

	/* Parameter byteCount */

/* 248 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 250 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 252 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hnsPresentationTime */

/* 254 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 256 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 258 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter hnsSampleDuration */

/* 260 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 262 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 264 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 266 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 268 */	NdrFcShort( 0x20 ),	/* ARM Stack size/offset = 32 */
/* 270 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure add_AudioMessageReceived */

/* 272 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 274 */	NdrFcLong( 0x0 ),	/* 0 */
/* 278 */	NdrFcShort( 0x8 ),	/* 8 */
/* 280 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 282 */	NdrFcShort( 0x0 ),	/* 0 */
/* 284 */	NdrFcShort( 0x34 ),	/* 52 */
/* 286 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x3,		/* 3 */
/* 288 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 290 */	NdrFcShort( 0x0 ),	/* 0 */
/* 292 */	NdrFcShort( 0x0 ),	/* 0 */
/* 294 */	NdrFcShort( 0x0 ),	/* 0 */
/* 296 */	NdrFcShort( 0x3 ),	/* 3 */
/* 298 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 300 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter __param0 */

/* 302 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 304 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 306 */	NdrFcShort( 0x18 ),	/* Type Offset=24 */

	/* Parameter __returnValue */

/* 308 */	NdrFcShort( 0x2112 ),	/* Flags:  must free, out, simple ref, srv alloc size=8 */
/* 310 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 312 */	NdrFcShort( 0x2e ),	/* Type Offset=46 */

	/* Return value */

/* 314 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 316 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 318 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure remove_AudioMessageReceived */

/* 320 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 322 */	NdrFcLong( 0x0 ),	/* 0 */
/* 326 */	NdrFcShort( 0x9 ),	/* 9 */
/* 328 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 330 */	NdrFcShort( 0x18 ),	/* 24 */
/* 332 */	NdrFcShort( 0x8 ),	/* 8 */
/* 334 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 336 */	0x10,		/* 16 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 338 */	NdrFcShort( 0x0 ),	/* 0 */
/* 340 */	NdrFcShort( 0x0 ),	/* 0 */
/* 342 */	NdrFcShort( 0x0 ),	/* 0 */
/* 344 */	NdrFcShort( 0x4 ),	/* 4 */
/* 346 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 348 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 350 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter __param0 */

/* 352 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 354 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 356 */	NdrFcShort( 0x2e ),	/* Type Offset=46 */

	/* Return value */

/* 358 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 360 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 362 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure add_VideoMessageReceived */

/* 364 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 366 */	NdrFcLong( 0x0 ),	/* 0 */
/* 370 */	NdrFcShort( 0xa ),	/* 10 */
/* 372 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 374 */	NdrFcShort( 0x0 ),	/* 0 */
/* 376 */	NdrFcShort( 0x34 ),	/* 52 */
/* 378 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x3,		/* 3 */
/* 380 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 382 */	NdrFcShort( 0x0 ),	/* 0 */
/* 384 */	NdrFcShort( 0x0 ),	/* 0 */
/* 386 */	NdrFcShort( 0x0 ),	/* 0 */
/* 388 */	NdrFcShort( 0x3 ),	/* 3 */
/* 390 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 392 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter __param0 */

/* 394 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 396 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 398 */	NdrFcShort( 0x18 ),	/* Type Offset=24 */

	/* Parameter __returnValue */

/* 400 */	NdrFcShort( 0x2112 ),	/* Flags:  must free, out, simple ref, srv alloc size=8 */
/* 402 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 404 */	NdrFcShort( 0x2e ),	/* Type Offset=46 */

	/* Return value */

/* 406 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 408 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 410 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure remove_CameraLocationChanged */


	/* Procedure remove_VideoMessageReceived */

/* 412 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 414 */	NdrFcLong( 0x0 ),	/* 0 */
/* 418 */	NdrFcShort( 0xb ),	/* 11 */
/* 420 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 422 */	NdrFcShort( 0x18 ),	/* 24 */
/* 424 */	NdrFcShort( 0x8 ),	/* 8 */
/* 426 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 428 */	0x10,		/* 16 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 430 */	NdrFcShort( 0x0 ),	/* 0 */
/* 432 */	NdrFcShort( 0x0 ),	/* 0 */
/* 434 */	NdrFcShort( 0x0 ),	/* 0 */
/* 436 */	NdrFcShort( 0x4 ),	/* 4 */
/* 438 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 440 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 442 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter __param0 */


	/* Parameter __param0 */

/* 444 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 446 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 448 */	NdrFcShort( 0x2e ),	/* Type Offset=46 */

	/* Return value */


	/* Return value */

/* 450 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 452 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 454 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_id */

/* 456 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 458 */	NdrFcLong( 0x0 ),	/* 0 */
/* 462 */	NdrFcShort( 0x6 ),	/* 6 */
/* 464 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 466 */	NdrFcShort( 0x0 ),	/* 0 */
/* 468 */	NdrFcShort( 0x2c ),	/* 44 */
/* 470 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 472 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 474 */	NdrFcShort( 0x0 ),	/* 0 */
/* 476 */	NdrFcShort( 0x0 ),	/* 0 */
/* 478 */	NdrFcShort( 0x0 ),	/* 0 */
/* 480 */	NdrFcShort( 0x2 ),	/* 2 */
/* 482 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 484 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 486 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 488 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 490 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 492 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 494 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 496 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_id */

/* 498 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 500 */	NdrFcLong( 0x0 ),	/* 0 */
/* 504 */	NdrFcShort( 0x7 ),	/* 7 */
/* 506 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 508 */	NdrFcShort( 0x10 ),	/* 16 */
/* 510 */	NdrFcShort( 0x8 ),	/* 8 */
/* 512 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 514 */	0x10,		/* 16 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 516 */	NdrFcShort( 0x0 ),	/* 0 */
/* 518 */	NdrFcShort( 0x0 ),	/* 0 */
/* 520 */	NdrFcShort( 0x0 ),	/* 0 */
/* 522 */	NdrFcShort( 0x4 ),	/* 4 */
/* 524 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 526 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 528 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 530 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 532 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 534 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 536 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 538 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 540 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_port */

/* 542 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 544 */	NdrFcLong( 0x0 ),	/* 0 */
/* 548 */	NdrFcShort( 0x8 ),	/* 8 */
/* 550 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 552 */	NdrFcShort( 0x0 ),	/* 0 */
/* 554 */	NdrFcShort( 0x22 ),	/* 34 */
/* 556 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 558 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 560 */	NdrFcShort( 0x0 ),	/* 0 */
/* 562 */	NdrFcShort( 0x0 ),	/* 0 */
/* 564 */	NdrFcShort( 0x0 ),	/* 0 */
/* 566 */	NdrFcShort( 0x2 ),	/* 2 */
/* 568 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 570 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 572 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 574 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 576 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Return value */

/* 578 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 580 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 582 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_port */

/* 584 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 586 */	NdrFcLong( 0x0 ),	/* 0 */
/* 590 */	NdrFcShort( 0x9 ),	/* 9 */
/* 592 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 594 */	NdrFcShort( 0x6 ),	/* 6 */
/* 596 */	NdrFcShort( 0x8 ),	/* 8 */
/* 598 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 600 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 602 */	NdrFcShort( 0x0 ),	/* 0 */
/* 604 */	NdrFcShort( 0x0 ),	/* 0 */
/* 606 */	NdrFcShort( 0x0 ),	/* 0 */
/* 608 */	NdrFcShort( 0x2 ),	/* 2 */
/* 610 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 612 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 614 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 616 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 618 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Return value */

/* 620 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 622 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 624 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_ipv4 */

/* 626 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 628 */	NdrFcLong( 0x0 ),	/* 0 */
/* 632 */	NdrFcShort( 0xa ),	/* 10 */
/* 634 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 636 */	NdrFcShort( 0x0 ),	/* 0 */
/* 638 */	NdrFcShort( 0x8 ),	/* 8 */
/* 640 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 642 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 644 */	NdrFcShort( 0x1 ),	/* 1 */
/* 646 */	NdrFcShort( 0x0 ),	/* 0 */
/* 648 */	NdrFcShort( 0x0 ),	/* 0 */
/* 650 */	NdrFcShort( 0x2 ),	/* 2 */
/* 652 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 654 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 656 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 658 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 660 */	NdrFcShort( 0x5a ),	/* Type Offset=90 */

	/* Return value */

/* 662 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 664 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 666 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_ipv4 */

/* 668 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 670 */	NdrFcLong( 0x0 ),	/* 0 */
/* 674 */	NdrFcShort( 0xb ),	/* 11 */
/* 676 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 678 */	NdrFcShort( 0x0 ),	/* 0 */
/* 680 */	NdrFcShort( 0x8 ),	/* 8 */
/* 682 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 684 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 686 */	NdrFcShort( 0x0 ),	/* 0 */
/* 688 */	NdrFcShort( 0x1 ),	/* 1 */
/* 690 */	NdrFcShort( 0x0 ),	/* 0 */
/* 692 */	NdrFcShort( 0x2 ),	/* 2 */
/* 694 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 696 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 698 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 700 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 702 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Return value */

/* 704 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 706 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 708 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_ipv6 */

/* 710 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 712 */	NdrFcLong( 0x0 ),	/* 0 */
/* 716 */	NdrFcShort( 0xc ),	/* 12 */
/* 718 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 720 */	NdrFcShort( 0x0 ),	/* 0 */
/* 722 */	NdrFcShort( 0x8 ),	/* 8 */
/* 724 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 726 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 728 */	NdrFcShort( 0x1 ),	/* 1 */
/* 730 */	NdrFcShort( 0x0 ),	/* 0 */
/* 732 */	NdrFcShort( 0x0 ),	/* 0 */
/* 734 */	NdrFcShort( 0x2 ),	/* 2 */
/* 736 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 738 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 740 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 742 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 744 */	NdrFcShort( 0x5a ),	/* Type Offset=90 */

	/* Return value */

/* 746 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 748 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 750 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_ipv6 */

/* 752 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 754 */	NdrFcLong( 0x0 ),	/* 0 */
/* 758 */	NdrFcShort( 0xd ),	/* 13 */
/* 760 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 762 */	NdrFcShort( 0x0 ),	/* 0 */
/* 764 */	NdrFcShort( 0x8 ),	/* 8 */
/* 766 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 768 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 770 */	NdrFcShort( 0x0 ),	/* 0 */
/* 772 */	NdrFcShort( 0x1 ),	/* 1 */
/* 774 */	NdrFcShort( 0x0 ),	/* 0 */
/* 776 */	NdrFcShort( 0x2 ),	/* 2 */
/* 778 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 780 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 782 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 784 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 786 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Return value */

/* 788 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 790 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 792 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_peerTag */

/* 794 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 796 */	NdrFcLong( 0x0 ),	/* 0 */
/* 800 */	NdrFcShort( 0xe ),	/* 14 */
/* 802 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 804 */	NdrFcShort( 0x0 ),	/* 0 */
/* 806 */	NdrFcShort( 0x24 ),	/* 36 */
/* 808 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x3,		/* 3 */
/* 810 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 812 */	NdrFcShort( 0x1 ),	/* 1 */
/* 814 */	NdrFcShort( 0x0 ),	/* 0 */
/* 816 */	NdrFcShort( 0x0 ),	/* 0 */
/* 818 */	NdrFcShort( 0x3 ),	/* 3 */
/* 820 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 822 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter ____returnValueSize */

/* 824 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 826 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 828 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 830 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 832 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 834 */	NdrFcShort( 0x76 ),	/* Type Offset=118 */

	/* Return value */

/* 836 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 838 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 840 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_peerTag */

/* 842 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 844 */	NdrFcLong( 0x0 ),	/* 0 */
/* 848 */	NdrFcShort( 0xf ),	/* 15 */
/* 850 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 852 */	NdrFcShort( 0x8 ),	/* 8 */
/* 854 */	NdrFcShort( 0x8 ),	/* 8 */
/* 856 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x3,		/* 3 */
/* 858 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 860 */	NdrFcShort( 0x0 ),	/* 0 */
/* 862 */	NdrFcShort( 0x1 ),	/* 1 */
/* 864 */	NdrFcShort( 0x0 ),	/* 0 */
/* 866 */	NdrFcShort( 0x3 ),	/* 3 */
/* 868 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 870 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter ____set_formalSize */

/* 872 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 874 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 876 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 878 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 880 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 882 */	NdrFcShort( 0x8e ),	/* Type Offset=142 */

	/* Return value */

/* 884 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 886 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 888 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnSignalBarsChanged */

/* 890 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 892 */	NdrFcLong( 0x0 ),	/* 0 */
/* 896 */	NdrFcShort( 0x6 ),	/* 6 */
/* 898 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 900 */	NdrFcShort( 0x8 ),	/* 8 */
/* 902 */	NdrFcShort( 0x8 ),	/* 8 */
/* 904 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 906 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 908 */	NdrFcShort( 0x0 ),	/* 0 */
/* 910 */	NdrFcShort( 0x0 ),	/* 0 */
/* 912 */	NdrFcShort( 0x0 ),	/* 0 */
/* 914 */	NdrFcShort( 0x2 ),	/* 2 */
/* 916 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 918 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter newSignal */

/* 920 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 922 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 924 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 926 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 928 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 930 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Start */


	/* Procedure OnCallStateChanged */

/* 932 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 934 */	NdrFcLong( 0x0 ),	/* 0 */
/* 938 */	NdrFcShort( 0x7 ),	/* 7 */
/* 940 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 942 */	NdrFcShort( 0x8 ),	/* 8 */
/* 944 */	NdrFcShort( 0x8 ),	/* 8 */
/* 946 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 948 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 950 */	NdrFcShort( 0x0 ),	/* 0 */
/* 952 */	NdrFcShort( 0x0 ),	/* 0 */
/* 954 */	NdrFcShort( 0x0 ),	/* 0 */
/* 956 */	NdrFcShort( 0x2 ),	/* 2 */
/* 958 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 960 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter cameraLocation */


	/* Parameter newState */

/* 962 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 964 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 966 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */


	/* Return value */

/* 968 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 970 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 972 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnCallStatusChanged */

/* 974 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 976 */	NdrFcLong( 0x0 ),	/* 0 */
/* 980 */	NdrFcShort( 0x8 ),	/* 8 */
/* 982 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 984 */	NdrFcShort( 0x8 ),	/* 8 */
/* 986 */	NdrFcShort( 0x8 ),	/* 8 */
/* 988 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 990 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 992 */	NdrFcShort( 0x0 ),	/* 0 */
/* 994 */	NdrFcShort( 0x0 ),	/* 0 */
/* 996 */	NdrFcShort( 0x0 ),	/* 0 */
/* 998 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1000 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1002 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter newStatus */

/* 1004 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1006 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1008 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 1010 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1012 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1014 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnCallAudioRouteChanged */

/* 1016 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1018 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1022 */	NdrFcShort( 0x9 ),	/* 9 */
/* 1024 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1026 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1028 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1030 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1032 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1034 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1036 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1038 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1040 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1042 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1044 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter newRoute */

/* 1046 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1048 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1050 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 1052 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1054 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1056 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnMediaOperationsChanged */

/* 1058 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1060 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1064 */	NdrFcShort( 0xa ),	/* 10 */
/* 1066 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1068 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1070 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1072 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1074 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1076 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1078 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1080 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1082 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1084 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1086 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter newOperations */

/* 1088 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1090 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1092 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 1094 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1096 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1098 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnCameraLocationChanged */

/* 1100 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1102 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1106 */	NdrFcShort( 0xb ),	/* 11 */
/* 1108 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1110 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1112 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1114 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1116 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1118 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1120 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1122 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1124 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1126 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1128 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter newCameraLocation */

/* 1130 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1132 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1134 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 1136 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1138 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1140 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetTransport */

/* 1142 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1144 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1148 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1150 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1152 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1154 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1156 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 1158 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1160 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1162 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1164 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1166 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1168 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1170 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter transport */

/* 1172 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 1174 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1176 */	NdrFcShort( 0x9a ),	/* Type Offset=154 */

	/* Return value */

/* 1178 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1180 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1182 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure StopMTProtoUpdater */


	/* Procedure Stop */

/* 1184 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1186 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1190 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1192 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1194 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1196 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1198 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 1200 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1202 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1204 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1206 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1208 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1210 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */


	/* Return value */

/* 1212 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1214 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1216 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateVoIPControllerWrapper */


	/* Procedure ToggleCamera */

/* 1218 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1220 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1224 */	NdrFcShort( 0x9 ),	/* 9 */
/* 1226 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1228 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1230 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1232 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 1234 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1236 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1238 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1240 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1242 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1244 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */


	/* Return value */

/* 1246 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1248 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1250 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure add_CameraLocationChanged */

/* 1252 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1254 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1258 */	NdrFcShort( 0xa ),	/* 10 */
/* 1260 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 1262 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1264 */	NdrFcShort( 0x34 ),	/* 52 */
/* 1266 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x3,		/* 3 */
/* 1268 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1270 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1272 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1274 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1276 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1278 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 1280 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter __param0 */

/* 1282 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 1284 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1286 */	NdrFcShort( 0xac ),	/* Type Offset=172 */

	/* Parameter __returnValue */

/* 1288 */	NdrFcShort( 0x2112 ),	/* Flags:  must free, out, simple ref, srv alloc size=8 */
/* 1290 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1292 */	NdrFcShort( 0x2e ),	/* Type Offset=46 */

	/* Return value */

/* 1294 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1296 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1298 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_InitTimeout */

/* 1300 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1302 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1306 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1308 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1310 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1312 */	NdrFcShort( 0x2c ),	/* 44 */
/* 1314 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1316 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1318 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1320 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1322 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1324 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1326 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1328 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 1330 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1332 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1334 */	0xc,		/* FC_DOUBLE */
			0x0,		/* 0 */

	/* Return value */

/* 1336 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1338 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1340 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_InitTimeout */

/* 1342 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1344 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1348 */	NdrFcShort( 0x7 ),	/* 7 */
/* 1350 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 1352 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1354 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1356 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1358 */	0x10,		/* 16 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1360 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1362 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1364 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1366 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1368 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 1370 */	0x9f,		/* 159 */
			0x84,		/* 132 */
/* 1372 */	0x85,		/* 133 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 1374 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1376 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1378 */	0xc,		/* FC_DOUBLE */
			0x0,		/* 0 */

	/* Return value */

/* 1380 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1382 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 1384 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_RecvTimeout */

/* 1386 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1388 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1392 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1394 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1396 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1398 */	NdrFcShort( 0x2c ),	/* 44 */
/* 1400 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1402 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1404 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1406 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1408 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1410 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1412 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1414 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 1416 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1418 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1420 */	0xc,		/* FC_DOUBLE */
			0x0,		/* 0 */

	/* Return value */

/* 1422 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1424 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1426 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_RecvTimeout */

/* 1428 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1430 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1434 */	NdrFcShort( 0x9 ),	/* 9 */
/* 1436 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 1438 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1440 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1442 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1444 */	0x10,		/* 16 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1446 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1448 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1450 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1452 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1454 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 1456 */	0x9f,		/* 159 */
			0x84,		/* 132 */
/* 1458 */	0x85,		/* 133 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 1460 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1462 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1464 */	0xc,		/* FC_DOUBLE */
			0x0,		/* 0 */

	/* Return value */

/* 1466 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1468 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 1470 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Start */


	/* Procedure HandleUpdatePhoneCall */

/* 1472 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1474 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1478 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1480 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1482 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1484 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1486 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 1488 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1490 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1492 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1494 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1496 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1498 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */


	/* Return value */

/* 1500 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1502 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1504 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DoPeriodicKeepAlive */


	/* Procedure Stop */


	/* Procedure Stop */


	/* Procedure StartMTProtoUpdater */

/* 1506 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1508 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1512 */	NdrFcShort( 0x7 ),	/* 7 */
/* 1514 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1516 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1518 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1520 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 1522 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1524 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1526 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1528 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1530 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1532 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */


	/* Return value */


	/* Return value */


	/* Return value */

/* 1534 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1536 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1538 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DeleteVoIPControllerWrapper */

/* 1540 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1542 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1546 */	NdrFcShort( 0xa ),	/* 10 */
/* 1548 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1550 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1552 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1554 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 1556 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1558 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1560 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1562 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1564 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1566 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */

/* 1568 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1570 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1572 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetConfig */

/* 1574 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1576 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1580 */	NdrFcShort( 0xb ),	/* 11 */
/* 1582 */	NdrFcShort( 0x2c ),	/* ARM Stack size/offset = 44 */
/* 1584 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1586 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1588 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 1590 */	0x14,		/* 20 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 1592 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1594 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1596 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1598 */	NdrFcShort( 0xa ),	/* 10 */
/* 1600 */	0x8,		/* 8 */
			0x80,		/* 128 */
/* 1602 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 1604 */	0x83,		/* 131 */
			0x9d,		/* 157 */
/* 1606 */	0xfc,		/* 252 */
			0x6,		/* 6 */
/* 1608 */	0x0,		/* 0 */
			0x0,		/* 0 */

	/* Parameter config */

/* 1610 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 1612 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1614 */	NdrFcShort( 0xc2 ),	/* Type Offset=194 */

	/* Return value */

/* 1616 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1618 */	NdrFcShort( 0x28 ),	/* ARM Stack size/offset = 40 */
/* 1620 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetEncryptionKey */

/* 1622 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1624 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1628 */	NdrFcShort( 0xc ),	/* 12 */
/* 1630 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 1632 */	NdrFcShort( 0xd ),	/* 13 */
/* 1634 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1636 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x4,		/* 4 */
/* 1638 */	0x10,		/* 16 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 1640 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1642 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1644 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1646 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1648 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 1650 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 1652 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter __keySize */

/* 1654 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1656 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1658 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter key */

/* 1660 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1662 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1664 */	NdrFcShort( 0x8e ),	/* Type Offset=142 */

	/* Parameter isOutgoing */

/* 1666 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1668 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1670 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 1672 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1674 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 1676 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetPublicEndpoints */

/* 1678 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1680 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1684 */	NdrFcShort( 0xd ),	/* 13 */
/* 1686 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 1688 */	NdrFcShort( 0x15 ),	/* 21 */
/* 1690 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1692 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x5,		/* 5 */
/* 1694 */	0x10,		/* 16 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 1696 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1698 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1700 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1702 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1704 */	0x5,		/* 5 */
			0x80,		/* 128 */
/* 1706 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 1708 */	0x83,		/* 131 */
			0xfc,		/* 252 */

	/* Parameter __endpointsSize */

/* 1710 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1712 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1714 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter endpoints */

/* 1716 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1718 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1720 */	NdrFcShort( 0xf6 ),	/* Type Offset=246 */

	/* Parameter allowP2P */

/* 1722 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1724 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1726 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Parameter connectionMaxLayer */

/* 1728 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1730 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 1732 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1734 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1736 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 1738 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetProxy */

/* 1740 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1742 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1746 */	NdrFcShort( 0xe ),	/* 14 */
/* 1748 */	NdrFcShort( 0x1c ),	/* ARM Stack size/offset = 28 */
/* 1750 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1752 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1754 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 1756 */	0x12,		/* 18 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 1758 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1760 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1762 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1764 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1766 */	0x6,		/* 6 */
			0x80,		/* 128 */
/* 1768 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 1770 */	0x83,		/* 131 */
			0xfc,		/* 252 */
/* 1772 */	0xfc,		/* 252 */
			0x0,		/* 0 */

	/* Parameter proxy */

/* 1774 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 1776 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1778 */	NdrFcShort( 0x10c ),	/* Type Offset=268 */

	/* Return value */

/* 1780 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1782 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 1784 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Start */

/* 1786 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1788 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1792 */	NdrFcShort( 0xf ),	/* 15 */
/* 1794 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1796 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1798 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1800 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 1802 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1804 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1806 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1808 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1810 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1812 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */

/* 1814 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1816 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1818 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Connect */

/* 1820 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1822 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1826 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1828 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1830 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1832 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1834 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 1836 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1838 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1840 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1842 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1844 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1846 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */

/* 1848 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1850 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1852 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetMicMute */

/* 1854 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1856 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1860 */	NdrFcShort( 0x11 ),	/* 17 */
/* 1862 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1864 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1866 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1868 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1870 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1872 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1874 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1876 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1878 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1880 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1882 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter mute */

/* 1884 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1886 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1888 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 1890 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1892 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1894 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SwitchSpeaker */

/* 1896 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1898 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1902 */	NdrFcShort( 0x12 ),	/* 18 */
/* 1904 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1906 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1908 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1910 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1912 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1914 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1916 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1918 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1920 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1922 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1924 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter external */

/* 1926 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1928 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1930 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 1932 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1934 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1936 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure UpdateServerConfig */

/* 1938 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1940 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1944 */	NdrFcShort( 0x13 ),	/* 19 */
/* 1946 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1948 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1950 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1952 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 1954 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 1956 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1958 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1960 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1962 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1964 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1966 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter json */

/* 1968 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 1970 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1972 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Return value */

/* 1974 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1976 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1978 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetPreferredRelayID */

/* 1980 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1982 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1986 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1988 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1990 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1992 */	NdrFcShort( 0x2c ),	/* 44 */
/* 1994 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1996 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1998 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2000 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2002 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2004 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2006 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2008 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2010 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2012 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2014 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 2016 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2018 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2020 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetLastError */

/* 2022 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2024 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2028 */	NdrFcShort( 0x15 ),	/* 21 */
/* 2030 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2032 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2034 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2036 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2038 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2040 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2042 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2044 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2046 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2048 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2050 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2052 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2054 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2056 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 2058 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2060 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2062 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetDebugLog */

/* 2064 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2066 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2070 */	NdrFcShort( 0x16 ),	/* 22 */
/* 2072 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2074 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2076 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2078 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 2080 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 2082 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2084 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2086 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2088 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2090 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2092 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2094 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 2096 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2098 */	NdrFcShort( 0x5a ),	/* Type Offset=90 */

	/* Return value */

/* 2100 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2102 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2104 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetDebugString */

/* 2106 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2108 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2112 */	NdrFcShort( 0x17 ),	/* 23 */
/* 2114 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2116 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2118 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2120 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 2122 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 2124 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2126 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2128 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2130 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2132 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2134 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2136 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 2138 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2140 */	NdrFcShort( 0x5a ),	/* Type Offset=90 */

	/* Return value */

/* 2142 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2144 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2146 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetVersion */

/* 2148 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2150 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2154 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2156 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2158 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2160 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2162 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 2164 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 2166 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2168 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2170 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2172 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2174 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2176 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2178 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 2180 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2182 */	NdrFcShort( 0x5a ),	/* Type Offset=90 */

	/* Return value */

/* 2184 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2186 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2188 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetSignalBarsCount */

/* 2190 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2192 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2196 */	NdrFcShort( 0x19 ),	/* 25 */
/* 2198 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2200 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2202 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2204 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2206 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2208 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2210 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2212 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2214 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2216 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2218 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2220 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2222 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2224 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 2226 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2228 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2230 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetStatusCallback */

/* 2232 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2234 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2238 */	NdrFcShort( 0x1a ),	/* 26 */
/* 2240 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2242 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2244 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2246 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 2248 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2250 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2252 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2254 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2256 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2258 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2260 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter statusListener */

/* 2262 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 2264 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2266 */	NdrFcShort( 0x128 ),	/* Type Offset=296 */

	/* Return value */

/* 2268 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2270 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2272 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure InitiateOutgoingCall2 */

/* 2274 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2276 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2280 */	NdrFcShort( 0x1b ),	/* 27 */
/* 2282 */	NdrFcShort( 0x28 ),	/* ARM Stack size/offset = 40 */
/* 2284 */	NdrFcShort( 0x30 ),	/* 48 */
/* 2286 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2288 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x6,		/* 6 */
/* 2290 */	0x14,		/* 20 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 2292 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2294 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2296 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2298 */	NdrFcShort( 0x9 ),	/* 9 */
/* 2300 */	0x8,		/* 8 */
			0x80,		/* 128 */
/* 2302 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 2304 */	0x83,		/* 131 */
			0x9d,		/* 157 */
/* 2306 */	0xfc,		/* 252 */
			0x5,		/* 5 */
/* 2308 */	0x0,		/* 0 */
			0x0,		/* 0 */

	/* Parameter recepientName */

/* 2310 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 2312 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2314 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Parameter recepientId */

/* 2316 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2318 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2320 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter callId */

/* 2322 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2324 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 2326 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter callAccessHash */

/* 2328 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2330 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 2332 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2334 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2336 */	NdrFcShort( 0x20 ),	/* ARM Stack size/offset = 32 */
/* 2338 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2340 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2342 */	NdrFcShort( 0x24 ),	/* ARM Stack size/offset = 36 */
/* 2344 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure InitiateOutgoingCall1 */

/* 2346 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2348 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2352 */	NdrFcShort( 0x1c ),	/* 28 */
/* 2354 */	NdrFcShort( 0x80 ),	/* ARM Stack size/offset = 128 */
/* 2356 */	NdrFcShort( 0x5a ),	/* 90 */
/* 2358 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2360 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x11,		/* 17 */
/* 2362 */	0x14,		/* 20 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 2364 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2366 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2368 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2370 */	NdrFcShort( 0x1f ),	/* 31 */
/* 2372 */	0x8,		/* 8 */
			0x80,		/* 128 */
/* 2374 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 2376 */	0x83,		/* 131 */
			0x9d,		/* 157 */
/* 2378 */	0xfc,		/* 252 */
			0x1b,		/* 27 */
/* 2380 */	0x0,		/* 0 */
			0x0,		/* 0 */

	/* Parameter recepientName */

/* 2382 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 2384 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2386 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Parameter recepientId */

/* 2388 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2390 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2392 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter callId */

/* 2394 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2396 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 2398 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter callAccessHash */

/* 2400 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2402 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 2404 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter config */

/* 2406 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 2408 */	NdrFcShort( 0x20 ),	/* ARM Stack size/offset = 32 */
/* 2410 */	NdrFcShort( 0xc2 ),	/* Type Offset=194 */

	/* Parameter __keySize */

/* 2412 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2414 */	NdrFcShort( 0x40 ),	/* ARM Stack size/offset = 64 */
/* 2416 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter key */

/* 2418 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2420 */	NdrFcShort( 0x44 ),	/* ARM Stack size/offset = 68 */
/* 2422 */	NdrFcShort( 0x142 ),	/* Type Offset=322 */

	/* Parameter outgoing */

/* 2424 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2426 */	NdrFcShort( 0x48 ),	/* ARM Stack size/offset = 72 */
/* 2428 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Parameter __emojisSize */

/* 2430 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2432 */	NdrFcShort( 0x4c ),	/* ARM Stack size/offset = 76 */
/* 2434 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter emojis */

/* 2436 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2438 */	NdrFcShort( 0x50 ),	/* ARM Stack size/offset = 80 */
/* 2440 */	NdrFcShort( 0x152 ),	/* Type Offset=338 */

	/* Parameter __endpointsSize */

/* 2442 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2444 */	NdrFcShort( 0x54 ),	/* ARM Stack size/offset = 84 */
/* 2446 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter endpoints */

/* 2448 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2450 */	NdrFcShort( 0x58 ),	/* ARM Stack size/offset = 88 */
/* 2452 */	NdrFcShort( 0x16c ),	/* Type Offset=364 */

	/* Parameter allowP2P */

/* 2454 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2456 */	NdrFcShort( 0x5c ),	/* ARM Stack size/offset = 92 */
/* 2458 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Parameter connectionMaxLayer */

/* 2460 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2462 */	NdrFcShort( 0x60 ),	/* ARM Stack size/offset = 96 */
/* 2464 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter proxy */

/* 2466 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 2468 */	NdrFcShort( 0x64 ),	/* ARM Stack size/offset = 100 */
/* 2470 */	NdrFcShort( 0x10c ),	/* Type Offset=268 */

	/* Parameter __returnValue */

/* 2472 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2474 */	NdrFcShort( 0x78 ),	/* ARM Stack size/offset = 120 */
/* 2476 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2478 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2480 */	NdrFcShort( 0x7c ),	/* ARM Stack size/offset = 124 */
/* 2482 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnIncomingCallReceived */

/* 2484 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2486 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2490 */	NdrFcShort( 0x1d ),	/* 29 */
/* 2492 */	NdrFcShort( 0x34 ),	/* ARM Stack size/offset = 52 */
/* 2494 */	NdrFcShort( 0x30 ),	/* 48 */
/* 2496 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2498 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x8,		/* 8 */
/* 2500 */	0x16,		/* 22 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 2502 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2504 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2506 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2508 */	NdrFcShort( 0xc ),	/* 12 */
/* 2510 */	0xa,		/* 10 */
			0x80,		/* 128 */
/* 2512 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 2514 */	0x83,		/* 131 */
			0xfc,		/* 252 */
/* 2516 */	0x9f,		/* 159 */
			0x9d,		/* 157 */
/* 2518 */	0xfc,		/* 252 */
			0x6,		/* 6 */
/* 2520 */	0x0,		/* 0 */
			0x0,		/* 0 */

	/* Parameter contactName */

/* 2522 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 2524 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2526 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Parameter contactId */

/* 2528 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2530 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2532 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter contactImage */

/* 2534 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 2536 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 2538 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Parameter callId */

/* 2540 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2542 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 2544 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter callAccessHash */

/* 2546 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2548 */	NdrFcShort( 0x20 ),	/* ARM Stack size/offset = 32 */
/* 2550 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter incomingCallDialogDismissedCallback */

/* 2552 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 2554 */	NdrFcShort( 0x28 ),	/* ARM Stack size/offset = 40 */
/* 2556 */	NdrFcShort( 0x182 ),	/* Type Offset=386 */

	/* Parameter __returnValue */

/* 2558 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2560 */	NdrFcShort( 0x2c ),	/* ARM Stack size/offset = 44 */
/* 2562 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2564 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2566 */	NdrFcShort( 0x30 ),	/* ARM Stack size/offset = 48 */
/* 2568 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure HoldCall */

/* 2570 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2572 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2576 */	NdrFcShort( 0x1e ),	/* 30 */
/* 2578 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2580 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2582 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2584 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2586 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2588 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2590 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2592 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2594 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2596 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2598 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2600 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2602 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2604 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2606 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2608 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2610 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ResumeCall */

/* 2612 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2614 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2618 */	NdrFcShort( 0x1f ),	/* 31 */
/* 2620 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2622 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2624 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2626 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2628 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2630 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2632 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2634 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2636 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2638 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2640 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2642 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2644 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2646 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2648 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2650 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2652 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure EndCall */

/* 2654 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2656 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2660 */	NdrFcShort( 0x20 ),	/* 32 */
/* 2662 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2664 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2666 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2668 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2670 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2672 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2674 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2676 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2678 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2680 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2682 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2684 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2686 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2688 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2690 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2692 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2694 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ToggleCamera */

/* 2696 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2698 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2702 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2704 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2706 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2708 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2710 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2712 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2714 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2716 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2718 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2720 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2722 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2724 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2726 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2728 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2730 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2732 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2734 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2736 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_CallStatus */

/* 2738 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2740 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2744 */	NdrFcShort( 0x22 ),	/* 34 */
/* 2746 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2748 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2750 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2752 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2754 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2756 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2758 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2760 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2762 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2764 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2766 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2768 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2770 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2772 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 2774 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2776 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2778 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_MediaOperations */

/* 2780 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2782 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2786 */	NdrFcShort( 0x23 ),	/* 35 */
/* 2788 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2790 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2792 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2794 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2796 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2798 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2800 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2802 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2804 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2806 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2808 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2810 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2812 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2814 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 2816 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2818 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2820 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_IsShowingVideo */

/* 2822 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2824 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2828 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2830 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2832 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2834 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2836 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2838 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2840 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2842 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2844 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2846 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2848 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2850 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2852 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2854 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2856 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2858 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2860 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2862 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_IsShowingVideo */

/* 2864 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2866 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2870 */	NdrFcShort( 0x25 ),	/* 37 */
/* 2872 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2874 */	NdrFcShort( 0x5 ),	/* 5 */
/* 2876 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2878 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2880 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2882 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2884 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2886 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2888 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2890 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2892 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter value */

/* 2894 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2896 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2898 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2900 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2902 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2904 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_IsRenderingVideo */

/* 2906 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2908 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2912 */	NdrFcShort( 0x26 ),	/* 38 */
/* 2914 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2916 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2918 */	NdrFcShort( 0x21 ),	/* 33 */
/* 2920 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2922 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2924 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2926 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2928 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2930 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2932 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2934 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 2936 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2938 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2940 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2942 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2944 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2946 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_IsRenderingVideo */

/* 2948 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2950 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2954 */	NdrFcShort( 0x27 ),	/* 39 */
/* 2956 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 2958 */	NdrFcShort( 0x5 ),	/* 5 */
/* 2960 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2962 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 2964 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 2966 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2968 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2970 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2972 */	NdrFcShort( 0x2 ),	/* 2 */
/* 2974 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 2976 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter value */

/* 2978 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2980 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 2982 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 2984 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2986 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 2988 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_CameraLocation */

/* 2990 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2992 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2996 */	NdrFcShort( 0x28 ),	/* 40 */
/* 2998 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3000 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3002 */	NdrFcShort( 0x24 ),	/* 36 */
/* 3004 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3006 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3008 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3010 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3012 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3014 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3016 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3018 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3020 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3022 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3024 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 3026 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3028 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3030 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_AvailableAudioRoutes */

/* 3032 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3034 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3038 */	NdrFcShort( 0x29 ),	/* 41 */
/* 3040 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3042 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3044 */	NdrFcShort( 0x24 ),	/* 36 */
/* 3046 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3048 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3050 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3052 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3054 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3056 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3058 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3060 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3062 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3064 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3066 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 3068 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3070 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3072 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_AudioRoute */

/* 3074 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3076 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3080 */	NdrFcShort( 0x2a ),	/* 42 */
/* 3082 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3084 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3086 */	NdrFcShort( 0x24 ),	/* 36 */
/* 3088 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3090 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3092 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3094 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3096 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3098 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3100 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3102 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3104 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3106 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3108 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 3110 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3112 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3114 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_AudioRoute */

/* 3116 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3118 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3122 */	NdrFcShort( 0x2b ),	/* 43 */
/* 3124 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3126 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3128 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3130 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3132 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3134 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3136 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3138 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3140 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3142 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3144 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter newRoute */

/* 3146 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3148 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3150 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 3152 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3154 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3156 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_OtherPartyName */

/* 3158 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3160 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3164 */	NdrFcShort( 0x2c ),	/* 44 */
/* 3166 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3168 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3170 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3172 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 3174 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 3176 */	NdrFcShort( 0x1 ),	/* 1 */
/* 3178 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3180 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3182 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3184 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3186 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3188 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 3190 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3192 */	NdrFcShort( 0x5a ),	/* Type Offset=90 */

	/* Return value */

/* 3194 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3196 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3198 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_OtherPartyId */

/* 3200 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3202 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3206 */	NdrFcShort( 0x2d ),	/* 45 */
/* 3208 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3210 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3212 */	NdrFcShort( 0x2c ),	/* 44 */
/* 3214 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3216 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3218 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3220 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3222 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3224 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3226 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3228 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3230 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3232 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3234 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 3236 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3238 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3240 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_CallStartTime */

/* 3242 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3244 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3248 */	NdrFcShort( 0x2e ),	/* 46 */
/* 3250 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3252 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3254 */	NdrFcShort( 0x34 ),	/* 52 */
/* 3256 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3258 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3260 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3262 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3264 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3266 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3268 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3270 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3272 */	NdrFcShort( 0x2112 ),	/* Flags:  must free, out, simple ref, srv alloc size=8 */
/* 3274 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3276 */	NdrFcShort( 0x2e ),	/* Type Offset=46 */

	/* Return value */

/* 3278 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3280 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3282 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_CallId */

/* 3284 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3286 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3290 */	NdrFcShort( 0x2f ),	/* 47 */
/* 3292 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3294 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3296 */	NdrFcShort( 0x2c ),	/* 44 */
/* 3298 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3300 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3302 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3304 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3306 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3308 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3310 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3312 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3314 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3316 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3318 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 3320 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3322 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3324 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_CallAccessHash */

/* 3326 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3328 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3332 */	NdrFcShort( 0x30 ),	/* 48 */
/* 3334 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3336 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3338 */	NdrFcShort( 0x2c ),	/* 44 */
/* 3340 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3342 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3344 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3346 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3348 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3350 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3352 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3354 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3356 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3358 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3360 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 3362 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3364 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3366 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_AcceptedCallId */

/* 3368 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3370 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3374 */	NdrFcShort( 0x31 ),	/* 49 */
/* 3376 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3378 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3380 */	NdrFcShort( 0x2c ),	/* 44 */
/* 3382 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3384 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3386 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3388 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3390 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3392 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3394 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3396 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3398 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3400 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3402 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 3404 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3406 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3408 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_AcceptedCallId */

/* 3410 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3412 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3416 */	NdrFcShort( 0x32 ),	/* 50 */
/* 3418 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 3420 */	NdrFcShort( 0x10 ),	/* 16 */
/* 3422 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3424 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3426 */	0x10,		/* 16 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3428 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3430 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3432 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3434 */	NdrFcShort( 0x4 ),	/* 4 */
/* 3436 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 3438 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 3440 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter value */

/* 3442 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3444 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3446 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 3448 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3450 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 3452 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_Key */

/* 3454 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3456 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3460 */	NdrFcShort( 0x33 ),	/* 51 */
/* 3462 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 3464 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3466 */	NdrFcShort( 0x24 ),	/* 36 */
/* 3468 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x3,		/* 3 */
/* 3470 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 3472 */	NdrFcShort( 0x1 ),	/* 1 */
/* 3474 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3476 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3478 */	NdrFcShort( 0x3 ),	/* 3 */
/* 3480 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 3482 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter ____returnValueSize */

/* 3484 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3486 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3488 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3490 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 3492 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3494 */	NdrFcShort( 0x76 ),	/* Type Offset=118 */

	/* Return value */

/* 3496 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3498 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3500 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_Outgoing */

/* 3502 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3504 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3508 */	NdrFcShort( 0x34 ),	/* 52 */
/* 3510 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3512 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3514 */	NdrFcShort( 0x21 ),	/* 33 */
/* 3516 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 3518 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3520 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3522 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3524 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3526 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3528 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3530 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3532 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3534 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3536 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 3538 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3540 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3542 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_Emojis */

/* 3544 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3546 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3550 */	NdrFcShort( 0x35 ),	/* 53 */
/* 3552 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 3554 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3556 */	NdrFcShort( 0x24 ),	/* 36 */
/* 3558 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x3,		/* 3 */
/* 3560 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 3562 */	NdrFcShort( 0x1 ),	/* 1 */
/* 3564 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3566 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3568 */	NdrFcShort( 0x3 ),	/* 3 */
/* 3570 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 3572 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter ____returnValueSize */

/* 3574 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 3576 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3578 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3580 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 3582 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3584 */	NdrFcShort( 0x194 ),	/* Type Offset=404 */

	/* Return value */

/* 3586 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3588 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3590 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Start */

/* 3592 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3594 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3598 */	NdrFcShort( 0x6 ),	/* 6 */
/* 3600 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 3602 */	NdrFcShort( 0x18 ),	/* 24 */
/* 3604 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3606 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x4,		/* 4 */
/* 3608 */	0x10,		/* 16 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3610 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3612 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3614 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3616 */	NdrFcShort( 0x4 ),	/* 4 */
/* 3618 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 3620 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 3622 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter pts */

/* 3624 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3626 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3628 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter date */

/* 3630 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3632 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3634 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter qts */

/* 3636 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3638 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3640 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 3642 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3644 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 3646 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DiscardCall */

/* 3648 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3650 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3654 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3656 */	NdrFcShort( 0x1c ),	/* ARM Stack size/offset = 28 */
/* 3658 */	NdrFcShort( 0x20 ),	/* 32 */
/* 3660 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3662 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x3,		/* 3 */
/* 3664 */	0x12,		/* 18 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3666 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3668 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3670 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3672 */	NdrFcShort( 0x6 ),	/* 6 */
/* 3674 */	0x6,		/* 6 */
			0x80,		/* 128 */
/* 3676 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 3678 */	0x83,		/* 131 */
			0xfc,		/* 252 */
/* 3680 */	0xfc,		/* 252 */
			0x0,		/* 0 */

	/* Parameter id */

/* 3682 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3684 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3686 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter accessHash */

/* 3688 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3690 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 3692 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 3694 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3696 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 3698 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReceivedCall */

/* 3700 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3702 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3706 */	NdrFcShort( 0x9 ),	/* 9 */
/* 3708 */	NdrFcShort( 0x1c ),	/* ARM Stack size/offset = 28 */
/* 3710 */	NdrFcShort( 0x20 ),	/* 32 */
/* 3712 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3714 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x3,		/* 3 */
/* 3716 */	0x12,		/* 18 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3718 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3720 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3722 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3724 */	NdrFcShort( 0x6 ),	/* 6 */
/* 3726 */	0x6,		/* 6 */
			0x80,		/* 128 */
/* 3728 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 3730 */	0x83,		/* 131 */
			0xfc,		/* 252 */
/* 3732 */	0xfc,		/* 252 */
			0x0,		/* 0 */

	/* Parameter id */

/* 3734 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3736 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3738 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Parameter accessHash */

/* 3740 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3742 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 3744 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 3746 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3748 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 3750 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure StartServer */

/* 3752 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3754 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3758 */	NdrFcShort( 0x6 ),	/* 6 */
/* 3760 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 3762 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3764 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3766 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x3,		/* 3 */
/* 3768 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 3770 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3772 */	NdrFcShort( 0x1 ),	/* 1 */
/* 3774 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3776 */	NdrFcShort( 0x3 ),	/* 3 */
/* 3778 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 3780 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter __outOfProcServerClassNamesSize */

/* 3782 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 3784 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3786 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter outOfProcServerClassNames */

/* 3788 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 3790 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3792 */	NdrFcShort( 0x1b6 ),	/* Type Offset=438 */

	/* Return value */

/* 3794 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3796 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3798 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_CallController */

/* 3800 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3802 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3806 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3808 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3810 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3812 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3814 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 3816 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3818 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3820 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3822 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3824 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3826 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3828 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3830 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 3832 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3834 */	NdrFcShort( 0x1cc ),	/* Type Offset=460 */

	/* Return value */

/* 3836 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3838 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3840 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_VideoRenderer */

/* 3842 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3844 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3848 */	NdrFcShort( 0x9 ),	/* 9 */
/* 3850 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3852 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3854 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3856 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 3858 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3860 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3862 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3864 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3866 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3868 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3870 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3872 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 3874 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3876 */	NdrFcShort( 0x1e2 ),	/* Type Offset=482 */

	/* Return value */

/* 3878 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3880 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3882 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_VideoRenderer */

/* 3884 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3886 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3890 */	NdrFcShort( 0xa ),	/* 10 */
/* 3892 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3894 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3896 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3898 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 3900 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3902 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3904 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3906 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3908 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3910 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3912 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter value */

/* 3914 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 3916 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3918 */	NdrFcShort( 0x1e6 ),	/* Type Offset=486 */

	/* Return value */

/* 3920 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3922 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3924 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_MTProtoUpdater */

/* 3926 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3928 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3932 */	NdrFcShort( 0xb ),	/* 11 */
/* 3934 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3936 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3938 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3940 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 3942 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3944 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3946 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3948 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3950 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3952 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3954 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 3956 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 3958 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 3960 */	NdrFcShort( 0x1f8 ),	/* Type Offset=504 */

	/* Return value */

/* 3962 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 3964 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 3966 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_MTProtoUpdater */

/* 3968 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 3970 */	NdrFcLong( 0x0 ),	/* 0 */
/* 3974 */	NdrFcShort( 0xc ),	/* 12 */
/* 3976 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 3978 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3980 */	NdrFcShort( 0x8 ),	/* 8 */
/* 3982 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 3984 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 3986 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3988 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3990 */	NdrFcShort( 0x0 ),	/* 0 */
/* 3992 */	NdrFcShort( 0x2 ),	/* 2 */
/* 3994 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 3996 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter value */

/* 3998 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 4000 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 4002 */	NdrFcShort( 0x1fc ),	/* Type Offset=508 */

	/* Return value */

/* 4004 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 4006 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 4008 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_CaptureController */

/* 4010 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 4012 */	NdrFcLong( 0x0 ),	/* 0 */
/* 4016 */	NdrFcShort( 0xd ),	/* 13 */
/* 4018 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 4020 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4022 */	NdrFcShort( 0x8 ),	/* 8 */
/* 4024 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 4026 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 4028 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4030 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4032 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4034 */	NdrFcShort( 0x2 ),	/* 2 */
/* 4036 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 4038 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 4040 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 4042 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 4044 */	NdrFcShort( 0x20e ),	/* Type Offset=526 */

	/* Return value */

/* 4046 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 4048 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 4050 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_TransportController */

/* 4052 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 4054 */	NdrFcLong( 0x0 ),	/* 0 */
/* 4058 */	NdrFcShort( 0xe ),	/* 14 */
/* 4060 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 4062 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4064 */	NdrFcShort( 0x8 ),	/* 8 */
/* 4066 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 4068 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 4070 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4072 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4074 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4076 */	NdrFcShort( 0x2 ),	/* 2 */
/* 4078 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 4080 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 4082 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 4084 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 4086 */	NdrFcShort( 0x224 ),	/* Type Offset=548 */

	/* Return value */

/* 4088 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 4090 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 4092 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetCurrentProcessId */

/* 4094 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 4096 */	NdrFcLong( 0x0 ),	/* 0 */
/* 4100 */	NdrFcShort( 0x6 ),	/* 6 */
/* 4102 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 4104 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4106 */	NdrFcShort( 0x24 ),	/* 36 */
/* 4108 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 4110 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 4112 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4114 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4116 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4118 */	NdrFcShort( 0x2 ),	/* 2 */
/* 4120 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 4122 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 4124 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 4126 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 4128 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 4130 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 4132 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 4134 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetUiDisconnectedEventName */

/* 4136 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 4138 */	NdrFcLong( 0x0 ),	/* 0 */
/* 4142 */	NdrFcShort( 0x7 ),	/* 7 */
/* 4144 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 4146 */	NdrFcShort( 0x8 ),	/* 8 */
/* 4148 */	NdrFcShort( 0x8 ),	/* 8 */
/* 4150 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x3,		/* 3 */
/* 4152 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 4154 */	NdrFcShort( 0x1 ),	/* 1 */
/* 4156 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4158 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4160 */	NdrFcShort( 0x3 ),	/* 3 */
/* 4162 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 4164 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter backgroundProcessId */

/* 4166 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 4168 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 4170 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 4172 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 4174 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 4176 */	NdrFcShort( 0x5a ),	/* Type Offset=90 */

	/* Return value */

/* 4178 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 4180 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 4182 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetBackgroundProcessReadyEventName */

/* 4184 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 4186 */	NdrFcLong( 0x0 ),	/* 0 */
/* 4190 */	NdrFcShort( 0x8 ),	/* 8 */
/* 4192 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 4194 */	NdrFcShort( 0x8 ),	/* 8 */
/* 4196 */	NdrFcShort( 0x8 ),	/* 8 */
/* 4198 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x3,		/* 3 */
/* 4200 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 4202 */	NdrFcShort( 0x1 ),	/* 1 */
/* 4204 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4206 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4208 */	NdrFcShort( 0x3 ),	/* 3 */
/* 4210 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 4212 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter backgroundProcessId */

/* 4214 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 4216 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 4218 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 4220 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 4222 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 4224 */	NdrFcShort( 0x5a ),	/* Type Offset=90 */

	/* Return value */

/* 4226 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 4228 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 4230 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_Instance */

/* 4232 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 4234 */	NdrFcLong( 0x0 ),	/* 0 */
/* 4238 */	NdrFcShort( 0x9 ),	/* 9 */
/* 4240 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 4242 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4244 */	NdrFcShort( 0x8 ),	/* 8 */
/* 4246 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 4248 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 4250 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4252 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4254 */	NdrFcShort( 0x0 ),	/* 0 */
/* 4256 */	NdrFcShort( 0x2 ),	/* 2 */
/* 4258 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 4260 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 4262 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 4264 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 4266 */	NdrFcShort( 0x228 ),	/* Type Offset=552 */

	/* Return value */

/* 4268 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 4270 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 4272 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const PhoneVoIPApp2EBackEnd_MIDL_TYPE_FORMAT_STRING PhoneVoIPApp2EBackEnd__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/*  4 */	NdrFcLong( 0x905a0fe0 ),	/* -1873145888 */
/*  8 */	NdrFcShort( 0xbc53 ),	/* -17325 */
/* 10 */	NdrFcShort( 0x11df ),	/* 4575 */
/* 12 */	0x8c,		/* 140 */
			0x49,		/* 73 */
/* 14 */	0x0,		/* 0 */
			0x1e,		/* 30 */
/* 16 */	0x4f,		/* 79 */
			0xc6,		/* 198 */
/* 18 */	0x86,		/* 134 */
			0xda,		/* 218 */
/* 20 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 22 */	0x1,		/* FC_BYTE */
			0x5c,		/* FC_PAD */
/* 24 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 26 */	NdrFcLong( 0xf2035e6a ),	/* -234660246 */
/* 30 */	NdrFcShort( 0x8067 ),	/* -32665 */
/* 32 */	NdrFcShort( 0x3abb ),	/* 15035 */
/* 34 */	0xa7,		/* 167 */
			0x95,		/* 149 */
/* 36 */	0x7b,		/* 123 */
			0x33,		/* 51 */
/* 38 */	0x4c,		/* 76 */
			0x67,		/* 103 */
/* 40 */	0xa2,		/* 162 */
			0xed,		/* 237 */
/* 42 */	
			0x11, 0x4,	/* FC_RP [alloced_on_stack] */
/* 44 */	NdrFcShort( 0x2 ),	/* Offset= 2 (46) */
/* 46 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 48 */	NdrFcShort( 0x8 ),	/* 8 */
/* 50 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 52 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 54 */	0xb,		/* FC_HYPER */
			0x5c,		/* FC_PAD */
/* 56 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 58 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 60 */	
			0x11, 0x4,	/* FC_RP [alloced_on_stack] */
/* 62 */	NdrFcShort( 0x1c ),	/* Offset= 28 (90) */
/* 64 */	
			0x13, 0x0,	/* FC_OP */
/* 66 */	NdrFcShort( 0xe ),	/* Offset= 14 (80) */
/* 68 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 70 */	NdrFcShort( 0x2 ),	/* 2 */
/* 72 */	0x9,		/* Corr desc: FC_ULONG */
			0x0,		/*  */
/* 74 */	NdrFcShort( 0xfffc ),	/* -4 */
/* 76 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 78 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 80 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 82 */	NdrFcShort( 0x8 ),	/* 8 */
/* 84 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (68) */
/* 86 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 88 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 90 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 92 */	NdrFcShort( 0x0 ),	/* 0 */
/* 94 */	NdrFcShort( 0x4 ),	/* 4 */
/* 96 */	NdrFcShort( 0x0 ),	/* 0 */
/* 98 */	NdrFcShort( 0xffde ),	/* Offset= -34 (64) */
/* 100 */	
			0x12, 0x0,	/* FC_UP */
/* 102 */	NdrFcShort( 0xffea ),	/* Offset= -22 (80) */
/* 104 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 106 */	NdrFcShort( 0x0 ),	/* 0 */
/* 108 */	NdrFcShort( 0x4 ),	/* 4 */
/* 110 */	NdrFcShort( 0x0 ),	/* 0 */
/* 112 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (100) */
/* 114 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 116 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 118 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 120 */	NdrFcShort( 0x2 ),	/* Offset= 2 (122) */
/* 122 */	
			0x13, 0x0,	/* FC_OP */
/* 124 */	NdrFcShort( 0x2 ),	/* Offset= 2 (126) */
/* 126 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 128 */	NdrFcShort( 0x1 ),	/* 1 */
/* 130 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 132 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 134 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 136 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 138 */	
			0x11, 0x0,	/* FC_RP */
/* 140 */	NdrFcShort( 0x2 ),	/* Offset= 2 (142) */
/* 142 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 144 */	NdrFcShort( 0x1 ),	/* 1 */
/* 146 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 148 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 150 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 152 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 154 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 156 */	NdrFcLong( 0xf5a3c2ae ),	/* -173817170 */
/* 160 */	NdrFcShort( 0xef7b ),	/* -4229 */
/* 162 */	NdrFcShort( 0x3de2 ),	/* 15842 */
/* 164 */	0x8b,		/* 139 */
			0xe,		/* 14 */
/* 166 */	0x8e,		/* 142 */
			0x8b,		/* 139 */
/* 168 */	0x3c,		/* 60 */
			0xd2,		/* 210 */
/* 170 */	0xd,		/* 13 */
			0x9d,		/* 157 */
/* 172 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 174 */	NdrFcLong( 0x1698b961 ),	/* 379107681 */
/* 178 */	NdrFcShort( 0xf90e ),	/* -1778 */
/* 180 */	NdrFcShort( 0x30d0 ),	/* 12496 */
/* 182 */	0x80,		/* 128 */
			0xff,		/* 255 */
/* 184 */	0x22,		/* 34 */
			0xe9,		/* 233 */
/* 186 */	0x4c,		/* 76 */
			0xf6,		/* 246 */
/* 188 */	0x6d,		/* 109 */
			0x7b,		/* 123 */
/* 190 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 192 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 194 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 196 */	NdrFcShort( 0x20 ),	/* 32 */
/* 198 */	NdrFcShort( 0x0 ),	/* 0 */
/* 200 */	NdrFcShort( 0x0 ),	/* Offset= 0 (200) */
/* 202 */	0xc,		/* FC_DOUBLE */
			0xc,		/* FC_DOUBLE */
/* 204 */	0xe,		/* FC_ENUM32 */
			0x3,		/* FC_SMALL */
/* 206 */	0x3,		/* FC_SMALL */
			0x3,		/* FC_SMALL */
/* 208 */	0x3d,		/* FC_STRUCTPAD1 */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 210 */	0x0,		/* 0 */
			NdrFcShort( 0xff95 ),	/* Offset= -107 (104) */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 214 */	0x0,		/* 0 */
			NdrFcShort( 0xff91 ),	/* Offset= -111 (104) */
			0x5b,		/* FC_END */
/* 218 */	
			0x11, 0x0,	/* FC_RP */
/* 220 */	NdrFcShort( 0x1a ),	/* Offset= 26 (246) */
/* 222 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 224 */	NdrFcShort( 0x18 ),	/* 24 */
/* 226 */	NdrFcShort( 0x0 ),	/* 0 */
/* 228 */	NdrFcShort( 0x0 ),	/* Offset= 0 (228) */
/* 230 */	0xb,		/* FC_HYPER */
			0x6,		/* FC_SHORT */
/* 232 */	0x3e,		/* FC_STRUCTPAD2 */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 234 */	0x0,		/* 0 */
			NdrFcShort( 0xff7d ),	/* Offset= -131 (104) */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 238 */	0x0,		/* 0 */
			NdrFcShort( 0xff79 ),	/* Offset= -135 (104) */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 242 */	0x0,		/* 0 */
			NdrFcShort( 0xff75 ),	/* Offset= -139 (104) */
			0x5b,		/* FC_END */
/* 246 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x7,		/* 7 */
/* 248 */	NdrFcShort( 0x0 ),	/* 0 */
/* 250 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 252 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 254 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 256 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 260 */	NdrFcShort( 0x0 ),	/* Corr flags:  */
/* 262 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 264 */	NdrFcShort( 0xffd6 ),	/* Offset= -42 (222) */
/* 266 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 268 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 270 */	NdrFcShort( 0x14 ),	/* 20 */
/* 272 */	NdrFcShort( 0x0 ),	/* 0 */
/* 274 */	NdrFcShort( 0x0 ),	/* Offset= 0 (274) */
/* 276 */	0xe,		/* FC_ENUM32 */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 278 */	0x0,		/* 0 */
			NdrFcShort( 0xff51 ),	/* Offset= -175 (104) */
			0x6,		/* FC_SHORT */
/* 282 */	0x3e,		/* FC_STRUCTPAD2 */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 284 */	0x0,		/* 0 */
			NdrFcShort( 0xff4b ),	/* Offset= -181 (104) */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 288 */	0x0,		/* 0 */
			NdrFcShort( 0xff47 ),	/* Offset= -185 (104) */
			0x5b,		/* FC_END */
/* 292 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 294 */	0xe,		/* FC_ENUM32 */
			0x5c,		/* FC_PAD */
/* 296 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 298 */	NdrFcLong( 0x39126060 ),	/* 957505632 */
/* 302 */	NdrFcShort( 0x292 ),	/* 658 */
/* 304 */	NdrFcShort( 0x36d6 ),	/* 14038 */
/* 306 */	0xb3,		/* 179 */
			0xf8,		/* 248 */
/* 308 */	0x9a,		/* 154 */
			0xc4,		/* 196 */
/* 310 */	0x15,		/* 21 */
			0x6c,		/* 108 */
/* 312 */	0x65,		/* 101 */
			0x1d,		/* 29 */
/* 314 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 316 */	0x3,		/* FC_SMALL */
			0x5c,		/* FC_PAD */
/* 318 */	
			0x11, 0x0,	/* FC_RP */
/* 320 */	NdrFcShort( 0x2 ),	/* Offset= 2 (322) */
/* 322 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 324 */	NdrFcShort( 0x1 ),	/* 1 */
/* 326 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 328 */	NdrFcShort( 0x40 ),	/* ARM Stack size/offset = 64 */
/* 330 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 332 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 334 */	
			0x11, 0x0,	/* FC_RP */
/* 336 */	NdrFcShort( 0x2 ),	/* Offset= 2 (338) */
/* 338 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 340 */	NdrFcShort( 0x0 ),	/* 0 */
/* 342 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 344 */	NdrFcShort( 0x4c ),	/* ARM Stack size/offset = 76 */
/* 346 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 348 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 352 */	NdrFcShort( 0x0 ),	/* Corr flags:  */
/* 354 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 356 */	NdrFcShort( 0xff04 ),	/* Offset= -252 (104) */
/* 358 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 360 */	
			0x11, 0x0,	/* FC_RP */
/* 362 */	NdrFcShort( 0x2 ),	/* Offset= 2 (364) */
/* 364 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x7,		/* 7 */
/* 366 */	NdrFcShort( 0x0 ),	/* 0 */
/* 368 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 370 */	NdrFcShort( 0x54 ),	/* ARM Stack size/offset = 84 */
/* 372 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 374 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 378 */	NdrFcShort( 0x0 ),	/* Corr flags:  */
/* 380 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 382 */	NdrFcShort( 0xff60 ),	/* Offset= -160 (222) */
/* 384 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 386 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 388 */	NdrFcLong( 0x91ddee70 ),	/* -1847726480 */
/* 392 */	NdrFcShort( 0xaa90 ),	/* -21872 */
/* 394 */	NdrFcShort( 0x38e7 ),	/* 14567 */
/* 396 */	0xb4,		/* 180 */
			0xe5,		/* 229 */
/* 398 */	0xf7,		/* 247 */
			0x95,		/* 149 */
/* 400 */	0x95,		/* 149 */
			0x69,		/* 105 */
/* 402 */	0xcb,		/* 203 */
			0x5c,		/* 92 */
/* 404 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 406 */	NdrFcShort( 0x2 ),	/* Offset= 2 (408) */
/* 408 */	
			0x13, 0x0,	/* FC_OP */
/* 410 */	NdrFcShort( 0x2 ),	/* Offset= 2 (412) */
/* 412 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 414 */	NdrFcShort( 0x0 ),	/* 0 */
/* 416 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 418 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 420 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 422 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 426 */	NdrFcShort( 0x0 ),	/* Corr flags:  */
/* 428 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 430 */	NdrFcShort( 0xfeac ),	/* Offset= -340 (90) */
/* 432 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 434 */	
			0x11, 0x0,	/* FC_RP */
/* 436 */	NdrFcShort( 0x2 ),	/* Offset= 2 (438) */
/* 438 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 440 */	NdrFcShort( 0x0 ),	/* 0 */
/* 442 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 444 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 446 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 448 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 452 */	NdrFcShort( 0x0 ),	/* Corr flags:  */
/* 454 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 456 */	NdrFcShort( 0xfea0 ),	/* Offset= -352 (104) */
/* 458 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 460 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 462 */	NdrFcShort( 0x2 ),	/* Offset= 2 (464) */
/* 464 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 466 */	NdrFcLong( 0x6b50718 ),	/* 112527128 */
/* 470 */	NdrFcShort( 0x3528 ),	/* 13608 */
/* 472 */	NdrFcShort( 0x3b66 ),	/* 15206 */
/* 474 */	0xbe,		/* 190 */
			0x76,		/* 118 */
/* 476 */	0xe1,		/* 225 */
			0x83,		/* 131 */
/* 478 */	0xaa,		/* 170 */
			0x80,		/* 128 */
/* 480 */	0xd4,		/* 212 */
			0xa5,		/* 165 */
/* 482 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 484 */	NdrFcShort( 0x2 ),	/* Offset= 2 (486) */
/* 486 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 488 */	NdrFcLong( 0x6928ca7b ),	/* 1764280955 */
/* 492 */	NdrFcShort( 0x166d ),	/* 5741 */
/* 494 */	NdrFcShort( 0x3b37 ),	/* 15159 */
/* 496 */	0x90,		/* 144 */
			0x10,		/* 16 */
/* 498 */	0xfb,		/* 251 */
			0xab,		/* 171 */
/* 500 */	0x2c,		/* 44 */
			0x7e,		/* 126 */
/* 502 */	0x92,		/* 146 */
			0xb0,		/* 176 */
/* 504 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 506 */	NdrFcShort( 0x2 ),	/* Offset= 2 (508) */
/* 508 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 510 */	NdrFcLong( 0x4fa5f2c4 ),	/* 1336275652 */
/* 514 */	NdrFcShort( 0x8612 ),	/* -31214 */
/* 516 */	NdrFcShort( 0x35c9 ),	/* 13769 */
/* 518 */	0xbf,		/* 191 */
			0xaa,		/* 170 */
/* 520 */	0x96,		/* 150 */
			0x7c,		/* 124 */
/* 522 */	0x2c,		/* 44 */
			0x81,		/* 129 */
/* 524 */	0x9f,		/* 159 */
			0xa7,		/* 167 */
/* 526 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 528 */	NdrFcShort( 0x2 ),	/* Offset= 2 (530) */
/* 530 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 532 */	NdrFcLong( 0x8313dbea ),	/* -2095850518 */
/* 536 */	NdrFcShort( 0xfd3b ),	/* -709 */
/* 538 */	NdrFcShort( 0x3071 ),	/* 12401 */
/* 540 */	0x80,		/* 128 */
			0x35,		/* 53 */
/* 542 */	0x7b,		/* 123 */
			0x61,		/* 97 */
/* 544 */	0x16,		/* 22 */
			0x58,		/* 88 */
/* 546 */	0xda,		/* 218 */
			0xd8,		/* 216 */
/* 548 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 550 */	NdrFcShort( 0xfe74 ),	/* Offset= -396 (154) */
/* 552 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 554 */	NdrFcShort( 0x2 ),	/* Offset= 2 (556) */
/* 556 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 558 */	NdrFcLong( 0xc8afe1a8 ),	/* -927997528 */
/* 562 */	NdrFcShort( 0x92fc ),	/* -27908 */
/* 564 */	NdrFcShort( 0x3783 ),	/* 14211 */
/* 566 */	0x95,		/* 149 */
			0x20,		/* 32 */
/* 568 */	0xd6,		/* 214 */
			0xbb,		/* 187 */
/* 570 */	0xc5,		/* 197 */
			0x7,		/* 7 */
/* 572 */	0xb2,		/* 178 */
			0x4a,		/* 74 */

			0x0
        }
    };

static const USER_MARSHAL_ROUTINE_QUADRUPLE UserMarshalRoutines[ WIRE_MARSHAL_TABLE_SIZE ] = 
        {
            
            {
            HSTRING_UserSize
            ,HSTRING_UserMarshal
            ,HSTRING_UserUnmarshal
            ,HSTRING_UserFree
            }

        };



/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0000, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IUnknown, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler, ver. 0.0,
   GUID={0xF2035E6A,0x8067,0x3ABB,{0xA7,0x95,0x7B,0x33,0x4C,0x67,0xA2,0xED}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(4) ___x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandlerProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler::Invoke */
};

const CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandlerStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler_ServerInfo,
    4,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler, ver. 0.0,
   GUID={0x1698B961,0xF90E,0x30D0,{0x80,0xFF,0x22,0xE9,0x4C,0xF6,0x6D,0x7B}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_FormatStringOffsetTable[] =
    {
    58
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(4) ___x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandlerProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler::Invoke */
};

const CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandlerStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler_ServerInfo,
    4,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback, ver. 0.0,
   GUID={0x91DDEE70,0xAA90,0x38E7,{0xB4,0xE5,0xF7,0x95,0x95,0x69,0xCB,0x5C}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_FormatStringOffsetTable[] =
    {
    100
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(4) ___x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallbackProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback::Invoke */
};

const CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallbackStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback_ServerInfo,
    4,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0003, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IInspectable, ver. 0.0,
   GUID={0xAF86E2E0,0xB12D,0x4c6a,{0x9C,0x5A,0xD7,0xAA,0x65,0x10,0x1E,0x90}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals, ver. 0.0,
   GUID={0xF5A3C2AE,0xEF7B,0x3DE2,{0x8B,0x0E,0x8E,0x8B,0x3C,0xD2,0x0D,0x9D}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    158,
    206,
    272,
    320,
    364,
    412
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(12) ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtualsProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals::WriteAudio */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals::WriteVideo */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals::add_AudioMessageReceived */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals::remove_AudioMessageReceived */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals::add_VideoMessageReceived */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals::remove_VideoMessageReceived */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtualsStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_ServerInfo,
    12,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0004, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals, ver. 0.0,
   GUID={0x044DEA28,0x0E8D,0x3A16,{0xA2,0xC1,0xBE,0x95,0xC0,0xBE,0xD5,0xE5}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtualsProxyVtbl = 
{
    0,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtualsStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_ServerInfo,
    6,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0005, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals, ver. 0.0,
   GUID={0x0CC88A54,0x89AF,0x3CC6,{0x9B,0x95,0xF8,0xF2,0x24,0x28,0xAB,0xED}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    456,
    498,
    542,
    584,
    626,
    668,
    710,
    752,
    794,
    842
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(16) ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtualsProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::get_id */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::put_id */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::get_port */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::put_port */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::get_ipv4 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::put_ipv4 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::get_ipv6 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::put_ipv6 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::get_peerTag */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals::put_peerTag */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtualsStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_ServerInfo,
    16,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0006, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener, ver. 0.0,
   GUID={0x39126060,0x0292,0x36D6,{0xB3,0xF8,0x9A,0xC4,0x15,0x6C,0x65,0x1D}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    890,
    932,
    974,
    1016,
    1058,
    1100
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(12) ___x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListenerProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener::OnSignalBarsChanged */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener::OnCallStateChanged */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener::OnCallStatusChanged */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener::OnCallAudioRouteChanged */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener::OnMediaOperationsChanged */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener::OnCameraLocationChanged */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListenerStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_ServerInfo,
    12,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0007, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals, ver. 0.0,
   GUID={0x8313DBEA,0xFD3B,0x3071,{0x80,0x35,0x7B,0x61,0x16,0x58,0xDA,0xD8}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    1142,
    932,
    1184,
    1218,
    1252,
    412
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(12) ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtualsProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals::SetTransport */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals::Start */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals::Stop */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals::ToggleCamera */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals::add_CameraLocationChanged */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals::remove_CameraLocationChanged */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtualsStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_ServerInfo,
    12,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0008, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals, ver. 0.0,
   GUID={0x64B31D5B,0x1A27,0x37A8,{0xBC,0xBC,0xC0,0xBB,0xD5,0x31,0x4C,0x79}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtualsProxyVtbl = 
{
    0,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtualsStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_ServerInfo,
    6,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0009, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig, ver. 0.0,
   GUID={0xA9F22E31,0xD4E1,0x3940,{0xBA,0x20,0xDC,0xB2,0x09,0x73,0xB0,0x9F}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    1300,
    1342,
    1386,
    1428
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(10) ___x_ABI_CPhoneVoIPApp_CBackEnd_CIConfigProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig::get_InitTimeout */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig::put_InitTimeout */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig::get_RecvTimeout */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig::put_RecvTimeout */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_CIConfigStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_ServerInfo,
    10,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0010, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals, ver. 0.0,
   GUID={0x06B50718,0x3528,0x3B66,{0xBE,0x76,0xE1,0x83,0xAA,0x80,0xD4,0xA5}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    1472,
    1506,
    1184,
    1218,
    1540,
    1574,
    1622,
    1678,
    1740,
    1786,
    1820,
    1854,
    1896,
    1938,
    1980,
    2022,
    2064,
    2106,
    2148,
    2190,
    2232,
    2274,
    2346,
    2484,
    2570,
    2612,
    2654,
    2696,
    2738,
    2780,
    2822,
    2864,
    2906,
    2948,
    2990,
    3032,
    3074,
    3116,
    3158,
    3200,
    3242,
    3284,
    3326,
    3368,
    3410,
    3454,
    3502,
    3544
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(54) ___x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtualsProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::HandleUpdatePhoneCall */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::StartMTProtoUpdater */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::StopMTProtoUpdater */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::CreateVoIPControllerWrapper */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::DeleteVoIPControllerWrapper */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::SetConfig */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::SetEncryptionKey */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::SetPublicEndpoints */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::SetProxy */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::Start */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::Connect */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::SetMicMute */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::SwitchSpeaker */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::UpdateServerConfig */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::GetPreferredRelayID */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::GetLastError */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::GetDebugLog */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::GetDebugString */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::GetVersion */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::GetSignalBarsCount */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::SetStatusCallback */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::InitiateOutgoingCall2 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::InitiateOutgoingCall1 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::OnIncomingCallReceived */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::HoldCall */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::ResumeCall */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::EndCall */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::ToggleCamera */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_CallStatus */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_MediaOperations */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_IsShowingVideo */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::put_IsShowingVideo */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_IsRenderingVideo */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::put_IsRenderingVideo */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_CameraLocation */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_AvailableAudioRoutes */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_AudioRoute */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::put_AudioRoute */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_OtherPartyName */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_OtherPartyId */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_CallStartTime */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_CallId */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_CallAccessHash */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_AcceptedCallId */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::put_AcceptedCallId */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_Key */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_Outgoing */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals::get_Emojis */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtualsStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_ServerInfo,
    54,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0011, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer, ver. 0.0,
   GUID={0x6928CA7B,0x166D,0x3B37,{0x90,0x10,0xFB,0xAB,0x2C,0x7E,0x92,0xB0}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    1472,
    1506
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(8) ___x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRendererProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer::Start */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer::Stop */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRendererStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_ServerInfo,
    8,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0012, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater, ver. 0.0,
   GUID={0x4FA5F2C4,0x8612,0x35C9,{0xBF,0xAA,0x96,0x7C,0x2C,0x81,0x9F,0xA7}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    3592,
    1506,
    3648,
    3700
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(10) ___x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdaterProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater::Start */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater::Stop */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater::DiscardCall */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater::ReceivedCall */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdaterStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_ServerInfo,
    10,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0013, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals, ver. 0.0,
   GUID={0xC8AFE1A8,0x92FC,0x3783,{0x95,0x20,0xD6,0xBB,0xC5,0x07,0xB2,0x4A}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    3752,
    1506,
    3800,
    3842,
    3884,
    3926,
    3968,
    4010,
    4052
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(15) ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtualsProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::StartServer */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::DoPeriodicKeepAlive */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::get_CallController */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::get_VideoRenderer */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::put_VideoRenderer */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::get_MTProtoUpdater */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::put_MTProtoUpdater */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::get_CaptureController */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals::get_TransportController */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtualsStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_ServerInfo,
    15,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0014, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics, ver. 0.0,
   GUID={0x2C1E9C37,0x6827,0x38F7,{0x85,0x7C,0x02,0x16,0x42,0xCA,0x42,0x8B}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    4094,
    4136,
    4184,
    4232
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_ProxyInfo =
    {
    &Object_StubDesc,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_ProcFormatString.Format,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(10) ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStaticsProxyVtbl = 
{
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_ProxyInfo,
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics::GetCurrentProcessId */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics::GetUiDisconnectedEventName */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics::GetBackgroundProcessReadyEventName */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics::get_Instance */
};


static const PRPC_STUB_FUNCTION __x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStaticsStubVtbl =
{
    &IID___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_ServerInfo,
    10,
    &__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_PhoneVoIPApp2EBackEnd_0000_0015, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */

static const MIDL_STUB_DESC Object_StubDesc = 
    {
    0,
    NdrOleAllocate,
    NdrOleFree,
    0,
    0,
    0,
    0,
    0,
    PhoneVoIPApp2EBackEnd__MIDL_TypeFormatString.Format,
    1, /* -error bounds_check flag */
    0x50002, /* Ndr library version */
    0,
    0x800025b, /* MIDL Version 8.0.603 */
    0,
    UserMarshalRoutines,
    0,  /* notify & notify_flag routine table */
    0x1, /* MIDL flag */
    0, /* cs routines */
    0,   /* proxy/server info */
    0
    };

const CInterfaceProxyVtbl * const _PhoneVoIPApp2EBackEnd_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtualsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtualsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIConfigProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStaticsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtualsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtualsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListenerProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandlerProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandlerProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallbackProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRendererProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtualsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtualsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdaterProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtualsProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _PhoneVoIPApp2EBackEnd_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtualsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtualsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIConfigStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStaticsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtualsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtualsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListenerStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandlerStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandlerStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallbackStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRendererStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtualsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtualsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdaterStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtualsStubVtbl,
    0
};

PCInterfaceName const _PhoneVoIPApp2EBackEnd_InterfaceNamesList[] = 
{
    "__x_ABI_CPhoneVoIPApp_CBackEnd_C____ICallControllerPublicNonVirtuals",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportProtectedNonVirtuals",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_CIConfig",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsStatics",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_C____IEndpointPublicNonVirtuals",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCaptureProtectedNonVirtuals",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_CICallControllerStatusListener",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_CICameraLocationChangedEventHandler",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_CIMessageReceivedEventHandler",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_CIIncomingCallDialogDismissedCallback",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_CIVideoRenderer",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_C____IGlobalsPublicNonVirtuals",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndTransportPublicNonVirtuals",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_CIMTProtoUpdater",
    "__x_ABI_CPhoneVoIPApp_CBackEnd_C____IBackEndCapturePublicNonVirtuals",
    0
};

const IID *  const _PhoneVoIPApp2EBackEnd_BaseIIDList[] = 
{
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    0,
    0,
    0,
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    0
};


#define _PhoneVoIPApp2EBackEnd_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _PhoneVoIPApp2EBackEnd, pIID, n)

int __stdcall _PhoneVoIPApp2EBackEnd_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _PhoneVoIPApp2EBackEnd, 15, 8 )
    IID_BS_LOOKUP_NEXT_TEST( _PhoneVoIPApp2EBackEnd, 4 )
    IID_BS_LOOKUP_NEXT_TEST( _PhoneVoIPApp2EBackEnd, 2 )
    IID_BS_LOOKUP_NEXT_TEST( _PhoneVoIPApp2EBackEnd, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _PhoneVoIPApp2EBackEnd, 15, *pIndex )
    
}

const ExtendedProxyFileInfo PhoneVoIPApp2EBackEnd_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _PhoneVoIPApp2EBackEnd_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _PhoneVoIPApp2EBackEnd_StubVtblList,
    (const PCInterfaceName * ) & _PhoneVoIPApp2EBackEnd_InterfaceNamesList,
    (const IID ** ) & _PhoneVoIPApp2EBackEnd_BaseIIDList,
    & _PhoneVoIPApp2EBackEnd_IID_Lookup, 
    15,
    2,
    0, /* table of [async_uuid] interfaces */
    0, /* Filler1 */
    0, /* Filler2 */
    0  /* Filler3 */
};
#if _MSC_VER >= 1200
#pragma warning(pop)
#endif


#endif /* if defined(_ARM_) */

