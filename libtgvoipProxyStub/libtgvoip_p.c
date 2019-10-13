

/* this ALWAYS GENERATED file contains the proxy stub code */


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


#include "libtgvoip.h"

#define TYPE_FORMAT_STRING_SIZE   171                               
#define PROC_FORMAT_STRING_SIZE   1171                              
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   1            

typedef struct _libtgvoip_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } libtgvoip_MIDL_TYPE_FORMAT_STRING;

typedef struct _libtgvoip_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } libtgvoip_MIDL_PROC_FORMAT_STRING;

typedef struct _libtgvoip_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } libtgvoip_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const libtgvoip_MIDL_TYPE_FORMAT_STRING libtgvoip__MIDL_TypeFormatString;
extern const libtgvoip_MIDL_PROC_FORMAT_STRING libtgvoip__MIDL_ProcFormatString;
extern const libtgvoip_MIDL_EXPR_FORMAT_STRING libtgvoip__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_Clibtgvoip_CIStateCallback_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_Clibtgvoip_CIStateCallback_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_ProxyInfo;


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


static const libtgvoip_MIDL_PROC_FORMAT_STRING libtgvoip__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure get_id */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x6 ),	/* 6 */
/*  8 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 10 */	NdrFcShort( 0x0 ),	/* 0 */
/* 12 */	NdrFcShort( 0x2c ),	/* 44 */
/* 14 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 16 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 18 */	NdrFcShort( 0x0 ),	/* 0 */
/* 20 */	NdrFcShort( 0x0 ),	/* 0 */
/* 22 */	NdrFcShort( 0x0 ),	/* 0 */
/* 24 */	NdrFcShort( 0x2 ),	/* 2 */
/* 26 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 28 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 30 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 32 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 34 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 36 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 38 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 40 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_id */

/* 42 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 44 */	NdrFcLong( 0x0 ),	/* 0 */
/* 48 */	NdrFcShort( 0x7 ),	/* 7 */
/* 50 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 52 */	NdrFcShort( 0x10 ),	/* 16 */
/* 54 */	NdrFcShort( 0x8 ),	/* 8 */
/* 56 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 58 */	0x10,		/* 16 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 60 */	NdrFcShort( 0x0 ),	/* 0 */
/* 62 */	NdrFcShort( 0x0 ),	/* 0 */
/* 64 */	NdrFcShort( 0x0 ),	/* 0 */
/* 66 */	NdrFcShort( 0x4 ),	/* 4 */
/* 68 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 70 */	0x9f,		/* 159 */
			0x82,		/* 130 */
/* 72 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 74 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 76 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 78 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 80 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 82 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 84 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_port */

/* 86 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 88 */	NdrFcLong( 0x0 ),	/* 0 */
/* 92 */	NdrFcShort( 0x8 ),	/* 8 */
/* 94 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 96 */	NdrFcShort( 0x0 ),	/* 0 */
/* 98 */	NdrFcShort( 0x22 ),	/* 34 */
/* 100 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 102 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 104 */	NdrFcShort( 0x0 ),	/* 0 */
/* 106 */	NdrFcShort( 0x0 ),	/* 0 */
/* 108 */	NdrFcShort( 0x0 ),	/* 0 */
/* 110 */	NdrFcShort( 0x2 ),	/* 2 */
/* 112 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 114 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 116 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 118 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 120 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Return value */

/* 122 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 124 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 126 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_port */

/* 128 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 130 */	NdrFcLong( 0x0 ),	/* 0 */
/* 134 */	NdrFcShort( 0x9 ),	/* 9 */
/* 136 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 138 */	NdrFcShort( 0x6 ),	/* 6 */
/* 140 */	NdrFcShort( 0x8 ),	/* 8 */
/* 142 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 144 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 146 */	NdrFcShort( 0x0 ),	/* 0 */
/* 148 */	NdrFcShort( 0x0 ),	/* 0 */
/* 150 */	NdrFcShort( 0x0 ),	/* 0 */
/* 152 */	NdrFcShort( 0x2 ),	/* 2 */
/* 154 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 156 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 158 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 160 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 162 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Return value */

/* 164 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 166 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 168 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_ipv4 */

/* 170 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 172 */	NdrFcLong( 0x0 ),	/* 0 */
/* 176 */	NdrFcShort( 0xa ),	/* 10 */
/* 178 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 180 */	NdrFcShort( 0x0 ),	/* 0 */
/* 182 */	NdrFcShort( 0x8 ),	/* 8 */
/* 184 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 186 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 188 */	NdrFcShort( 0x1 ),	/* 1 */
/* 190 */	NdrFcShort( 0x0 ),	/* 0 */
/* 192 */	NdrFcShort( 0x0 ),	/* 0 */
/* 194 */	NdrFcShort( 0x2 ),	/* 2 */
/* 196 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 198 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 200 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 202 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 204 */	NdrFcShort( 0x28 ),	/* Type Offset=40 */

	/* Return value */

/* 206 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 208 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 210 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_ipv4 */

/* 212 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 214 */	NdrFcLong( 0x0 ),	/* 0 */
/* 218 */	NdrFcShort( 0xb ),	/* 11 */
/* 220 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 222 */	NdrFcShort( 0x0 ),	/* 0 */
/* 224 */	NdrFcShort( 0x8 ),	/* 8 */
/* 226 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 228 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 230 */	NdrFcShort( 0x0 ),	/* 0 */
/* 232 */	NdrFcShort( 0x1 ),	/* 1 */
/* 234 */	NdrFcShort( 0x0 ),	/* 0 */
/* 236 */	NdrFcShort( 0x2 ),	/* 2 */
/* 238 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 240 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 242 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 244 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 246 */	NdrFcShort( 0x36 ),	/* Type Offset=54 */

	/* Return value */

/* 248 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 250 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 252 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_ipv6 */

/* 254 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 256 */	NdrFcLong( 0x0 ),	/* 0 */
/* 260 */	NdrFcShort( 0xc ),	/* 12 */
/* 262 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 264 */	NdrFcShort( 0x0 ),	/* 0 */
/* 266 */	NdrFcShort( 0x8 ),	/* 8 */
/* 268 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 270 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 272 */	NdrFcShort( 0x1 ),	/* 1 */
/* 274 */	NdrFcShort( 0x0 ),	/* 0 */
/* 276 */	NdrFcShort( 0x0 ),	/* 0 */
/* 278 */	NdrFcShort( 0x2 ),	/* 2 */
/* 280 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 282 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 284 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 286 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 288 */	NdrFcShort( 0x28 ),	/* Type Offset=40 */

	/* Return value */

/* 290 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 292 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 294 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_ipv6 */

/* 296 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 298 */	NdrFcLong( 0x0 ),	/* 0 */
/* 302 */	NdrFcShort( 0xd ),	/* 13 */
/* 304 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 306 */	NdrFcShort( 0x0 ),	/* 0 */
/* 308 */	NdrFcShort( 0x8 ),	/* 8 */
/* 310 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 312 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 314 */	NdrFcShort( 0x0 ),	/* 0 */
/* 316 */	NdrFcShort( 0x1 ),	/* 1 */
/* 318 */	NdrFcShort( 0x0 ),	/* 0 */
/* 320 */	NdrFcShort( 0x2 ),	/* 2 */
/* 322 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 324 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 326 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 328 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 330 */	NdrFcShort( 0x36 ),	/* Type Offset=54 */

	/* Return value */

/* 332 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 334 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 336 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure get_peerTag */

/* 338 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 340 */	NdrFcLong( 0x0 ),	/* 0 */
/* 344 */	NdrFcShort( 0xe ),	/* 14 */
/* 346 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 348 */	NdrFcShort( 0x0 ),	/* 0 */
/* 350 */	NdrFcShort( 0x24 ),	/* 36 */
/* 352 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x3,		/* 3 */
/* 354 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 356 */	NdrFcShort( 0x1 ),	/* 1 */
/* 358 */	NdrFcShort( 0x0 ),	/* 0 */
/* 360 */	NdrFcShort( 0x0 ),	/* 0 */
/* 362 */	NdrFcShort( 0x3 ),	/* 3 */
/* 364 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 366 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter ____returnValueSize */

/* 368 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 370 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 372 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 374 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 376 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 378 */	NdrFcShort( 0x44 ),	/* Type Offset=68 */

	/* Return value */

/* 380 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 382 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 384 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure put_peerTag */

/* 386 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 388 */	NdrFcLong( 0x0 ),	/* 0 */
/* 392 */	NdrFcShort( 0xf ),	/* 15 */
/* 394 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 396 */	NdrFcShort( 0x8 ),	/* 8 */
/* 398 */	NdrFcShort( 0x8 ),	/* 8 */
/* 400 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x3,		/* 3 */
/* 402 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 404 */	NdrFcShort( 0x0 ),	/* 0 */
/* 406 */	NdrFcShort( 0x1 ),	/* 1 */
/* 408 */	NdrFcShort( 0x0 ),	/* 0 */
/* 410 */	NdrFcShort( 0x3 ),	/* 3 */
/* 412 */	0x3,		/* 3 */
			0x80,		/* 128 */
/* 414 */	0x81,		/* 129 */
			0x82,		/* 130 */

	/* Parameter ____set_formalSize */

/* 416 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 418 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 420 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter __set_formal */

/* 422 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 424 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 426 */	NdrFcShort( 0x5c ),	/* Type Offset=92 */

	/* Return value */

/* 428 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 430 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 432 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnCallStateChanged */

/* 434 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 436 */	NdrFcLong( 0x0 ),	/* 0 */
/* 440 */	NdrFcShort( 0x6 ),	/* 6 */
/* 442 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 444 */	NdrFcShort( 0x8 ),	/* 8 */
/* 446 */	NdrFcShort( 0x8 ),	/* 8 */
/* 448 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 450 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 452 */	NdrFcShort( 0x0 ),	/* 0 */
/* 454 */	NdrFcShort( 0x0 ),	/* 0 */
/* 456 */	NdrFcShort( 0x0 ),	/* 0 */
/* 458 */	NdrFcShort( 0x2 ),	/* 2 */
/* 460 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 462 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter newState */

/* 464 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 466 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 468 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 470 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 472 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 474 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Start */

/* 476 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 478 */	NdrFcLong( 0x0 ),	/* 0 */
/* 482 */	NdrFcShort( 0x6 ),	/* 6 */
/* 484 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 486 */	NdrFcShort( 0x0 ),	/* 0 */
/* 488 */	NdrFcShort( 0x8 ),	/* 8 */
/* 490 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 492 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 494 */	NdrFcShort( 0x0 ),	/* 0 */
/* 496 */	NdrFcShort( 0x0 ),	/* 0 */
/* 498 */	NdrFcShort( 0x0 ),	/* 0 */
/* 500 */	NdrFcShort( 0x1 ),	/* 1 */
/* 502 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */

/* 504 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 506 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 508 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Connect */

/* 510 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 512 */	NdrFcLong( 0x0 ),	/* 0 */
/* 516 */	NdrFcShort( 0x7 ),	/* 7 */
/* 518 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 520 */	NdrFcShort( 0x0 ),	/* 0 */
/* 522 */	NdrFcShort( 0x8 ),	/* 8 */
/* 524 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x1,		/* 1 */
/* 526 */	0xc,		/* 12 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 528 */	NdrFcShort( 0x0 ),	/* 0 */
/* 530 */	NdrFcShort( 0x0 ),	/* 0 */
/* 532 */	NdrFcShort( 0x0 ),	/* 0 */
/* 534 */	NdrFcShort( 0x1 ),	/* 1 */
/* 536 */	0x1,		/* 1 */
			0x80,		/* 128 */

	/* Return value */

/* 538 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 540 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 542 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetPublicEndpoints */

/* 544 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 546 */	NdrFcLong( 0x0 ),	/* 0 */
/* 550 */	NdrFcShort( 0x8 ),	/* 8 */
/* 552 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 554 */	NdrFcShort( 0xd ),	/* 13 */
/* 556 */	NdrFcShort( 0x8 ),	/* 8 */
/* 558 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x4,		/* 4 */
/* 560 */	0x10,		/* 16 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 562 */	NdrFcShort( 0x0 ),	/* 0 */
/* 564 */	NdrFcShort( 0x1 ),	/* 1 */
/* 566 */	NdrFcShort( 0x0 ),	/* 0 */
/* 568 */	NdrFcShort( 0x4 ),	/* 4 */
/* 570 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 572 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 574 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter __endpointsSize */

/* 576 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 578 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 580 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter endpoints */

/* 582 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 584 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 586 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter allowP2P */

/* 588 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 590 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 592 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 594 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 596 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 598 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetNetworkType */

/* 600 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 602 */	NdrFcLong( 0x0 ),	/* 0 */
/* 606 */	NdrFcShort( 0x9 ),	/* 9 */
/* 608 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 610 */	NdrFcShort( 0x8 ),	/* 8 */
/* 612 */	NdrFcShort( 0x8 ),	/* 8 */
/* 614 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 616 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 618 */	NdrFcShort( 0x0 ),	/* 0 */
/* 620 */	NdrFcShort( 0x0 ),	/* 0 */
/* 622 */	NdrFcShort( 0x0 ),	/* 0 */
/* 624 */	NdrFcShort( 0x2 ),	/* 2 */
/* 626 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 628 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter type */

/* 630 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 632 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 634 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 636 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 638 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 640 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetStateCallback */

/* 642 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 644 */	NdrFcLong( 0x0 ),	/* 0 */
/* 648 */	NdrFcShort( 0xa ),	/* 10 */
/* 650 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 652 */	NdrFcShort( 0x0 ),	/* 0 */
/* 654 */	NdrFcShort( 0x8 ),	/* 8 */
/* 656 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 658 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 660 */	NdrFcShort( 0x0 ),	/* 0 */
/* 662 */	NdrFcShort( 0x0 ),	/* 0 */
/* 664 */	NdrFcShort( 0x0 ),	/* 0 */
/* 666 */	NdrFcShort( 0x2 ),	/* 2 */
/* 668 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 670 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter callback */

/* 672 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 674 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 676 */	NdrFcShort( 0x94 ),	/* Type Offset=148 */

	/* Return value */

/* 678 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 680 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 682 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetMicMute */

/* 684 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 686 */	NdrFcLong( 0x0 ),	/* 0 */
/* 690 */	NdrFcShort( 0xb ),	/* 11 */
/* 692 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 694 */	NdrFcShort( 0x5 ),	/* 5 */
/* 696 */	NdrFcShort( 0x8 ),	/* 8 */
/* 698 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 700 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 702 */	NdrFcShort( 0x0 ),	/* 0 */
/* 704 */	NdrFcShort( 0x0 ),	/* 0 */
/* 706 */	NdrFcShort( 0x0 ),	/* 0 */
/* 708 */	NdrFcShort( 0x2 ),	/* 2 */
/* 710 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 712 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter mute */

/* 714 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 716 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 718 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 720 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 722 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 724 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetEncryptionKey */

/* 726 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 728 */	NdrFcLong( 0x0 ),	/* 0 */
/* 732 */	NdrFcShort( 0xc ),	/* 12 */
/* 734 */	NdrFcShort( 0x14 ),	/* ARM Stack size/offset = 20 */
/* 736 */	NdrFcShort( 0xd ),	/* 13 */
/* 738 */	NdrFcShort( 0x8 ),	/* 8 */
/* 740 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x4,		/* 4 */
/* 742 */	0x10,		/* 16 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 744 */	NdrFcShort( 0x0 ),	/* 0 */
/* 746 */	NdrFcShort( 0x1 ),	/* 1 */
/* 748 */	NdrFcShort( 0x0 ),	/* 0 */
/* 750 */	NdrFcShort( 0x4 ),	/* 4 */
/* 752 */	0x4,		/* 4 */
			0x80,		/* 128 */
/* 754 */	0x81,		/* 129 */
			0x82,		/* 130 */
/* 756 */	0x83,		/* 131 */
			0x0,		/* 0 */

	/* Parameter __keySize */

/* 758 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 760 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 762 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter key */

/* 764 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 766 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 768 */	NdrFcShort( 0x5c ),	/* Type Offset=92 */

	/* Parameter isOutgoing */

/* 770 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 772 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 774 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 776 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 778 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 780 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetConfig */

/* 782 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 784 */	NdrFcLong( 0x0 ),	/* 0 */
/* 788 */	NdrFcShort( 0xd ),	/* 13 */
/* 790 */	NdrFcShort( 0x34 ),	/* ARM Stack size/offset = 52 */
/* 792 */	NdrFcShort( 0x37 ),	/* 55 */
/* 794 */	NdrFcShort( 0x8 ),	/* 8 */
/* 796 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x9,		/* 9 */
/* 798 */	0x18,		/* 24 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 800 */	NdrFcShort( 0x0 ),	/* 0 */
/* 802 */	NdrFcShort( 0x1 ),	/* 1 */
/* 804 */	NdrFcShort( 0x0 ),	/* 0 */
/* 806 */	NdrFcShort( 0xc ),	/* 12 */
/* 808 */	0xc,		/* 12 */
			0x80,		/* 128 */
/* 810 */	0x9f,		/* 159 */
			0x84,		/* 132 */
/* 812 */	0x85,		/* 133 */
			0x86,		/* 134 */
/* 814 */	0x87,		/* 135 */
			0x81,		/* 129 */
/* 816 */	0x82,		/* 130 */
			0x83,		/* 131 */
/* 818 */	0xf7,		/* 247 */
			0xf7,		/* 247 */
/* 820 */	0xf7,		/* 247 */
			0x0,		/* 0 */

	/* Parameter initTimeout */

/* 822 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 824 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 826 */	0xc,		/* FC_DOUBLE */
			0x0,		/* 0 */

	/* Parameter recvTimeout */

/* 828 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 830 */	NdrFcShort( 0x10 ),	/* ARM Stack size/offset = 16 */
/* 832 */	0xc,		/* FC_DOUBLE */
			0x0,		/* 0 */

	/* Parameter dataSavingMode */

/* 834 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 836 */	NdrFcShort( 0x18 ),	/* ARM Stack size/offset = 24 */
/* 838 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Parameter enableAEC */

/* 840 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 842 */	NdrFcShort( 0x1c ),	/* ARM Stack size/offset = 28 */
/* 844 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Parameter enableNS */

/* 846 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 848 */	NdrFcShort( 0x20 ),	/* ARM Stack size/offset = 32 */
/* 850 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Parameter enableAGC */

/* 852 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 854 */	NdrFcShort( 0x24 ),	/* ARM Stack size/offset = 36 */
/* 856 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Parameter logFilePath */

/* 858 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 860 */	NdrFcShort( 0x28 ),	/* ARM Stack size/offset = 40 */
/* 862 */	NdrFcShort( 0x36 ),	/* Type Offset=54 */

	/* Parameter statsDumpFilePath */

/* 864 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 866 */	NdrFcShort( 0x2c ),	/* ARM Stack size/offset = 44 */
/* 868 */	NdrFcShort( 0x36 ),	/* Type Offset=54 */

	/* Return value */

/* 870 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 872 */	NdrFcShort( 0x30 ),	/* ARM Stack size/offset = 48 */
/* 874 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetDebugString */

/* 876 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 878 */	NdrFcLong( 0x0 ),	/* 0 */
/* 882 */	NdrFcShort( 0xe ),	/* 14 */
/* 884 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 886 */	NdrFcShort( 0x0 ),	/* 0 */
/* 888 */	NdrFcShort( 0x8 ),	/* 8 */
/* 890 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 892 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 894 */	NdrFcShort( 0x1 ),	/* 1 */
/* 896 */	NdrFcShort( 0x0 ),	/* 0 */
/* 898 */	NdrFcShort( 0x0 ),	/* 0 */
/* 900 */	NdrFcShort( 0x2 ),	/* 2 */
/* 902 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 904 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 906 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 908 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 910 */	NdrFcShort( 0x28 ),	/* Type Offset=40 */

	/* Return value */

/* 912 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 914 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 916 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetDebugLog */

/* 918 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 920 */	NdrFcLong( 0x0 ),	/* 0 */
/* 924 */	NdrFcShort( 0xf ),	/* 15 */
/* 926 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 928 */	NdrFcShort( 0x0 ),	/* 0 */
/* 930 */	NdrFcShort( 0x8 ),	/* 8 */
/* 932 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 934 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 936 */	NdrFcShort( 0x1 ),	/* 1 */
/* 938 */	NdrFcShort( 0x0 ),	/* 0 */
/* 940 */	NdrFcShort( 0x0 ),	/* 0 */
/* 942 */	NdrFcShort( 0x2 ),	/* 2 */
/* 944 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 946 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 948 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 950 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 952 */	NdrFcShort( 0x28 ),	/* Type Offset=40 */

	/* Return value */

/* 954 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 956 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 958 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetLastError */

/* 960 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 962 */	NdrFcLong( 0x0 ),	/* 0 */
/* 966 */	NdrFcShort( 0x10 ),	/* 16 */
/* 968 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 970 */	NdrFcShort( 0x0 ),	/* 0 */
/* 972 */	NdrFcShort( 0x24 ),	/* 36 */
/* 974 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 976 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 978 */	NdrFcShort( 0x0 ),	/* 0 */
/* 980 */	NdrFcShort( 0x0 ),	/* 0 */
/* 982 */	NdrFcShort( 0x0 ),	/* 0 */
/* 984 */	NdrFcShort( 0x2 ),	/* 2 */
/* 986 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 988 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 990 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 992 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 994 */	0xe,		/* FC_ENUM32 */
			0x0,		/* 0 */

	/* Return value */

/* 996 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 998 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1000 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetPreferredRelayID */

/* 1002 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1004 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1008 */	NdrFcShort( 0x11 ),	/* 17 */
/* 1010 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1012 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1014 */	NdrFcShort( 0x2c ),	/* 44 */
/* 1016 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1018 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1020 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1022 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1024 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1026 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1028 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1030 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 1032 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1034 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1036 */	0xb,		/* FC_HYPER */
			0x0,		/* 0 */

	/* Return value */

/* 1038 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1040 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1042 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetVersion */

/* 1044 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1046 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1050 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1052 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1054 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1056 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1058 */	0x45,		/* Oi2 Flags:  srv must size, has return, has ext, */
			0x2,		/* 2 */
/* 1060 */	0xe,		/* 14 */
			0x3,		/* Ext Flags:  new corr desc, clt corr check, */
/* 1062 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1064 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1066 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1068 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1070 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1072 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter __returnValue */

/* 1074 */	NdrFcShort( 0x2113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=8 */
/* 1076 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1078 */	NdrFcShort( 0x28 ),	/* Type Offset=40 */

	/* Return value */

/* 1080 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1082 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1084 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure UpdateServerConfig */

/* 1086 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1088 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1092 */	NdrFcShort( 0x7 ),	/* 7 */
/* 1094 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1096 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1098 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1100 */	0x46,		/* Oi2 Flags:  clt must size, has return, has ext, */
			0x2,		/* 2 */
/* 1102 */	0xe,		/* 14 */
			0x5,		/* Ext Flags:  new corr desc, srv corr check, */
/* 1104 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1106 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1108 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1110 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1112 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1114 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter json */

/* 1116 */	NdrFcShort( 0x8b ),	/* Flags:  must size, must free, in, by val, */
/* 1118 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1120 */	NdrFcShort( 0x36 ),	/* Type Offset=54 */

	/* Return value */

/* 1122 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1124 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1126 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SwitchSpeaker */

/* 1128 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1130 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1134 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1136 */	NdrFcShort( 0xc ),	/* ARM Stack size/offset = 12 */
/* 1138 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1140 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1142 */	0x44,		/* Oi2 Flags:  has return, has ext, */
			0x2,		/* 2 */
/* 1144 */	0xe,		/* 14 */
			0x1,		/* Ext Flags:  new corr desc, */
/* 1146 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1148 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1150 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1152 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1154 */	0x2,		/* 2 */
			0x80,		/* 128 */
/* 1156 */	0x81,		/* 129 */
			0x0,		/* 0 */

	/* Parameter external */

/* 1158 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1160 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 1162 */	0x3,		/* FC_SMALL */
			0x0,		/* 0 */

	/* Return value */

/* 1164 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1166 */	NdrFcShort( 0x8 ),	/* ARM Stack size/offset = 8 */
/* 1168 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const libtgvoip_MIDL_TYPE_FORMAT_STRING libtgvoip__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/*  4 */	0xb,		/* FC_HYPER */
			0x5c,		/* FC_PAD */
/*  6 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/*  8 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 10 */	
			0x11, 0x4,	/* FC_RP [alloced_on_stack] */
/* 12 */	NdrFcShort( 0x1c ),	/* Offset= 28 (40) */
/* 14 */	
			0x13, 0x0,	/* FC_OP */
/* 16 */	NdrFcShort( 0xe ),	/* Offset= 14 (30) */
/* 18 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 20 */	NdrFcShort( 0x2 ),	/* 2 */
/* 22 */	0x9,		/* Corr desc: FC_ULONG */
			0x0,		/*  */
/* 24 */	NdrFcShort( 0xfffc ),	/* -4 */
/* 26 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 28 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 30 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 32 */	NdrFcShort( 0x8 ),	/* 8 */
/* 34 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (18) */
/* 36 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 38 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 40 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 42 */	NdrFcShort( 0x0 ),	/* 0 */
/* 44 */	NdrFcShort( 0x4 ),	/* 4 */
/* 46 */	NdrFcShort( 0x0 ),	/* 0 */
/* 48 */	NdrFcShort( 0xffde ),	/* Offset= -34 (14) */
/* 50 */	
			0x12, 0x0,	/* FC_UP */
/* 52 */	NdrFcShort( 0xffea ),	/* Offset= -22 (30) */
/* 54 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 56 */	NdrFcShort( 0x0 ),	/* 0 */
/* 58 */	NdrFcShort( 0x4 ),	/* 4 */
/* 60 */	NdrFcShort( 0x0 ),	/* 0 */
/* 62 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (50) */
/* 64 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 66 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 68 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 70 */	NdrFcShort( 0x2 ),	/* Offset= 2 (72) */
/* 72 */	
			0x13, 0x0,	/* FC_OP */
/* 74 */	NdrFcShort( 0x2 ),	/* Offset= 2 (76) */
/* 76 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 78 */	NdrFcShort( 0x1 ),	/* 1 */
/* 80 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 82 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 84 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 86 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 88 */	
			0x11, 0x0,	/* FC_RP */
/* 90 */	NdrFcShort( 0x2 ),	/* Offset= 2 (92) */
/* 92 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 94 */	NdrFcShort( 0x1 ),	/* 1 */
/* 96 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 98 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 100 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 102 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 104 */	
			0x11, 0x0,	/* FC_RP */
/* 106 */	NdrFcShort( 0x14 ),	/* Offset= 20 (126) */
/* 108 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 110 */	NdrFcLong( 0x95221757 ),	/* -1792927913 */
/* 114 */	NdrFcShort( 0xdccc ),	/* -9012 */
/* 116 */	NdrFcShort( 0x3c3b ),	/* 15419 */
/* 118 */	0xa5,		/* 165 */
			0xaf,		/* 175 */
/* 120 */	0x51,		/* 81 */
			0x4f,		/* 79 */
/* 122 */	0xc2,		/* 194 */
			0x8,		/* 8 */
/* 124 */	0xea,		/* 234 */
			0xf2,		/* 242 */
/* 126 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 128 */	NdrFcShort( 0x0 ),	/* 0 */
/* 130 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 132 */	NdrFcShort( 0x4 ),	/* ARM Stack size/offset = 4 */
/* 134 */	NdrFcShort( 0x1 ),	/* Corr flags:  early, */
/* 136 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 140 */	NdrFcShort( 0x0 ),	/* Corr flags:  */
/* 142 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 144 */	NdrFcShort( 0xffdc ),	/* Offset= -36 (108) */
/* 146 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 148 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 150 */	NdrFcLong( 0xa11cb322 ),	/* -1591954654 */
/* 154 */	NdrFcShort( 0x951e ),	/* -27362 */
/* 156 */	NdrFcShort( 0x3651 ),	/* 13905 */
/* 158 */	0x8a,		/* 138 */
			0xa4,		/* 164 */
/* 160 */	0xef,		/* 239 */
			0x42,		/* 66 */
/* 162 */	0xd1,		/* 209 */
			0xe1,		/* 225 */
/* 164 */	0x42,		/* 66 */
			0xf3,		/* 243 */
/* 166 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 168 */	0xe,		/* FC_ENUM32 */
			0x5c,		/* FC_PAD */

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



/* Standard interface: __MIDL_itf_libtgvoip_0000_0000, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IUnknown, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: IInspectable, ver. 0.0,
   GUID={0xAF86E2E0,0xB12D,0x4c6a,{0x9C,0x5A,0xD7,0xAA,0x65,0x10,0x1E,0x90}} */


/* Object interface: __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals, ver. 0.0,
   GUID={0x95221757,0xDCCC,0x3C3B,{0xA5,0xAF,0x51,0x4F,0xC2,0x08,0xEA,0xF2}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    0,
    42,
    86,
    128,
    170,
    212,
    254,
    296,
    338,
    386
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    libtgvoip__MIDL_ProcFormatString.Format,
    &__x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    libtgvoip__MIDL_ProcFormatString.Format,
    &__x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(16) ___x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtualsProxyVtbl = 
{
    &__x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_ProxyInfo,
    &IID___x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::get_id */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::put_id */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::get_port */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::put_port */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::get_ipv4 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::put_ipv4 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::get_ipv6 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::put_ipv6 */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::get_peerTag */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals::put_peerTag */
};


static const PRPC_STUB_FUNCTION __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_table[] =
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

CInterfaceStubVtbl ___x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtualsStubVtbl =
{
    &IID___x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals,
    &__x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_ServerInfo,
    16,
    &__x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_libtgvoip_0000_0001, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_Clibtgvoip_CIStateCallback, ver. 0.0,
   GUID={0xA11CB322,0x951E,0x3651,{0x8A,0xA4,0xEF,0x42,0xD1,0xE1,0x42,0xF3}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_Clibtgvoip_CIStateCallback_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    434
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_Clibtgvoip_CIStateCallback_ProxyInfo =
    {
    &Object_StubDesc,
    libtgvoip__MIDL_ProcFormatString.Format,
    &__x_ABI_Clibtgvoip_CIStateCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_Clibtgvoip_CIStateCallback_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    libtgvoip__MIDL_ProcFormatString.Format,
    &__x_ABI_Clibtgvoip_CIStateCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) ___x_ABI_Clibtgvoip_CIStateCallbackProxyVtbl = 
{
    &__x_ABI_Clibtgvoip_CIStateCallback_ProxyInfo,
    &IID___x_ABI_Clibtgvoip_CIStateCallback,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_CIStateCallback::OnCallStateChanged */
};


static const PRPC_STUB_FUNCTION __x_ABI_Clibtgvoip_CIStateCallback_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_Clibtgvoip_CIStateCallbackStubVtbl =
{
    &IID___x_ABI_Clibtgvoip_CIStateCallback,
    &__x_ABI_Clibtgvoip_CIStateCallback_ServerInfo,
    7,
    &__x_ABI_Clibtgvoip_CIStateCallback_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_libtgvoip_0000_0002, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals, ver. 0.0,
   GUID={0xA4312674,0xFAEC,0x3E92,{0xB8,0x4B,0x63,0x1A,0x08,0xBE,0xA9,0x63}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    476,
    510,
    544,
    600,
    642,
    684,
    726,
    782,
    876,
    918,
    960,
    1002
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_ProxyInfo =
    {
    &Object_StubDesc,
    libtgvoip__MIDL_ProcFormatString.Format,
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    libtgvoip__MIDL_ProcFormatString.Format,
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(18) ___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtualsProxyVtbl = 
{
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_ProxyInfo,
    &IID___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::Start */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::Connect */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::SetPublicEndpoints */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::SetNetworkType */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::SetStateCallback */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::SetMicMute */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::SetEncryptionKey */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::SetConfig */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::GetDebugString */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::GetDebugLog */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::GetLastError */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals::GetPreferredRelayID */
};


static const PRPC_STUB_FUNCTION __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_table[] =
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
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtualsStubVtbl =
{
    &IID___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals,
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_ServerInfo,
    18,
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_libtgvoip_0000_0003, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics, ver. 0.0,
   GUID={0xB855DB3B,0x9FD9,0x3215,{0x8F,0x33,0x73,0x17,0x3C,0x2A,0xD6,0xCD}} */

#pragma code_seg(".orpc")
static const unsigned short __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_FormatStringOffsetTable[] =
    {
    (unsigned short) -1,
    (unsigned short) -1,
    (unsigned short) -1,
    1044,
    1086,
    1128
    };

static const MIDL_STUBLESS_PROXY_INFO __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_ProxyInfo =
    {
    &Object_StubDesc,
    libtgvoip__MIDL_ProcFormatString.Format,
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    libtgvoip__MIDL_ProcFormatString.Format,
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(9) ___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStaticsProxyVtbl = 
{
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_ProxyInfo,
    &IID___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    0 /* IInspectable::GetIids */ ,
    0 /* IInspectable::GetRuntimeClassName */ ,
    0 /* IInspectable::GetTrustLevel */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics::GetVersion */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics::UpdateServerConfig */ ,
    (void *) (INT_PTR) -1 /* __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics::SwitchSpeaker */
};


static const PRPC_STUB_FUNCTION __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_table[] =
{
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    STUB_FORWARDING_FUNCTION,
    NdrStubCall2,
    NdrStubCall2,
    NdrStubCall2
};

CInterfaceStubVtbl ___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStaticsStubVtbl =
{
    &IID___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics,
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_ServerInfo,
    9,
    &__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_table[-3],
    CStdStubBuffer_DELEGATING_METHODS
};


/* Standard interface: __MIDL_itf_libtgvoip_0000_0004, ver. 0.0,
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
    libtgvoip__MIDL_TypeFormatString.Format,
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

const CInterfaceProxyVtbl * const _libtgvoip_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &___x_ABI_Clibtgvoip_CIStateCallbackProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStaticsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtualsProxyVtbl,
    ( CInterfaceProxyVtbl *) &___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtualsProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _libtgvoip_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &___x_ABI_Clibtgvoip_CIStateCallbackStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStaticsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtualsStubVtbl,
    ( CInterfaceStubVtbl *) &___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtualsStubVtbl,
    0
};

PCInterfaceName const _libtgvoip_InterfaceNamesList[] = 
{
    "__x_ABI_Clibtgvoip_CIStateCallback",
    "__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics",
    "__x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals",
    "__x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals",
    0
};

const IID *  const _libtgvoip_BaseIIDList[] = 
{
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    &IID_IInspectable,
    0
};


#define _libtgvoip_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _libtgvoip, pIID, n)

int __stdcall _libtgvoip_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _libtgvoip, 4, 2 )
    IID_BS_LOOKUP_NEXT_TEST( _libtgvoip, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _libtgvoip, 4, *pIndex )
    
}

const ExtendedProxyFileInfo libtgvoip_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _libtgvoip_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _libtgvoip_StubVtblList,
    (const PCInterfaceName * ) & _libtgvoip_InterfaceNamesList,
    (const IID ** ) & _libtgvoip_BaseIIDList,
    & _libtgvoip_IID_Lookup, 
    4,
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

