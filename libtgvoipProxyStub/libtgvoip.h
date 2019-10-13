

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


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


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __libtgvoip_h__
#define __libtgvoip_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef ____x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_FWD_DEFINED__
#define ____x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_FWD_DEFINED__
typedef interface __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals;

#ifdef __cplusplus
namespace ABI {
    namespace libtgvoip {
        interface __IEndpointPublicNonVirtuals;
    } /* end namespace */
} /* end namespace */

#endif /* __cplusplus */

#endif 	/* ____x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_FWD_DEFINED__ */


#ifndef ____x_ABI_Clibtgvoip_CIStateCallback_FWD_DEFINED__
#define ____x_ABI_Clibtgvoip_CIStateCallback_FWD_DEFINED__
typedef interface __x_ABI_Clibtgvoip_CIStateCallback __x_ABI_Clibtgvoip_CIStateCallback;

#ifdef __cplusplus
namespace ABI {
    namespace libtgvoip {
        interface IStateCallback;
    } /* end namespace */
} /* end namespace */

#endif /* __cplusplus */

#endif 	/* ____x_ABI_Clibtgvoip_CIStateCallback_FWD_DEFINED__ */


#ifndef ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_FWD_DEFINED__
#define ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_FWD_DEFINED__
typedef interface __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals;

#ifdef __cplusplus
namespace ABI {
    namespace libtgvoip {
        interface __IVoIPControllerWrapperPublicNonVirtuals;
    } /* end namespace */
} /* end namespace */

#endif /* __cplusplus */

#endif 	/* ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_FWD_DEFINED__ */


#ifndef ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_FWD_DEFINED__
#define ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_FWD_DEFINED__
typedef interface __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics;

#ifdef __cplusplus
namespace ABI {
    namespace libtgvoip {
        interface __IVoIPControllerWrapperStatics;
    } /* end namespace */
} /* end namespace */

#endif /* __cplusplus */

#endif 	/* ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_FWD_DEFINED__ */


/* header files for imported files */
#include "inspectable.h"
#include "AsyncInfo.h"
#include "EventToken.h"
#include "Windows.Foundation.h"

#ifdef __cplusplus
extern "C"{
#endif 


/* interface __MIDL_itf_libtgvoip_0000_0000 */
/* [local] */ 

#if defined(__cplusplus)
}
#endif // defined(__cplusplus)
#include <Windows.Foundation.h>
#if defined(__cplusplus)
extern "C" {
#endif // defined(__cplusplus)
#if !defined(__cplusplus)
#if !defined(__cplusplus)

typedef enum __x_ABI_Clibtgvoip_CCallState __x_ABI_Clibtgvoip_CCallState;


#endif /* end if !defined(__cplusplus) */


#endif
#if !defined(__cplusplus)
#if !defined(__cplusplus)

typedef enum __x_ABI_Clibtgvoip_CError __x_ABI_Clibtgvoip_CError;


#endif /* end if !defined(__cplusplus) */


#endif
#if !defined(__cplusplus)
#if !defined(__cplusplus)

typedef enum __x_ABI_Clibtgvoip_CNetworkType __x_ABI_Clibtgvoip_CNetworkType;


#endif /* end if !defined(__cplusplus) */


#endif
#if !defined(__cplusplus)
#if !defined(__cplusplus)

typedef enum __x_ABI_Clibtgvoip_CDataSavingMode __x_ABI_Clibtgvoip_CDataSavingMode;


#endif /* end if !defined(__cplusplus) */


#endif




#ifdef __cplusplus
namespace ABI {
namespace libtgvoip {
class Endpoint;
} /*libtgvoip*/
}
#endif
#ifdef __cplusplus
namespace ABI {
namespace libtgvoip {
class VoIPControllerWrapper;
} /*libtgvoip*/
}
#endif
#if !defined(__cplusplus)

#if !defined(__cplusplus)
/* [v1_enum] */ 
enum __x_ABI_Clibtgvoip_CCallState
    {
        CallState_WaitInit	= 1,
        CallState_WaitInitAck	= 2,
        CallState_Established	= 3,
        CallState_Failed	= 4
    } ;
#endif /* end if !defined(__cplusplus) */

#endif
#if !defined(__cplusplus)

#if !defined(__cplusplus)
/* [v1_enum] */ 
enum __x_ABI_Clibtgvoip_CError
    {
        Error_Unknown	= 0,
        Error_Incompatible	= 1,
        Error_Timeout	= 2,
        Error_AudioIO	= 3
    } ;
#endif /* end if !defined(__cplusplus) */

#endif
#if !defined(__cplusplus)

#if !defined(__cplusplus)
/* [v1_enum] */ 
enum __x_ABI_Clibtgvoip_CNetworkType
    {
        NetworkType_Unknown	= 0,
        NetworkType_GPRS	= 1,
        NetworkType_EDGE	= 2,
        NetworkType_UMTS	= 3,
        NetworkType_HSPA	= 4,
        NetworkType_LTE	= 5,
        NetworkType_WiFi	= 6,
        NetworkType_Ethernet	= 7,
        NetworkType_OtherHighSpeed	= 8,
        NetworkType_OtherLowSpeed	= 9,
        NetworkType_Dialup	= 10,
        NetworkType_OtherMobile	= 11
    } ;
#endif /* end if !defined(__cplusplus) */

#endif
#if !defined(__cplusplus)

#if !defined(__cplusplus)
/* [v1_enum] */ 
enum __x_ABI_Clibtgvoip_CDataSavingMode
    {
        DataSavingMode_Never	= 0,
        DataSavingMode_MobileOnly	= 1,
        DataSavingMode_Always	= 2
    } ;
#endif /* end if !defined(__cplusplus) */

#endif
#if !defined(____x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_INTERFACE_DEFINED__)
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_libtgvoip___IEndpointPublicNonVirtuals[] = L"libtgvoip.__IEndpointPublicNonVirtuals";
#endif /* !defined(____x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_INTERFACE_DEFINED__) */


/* interface __MIDL_itf_libtgvoip_0000_0000 */
/* [local] */ 

#ifdef __cplusplus

} /* end extern "C" */
namespace ABI {
    namespace libtgvoip {
        
        typedef enum CallState CallState;
        
    } /* end namespace */
} /* end namespace */

extern "C" { 
#endif

#ifdef __cplusplus

} /* end extern "C" */
namespace ABI {
    namespace libtgvoip {
        
        typedef enum Error Error;
        
    } /* end namespace */
} /* end namespace */

extern "C" { 
#endif

#ifdef __cplusplus

} /* end extern "C" */
namespace ABI {
    namespace libtgvoip {
        
        typedef enum NetworkType NetworkType;
        
    } /* end namespace */
} /* end namespace */

extern "C" { 
#endif

#ifdef __cplusplus

} /* end extern "C" */
namespace ABI {
    namespace libtgvoip {
        
        typedef enum DataSavingMode DataSavingMode;
        
    } /* end namespace */
} /* end namespace */

extern "C" { 
#endif





#ifdef __cplusplus
} /* end extern "C" */
namespace ABI {
    namespace libtgvoip {
        
        /* [v1_enum] */ 
        enum CallState
            {
                CallState_WaitInit	= 1,
                CallState_WaitInitAck	= 2,
                CallState_Established	= 3,
                CallState_Failed	= 4
            } ;
    } /* end namespace */
} /* end namespace */

extern "C" { 
#endif

#ifdef __cplusplus
} /* end extern "C" */
namespace ABI {
    namespace libtgvoip {
        
        /* [v1_enum] */ 
        enum Error
            {
                Error_Unknown	= 0,
                Error_Incompatible	= 1,
                Error_Timeout	= 2,
                Error_AudioIO	= 3
            } ;
    } /* end namespace */
} /* end namespace */

extern "C" { 
#endif

#ifdef __cplusplus
} /* end extern "C" */
namespace ABI {
    namespace libtgvoip {
        
        /* [v1_enum] */ 
        enum NetworkType
            {
                NetworkType_Unknown	= 0,
                NetworkType_GPRS	= 1,
                NetworkType_EDGE	= 2,
                NetworkType_UMTS	= 3,
                NetworkType_HSPA	= 4,
                NetworkType_LTE	= 5,
                NetworkType_WiFi	= 6,
                NetworkType_Ethernet	= 7,
                NetworkType_OtherHighSpeed	= 8,
                NetworkType_OtherLowSpeed	= 9,
                NetworkType_Dialup	= 10,
                NetworkType_OtherMobile	= 11
            } ;
    } /* end namespace */
} /* end namespace */

extern "C" { 
#endif

#ifdef __cplusplus
} /* end extern "C" */
namespace ABI {
    namespace libtgvoip {
        
        /* [v1_enum] */ 
        enum DataSavingMode
            {
                DataSavingMode_Never	= 0,
                DataSavingMode_MobileOnly	= 1,
                DataSavingMode_Always	= 2
            } ;
    } /* end namespace */
} /* end namespace */

extern "C" { 
#endif



extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0000_v0_0_s_ifspec;

#ifndef ____x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_INTERFACE_DEFINED__
#define ____x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_INTERFACE_DEFINED__

/* interface __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals */
/* [uuid][object] */ 



/* interface ABI::libtgvoip::__IEndpointPublicNonVirtuals */
/* [uuid][object] */ 


EXTERN_C const IID IID___x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals;

#if defined(__cplusplus) && !defined(CINTERFACE)
    } /* end extern "C" */
    namespace ABI {
        namespace libtgvoip {
            
            MIDL_INTERFACE("95221757-DCCC-3C3B-A5AF-514FC208EAF2")
            __IEndpointPublicNonVirtuals : public IInspectable
            {
            public:
                virtual /* [propget] */ HRESULT STDMETHODCALLTYPE get_id( 
                    /* [out][retval] */ INT64 *__returnValue) = 0;
                
                virtual /* [propput] */ HRESULT STDMETHODCALLTYPE put_id( 
                    /* [in] */ INT64 __set_formal) = 0;
                
                virtual /* [propget] */ HRESULT STDMETHODCALLTYPE get_port( 
                    /* [out][retval] */ UINT16 *__returnValue) = 0;
                
                virtual /* [propput] */ HRESULT STDMETHODCALLTYPE put_port( 
                    /* [in] */ UINT16 __set_formal) = 0;
                
                virtual /* [propget] */ HRESULT STDMETHODCALLTYPE get_ipv4( 
                    /* [out][retval] */ HSTRING *__returnValue) = 0;
                
                virtual /* [propput] */ HRESULT STDMETHODCALLTYPE put_ipv4( 
                    /* [in] */ HSTRING __set_formal) = 0;
                
                virtual /* [propget] */ HRESULT STDMETHODCALLTYPE get_ipv6( 
                    /* [out][retval] */ HSTRING *__returnValue) = 0;
                
                virtual /* [propput] */ HRESULT STDMETHODCALLTYPE put_ipv6( 
                    /* [in] */ HSTRING __set_formal) = 0;
                
                virtual /* [propget] */ HRESULT STDMETHODCALLTYPE get_peerTag( 
                    /* [out] */ UINT32 *____returnValueSize,
                    /* [out][retval][size_is][size_is] */ BYTE **__returnValue) = 0;
                
                virtual /* [propput] */ HRESULT STDMETHODCALLTYPE put_peerTag( 
                    /* [in] */ UINT32 ____set_formalSize,
                    /* [in][size_is] */ BYTE *__set_formal) = 0;
                
            };

            extern const __declspec(selectany) IID & IID___IEndpointPublicNonVirtuals = __uuidof(__IEndpointPublicNonVirtuals);

            
        }  /* end namespace */
    }  /* end namespace */
    extern "C" { 
    
#else 	/* C style interface */

    typedef struct __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtualsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetIids )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [out] */ ULONG *iidCount,
            /* [size_is][size_is][out] */ IID **iids);
        
        HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [out] */ HSTRING *className);
        
        HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [out] */ TrustLevel *trustLevel);
        
        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_id )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [out][retval] */ INT64 *__returnValue);
        
        /* [propput] */ HRESULT ( STDMETHODCALLTYPE *put_id )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [in] */ INT64 __set_formal);
        
        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_port )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [out][retval] */ UINT16 *__returnValue);
        
        /* [propput] */ HRESULT ( STDMETHODCALLTYPE *put_port )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [in] */ UINT16 __set_formal);
        
        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_ipv4 )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [out][retval] */ HSTRING *__returnValue);
        
        /* [propput] */ HRESULT ( STDMETHODCALLTYPE *put_ipv4 )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [in] */ HSTRING __set_formal);
        
        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_ipv6 )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [out][retval] */ HSTRING *__returnValue);
        
        /* [propput] */ HRESULT ( STDMETHODCALLTYPE *put_ipv6 )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [in] */ HSTRING __set_formal);
        
        /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_peerTag )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [out] */ UINT32 *____returnValueSize,
            /* [out][retval][size_is][size_is] */ BYTE **__returnValue);
        
        /* [propput] */ HRESULT ( STDMETHODCALLTYPE *put_peerTag )( 
            __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals * This,
            /* [in] */ UINT32 ____set_formalSize,
            /* [in][size_is] */ BYTE *__set_formal);
        
        END_INTERFACE
    } __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtualsVtbl;

    interface __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals
    {
        CONST_VTBL struct __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtualsVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_get_id(This,__returnValue)	\
    ( (This)->lpVtbl -> get_id(This,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_put_id(This,__set_formal)	\
    ( (This)->lpVtbl -> put_id(This,__set_formal) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_get_port(This,__returnValue)	\
    ( (This)->lpVtbl -> get_port(This,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_put_port(This,__set_formal)	\
    ( (This)->lpVtbl -> put_port(This,__set_formal) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_get_ipv4(This,__returnValue)	\
    ( (This)->lpVtbl -> get_ipv4(This,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_put_ipv4(This,__set_formal)	\
    ( (This)->lpVtbl -> put_ipv4(This,__set_formal) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_get_ipv6(This,__returnValue)	\
    ( (This)->lpVtbl -> get_ipv6(This,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_put_ipv6(This,__set_formal)	\
    ( (This)->lpVtbl -> put_ipv6(This,__set_formal) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_get_peerTag(This,____returnValueSize,__returnValue)	\
    ( (This)->lpVtbl -> get_peerTag(This,____returnValueSize,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_put_peerTag(This,____set_formalSize,__set_formal)	\
    ( (This)->lpVtbl -> put_peerTag(This,____set_formalSize,__set_formal) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* ____x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_libtgvoip_0000_0001 */
/* [local] */ 

#if !defined(____x_ABI_Clibtgvoip_CIStateCallback_INTERFACE_DEFINED__)
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_libtgvoip_IStateCallback[] = L"libtgvoip.IStateCallback";
#endif /* !defined(____x_ABI_Clibtgvoip_CIStateCallback_INTERFACE_DEFINED__) */


/* interface __MIDL_itf_libtgvoip_0000_0001 */
/* [local] */ 



extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0001_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0001_v0_0_s_ifspec;

#ifndef ____x_ABI_Clibtgvoip_CIStateCallback_INTERFACE_DEFINED__
#define ____x_ABI_Clibtgvoip_CIStateCallback_INTERFACE_DEFINED__

/* interface __x_ABI_Clibtgvoip_CIStateCallback */
/* [uuid][object] */ 



/* interface ABI::libtgvoip::IStateCallback */
/* [uuid][object] */ 


EXTERN_C const IID IID___x_ABI_Clibtgvoip_CIStateCallback;

#if defined(__cplusplus) && !defined(CINTERFACE)
    } /* end extern "C" */
    namespace ABI {
        namespace libtgvoip {
            
            MIDL_INTERFACE("A11CB322-951E-3651-8AA4-EF42D1E142F3")
            IStateCallback : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE OnCallStateChanged( 
                    /* [in] */ ABI::libtgvoip::CallState newState) = 0;
                
            };

            extern const __declspec(selectany) IID & IID_IStateCallback = __uuidof(IStateCallback);

            
        }  /* end namespace */
    }  /* end namespace */
    extern "C" { 
    
#else 	/* C style interface */

    typedef struct __x_ABI_Clibtgvoip_CIStateCallbackVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            __x_ABI_Clibtgvoip_CIStateCallback * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            __x_ABI_Clibtgvoip_CIStateCallback * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            __x_ABI_Clibtgvoip_CIStateCallback * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetIids )( 
            __x_ABI_Clibtgvoip_CIStateCallback * This,
            /* [out] */ ULONG *iidCount,
            /* [size_is][size_is][out] */ IID **iids);
        
        HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
            __x_ABI_Clibtgvoip_CIStateCallback * This,
            /* [out] */ HSTRING *className);
        
        HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
            __x_ABI_Clibtgvoip_CIStateCallback * This,
            /* [out] */ TrustLevel *trustLevel);
        
        HRESULT ( STDMETHODCALLTYPE *OnCallStateChanged )( 
            __x_ABI_Clibtgvoip_CIStateCallback * This,
            /* [in] */ __x_ABI_Clibtgvoip_CCallState newState);
        
        END_INTERFACE
    } __x_ABI_Clibtgvoip_CIStateCallbackVtbl;

    interface __x_ABI_Clibtgvoip_CIStateCallback
    {
        CONST_VTBL struct __x_ABI_Clibtgvoip_CIStateCallbackVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define __x_ABI_Clibtgvoip_CIStateCallback_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __x_ABI_Clibtgvoip_CIStateCallback_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __x_ABI_Clibtgvoip_CIStateCallback_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __x_ABI_Clibtgvoip_CIStateCallback_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __x_ABI_Clibtgvoip_CIStateCallback_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __x_ABI_Clibtgvoip_CIStateCallback_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __x_ABI_Clibtgvoip_CIStateCallback_OnCallStateChanged(This,newState)	\
    ( (This)->lpVtbl -> OnCallStateChanged(This,newState) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* ____x_ABI_Clibtgvoip_CIStateCallback_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_libtgvoip_0000_0002 */
/* [local] */ 

#if !defined(____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_INTERFACE_DEFINED__)
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_libtgvoip___IVoIPControllerWrapperPublicNonVirtuals[] = L"libtgvoip.__IVoIPControllerWrapperPublicNonVirtuals";
#endif /* !defined(____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_INTERFACE_DEFINED__) */


/* interface __MIDL_itf_libtgvoip_0000_0002 */
/* [local] */ 



extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0002_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0002_v0_0_s_ifspec;

#ifndef ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_INTERFACE_DEFINED__
#define ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_INTERFACE_DEFINED__

/* interface __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals */
/* [uuid][object] */ 



/* interface ABI::libtgvoip::__IVoIPControllerWrapperPublicNonVirtuals */
/* [uuid][object] */ 


EXTERN_C const IID IID___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals;

#if defined(__cplusplus) && !defined(CINTERFACE)
    } /* end extern "C" */
    namespace ABI {
        namespace libtgvoip {
            
            MIDL_INTERFACE("A4312674-FAEC-3E92-B84B-631A08BEA963")
            __IVoIPControllerWrapperPublicNonVirtuals : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE Start( void) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE Connect( void) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE SetPublicEndpoints( 
                    /* [in] */ UINT32 __endpointsSize,
                    /* [in][size_is] */ ABI::libtgvoip::__IEndpointPublicNonVirtuals **endpoints,
                    /* [in] */ boolean allowP2P) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE SetNetworkType( 
                    /* [in] */ ABI::libtgvoip::NetworkType type) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE SetStateCallback( 
                    /* [in] */ ABI::libtgvoip::IStateCallback *callback) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE SetMicMute( 
                    /* [in] */ boolean mute) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE SetEncryptionKey( 
                    /* [in] */ UINT32 __keySize,
                    /* [in][size_is] */ BYTE *key,
                    /* [in] */ boolean isOutgoing) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE SetConfig( 
                    /* [in] */ DOUBLE initTimeout,
                    /* [in] */ DOUBLE recvTimeout,
                    /* [in] */ ABI::libtgvoip::DataSavingMode dataSavingMode,
                    /* [in] */ boolean enableAEC,
                    /* [in] */ boolean enableNS,
                    /* [in] */ boolean enableAGC,
                    /* [in] */ HSTRING logFilePath,
                    /* [in] */ HSTRING statsDumpFilePath) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE GetDebugString( 
                    /* [out][retval] */ HSTRING *__returnValue) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE GetDebugLog( 
                    /* [out][retval] */ HSTRING *__returnValue) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE GetLastError( 
                    /* [out][retval] */ ABI::libtgvoip::Error *__returnValue) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE GetPreferredRelayID( 
                    /* [out][retval] */ INT64 *__returnValue) = 0;
                
            };

            extern const __declspec(selectany) IID & IID___IVoIPControllerWrapperPublicNonVirtuals = __uuidof(__IVoIPControllerWrapperPublicNonVirtuals);

            
        }  /* end namespace */
    }  /* end namespace */
    extern "C" { 
    
#else 	/* C style interface */

    typedef struct __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtualsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetIids )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [out] */ ULONG *iidCount,
            /* [size_is][size_is][out] */ IID **iids);
        
        HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [out] */ HSTRING *className);
        
        HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [out] */ TrustLevel *trustLevel);
        
        HRESULT ( STDMETHODCALLTYPE *Start )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This);
        
        HRESULT ( STDMETHODCALLTYPE *Connect )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This);
        
        HRESULT ( STDMETHODCALLTYPE *SetPublicEndpoints )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [in] */ UINT32 __endpointsSize,
            /* [in][size_is] */ __x_ABI_Clibtgvoip_C____IEndpointPublicNonVirtuals **endpoints,
            /* [in] */ boolean allowP2P);
        
        HRESULT ( STDMETHODCALLTYPE *SetNetworkType )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [in] */ __x_ABI_Clibtgvoip_CNetworkType type);
        
        HRESULT ( STDMETHODCALLTYPE *SetStateCallback )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [in] */ __x_ABI_Clibtgvoip_CIStateCallback *callback);
        
        HRESULT ( STDMETHODCALLTYPE *SetMicMute )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [in] */ boolean mute);
        
        HRESULT ( STDMETHODCALLTYPE *SetEncryptionKey )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [in] */ UINT32 __keySize,
            /* [in][size_is] */ BYTE *key,
            /* [in] */ boolean isOutgoing);
        
        HRESULT ( STDMETHODCALLTYPE *SetConfig )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [in] */ DOUBLE initTimeout,
            /* [in] */ DOUBLE recvTimeout,
            /* [in] */ __x_ABI_Clibtgvoip_CDataSavingMode dataSavingMode,
            /* [in] */ boolean enableAEC,
            /* [in] */ boolean enableNS,
            /* [in] */ boolean enableAGC,
            /* [in] */ HSTRING logFilePath,
            /* [in] */ HSTRING statsDumpFilePath);
        
        HRESULT ( STDMETHODCALLTYPE *GetDebugString )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [out][retval] */ HSTRING *__returnValue);
        
        HRESULT ( STDMETHODCALLTYPE *GetDebugLog )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [out][retval] */ HSTRING *__returnValue);
        
        HRESULT ( STDMETHODCALLTYPE *GetLastError )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [out][retval] */ __x_ABI_Clibtgvoip_CError *__returnValue);
        
        HRESULT ( STDMETHODCALLTYPE *GetPreferredRelayID )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals * This,
            /* [out][retval] */ INT64 *__returnValue);
        
        END_INTERFACE
    } __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtualsVtbl;

    interface __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals
    {
        CONST_VTBL struct __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtualsVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_Start(This)	\
    ( (This)->lpVtbl -> Start(This) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_Connect(This)	\
    ( (This)->lpVtbl -> Connect(This) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_SetPublicEndpoints(This,__endpointsSize,endpoints,allowP2P)	\
    ( (This)->lpVtbl -> SetPublicEndpoints(This,__endpointsSize,endpoints,allowP2P) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_SetNetworkType(This,type)	\
    ( (This)->lpVtbl -> SetNetworkType(This,type) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_SetStateCallback(This,callback)	\
    ( (This)->lpVtbl -> SetStateCallback(This,callback) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_SetMicMute(This,mute)	\
    ( (This)->lpVtbl -> SetMicMute(This,mute) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_SetEncryptionKey(This,__keySize,key,isOutgoing)	\
    ( (This)->lpVtbl -> SetEncryptionKey(This,__keySize,key,isOutgoing) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_SetConfig(This,initTimeout,recvTimeout,dataSavingMode,enableAEC,enableNS,enableAGC,logFilePath,statsDumpFilePath)	\
    ( (This)->lpVtbl -> SetConfig(This,initTimeout,recvTimeout,dataSavingMode,enableAEC,enableNS,enableAGC,logFilePath,statsDumpFilePath) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_GetDebugString(This,__returnValue)	\
    ( (This)->lpVtbl -> GetDebugString(This,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_GetDebugLog(This,__returnValue)	\
    ( (This)->lpVtbl -> GetDebugLog(This,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_GetLastError(This,__returnValue)	\
    ( (This)->lpVtbl -> GetLastError(This,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_GetPreferredRelayID(This,__returnValue)	\
    ( (This)->lpVtbl -> GetPreferredRelayID(This,__returnValue) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperPublicNonVirtuals_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_libtgvoip_0000_0003 */
/* [local] */ 

#if !defined(____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_INTERFACE_DEFINED__)
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_libtgvoip___IVoIPControllerWrapperStatics[] = L"libtgvoip.__IVoIPControllerWrapperStatics";
#endif /* !defined(____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_INTERFACE_DEFINED__) */


/* interface __MIDL_itf_libtgvoip_0000_0003 */
/* [local] */ 



extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0003_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0003_v0_0_s_ifspec;

#ifndef ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_INTERFACE_DEFINED__
#define ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_INTERFACE_DEFINED__

/* interface __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics */
/* [uuid][object] */ 



/* interface ABI::libtgvoip::__IVoIPControllerWrapperStatics */
/* [uuid][object] */ 


EXTERN_C const IID IID___x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics;

#if defined(__cplusplus) && !defined(CINTERFACE)
    } /* end extern "C" */
    namespace ABI {
        namespace libtgvoip {
            
            MIDL_INTERFACE("B855DB3B-9FD9-3215-8F33-73173C2AD6CD")
            __IVoIPControllerWrapperStatics : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE GetVersion( 
                    /* [out][retval] */ HSTRING *__returnValue) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE UpdateServerConfig( 
                    /* [in] */ HSTRING json) = 0;
                
                virtual HRESULT STDMETHODCALLTYPE SwitchSpeaker( 
                    /* [in] */ boolean external) = 0;
                
            };

            extern const __declspec(selectany) IID & IID___IVoIPControllerWrapperStatics = __uuidof(__IVoIPControllerWrapperStatics);

            
        }  /* end namespace */
    }  /* end namespace */
    extern "C" { 
    
#else 	/* C style interface */

    typedef struct __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStaticsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetIids )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This,
            /* [out] */ ULONG *iidCount,
            /* [size_is][size_is][out] */ IID **iids);
        
        HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This,
            /* [out] */ HSTRING *className);
        
        HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This,
            /* [out] */ TrustLevel *trustLevel);
        
        HRESULT ( STDMETHODCALLTYPE *GetVersion )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This,
            /* [out][retval] */ HSTRING *__returnValue);
        
        HRESULT ( STDMETHODCALLTYPE *UpdateServerConfig )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This,
            /* [in] */ HSTRING json);
        
        HRESULT ( STDMETHODCALLTYPE *SwitchSpeaker )( 
            __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics * This,
            /* [in] */ boolean external);
        
        END_INTERFACE
    } __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStaticsVtbl;

    interface __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics
    {
        CONST_VTBL struct __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStaticsVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_GetVersion(This,__returnValue)	\
    ( (This)->lpVtbl -> GetVersion(This,__returnValue) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_UpdateServerConfig(This,json)	\
    ( (This)->lpVtbl -> UpdateServerConfig(This,json) ) 

#define __x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_SwitchSpeaker(This,external)	\
    ( (This)->lpVtbl -> SwitchSpeaker(This,external) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* ____x_ABI_Clibtgvoip_C____IVoIPControllerWrapperStatics_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_libtgvoip_0000_0004 */
/* [local] */ 

#ifndef RUNTIMECLASS_libtgvoip_Endpoint_DEFINED
#define RUNTIMECLASS_libtgvoip_Endpoint_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_libtgvoip_Endpoint[] = L"libtgvoip.Endpoint";
#endif
#ifndef RUNTIMECLASS_libtgvoip_VoIPControllerWrapper_DEFINED
#define RUNTIMECLASS_libtgvoip_VoIPControllerWrapper_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_libtgvoip_VoIPControllerWrapper[] = L"libtgvoip.VoIPControllerWrapper";
#endif


/* interface __MIDL_itf_libtgvoip_0000_0004 */
/* [local] */ 



extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0004_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_libtgvoip_0000_0004_v0_0_s_ifspec;

/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  HSTRING_UserSize(     unsigned long *, unsigned long            , HSTRING * ); 
unsigned char * __RPC_USER  HSTRING_UserMarshal(  unsigned long *, unsigned char *, HSTRING * ); 
unsigned char * __RPC_USER  HSTRING_UserUnmarshal(unsigned long *, unsigned char *, HSTRING * ); 
void                      __RPC_USER  HSTRING_UserFree(     unsigned long *, HSTRING * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


